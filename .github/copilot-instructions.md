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
- All time like columns use `DateTime` with no zone (always assumed to be `UTC`).
- All entities have an `Id` primary key of type `integer`, with auto generated serial values.
- All enum table names are prefixed with `enum_`.
- All tables that are not shared between multiple users, has names prefixed with `scoped_` and have a `user_id` column to identify the owner user.
- All entities that are cached from external providers, should have the following columns always:
  - Not nullable `absolute_remote_url`, for the original source URL.
  - Not nullable and unique `hash` column for lookup, that uses `XxHash64` hash.
  - Nullable `last_synced_at` `DateTime` column to track when it was last fetched.

### Logging

- Serilog writes to `logs/front-tube-YYYYMMDD.log` (rolling daily), and console.
- Solution root auto-detected by finding `.sln` file in development, or via `AppContext.BaseDirectory` in production.
- Verbose logging everywhere, especially for async operations and failures.
- Objects that can log shall receive `ILoggerFactory` in the constructor, each creates `ILogger<T>` from it and use it.
- Log message should always finish with a dot.
- Log message should be of the following format: `[Method Name] Text 'Some parameter' text.` where `nameOf()` is used for `Method Name`, and parameters are logged via structured logging, and put in between single quotes. Finally, the message shall always end with a dot.
- Always log exceptions with `LogError(ex, "...")`, never just the message.
- Error logs from catch statements, should start always with `[Method Name] Unexpected error: Text`.
- All potentially spammy logs (e.g., in loops, frequent operations) should be at `Debug` level, not `Information`.


### Provider Integration
- All external data fetching is abstracted via `IProvider` interfaces.
- All providers return `Common` contracts defined in `ProviderCore` project, which are then mapped to database entities and view models.
- All `Common` contracts implement `ICacheableCommon` (since they are mapped to cacheable database entities) for easier handling in `CacheManager` and `RepositoryManager`.
- There are two types of `Common` contracts that Youtube usually returns:
  1. Objects that contain partial descriptive information, we call them `Metadata` objects (e.g., `VideoMetadata`, `ImageMetadata`), for example `ImageMetadata` does not contain the actual image bytes (needs to be fetched separately), just its URL, dimensions, etc..
  2. Objects with full descriptive information, we just call them (e.g., `Video`, `Channel`), However, they still do not contain full objects, for example `Video` does not contain the actual stream bytes, and needs to be fetched separately.

### Caching Mechanism
- Caching is handled by the `Cache` project in the backend, which interacts with the database and `IProvider` to store and retrieve cached data.
- From caching perspective, there are three sources of data:
  1. External providers (e.g., Invidious instances) - the original source of truth.
  2. Database cache - persistent storage of cached data fetched from external providers.
  3. In-memory cache - which is a second level `EF Core` cache for fast access during runtime, hence, query results (i.e database entities) are what is cached in memory.
- The cached data includes videos, channels, images, and any other relevant metadata, all cached database entities uses `ICacheableEntity`.
- `CacheManager` is responsible for managing the caching logic and interactions with both the database caches, and `provider`'s data, however only for top level objects, i.e. if a video has images, only the video is managed by `CacheManager`, while images will be managed by `RepositoryManager`, that in turn will use `CacheManager` that will trait them as top level objects then.
- `CacheManager` is responsible to check if the item is already cached in the database, and if it is still fresh (not expired/stale), if so, it returns it directly, otherwise it fetches fresh data from the external provider, maps it to the corresponding database entity, returns it, and updates the database cache in the background.
- `CacheableIdentity` is used to uniquely identify objects that can be cached, it combines `AbsoluteRemoteUrl`, `RemoteID`, and `Hash` (that built from `AbsoluteRemoteUrl`).
- We always consider `Hash` as the unique identifier of cached objects, and use it for lookups.
- `CacheManager` is responsible for managing the caching logic and interactions with both the database caches, and provider's data .
- All of the cached object types have configurable expiration times in `CacheConfig`, after which they are considered stale and will be refreshed upon the cache lookup.

### Repository Manager
- `RepositoryManager` is responsible for managing complex operations involving multiple cached entities, such as videos with their associated images, or channels with their videos.
- It orchestrates calls to `CacheManager` for each individual entity type, ensuring that all related entities are properly cached and linked together.
- It is also responsible to fetch separately the data that is not part of `Common` contracts, for example, fetching images bytes.
- It outputs `ViewModels` that are ready for UI consumption, by mapping the cached database entities or `Common` contracts to the corresponding `ViewModels` using simple mappers.

### ViewModel Creation and Usage
- All ViewModels are created and managed by the `Super` view model.
- Most `ViewModels`, are basically wrappers around database entities, or `Common` contracts, with added logic to manage state, operations, and events. All except super, inherit from `ViewModelBase`.
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



## Common Pitfalls

- ❌ Putting logic in `.razor` files instead of `.razor.cs`.
- ❌ Forgetting to unsubscribe from events (causes memory leaks).
- ❌ Not passing `CancellationToken` through async call chains.
- ❌ Hard-coding strings instead of using enums, configuration, defining constants.
- ❌ Ignoring build warnings (always fix them).
- ❌ Not ensuring thread-safety in Blazor components (use `InvokeAsync(StateHasChanged)`).
- ❌ Swallowing exceptions without logging or explanation.
- ❌ Do not use anything marked with [obsolete] attribute, if no replacement is given, ask first.