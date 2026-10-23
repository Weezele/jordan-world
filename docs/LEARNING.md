# Learning log — Unity for a Flutter dev

Each entry = one Unity concept, with a Flutter analogy where one fits, and
the gotchas. Terse by design.

---

## 0. The Unity Editor is your IDE **and** your scene builder

In Flutter, your IDE edits code and the running app is a separate process.
In Unity, the **Editor** is both: it edits code (via your external editor
like Rider/VS) *and* authors scenes, prefabs, materials, settings.
Play mode runs the game *inside* the Editor.

A `.unity` scene file is to Unity what `main.dart` + a tree of widgets is to
Flutter — except scenes are **serialized object graphs**, not code.

## 1. The six core Editor windows

| Window | What it is | Flutter analogy |
|---|---|---|
| **Hierarchy** | List of GameObjects in the open scene | the widget tree, but authored visually |
| **Scene view** | 3D viewport you fly around to compose the world | — (no close analog) |
| **Game view** | What the player sees through the active Camera | the running app simulator |
| **Inspector** | Edits the selected GameObject's components | the "properties" pane of an IDE, but live |
| **Project** | Everything under `Assets/` (files on disk) | your `lib/` + `assets/` folders |
| **Console** | Log / warning / error output | the Debug console |

Press **Ctrl+1 … Ctrl+5** to focus each window if the layout is cramped.

## 2. GameObject vs Component

Every "thing" in a scene is a **GameObject** — just a named container with a
`Transform` (position/rotation/scale). Behaviour is added as **Components**
(scripts or built-ins like `Rigidbody`, `MeshRenderer`).

Flutter analog: a GameObject ≈ a `Widget` that owns a `State`, and
Components ≈ the fields on that `State` that make the widget *do things*.
In Unity, you compose by adding components rather than by wrapping widgets.

**Gotcha**: a GameObject with no components other than `Transform` is a
valid empty parent/anchor. Perfectly normal.

## 3. Play mode is destructive

When you hit **Play** ▶ and change values in the Inspector, they revert the
moment you stop. There is a tint around the Editor when Play mode is on —
this is your reminder.

Flutter analog: like changing a `const` value in code during hot reload —
except in Unity it looks like a save, and it's not. Watch the tint.

**Mandatory setup**: the default tint is almost invisible. Make it
unmissable:
> **Edit → Preferences → Colors → General → Playmode tint**
> Pick a saturated red, suggested `R 255 · G 70 · B 70 · A 230`.

Lost-edit incidents from forgetting Play mode are the #1 beginner mistake.
Fix it before you ever hit ▶ a second time.

## 4. Proving the CLI is live

Any time I claim the CLI is "connected", there should be a visible
`unity command …` invocation whose JSON came back `"success": true`.
Trust the output, not the claim. Working CLI reads scene state in
~100 ms; if you ever want to check it yourself:

```powershell
unity status                                  # editor connected?
unity command scene list_open --json           # which scene is open?
unity command scene get_hierarchy --json       # what's in it?
```

