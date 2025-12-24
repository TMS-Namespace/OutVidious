---
applyTo: '**'
---

# FrontTube - Copilot Instructions

- **Context:** `Blazor` based, front end alternative/wrapper, to the famous `Invidious`, focused on video and channels management, organization, and caching.
- **Stack:** `.NET 8`, `Blazor Server`, `MudBlazor`, `Serilog`, `Polly`, `BitFaster.Caching`, `Entity Framework Core`, `PostgreSQL`
- **Tags:** @workspace

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
  - **Core** - Top level backend project, fully self-contained (manages everything internally, and does not expose what it reference), and abstracts all business logic in `ViewModels (VMs)` for UI consumption, and uses events to notify UI, It also manages configurations, orchestrates calls to `Providers` and `Repository`.
- **Frontend** - The end user UI implemented in various frameworks.
  - **WebUI** - Blazor based UI, consumes only `ViewModels` from backend (passed as parameters).

### ViewModels (VMs) Architecture

The app uses **Super** pattern. All business logic lives in ViewModels of `Core`, NOT in UI components:

- **Super**: Top level object and central manager and orchestrator for other VMs, manages configurations, exposes and manages other view models for UI consumption, consumed by UI, single instance for the whole UI app.
- **Video**: Represents a video, its metadata, and operations (play, download, cache, etc..), and playing state.
- **Channel**: Represents a channel, its metadata, list of videos, and operations (subscribe, fetch videos, etc..).
- **Image**: Represents an image, its URL, loading state, and operations (load, cache, etc..).
- **Configurations**: Manages various app settings files (the configuration that will represents the whole serialized file, shall inherit `ConfigFile`), loading/saving to disk, encryption of sensitive data. Configurations are exposed directly to the UI via the corresponding VM, so that UI can alter them, single instance, managed by `Super`.

## Coding Standards

- **Conservatism**: Scope of properties getters and setters, methods, classes should be as restrictive as possible (e.g., `public get; private set;`), prefer using `internal` for entities that are not used outside the assembly, and  use `<ProjectReference Include=".." PrivateAssets="all"/>` in project references to avoid not needed exposing.
- **Async methods**: Suffix with `Async`, require `CancellationToken` parameter without default value, pass it to all async calls.
- **Disposable resources**: Everything that subscribes to events/event handlers must implement `IDisposable`, dispose subscriptions/tokens.
- **Error handling**: Log with Serilog, never swallow exceptions without comment explanation, propagate then terminate exceptions at UI, show user-friendly messages at UI level.
- **File organization**: One class/record/enum per file in categorized folders (`Enums/`, `Interfaces/`, `ViewModels/`, `Services/`). If a class becomes too big, split into partial classes across multiple files by grouped functionality, and suffix with `.<GroupPurpose>.cs`.
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
- **Namespaces Aliases**: Use namespace aliases in case of conflicts or for better clarity.

## Critical Patterns

### Database design
- Code-first approach using `Entity Framework Core` and `PostgreSQL`.
- All SQL tables and entities are named using `snake_case`, and mapped via EF Core to `PascalCase` C# classes.
- Used to cache data fetched from external providers, and user data (e.g., subscriptions, play lists, watch history etc...).
- All entities have an `Id` primary key of type `Guid`.
- All enum table names are prefixed with `enum_`.
- All tables that are not shared between multiple users, has names prefixed with `scoped_` and have a `user_id` column to identify the owner user.

### Logging

- Serilog writes to `logs/front-tube-YYYYMMDD.log` (rolling daily), and console.
- Solution root auto-detected by finding `.sln` file in development, or via `AppContext.BaseDirectory` in production.
- Verbose logging everywhere, especially for async operations and failures.
- Objects that can log shall receive `ILoggerFactory` in the constructor, each creates `ILogger<T>` from it and use it.

