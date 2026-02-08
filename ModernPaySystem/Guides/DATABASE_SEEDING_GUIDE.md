# Database Seeding System - Implementation Guide

## Overview

A comprehensive database seeding system has been created to support:
- Multiple environments (Development & Production)
- Configurable data quantities
- SOLID principles implementation
- Bogus 35.6.5 for random data generation
- Proper dependency ordering
- Entity relationships

## Architecture

### Components

1. **SeedingConfiguration**
   - Configures seeding behavior
   - Supports environment-specific defaults
   - Controls quantities for each entity

2. **IEntitySeeder Interface**
   - Defines seeding contract
   - Supports dependency ordering
   - Checks for existing data

3. **Entity Seeders**
   - PermissionSeeder (Order: 1)
   - RoleSeeder (Order: 2)
   - UserSeeder (Order: 3)
   - TemplateSeeder (Order: 4)
   - RequestSeeder (Order: 5)
   - ResponseSeeder (Order: 6)
   - AttachmentSeeder (Order: 7)

4. **SeederOrchestrator**
   - Manages seeding workflow
   - Handles dependencies
   - Provides logging
   - Can clear database

## Configuration (appsettings.json)

```json
{
  "Seeding": {
    "Enabled": true,
    "Environment": "Development",
    "ClearExistingData": false,
    "Quantities": {
      "RoleCount": 5,
      "PermissionCount": 20,
      "UserCount": 25,
      "TemplateCount": 10,
      "RequestCount": 50,
      "ResponseCount": 30,
      "AttachmentCount": 20
    }
  }
}
```

## Usage

### Enable Seeding on Startup

In `Program.cs`, the seeding is automatically triggered:

```csharp
if (builder.Configuration.GetValue<bool>("Seeding:Enabled"))
{
    using (var scope = app.Services.CreateScope())
    {
        var orchestrator = scope.ServiceProvider.GetRequiredService<ISeederOrchestrator>();
        await orchestrator.SeedDatabaseAsync();
    }
}
```

### Seeding Order

Entities are seeded in dependency order:

```
1. Permissions (no dependencies)
2. Roles (no dependencies)
3. Users (depends on Roles)
4. Templates (no dependencies)
5. Requests (depends on Users, Templates)
6. Responses (depends on Users, Requests)
7. Attachments (no dependencies)
```

## SOLID Principles Implementation

### Single Responsibility
- Each seeder handles one entity type
- Clear separation of concerns

### Open/Closed
- Easy to add new seeders
- Closed for modification of existing code

### Liskov Substitution
- All seeders implement IEntitySeeder
- Can be used interchangeably

### Interface Segregation
- Minimal IEntitySeeder interface
- Only necessary methods

### Dependency Inversion
- Depends on abstractions, not concretions
- Injected via DI container

## Features

✅ Environment-aware configuration  
✅ Configurable data quantities  
✅ Bogus integration for realistic data  
✅ Dependency ordering  
✅ Duplicate prevention  
✅ Logging support  
✅ Error handling  
✅ Optional data clearing  

## Environment-Specific Defaults

### Development
- More test data for testing
- Configured quantities in appsettings

### Production
- Minimal required data
- Overrides applied automatically

## Bogus Integration

Bogus 35.6.5 generates realistic random data:

```csharp
// Permissions
- Fixed list of system permissions

// Roles
- Fixed list of role names

// Users
- Random usernames (Internet.UserName())
- Random hashed passwords

// Templates
- Random product names
- Random lorem ipsum content

// Requests
- Random from existing Users
- Random from existing Templates

// Responses
- Random from approved Requests
- Random from existing Users

// Attachments
- Random file names
- Random file sizes
- Random file types
```

## Adding New Seeders

1. Create new seeder class inheriting from `EntitySeederBase<T>`
2. Implement `Order` property (ordering)
3. Implement `SeedAsync` method (seeding logic)
4. Register in `SeedingServiceRegistration`

Example:

```csharp
public class CustomSeeder : EntitySeederBase<CustomEntity>
{
    public override int Order => 10;

    public override async Task SeedAsync(AppDbContext context, SeedingConfiguration configuration)
    {
        var items = GenerateData(configuration.Quantities.CustomCount);
        await AddEntitiesAsync(context, items);
    }

    private List<CustomEntity> GenerateData(int count)
    {
        // Generate using Bogus
        return new List<CustomEntity>();
    }
}
```

## Configuration in appsettings.json

```json
"Seeding": {
  "Enabled": true,
  "Environment": "Development",
  "ClearExistingData": false,
  "Quantities": {
    "RoleCount": 5,
    "PermissionCount": 20,
    "UserCount": 25,
    "TemplateCount": 10,
    "RequestCount": 50,
    "ResponseCount": 30,
    "AttachmentCount": 20
  }
}
```

## Development vs Production

Development mode creates more data for testing:
- 5 roles
- 20 permissions
- 25 users
- 10 templates
- 50 requests
- 30 responses
- 20 attachments

Production mode creates minimal data:
- 3 roles (Admin, Manager, User)
- 10 permissions
- 5 users
- 2 templates
- 5 requests
- 3 responses
- 2 attachments

## Logging

Seeders provide comprehensive logging:

```
info: SeederOrchestrator - Starting database seeding...
info: SeederOrchestrator - Seeding Permission...
info: SeederOrchestrator - Successfully seeded Permission
info: SeederOrchestrator - Skipping Role seeding - data already exists
...
info: SeederOrchestrator - Database seeding completed successfully
```

## Error Handling

- Validates dependencies (Roles before Users)
- Logs errors during seeding
- Prevents duplicate seeding
- Optional data clearing

## Next Steps

1. Configure `appsettings.json` with desired quantities
2. Set environment (Development/Production)
3. Run application - seeding happens automatically
4. Disable seeding by setting `Enabled: false` if needed

## Disabling Seeding

In `appsettings.json`:

```json
"Seeding": {
  "Enabled": false
}
```

Or in `appsettings.Production.json`:

```json
{
  "Seeding": {
    "Enabled": false
  }
}
```

---

**Status:** Implementation ready with 7 seeders  
**Package:** Bogus 35.6.5  
**Principles:** SOLID  
**Environments:** Development & Production
