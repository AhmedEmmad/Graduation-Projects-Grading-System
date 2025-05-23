# Graduation Projects Grading System

The **Graduation Projects Grading System** is built to simplify and enhance the grading process for graduation projects. Leveraging the **Onion Architecture and Clean Architecture principles**, the system ensures modularity, scalability, and maintainability. It provides a robust backend with RESTful APIs, real-time updates, and secure authentication, making it an efficient solution for academic institutions.

## System Design

### High-Level Architecture

The system is structured using the Onion Architecture, which promotes separation of concerns by organizing the codebase into four core layers. Each layer has distinct responsibilities, ensuring modularity and ease of maintenance.


#### 1. Presentation Layer (GraduationProjectsGradingSystem.APIs)
The entry point for client interactions, built with ASP.NET Core 8.0 for high performance and cross-platform compatibility.

**Responsibilities:**
- Exposes **RESTful API endpoints** for seamless client-server communication.
- Handles **HTTP requests and responses** with robust error handling.
- Uses **SignalR** for real-time notifications (e.g., send instructions to doctors).
- Provides **Swagger/OpenAPI documentation** at the /swagger endpoint for **API exploration and testing**.
- Implements **JWT-based authentication** to secure endpoints and manage user sessions.
- Custom **Error Handling Of All Program** Middleware.

**Technologies:**
- .NET Core (8.0)
- Microsoft.AspNetCore.SignalR (12.0)
- Swashbuckle.AspNetCore (for Swagger) (6.6.2)
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.11)
- Microsoft.AspNetCore.Mvc.NewtonsoftJson (8.0.11)
- Microsoft.EntityFrameworkCore (9.0.0)
- EPPlus (8.0.3)
- Microsoft.EntityFrameworkCore.SqlServer (9.0.0)
- Microsoft.EntityFrameworkCore.Tools (9.0.0)


#### 2. Application Layer (GraduationProjectsGradingSystem.Service)
Encapsulates the system's business logic, orchestrating data flow and ensuring modular functionality.

**Responsibilities:**
- Implements **service interfaces** for reusable and testable business logic.
- Manages **data transformation and validation** to ensure data integrity.
- Handles **cross-cutting concerns** like error handling.
- Acts as an intermediary between the **presentation and data access layers**.

**Technologies:**
- .NET Core (8.0)
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.11)


#### 3. Domain Layer (GraduationProjectsGradingSystem.Core)
The core of the system, defining business entities and rules, independent of external frameworks for maximum portability.

**Responsibilities:**
- Defines **entity models**.
- Specifies **domain interfaces** for consistent abstractions.
- Enforces **business rules and validation logic** to maintain data consistency.
- Manages **identity and access control policies** for secure user management.

**Technologies:**
- .NET Core (8.0)
- Microsoft.AspNetCore.Identity.EntityFrameworkCore (8.0.11)


#### 4. Data Access Layer (GraduationProjectsGradingSystem.Repository)
Handles all database interactions and persistence logic, ensuring efficient data operations.

**Responsibilities:**
- Uses **Entity Framework Core** for Object-Relational Mapping (ORM) and **LINQ** for expressive querying.
- Implements the **Repository pattern** to abstract data access logic.(Implements all repositories interfaces)
- Manages **database context and migrations** for schema consistency.
- Optimizes **database operations** for performance.
- **Seeding data** as hardcoded. 

**Technologies:**
- Entity Framework Core (9.0.0)
- Microsoft.EntityFrameworkCore.SqlServer (9.0.0)
- Microsoft.EntityFrameworkCore.Tools (9.0.0)
- Microsoft.Extensions.Configuration (9.0.4)
- Microsoft.AspNetCore.ldentity.EntityFrameworkCore (8.0.11)


### Key Features
- **Scalable Architecture:** The layered design supports easy extension and maintenance.
- **Real-Time Updates:** SignalR enables instant notifications.
- **Secure Access:** JWT authentication ensures role-based access control (e.g., Admin, Doctor, Student).
- **Developer-Friendly:** Swagger integration simplifies API testing and integration.
- **Efficient Data Management:** Entity Framework Core and the Repository pattern streamline database operations.

This architecture ensures the **Graduation Projects Grading System** is a reliable, secure, and efficient solution for managing academic grading processes.

---

