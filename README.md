\## License

This project is \*\*proprietary\*\*. You are allowed to view the code but \*\*cannot redistribute\*\*, \*\*modify\*\*, or use it for \*\*commercial purposes\*\* without permission from the author.

# Portfolio Management Application for Tracking Stocks, ETFs, and Investment Performance

## Project Overview:

- **Track multiple investment portfolios**
- **Record buy transactions**
- **Sell transactions** (later)
- **Monitor real-time stock prices**
- **Calculate profile/loss and returns**
- **Visualize portfolio performance over time**
- **Analyze asset allocation and sector distribution**

## Architecture:

### System Design:

#### Pattern:

- **Modular Monolith** (evolvable to microservices)

#### API:

- RESTful with JWT authentication

#### Frontend:

- React + TypeScript (SPA)

#### Backend:

- ASP.NET Core 8 Web API

#### Database:

- PostgreSQL

#### Caching:

- Redis
- For Production, use managed Redis services
  Azure: Azure Cache for Redis
  AWS: Amazon ElastiCache (Redis)
  Railway: Railway Redis add-on
  Redis Cloud: redis.com (free tier available)

#### Message Queue:

- RabbitMQ

#### Containerization:

- Docker

### Design Principles:

- **Clean Architecture**: Separation of concerns with distinct layers
- **SOLID Principles**: Maintainable and extensible code
- **DRY**: Don’t Repeat Yourself
- **KISS**: Keep It Simple, Stupid
- **Security First**: Authentication, authorization, input validation
- **Test-Driven**: Comprehensive test coverage

## Prerequisites:

### Development Environment:

- **.NET 8 SDK**: [Download .NET](https://dotnet.microsoft.com/download/dotnet)
- **Node.js 20 LTS**: [Download Node.js](https://nodejs.org/)
- **PostgreSQL 16**: [PostgreSQL Downloads](https://www.postgresql.org/download/)
- **Redis**: [Download Redis](https://redis.io/download)
- **Docker Desktop**: [Docker Desktop](https://www.docker.com/products/docker-desktop)
- **Git**: [Git Downloads](https://git-scm.com/downloads)
