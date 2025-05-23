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
