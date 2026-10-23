# Jordan World — session handoff

Last updated: **2026-10-23**. Written at the end of the first working
session so the next session can pick up with zero context lost.

## TL;DR

Phase −1 (Unity crash course in `sandbox/`) is **6 of 7 steps done**.
A **release APK is on the user's Android phone and runs**. Step 7
(Profiler tour) was interrupted mid-way: a Development APK was queued
via `unity command build run` but the user interrupted the polling
loop before I could confirm success and before I could walk through
the Profiler. Next session should finish step 7, then move to Phase 0.

## Where to begin, concretely

1. Read `jordan-world-claude-code-prompt.md` (the master brief, lives
   at repo root). It defines architecture, phases, and working rules.
2. Read `docs/PROGRESS.md` for the running work log.
3. Read `docs/LEARNING.md` sections 1–15 — the user is a Flutter dev
   new to Unity, and that file is their running teach-mode notebook.
   Keep the Flutter analogies going when you add new entries.
4. Confirm you have memories loaded:
   `[[user_profile]]`, `[[feedback_playmode_tint]]`,
   `[[feedback_prove_cli]]`, `[[project_jordan_world]]`.
   If the harness didn't load them, read
   `C:\Users\Pc\.claude\projects\C--Dev-jordan-world\memory\MEMORY.md`.

## Environment & tooling (verified live last session)

- **OS**: Windows 11 Pro, PowerShell + Git Bash both available.
  - **Git-Bash gotcha**: forward-slash args get path-mangled. Prefix
    every `unity command …` call with `MSYS_NO_PATHCONV=1`, or run
    from PowerShell. Already documented in `docs/LEARNING.md` §4.
- **Unity Editor**: 6000.3.27f1, Universal 3D template, open on the
  sandbox project at `C:\Dev\jordan-world\sandbox`. Live Pipeline
  server on port 7800 (confirm with `unity status`).
- **Unity CLI**: `C:\Users\Pc\AppData\Local\Unity\bin\unity.exe`,
  version **1.0.0-beta.13** (beta.14 is available — the CLI nagged a
  self-update notification). The user has not opted into the update
  yet; defer unless it blocks something.
- **Unity CLI skill** installed into Claude Code at
  `C:\Users\Pc\.claude\skills\unity-cli`. It's auto-loaded.
- **Android stack**: Build Support + SDK/NDK Tools + OpenJDK all
  installed in Unity Hub. `adb` lives at
  `C:\Program Files\Unity\Hub\Editor\6000.3.27f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe`.
- **Target device**: `VKP_NX9` (device id `AP4EVB6325005624`).
  Was connected and authorised last session. Will need to re-authorise
  the "Allow USB debugging" prompt if the phone was unplugged.
- **Active build target**: Android. Switching back/away triggers a
  full reimport (~1 min). Leave it on Android for now.

## Sandbox project state (`sandbox/`)

**Scene**: `Assets/Scenes/SampleScene.unity` (saved).
```
Main Camera      [+ CinemachineBrain]
Directional Light
Global Volume
Ground           plane, scale (10,1,10) → 100×100 m
Car              cube, prefab instance of Assets/Resources/Car.prefab
                 components: Transform, MeshFilter, BoxCollider,
                             MeshRenderer, Rigidbody, CarDriver
ChaseCam         [CinemachineCamera, CinemachineFollow (offset 0,3,-8),
                  CinemachineRotationComposer]
                 Target.TrackingTarget → /Car
```

**Scripts & assemblies**:
- `Assets/Scripts/Sandbox.Game.asmdef` — game code assembly,
  autoreferenced, refs `Unity.InputSystem`.
- `Assets/Scripts/CarDriver.cs` — reads WASD + touchscreen, applies
  AddForce + AddTorque in `FixedUpdate`, takes a `[SerializeField]
  CarConfig config`.
- `Assets/Scripts/CarConfig.cs` — `ScriptableObject`, `[CreateAssetMenu]`.

**Assets**:
- `Assets/Data/CarConfig.asset` — the live config asset.
- `Assets/Resources/Car.prefab` — moved out of `Assets/Prefabs/` so
  `Resources.Load<GameObject>("Car")` works at runtime. **Important**
  for the PlayMode test.

**Tests** (all green on last run):
- `Assets/Tests/EditMode/Sandbox.Tests.EditMode.asmdef`
  - `CarConfigTests.NewCarConfig_HasSensibleDefaults` ✓
  - `CarConfigTests.ProjectCarConfigAsset_LoadsAndIsSensible` ✓
- `Assets/Tests/PlayMode/Sandbox.Tests.PlayMode.asmdef`
  - `CarPhysicsPlayTests.CarPrefab_Instantiates_AndFallsOntoGround_UnderGravity` ✓

