# Geospatial Collectible Exchange Tracker (Backend Architecture)

## Project Overview

The Geospatial Collectible Exchange Tracker is a highly performant, backend-focused system designed to address inventory management and spatial querying challenges from location-based gaming. Built with an API-first approach, the system provides a robust centralized platform for tracking item ownership, executing complex geospatial queries, and managing secure, asynchronous exchange transactions.

## Core Features

- **Advanced Geospatial Queries:** Leverages PostGIS to perform complex spatial operations, allowing the system to calculate precise distances and instantly retrieve collectibles within a specific radius of a user's coordinates.
- **Microservices-Oriented Background Processing:** Integrates a lightweight, high-concurrency Go (Golang) background worker to handle automated system tasks, such as spawning geo-collectibles and cleaning up expired transactions, completely decoupled from the main API.
- **Bidirectional Exchange State Machine:** A robust transaction engine that manages the lifecycle of item exchanges (Pending, Sent, Completed), ensuring data integrity and preventing double-spending of virtual assets.
- **Secure Authentication & Data Flow:** Implements stateless JWT authentication and BCrypt password hashing. Enforces strict Separation of Concerns (SoC) using Data Transfer Objects (DTOs) and Entity Framework LINQ projections to optimize database payload and prevent N+1 query issues.

## System Architecture

The application is built on a decoupled architecture, demonstrating enterprise-level backend patterns:

### Architectural Components

1.  **Core Web API (.NET 8):** The primary entry point for client requests. Structured into Controllers and Data Access layers, handling business logic, authentication, and optimized HTTP responses.
2.  **Asynchronous Worker (Go):** A standalone background service running on timed intervals (cron jobs) to perform heavy database write operations and system maintenance without impacting the main API's read throughput.
3.  **Geospatial Database (PostgreSQL + PostGIS):** Serves as the single source of truth, utilizing spatial indexing to perform mathematical geometry calculations directly at the database level.
4.  **Distributed Caching (Redis):** Implements the Cache-Aside pattern to store frequently accessed data (e.g., global leaderboards or high-traffic coordinate grids), drastically reducing database overhead and latency.

## Technology Stack

### API & Core Logic

- **Framework:** C# .NET 8 Web API
- **ORM & Data Access:** Entity Framework Core
- **Security:** JSON Web Tokens (JWT), BCrypt.Net

### Background Service (Microservice)

- **Language:** Go (Golang)
- **Database Driver:** `pgx` / `GORM`
- **Concurrency:** Goroutines & Channels for scheduling

### Data & Infrastructure

- **Primary Database:** PostgreSQL with PostGIS extension
- **Distributed Cache:** Redis
- **Containerization:** Docker & Docker Compose for isolated local development
- **Deployment:** GitHub Actions (CI/CD pipeline) / Azure App Service
