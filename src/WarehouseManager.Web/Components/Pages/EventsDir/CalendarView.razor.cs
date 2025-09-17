using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using WarehouseManager.Core.Entities;

namespace WarehouseManager.Web.Components.Pages.EventsDir;

public partial class CalendarView : ComponentBase
{
    protected List<Event> events = new();

    // Properties for the component
    protected RadzenScheduler<Event> scheduler;

    // This method is called when the component is initialized
    protected override void OnInitialized()
    {
        // In a real application, you would inject a repository and load data here.
        // For now, we use sample data.
        events = new List<Event>
        {
            new()
            {
                Name = "Rock Concert",
                StartDate = DateTime.Today.AddDays(1).AddHours(18),
                EndDate = DateTime.Today.AddDays(1).AddHours(23),
                ClientName = "Music Fest Inc.",
                Location = "O2 Arena"
            },
            new()
            {
                Name = "Corporate Conference",
                StartDate = DateTime.Today.AddDays(4),
                EndDate = DateTime.Today.AddDays(6),
                ClientName = "Tech Solutions Ltd.",
                Location = "Prague Congress Centre"
            }
        };
    }

    // This C# method is triggered when you click an appointment in the UI
    protected async Task OnAppointmentSelect(SchedulerAppointmentSelectEventArgs<Event> args)
    {
        Console.WriteLine($"Event '{args.Data.Name}' for client '{args.Data.ClientName}' selected.");

        // You can add logic here to open a dialog or navigate to an event detail page
        await Task.CompletedTask; // Placeholder for async operations
    }
}