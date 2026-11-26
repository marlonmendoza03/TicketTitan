# Graph Report - TicketTitan  (2026-11-26)

## Corpus Check
- 74 files · ~21,430 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 7 file(s) not represented in the graph (top: (none) 4, .http 1, .xml 1)

## Summary
- 594 nodes · 1068 edges · 31 communities (26 shown, 5 thin omitted)
- Extraction: 94% EXTRACTED · 6% INFERRED · 0% AMBIGUOUS · INFERRED: 65 edges (avg confidence: 0.87)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `a60cf072`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- ServiceResult Pattern & Core Types
- Controllers & Application Entry Points
- TicketService
- EF Core Migrations & Database Schema
- E2E Tests (Playwright)
- GitHub Actions CI/CD Pipeline
- Docker Compose Configuration
- Launch Settings & Debug Config
- NuGet Package Dependencies
- AuthController
- UI Types (Story Chat & Components)
- Gemini Chat DTOs & Story Setup
- Story Attachment DTOs & Models
- Ticket Models & Domain
- Story Chat Attachment Models
- Story Chat Session Models
- Story Chat Message Models & DbContext
- Service Dependencies (Secrets)
- Service Dependencies (TicketTitan)
- Service Dependencies (Local)
- ApplicationDbContext & DbSets
- GeminiChatResponseDto
- Session Status Enum
- CLAUDE.md
- TicketTitan
- UI PageHeading Component
- TicketTitan End-to-End Tests
- system_componentmodel_dataannotations
- Copilot Instructions
- .claude/CLAUDE.md

## God Nodes (most connected - your core abstractions)
1. `ServiceResult` - 36 edges
2. `StoryChatSessionDto` - 33 edges
3. `TicketTitan.DTO` - 26 edges
4. `StoryChatAttachment` - 21 edges
5. `StoryChatSession` - 20 edges
6. `Ticket` - 19 edges
7. `ApplicationDbContext` - 18 edges
8. `TicketTitan.Models` - 17 edges
9. `StoryChatMessage` - 17 edges
10. `TicketTitan` - 17 edges

## Surprising Connections (you probably didn't know these)
- `Repository structure` --references--> `ApplicationDbContext`  [INFERRED]
  README.md → Data/ApplicationDbContext.cs
- `Important Conventions` --references--> `StoryChatController`  [INFERRED]
  CLAUDE.md → Controllers/StoryChatController.cs
- `Key Components` --references--> `RoleType`  [INFERRED]
  CLAUDE.md → Models/StoryChatMessage.cs
- `Key Components` --references--> `StoryChatMessage`  [INFERRED]
  CLAUDE.md → Models/StoryChatMessage.cs
- `Key Components` --references--> `StoryChatSession`  [INFERRED]
  CLAUDE.md → Models/StoryChatSession.cs

## Import Cycles
- None detected.

## Communities (31 total, 5 thin omitted)

### Community 0 - "ServiceResult Pattern & Core Types"
Cohesion: 0.07
Nodes (22): ServiceResult, ErrorMessage, IsSuccess, Value, StoryChatController, StoryChatMessageDto, Content, Role (+14 more)

### Community 1 - "Controllers & Application Entry Points"
Cohesion: 0.05
Nodes (18): Constants, TicketTitan.Controllers, TicketTitan.Services, TicketTitan.Data, TicketTitan.Models, TicketTitan.Filters, TicketTitan.Validators, TicketTitan.DTO (+10 more)

### Community 2 - "TicketService"
Cohesion: 0.14
Nodes (6): GoogleLoginResultDto, TicketNotificationDto, TicketResponseDto, TicketHub, ITicketService, TicketService

### Community 3 - "EF Core Migrations & Database Schema"
Cohesion: 0.13
Nodes (6): TicketTitan.Migrations, InitialSupabaseCreate, AddStoryChatPersistence, FixStoryChatRoleType, AddStoryChatAttachments, ApplicationDbContextModelSnapshot

### Community 4 - "E2E Tests (Playwright)"
Cohesion: 0.06
Nodes (24): authFile, description, devDependencies, @playwright/test@^1.45.0, @types/node@^20.14.0, name, private, scripts (+16 more)

### Community 5 - "GitHub Actions CI/CD Pipeline"
Cohesion: 0.06
Nodes (34): environment, permissions, runs-on, steps, env, CONTAINER_APP_ENV, CONTAINER_APP_NAME, IMAGE_NAME (+26 more)

### Community 6 - "Docker Compose Configuration"
Cohesion: 0.09
Nodes (22): context, dockerfile, ASPNETCORE_ENVIRONMENT, ASPNETCORE_URLS, Authentication__Google__ClientId, ConnectionStrings__DefaultConnection, Gemini__ApiKey, JwtSettings__Audience (+14 more)

### Community 7 - "Launch Settings & Debug Config"
Cohesion: 0.10
Nodes (21): commandName, environmentVariables, launchUrl, publishAllPorts, useSSL, ASPNETCORE_ENVIRONMENT, ASPNETCORE_HTTP_PORTS, ASPNETCORE_HTTPS_PORTS (+13 more)

### Community 8 - "NuGet Package Dependencies"
Cohesion: 0.11
Nodes (18): net10.0, Azure.Extensions.AspNetCore.Configuration.Secrets (1.5.2), Azure.Identity (1.21.0), FluentValidation (12.2.0), FluentValidation.AspNetCore (11.3.1), FluentValidation.DependencyInjectionExtensions (12.2.0), Google.Apis.Auth (1.78.0), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.14) (+10 more)

