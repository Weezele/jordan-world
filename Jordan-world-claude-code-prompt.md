# Claude Code prompt — "Jordan World" mobile game (Unity, Clean Architecture, feature-first)

Paste everything below the line into Claude Code at the root of an empty git repo. Install Unity Hub and **Unity 6.3 LTS** with the Android and iOS modules first.

---

## Role & goal

You are the lead engineer on **Jordan World**, a **native mobile game for Android and iOS** (not a website) built in **Unity 6.3 LTS with C#**.

It is an open world covering the **entire Hashemite Kingdom of Jordan** on real geography. The player can **walk**, **drive a car**, and **fly a plane**, switching modes seamlessly.

References:
- **ammandrive.app**: Amman generated procedurally from OpenStreetMap plus elevation data. It has a stylized look.
- **wascape.com**: photoreal Gaussian-splat captures of single streets.

Our target:
- Ammandrive's procedural, country-scale approach, with **clearly better rendering**: PBR, atmosphere, shadows, believable Amman limestone architecture, and convincing desert materials.
- Optional photoreal splat "hero" locations later.

## Non-negotiable architecture: Clean Architecture, feature-first

Structure everything by feature, then by layer. **Every layer of every feature is its own `.asmdef`**, so that dependency rules are enforced by the compiler.

```
Assets/_Project/
  App/                                # Composition root only
    App.asmdef                         # Bootstrap scene, root LifetimeScope; references all features
  Core/                               # Shared kernel — keep tiny
    Domain/      Core.Domain.asmdef        # noEngineReferences: true. Result<T>, Errors, GeoCoord, value objects, IClock
    Application/ Core.Application.asmdef   # cross-feature contracts (interfaces, event/message types)
    Infrastructure/ Core.Infrastructure.asmdef  # logging, file system, HTTP, persistence adapters
    Presentation/ Core.Presentation.asmdef # base ViewModel/Presenter, UI Toolkit helpers
  Features/
    <FeatureName>/
      Domain/          <Feature>.Domain.asmdef          # pure C#, noEngineReferences: true
      Application/     <Feature>.Application.asmdef     # use cases, ports (interfaces); refs Domain + Core.Application
      Infrastructure/  <Feature>.Infrastructure.asmdef  # implements ports: Cesium, file/network, Unity APIs
      Presentation/    <Feature>.Presentation.asmdef    # MonoBehaviours, Views, ViewModels, prefabs, UI
      Installers/      <Feature>.Installers.asmdef      # VContainer registrations for this feature
      Tests/EditMode/  <Feature>.Tests.EditMode.asmdef
      Tests/PlayMode/  <Feature>.Tests.PlayMode.asmdef
```

**Features**, created as their phases arrive:
- `GeoReference` (projection, floating origin)
- `Terrain`
- `Roads`
- `Buildings`
- `WorldStreaming` (tile scheduler)
- `Input`
- `CameraRig`
- `Player` (mode switching, shared `IPlayerMode`)
- `Walk`
- `Drive`
- `Fly`
- `Environment` (sky, weather, time of day)
- `Map` (minimap, fast travel)
- `Settings` (quality tiers)
- `Localization` (Arabic/English, RTL)
- `Landmarks`
- `HeroZones` (splats)
- `Audio`

**Dependency rules**:
- Domain depends on nothing (except Core.Domain).
- Application depends only on Domain.
- Infrastructure and Presentation depend on Application and Domain, never on each other.
- **Features never reference another feature's internals.** They talk only through contracts in `Core.Application`, or through MessagePipe messages.
- Only `App` and `Installers` see concrete types.
- Add an EditMode **architecture test** that reflects over the assemblies and fails if any rule is broken.

**Patterns**:
- DI with **VContainer**.
- Async with **UniTask**.
- Messaging with **MessagePipe**.
- Reactive state with **R3**.
- UI with **UI Toolkit**.
- **Presentation uses a Cubit-like ViewModel**: an immutable `record` state, intent methods, and state exposed as `ReadOnlyReactiveProperty<TState>`. Views only render state and forward input.
- Domain uses value objects (`GeoCoord`, `TileId`, `Meters`, `Speed`) instead of primitive obsession.
- Use cases are single-purpose classes with one `Execute`/`ExecuteAsync` method.
- MonoBehaviours stay thin adapters. No game logic in `Update` that can't be unit-tested.

**Hot paths** (mesh generation, streaming, physics prep):
- Use the Jobs System + Burst + Unity.Mathematics inside the Infrastructure layer, behind Application ports.
- Avoid GC allocations per frame.

## Working rules for you (Claude Code)

1. **Work phase by phase.** On the first run, create:
   - `docs/PLAN.md` (these phases);
   - `docs/PROGRESS.md`;
   - `docs/DECISIONS.md` (short ADRs);
   - `docs/ATTRIBUTION.md`;
   - `docs/ARCHITECTURE.md` (with a diagram of features, layers and allowed dependencies).