Run them (user's Editor is open, so use the live pipeline, not the
batch `unity test`):

```powershell
unity command test run --mode editor
unity command test run --mode playmode --async_tests true
unity command test status                                 # poll
```

**Builds**:
- `sandbox/Builds/Sandbox.apk` — release APK, 35.2 MB, IL2CPP/ARM64,
  installed on the phone and runs. Package id
  `app.jordanworld.sandbox`.
- `sandbox/Builds/Sandbox-Dev.apk` — **in progress at handoff**. A
  dev build (`Development + ConnectWithProfiler + AllowDebugging`
  options) was queued at ~20:22; last polled status at ~20:25 was
  `building`, with the `Sandbox_BurstDebugInformation_DoNotShip`
  folder actively updating. The APK file itself does **not** yet
  exist in `sandbox/Builds/`. The build may have finished (or failed)
  in the time since handoff. **Next session must re-check at
  kickoff** with `unity command build status`. If `completed`, the
  APK at `sandbox/Builds/Sandbox-Dev.apk` is ready to `adb install`.
  If still `building`, let it finish (dev + IL2CPP first-time can be
  slow). If `failed`, read errors — likely cause would be a bad
  BuildOption flag combo.

**PlayerSettings** (applied via `script eval_file configure_player.cs`):
- company "Jordan World", product "Sandbox", version 0.1.0
- bundle id `app.jordanworld.sandbox`
- scripting backend IL2CPP, API compat .NET Standard
- Android: ARM64 only, minSdk API25 (project default was 25, override
  to 24 was ignored), targetSdk Auto, bundleVersionCode 1

The script lives at `sandbox/configure_player.cs` — reuse it when
PlayerSettings need changes. Roslyn script-eval does **not** accept
`using` directives; use fully-qualified type names.

## Phase −1 progress

| Step | Done | Notes |
|------|------|-------|
| 1 — Editor walkthrough | ✓ | Hierarchy / Scene / Game / Inspector / Project / Console. |
| — Play mode red tint | ✓ (teaching) | User flagged as mandatory; now in `docs/LEARNING.md` §3 and in `[[feedback_playmode_tint]]`. |
| 2 — Ground + box car + Rigidbody | ✓ | Entirely CLI-driven. |
| 3 — Drive script + Input System + Cinemachine | ✓ | Cinemachine 3.1.7 installed. Keyboard + touch inputs. |
| 4 — Prefab + ScriptableObject | ✓ | Car.prefab + CarConfig.asset. |
| 5 — EditMode + PlayMode tests via CLI | ✓ | 3/3 pass. Introduced `.asmdef`s. |
| 6 — Android APK on-device | ✓ | 35.2 MB APK, installed, runs with chase cam + touch driving. |
| 7 — Profiler tour | **~20% — interrupted** | Dev-APK build queued, not confirmed, not installed, not profiled. |

Phase −1 **Done when** criteria from the brief:
> the car runs on my phone, and I've made two changes myself (e.g.
> tuning speed, adding a jump) without your code.

- The car **runs on the phone** — ✓.
- The two user-made changes — the user did the practice tasks after
  steps 2, 3, 4, 5, 6 (changing Rigidbody mass; editing CarDriver
  motorForce + maxSpeed; swapping to a TaxiConfig; adding an
  intentionally-failing test; rebuilding with higher Max Speed). All
  self-reported "go", none independently verified. **Ask the user to
  confirm two independent changes stuck** before closing Phase −1.

## Immediate next actions (session kickoff)

1. **Confirm Editor is still running + CLI is live**:
   ```powershell
   unity status
   unity command scene list_open --json
   ```
   Memory `[[feedback_prove_cli]]` requires showing the invocation,
   not just asserting connectivity.

2. **Check the stranded dev-APK build**:
   ```powershell
   unity command build status
   ```
   - If `completed` and `sandbox/Builds/Sandbox-Dev.apk` exists:
     `adb install -r sandbox\Builds\Sandbox-Dev.apk`, then launch:
     `adb shell monkey -p app.jordanworld.sandbox -c android.intent.category.LAUNCHER 1`.
     Then walk the user through connecting Unity's Profiler window:
     `Window → Analysis → Profiler → the dropdown at the top left
     should list the phone as "AndroidPlayer(ADB@...)"` → Record.
   - If `failed`: read the errors, re-queue if sensible.
   - If still `building` or `queued`: wait it out (incremental dev
     build should be ~1–2 min since the release was ~3.5 min).

