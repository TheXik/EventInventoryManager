namespace WarehouseManager.Application.Interfaces;

public interface IChatAiService
{
    /// <summary>
    ///     Sends the given user input along with prior chat history to an AI model and returns assistant reply.
    /// </summary>
    /// <param name="systemPrompt">System persona/instructions.</param>
    /// <param name="history">Chat history as (fromUser, text).</param>
    /// <param name="userInput">Current user input.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<string> AskAsync(string systemPrompt, IEnumerable<(bool fromUser, string text)> history, string userInput,
        CancellationToken ct = default);
}