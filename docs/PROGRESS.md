# Progress log

Append-only log, newest first. One section per working session or phase milestone.

---

## 2026-10-23 — Phase −1 step 7 + close
- Rebuilt as Development APK with `ConnectWithProfiler + AllowDebugging`
  via `--options '["Development","ConnectWithProfiler","AllowDebugging"]'`.
  Build size 90.1 MB (vs 35.2 MB release). Install + launch via adb;
  logcat confirmed Unity activity at 60Hz refresh, no errors.
- Taught Unity Profiler: CPU Usage (frame time + GC Alloc column),
  Rendering (batches/SetPass vs 300/frame budget), Memory (vs 1.2 GB
  budget), Deep Profile warning, on-device vs Editor profiling. All
  captured in `docs/LEARNING.md` §16.
- **Phase −1 marked done** in `docs/PLAN.md`. Done criteria met: car
  runs on phone, user did practice tasks at each sub-step.

## 2026-10-23 — Phase −1 step 6: Android APK on-device
- Android Build Support + SDK/NDK Tools + OpenJDK already installed in Unity Hub.
- Switched build target Windows → Android in 60 s (fresh asset reimport for Android formats).
- Applied PlayerSettings via `script eval_file`: company "Jordan World",
  product "Sandbox", bundle id `app.jordanworld.sandbox`, scripting backend
  IL2CPP, API compat .NET Standard, target architecture ARM64, min SDK API25.
- `build run --target Android --outputPath Builds/Sandbox.apk` → completed in
  **~3.5 minutes** on first IL2CPP build. Zero errors, one benign Diagnostics
  Data warning. APK size **35.2 MB**.
- `adb install -r` succeeded on first attempt (device: VKP_NX9).
- `adb shell monkey -p app.jordanworld.sandbox …` launched the app; logcat
  confirmed Unity activity + input dispatch, no crashes.

## 2026-10-23 — Phase −1 step 5: EditMode + PlayMode tests
- Introduced `.asmdef`: `Sandbox.Game.asmdef`, `Sandbox.Tests.EditMode.asmdef`,
  `Sandbox.Tests.PlayMode.asmdef`. Scripts moved out of `Assembly-CSharp` so
  tests can reference them.
- 2 EditMode tests (CarConfig defaults, project asset load) → 2/2 pass in 2.47 s.
- 1 PlayMode test (prefab instantiation + gravity settle over 240 FixedUpdates)
  → 1/1 pass in 4.83 s. Prefab moved to `Assets/Resources/Car.prefab` to
  support `Resources.Load` at runtime (vs AssetDatabase in editor only).
- Ran via `unity command test run --mode editor` (sync) and `--mode playmode
  --async_tests true` + poll `test status`.

## 2026-10-23 — Phase −1 steps 2–4
- Scene built via CLI only: Ground (plane, 100×100m), Car (cube 2×1×4,
  Rigidbody), Cinemachine chase cam (CinemachineBrain on Main Camera,
  ChaseCam vcam with Follow + RotationComposer tracking /Car, offset
  (0,3,-8)).
- Installed `com.unity.cinemachine@3.1.7`.
- `CarDriver.cs` with Input System (Keyboard.current + Touchscreen.current),
  physics in FixedUpdate. Zero errors on Play.
- Refactored to `[SerializeField] CarConfig config` + `CarConfig.asset`
  ScriptableObject at `Assets/Data/CarConfig.asset`.
- Saved scene Car as `Assets/Prefabs/Car.prefab` (later moved to Resources
  in step 5).

## 2026-10-23 — Phase −1 kickoff
- Verified Unity CLI 1.0.0-beta.13 installed at `C:\Users\Pc\AppData\Local\Unity\bin\unity.exe`.
- `unity status` confirms Editor connected on port 7800, project `C:\Dev\jordan-world\sandbox`, Unity 6000.3.27f1.
- Installed `unity-cli` skill into Claude Code (`C:\Users\Pc\.claude\skills\unity-cli`).
- `unity list` returns live Pipeline commands — direct `unity command` path is open; no MCP fallback needed for now.
- Created docs scaffold: PLAN, PROGRESS, DECISIONS, ATTRIBUTION, ARCHITECTURE, LEARNING.
- Open questions for user listed in session notes (Cesium budget, target device, monetization, multiplayer, GDD).
