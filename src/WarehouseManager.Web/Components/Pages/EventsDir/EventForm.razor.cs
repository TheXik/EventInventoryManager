using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using WarehouseManager.Core.Entities;

namespace WarehouseManager.Web.Components.Pages.EventsDir;

/// <summary>
/// Modal form component for creating and editing events
/// Provides a comprehensive form with validation and event callbacks
/// </summary>
public partial class EventForm
{
    // Data and UI State
    private Event _eventModel = new();
    private bool _isNew;
    private bool _showModal;
    private string? _errorMessage;
    private bool _isLoading = false;

    // Parameters
    [Parameter] public EventCallback<Event> OnSave { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    // Dependency Injection
    [Inject] private ILogger<EventForm> Logger { get; set; } = default!;

    /// <summary>
    /// Opens the form for creating a new event with default values
    /// </summary>
    public void Create()
    {
        try
        {
            _isNew = true;
            _errorMessage = null;
            _eventModel = new Event
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(2),
                Color = "#0d6efd" // Default blue color
            };
            _showModal = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to initialize new event: {ex.Message}";
            Logger.LogError(ex, "Error creating new event form");
        }
    }

    /// <summary>
    /// Opens the form for editing an existing event
    /// </summary>
    /// <param name="eventToEdit">The event to edit</param>
    public void Edit(Event eventToEdit)
    {
        try
        {
            _isNew = false;
            _errorMessage = null;
            _eventModel = eventToEdit;
            _showModal = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load event for editing: {ex.Message}";
            Logger.LogError(ex, "Error editing event {EventId}", eventToEdit.Id);
        }
    }

    /// <summary>
    /// Closes the modal form and notifies the parent component
    /// </summary>
    public void Close()
    {
        _showModal = false;
        _errorMessage = null;
        StateHasChanged();
        OnClose.InvokeAsync();
    }

    /// <summary>
    /// Handles form submission when validation passes
    /// </summary>
    private async Task HandleValidSubmit()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            
            // Trim string properties
            _eventModel.Name = _eventModel.Name?.Trim();
            _eventModel.Location = _eventModel.Location?.Trim();
            _eventModel.ClientName = _eventModel.ClientName?.Trim();
            _eventModel.ClientContact = _eventModel.ClientContact?.Trim();
            _eventModel.Description = _eventModel.Description?.Trim();
            
            await OnSave.InvokeAsync(_eventModel);
            Close();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to save event: {ex.Message}";
            Logger.LogError(ex, "Error saving event");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
}