**Git-Bash gotcha**: Git Bash on Windows auto-translates any arg starting
with `/` into a Windows path (so `--target /Car` becomes `C:/Program
Files/Git/Car` and the Pipeline server can't find it). Prefix the command
with `MSYS_NO_PATHCONV=1` or use PowerShell. PowerShell has no such
translation.

---

## 5. GameObject + Component — composition over inheritance

The sandbox now contains:

| GameObject | Components attached | Notes |
|---|---|---|
| `Ground` | Transform + MeshFilter + MeshCollider + MeshRenderer | floor plane |
| `Car` | Transform + MeshFilter + BoxCollider + MeshRenderer + **Rigidbody** | cube with physics |

The pattern is **always** the same: a GameObject is a named node in the
scene with a Transform; everything else — mesh, collider, physics,
scripts — is just a **Component** added to it.

Flutter mapping:
- `GameObject` ≈ a `StatefulWidget` instance in the tree.
- `Component` ≈ one field on that widget's `State` that *does* something.
- Adding a `Rigidbody` is like adding `physics: PhysicsBehavior()` to a
  State class — the behaviour is now live without touching anything else.
- There is **no "extends" ladder** of GameObject types. If you want a
  car, you compose: Cube mesh + BoxCollider + Rigidbody + your `CarInput`
  script. There is no `class SportsCar extends Car extends Vehicle`.

**Why Unity does it this way**: deep inheritance trees ossify. Composition
lets you swap behaviours (one physics component → another) without
touching unrelated code. Clean Architecture's own preference for
composition over inheritance maps 1:1 onto this.

## 6. Transform — the one mandatory component

Every GameObject has exactly one `Transform` and you can't remove it. It
stores `position`, `rotation (Quaternion)`, `scale` — all in **local
space** relative to the parent (or world space if the GameObject is a
scene root).

- World-space position/rotation are read via `transform.position` /
  `transform.rotation`.
- Local-space via `transform.localPosition` / `transform.localRotation`.
- Scaling a child inherits the parent's scale as a multiplier
  (non-uniform scaling has gotchas with physics; keep rigidbodies at
  scale 1 or compensate in collider geometry).

Flutter mapping: Transform ≈ a `Transform.translate` / `.scale` /
`.rotate` widget merged with the Positioned data of its parent. Unlike
Flutter, position is a `Vector3`, not a 2D offset.

## 7. Rigidbody — "this GameObject obeys physics"

Adding a `Rigidbody` to `Car` tells PhysX: *this body has mass,
accumulates forces, and reacts to collisions*. Press **▶** now and the
cube falls, hits the ground plane (which has a `MeshCollider`), and
bounces/settles.

Mandatory partnerships:
- Physics motion → **Rigidbody** on the moving body.
- To collide → both bodies need **Colliders** (BoxCollider, MeshCollider, …).
- Rigidbody drives Transform. **Never set `transform.position = …` on a
  Rigidbody-owning GameObject every frame** — use `Rigidbody.MovePosition`
  or AddForce. Direct Transform writes bypass PhysX and cause the classic
  "jitter + wrong collisions" bug.

## 8. MonoBehaviour lifecycle — Flutter widget lifecycle, but bigger

A `MonoBehaviour` is any C# script you attach to a GameObject as a
Component. Its lifecycle methods are called by Unity in a fixed order:

| Method | When | Flutter analog |
|---|---|---|
| `Awake()` | Once, when the GameObject is first loaded | `initState()` — but may fire while inactive |
| `OnEnable()` | Every time the component becomes enabled | `initState()`+ (also on re-mount) |
| `Start()` | Once, just before the first frame it's active | `didChangeDependencies()` the first time |
| `Update()` | Every rendered frame | `WidgetsBinding.instance.addPostFrameCallback` × ∞ |
| `FixedUpdate()` | Every physics step (fixed Δt, default 50 Hz) | — (no Flutter analog) |
| `LateUpdate()` | After all `Update()`s this frame | `addPostFrameCallback` once |
| `OnDisable()` / `OnDestroy()` | On disable / destruction | `dispose()` |

Golden rules:
- **Physics forces** go in `FixedUpdate`, **not** `Update`. Camera follow
  goes in `LateUpdate`.
- **Zero per-frame allocations in `Update`** on Jordan World — strings,
  LINQ, `new List<…>()` are forbidden in gameplay hot paths.
- You never call these yourself. Unity does, via the component.

---

## 9. Input System — the new, action-first input stack

Unity has two input stacks: the old `Input.GetKey("w")` **Input Manager**
(deprecated) and the new **Input System** (`com.unity.inputsystem`,
`using UnityEngine.InputSystem`). We use the new one everywhere.

Two usage styles:
1. **Direct device reads** (what `CarDriver.cs` does):
   ```csharp
   var kb = Keyboard.current;
   if (kb != null && kb.wKey.isPressed) throttle += 1f;
   ```
   Simple, test-friendly, works for sandbox code.
2. **InputActions assets** (`.inputactions` file): declare named actions
   like `Move` (Vector2), `Jump` (Button), bind them to multiple devices
   at once. Use `PlayerInput` or auto-generated C# wrappers to subscribe.
   This is what Phase 1's `Input` feature will use.

Device singletons we rely on:
- `Keyboard.current` — null in builds without a keyboard, so always
  null-check.
- `Touchscreen.current` — same deal; use `touches` for finger streams.
- `Mouse.current` / `Gamepad.current` — same pattern.

**Why action-first**: binding "forward" to `W` *and* right-trigger *and*
touch-stick-up in one action keeps logic device-agnostic. On mobile we'll
bind touch; on an attached controller, gamepad; in the Editor, keyboard
— no `if (platform == Mobile)` branches in gameplay.

## 10. Cinemachine — procedural cameras you don't hand-code

A gameplay camera that follows the player, anticipates corners, respects
collision, and blends between shots is a *lot* of code. Cinemachine
("CM") is Unity's "camera brain + director" package that handles that
procedurally.

CM 3.x architecture (the version we installed):

| Component | On | Role |
|---|---|---|
| `CinemachineBrain` | **Main Camera** | Reads active vcam, drives the real camera |
| `CinemachineCamera` | each vcam GameObject | A "virtual camera" the Brain can switch to |
| `CinemachineFollow` | vcam | Procedural body: track `Target.TrackingTarget` with `FollowOffset` |
| `CinemachineRotationComposer` | vcam | Procedural aim: point at `Target.LookAtTarget` with lead/damping |

In our sandbox the active vcam is `/ChaseCam`:
```
Target.TrackingTarget → /Car   (follow target, same used for LookAt by default)
FollowOffset         → (0, 3, −8)  // 3 m up, 8 m behind
```

In Play, the Brain reads ChaseCam each frame and overrides Main Camera's
Transform. That's why the Edit-mode `Main Camera` capture looked wrong
(static) and the Play-mode capture framed the car.

Flutter analogy: Cinemachine ≈ a `NavigatorObserver` that automatically
drives your `ScrollController` to keep a widget on-screen. You declare
"what to care about" (targets, offsets, dead zones) and it animates
smoothly without per-frame code.

**Priority**: when two vcams are active, the one with higher `Priority`
wins. For scene switches, you don't move cameras — you flip priorities
and CM blends between them.

---

## 11. ScriptableObject — "const config class on disk"

A `ScriptableObject` is a `UnityEngine.Object` that **isn't attached to
a GameObject**. It lives in `Assets/` as a `.asset` file and can be
referenced by any MonoBehaviour's serialized field.

Our sandbox now has `Assets/Data/CarConfig.asset`:
```csharp
[CreateAssetMenu(menuName = "Jordan World/Sandbox/Car Config", fileName = "CarConfig")]
public class CarConfig : ScriptableObject {
    public float motorForce = 1500f;
    public float steerTorque = 400f;
    public float maxSpeed = 25f;
    …
}
```

`CarDriver` holds a `[SerializeField] CarConfig config;` field. The Car
prefab stores a *reference* to the asset — not a copy of the values.
Edit the asset and every Car in every scene picks up the change.

**Flutter mapping**:
- `ScriptableObject` ≈ a `const class` of config values, except editable
  in the Inspector by designers who don't open the IDE.
- `[SerializeField]` ≈ a `@visibleForTesting` + `final` field that the
  Inspector exposes.
- `[CreateAssetMenu]` ≈ a factory + `Create > Asset Menu` entry so you
  can right-click in the Project panel → `Create > Jordan World >
  Sandbox > Car Config` → new `.asset`.

**Why we use them in Jordan World**:
- **Three car archetypes** (Phase 7) will be three `CarConfig` assets
  (sedan, pickup, taxi) — not three classes.
- **Quality tiers** (`Settings` feature) are ScriptableObjects.
- Designers tune values; engineers don't rebuild.
- Each `.asset` is a tiny YAML file in git — diffs read naturally.

**Gotcha**: ScriptableObjects are **shared singletons**. If you mutate
`config.maxSpeed` at runtime, every GameObject referencing that asset
sees the change — and the change *can* persist to disk in the Editor,
but **not** in a built player. Treat them as read-only in gameplay code,
mutate only in editor tools.

## 12. Prefab — the reusable blueprint of a GameObject

A **Prefab** is a saved template: a GameObject tree with its components
and their serialized values, stored at `Assets/Prefabs/<Name>.prefab`.
Dragging the asset into a scene creates a **prefab instance** — any
later edit to the prefab asset propagates to every instance in every
scene.

Our sandbox has `Assets/Prefabs/Car.prefab`. The `/Car` object in the
scene is now a *connected instance* — notice the little blue cube icon
beside its name in the Hierarchy.

**Prefab mechanics**:
- Edit prefab in-place → double-click the asset → opens an isolated
  prefab stage. Changes apply to all instances when you exit the stage.
- **Overrides**: tweak an instance in the scene (e.g., move the Car to
  `(10,2,0)`) → the override is highlighted in bold in the Inspector
  and can be reverted or applied back to the prefab.
- **Prefab variants** (`prefab create_variant`): a prefab that inherits
  from a base and overrides specific fields — our Phase 7 "taxi" could
  be a variant of the base `Car` prefab plus a yellow paint override.

**Flutter mapping**:
- Prefab ≈ a `Widget` *constructor* + factory, stored as a reusable asset.
- Prefab instance ≈ an instance of that widget in the tree.
- Prefab variant ≈ a subclass widget that overrides a few constructor
  defaults.
- Nested prefabs ≈ widget composition — a prefab can contain other
  prefab instances.

**Prefab + ScriptableObject = the designer-friendly combo**:
- The *prefab* says "a Car is a cube + Rigidbody + BoxCollider + CarDriver."
- The *ScriptableObject* says "this specific Car's torque is 400."
- Swap the ScriptableObject reference → one line, three different cars.
- This is **exactly** what Phase 7's `Drive` feature will do.

**Gotcha**: a prefab stores *values and component references*, not the
`CarDriver` script itself — the script lives in `Assets/Scripts/`. Rename
the C# class without using Unity's rename-safe refactor (or without the
`GUID.cs.meta` file) and every prefab instance becomes an orphaned
"Missing (Mono Script)" entry. Always rename via Unity (right-click →
Rename in Project window) or Rider's Unity-aware refactor.

