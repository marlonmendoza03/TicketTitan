# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TicketTitan is an ASP.NET Core 10 Web API backend that has evolved from a ticketing system into an AI-powered content generation platform. It supports Google OAuth + JWT cookie authentication, Gemini-powered story/chat workflows, and SignalR real-time updates. The frontend is a separate React+Vite repo.

## Build & Run Commands

```bash
dotnet build                          # Build the project
dotnet run                            # Run locally (default: http://localhost:8080 or https://localhost:7049)
dotnet ef migrations add <Name>       # Add a new EF Core migration
dotnet ef database update             # Apply pending migrations
docker compose up --build             # Full stack via Docker
```

Tests are not currently present in this project. Use `dotnet test` once they are added.

## Architecture

### Request Flow

Controllers → Services → DbContext (EF Core + Npgsql/PostgreSQL). All services return `ServiceResult<T>` — a readonly record struct with `IsSuccess`, `Value`, and `ErrorMessage` — providing consistent error handling across the codebase.

### Key Components

| Layer | Description |
|-------|-------------|
| `Controllers/` | API endpoints; all controllers use `[ApiController]` and route templates like `api/[controller]` |
| `Services/` | Business logic. Each service implements an interface from `Interfaces/` |
| `Data/ApplicationDbContext.cs` | EF Core context. Entities: `Ticket`, `StoryChatSession`, `StoryChatMessage`. Users managed via ASP.NET Identity (`ApplicationUser`) |
| `Models/` | Domain entities and enums (`RoleType` for chat messages) |
| `DTO/` | Request/response shapes; POCOs used at API boundaries only |
| `Filters/CookieCsrfAuthorizationFilter.cs` | Global async authorization filter applied to all controller actions; skips GET/HEAD/OPTIONS/TRACE, `/api/auth/google-login`, and any request carrying an `Authorization` header |
| `Hubs/TicketHub.cs` | Single SignalR hub broadcasting to the `"HighPriorityAlerts"` group |
| `Validators/` | FluentValidation validators registered globally via `AddValidatorsFromAssemblyContaining<>` |

### Authentication

- Google OAuth via `Google.Apis.Auth`; users are created on first login and assigned the `User` role
- A short-lived application JWT is issued and stored as an `HttpOnly` cookie (`TicketTitan.Auth`)
- The JWT bearer middleware also reads the same cookie as a fallback in `OnMessageReceived`
- CSRF protection uses antiforgery tokens exchanged via `/api/Auth/csrf-token`; the token must be sent in the `X-CSRF-TOKEN` header on mutations

### External Integrations

- **Gemini**: Calls hit `https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={ApiKey}`. Default model is `gemini-3.5-flash-lite`; a dedicated 3-minute timeout policy exists in `Program.cs` but is not wired to any controller yet.
- **Azure Key Vault**: If `VaultUri` env var is set, configuration is merged from Azure Key Vault at startup via `DefaultAzureCredential`.
- **Data Protection keys** are persisted to disk under `dataprotection-keys/` (mounted in Docker).

### CORS

Policy `UiClient` allows credentials from `http://localhost:5173` and `https://ticket-titan-ruby.vercel.app`. Update both origins if the frontend URL changes.

## Important Conventions

- Always read a file before editing it; the Copilot instructions note that stale cached versions may be referenced.
- Service methods always return `ServiceResult<T>`; controllers should check `IsSuccess` and map failures to appropriate HTTP status codes (400, 404, 403 as shown in `TicketsController`).
- Chat message roles are stored as integers (`RoleType` enum); `ToGeminiRole()` converts them for the Gemini API (`user`/`model`).
- The `GeminiChatController` is decommissioned — its endpoints are commented out. New Gemini-backed features should go through `StoryChatController` / `StoryChatService`.

## Deployment

The app runs inside a Linux container on port **8080**. Required environment variables:

| Variable | Purpose |
|----------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `JwtSettings__Secret` | JWT signing key |
| `JwtSettings__Issuer` / `Audience` / `ExpiryInMinutes` | JWT settings |
| `Authentication__Google__ClientId` / `ClientSecret` | Google OAuth |
| `Gemini__ApiKey` | Google Gemini API key |
| `VaultUri` | Optional: Azure Key Vault endpoint for secret loading |

Migrations live in `Migrations/`. Run `dotnet ef database update` after adding new ones.

## UI Project
- You can access the UI project on the same directory under `ticket-titan-ui` folder name.
- The UI uses React and TypeScript and you can use it as a reference and/or work on it for a full-stack feature.
- Always be mindful of the best practices when working on the UI repository

## Obsidian Second Brain (Mandatory Workflow)
- **Vault Path:** `C:\Users\ntmar\OneDrive\Documents\TicketTitan-Brain`
- **Before reading codebase files:** Always read `00-Architecture/` (`api-endpoints.md`, `database-schema.md`, `deployment-setup.md`, `signalr-hubs.md`) and the latest file in `02-Sessions/` first to get context without scanning the whole repo.
- **After architectural or schema changes:** Update the matching doc in `00-Architecture/` and append a short decision note in `01-Decisions/ADR-XXX-<topic>.md`.
- **On wrap-up / before `/clear`:** Write a concise summary (changes made, files touched, next steps) to `02-Sessions/YYYY-MM-DD.md`.

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

Rules:
- When graphify-out/graph.json exists, run `graphify query "<question>"` before your first grep, search or source-file read for a codebase question, and put this rule in every subagent prompt that explores code. Read raw files only for lines the graph or an earlier read already located. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. When the answer is under a Markdown heading, `graphify section "<heading>"` prints that section's text, so the .md need not be opened. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).
- A committed graphify-out/graph.json is rewritten whole on every build, so a large diff to it in a pull request is expected. Do not flag it as churn or suggest skipping the graph rebuild or its hooks.