## 🏛️ Technologies Used

This section outlines the technologies, tools, and frameworks used to build the **Graduation Projects Grading System**, a secure, scalable, and high-performance backend platform. The stack leverages modern **.NET 8** technologies to ensure cross-platform compatibility, real-time functionality, and robust API development. The system is structured using the **Onion Architecture**, with each layer utilizing specific packages and tools to enhance functionality.

### Core Technologies

The backend is built on a foundation of .NET 8 and complementary frameworks to handle business logic, data access, real-time communication, and authentication.

| Technology | Purpose | Usage |
|------------|---------|-------|
| **.NET 8** | Main framework for cross-platform services | Implements backend logic, APIs, and services using C#. |
| **ASP.NET Core 8.0** | Framework for RESTful APIs and web apps | Manages HTTP requests, routing, controllers, and middleware for the API layer. |
| **Entity Framework Core 9.0** | Object-Relational Mapper (ORM) | Handles database access, migrations, and code-first data modeling for efficient data operations. |
| **SQL Server / MonsterDatabase** | Relational database management | Development: Local SQL Server. Production: MonsterDatabase via MonsterASP.NET for persistent storage. |
| **SignalR** | Real-time web functionality | Enables instant notifications and live updates (e.g., grading updates) via WebSockets. |
| **ASP.NET Core Identity** | Authentication and authorization | Manages user accounts, roles (Admin, Doctor, Student), and JWT-based authentication. |

> **Note**: .NET 8 ensures high performance and cross-platform compatibility, while SignalR provides seamless real-time features for dynamic user experiences.

### Layer-Specific Packages

The backend is organized into four layers (API, Core, Repository, and Service), each leveraging specific NuGet packages to enhance functionality. Below is an updated list of packages, reflecting the latest versions as of May 2025 (based on typical .NET release cycles and the provided data).

#### API Layer
The **Presentation Layer** (`GraduationProjectsGradingSystem.APIs`) handles client interactions and exposes RESTful APIs.

| Package | Version | Purpose | Usage |
|---------|---------|---------|-------|
| `EPPlus` | 8.0.3 | Spreadsheet library | Generates Excel reports for data exports (e.g., grading reports). |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.11 | JWT authentication middleware | Secures APIs with bearer token authentication. |
| `Microsoft.AspNetCore.Mvc.NewtonsoftJson` | 8.0.11 | JSON handling | Supports JSON serialization and JSON Patch formatting for APIs. |
| `Microsoft.AspNetCore.SignalR` | 8.0.11 | Real-time communication | Powers SignalR hubs for real-time notifications. |
| `Microsoft.EntityFrameworkCore` | 9.0.0 | ORM framework | Manages data access for API endpoints. |
| `Microsoft.EntityFrameworkCore.SqlServer` | 9.0.0 | SQL Server provider | Connects APIs to SQL Server or MonsterDatabase. |
| `Microsoft.EntityFrameworkCore.Tools` | 9.0.0 | EF Core tools | Supports database migrations and schema management. |
| `Swashbuckle.AspNetCore` | 6.8.0 | Swagger tools | Provides interactive API documentation via Swagger UI at `/swagger`. |

#### Core Layer
The **Domain Layer** (`GraduationProjectsGradingSystem.Core`) defines business entities and rules, independent of external frameworks.

| Package | Version | Purpose | Usage |
|---------|---------|---------|-------|
| `Microsoft.AspNetCore.Http.Features` | 8.0.11 | HTTP feature interfaces | Provides abstractions for HTTP features used in services. |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 8.0.11 | ASP.NET Core Identity with EF Core | Manages user identities, roles, and persistence. |

#### Repository Layer
The **Data Access Layer** (`GraduationProjectsGradingSystem.Repository`) handles database interactions using the Repository pattern.

| Package | Version | Purpose | Usage |
|---------|---------|---------|-------|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 8.0.11 | Identity persistence | Persists user and role data to the database. |
| `Microsoft.EntityFrameworkCore` | 9.0.0 | ORM framework | Provides ORM capabilities for database operations. |
| `Microsoft.EntityFrameworkCore.SqlServer` | 9.0.0 | SQL Server provider | Enables connectivity to SQL Server or MonsterDatabase. |
| `Microsoft.EntityFrameworkCore.Tools` | 9.0.0 | EF Core tools | Supports database migrations and schema management. |
| `Microsoft.Extensions.Configuration` | 9.0.4 | Configuration management | Manages database connection strings and settings. |

