# TicketTitan

TicketTitan is a .NET 10 web application that has been pivoted from a ticketing system into a faceless AI content generation platform. It supports authenticated story creation workflows, generation jobs, Gemini-powered scripting and QA, and a React frontend for story setup and progress tracking.

This repo contains the backend API. The frontend repo is separate and is expected to call the API at the configured Azure or local host URL.

## Overview

TicketTitan now supports:
- authenticated users with Identity + Google OAuth + JWT cookie auth
- a story request workflow
- category management
- AI-assisted story creation and generation jobs
- Gemini-based scripting and review
- background worker processing using `System.Threading.Channels`
- SignalR progress updates
- Dockerized local development
- Azure Container Apps deployment

## Tech stack

- ASP.NET Core 10
- EF Core + Npgsql
- ASP.NET Core Identity
- JWT in cookie authentication
- FluentValidation
- SignalR
- React + Vite frontend
- Google Gemini API
- Vertex AI for video generation
- Supabase Storage or Cloudflare R2 for media assets
- Azure Key Vault integration
- Docker + Docker Compose

## Current backend focus

The backend is evolving from ticketing toward the following model:

- `Categories`
- `Characters`
- `StoryRequests`
- `Episodes`
- `Scenes`
- `Clips`
- `GenerationJobs`

The system is designed around queued background work so long-running generation jobs can continue even if the app restarts.

## Repository structure

- `Program.cs` – app registration, auth, CORS, services, Swagger, SignalR setup
- `Controllers/` – API controllers
- `Services/` – business logic for Gemini, auth, generation jobs, etc.
- `Data/` – EF Core `ApplicationDbContext`
- `Models/` – domain entities
- `DTO/` – request/response objects
- `Interfaces/` – service contracts
- `Filters/` – auth/anti-forgery filters
- `Hubs/` – SignalR hubs
- `Validators/` – FluentValidation validators
- `Dockerfile` – container image for the API
- `docker-compose.yml` – local container dev stack
- `.env` – environment variables for local development

## Local development

### Prerequisites

- .NET 10 SDK
- Docker Desktop / Docker Engine
- Node.js 20+ for the frontend
- access to a PostgreSQL database (Supabase/Postgres or local PG)
- Gemini API key
- optional Azure Key Vault configuration

### Run API locally

The app will typically be available at:

- `http://localhost:8080`
- `https://localhost:7049` depending on your launch configuration

### Run with Docker Compose


This uses the included `Dockerfile` and `docker-compose.yml`.

## Gemini chat endpoint

A Gemini chat endpoint has been added for AI-assisted story ideation and script prompting.

Example route:
- `POST /api/geminichat/chat`
- route is case-insensitive in ASP.NET, so `/api/GeminiChat/chat` also works

Example request body:
{ "messages": [ { "role": "user", "content": "Give me an idea for an AI drama topic" } ], "model": "gemini-2.5-flash" }

Example response:

If your route config is customized, the exact path may be:
- `/api/chat`

Use the route that matches your current `GeminiChatController` configuration.

## Auth and security

The backend keeps the existing auth infrastructure:
- ASP.NET Identity
- Google OAuth
- JWT cookie auth
- antiforgery validation
- CORS policy for the frontend
- Key Vault configuration loading in `Program.cs`

The anti-forgery filter is active for non-GET requests. When calling endpoints from the frontend, make sure the app is sending the required CSRF token and auth cookie/session state.

## Worker / queue design

The backend uses:
- `System.Threading.Channels`
- a background worker service
- persisted generation job state in Postgres

This supports:
- script generation
- scene specs generation
- audio generation
- clip generation
- QA scoring
- re-roll logic
- final stitching / bundle generation

## Deployment

The project is designed for Azure deployment, including:
- Azure Container Apps
- Azure Key Vault secret loading
- environment variables injected at runtime
- CORS configured for frontend domains

## Important notes

- Gemini and Vertex requests should never be exposed directly in the frontend.
- Google service account credentials should remain in environment variables or Key Vault.
- The production deployment must be aligned with the current route configuration and latest container revision.
- The app is being migrated from a ticketing domain to a generation domain; older ticket-specific code and controllers should be removed or replaced as the migration continues.

## Useful commands

Build: dotnet build
Run: dotnet run
Docker: docker compose up --build

## License

This project is intended for internal product work and experimental AI generation workflows. Update license details as needed for your deployment and distribution requirements.

## Roadmap

- remove legacy ticketing domain
- finalize EF Core schema for generation entities
- add queue worker and persisted job tracking
- integrate Gemini script generation and scene breakdowns
- integrate Vertex Veo clip generation
- implement TTS and clip QA evaluation
- implement stitch/bundle export for editing workflows
- wire live generation updates via SignalR
- finalize frontend UX and story creation flow