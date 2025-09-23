using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using WarehouseManager.Core.Entities;

namespace WarehouseManager.Web.Components.Pages.EventsDir;

public partial class EventCalendarView
{
    private RadzenScheduler<Event> _scheduler = default!;

    [Parameter] public IEnumerable<Event>? Events { get; set; }

    [Parameter] public EventCallback<Event> OnEventClick { get; set; }

    [Parameter] public EventCallback<DateTime> OnDateSelect { get; set; }

    [Parameter] public EventCallback<Event> OnEventUpdate { get; set; }

    private async Task OnSlotSelect(SchedulerSlotSelectEventArgs args)
    {
        await OnDateSelect.InvokeAsync(args.Start);
    }

    private async Task OnAppointmentSelect(SchedulerAppointmentSelectEventArgs<Event> args)
    {
        await OnEventClick.InvokeAsync(args.Data);
    }

    private async Task OnAppointmentMove(SchedulerAppointmentMoveEventArgs args)
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

    private void OnAppointmentRender(SchedulerAppointmentRenderEventArgs<Event> args)
    {
        if (!string.IsNullOrEmpty(args.Data.Color)) args.Attributes["style"] = $"background-color: {args.Data.Color};";
    }
}