### Community 9 - "AuthController"
Cohesion: 0.12
Nodes (4): AuthController, CookieCsrfAuthorizationFilter, IAuthService, AuthService

### Community 10 - "UI Types (Story Chat & Components)"
Cohesion: 0.14
Nodes (13): CategoryDto, ChatRole, SessionStatus, Approved, AwaitingReferences, Draft, StoryAttachmentDto, StoryChatMessageDto (+5 more)

### Community 11 - "Gemini Chat DTOs & Story Setup"
Cohesion: 0.14
Nodes (11): GeminiChatMessageDto, Content, Role, GeminiChatRequestDto, Messages, Model, StorySetup, StorySetup (+3 more)

### Community 12 - "Story Attachment DTOs & Models"
Cohesion: 0.17
Nodes (11): StoryAttachmentDto, ContentType, DownloadUrl, FileName, FileSize, Id, Type, UploadedAt (+3 more)

### Community 13 - "Ticket Models & Domain"
Cohesion: 0.14
Nodes (14): Key Components, ApplicationUser, Ticket, Category, CreatedAt, CreatedByUser, CreatedByUserId, Description (+6 more)

### Community 14 - "Story Chat Attachment Models"
Cohesion: 0.17
Nodes (12): StoryChatAttachment, ContentType, DownloadUrl, FileName, FileSize, Id, Session, SessionId (+4 more)

### Community 15 - "Story Chat Session Models"
Cohesion: 0.20
Nodes (9): StoryChatSession, Attachments, CreatedAt, Id, Messages, Status, Title, UpdatedAt (+1 more)

### Community 16 - "Story Chat Message Models & DbContext"
Cohesion: 0.25
Nodes (7): StoryChatMessage, Content, CreatedAt, Id, Role, Session, SessionId

### Community 17 - "Service Dependencies (Secrets)"
Cohesion: 0.22
Nodes (8): dependencies, secrets1, sqlite1, connectionId, dynamicId, type, connectionId, type

### Community 18 - "Service Dependencies (TicketTitan)"
Cohesion: 0.22
Nodes (8): dependencies, secrets1, connectionId, dynamicId, resourceId, secretStore, serviceConnectorResourceId, type

### Community 19 - "Service Dependencies (Local)"
Cohesion: 0.25
Nodes (7): dependencies, secrets1, sqlite1, type, connectionId, secretStore, type

### Community 20 - "ApplicationDbContext & DbSets"
Cohesion: 0.25
Nodes (5): ApplicationDbContext, StoryChatAttachments, StoryChatMessages, StoryChatSessions, Tickets

### Community 21 - "GeminiChatResponseDto"
Cohesion: 0.13
Nodes (8): GeminiChatResponseDto, content, createdAt, role, SessionId, IGeminiChatService, AttachmentService, GeminiChatService

### Community 22 - "Session Status Enum"
Cohesion: 0.40
Nodes (4): SessionStatus, Approved, AwaitingReferences, Draft

### Community 23 - "CLAUDE.md"
Cohesion: 0.09
Nodes (19): Architecture, Authentication, Build & Run Commands, CORS, Deployment, External Integrations, graphify, Important Conventions (+11 more)

### Community 24 - "TicketTitan"
Cohesion: 0.12
Nodes (16): Auth and security, Current backend focus, Deployment, Important notes, License, Local development, Overview, Prerequisites (+8 more)

### Community 27 - "TicketTitan End-to-End Tests"
Cohesion: 0.15
Nodes (12): Authentication Methods, CI/CD Integration, Extending Tests, Method 1: Google OAuth (Full Flow), Method 2: Pre-authenticated Cookies (Fast), Quick Start, Running Specific Tests, Test Assets (+4 more)

## Knowledge Gaps
- **99 isolated node(s):** `IsSuccess`, `Value`, `ErrorMessage`, `Draft`, `AwaitingReferences` (+94 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 295 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **5 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ApplicationDbContext` connect `ApplicationDbContext & DbSets` to `ServiceResult Pattern & Core Types`, `Controllers & Application Entry Points`, `TicketService`, `Ticket Models & Domain`, `Story Chat Attachment Models`, `Story Chat Session Models`, `Story Chat Message Models & DbContext`, `GeminiChatResponseDto`, `TicketTitan`?**
  _High betweenness centrality (0.057) - this node is a cross-community bridge._
- **What connects `IsSuccess`, `Value`, `ErrorMessage` to the rest of the system?**
  _99 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `ServiceResult Pattern & Core Types` be split into smaller, more focused modules?**
  _Cohesion score 0.06554621848739496 - nodes in this community are weakly interconnected._
- **Why does `TicketTitan.DTO` connect `Controllers & Application Entry Points` to `Gemini Chat DTOs & Story Setup`, `system_componentmodel_dataannotations`, `GeminiChatResponseDto`?**
  _High betweenness centrality (0.054) - this node is a cross-community bridge._
- **Should `Controllers & Application Entry Points` be split into smaller, more focused modules?**
  _Cohesion score 0.05365296803652968 - nodes in this community are weakly interconnected._
- **Why does `StoryChatSessionDto` connect `ServiceResult Pattern & Core Types` to `Controllers & Application Entry Points`, `EF Core Migrations & Database Schema`, `Story Attachment DTOs & Models`, `Session Status Enum`?**
  _High betweenness centrality (0.037) - this node is a cross-community bridge._
- **Should `TicketService` be split into smaller, more focused modules?**
  _Cohesion score 0.14285714285714285 - nodes in this community are weakly interconnected._