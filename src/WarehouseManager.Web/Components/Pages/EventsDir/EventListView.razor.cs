using Microsoft.AspNetCore.Components;
using WarehouseManager.Core.Entities;

namespace WarehouseManager.Web.Components.Pages.EventsDir;

/// <summary>
/// List view component for displaying events in a card-based layout
/// Provides edit and delete actions for each event
/// </summary>
public partial class EventListView
{
    /// <summary>
    /// Collection of events to display in the list view
    /// </summary>
    [Parameter] public IEnumerable<Event>? Events { get; set; }

    /// <summary>
    /// Callback triggered when an event is selected for editing
    /// </summary>
    [Parameter] public EventCallback<Event> OnEditClick { get; set; }

    /// <summary>
    /// Callback triggered when an event is selected for deletion
    /// </summary>
    [Parameter] public EventCallback<Event> OnDeleteClick { get; set; }
}