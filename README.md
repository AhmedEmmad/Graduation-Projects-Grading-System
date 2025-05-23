<h1> Graduation-Projects-Grading-System</h1>

# System Design:

### High-Level Architecture

<b>Graduation Projects Grading System</b> is a modern web application built using a layered architecture pattern(Onion Architecture), following clean architecture principles with clear separation of concerns. The system is designed to handle academic grading processes, student management, and administrative tasks for educational institutions.

##### Architecture Layers

The system is structured into four main layers:
    - Presentation Layer (GraduationProjectsGradingSystem.APIs)
        Built using ASP.NET Core 8.0, this layer is the entry point for client interactions.
         - Exposes RESTful API endpoints for seamless communication.
         - Manages HTTP requests and responses.
         - Utilizes SignalR for real-time notifications.
         - Provides Swagger/OpenAPI documentation for API exploration.
         - Implements JWT-based authentication for secure access.

    - Application Layer (GraduationProjectsGradingSystem.Service)
        Encapsulates the system's business logic and orchestrates data flow.
         - Implements service interfaces for modular functionality.
         - Handles data transformation and validation.
         - Manages cross-cutting concerns (e.g., logging, error handling).
         - Coordinates interactions between the presentation layer and data access layer.

    - Domain Layer (GraduationProjectsGradingSystem.Core)
        Defines the core business entities and rules, independent of external frameworks.
         - Contains entity models representing system data.
         - Defines domain interfaces for consistent abstractions.
         - Enforces business rules and validation logic.
         - Manages identity and access control policies.

    - Data Access Layer (GraduationProjectsGradingSystem.Repository)
        Handles all database interactions and persistence logic.
         - Utilizes Entity Framework Core for ORM and LINQ for querying.
         - Implements data access patterns (e.g., Repository pattern).
         - Manages database context and migrations.
         - Executes database operations efficiently.
