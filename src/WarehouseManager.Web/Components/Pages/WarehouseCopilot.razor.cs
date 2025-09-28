using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using WarehouseManager.Application.Interfaces;
using WarehouseManager.Core.Entities;
using WarehouseManager.Core.Entities.Rentals;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.Components.Pages;

public partial class WarehouseCopilot
{
    private const string SystemPrompt = "You are an AI assistant for a warehouse. Answer questions about inventory, events, and rentals using only the data provided. Use plain text only.";
    private readonly List<Message> _messages = new();
    private string _draft = string.Empty;

    [Inject] private IChatAiService ChatAi { get; set; } = default!;
    [Inject] private IInventoryItemRepository InventoryRepo { get; set; } = default!;
    [Inject] private IEventRepository EventRepo { get; set; } = default!;
    [Inject] private IRentalRepository RentalRepo { get; set; } = default!;


    private async Task Send()
    {
        var text = _draft.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        _messages.Add(new Message(true, text, DateTime.Now));
        _draft = string.Empty;

        var context = await BuildContextAsync();
        var reply = await ChatAi.AskAsync(context, _messages.Select(m => (m.FromUser, m.Text)), text);
        _messages.Add(new Message(false, reply, DateTime.Now));
        StateHasChanged();
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await Send();
    }

    private async Task<string> BuildContextAsync()
    {
        try
        {
            var items = (await InventoryRepo.GetAllAsync()).ToList();
            var events = (await EventRepo.GetAllAsync()).ToList();
            var rentals = (await RentalRepo.GetAllAsync()).ToList();

            var sb = new StringBuilder();
            sb.AppendLine(SystemPrompt);
            sb.AppendLine();
            sb.AppendLine("INVENTORY:");
            foreach (var item in items)
            {
                sb.AppendLine($"- {item.Name} (ID:{item.Id}) | Available: {item.AvailableQuantity}/{item.TotalQuantity} | Category: {item.Category?.Name} | Condition: {item.Condition} | Weight: {item.Weight}kg | Dimensions: {item.Height}x{item.Width}x{item.Length}cm | Rental Price: ${item.RentalPricePerDay}/day | Loading Priority: {item.TruckLoadingPriority} | Rental Status: {item.RentalStatus}");
                if (!string.IsNullOrWhiteSpace(item.Description))
                    sb.AppendLine($"  Description: {item.Description}");
                if (!string.IsNullOrWhiteSpace(item.ConditionDescription))
                    sb.AppendLine($"  Condition Notes: {item.ConditionDescription}");
                if (!string.IsNullOrWhiteSpace(item.RentalDescription))
                    sb.AppendLine($"  Rental Notes: {item.RentalDescription}");
            }

            sb.AppendLine();
            sb.AppendLine("EVENTS:");
            foreach (var evt in events)
            {
                sb.AppendLine($"- {evt.Name} (ID:{evt.Id}) | {evt.StartDate:yyyy-MM-dd} to {evt.EndDate:yyyy-MM-dd} | Client: {evt.ClientName} | Location: {evt.Location} | Contact: {evt.ClientContact} | Color: {evt.Color}");
                if (!string.IsNullOrWhiteSpace(evt.Description))
                    sb.AppendLine($"  Description: {evt.Description}");
                sb.AppendLine($"  Items ({evt.EventInventoryItems?.Count ?? 0}):");
                if (evt.EventInventoryItems != null)
                {
                    foreach (var eventItem in evt.EventInventoryItems)
                    {
                        var item = items.FirstOrDefault(i => i.Id == eventItem.InventoryItemId);
                        sb.AppendLine($"    - {item?.Name ?? "Unknown"} (ID:{eventItem.InventoryItemId}) | Qty: {eventItem.Quantity}");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("RENTALS:");
            foreach (var rental in rentals)
            {
                sb.AppendLine($"- {rental.ClientName} (ID:{rental.RentalId}) | {rental.RentalDate:yyyy-MM-dd} to {rental.ExpectedReturnDate:yyyy-MM-dd} | Status: {rental.Status} | Payment: {rental.PaymentStatus} | Contact: {rental.ContactInfo} | Discount: {rental.DiscountPercentage}%");
                if (rental.ActualReturnDate.HasValue)
                    sb.AppendLine($"  Returned: {rental.ActualReturnDate.Value:yyyy-MM-dd}");
                if (!string.IsNullOrWhiteSpace(rental.Notes))
                    sb.AppendLine($"  Notes: {rental.Notes}");
                sb.AppendLine($"  Items ({rental.RentalItems?.Count ?? 0}):");
                if (rental.RentalItems != null)
                {
                    foreach (var rentalItem in rental.RentalItems)
                    {
                        var item = items.FirstOrDefault(i => i.Id == rentalItem.InventoryItemId);
                        var stillOut = rentalItem.QuantityRented - rentalItem.QuantityReturned;
                        sb.AppendLine($"    - {item?.Name ?? "Unknown"} (ID:{rentalItem.InventoryItemId}) | Rented: {rentalItem.QuantityRented} | Returned: {rentalItem.QuantityReturned} | Still Out: {stillOut} | Price/Day: ${rentalItem.PricePerDayAtTimeOfRental}");
                    }
                }
            }

            return sb.ToString();
        }
        catch
        {
            return SystemPrompt;
        }
    }

    private record Message(bool FromUser, string Text, DateTime At);
}