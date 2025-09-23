using Microsoft.AspNetCore.Components;
using WarehouseManager.Core.Entities;

namespace WarehouseManager.Web.Components.Pages.EventsDir;

public partial class EventListView
{
    [Parameter] public IEnumerable<Event>? Events { get; set; }

    [Parameter] public EventCallback<Event> OnEditClick { get; set; }

    [Parameter] public EventCallback<Event> OnDeleteClick { get; set; }
}