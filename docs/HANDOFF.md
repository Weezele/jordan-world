# Jordan World — session handoff

Last updated: **2026-10-23 (end of session 1)**. Written so the next
session can pick up with zero context lost.

## TL;DR

**Phase −1 is complete and pushed to GitHub.** The next session's job
is to kick off **Phase 0** (real project skeleton + architecture
guardrails).

- Repo: <https://github.com/Weezele/jordan-world> (private)
- Branch: `main` (tracking `origin/main`)
- Commits:
  - `3968002` — chore: initial repo with Phase −1 sandbox and docs (107 files)
  - `41f5675` — chore: lift Unity .gitattributes to repo root
- Working tree is clean.
- Release + Dev APKs are installed on the user's phone and run.

## Where to begin, concretely

1. **Read these first, in order:**
   1. `Jordan-world-claude-code-prompt.md` (master brief at repo root)
   2. `docs/PLAN.md` (phase status table)
   3. `docs/PROGRESS.md` (running work log, newest first)
   4. `docs/LEARNING.md` (16 teach-mode entries for the user — a
      Flutter dev new to Unity; keep adding entries in that style)
   5. `docs/DECISIONS.md` (two ADRs so far; Phase 0 will add more)
   6. `docs/ARCHITECTURE.md` (feature-first Clean Architecture rules)
2. **Confirm memories loaded:** `[[user_profile]]`,
   `[[feedback_playmode_tint]]`, `[[feedback_prove_cli]]`,
   `[[project_jordan_world]]`. If the harness didn't surface them,
   read `C:\Users\Pc\.claude\projects\C--Dev-jordan-world\memory\MEMORY.md`.
3. **Prove the CLI is live** before claiming anything about the
   Editor (per `[[feedback_prove_cli]]`):
   ```powershell
   unity status
   unity command scene list_open --json
   ```

## Environment & tooling (verified live last session)

- **OS**: Windows 11 Pro. PowerShell + Git Bash both available.
  - **Git-Bash gotcha**: forward-slash args get path-mangled. Prefix
    every `unity command …` call with `MSYS_NO_PATHCONV=1`, or use
    PowerShell. Documented in `docs/LEARNING.md` §4.
- **Unity Editor**: 6000.3.27f1, Universal 3D template, open on
  `C:\Dev\jordan-world\sandbox`. Live Pipeline server on port 7800.
  Phase 0's real project will be a **second** Unity project at the
  repo root (not inside sandbox) — see §Phase 0 kickoff below.
- **Unity CLI**: `C:\Users\Pc\AppData\Local\Unity\bin\unity.exe` —
  1.0.0-beta.13. CLI nagged about beta.14; the user hasn't opted in.
  Defer unless something blocks.
- **Unity CLI skill** installed into Claude Code at
  `C:\Users\Pc\.claude\skills\unity-cli` (auto-loaded).
- **Android stack**: Build Support + SDK/NDK Tools + OpenJDK installed.
  `adb` at
  `C:\Program Files\Unity\Hub\Editor\6000.3.27f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe`.
- **Target device**: `VKP_NX9` (`AP4EVB6325005624`). Was connected +
  authorised last session; may need to re-authorise the "Allow USB
  debugging" prompt after a reconnect.
- **Active build target**: Android. Leave it; switching back costs
  a full reimport.

## Git / GitHub state

- Local git identity (scoped to this repo only): `Weezele` /
  `wesamnazzal8@outlook.com`.
- Remote `origin` → `https://github.com/Weezele/jordan-world.git`
  (private).
- `.gitignore` is Unity-aware + defensive about secrets and pipeline
  data. `.gitattributes` is the canonical Unity template with LFS +
  YAML-merge macros, lifted to the repo root in commit `41f5675`.
- No Git LFS objects pushed yet — the only binaries so far are three
  ~200 KB PNG screen captures. Phase 2+ will introduce real binary
  assets; set up `git lfs track` then.

## Sandbox project state (`sandbox/`) — frozen, done

