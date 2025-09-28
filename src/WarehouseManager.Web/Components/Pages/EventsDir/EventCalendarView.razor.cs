using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Radzen;
using Radzen.Blazor;
using WarehouseManager.Core.Entities;

namespace WarehouseManager.Web.Components.Pages.EventsDir;

/// <summary>
/// Calendar view component for displaying and managing events
/// Provides drag-and-drop functionality and event interaction
/// </summary>
public partial class EventCalendarView
{
    // Component References
    private RadzenScheduler<Event> _scheduler = default!;

    // Parameters
    [Parameter] public IEnumerable<Event>? Events { get; set; }
    [Parameter] public EventCallback<Event> OnEventClick { get; set; }
    [Parameter] public EventCallback<DateTime> OnDateSelect { get; set; }
    [Parameter] public EventCallback<Event> OnEventUpdate { get; set; }

    // Dependency Injection
    [Inject] private ILogger<EventCalendarView> Logger { get; set; } = default!;

    /// <summary>
    /// Handles slot selection events (clicking on empty calendar slots)
    /// </summary>
    /// <param name="args">The slot selection event arguments</param>
    private async Task OnSlotSelect(SchedulerSlotSelectEventArgs args)
    {
        try
        {
            await OnDateSelect.InvokeAsync(args.Start);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling slot select");
        }
    }

    /// <summary>
    /// Handles appointment selection events (clicking on events)
    /// </summary>
    /// <param name="args">The appointment selection event arguments</param>
    private async Task OnAppointmentSelect(SchedulerAppointmentSelectEventArgs<Event> args)
    {
        try
        {
            await OnEventClick.InvokeAsync(args.Data);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling appointment select");
        }
    }

    /// <summary>
    /// Handles appointment move events (drag and drop)
    /// Updates event dates while preserving duration
    /// </summary>
    /// <param name="args">The appointment move event arguments</param>
    private async Task OnAppointmentMove(SchedulerAppointmentMoveEventArgs args)
    {
        try
        {
            var eventToUpdate = args.Appointment.Data as Event;
            if (eventToUpdate != null)
            {
                var duration = eventToUpdate.EndDate - eventToUpdate.StartDate;

                eventToUpdate.StartDate = args.SlotDate;
                eventToUpdate.EndDate = args.SlotDate + duration;

                await OnEventUpdate.InvokeAsync(eventToUpdate);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling appointment move");
        }
    }

    /// <summary>
    /// Handles appointment rendering to apply custom styling
    /// </summary>
    /// <param name="args">The appointment render event arguments</param>
    private void OnAppointmentRender(SchedulerAppointmentRenderEventArgs<Event> args)
    {
        try
        {
            if (!string.IsNullOrEmpty(args.Data.Color))
            {
                args.Attributes["style"] = $"background-color: {args.Data.Color};";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error rendering appointment");
        }
    }
}