---

## 13. Assembly definitions (`.asmdef`) — the compilation unit

By default, every `.cs` under `Assets/` compiles into one giant assembly
called **`Assembly-CSharp`**. That is fine for a tiny sandbox but
disastrous for a real project: every one-line change triggers a full
re-link of every script.

An **`.asmdef`** (Assembly Definition) carves out a sub-folder as its
own assembly. We just did this twice in the sandbox:
```
Assets/Scripts/Sandbox.Game.asmdef            # the game code
Assets/Tests/EditMode/Sandbox.Tests.EditMode.asmdef
Assets/Tests/PlayMode/Sandbox.Tests.PlayMode.asmdef
```

Benefits:
- **Fast incremental compiles**: changing a line in one asmdef recompiles
  only that assembly.
- **Explicit dependencies**: a `references` list declares what other
  assemblies this one can see. Everything else is invisible — a compile
  error, not a lint rule.
- **Compile-time Clean Architecture**: this is the mechanism Jordan
  World uses to enforce "Domain can't reach into Infrastructure." It's
  not a convention; it's the compiler saying no.

**One-way reference rule** (surprised me in step 5): `Assembly-CSharp`
auto-references *every* asmdef so your legacy scripts can see new code,
but asmdefs **cannot** reference `Assembly-CSharp` back. So the moment
you want tests to see your scripts, those scripts must live in their
own asmdef — exactly why `Sandbox.Game.asmdef` had to exist before the
tests could compile.