**Scene** (`Assets/Scenes/SampleScene.unity`, saved):
```
Main Camera      [+ CinemachineBrain]
Directional Light
Global Volume
Ground           plane, scale (10,1,10) → 100×100 m
Car              cube prefab instance (Resources/Car.prefab)
                 Transform, MeshFilter, BoxCollider, MeshRenderer,
                 Rigidbody, CarDriver
ChaseCam         CinemachineCamera + CinemachineFollow (offset
                 0,3,-8) + CinemachineRotationComposer
                 Target.TrackingTarget → /Car
```

**Scripts & assemblies**:
- `Assets/Scripts/Sandbox.Game.asmdef` — autoreferenced, refs
  `Unity.InputSystem`.
- `Assets/Scripts/CarDriver.cs` — WASD + touchscreen, physics in
  `FixedUpdate`, `[SerializeField] CarConfig config`.
- `Assets/Scripts/CarConfig.cs` — ScriptableObject, `[CreateAssetMenu]`.

**Assets**:
- `Assets/Data/CarConfig.asset` — the live config.
- `Assets/Resources/Car.prefab` — moved out of `Assets/Prefabs/` so
  `Resources.Load<GameObject>("Car")` works at runtime. `Prefabs/`
  may still exist as an empty folder; harmless, user can delete.

**Tests** (3/3 green last run):
- `CarConfigTests.NewCarConfig_HasSensibleDefaults`
- `CarConfigTests.ProjectCarConfigAsset_LoadsAndIsSensible`
- `CarPhysicsPlayTests.CarPrefab_Instantiates_AndFallsOntoGround_UnderGravity`

Run them:
```powershell
unity command test run --mode editor
unity command test run --mode playmode --async_tests true
unity command test status
```

**Builds** (in `sandbox/Builds/`, gitignored):
- `Sandbox.apk` — 35 MB release, IL2CPP/ARM64. Installed on phone.
- `Sandbox-Dev.apk` — 90 MB development build with
  `ConnectWithProfiler + AllowDebugging`. Installed on phone. Use
  this one when teaching Profiler attach.

**PlayerSettings** (applied via `sandbox/configure_player.cs` ran
through `unity command script eval_file`):
- company "Jordan World", product "Sandbox", version 0.1.0
- bundle id `app.jordanworld.sandbox`
- scripting backend IL2CPP, API compat .NET Standard
- Android: ARM64 only, minSdk API25, targetSdk Auto,
  bundleVersionCode 1

Reuse `sandbox/configure_player.cs` as the pattern for Phase 0
PlayerSettings. Remember: Roslyn script-eval does **not** accept
`using` directives — fully-qualify everything.

## Phase status

| # | Phase | Status |
|---|-------|--------|
| −1 | Unity crash course (`sandbox/`) | ✅ |
| 0  | Project skeleton & architecture guardrails | ⏳ **next** |
| 1  | GeoReference, streaming core, input, camera | ⏳ |
| 2+ | … | ⏳ |

## Phase 0 kickoff — the next job

Full criteria in `Jordan-world-claude-code-prompt.md` under
**Phase 0**. Summary of what the next session needs to deliver:

