---
applyTo: '**'
---

# FrontTube - Copilot Instructions

**Context:** `Blazor` based, front end alternative/wrapper, to the famous `Invidious`, focused on video and channels management, organization, and caching.
**Stack:** `.NET 8`, `Blazor Server`, `MudBlazor`, `Serilog`, `Polly`, `BitFaster.Caching`, `Entity Framework Core`, `PostgreSQL`
**Tags:** @workspace

## Architecture Overview

### Project Structure
- **Backend** - Various projects to handle business logic, data access, caching, view models, etc. and consists of:
  - **Providers** - Abstractions and implementations for external extractors (e.g., `YouTube`, `Invidious` instances)
    - **Invidious** - Connects to Invidious instances, fetches video/channel data ect.. and maps them to common contracts.
  - **Common** - Shared models, enums, interfaces, utilities across backend projects.
    - **ProviderCore** - the shared contracts, models, enums, and interfaces between `Core` and various `Providers`.
  - **Repository** - Data access layer, handles local storage of cached objects etc.
    - **DataBase** - A Code first, `PostgreSQL` database access via EF Core, migrations, DbContext, entities.
    - **Cache** - Caching layer, abstracts caching logic, uses `DataBase` to persist cached data, and `Providers` to fetch data when not in cache.
  - **Core** - Top level backend project, that abstracts all business logic in `ViewModels (VMs)` for UI consumption, and uses events to notify UI, It also manages configurations, orchestrates calls to `Providers` and `Repository`.
- **Frontend** - The end user UI implemented in various frameworks.
  - **WebUI** - Blazor based UI, consumes only `ViewModels` from backend (passed as parameters).

### ViewModels (VMs) Architecture

The app uses **Super** pattern. All business logic lives in ViewModels of `Core`, NOT in UI components:

- **Super**: Top level object and central manager and orchestrator for other VMs, manages configurations, exposes and manages other view models for UI consumption, consumed by UI, single instance for the whole UI app.
- **Video**: Represents a video, its metadata, and operations (play, download, cache, etc..), and playing state.
- **Channel**: Represents a channel, its metadata, list of videos, and operations (subscribe, fetch videos, etc..).
- **Image**: Represents an image, its URL, loading state, and operations (load, cache, etc..).
- **Configurations**: Manages various app settings files (the configuration that will represents the whole serialized file, shall inherit `ConfigFile`), loading/saving to disk, encryption of sensitive data. Configurations are exposed directly to the UI via the corresponding VM, so that UI can alter them, single instance, managed by `Super`

## Critical Patterns

### Blazor Component Rules

- **NEVER** put C# code in `.razor` files - always use `.razor.cs` code-behind
- **NEVER** use `@code`, `@inject`, `@using` in `.razor` files, prefer code behind or `_Imports.razor`
- We have only one service that can be injected into components `Orchestrator`, which is used to create the `Super` VM, manage and parse Urls, etc..
- `Orchestrator` service should contain all of the needed logic to manage and sync other components and services, all possible UI logic should be put in it, if is makes no sense for it to be in VMs, No other services should be created.
- Components can receive `ViewModels` via `[Parameter]`.
- Subscribe to `ViewModel` events in `OnParametersSet` or when needed, unsubscribe in `Dispose`
- Always suffix component names with `Component` and put them in `Components/`, pages with `Page` in `Pages/`, dialogs with `Dialog` in `Dialogs/`.
- Always avoid using `<div/>` and other raw HTML elements, prefer MudBlazor components instead.

### Logging

- Serilog writes to `logs/front-tube-YYYYMMDD.log` (rolling daily).
- Solution root auto-detected by finding `.sln` file in development, or via `AppContext.BaseDirectory` in production.
- Verbose logging everywhere, especially for async operations and failures.
- Objects that can log shall receive `ILoggerFactory` in the constructor, each creates `ILogger<T>` from it and use it.

## Coding Standards

