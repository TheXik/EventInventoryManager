namespace WarehouseManager.Application.Interfaces;

/// <summary>
/// Defines the contract for the application's AI chat backend integration
/// Implementations wrap a concrete LLM provider and normalize requests/responses.
/// </summary>
public interface IChatAiService
{
    /// <summary>
    /// Sends the given user input along with prior chat history to an AI model and returns the assistants reply.
    /// </summary>
    /// <param name="systemPrompt">System instructions for the LLM</param>
    /// <param name="history">Chat history as (fromUser, text)</param>
    /// <param name="userInput">Current user input</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Assistants textual reply</returns>
    Task<string> AskAsync(string systemPrompt, IEnumerable<(bool fromUser, string text)> history, string userInput,
        CancellationToken ct = default);
}