**Phase 0 preview**: the real project will have ~90 asmdefs — one per
(feature, layer). An EditMode "architecture test" will reflect over
loaded assemblies and fail the build if any asmdef dependency violates
the Clean Architecture rules.

## 14. Unity Test Framework — EditMode vs PlayMode

Two kinds of tests, two very different runtimes:

| | **EditMode** | **PlayMode** |
|---|---|---|
| Runs in | Editor, synchronously | Editor's Play mode (full engine loop) |
| Speed | Milliseconds | Seconds (domain reload + Play-mode entry) |
| Can touch | `UnityEditor.*`, scene files, asset database | `GameObject`, physics, coroutines, Input, audio |
| Attribute | `[Test]` (synchronous) | `[UnityTest]` returning `IEnumerator` |
| Use for | Pure logic, data validation, editor tools | Scene behavior, physics, streaming, async flows |

Our sandbox has one of each:
- EditMode `CarConfigTests.NewCarConfig_HasSensibleDefaults` → create a
  `ScriptableObject` in memory, assert fields are sane. ~7 ms.
- PlayMode `CarPhysicsPlayTests.CarPrefab_Instantiates_AndFallsOntoGround…`
  → instantiate the prefab 5 m up, step `WaitForFixedUpdate` 240 times
  (~4 s of simulated physics at the default 50 Hz), assert y dropped
  under 2 m. ~4.8 s.

**How Unity classifies an asmdef as EditMode vs PlayMode**:
- References `UnityEditor.TestRunner` → **EditMode**.
- Compiles only for Editor (`includePlatforms: ["Editor"]`) → **EditMode**
  even without the `UnityEditor.TestRunner` reference.