2. **At the start of each phase:** restate the goal, list the tasks, and **verify the current versions and APIs** of every package you touch (Unity docs, GitHub releases, OpenUPM). Never trust memory for APIs.
3. **Everything must be reproducible from code and CLI.** I won't hand-click in the editor.
   - Build scenes, prefabs, materials and project settings through **editor scripts** (`[MenuItem]` plus `-executeMethod`), not hand-edited YAML.
   - Force text serialization.
   - Use Git LFS for binaries.
   - Use a Unity `.gitignore`.
4. **Driving the Unity Editor — use the official Unity CLI.**
   - In Phase −1, help me install it and set it up: `unity pipeline install`, `unity mcp configure claude`, and install the Unity skills for Claude Code (check `unity skill install` / `unity mcp configure --list` for the right agent name).
   - Prefer the direct `unity command` / `unity eval` over MCP for speed. Use MCP mode only where direct commands can't do the job.
   - Use the CLI to read the console, fix compile errors, create and inspect GameObjects, run tests and build — so I rarely need to click.
   - Verify the current CLI docs before use. Do **not** use the deprecated in-editor AI Assistant MCP server.
   - The Unity Editor must be open for live commands, so tell me when to open it.
5. **CLI loop:**
   - Run tests with `Unity -batchmode -projectPath . -runTests -testPlatform EditMode|PlayMode -testResults ...`.
   - Build with `-executeMethod Build.Android` / `Build.iOS`.
   - Wrap these in a `Makefile` or `scripts/` (`make test`, `make build-android`, `make tiles AREA=amman`).
6. **At the end of each phase:**
   - All tests pass, including the architecture test.
   - A PlayMode smoke test loads the scene, flies or drives a scripted path, captures `docs/screens/phase-N.png`, and logs FPS, draw calls, batches, memory and GC allocations.
   - Update PROGRESS.md and commit with a conventional message.
   - Then **STOP and wait for me to reply "go"**.
7. **Ask me first** before anything that costs money (Cesium ion paid tiers, CDN, Google APIs), before creating accounts, before downloads over 2 GB, and before changing the stack.
8. **Licenses:**
   - Use only data and assets that are compatible with a free, ad-supported commercial mobile game.
   - Log each one in ATTRIBUTION.md and show the credits in-game.
   - **Never cache Google Photorealistic 3D Tiles.**
9. **Teach mode — I am new to Unity.**
   - I'm an experienced Flutter/Dart developer: BLoC/Cubit, clean architecture, feature-first. I have never used Unity.
   - Every time a phase introduces a Unity concept, add a short entry to `docs/LEARNING.md`: what it is, a Flutter analogy where one fits, and the gotchas.
   - When something has to be done in the editor, give me exact click-by-click steps.
   - At the end of each phase, give me a 3–5 bullet "what you should now understand" summary, plus one small change I should make myself to practise.
   - Keep explanations short. No essays.
10. **Performance budgets** (URP mobile):
   - Mid-range Android, "medium" tier: 30 fps stable.
   - Flagship, "high" tier: 60 fps.
   - Under ~300 batches per frame.
   - Under 1.2 GB RAM.
   - Zero per-frame GC allocations in gameplay loops.
   - Quality tier auto-detected at first launch.

## Stack (verify each in Phase 0)

- **Unity 6.3 LTS**, URP (Forward+). Evaluate GPU Resident Drawer and STP upscaling on mobile.
- **Scripting backend**: IL2CPP, ARM64 only (Cesium requires this).
- **Cesium for Unity**: `CesiumGeoreference`, `CesiumOriginShift` (floating origin) and terrain streaming. We use it as **infrastructure only**, behind the `GeoReference` and `Terrain` ports.
- **Packages**: VContainer, UniTask, MessagePipe, R3, Unity Input System, Cinemachine, Addressables, Localization package, Unity Test Framework.
- **Physics**: Unity PhysX. Use WheelCollider, or a custom raycast vehicle if WheelCollider isn't good enough. Write an ADR either way.
- **Offline data pipeline** in `/pipeline`, outside `Assets/`: Python in Docker, using GDAL, osmium, pyproj, shapely and rasterio.
  - It outputs terrain tiles (quantized-mesh, or heightmaps) and **compact binary vector tiles** for roads, buildings, landuse and POIs, using FlatBuffers or MessagePack with a versioned schema.
  - These tiles feed runtime procedural mesh generation in Unity.

## Geography

- Jordan bbox ≈ lon 34.9–39.3°E, lat 29.2–33.4°N.
- Elevation runs from about −430 m (Dead Sea) to about 1,854 m (Jabal Umm ad Dami).
- Cesium georeference uses WGS84/ECEF. The offline pipeline tiles in a quadtree (document the scheme in DECISIONS.md).

