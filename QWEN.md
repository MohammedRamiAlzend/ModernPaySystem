# ModernPaySystem

## Project Overview

ModernPaySystem is a modern payment processing system built using .NET 10 with a clean architecture approach. The system follows SOLID principles and implements a layered architecture to ensure separation of concerns, maintainability, and scalability.

## Architecture

The system follows a 5-layer architecture pattern:

### 1. Presentation Layer (`ModernPaySystem`)
- Main web API project using ASP.NET Core
- Contains controllers for handling HTTP requests
- Configures services and middleware
- Uses JWT authentication and authorization policies
- Implements custom permission middleware

### 2. Application Layer (`ModernPaySystem.Application`)
- Contains business logic abstractions
- Defines DTOs (Data Transfer Objects) for data exchange
- Defines interfaces for services and repositories
- Contains use cases and application services
- Acts as a bridge between presentation and domain layers

### 3. Domain Layer (`ModernPaySystem.Domain`)
- Contains business entities and domain models
- Defines domain interfaces and abstractions
- Implements business rules and validation
- Contains shared entities (User, Role, Permission, etc.)
- Contains transaction system entities (Request, Response, Template, etc.)

### 4. Infrastructure Layer (`ModernPaySystem.Infrastructure`)
- Implements cross-cutting concerns
- Handles authentication and authorization
- Provides infrastructure services
- Contains extension methods and service registrations
- Manages JWT token generation and validation

### 5. Persistence Layer (`ModernPaySystem.Infrastructure.Persistence`)
- Handles data persistence using Entity Framework Core
- Contains database context (AppDbContext)
- Implements repositories and unit of work pattern
- Manages database migrations
- Contains seeding logic for initial data

## Technologies Used

- **.NET 10**: Latest version of the .NET platform
- **ASP.NET Core**: Web framework for building APIs
- **Entity Framework Core 10.0.2**: ORM for data access
- **SQL Server**: Primary database system
- **JWT Authentication**: Token-based authentication
- **Scalar.AspNetCore**: API documentation tool
- **Bogus**: Data generation library for seeding

## Key Features

### Authentication & Authorization
- JWT-based authentication system
- Role-based access control
- Custom permission system with middleware
- Secure password hashing

### Entity Model
- **User Management**: User accounts with roles and permissions
- **Request/Response System**: Transaction workflow management
- **Template System**: Reusable transaction templates
- **Attachment Support**: File attachments for requests/responses
- **Sub-system Integration**: Support for multiple subsystems

### Data Models
- **Shared Entities**: User, Role, Permission, SubSystem, Attachment
- **Transaction Entities**: Request, Response, Template, RequestAttachment, ResponseAttachment
- **Relationships**: Many-to-many relationships between users/roles, roles/permissions, etc.

## Building and Running

### Prerequisites
- .NET 10 SDK
- SQL Server (local or remote instance)

### Setup Instructions
1. Clone the repository
2. Navigate to the solution directory
3. Restore packages: `dotnet restore`
4. Update database: `dotnet ef database update`
5. Run the application: `dotnet run`

### Configuration
- Connection string configured in `appsettings.json`
- JWT settings for authentication
- Seeding configuration for development data

## Development Conventions

### Coding Standards
- Follows .NET coding conventions
- Uses nullable reference types
- Implements dependency injection throughout
- Uses async/await for I/O operations

### Architecture Patterns
- Clean Architecture principles
- Repository pattern for data access
- Unit of Work pattern for transaction management
- Service layer abstraction
- DTOs for API contracts

### Security Practices
- Password hashing for user credentials
- JWT tokens with expiration
- Role-based authorization
- Custom permission system

## Project Structure

```
ModernPaySystem/
├── ModernPaySystem/                 # Presentation layer (Web API)
│   ├── Controllers/                 # API controllers
│   ├── Properties/
│   ├── appsettings.json            # Configuration
│   └── Program.cs                  # Startup configuration
├── ModernPaySystem.Application/     # Application layer
│   ├── DTOs/                       # Data transfer objects
│   ├── Interfaces/                 # Service interfaces
│   ├── Repos/                      # Repository interfaces
│   ├── Services/                   # Application services
│   └── UseCases/                   # Business use cases
├── ModernPaySystem.Domain/          # Domain layer
│   ├── Entities/                   # Domain entities
│   │   ├── Abstraction/            # Base entity classes
│   │   ├── SharedEntities/         # Shared domain entities
│   │   └── TransactionSystemEntities/ # Transaction-specific entities
│   ├── Interfaces/                 # Domain interfaces
│   └── Commons/                    # Common utilities
├── ModernPaySystem.Infrastructure/  # Infrastructure layer
│   ├── Auth/                       # Authentication services
│   ├── Extensions/                 # Extension methods
│   └── Services/                   # Infrastructure services
└── ModernPaySystem.Infrastructure.Persistence/ # Persistence layer
    ├── Migrations/                 # EF Core migrations
    ├── Repos/                      # Repository implementations
    ├── Seeding/                    # Data seeding logic
    ├── UnitOfWork/                 # Unit of work implementation
    ├── AppDbContext.cs             # Database context
    └── PersistenceServiceRegistration.cs # Service registration
```

## API Documentation

The system uses Scalar.AspNetCore for API documentation, accessible when running in development mode.

## Database Schema

The system uses Entity Framework Core with SQL Server. The main entities include:
- Users with roles and permissions
- Transaction requests and responses
- Templates for transactions
- Attachments for requests/responses
- Sub-system integration tables

## Seeding

The application includes a seeding mechanism that can populate the database with sample data during startup, configurable through appsettings.json.

## Testing

While not explicitly shown in the current structure, the architecture supports unit testing through its layered design and dependency injection patterns.