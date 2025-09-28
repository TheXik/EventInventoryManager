# Event Inventory Manager - Developer Guide

This guide provides technical documentation for developers working on the Event Inventory Manager project.

## Project Overview

Event Inventory Manager is a web-based warehouse management system built with ASP.NET Core Blazor. It helps event companies manage inventory, plan events, handle rentals, and track equipment allocation.

### Core Functionality
- Inventory management with categories and conditions
- Event planning with item allocation
- Rental system with status tracking
- AI-powered assistant for queries
- Dashboard with statistics and basic overview

---

## Architecture

The project follows Clean Architecture (or so called Onion architecture) principles with clear separation of concerns:



### Dependency Flow
- Web depends on Infrastructure and Application
- Infrastructure depends on Application and Core
- Application depends on Core
- Core has no dependencies only business logic


---

## Technology Stack

### Backend
- .NET 8
- ASP.NET Core Blazor Server
- SQLite Database

### Frontend
- Radzen Blazor Components
- Bootstrap CSS
- Font Awesome Icons

### External Services
- Google Gemini AI (for chatbot)
- SendGrid (for email notifications)

---


## Setup and Development

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Git

### Useful commands

#### Update database 

- dotnet ef database update --startup-project WarehouseManager.Web --project WarehouseManager.Infrastructure

#### Add new migration 
- dotnet ef migrations add MigrationName --startup-project WarehouseManager.Web --project WarehouseManager.Infrastructure


#### Configuration
- AI service: Set Gemini API key in user secrets
- Email service: Configure SendGrid in user secrets

---

## Database

### Key Entities
- InventoryItem: Warehouse equipment with quantities and conditions
- Event: Scheduled events with client information
- EventInventoryItem: Many-to-many relationship between events and items
- Rental: Customer rental records
- RentalItem: Items within rentals
- ItemCategory: Categorization of inventory items

### Relationships
- Event 1:N EventInventoryItem N:1 InventoryItem
- Rental 1:N RentalItem N:1 InventoryItem
- InventoryItem N:1 ItemCategory

---

## Code Structure

### Core Layer
- Entities: InventoryItem, Event, Rental, etc.
- Enums: AvailabilityStatus, Condition, PaymentStatus
- Business logic and validation

### Application Layer
- Repository interfaces
- Service contracts
- Use case definitions

### Infrastructure Layer
- Repository implementations
- Database context
- External services (AI, Email)
- Migrations

### Web Layer
- Blazor components
- Page components
- Layout and navigation
- State management

---



## Some developer patterns that I used might be helpful to keep this project using the same conventions

### Repository Pattern Implementation

The project uses a consistent repository pattern with the following structure:

#### Interface Definition (Application Layer)
```csharp
public interface IInventoryItemRepository
{
    Task<InventoryItem?> GetByIdAsync(int id);
    Task<IEnumerable<InventoryItem>> GetAllAsync();
    Task AddAsync(InventoryItem item);
    Task UpdateAsync(InventoryItem item);
    Task DeleteAsync(int id);
    // and more but these are the basic
}
```

#### Repository Implementation (Infrastructure Layer)
```csharp
public class InventoryItemRepository : IInventoryItemRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryItem?> GetByIdAsync(int id)
    {
        return await _context.InventoryItems.FindAsync(id);
    }
    
}
```

#### Key Repository Conventions:
- All methods are async and return Task or generic tasks
- Always call SaveChangesAsync() after modifications

### Blazor Component Architecture

#### Component Structure Pattern

```csharp
public partial class ComponentName
{
    // Constants
    private const int MaxItems = 10;
    
    // State Management
    private bool _isLoading = true;
    private string? _errorMessage;
    private List<Entity> _items = new();
    
    // UI State
    private bool _showModal;
    private Entity? _selectedItem;
    
    // Dependency Injection
    [Inject] private IRepository Repository { get; set; } = default!;
    [Inject] private ILogger<ComponentName> Logger { get; set; } = default!;
    
    // Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }
    
    // Private Methods
    private async Task LoadData()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            _items = (await Repository.GetAllAsync()).ToList();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load data: {ex.Message}";
            Logger.LogError(ex, "Error loading data");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
}
```


### Dependency Injection Setup

#### Service Registration Pattern
```csharp
// Repository registrations
builder.Services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();

// External service configuration
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));
builder.Services.AddScoped<IChatAiService, ChatAiService>();



```

### Common Development Tasks

#### Adding a New Entity
1. Create entity in `Core/Entities/`
2. Add DbSet to `ApplicationDbContext`
3. Create repository interface in `Application/Interfaces/`
4. Implement repository in `Infrastructure/Repositories/`
5. Register service in `Program.cs`
6. Create migration: `dotnet ef migrations add AddNewEntity`
7. Update database: `dotnet ef database update`

#### Adding a New Blazor Page
1. Create `.razor` and `.razor.cs` files in `Web/Components/Pages/`
2. Follow the component structure pattern above
3. Add navigation link in `NavMenu.razor`
4. Register any new services in `Program.cs`

---

## Improvement Possibilities

### Features
- Implement advanced reporting
- Create mobile app
- Add real-time notifications
- Implement audit logging
- Improve AI with features like (adding, deleting, creating events etc..)

### User Experience
- Implement bulk operations
- Create custom dashboards
- Add data export functionality





