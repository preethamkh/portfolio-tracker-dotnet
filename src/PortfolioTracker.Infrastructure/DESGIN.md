# PortfolioTracker.Infrastructure Design Overview

## Purpose

The `PortfolioTracker.Infrastructure` project provides the data access and persistence layer for the Portfolio Tracker application. It is responsible for interacting with the database and implementing repository patterns, leveraging Entity Framework Core with PostgreSQL.

## Architecture Summary

- **Framework**: .NET 8
- **ORM**: Entity Framework Core (`Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`)
- **Database**: PostgreSQL
- **Dependencies**: References the `PortfolioTracker.Core` project for domain models and business logic.

## Key Responsibilities

- **DbContext Implementation**: Centralizes database configuration and entity mappings.
- **Repositories & Data Services**: Encapsulates CRUD operations and queries for domain entities.
- **Migrations & Design-Time Support**: Uses EF Core Design package for schema migrations and tooling.
- **Separation of Concerns**: Keeps infrastructure logic isolated from core business logic.

## Folder Structure

- **SERVICES**
- **External integrations and technical concerns**: Handles connectivity and integration logic.
- **Work with external APIs, databases, caching**: Manages communication with third-party services and infrastructure components.
- **Example**: `AlphaVantageService`, `FinnhubService` (future)

## Integration

- Exposes infrastructure services to the application layer via dependency injection.
- Ensures all data operations conform to domain models defined in the core project.

---

## Notes
External API (Alpha Vantage)
    returns raw JSON
StockQuoteDto (ExternalData DTO  = these shape API responses. Only Exteranl DTOs change if we change API providers) <- Normalizes API response
    consumed by
SecurityService
    maps to
Security Entity (your domain model)
    exposed as
SecurityDto (your domain DTO = shape our business logic)
    returned to
Controller -> Client
