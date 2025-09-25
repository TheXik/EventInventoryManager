using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WarehouseManager.Application.Interfaces;

namespace WarehouseManager.Infrastructure.Services;

public class ChatAiService : IChatAiService
{
    private readonly HttpClient _http;
    private readonly GeminiOptions _options;

    public ChatAiService(HttpClient http, IOptions<GeminiOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<string> AskAsync(
        string systemPrompt,
        IEnumerable<(bool fromUser, string text)> history,
        string userInput, CancellationToken ct = default)
    {
        var apiKey = _options.ApiKey ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        var model = string.IsNullOrWhiteSpace(_options.Model) ? "gemini-2.0-flash" : _options.Model;
        if (string.IsNullOrWhiteSpace(apiKey))
            return "AI is not configured. Set GEMINI_API_KEY env var or Gemini:ApiKey in appsettings.";

        // Endpoint for Google Generative Language API 
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey)}";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);

        // Build contents from history and current user input
        var contents = new List<object>();

        var systemInstruction = new
        {
            parts = new[] { new { text = systemPrompt } }
        };

        foreach (var (fromUser, text) in history)
            contents.Add(new
            {
                role = fromUser ? "user" : "model",
                parts = new[] { new { text } }
            });
        // Append the latest user input explicitly
        contents.Add(new
        {
            role = "user",
            parts = new[] { new { text = userInput } }
        });

        var body = new
        {
            system_instruction = systemInstruction,
            contents,
            generationConfig = new
            {
                temperature = 0.7,
                maxOutputTokens = 2000,
                topK = 20,
                topP = 0.9
            }
        };

        req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        try
        {
            using var resp = await _http.SendAsync(req, ct);
            resp.EnsureSuccessStatusCode();
            using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var root = doc.RootElement;
            // candidates[0].content.parts[0].text
            var text = root.GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
            return text ?? string.Empty;
        }
        catch (Exception ex)
        {
            return $"AI request failed: {ex.Message}";
        }
    }
}