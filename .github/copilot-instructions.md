---
applyTo: '**'
---

# OutVidious - Copilot Instructions

**Context:** Blazor Server base, front end alternative/wrapper, to the famous Invidious YouTube front-end, focused on video and channels management and organization.
**Stack:** .NET 8, Blazor Server, MudBlazor, Serilog, Polly  
**Tags:** @workspace

## Architecture Overview

### Project Structure

- **Core** - Abstracts all business logic in ViewModels (VMs) for UI consumption, and uses events to notify UI
- **WebGUI** - Consumes ViewModels (passed as parameters) and renders UI using Blazor Server and MudBlazor

### ViewModels (VMs) Architecture

The app uses **Super** pattern. All business logic lives in ViewModels, NOT in UI components:

- **Super**: Top level and central manager for other VMs, manages configurations, exposes and manages other view models for UI consumption, consumed by UI, single instance per page.
- **Configurations**: Manages various app settings files (the configuration that will represents the whole serialized file, shall inherit `ConfigFile`), loading/saving to disk, encryption of sensitive data. Configurations are exposed directly to the UI via the corresponding VM, so that UI can alter them, single instance, managed by `Super`

## Critical Patterns

### Blazor Component Rules

- **NEVER** put C# code in `.razor` files - always use `.razor.cs` code-behind
- **NEVER** use `@code`, `@inject`, `@using` in `.razor` files, prefer code behind or `_Imports.razor`
- We have only one service that can be injected into components `Orchestrator`, which is used to create the `Super` VM, manage and parse Urls, etc..
- `Orchestrator` service should contain all of the needed logic to manage and sync other components and services, all possible UI logic should be put in it, if is makes no sense for it to be in VMs, No other services should be created.
- Components can receive ViewModels via `[Parameter]`.
- Subscribe to ViewModel events in `OnParametersSet` or when needed, unsubscribe in `Dispose`
- Always suffix component names with `Component`, pages with `Page`, dialogs with `Dialog`

### Logging

- Serilog writes to `logs/swft-YYYYMMDD.log` (rolling daily)
- Solution root auto-detected by finding `.sln` file
- Verbose logging everywhere, especially for async operations and failures

## Coding Standards

- **Conservatism**: Scope of properties getters and setters, methods, classes should be as restrictive as possible (e.g., `public get; private set;`)
- **Async methods**: Suffix with `Async`, require `CancellationToken` parameter without default value, pass it to all async calls
- **Disposable resources**: Everything that subscribes to events/event handlers must implement `IDisposable`, dispose subscriptions/tokens
- **Error handling**: Log with Serilog, never swallow exceptions without comment explanation, propagate then terminate exceptions at UI, show user-friendly messages at UI level.
- **File organization**: One class/record/enum per file in categorized folders (Enums/, Interfaces/, ViewModels/, Services/). If a class becomes too big, split into partial classes across multiple files by grouped functionality, and suffix with `.<GroupPurpose>.cs`.
- **Immutability**: Prefer records over classes when possible
- **Strong typing**: Use enums instead of strings (e.g., `FileOperationType`, `ConflictResolutionStrategy`)
- **C# 13 conventions**: File-scoped namespaces, required properties, records, etc
- **Package management**: Centralized in `Directory.Packages.props`
- **Abstractions and Isolation**: Use SOLID principles whenever it makes sense
- **Code style**: Clear naming, empty lines between members and code blocks, always use `{}` for control structures, lists/collection etc.. variables shall be always initialized to empty lists
- **Conciseness**: Keep classes and methods focused and concise
- **Reuse**: Before creating new variables, methods, classes, ensure across projects that there is not already similar implementation that can be reused or extended.
- **UI is for display only**: No business logic in UI components, all in ViewModels, only very UI-specific logic allowed
- **Simple Mappers**: Use simple mappers to convert between Models and ViewModels, avoid having logic in them, or throwing exceptions
- **View Models First**: always prefer writing functions to use and pass around ViewModels instead of variables like path etc.

## Key Workflows

### Components Rules

1. Receive ViewModels as parameters: `[Parameter]`
2. Subscribe to events in `OnParametersSet()` or when needed, unsubscribe in `Dispose()`
3. Use `EventCallback` for parent communication
4. Ensure thread-safety with `InvokeAsync(StateHasChanged)` when updating UI from sync `EventHandler` based events.
5. Dispose of all event subscriptions in `Dispose()`

### Testing

- All test projects go under `tests/` folder, named as `<ProjectName>.UnitTests`, similar for integration/system tests
- When created, use: xUnit, FluentAssertions, Moq, Bogus (already in `Directory.Packages.props`)
- Always create specific DataGenerator classes for test data generation, if not mocked

## Common Pitfalls

- ❌ Putting logic in `.razor` files instead of `.razor.cs`
- ❌ Forgetting to unsubscribe from events (causes memory leaks)
- ❌ Not passing `CancellationToken` through async call chains
- ❌ Hard-coding strings instead of using enums, configuration, defining constants
- ❌ Ignoring build warnings (always fix them)
- ❌ Not ensuring thread-safety in Blazor components (use `InvokeAsync(StateHasChanged)`)
- ❌ Swallowing exceptions without logging or explanation
- ❌ Do not use anything marked with [obsolete] attribute, if no replacement is given, ask first