- **Conservatism**: Scope of properties getters and setters, methods, classes should be as restrictive as possible (e.g., `public get; private set;`), prefer using `internal` for entities that are not used outside the assembly, and  use `<ProjectReference Include=".." PrivateAssets="all"/>` in project references to avoid not needed exposing.
- **Async methods**: Suffix with `Async`, require `CancellationToken` parameter without default value, pass it to all async calls.
- **Disposable resources**: Everything that subscribes to events/event handlers must implement `IDisposable`, dispose subscriptions/tokens.
- **Error handling**: Log with Serilog, never swallow exceptions without comment explanation, propagate then terminate exceptions at UI, show user-friendly messages at UI level.
- **File organization**: One class/record/enum per file in categorized folders (Enums/, Interfaces/, ViewModels/, Services/). If a class becomes too big, split into partial classes across multiple files by grouped functionality, and suffix with `.<GroupPurpose>.cs`.
- **Immutability**: Prefer records over classes when possible.
- **Strong typing**: Use enums instead of strings (e.g., `FileOperationType`, `ConflictResolutionStrategy`)
- **C# 13 conventions**: File-scoped namespaces, required properties, records, etc.
- **Package management**: Centralized in `Directory.Packages.props`.
- **Abstractions and Isolation**: Use SOLID principles whenever it makes sense.
- **Code style**: Clear naming, empty lines between members and code blocks, always use `{}` for control structures, lists/collection etc.. variables shall be always initialized to empty lists.
- **Conciseness**: Keep classes and methods focused and concise.
- **Reuse**: Before creating new variables, methods, classes, ensure across projects that there is not already similar implementation that can be reused or extended.
- **UI is for display only**: No business logic in UI components, all in `ViewModels`, only very UI-specific logic allowed.
- **Simple Mappers**: Use simple mappers to convert between Models and `ViewModels`, avoid having logic in them, or throwing exceptions.
- **View Models First**: always prefer writing functions to use and pass around `ViewModels` instead of variables like path etc.
- **Razor/HTML formatting**: Always format Blazor/HTML code with proper indentation and line breaks for better readability. Always put each attribute in an aligned new line. Separate components with empty lines.
- **Resiliency**: Use `Polly` for transient fault handling in all external calls (e.g., HTTP requests to Invidious instances), with configurable retries count, exponential backoff, and circuit breakers as appropriate.
- **No Hard-Coding**: No hard-coded strings, URLs, paths, or configuration values. Use enums, configuration files, or clearly defined constants.

## Key Workflows

### Components Rules

1. Receive ViewModels as parameters: `[Parameter]`.
2. Subscribe to events in `OnParametersSet()` or when needed, unsubscribe in `Dispose()`.
3. Use `EventCallback` for parent communication.
4. Ensure thread-safety with `InvokeAsync(StateHasChanged)` when updating UI from sync `EventHandler` based events.
5. Dispose of all event subscriptions in `Dispose()`.

### Testing

- All test projects go under `tests/` folder, named as `<ProjectName>.UnitTests`, similar for integration/system tests
- When created, use: `xUnit`, `FluentAssertions`, `Moq`, `Bogus`.
- Always create specific DataGenerator classes for test data generation, with randomized data via `Bogus`, if not mocked.
- When it is needed to access private members, it is allowed to make them `internal`, and use `<InternalsVisibleTo Include="..." />` in the project file, with conservatism in mind.

### Caching Mechanism

## Common Pitfalls

- ❌ Putting logic in `.razor` files instead of `.razor.cs`.
- ❌ Forgetting to unsubscribe from events (causes memory leaks).
- ❌ Not passing `CancellationToken` through async call chains.
- ❌ Hard-coding strings instead of using enums, configuration, defining constants.
- ❌ Ignoring build warnings (always fix them).
- ❌ Not ensuring thread-safety in Blazor components (use `InvokeAsync(StateHasChanged)`).
- ❌ Swallowing exceptions without logging or explanation.
- ❌ Do not use anything marked with [obsolete] attribute, if no replacement is given, ask first.