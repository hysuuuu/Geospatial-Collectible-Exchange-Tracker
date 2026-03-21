# Geospatial Collectible Exchange Tracker

## Project Overview

The Geospatial Collectible Exchange Tracker is a full-stack web application designed to address inventory management and asymmetric exchange tracking challenges from location-based gaming. The system provides a centralized platform for users to track item ownership, visualize their collection footprint through geospatial data, and manage complex exchange transactions.

## Core Features

- **Bidirectional Exchange State Machine:** A robust transaction engine that manages the lifecycle of item exchanges (Pending, Sent, Completed), ensuring data integrity and preventing double-spending of virtual assets.
- **Geospatial Visualization:** Integration of mapping services to project collectible coordinates onto an interactive interface, providing users with a visual representation of their global collection progress.
- **Inventory Management & Tagging:** A high-performance filtering system that allows users to categorize items by location, rarity, or custom tags, facilitating efficient identification of duplicates for trade.

## System Architecture

The application is built on a decoupled, N-tier architecture to ensure separation of concerns and ease of maintenance.

### Architectural Components

1.  **N-Tier Backend:** The core API is structured into Controllers, Services, and Data Access layers. This ensures that business logic is isolated from HTTP handling and persistence logic.
2.  **Distributed Caching (Redis):** Implements the Cache-Aside pattern to store frequently accessed data such as user dashboards and global map coordinates, reducing latency and database overhead.

## Technology Stack

### Frontend

- **Framework:** React
- **State Management:** Context API / Redux Toolkit
- **Mapping UI:**
- **Hosting:** Vercel

### Backend

- **Framework:** C# .NET 8 Web API
- **Object-Relational Mapper (ORM):** Entity Framework Core
- **Security:** JSON Web Tokens (JWT) for stateless authentication
- **Containerization:** Docker for infrastructure services
- **Hosting:** Azure App Service

### Data & Middleware

- **Primary Database:** SQL Server / PostgreSQL
- **Distributed Cache:** Redis
