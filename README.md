# AcmHouse API — Real Estate Agency Backend

[![CI](https://github.com/maxmetelitsa/AcmHouse-Api/actions/workflows/ci.yml/badge.svg)](https://github.com/maxmetelitsa/AcmHouse-Api/actions/workflows/ci.yml)

REST API for the AcmHouse real estate agency: a property catalog with filters, clients, rental bookings with date-conflict checks, JWT sign-in with roles and revenue reports. It is the backend counterpart of the [AcmHouse desktop app](https://github.com/maxmetelitsa/AcmHouse-RealEstate).

ASP.NET Core 8 Web API · Entity Framework Core 8 · MS SQL Server · JWT · Swagger · xUnit · Docker · GitHub Actions

> 🇷🇺 Русское описание — [ниже](#русская-версия).

## Features

- **Property catalog** — public; filters by rental type, property type, price range, rooms and text; paging; sorted by price.
- **Availability check** — is a property free for given dates, and what would the stay cost.
- **Clients** — create and search tenants; passport number is validated and unique.
- **Bookings** — the server calculates the price and the agent's profit, rejects overlapping dates with `409 Conflict`, allows check-in on the previous guest's check-out day; bookings can be cancelled before they start.
- **Roles** — agents sign contracts and can cancel only their own; administrators manage properties and see reports.
- **Revenue report** — totals and per-agent revenue and profit for a period.
- **Errors** — every error is an RFC 7807 problem details response with the right status code (400, 401, 403, 404, 409).

## Architecture

Clean Architecture: dependencies point inwards, the business rules know nothing about the web or the database.

```
AcmHouse.Api             Controllers, JWT, Swagger, error handling      ─┐
AcmHouse.Infrastructure  EF Core DbContext, repositories, PBKDF2 hashing  ├─▶ AcmHouse.Application ─▶ AcmHouse.Domain
                                                                         ─┘   services, DTOs,            entities,
                                                                              repository interfaces      rental rules
```

| Project | What is there |
| --- | --- |
| `AcmHouse.Domain` | Entities (`Property`, `Client`, `Employee`, `Booking`) and `RentalRules`: period limits, price, agent profit, date overlap |
| `AcmHouse.Application` | Use cases (`BookingService`, `PropertyService`, …), DTOs with validation attributes, repository interfaces, exceptions |
| `AcmHouse.Infrastructure` | `AppDbContext` with Fluent API configuration, repositories, demo data seeder, password hasher |
| `AcmHouse.Api` | Controllers, JWT bearer authentication, Swagger with an "Authorize" button, exception → problem details mapping |

## API

| Method | Endpoint | Access | Description |
| --- | --- | --- | --- |
| `POST` | `/api/auth/login` | anyone | Sign in, get a JWT |
| `GET` | `/api/properties` | anyone | Catalog: `rentalType`, `type`, `search`, `minPrice`, `maxPrice`, `minRooms`, `page`, `pageSize` |
| `GET` | `/api/properties/{id}` | anyone | One property |
| `GET` | `/api/properties/{id}/availability?from=&to=` | anyone | Free or not, and the price |
| `POST` `PUT` `DELETE` | `/api/properties[/{id}]` | Admin | Manage properties (a rented property cannot be deleted) |
| `GET` `POST` | `/api/clients` | Agent, Admin | Search and create clients |
| `GET` | `/api/clients/{id}` | Agent, Admin | One client |
| `POST` | `/api/bookings` | Agent, Admin | Sign a rental contract |
| `GET` | `/api/bookings?propertyId=&from=&to=` | Agent, Admin | List bookings |
| `GET` | `/api/bookings/{id}` | Agent, Admin | One booking |
| `POST` | `/api/bookings/{id}/cancel` | owner Agent, Admin | Cancel a booking that has not started |
| `GET` | `/api/reports/revenue?from=&to=` | Admin | Revenue and profit per agent |
| `GET` | `/health` | anyone | Health check |

## Getting started

### Visual Studio + LocalDB (Windows)

1. Open `AcmHouse.Api.sln` in Visual Studio 2022 and set **AcmHouse.Api** as the startup project.
2. Press **F5**. On the first run the database `AcmHouse` is created in LocalDB and filled with demo data.
3. Swagger opens at `https://localhost:7180/swagger`.

### Docker (any OS)

```bash
docker compose up --build
```

Then open http://localhost:8080/swagger. SQL Server runs in a container next to the API.

### Demo accounts

All data is fictional.

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@acmhouse.by` | `Admin1234` |
| Agent | `ivan.petrov@acmhouse.by` | `Demo1234` |

In Swagger: call `POST /api/auth/login`, copy `accessToken`, press **Authorize** and paste it.

### Configuration

| Setting | Where | Notes |
| --- | --- | --- |
| `ConnectionStrings:Default` | `appsettings.json` | LocalDB by default |
| `Jwt:Key` | `appsettings.Development.json`, user secrets or `Jwt__Key` | At least 32 characters; the dev key must not be used in production |
| `Database:Initialize` | `appsettings.Development.json` | Creates the database and demo data on startup |

## Tests

```bash
dotnet test
```

- **AcmHouse.UnitTests** — 52 tests: rental rules (including the check-out-day edge cases), password hashing and every service with in-memory repositories.
- **AcmHouse.IntegrationTests** — the whole API through `WebApplicationFactory`: real routing, validation, JWT and EF Core (in-memory provider). Covers 401/403/404/409 responses and the full booking flow.

GitHub Actions builds the solution and runs all tests on every push.

## Known limitations and next steps

- The database is created with `EnsureCreated`. The next step is EF Core migrations: `dotnet ef migrations add InitialCreate -p src/AcmHouse.Infrastructure -s src/AcmHouse.Api`.
- Two agents booking the same dates at the same moment could both pass the check; a unique constraint or a serializable transaction would close that gap.
- There are no refresh tokens; an access token lives 60 minutes.

## How it was built

I designed the domain from my AcmHouse desktop app (2024) and built this API in 2026 with the help of [Claude](https://claude.ai), an AI assistant, to learn ASP.NET Core, EF Core and Clean Architecture on a subject I know well.

---

## Русская версия

**AcmHouse API** — REST API агентства недвижимости, серверная часть к десктопному приложению [AcmHouse](https://github.com/maxmetelitsa/AcmHouse-RealEstate).

**Что умеет:** каталог объектов с фильтрами и постраничным выводом; проверка, свободен ли объект на даты, и расчёт стоимости; клиенты с проверкой номера паспорта; бронирование с расчётом цены и прибыли агента на сервере и отказом `409 Conflict` при пересечении дат; отмена брони до начала аренды; вход по JWT с ролями «агент» и «администратор»; отчёт по выручке; ошибки в формате Problem Details (RFC 7807).

**Технологии:** C#, ASP.NET Core 8 Web API, Entity Framework Core 8, MS SQL Server, JWT, Swagger, xUnit, Docker, GitHub Actions. Архитектура — Clean Architecture: Domain, Application, Infrastructure, Api.

**Тесты:** 52 модульных теста и интеграционные тесты всего API через `WebApplicationFactory`. GitHub Actions собирает проект и запускает тесты при каждом изменении.

**Запуск:** откройте `AcmHouse.Api.sln` в Visual Studio 2022 и нажмите F5 — база создастся в LocalDB, откроется Swagger. Или `docker compose up --build` и http://localhost:8080/swagger. Вход администратора: `admin@acmhouse.by` / `Admin1234`, агента: `ivan.petrov@acmhouse.by` / `Demo1234`.

**Как сделан:** предметную область я взял из своего десктопного AcmHouse (2024), а API построил в 2026 году с помощью ИИ-ассистента [Claude](https://claude.ai), чтобы освоить ASP.NET Core, EF Core и Clean Architecture на знакомой задаче.
