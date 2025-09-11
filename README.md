# Event Inventory Manager

A web application for managing inventory and equipment for event companies. This solution replaces the current Excel-based system, providing efficient tools for tracking equipment, managing rentals, and planning events.

## Features

### Inventory Management
- Add, edit, and delete inventory items
- Track item status and location
- Export inventory to PDF for field work

### Equipment Rental
- Check-out and check-in equipment
- Track rentals (who, what, return date)
- View active rentals and due dates

### Event Planning
- Create event checklists with required equipment
- Automatic availability checking for items
- Conflict detection for double-booked equipment

### Loading Optimization
- Smart algorithm to sort items for loading (heaviest/largest first)
- Optimized packing lists for events
- Visual loading guide

### AI Assistant (Planned)
- Natural language queries (e.g., "Where is X?", "How many tables are available?")
- Event setup suggestions
- Equipment recommendations based on event type


## Getting Started

### Prerequisites
- .NET 8.0 SDK 


### Installation
1. Clone the repository
2. Restore NuGet packages
3. Update the connection string in `appsettings.json`
4. Run database migrations
5. Start the application

## Project Structure
- **WarehouseManager.Core**: Domain models and business logic
- **WarehouseManager.Application**: Application services and use cases
- **WarehouseManager.Infrastructure**: Data access and external services
- **WarehouseManager.Web**: Web interface and API endpoints
