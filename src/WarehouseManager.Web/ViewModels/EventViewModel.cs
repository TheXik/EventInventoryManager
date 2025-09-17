using System.ComponentModel.DataAnnotations;
using WarehouseManager.Core.Enums;

namespace WarehouseManager.Web.ViewModels;

public class EventViewModel
{
    public int Id { get; set; }

    [Required] public string Name { get; set; }

    [Required] public DateTime? StartDate { get; set; }

    [Required] public DateTime? EndDate { get; set; }

    public string? ClientName { get; set; }
    public string? Location { get; set; }
    public EventStatus EventStatus { get; set; }
}