## Data sources (verify the current URL and license before use)

| Layer | Source | License |
|---|---|---|
| Roads, POIs, landuse, water, airports | OpenStreetMap Jordan extract (Geofabrik) | ODbL |
| Missing building footprints | Microsoft GlobalMLBuildingFootprints and/or Overture Maps | check |
| Elevation | Copernicus GLO-30 DEM (fallback SRTM) | check |
| Land cover → ground materials | ESA WorldCover 10 m | CC BY 4.0 |
| Optional online "photo mode" | Google Photorealistic 3D Tiles via Cesium | live stream only, no caching; **ask me first** |

---

## Phases

### Phase −1 — Unity crash course (throwaway `sandbox/` project)
Goal: teach me Unity by building something tiny. Keep it out of the main project.

1. Create a new URP project in `sandbox/`. Walk me through the editor: Hierarchy, Scene and Game views, Inspector, Project window, Play mode, and the Console.
2. Build a ground plane and a box "car" with a Rigidbody. Explain GameObjects, Components, Transforms and the MonoBehaviour lifecycle using Flutter analogies.
3. Add a C# script to drive it with keyboard and on-screen touch controls using the Input System. Then add a follow camera with Cinemachine.
4. Turn the car into a prefab, and move the car's settings into a ScriptableObject. Explain both concepts.
5. Write one EditMode test and one PlayMode test, and run them from the CLI.
6. Build an APK and install it on my Android phone. Walk me through Player Settings, IL2CPP and ARM64.
7. Open the Profiler and show me how to read FPS, the CPU frame time breakdown and GC allocations.

**Done when:** the car runs on my phone, and I've made two changes myself (e.g. tuning speed, adding a jump) without your code.

### Phase 0 — Project skeleton & architecture guardrails
- Create the Unity project, set up URP mobile, and set IL2CPP/ARM64.
- Install the packages.
- Create the full folder and asmdef structure for `Core`, `App` and two sample features (`Settings`, `Input`).
- Set up VContainer root and feature scopes.
- Write the **architecture test**.
- Add the Makefile CLI loop.
- Set up CI with GitHub Actions + GameCI for tests and the Android build. **Ask me before adding secrets.**
- **Done when:**
  - a bootstrap scene builds into an Android APK through the CLI;
  - EditMode and PlayMode tests run from the CLI;
  - the architecture test fails when you deliberately add a forbidden reference (prove it, then revert).

### Phase 1 — GeoReference, streaming core, input, camera
- **`GeoReference` feature:**
  - Domain: `GeoCoord`, conversions;
  - Infrastructure: a Cesium adapter;
  - origin shift.
- **`WorldStreaming` feature:**
  - tile quadtree;
  - priority queue (distance, frustum, player speed);
  - cancellation;
  - memory budget;
  - LRU eviction;
  - pooling.
- **`Input` feature:** touch virtual sticks, gamepad and keyboard for editor testing.
- **`CameraRig` feature:** Cinemachine behind a port.
- Debug HUD showing fps, lat/lon, altitude, tile count and memory.
- **Done when:** a synthetic 400 × 400 km tile world streams smoothly at airliner speed, with no jitter at the far corner and memory within budget, on a real Android device.

### Phase 2 — Data pipeline (pilot: Greater Amman ~40 × 40 km)
- Download and clip OSM, DEM, WorldCover and footprints.
- Bake per-tile:
  - terrain;
  - land-cover map;
  - road graph (class, lanes, width, bridge/tunnel/layer, roundabouts);
  - buildings (height/levels, or an estimate);
  - POIs.
- Write a versioned schema, a manifest per LOD, and an idempotent `make tiles AREA=amman`.
- Add a validation report.
- **Done when:** the Amman bake finishes in under 30 min on a laptop, and the Unity side deserializes a tile in under 5 ms on a worker thread.

### Phase 3 — Terrain feature
- Stream terrain, either our own tiles or Cesium World Terrain during development (ask me first).
- Custom URP terrain shader:
  - blend by land cover and slope (limestone rock, sand, gravel desert, farmland, urban, scrub);
  - triplanar on cliffs;
  - macro variation so tiling isn't visible.
- Colliders at near LOD only.
- **Done when:** the Amman hills, Wadi Abdoun and the descent to the Dead Sea read correctly from the ground and from 3 km up.

### Phase 4 — Roads feature
- Runtime road meshes with Burst jobs, from the road graph:
  - per-class width and markings;
  - smooth curves;
  - proper junctions and **roundabouts** (all 8 circles);
  - bridges (Abdoun Bridge) and tunnels;
  - terrain conformation;
  - curbs and sidewalks.
- Expose a road graph for AI and navigation in Application.
- **Done when:** a scripted drive from the 1st to the 8th Circle runs on a gap-free surface.