### ViewModel Creation and Usage
- All ViewModels are created and managed by the `Super`view model.
- `Super` shall be a singleton for the whole UI app, created once via the `Orchestrator` singleton service.
- `Super` receives only `ILoggerFactory` and `IHttpClientFactory` in its constructor, and exposes it an internal property to other VMs. Each view model creates its own `ILogger<T>` from it during construction.
- All view models has reference to `Super`.
- UI components receive ViewModels as parameters from their parent components or pages.
- ViewModels expose events to notify the UI of state changes, data updates, or errors.
- UI components subscribe to these events to update the UI accordingly.
- `ViewModels` should not swallow exceptions silently; they must log errors and propagate them to the UI for user-friendly handling.

### Blazor Components Rules

  - **NEVER** put C# code in `.razor` files - always use `.razor.cs` code-behind
  - **NEVER** use `@code`, `@inject`, `@using` in `.razor` files, prefer code behind or `_Imports.razor`
  - We have only one singleton service that can be injected into components `Orchestrator`, which is used to create the `Super` VM, manage and parse Urls, etc..
  - `Orchestrator` service should contain all of the needed logic to manage and sync other components and services, all possible UI logic should be put in it, if is makes no sense for it to be in VMs, No other services should be created.
  - Components can receive `ViewModels` via `[Parameter]`.
  - Subscribe to `ViewModel` events in `OnParametersSet` or when needed, unsubscribe in `Dispose`
  - Always suffix component names with `Component` and put them in `Components/`, pages with `Page` in `Pages/`, dialogs with `Dialog` in `Dialogs/`.
  - Always avoid using `<div/>` and other raw HTML elements, prefer MudBlazor components instead.
  - Use `EventCallback` for parent communication.
  - Ensure thread-safety with `InvokeAsync(StateHasChanged)` when updating UI from sync `EventHandler` based events.
  - Dispose of all event subscriptions in `Dispose()`.
  - Avoid using `JavaScript` unless absolutely necessary, prefer Blazor and C# implementations.
  - All `JavaScript` interop calls must be wrapped in try-catch and logged on failure.
  - All browser console logs must be captured and logged via Serilog.

### Testing

- All test projects go under `tests/` folder, named as `<ProjectName>.UnitTests`, similar for integration/system tests
- When created, use: `xUnit`, `FluentAssertions`, `Moq`, `Bogus`.
- Always create specific DataGenerator classes for test data generation, with randomized data via `Bogus`, if not mocked.
- When it is needed to access private members, it is allowed to make them `internal`, and use `<InternalsVisibleTo Include="..." />` in the project file, with conservatism in mind.

### Caching Mechanism

- We are caching all of the data fetched from external providers, first in an in-memory cache for fast access, then in a persistent database cache for long-term storage.
- The cached data includes videos, channels, images, and any other relevant metadata.
- Caching is handled by the `Cache` project in the backend, which interacts with the database to store and retrieve cached data.
- All of the cached object types have configurable expiration times, after which they are considered stale and will be refreshed upon the request.
- All database operations that related to caching, shall be done in the background, and return the fetched data to the requester as soon as possible, without waiting for the data base operation to complete.
- Object lookups should be performed using there unique identifiers (which is of `GUID` type) to ensure accurate retrieval.
- The caching logic goes as follows:
  1. When data is requested, first check the in-memory cache.
  2. If data is found and is fresh (not expired), return it.
  3. If data is found but expired, fetch fresh data from the external provider, return it, and update the database cache in the background.
  3. If data is not found in the memory cache, check the database cache, if found and fresh, set it in the memory cache and return it.
  5. If the found data in the database cache is expired, fetch fresh data from the external provider, return it, and update both the memory and database caches in the background.
  4. If the data is not found in the database cache too, fetch it from the external provider, return it, and store it in both the memory and database caches in the background.

## Common Pitfalls

- ❌ Putting logic in `.razor` files instead of `.razor.cs`.
- ❌ Forgetting to unsubscribe from events (causes memory leaks).
- ❌ Not passing `CancellationToken` through async call chains.
- ❌ Hard-coding strings instead of using enums, configuration, defining constants.
- ❌ Ignoring build warnings (always fix them).
- ❌ Not ensuring thread-safety in Blazor components (use `InvokeAsync(StateHasChanged)`).
- ❌ Swallowing exceptions without logging or explanation.
- ❌ Do not use anything marked with [obsolete] attribute, if no replacement is given, ask first.