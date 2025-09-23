using Microsoft.AspNetCore.Components;
using WarehouseManager.Core.Entities;

namespace WarehouseManager.Web.Components.Pages.EventsDir
{
    public partial class EventForm
    {
        [Parameter]
        public EventCallback<Event> OnSave { get; set; }

        [Parameter]
        public EventCallback OnClose { get; set; } // Add a callback for closing

        private bool _showModal = false;
        private bool _isNew = false;
        private Event _eventModel = new();

        // A public method to open the form for a new event
        public void Create()
        {
            _isNew = true;
            _eventModel = new Event
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(2),
                Color = "#0d6efd" // A default blue color
            };
            _showModal = true;
            StateHasChanged();
        }

        // A public method to open the form for editing an existing event
        public void Edit(Event eventToEdit)
        {
            _isNew = false;
            _eventModel = eventToEdit;
            _showModal = true;
            StateHasChanged();
        }

        public void Close()
        {
            _showModal = false;
            StateHasChanged();
            OnClose.InvokeAsync(); // Notify the parent that the modal is closed
        }

        private async Task HandleValidSubmit()
        {
            await OnSave.InvokeAsync(_eventModel);
            Close();
        }
    }
}