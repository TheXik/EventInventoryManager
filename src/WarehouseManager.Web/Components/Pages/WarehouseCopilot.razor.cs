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
    // System prompt kept internal for future LLM integration
    private const string SystemPrompt =
        "You are the 'AI Assistant' for an event company's warehouse. Be proactive, friendly, and professional. Provide accurate, structured answers grounded strictly in the provided context. Prefer concise paragraphs with bullet points and short headings. Explain reasoning briefly when helpful, highlight risks or gaps, and suggest next steps. Always ask a short clarifying or follow‑up question when appropriate to keep the conversation moving.";

    private readonly List<Message> _messages = new();
    private string _draft = string.Empty;

    [Inject] private IChatAiService ChatAi { get; set; } = default!;
    [Inject] private IInventoryItemRepository InventoryRepo { get; set; } = default!;
    [Inject] private IEventRepository EventRepo { get; set; } = default!;
    [Inject] private IRentalRepository RentalRepo { get; set; } = default!;


    private async Task Send()
    {
        var text = (_draft ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        _messages.Add(new Message(true, text, DateTime.Now));
        _draft = string.Empty;

        // Build live app context to ground the model and prevent fabrication
        var composedPrompt = await BuildContextualSystemPromptAsync();

        // Call AI backend 
        var reply = await ChatAi.AskAsync(composedPrompt, _messages.Select(m => (m.FromUser, m.Text)), text);
        _messages.Add(new Message(false, reply, DateTime.Now));
        StateHasChanged();
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await Send();
    }

    private async Task<string> BuildContextualSystemPromptAsync()
    {
        try
        {
            var items = (await InventoryRepo.GetAllAsync()).ToList();
            var eventsList = (await EventRepo.GetAllAsync()).ToList();
            var rentals = (await RentalRepo.GetAllAsync()).ToList();

            var now = DateTime.Now;
            var overdue = rentals
                .Where(r => r.Status != RentalOrderStatus.Returned && r.ExpectedReturnDate.Date < now.Date)
                .OrderBy(r => r.ExpectedReturnDate)
                .ToList();

            // Inventory: include all items with full status
            var itemLines = items
                .OrderBy(i => i.Name)
                .Select(i =>
                    $"- {i.Id} | {i.Name} | Avail: {i.AvailableQuantity}/{i.TotalQuantity} | Availability: {i.AvailabilityStatus} | RentalStatus: {i.RentalStatus} | Condition: {i.Condition} |  {(string.IsNullOrWhiteSpace(i.ConditionDescription) ? string.Empty : $"({i.ConditionDescription})")}");

            // Events: include items per event
            var eventBlocks = eventsList
                .OrderBy(e => e.StartDate)
                .Select(e => new
                {
                    Header =
                        $"- EVENT #{e.Id}: {e.Name} | {e.StartDate:yyyy-MM-dd} → {e.EndDate:yyyy-MM-dd} | Items: {e.EventInventoryItems?.Count ?? 0}",
                    Lines = (e.EventInventoryItems ?? new List<EventInventoryItem>())
                        .Select(ei => $"    • ItemId:{ei.InventoryItemId} Qty:{ei.Quantity}")
                });

            // Rentals: include items per rental
            var rentalBlocks = rentals
                .OrderBy(r => r.ExpectedReturnDate)
                .Select(r => new
                {
                    Header =
                        $"- RENTAL #{r.RentalId}: {r.ClientName} | Status:{r.Status} | Payment:{r.PaymentStatus} | Dates: {r.RentalDate:yyyy-MM-dd} → {r.ExpectedReturnDate:yyyy-MM-dd}{(r.ActualReturnDate.HasValue ? $" (Returned:{r.ActualReturnDate.Value:yyyy-MM-dd})" : string.Empty)}",
                    Lines = (r.RentalItems ?? new List<RentalItem>())
                        .Select(ri =>
                            $"    • ItemId:{ri.InventoryItemId} QtyRented:{ri.QuantityRented} QtyReturned:{ri.QuantityReturned} Price/Day:{ri.PricePerDayAtTimeOfRental}")
                });

            var sb = new StringBuilder();
            sb.AppendLine(SystemPrompt);
            sb.AppendLine();
            sb.AppendLine("CRITICAL RULES:");
            sb.AppendLine(
                "- Answer ONLY using the data in the CONTEXT below. If something is not present, say you don't have that information and offer to check or ask a human.");
            sb.AppendLine("- Do NOT invent item names, events, quantities, clients, dates, or locations.");
            sb.AppendLine("- Be clear and structured. Provide helpful detail without inventing facts. If a list is long, summarize counts and highlight the most relevant items.");
            sb.AppendLine();
            sb.AppendLine("CONTEXT BEGIN");
            sb.AppendLine($"Now: {now:yyyy-MM-dd HH:mm}");
            sb.AppendLine();
            sb.AppendLine("Inventory (ALL ITEMS):");
            foreach (var line in itemLines) sb.AppendLine(line);
            sb.AppendLine();
            sb.AppendLine("Events (ALL, WITH ITEMS):");
            foreach (var block in eventBlocks)
            {
                sb.AppendLine(block.Header);
                foreach (var ln in block.Lines) sb.AppendLine(ln);
            }

            sb.AppendLine();
            sb.AppendLine("Rentals (ALL, WITH ITEMS):");
            foreach (var block in rentalBlocks)
            {
                sb.AppendLine(block.Header);
                foreach (var ln in block.Lines) sb.AppendLine(ln);
            }

            sb.AppendLine();
            sb.AppendLine("Overdue Rentals:");
            if (overdue.Any())
                foreach (var r in overdue)
                    sb.AppendLine($"- RENTAL #{r.RentalId} | {r.ClientName} | Due: {r.ExpectedReturnDate:yyyy-MM-dd}");
            else sb.AppendLine("- None");
            sb.AppendLine("CONTEXT END");

            sb.AppendLine();
            sb.AppendLine(
                "If the user asks to 'prep gear' for an event, verify each requested item exists and has sufficient AvailableQuantity; if not, report the shortfall. Do not assign times/locations that are not in the context.");

            return sb.ToString();
        }
        catch
        {
            // Fallback: still enforce non-fabrication even if context fetch fails
            return $"{SystemPrompt}\nRULE: If you lack data, say so. Do not fabricate details.";
        }
    }

    private record Message(bool FromUser, string Text, DateTime At);
}