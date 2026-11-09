namespace TicketTitan.Options
{
    public sealed class GeminiOptions
    {
        public const string SectionName = "Gemini";

        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gemini-2.5-flash";
        // 0 turns off "thinking" on Gemini 2.5 Flash for faster chat replies. Set to null for models that require thinking.
        public int? ThinkingBudget { get; set; } = 0;
        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/";
    }
}
