\## License

You are allowed to view the code but \*\*cannot redistribute\*\*, \*\*modify\*\*, or use it for \*\*commercial purposes\*\* without permission from the author.

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
- **Frontend**: Shared React + TypeScript + Vite + Tailwind CSS (SPA), designed to work with multiple backend stacks
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

The React frontend and PostgreSQL schema are shared, enabling rapid development and consistent user experience across all stacks. Backend switching is supported via environment variables.

### Design Principles

- **Clean Architecture**: Separation of concerns with distinct layers
- **SOLID Principles**: Maintainable and extensible code
- **DRY**: Don’t Repeat Yourself
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

## See Also

- [MERN/JS Backend (planned)](https://github.com/preethamkh/porfolio-tracker-mern)
- [Laravel/PHP Backend (planned)](https://github.com/preethamkh/porfolio-tracker-laravel)
- [Shared React Frontend (planned)]()

The .NET 8 stack is the reference implementation for business logic, API contracts, and database schema.


### Development Environment:

- **.NET 8 SDK**: [Download .NET](https://dotnet.microsoft.com/download/dotnet)
- **Node.js 20 LTS**: [Download Node.js](https://nodejs.org/)
- **PostgreSQL 16**: [PostgreSQL Downloads](https://www.postgresql.org/download/)
- **Redis**: [Download Redis](https://redis.io/download)
- **Docker Desktop**: [Docker Desktop](https://www.docker.com/products/docker-desktop)
- **Git**: [Git Downloads](https://git-scm.com/downloads)
