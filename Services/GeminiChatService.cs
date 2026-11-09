using System.Text;
using System.Text.Json;
using TicketTitan.Common;
using TicketTitan.DTO;
using TicketTitan.Interfaces;

namespace TicketTitan.Services
{
    public class GeminiChatService : IGeminiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public GeminiChatService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<ServiceResult<GeminiChatResponseDto>> SendChatAsync(string prompt, string model, CancellationToken cancellationToken)
        {
            var result = await SendChatAsync(new[]
            {
                ("user", prompt)
            },model, cancellationToken);

            if (!result.IsSuccess)
            {
                return ServiceResult<GeminiChatResponseDto>.Failure(result.ErrorMessage);
            }

            return ServiceResult<GeminiChatResponseDto>.Success(result.Value);
        }

        public async Task<ServiceResult<GeminiChatResponseDto>> SendChatAsync(IReadOnlyList<(string Role, string Content)> messages, string model, CancellationToken cancellationToken)
        {
            var apiKey = _configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key is not configured");

            var requestBody = new
            {
                contents = messages.Select(m => new
                {
                    role = m.Role == "assistant" ? "model" : "user",
                    parts = new[]
                    {
                        new { text = m.Content }
                    }
                }).ToArray()
            };

            var json = JsonSerializer.Serialize(requestBody);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/v1beta/models/{model}:generateContent?key={apiKey}");

            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ServiceResult<GeminiChatResponseDto>.Failure($"Gemini request failed: {(int)response.StatusCode} - {responseBody}");
            }

            using var doc = JsonDocument.Parse(responseBody);

            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            var role = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("role")
                .GetString();
            DateTime time = DateTime.Now;

            var reply = new GeminiChatResponseDto
            {
                content = text,
                role = role,
                createdAt = time
            };

            return ServiceResult<GeminiChatResponseDto>.Success(reply);
        }
    }
}
