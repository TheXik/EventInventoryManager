using Microsoft.AspNetCore.Components;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities;

namespace WarehouseManager.Web.Components.Pages.EventsDir;

public partial class Events
{
    // This is the component reference for your modal form
    private EventForm _eventForm = default!;

    // This list holds all the event data
    private List<Event> _events = new();

    // This boolean controls the view toggle
    private bool isCalendarView;

    [Inject] private IEventRepository EventRepository { get; set; } = default!;

    [Inject] private ILogger<Events> Logger { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await LoadEvents();
    }

    private async Task LoadEvents()
    {
        try
        {
            var eventsData = await EventRepository.GetAllAsync();
            _events = eventsData.ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading events.");
            //TODO ADD BETTER NOTIFICATION
        }
    }

    private void AddNewEvent(DateTime startDate)
    {
        var newEvent = new Event
        {
            StartDate = startDate,
            EndDate = startDate.AddHours(1),
            Color = "#3788d8"
        };
        _eventForm.Create();
    }

    private void EditEvent(Event eventToEdit)
    {
        _eventForm.Edit(eventToEdit);
    }

    private async Task DeleteEvent(Event eventToDelete)
    {
        // TODO add a confirmation dialog here before deleting
        await EventRepository.DeleteAsync(eventToDelete.Id);
        await LoadEvents();
        StateHasChanged();
    }

    private async Task HandleSaveEvent(Event savedEvent)
    {
        if (savedEvent.Id == 0)
            await EventRepository.AddAsync(savedEvent);
        else
            await EventRepository.UpdateAsync(savedEvent);
        await LoadEvents();
        StateHasChanged();
    }

    private async Task HandleEventUpdate(Event updatedEvent)
    {
        await EventRepository.UpdateAsync(updatedEvent);
        await LoadEvents();
        StateHasChanged();
    }
}