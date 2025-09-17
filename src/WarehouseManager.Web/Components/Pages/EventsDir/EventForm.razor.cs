using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using WarehouseManager.Core.Entities;

namespace WarehouseManager.Web.Components.Pages.EventsDir
{
    public partial class EventForm
    {
        [Parameter]
        public EventCallback<Event> OnSave { get; set; }

        private bool _showModal = false;
        private bool _isNew = false;
        private Event _eventModel = new();

        public void Open(Event eventModel, bool isNew)
        {
            _isNew = isNew;
            _eventModel = eventModel;
            _showModal = true;
            StateHasChanged();
        }

        public void Close()
        {
            _showModal = false;
            StateHasChanged();
        }

        private async Task Save()
        {
            await OnSave.InvokeAsync(_eventModel);
            Close();
        }
    }
}