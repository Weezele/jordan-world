# Architecture Decision Records

Short ADRs. Format: context → decision → consequences. Newest first.

---

## ADR-0001 — Clean Architecture, feature-first, one asmdef per (feature, layer)
**Date**: 2026-10-23
**Status**: Accepted (dictated by project brief).

**Context**: Jordan World is a large Unity project with many orthogonal
subsystems (georeference, streaming, roads, buildings, player modes, …) that
must stay independently testable and refactorable.

**Decision**: Every feature splits into Domain / Application / Infrastructure
/ Presentation / Installers / Tests, each shipped as its own `.asmdef`.
Dependency rules are enforced by the compiler and by an EditMode architecture
test that reflects over the assemblies.

**Consequences**: More files up front. Zero accidental cross-feature coupling.
Compile times stay low because small assemblies recompile independently.
Enables parallel work.

---

## ADR-0002 — Unity CLI as the primary editor-driving mechanism
**Date**: 2026-10-23
**Status**: Accepted.

**Context**: Project must be reproducible from code and CLI. User is new to
Unity and does not want to hand-click. The deprecated in-editor AI Assistant
MCP server is explicitly off-limits.

**Decision**: Use the official `unity` CLI (Unity Technologies) to drive the
running Editor. Prefer direct `unity command` / `unity eval` over MCP for
speed; fall back to MCP only when direct commands can't do a job. The CLI
reads the console, edits assets, runs tests, and triggers builds.

**Consequences**: Every editor action must be scripted. Humans never need to
click a non-obvious menu. Everything is replayable in CI.
