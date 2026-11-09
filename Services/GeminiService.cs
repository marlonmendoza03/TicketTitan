using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TicketTitan.Common;
using TicketTitan.DTO;
using TicketTitan.Interfaces;
using TicketTitan.Options;

namespace TicketTitan.Services
{
    // Calls the Gemini REST API (generateContent). Gemini is stateless, so the
    // full chat history is sent on every request.
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiOptions _options;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(HttpClient httpClient, IOptions<GeminiOptions> options, ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<ServiceResult<string>> GenerateStoryReplyAsync(StoryChatRequestDto request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                return ServiceResult<string>.Failure("Gemini API key is not configured.");
            }

            var body = new GenerateContentRequest(
                SystemInstruction: new Content(null, [new Part(BuildSystemPrompt(request.Setup))]),
                Contents: request.Messages
                    .Select(m => new Content(m.Role == "assistant" ? "model" : "user", [new Part(m.Content)]))
                    .ToList(),
                GenerationConfig: new GenerationConfig(
                    Temperature: 0.9,
                    MaxOutputTokens: 2048,
                    ThinkingConfig: _options.ThinkingBudget is int budget ? new ThinkingConfig(budget) : null)
            );

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"v1beta/models/{_options.Model}:generateContent")
            {
                Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Add("x-goog-api-key", _options.ApiKey);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException ||
                (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
            {
                _logger.LogWarning(ex, "Could not reach Gemini.");
                return ServiceResult<string>.Failure("Could not reach Gemini.");
            }

            using var _ = response;

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Gemini returned {StatusCode}: {Error}", (int)response.StatusCode, error);
                return ServiceResult<string>.Failure("Gemini request failed.");
            }

            var result = await response.Content.ReadFromJsonAsync<GenerateContentResponse>(JsonOptions, cancellationToken);
            var text = string.Concat(
                result?.Candidates?.FirstOrDefault()?.Content?.Parts?.Select(p => p.Text) ?? []);

            return string.IsNullOrWhiteSpace(text)
                ? ServiceResult<string>.Failure("Gemini returned an empty reply.")
                : ServiceResult<string>.Success(text.Trim());
        }

        private static string BuildSystemPrompt(StorySetupDto setup) => $"""
            You are a story writer for a faceless short-form video channel.
            You help the user plan a {setup.CategoryName} series of {setup.EpisodeCount} episode(s),
            each about {setup.EpisodeDurationSec} seconds long when narrated.
            Each episode will later be split into scenes of roughly 8 seconds,
            so about {Math.Max(1, setup.EpisodeDurationSec / 8)} scenes per episode.

            On the first message, propose a short series premise and a one-line outline per episode.
            On later messages, revise the outline based on the user's feedback.
            Keep replies concise, use plain text with simple line breaks (no Markdown headings),
            and end with one short question about what to adjust next.
            """;

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private record GenerateContentRequest(
            Content SystemInstruction,
            List<Content> Contents,
            GenerationConfig GenerationConfig);

        private record Content(string? Role, List<Part> Parts);

        private record Part(string? Text);

        private record GenerationConfig(double Temperature, int MaxOutputTokens, ThinkingConfig? ThinkingConfig);

        private record ThinkingConfig(int ThinkingBudget);

        private record GenerateContentResponse(List<Candidate>? Candidates);

        private record Candidate(Content? Content);
    }
}