### Phase 5 — Buildings feature (the "Amman look")
- Procedural buildings from footprints:
  - white and cream limestone facade shader, with window grids and balconies;
  - rooftop water tanks and satellite dishes;
  - shopfronts on commercial streets;
  - roof variants.
- LOD chain: detailed → simple → merged block → impostor.
- Per-tile mesh merging and instancing.
- Emissive windows at night.
- **`Landmarks` feature:** placeholder hand-built models for the Abdali towers, King Abdullah I Mosque, Roman Theatre, Citadel and Raghadan flagpole.
- **Done when:** it's recognisably Amman at street level and stays within the batch budget.

### Phase 6 — Player + Walk features
- **`Player` feature:** an `IPlayerMode` contract and a mode state machine (Walk ↔ Drive ↔ Fly), with enter/exit vehicle use cases.
- **`Walk` feature:**
  - CharacterController on slopes, stairs and step offset;
  - third- and first-person views;
  - CC0 character and animations (e.g. Quaternius), logged in ATTRIBUTION.
- **Done when:** you can walk downtown and up the Citadel without clipping or getting stuck, and mode-switch use cases are unit-tested.

### Phase 7 — Drive feature
- Vehicle physics: suspension, tires, engine torque curve, gears, handbrake.
- Mobile controls: tilt or touch steering.
- Chase and interior cameras.
- Three car archetypes defined as ScriptableObject configs (sedan, pickup, taxi).
- Collisions.
- Engine and tire audio.
- **Done when:**
  - Airport Road at 120 km/h is stable;
  - steep Amman streets are drivable;
  - the Dead Sea highway descent works;
  - nothing pops in within 300 m.

### Phase 8 — Fly feature
- Simplified aerodynamic model in Domain as pure C#, so it's unit-testable: lift and drag vs angle of attack, thrust, stall, control surfaces, flaps and gear.
- Arcade/realistic toggle.
- Runways from OSM aeroways at Queen Alia, Amman Civil (Marka) and King Hussein Aqaba.
- Far-field terrain LOD and haze up to 6 km altitude.
- **Done when:** a scripted flight takes off at Queen Alia, flies about 300 km to Aqaba without streaming hitches, and lands.

### Phase 9 — Environment & render quality
- **`Environment` feature:**
  - sky and atmosphere with time of day (check whether URP 6.x has a physically based sky; otherwise implement one);
  - cascaded shadows tuned per tier;
  - SSAO;
  - bloom;
  - ACES/Neutral tonemapping;
  - weather: khamaseen dust, haze, rain, rare snow.
- Water for the Dead Sea, the Gulf of Aqaba and reservoirs.
- Instanced vegetation: olive trees, palms in Aqaba and the Jordan Valley, scrub.
- Street lamps at night.
- **`Settings` feature:** low/medium/high/ultra tiers, auto-detected.
- **Done when:** before/after screenshots against Phase 5 show a clear upgrade and budgets still hold.

### Phase 10 — Scale out to all of Jordan
- Run the full-country bake.
- Make the regions look distinct: Irbid, Zarqa, Salt, Madaba, Karak, Ma'an, Aqaba, Jordan Valley, Wadi Rum, eastern desert.
- Add the Desert Highway, King's Highway and Dead Sea Highway.
- Delivery:
  - starter region (Amman) inside the app via Play Asset Delivery / iOS On-Demand Resources;
  - the rest streamed from a CDN (**ask me first**);
  - on-device cache with a size cap.
- **Done when:** you can drive Amman → Aqaba continuously on the Desert Highway, Petra and Wadi Rum render from the air, and the install and download sizes are documented.

### Phase 11 — HeroZones (Gaussian splats)
- Evaluate a maintained Unity Gaussian-splat renderer that works on mobile, and write an ADR.
- Splat zones load near the location and blend into the procedural world, with proxy colliders.
- Test with one capture I'll provide (e.g. the Roman Theatre).
- Write `docs/CAPTURE_GUIDE.md`.
- **Done when:** entering the zone shows no visible seam or hitch on a mid-range device.

### Phase 12 — Shipping polish
- **`Map` feature:** minimap and fast travel.
- **`Localization` feature:** Arabic by default, full RTL UI, English toggle.
- Thermal- and battery-aware quality.
- Save system.
- Crash reporting.
- Store-ready Android AAB and iOS build via the CLI.
- **Done when:** a signed AAB runs at 30 fps medium on a mid-range device. Record the device in PROGRESS.md.

### (Optional) Phase 13 — Game layer
Pick from missions, taxi/delivery jobs, discoverable places, flight challenges, photo mode, achievements and multiplayer presence. Each becomes a new feature module. Write a design doc first and wait for my approval.

---

Start **Phase −1** now. First summarise your understanding, then list any questions whose answers would change the architecture.