1. **Create the real Unity project at the repo root** (not inside
   `sandbox/`). Open question for the user: do they want the real
   project to live at `C:\Dev\jordan-world\` itself, or at a
   subfolder like `game/` so docs/ and future `pipeline/` stay
   obvious at root? Ask before scaffolding.
2. **Set up URP mobile**, IL2CPP, ARM64 — reuse the pattern from
   `sandbox/configure_player.cs`.
3. **Install the package stack**: VContainer, UniTask, MessagePipe,
   R3, Unity Input System, Cinemachine (3.1.7+), Addressables,
   Localization, Unity Test Framework. Verify current versions
   against OpenUPM / Unity registry (don't trust memory per rule 2).
4. **Create the full folder + asmdef structure** per
   `docs/ARCHITECTURE.md`:
   ```
   Assets/_Project/
     App/                                 App.asmdef
     Core/
       Domain/        Core.Domain.asmdef           (noEngineReferences)
       Application/   Core.Application.asmdef
       Infrastructure/Core.Infrastructure.asmdef
       Presentation/  Core.Presentation.asmdef
     Features/
       Settings/    (all 7 asmdefs)
       Input/       (all 7 asmdefs)
   ```
5. **VContainer root** in a bootstrap scene with child scopes for
   Settings and Input.
6. **Architecture test** in `Core.Tests.EditMode` that reflects over
   loaded assemblies and fails on forbidden references. **Prove it
   fails** on a deliberately-broken reference, then revert.
7. **Makefile / scripts** at repo root with `make test`,
   `make build-android` (reusing patterns from
   `sandbox/configure_player.cs`).
8. **GitHub Actions + GameCI** for tests + Android build.
   **Ask the user before adding secrets** (Unity license, Android
   signing). CI shouldn't push to Weezele/jordan-world without the
   right credentials wired.

**Done when:**
- Bootstrap scene builds into an Android APK via the CLI.
- EditMode + PlayMode tests run via the CLI.
- Architecture test fails on a deliberate forbidden ref, then
  reverts to passing.

## Open architectural questions (still unanswered)

None block Phase 0, but these shape later phases — ask when they
feel relevant:

1. **Cesium ion** — free tier only, or willing to pay? Shapes
   Terrain feature (Phase 3) dev loop.
2. **Target mid-range Android device/SoC** for the 30 fps medium
   budget?
3. **Monetization** — hard-rule-out NC/SA assets from day 1, or
   relax until Phase 10/12?
4. **Multiplayer presence** (optional Phase 13) — plausible yes
   → design Player/Input to be network-friendly early; no → ignore.
5. **Game design doc** — exists already, or design in Phase 13?

## Gotchas & lessons captured (all in `docs/LEARNING.md`)

- §3 — Play mode is destructive. User wants a loud red tint
  (`R 255 · G 70 · B 70 · A 230`). I asked last session whether this
  was actually set; **unconfirmed**. Worth re-checking at kickoff.
- §4 — Git Bash on Windows mangles `/`-prefixed args. Use
  `MSYS_NO_PATHCONV=1` or PowerShell.
- §8 — Physics in `FixedUpdate`, camera in `LateUpdate`, never set
  `transform.position` per-frame on a Rigidbody GameObject.
- §13 — `Assembly-CSharp` auto-refs all asmdefs (one-way); asmdefs
  cannot ref `Assembly-CSharp` back. Code must be in its own asmdef
  before tests can reference it.
- §14 — PlayMode tests over Pipeline need `--async_tests true` +
  poll `test status`. Top-level `unity test` CLI refuses to run
  while the Editor is open.
- §15 — `script eval` / `eval_file` doesn't accept `using`
  directives. Fully-qualify.
- §16 — Profiler attaches to Android when the APK is built with
  `Development + ConnectWithProfiler`. Use the dev APK, not the
  release one.

## Pending project rules I haven't exercised yet

Matters for Phase 0:

- **Force text serialization** — sandbox already has it; verify on
  the Phase 0 project at creation.
- **Git LFS** — not needed for Phase 0 (no binary assets yet), set
  up before Phase 2 pipeline outputs.
- **ADR cadence** — Phase 0 should add ADRs for: physics choice
  (WheelCollider vs custom raycast — defer with "TBD in Phase 7" is
  fine), URP Forward+ vs Deferred, VContainer vs Zenject (we picked
  VContainer in the brief; just record why: smaller, faster, modern).
- **`docs/ATTRIBUTION.md`** has no entries yet; populate when any
  asset with attribution lands.
- **Never cache Google Photorealistic 3D Tiles** — hard rule, flag
  if anyone wires it up.
- **Ask before anything that costs money / needs an account /
  downloads >2 GB.**

## Files to NOT touch unless the user asks

- `Jordan-world-claude-code-prompt.md` — the master brief. Only
  modify on explicit user request.
- `sandbox/Assets/InputSystem_Actions.inputactions`,
  `sandbox/Assets/Readme.asset`, `sandbox/Assets/TutorialInfo/` —
  Universal 3D template defaults. Unused by our code; harmless.
- `sandbox/configure_player.cs` — the script-eval source we reused
  for PlayerSettings. Keep it as a pattern reference, don't delete.

## UI housekeeping flagged by the user screenshot (end of session)

- **Right panel is "Background Tasks", not Inspector.** User should
  do `Window → General → Inspector` (`Ctrl+3`) and dock it. Phase 0
  teaching will need the Inspector visible for the architecture-test
  demo.
- **Empty `Assets/Prefabs/` folder** in the sandbox (Car.prefab was
  moved to Resources). Harmless; delete if it bothers them.
