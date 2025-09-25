namespace WarehouseManager.Infrastructure.Services;

/// <summary>
/// GeminiOptions is a class that contains the options for the Gemini API
/// </summary>
public class GeminiOptions
{   
    /// <summary>
    /// Api Key for the Gemini API 
    /// </summary>
    public string? ApiKey { get; set; }
    /// <summary>
    /// Model for the Gemini API that should be used
    /// </summary>
    public string? Model { get; set; }
}