#### Service Layer
The **Application Layer** (`GraduationProjectsGradingSystem.Service`) encapsulates business logic and coordinates data flow.

| Package | Version | Purpose | Usage |
|---------|---------|---------|-------|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.11 | JWT authentication middleware | Enables JWT-based authentication for services. |

### API Development and Documentation

The backend exposes RESTful APIs with comprehensive documentation and testing capabilities.

| Tool | Purpose | Usage |
|------|---------|-------|
| **Swagger (Swashbuckle)** | API documentation and testing | Provides interactive API documentation at the `/swagger` endpoint, allowing developers to explore and test endpoints. |
| **Postman** | API testing and collaboration | Used for manual API testing; Postman collections are available in the `docs` folder for automated testing workflows. |

> **Tip**: Developers can explore APIs via Swagger UI at `https://localhost:<port>/swagger` or import Postman collections for streamlined testing.

### Supporting Tools and Libraries

Additional tools enhance development, maintainability, and scalability.

| Tool/Library | Purpose | Usage |
|--------------|---------|-------|
| **Dependency Injection** | Service management | Uses .NET Core’s built-in DI container to manage service lifetimes and promote loose coupling. |
| **Microsoft.Extensions.Logging** | Diagnostics and monitoring | Provides structured logging for debugging and monitoring application behavior. |
| **CORS** | Cross-origin requests | Configured to allow secure communication between the backend and frontend applications. |
| **Microsoft.AspNetCore.Mvc** | API endpoint development | Builds controllers and API endpoints for RESTful interactions. |
| **Microsoft.AspNetCore.Cors** | CORS policy management | Manages cross-origin resource sharing policies for secure API access. |
| **Microsoft.AspNetCore.SignalR** | Real-time communication | Powers SignalR hubs for live updates (e.g., grading notifications). |
| **Microsoft.EntityFrameworkCore.Tools** | Database management | Supports migrations and schema updates for Entity Framework Core. |

### Development and Deployment

The backend is designed for both development and production environments with distinct configurations.

#### Development
- **Environment**: Localhost
- **Database**: SQL Server
- **IDE**: Visual Studio 2022 or Visual Studio Code (see [VS Code Setup](vscode.md))
- **Version Control**: Git & GitHub for collaboration and code management

#### Production
- **Hosting**: MonsterASP.NET
- **Database**: MonsterDatabase for persistent storage
- **Scalability**: Leverages MonsterASP.NET’s infrastructure for high availability and load balancing

> **Note**: The production environment ensures scalability and reliability, while the local development setup allows for rapid iteration and testing.

### Updates and Considerations (May 2025)

To ensure the system remains up-to-date:
- **NuGet Packages**: The listed package versions (e.g., `Microsoft.EntityFrameworkCore 9.0.0`, `Swashbuckle.AspNetCore 6.8.0`) are based on the latest stable releases as of May 2025. Check [NuGet.org](https://www.nuget.org) for newer versions and update via `dotnet add package`.
- **.NET 8 Support**: .NET 8 is under long-term support (LTS) until November 2026. Monitor [Microsoft’s .NET roadmap](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support) for .NET 9 updates.
- **SignalR**: Ensure compatibility with the latest ASP.NET Core version for real-time features. Upgrade to `Microsoft.AspNetCore.SignalR 8.0.11` if not already applied.
- **MonsterDatabase**: Verify MonsterASP.NET’s documentation for any updates to MonsterDatabase configurations or hosting requirements.

### Summary

The **Graduation Projects Grading System** backend combines **.NET 8**, **ASP.NET Core**, and **Entity Framework Core**, augmented by layer-specific NuGet packages, to deliver a robust, secure, and maintainable platform. **SignalR** enables real-time updates, **Swagger** provides comprehensive API documentation, and structured logging ensures a high-quality developer experience. The system is designed for seamless integration with frontend applications and supports future scalability through its modular Onion Architecture.

For further details, refer to the [System Design](#system-design) section or explore the repository at [https://github.com/AhmedEmmad/Graduation-Projects-Grading-System](https://github.com/AhmedEmmad/Graduation-Projects-Grading-System).