3. **Finish step 7**: the walkthrough still needs to cover
   - CPU Usage module (frame time, hierarchy view, GC Alloc column).
   - Rendering module (SetPass calls, batches, draw calls — our Phase
     1 budget is <300 batches/frame).
   - Memory module (RAM: project budget 1.2 GB).
   - Where to find "zero per-frame GC in gameplay loops" evidence —
     the GC Alloc column in CPU Usage.
   - Deep Profile vs regular — enable only briefly, it skews numbers.
   - On-device vs in-Editor profiling — explain both; the device is
     the only one that matters for the mobile budget.
   - Add a `## 16.` section to `docs/LEARNING.md`.
   - Give a tiny practice task (e.g. "add
     `new List<int>()` to `Update()` on the Car, see the GC column
     light up, remove it").
   - Update `docs/PROGRESS.md`.

4. **Confirm Phase −1 close with the user**: ask them to name the two
   independent changes they made during the practice tasks. Then
   mark Phase −1 ✓ in `docs/PLAN.md`, commit, and stop for "go" on
   Phase 0.

5. **Phase 0 kickoff** when the user says go. Phase 0 is the first
   step of the **real** project (not `sandbox/`): create the Unity
   project under the repo root (not inside `sandbox/`), set up URP
   mobile with IL2CPP/ARM64, install the package stack (VContainer,
   UniTask, MessagePipe, R3, Unity Input System, Cinemachine,
   Addressables, Localization, Unity Test Framework), create the
   full `Assets/_Project/{App,Core,Features}` tree with Core and two
   sample features (Settings, Input), each with its own `.asmdef`
   per layer. Then the EditMode **architecture test**, the Makefile
   CLI loop, GitHub Actions + GameCI. **Ask the user before adding
   any CI secrets.** Prove the architecture test fails on a
   forbidden reference, then revert. Full criteria in the brief.

## Open questions still unanswered

Carried over from Phase −1 kickoff — none block Phase 0 but affect
later phases:

1. **Cesium ion** — free tier only, or willing to pay? Shapes
   Terrain feature's dev loop (Phase 3).
2. **Target mid-range Android** — specific device/SoC to target for
   the 30 fps medium tier?
3. **Monetization** — hard rule out NC/SA assets from day 1, or
   relax until Phase 10/12?
4. **Multiplayer presence** (optional Phase 13) — plausible yes
   → design Player/Input to be network-friendly early; no → ignore.
5. **Game design doc** — exists already, or design in Phase 13?

## Gotchas & lessons captured

Each has a dedicated section in `docs/LEARNING.md`:

- **§3** — Play mode is destructive. The user wants a loud red tint
  (`R 255 · G 70 · B 70 · A 230`). Set in Edit → Preferences →
  Colors → General → Playmode tint.
- **§4** — Git Bash on Windows mangles `/`-prefixed args. Use
  `MSYS_NO_PATHCONV=1` or PowerShell.
- **§8** — Physics forces in `FixedUpdate`, not `Update`. Camera
  follow in `LateUpdate`. Never set `transform.position` on a
  Rigidbody GameObject per-frame.
- **§13** — `Assembly-CSharp` auto-refs all asmdefs (one-way);
  asmdefs cannot ref `Assembly-CSharp` back. Scripts must be in
  their own asmdef before tests can reference them.
- **§14** — PlayMode tests over Pipeline must use `--async_tests
  true` + poll `test status`. The top-level `unity test` CLI
  refuses to run while the Editor is open.
- **§15** — `script eval` / `script eval_file` doesn't accept
  `using` directives. Fully-qualify everything.

## Pending project rules I haven't exercised yet

These matter for Phase 0 onward:

- **Force text serialization**, Git LFS, Unity `.gitignore` at the
  real project root. Sandbox doesn't have these yet because it's
  throwaway. Set them when scaffolding Phase 0.
- **ADR cadence**: one short ADR per architectural decision in
  `docs/DECISIONS.md`. Two seeded (0001 clean arch, 0002 Unity CLI).
  Phase 0 should add at least: physics choice (WheelCollider vs
  custom raycast), MessagePipe vs plain `IObservable`, URP Forward+
  vs Deferred.
- **Attribution.md** has no entries yet. Any CC0/attribution asset
  used from Phase 1 onwards must be logged with license + credit line.
- **Never cache Google Photorealistic 3D Tiles** — hard rule, flag
  if any team member wires it up accidentally.
- **Ask before anything that costs money** (Cesium paid tier, CDN,
  Google APIs), creates accounts, or downloads >2 GB.

## Files to NOT touch unless the user asks

- `jordan-world-claude-code-prompt.md` — the master brief, immutable
  except on user request.
- `sandbox/Assets/InputSystem_Actions.inputactions`,
  `sandbox/Assets/Readme.asset`, `sandbox/Assets/TutorialInfo/` —
  shipped by the Universal 3D template. Not used by our code; leave
  alone until Phase 0.
- `sandbox/configure_player.cs` — the script-eval source we re-run
  for PlayerSettings. Keep it rather than rewriting from scratch.
