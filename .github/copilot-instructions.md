---
applyTo: '**'
---

# SWFT - Copilot Instructions

**Context:** Blazor Server file manager using ViewModel architecture, for SFTP, FTPS, SMB, SSH, and local file system.
**Stack:** .NET 8, Blazor Server, MudBlazor, Serilog, Polly  
**Tags:** @workspace

## Architecture Overview

### Project Structure

- **Common** - Interfaces (`RawClientBase` etc.), contracts, enums, exceptions. Meant to abstract various client behaviors that are shared between `RawClients` and `Core` project only. (however the custom `Exceptions` are shared between all projects).
- **Core** - Abstracts all business logic in ViewModels (VMs) for UI consumption, and uses events to notify UI
- **RawClients** - Abstracts via `RawClientBase` various implementations for file access protocols/methods. Loaded using plug-in architecture. 
- **WebGUI** - Consumes ViewModels (passed as parameters) and renders UI using Blazor Server and MudBlazor

### ViewModels (VMs) Architecture

The app uses **Super** pattern. All business logic lives in ViewModels, NOT in UI components:

- **Super**: Top level and central manager for other VMs, manages configurations, exposes and manages other view models for UI consumption, consumed by UI, single instance per page.
- **Configurations**: Manages various app settings files (the configuration that will represents the whole serialized file, shall inherit `ConfigFile`), loading/saving to disk, encryption of sensitive data. Configurations are exposed directly to the UI via the corresponding VM, so that UI can alter them, single instance, managed by `Super`
- **Provider**: Represents a file system access protocol/method, i.e. one particular `RawClientBase` implementation. Manages `DataSource`, managed by `Super`, that creates a list of them based on configurations
- **DataSource**: Represents a configuration for a specific file server/file system. Manages a list of `Client`.Exposes its own configs to the UI, managed by `Provider`, that can create/update them using `Configurations` VM
- **Client**: Represents a single connection/session to a specific `DataSource`, wraps and manages the underlying `RawClientBase` instance, exposes connection state/events to the UI, managed by `DataSource`, that can create multiple clients (i.e. multiple connections to same data source)
- **ItemLister**: Represents a file browser to specific `DataSource` via specific `Client`, manages navigation/selections/listing directory contents of items, also it can perform various file/directory manipulations via `Item` (if the operation is in-place, i.e. happens always in the same directory, and hence needs no clipboard entry, like renaming, uploading, downloading, creating or deleting), managed by `Super`, that can create multiple listers and attach/connect them with data sources
- **Item**: Represents a file, directory or symbolic link/shortcut, exposes (via `Operations` VM) in-place operations, references single lister, managed by `ItemLister`
- **Operations**: Manages cross-provider and cross-server file operations queue and log of those operations, single instance, managed by `Super`
- **Operation**: Contains the heavy logic for single item operations, multi-item operations, recursive operations like deleting/coping/moving/downloading/uploading directories, calculating directory size, as well as reporting those operations progress, may reference a `ClipboardEntry` if relevant, managed by `Operations`
- **Clipboard**: Represents the cross-Item-listers clipboard state for copy/cut/paste/move operations, single instance, manages a list of `ClipboardEntry` VMs, managed by `Super`
- **ClipboardEntry**: Represents a single entry in the clipboard, references source and destination `ItemLister`s, source and destination items, creates and reference new `Operation` (via `Operations`) when executed, managed by `Clipboard`
- **Bookmarks**: Represents and manages the saved bookmarks to a specific path on a specific `DataSource`, inherits the simple VM `Breadcrumb`, exposes its config to the UI, single instance, managed by `Super`
- Each VM shall have a reference to `Super`, if single instance, or to its parent VM if multiple instances.
- All VMs, except `Super`, Inherits `ViewModelBase` class.
- I/O related VMs (`Client`, `DataSource`, `Operations`, `Configurations`, `ItemLister`), inherits `IOViewModelBase` class (which inherits `ViewModelBase` and `IDisposable`), and exposes state events and properties.

## Critical Patterns

### Blazor Component Rules

- **NEVER** put C# code in `.razor` files - always use `.razor.cs` code-behind
- **NEVER** use `@code`, `@inject`, `@using` in `.razor` files, prefer code behind or `_Imports.razor`
- We have only one service that can be injected into components `Orchestrator`, which is used to create the `Super` VM, manage and parse Urls, etc..
- `Orchestrator` service should contain all of the needed logic to manage and sync other components and services, all possible UI logic should be put in it, if is makes no sense for it to be in VMs, No other services should be created.
- Components can receive ViewModels via `[Parameter]`.
- Subscribe to ViewModel events in `OnParametersSet` or when needed, unsubscribe in `Dispose`
- Always suffix component names with `Component`, pages with `Page`, dialogs with `Dialog`

### File/Directory Operations

- All functions that uses or manipulates paths, must be host OS agnostic, since this path is not on the current machine, so using for example `Path.Combine` is not correct, instead use `PathManager` helper class, that implements .NET's `Path` methods in a host-agnostic way. The only exception for this, is the places where we are reading configuration file or loading plugins frm the local file system.
- All file/directory operations (copy/move/delete/rename/upload/download) are performed via `Operations` VM, which manages a queue of `Operation` VMs.
- All operations shall be robust, resilient to all failures, with retry policy (via `Polly`) and report progress and errors to the UI in an async, thread safe way.
- Each `Operation` VM performs a single operation (which may be recursive or multi-item), and reports state via events.
- `Operation` VMs are created by `ClipboardEntry` VMs when clipboard operations are executed, or by `Item` VMs when in-place operations are performed
- `Operation` VMs are executed sequentially, one at a time, to avoid overwhelming servers and ensure predictable progress reporting.
- `Operation` VMs create and use dedicated `Client` when the operation is not in-place, otherwise uses the `Client` of the `ItemLister` that owns the `Item` being manipulated.
- On copy/paste between different data sources, `Operation` VM handles reading from source and writing to destination, reporting progress accordingly
- (Critical!) Copy/paste operations shall handle conflicts (e.g., file already exists) based on in UI user-chosen strategy (skip/overwrite/rename/auto-rename for single items, and skip/skip all/overwrite/overwrite all/rename/rename-all for multiple items or directories, in addition to the ability to pause or cancel), and report any errors encountered during the operation
- All operations support cancellation via `CancellationToken`, and report completion status (done/failure/cancelled etc.. defined in `IOStateType` and `IOViewModelBase`) via events
- `Operation` VM file shall be split into partial classes, with naming like `Operations.Single.File.cs`, `Operations.Multi.File.cs`, `Operations.Single.Directory.cs` etc.. to keep each file focused and concise

### Data Persistence

- Configs are stored in `Data/providers.json` (encrypted), `Data/bookmarks.json`, file format definitions in `Data/file-formats.json`
- Encryption key configured in `appsettings.json` under `Encryption:Key`
- Data directory auto-located at solution root or configured via `Storage:DataDirectory`

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

### Building & Running

```bash
# Build entire solution
dotnet build

# Run WebGUI (from solution root)
dotnet run --project src/WebGUI/TMS.Apps.Web.SWFT.WebGUI.csproj

# Access at https://localhost:5001 or http://localhost:5000
```

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