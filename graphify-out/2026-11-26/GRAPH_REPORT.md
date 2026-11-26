# Graph Report - TicketTitan  (2026-11-26)

## Corpus Check
- Corpus is ~13,172 words - fits in a single context window. You may not need a graph.

## Summary
- 539 nodes · 1003 edges · 27 communities (23 shown, 4 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 52 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- ServiceResult Pattern & Core Types
- Controllers & Application Entry Points
- AuthController & Authentication
- EF Core Migrations & Database Schema
- E2E Tests (Playwright)
- GitHub Actions CI/CD Pipeline
- Docker Compose Configuration
- Launch Settings & Debug Config
- NuGet Package Dependencies
- Auth Service & User Identity
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
- CSRF Authorization Filter
- Session Status Enum
- Role Type Enum & Extensions
- Upload Attachment Request
- UI PageHeading Component

## God Nodes (most connected - your core abstractions)
1. `ServiceResult` - 36 edges
2. `StoryChatSessionDto` - 33 edges
3. `TicketTitan.DTO` - 26 edges
4. `StoryChatAttachment` - 21 edges
5. `StoryChatSession` - 19 edges
6. `Ticket` - 18 edges
7. `ApplicationDbContext` - 17 edges
8. `TicketTitan.Models` - 17 edges
9. `TicketTitan` - 17 edges
10. `StoryAttachmentDto` - 16 edges

## Surprising Connections (you probably didn't know these)
- `Tickets` --references--> `Ticket`  [EXTRACTED]
  Data/ApplicationDbContext.cs → Models/Ticket.cs
- `StoryChatSessions` --references--> `StoryChatSession`  [EXTRACTED]
  Data/ApplicationDbContext.cs → Models/StoryChatSession.cs
- `StoryChatAttachments` --references--> `StoryChatAttachment`  [EXTRACTED]
  Data/ApplicationDbContext.cs → Models/StoryChatAttachment.cs
- `AuthController` --references--> `IAuthService`  [EXTRACTED]
  Controllers/AuthController.cs → Interfaces/IAuthService.cs
- `StoryChatMessageDto` --references--> `RoleType`  [EXTRACTED]
  DTO/StoryChatMessageDto.cs → Models/StoryChatMessage.cs

## Import Cycles
- None detected.

## Communities (27 total, 4 thin omitted)

### Community 0 - "ServiceResult Pattern & Core Types"
Cohesion: 0.06
Nodes (29): ServiceResult, ErrorMessage, IsSuccess, Value, StoryChatController, GeminiChatResponseDto, content, createdAt (+21 more)

### Community 1 - "Controllers & Application Entry Points"
Cohesion: 0.05
Nodes (18): Constants, TicketTitan.Controllers, TicketTitan.Services, TicketTitan.Data, TicketTitan.Models, TicketTitan.Filters, TicketTitan.Validators, TicketTitan.DTO (+10 more)

### Community 2 - "AuthController & Authentication"
Cohesion: 0.08
Nodes (9): AuthController, GeminiChatController, TicketsController, CreateTicketDto, TicketNotificationDto, TicketResponseDto, TicketHub, ITicketService (+1 more)

### Community 3 - "EF Core Migrations & Database Schema"
Cohesion: 0.13
Nodes (6): TicketTitan.Migrations, InitialSupabaseCreate, AddStoryChatPersistence, FixStoryChatRoleType, AddStoryChatAttachments, ApplicationDbContextModelSnapshot

### Community 4 - "E2E Tests (Playwright)"
Cohesion: 0.07
Nodes (23): description, devDependencies, @playwright/test@^1.45.0, @types/node@^20.14.0, name, private, scripts, install:all (+15 more)

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

### Community 9 - "Auth Service & User Identity"
Cohesion: 0.17
Nodes (4): ApplicationUser, GoogleLoginResultDto, IAuthService, AuthService

### Community 10 - "UI Types (Story Chat & Components)"
Cohesion: 0.14
Nodes (13): CategoryDto, ChatRole, SessionStatus, Approved, AwaitingReferences, Draft, StoryAttachmentDto, StoryChatMessageDto (+5 more)

### Community 11 - "Gemini Chat DTOs & Story Setup"
Cohesion: 0.14
Nodes (11): GeminiChatMessageDto, Content, Role, GeminiChatRequestDto, Messages, Model, StorySetup, StorySetup (+3 more)

### Community 12 - "Story Attachment DTOs & Models"
Cohesion: 0.15
Nodes (11): StoryAttachmentDto, ContentType, DownloadUrl, FileName, FileSize, Id, Type, UploadedAt (+3 more)

### Community 13 - "Ticket Models & Domain"
Cohesion: 0.15
Nodes (12): Ticket, Category, CreatedAt, CreatedByUser, CreatedByUserId, Description, Id, Priority (+4 more)

### Community 14 - "Story Chat Attachment Models"
Cohesion: 0.17
Nodes (12): StoryChatAttachment, ContentType, DownloadUrl, FileName, FileSize, Id, Session, SessionId (+4 more)

### Community 15 - "Story Chat Session Models"
Cohesion: 0.20
Nodes (9): StoryChatSession, Attachments, CreatedAt, Id, Messages, Status, Title, UpdatedAt (+1 more)

### Community 16 - "Story Chat Message Models & DbContext"
Cohesion: 0.22
Nodes (8): StoryChatMessages, StoryChatMessage, Content, CreatedAt, Id, Role, Session, SessionId

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
Cohesion: 0.29
Nodes (4): ApplicationDbContext, StoryChatAttachments, StoryChatSessions, Tickets

### Community 22 - "Session Status Enum"
Cohesion: 0.40
Nodes (4): SessionStatus, Approved, AwaitingReferences, Draft

### Community 23 - "Role Type Enum & Extensions"
Cohesion: 0.40
Nodes (4): RoleType, Assistant, User, RoleTypeExtensions

## Knowledge Gaps
- **99 isolated node(s):** `IsSuccess`, `Value`, `ErrorMessage`, `Draft`, `AwaitingReferences` (+94 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 253 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **4 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TicketTitan.DTO` connect `Controllers & Application Entry Points` to `ServiceResult Pattern & Core Types`, `Upload Attachment Request`, `Gemini Chat DTOs & Story Setup`, `Story Attachment DTOs & Models`?**
  _High betweenness centrality (0.061) - this node is a cross-community bridge._
- **What connects `IsSuccess`, `Value`, `ErrorMessage` to the rest of the system?**
  _99 weakly-connected nodes found - possible missing edges._
- **Should `ServiceResult Pattern & Core Types` be split into smaller, more focused modules?**
  _Cohesion score 0.0628174284950548 - nodes in this community are weakly interconnected._
- **Why does `StoryChatSessionDto` connect `ServiceResult Pattern & Core Types` to `Controllers & Application Entry Points`, `EF Core Migrations & Database Schema`, `Story Attachment DTOs & Models`, `Session Status Enum`?**
  _High betweenness centrality (0.044) - this node is a cross-community bridge._
- **Should `Controllers & Application Entry Points` be split into smaller, more focused modules?**
  _Cohesion score 0.052943354313217325 - nodes in this community are weakly interconnected._
- **Why does `ApplicationDbContext` connect `ApplicationDbContext & DbSets` to `ServiceResult Pattern & Core Types`, `Controllers & Application Entry Points`, `AuthController & Authentication`, `Auth Service & User Identity`, `Ticket Models & Domain`, `Story Chat Attachment Models`, `Story Chat Session Models`, `Story Chat Message Models & DbContext`?**
  _High betweenness centrality (0.034) - this node is a cross-community bridge._
- **Should `AuthController & Authentication` be split into smaller, more focused modules?**
  _Cohesion score 0.08080808080808081 - nodes in this community are weakly interconnected._