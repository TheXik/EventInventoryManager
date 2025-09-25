using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WarehouseManager.Application.Interfaces;

namespace WarehouseManager.Infrastructure.Services;

/// <summary>
/// Service for interacting with the Google Gemini API
/// </summary>
public class ChatAiService : IChatAiService
{
    private readonly HttpClient _http;
    private readonly GeminiOptions _options;

    /// <summary>
    /// Constructor for ChatAiService Initializes the HttpClient and GeminiOptions
    /// </summary>
    /// <param name="http">HttpClient for making HTTP requests</param>
    /// <param name="options">Options for the Gemini API</param>
    public ChatAiService(HttpClient http, IOptions<GeminiOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    /// <summary>
    /// Sends a prompt to the Gemini API including system instructions and conversation history
    /// </summary>
    /// <param name="systemPrompt">The system level instructions for the AI model</param>
    /// <param name="history">An enumerable of past conversations to provide context</param>
    /// <param name="userInput">The users most recent message</param>
    /// <param name="ct">A cancellation token</param>
    /// <returns>The AI's text response</returns>
    public async Task<string> AskAsync(
        string systemPrompt,
        IEnumerable<(bool fromUser, string text)> history,
        string userInput,
        CancellationToken ct = default)
    {
        var apiKey = _options.ApiKey ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return "AI is not configured. Set GEMINI_API_KEY env !!!!! :)";
        }

        try
        {
            using var request = CreateApiRequest(apiKey, systemPrompt, history, userInput);
            using var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
            return await ExtractTextFromResponseAsync(response, ct);
        }
        catch (Exception ex)
        {
            return $"AI request failed: {ex.Message}";
        }
    }

    /// <summary>
    /// Creates the HttpRequestMessage for the Gemini API call
    /// </summary>
    private HttpRequestMessage CreateApiRequest(string apiKey, string systemPrompt,
        IEnumerable<(bool fromUser, string text)> history, string userInput)
    {
        var model = string.IsNullOrWhiteSpace(_options.Model) ? "gemini-2.0-flash" : _options.Model;

        // Endpoint for Google Generative Language API 
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey)}";

        var requestBody = BuildRequestBody(systemPrompt, history, userInput);

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        return request;
    }

    /// <summary>
    /// Builds the JSON request body object from the provided prompts and history
    /// </summary>
    private object BuildRequestBody(string systemPrompt, IEnumerable<(bool fromUser, string text)> history,
        string userInput)
    {
        // Build contents from history and current user input
        var contents = new List<object>();

        var systemInstruction = new
        {
            parts = new[] { new { text = systemPrompt } }
        };

        foreach (var (fromUser, text) in history)
        {
            contents.Add(new
            {
                role = fromUser ? "user" : "model",
                parts = new[] { new { text } }
            });
        }

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
                temperature = 0.7, // controls the randomness of the AIs responses. Lower values make the responses more deterministic.
                maxOutputTokens = 1500,
                topK = 20, // consider the top 20 most likely words.
                topP = 0.9 // Tells the AI to consider the most likely words whose probabilities add up to 90%
            }
        };

        return body;
    }

    /// <summary>
    /// Parses the HttpResponseMessage and extracts the AI's response text
    /// </summary>
    private async Task<string> ExtractTextFromResponseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        using var stream = await response.Content.ReadAsStreamAsync(ct);
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
}
