# Portfolio Management Application for Tracking Stocks, ETFs, and Investment Performance

## Project Overview

- **Track multiple investment portfolios**
- **Record buy/sell transactions**
- **Monitor real-time stock prices**
- **Visualize portfolio performance over time**
- **Analyze asset allocation and sector distribution**

## Architecture

### System Design

- **Modular Monolith** (evolvable to microservices)
- **RESTful API** with JWT authentication
- **Frontend**: Blazor WebAssembly SPA (this repo), designed to work with multiple backend stacks
- **Backend**: ASP.NET Core 8 Web API (this repo)
- **Database**: PostgreSQL (schema designed for reuse across stacks)
- **Caching**: Redis
- **Message Queue**: RabbitMQ (planned for later)
- **Containerization**: Docker

### Multi-Stack Vision

This project is part of a broader architecture supporting three backend stacks:

- **.NET 8 (this repo)**
- **MERN/JS (Node.js + NestJS + MongoDB/Prisma)**
- **PHP (Laravel 11 + PostgreSQL)**

The Blazor frontend and PostgreSQL schema are shared, enabling rapid development and consistent user experience across all stacks. Backend switching is supported via environment variables.

### Design Principles

- **Clean Architecture**: Separation of concerns with distinct layers
- **SOLID Principles**: Maintainable and extensible code
- **DRY**: Don't Repeat Yourself
- **KISS**: Keep It Simple, Stupid
- **Security First**: Authentication, authorization, input validation
- **Test-Driven**: Comprehensive test coverage

## Prerequisites

- **.NET 8 SDK**
- **Node.js 20 LTS**
- **PostgreSQL 16**
- **Redis**
- **Docker Desktop**
- **Git**
- **Node.js 20 LTS** (for Playwright E2E: `npm install && npx playwright install chromium`)

## See Also

- [MERN/JS Backend (planned)](https://github.com/preethamkh/porfolio-tracker-mern)
- [Laravel/PHP Backend (planned)](https://github.com/preethamkh/porfolio-tracker-laravel)
- [Blazor Frontend (this repo)](./BlazorFrontend/README.md)

The .NET 8 stack is the reference implementation for business logic, API contracts, and database schema.

## Development Environment

- **.NET 8 SDK**: [Download .NET](https://dotnet.microsoft.com/download/dotnet)
- **Node.js 20 LTS**: [Download Node.js](https://nodejs.org/)
- **PostgreSQL 16**: [PostgreSQL Downloads](https://www.postgresql.org/download/)
- **Redis**: [Download Redis](https://redis.io/download)
- **Docker Desktop**: [Docker Desktop](https://www.docker.com/products/docker-desktop)
- **Git**: [Git Downloads](https://git-scm.com/downloads)

## Blazor Frontend (WebAssembly)

A standalone Blazor WebAssembly app at `BlazorFrontend/` that consumes the API at `https://localhost:7001` (dev) or a configurable base URL from `wwwroot/appsettings.json`.

### Running the Blazor app locally

```bash
dotnet run --project BlazorFrontend
```

- The app will start at `https://localhost:5173/` (or your browser's default port)
- First run will prompt to trust the HTTPS certificate if using HTTPS backend
- API base URL can be overridden by setting the `ApiBaseUrl` config key in `BlazorFrontend/wwwroot/appsettings.json`

### Available pages

| Route | Description |
|-------|-------------|
| `/` | Dashboard — lists all user portfolios and aggregate holdings |
| `/login` | Login page (public) |
| `/register` | Register page (public) |
| `/securities` | Search securities from the market-data provider |
| `/portfolios` | List/create/edit/delete portfolios |
| `/portfolios/{id}` | Portfolio detail: holdings table, transaction history, add holding, record transaction |

### Authentication

- Register → auto-login → redirected to `/`
- `/`, `/portfolios`, `/securities` etc. require a valid JWT session (`/login`)
- Unauthenticated visitors are redirected to `/login?returnUrl=<original>` preserving the query string

### Rate limiting / free-tier notes

The backend consumes Alpha Vantage and Yahoo Finance for real-time quotes. On the free tier, those APIs throttle after a few dozen calls per minute. The application wraps both providers in a **caching decorator** (Redis) and retries with exponential back‑off (`Polly`). For production deployment behind a paid tier or self-hosted provider, simply configure `StockDataProvider:ActiveProvider` in the API's `appsettings.json`.

## E2E Tests (Playwright)

Browser-based end-to-end tests live in `tests/e2e/`.

### Running

```bash
cd tests/e2e
npm install          # first time only
npx playwright install chromium
npm test              # headless
npm run report        # open the HTML report
```

Tests register a unique user per run so repeated executions stay independent against a shared dev DB.

## API Reference

Key endpoints (all under `https://localhost:7001/api/`):

- `POST /auth/login` — `{email, password}` → `{token, user}`
- `POST /auth/register` — `{email, password, fullName?}` → `{token, user}`
- `GET /auth/me` — current user (protected)
- `GET /portfolios` — list user portfolios
- `POST /portfolios` — create portfolio
- `GET /securities/search?query=` — search stocks/ETFs
- `GET /portfolios/{portfolioId}/holdings` — holdings table
- `POST /portfolios/{portfolioId}/holdings` — add a holding (securityId + shares)
- `GET /portfolios/{portfolioId}/transactions` — transaction history
- `POST /portfolios/{portfolioId}/transactions` — record a buy/sell

All API routes are user-scoped: `/api/users/{userId}/...`. The Blazor frontend obtains `userId` from the JWT session automatically.

## Deployment

- **Docker**: `docker compose up --build` (docker-compose at repo root defines API + PostgreSQL + Redis)
- **Render / Fly.io**: deploy the API project; set environment vars `ApiBaseUrl`, `JwtSettings:Secret`, `Redis__ConnectionString`
- **Free-tier caveats**: see rate‑limiting note above; set `StockDataProvider:ActiveProvider` to `"yahoofinance"` to use the more generous Yahoo plan if Alpha Vantage quota is exhausted