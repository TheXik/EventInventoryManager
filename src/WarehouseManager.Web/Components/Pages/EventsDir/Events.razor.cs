using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities;

namespace WarehouseManager.Web.Components.Pages.EventsDir;

/// <summary>
/// Main events management page component
/// Provides both calendar and list views for event management with CRUD operations
/// </summary>
public partial class Events
{
    // Component References (event form)
    private EventForm _eventForm = default!; 

    private List<Event> _events = new(); // List of events
    private bool isCalendarView; // Calendar view flag
    private string? _errorMessage;
    private bool _isLoading = true;

    // Dependency Injection
    [Inject] private IEventRepository EventRepository { get; set; } = default!;
    [Inject] private ILogger<Events> Logger { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    /// <summary>
    /// Initializes the component by loading all events from the database
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await LoadEvents();
    }

    /// <summary>
    /// Loads all events from the database and updates the UI state.
    /// </summary>
    private async Task LoadEvents()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            
            var eventsData = await EventRepository.GetAllAsync();
            _events = eventsData.OrderByDescending(e => e.StartDate).ToList();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load events: {ex.Message}";
            Logger.LogError(ex, "Error loading events");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Opens the event form for creating a new event
    /// </summary>
    /// <param name="startDate">The suggested start date for the new event</param>
    private void AddNewEvent(DateTime startDate)
    {
        _eventForm.Create();
    }

    /// <summary>
    /// Opens the event form for editing an existing event
    /// </summary>
    /// <param name="eventToEdit">The event to edit</param>
    private void EditEvent(Event eventToEdit)
    {
        _eventForm.Edit(eventToEdit);
    }

    /// <summary>
    /// Deletes an event after user confirmation
    /// </summary>
    /// <param name="eventToDelete">The event to delete</param>
    private async Task DeleteEvent(Event eventToDelete)
    {
        try
        {
            // Show confirmation dialog
            var confirmed = await JS.InvokeAsync<bool>("confirm", 
                $"Are you sure you want to delete the event '{eventToDelete.Name}'? This will return all allocated items to inventory and cannot be undone.");
            if (!confirmed) return;

            _isLoading = true;
            _errorMessage = null;
            
            await EventRepository.DeleteAsync(eventToDelete.Id);
            await LoadEvents();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to delete event: {ex.Message}";
            Logger.LogError(ex, "Error deleting event {EventId}", eventToDelete.Id);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handles saving events (both new and existing)
    /// </summary>
    /// <param name="savedEvent">The event to save</param>
    private async Task HandleSaveEvent(Event savedEvent)
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            
            if (savedEvent.Id == 0)
                await EventRepository.AddAsync(savedEvent);
            else
                await EventRepository.UpdateAsync(savedEvent);
                
            await LoadEvents();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to save event: {ex.Message}";
            Logger.LogError(ex, "Error saving event {EventId}", savedEvent.Id);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handles event updates from the calendar view (drag & drop)
    /// </summary>
    /// <param name="updatedEvent">The updated event</param>
    private async Task HandleEventUpdate(Event updatedEvent)
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            
            await EventRepository.UpdateAsync(updatedEvent);
            await LoadEvents();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to update event: {ex.Message}";
            Logger.LogError(ex, "Error updating event {EventId}", updatedEvent.Id);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
}