- Otherwise (references `UnityEngine.TestRunner`, platform-agnostic) → **PlayMode**.

This bit me in step 5: my PlayMode asmdef was Editor-only (because the
test used `UnityEditor.AssetDatabase`), so Unity misclassified it as
EditMode. Fix: load the prefab via `Resources.Load<T>("Name")` instead,
which works at runtime, and drop the Editor-only constraint.

**Running tests from the Unity CLI**:
```powershell
# EditMode (synchronous, fast)
unity command test run --mode editor

# PlayMode (async — enters Play mode which triggers a domain reload)
unity command test run --mode playmode --async_tests true
unity command test status                      # poll until status=completed
```

**`unity test`** (the top-level CLI command) spawns a **separate**
batch-mode Editor, so it refuses to run while your Editor is already
open on the project. `unity command test run` uses the live Editor
through Pipeline — use this when you're developing. Use top-level
`unity test` in CI.

---

## 15. Build targets, PlayerSettings, IL2CPP, ARM64

Shipping to Android changes four things at once:

| Thing | What it is | Why we picked ours |
|---|---|---|
| **Build target** | The platform the engine compiles for (Windows / Android / iOS / WebGL …). Switching reimports every asset for that target's texture formats, audio compression, shader variants. Slow (minutes). | Android for mobile. iOS parity in Phase 12. |
| **Scripting backend** | How C# is executed on the device. **Mono** JITs IL at runtime. **IL2CPP** transpiles IL to C++, then compiles to a native binary. | **IL2CPP** — mandatory on iOS, required by Cesium, 2–4× faster than Mono. Also lets Unity link against native code (Cesium's C++ core). |
| **API compatibility** | Which .NET BCL surface is available. `.NET Standard 2.1` (small, portable) vs `.NET Framework` (full, bigger). | **.NET Standard 2.1** — enough for everything we do, keeps binary small. |
| **Target architecture** | Which CPU instruction sets to compile for. `ARMv7`, `ARM64`, `x86_64`. | **ARM64 only** — Cesium requires it. ARMv7 is dead. x86_64 only for emulators (we skip; use scrcpy). |

**Min SDK vs Target SDK** (Android specifics):
- **minSdkVersion** = lowest Android API level your app will install on.
  API 24 (Nougat, 2016) is a reasonable Jordan World floor. Our sandbox
  landed on API 25.
- **targetSdkVersion** = the API level you built against (dictates
  runtime behavior like scoped storage, permission model). Google Play
  bumps the required value yearly; we use `Auto` = latest-known.

**Our sandbox PlayerSettings** (applied via `script eval_file`):
```csharp
PlayerSettings.companyName = "Jordan World";
PlayerSettings.productName = "Sandbox";
PlayerSettings.SetApplicationIdentifier(Android, "app.jordanworld.sandbox");
PlayerSettings.SetScriptingBackend(Android, IL2CPP);
PlayerSettings.SetApiCompatibilityLevel(Android, NET_Standard);
PlayerSettings.Android.targetArchitectures = ARM64;
PlayerSettings.Android.minSdkVersion       = AndroidApiLevel24;
PlayerSettings.Android.targetSdkVersion    = AndroidApiLevelAuto;
```

The bundle id (`app.jordanworld.sandbox`) is the Android equivalent of
a Flutter `applicationId` and iOS `CFBundleIdentifier`. Once published,
you can't change it — Google Play treats a changed id as a new app.

**The build command we used**:
```powershell
unity command build.target switch --target Android --confirm true
unity command build run --target Android --outputPath Builds/Sandbox.apk --confirm true
unity command build status                                              # poll until completed
```
First build: ~3.5 minutes (IL2CPP transpiles everything to C++ and
native-compiles via NDK). Rebuilds are much faster — IL2CPP caches.

**adb** = Android Debug Bridge. Unity bundles it inside the SDK module
at `…/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb.exe`.
```powershell
adb devices -l                                 # verify phone is attached + authorised
adb install -r Builds/Sandbox.apk              # install (replace if present)
adb shell monkey -p app.jordanworld.sandbox -c android.intent.category.LAUNCHER 1
                                               # launch the app by package id
adb logcat -d -s Unity:V -t 100                # last 100 Unity log lines
```

Flutter analog: `adb` ≈ `flutter devices` + `flutter install`. IL2CPP ≈
Dart's AOT compile (`flutter build apk`). Scripting backend choice ≈
JIT (debug) vs AOT (release) — but in Unity the choice is per-platform
and persistent.

