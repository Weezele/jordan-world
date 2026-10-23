# Architecture

Clean Architecture, feature-first. Every (feature, layer) is its own
`.asmdef`, so illegal references are **compile errors**, not review nits.

## Layer dependency rules

```
                 ┌──────────────────────┐
                 │  App  (composition)  │
                 └──────────┬───────────┘
                            │ sees concrete types
                            ▼
   ┌──────────────────────────────────────────────┐
   │ Features/<Name>/Installers (VContainer wire) │
   └───────┬─────────────────────┬────────────────┘
           │                     │
           ▼                     ▼
   ┌───────────────┐     ┌───────────────┐
   │ Presentation  │     │ Infrastructure│
   │ (MonoB, UI)   │     │ (Cesium, I/O) │
   └───────┬───────┘     └───────┬───────┘
           │                     │
           ▼                     ▼
         ┌──────────────────────────┐
         │     Application          │
         │ (use cases + ports)      │
         └──────────────┬───────────┘
                        ▼
         ┌──────────────────────────┐
         │        Domain            │
         │ (pure C#, no engine ref) │
         └──────────────────────────┘
```

- **Domain** depends on nothing (except `Core.Domain`). `noEngineReferences: true`.
- **Application** depends only on Domain + `Core.Application`.
- **Infrastructure** implements Application ports; may use UnityEngine, Cesium, I/O.
- **Presentation** reads/writes Application; may use UnityEngine + UI Toolkit.
- **Infrastructure ↔ Presentation**: **never** reference each other.
- Cross-feature talk happens only through `Core.Application` interfaces or
  **MessagePipe** events.
- Only `App` and `Installers` see concrete types from Infrastructure/Presentation.

## Folder shape

```
Assets/_Project/
  App/                                   App.asmdef
  Core/
    Domain/        Core.Domain.asmdef           (noEngineReferences)
    Application/   Core.Application.asmdef
    Infrastructure/Core.Infrastructure.asmdef
    Presentation/  Core.Presentation.asmdef
  Features/
    <FeatureName>/
      Domain/        <Feature>.Domain.asmdef    (noEngineReferences)
      Application/   <Feature>.Application.asmdef
      Infrastructure/<Feature>.Infrastructure.asmdef
      Presentation/  <Feature>.Presentation.asmdef
      Installers/    <Feature>.Installers.asmdef
      Tests/EditMode/<Feature>.Tests.EditMode.asmdef
      Tests/PlayMode/<Feature>.Tests.PlayMode.asmdef
```

## Patterns

- **DI**: VContainer (root LifetimeScope in `App`, child scopes per feature).
- **Async**: UniTask everywhere; no `async void`, no `Task`.
- **Messaging**: MessagePipe for cross-feature events.
- **Reactive state**: R3 (`ReadOnlyReactiveProperty<T>`).
- **UI**: UI Toolkit.
- **Presentation**: Cubit-like ViewModels.
  - State is an immutable `record`.
  - Intent methods (`OnFoo()`) mutate by emitting a new state.
  - Views subscribe to `State` and render; they never hold logic.
- **Domain value objects**: `GeoCoord`, `TileId`, `Meters`, `Speed`, …
  (no primitive obsession).
- **Use cases**: one class, one `ExecuteAsync` method.
- **MonoBehaviours**: thin adapters only. No logic in `Update` that can't be
  unit-tested in Domain/Application.

## Hot paths

Mesh generation, tile streaming, physics prep → Jobs + Burst +
Unity.Mathematics in Infrastructure, behind Application ports.
**Zero per-frame GC allocations** in gameplay loops.

## Architecture test

`Core.Tests.EditMode/ArchitectureTests.cs` reflects over loaded assemblies
and asserts the rules above. Breaking a rule must fail CI.