**Gotchas we hit**:
- `unity test` (top-level) and `unity command build run` can't both
  drive the same Editor at once — the former spawns a fresh batch
  Editor, the latter uses yours. Use `unity command test run` + `build
  run` in the live Editor.
- First time connecting a phone: **enable USB debugging + authorise the
  PC fingerprint on the phone's own screen** or `adb devices` shows
  nothing. We hit this.
- Git Bash on Windows mangles arg values starting with `/`. Prefix
  commands with `MSYS_NO_PATHCONV=1`, or use PowerShell.

---

## 16. Unity Profiler — the only loop that matters

Unity ships a Profiler window (`Window → Analysis → Profiler`, or
`Ctrl+7`) that samples the running engine every frame and shows:

| Module | What | Jordan World budget |
|---|---|---|
| **CPU Usage** | ms per frame, split by Scripts/Physics/Rendering/GC, with a hierarchy drill-down and per-call GC Alloc | 30 fps mid-range Android (33.3 ms), 60 fps flagship (16.6 ms) |
| **Rendering** | Batches, SetPass calls, Draw calls, Tris/Verts | **< 300 batches / frame** |
| **Memory** | Total Reserved, Textures, Meshes, Audio, Other | **< 1.2 GB RAM** |
| **GPU** | GPU ms, needs device + driver support | per-frame GPU time stays under the CPU budget |

**Editor Play mode vs on-device**: profiling in the Editor is useful
for authoring loops but **the only numbers that matter for mobile
budgets are on-device**. The Editor runs on a 32-core desktop GPU
with 32 GB RAM — it will lie to you.

### Attaching the Profiler to an Android device

Two things must be true:
1. The APK was built with **Development Build** + **ConnectWithProfiler**.
   Via CLI:
   ```powershell
   unity command build run --target Android --outputPath Builds/Dev.apk ^
     --options '["Development","ConnectWithProfiler","AllowDebugging"]' --confirm true
   ```
2. The phone is reachable over `adb` (`adb devices -l` lists it).

Then: open the Profiler → top-left connection dropdown → pick
`AndroidPlayer(ADB@app.jordanworld.sandbox)` → click Record (red circle).

### The GC Alloc column — our hard line

Project rule 10 is non-negotiable: **zero per-frame GC allocations
in gameplay loops**. Why: every allocation enters the Boehm GC's
tenure cycle, which eventually causes a 10-30 ms pause mid-frame on
mobile. One hitched frame per minute is enough to break the "feels
smooth" illusion.

How to catch allocations:
- Record a Profiler trace while gameplay runs.
- Freeze on any frame (click the graph).
- **Hierarchy** tab → sort by **GC Alloc** descending.
- Anything in your gameplay stack with a non-zero number is a bug.

Common sources that we ban in `Update`/`FixedUpdate`:
- `new List<T>()`, `new T[n]`, `new StringBuilder()`
- LINQ (`.Where`, `.Select`, `.OrderBy` all allocate)
- `string` concatenation / interpolation (allocates)
- `Debug.Log(…)` with an interpolated string
- `GameObject.Find`, `FindObjectsOfType` (allocates arrays)
- `foreach` over some collection types in old Mono (fixed in current
  Unity, but still a tell-tale)

Pre-allocate once in `Awake`, reuse the buffer forever.

### Deep Profile — occasional use only

Profiler toolbar → **Deep Profile** button. Instruments *every*
method, not just Unity's built-in sample points. The trace becomes
exhaustive but the frame times double. Enable it to find the one
mystery method eating time, then **turn it off** before trusting
the numbers again.

### Three warm-up habits

1. **Profile early, not just before shipping.** The sooner a bad
   per-frame allocation shows up, the easier it is to fix.
2. **Always profile a Development Build on the target device.** The
   Editor's "15 ms Scripts!" claim is meaningless; a mid-tier Android
   SoC might take 60 ms for the same work.
3. **Record short bursts (5-10 s)**, drop them into a file (`File →
   Save…` saves a `.data` capture). Share captures in PR descriptions
   when discussing a perf regression.

Flutter analogy: Unity Profiler ≈ DevTools' Performance view, but
with much finer sampling (every frame, every call tree, every
allocation site) and a hard requirement to run against the real
device because the host is 100× faster than the target.
