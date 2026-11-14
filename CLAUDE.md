# BlockDiggerz

## Game overview
A 2D falling-block roguelike in the spirit of classic block-stacking puzzlers. The core loop: pieces fall onto a grid, the player moves and rotates them to complete and clear lines, and between rounds the player picks upgrades, special pieces or modifiers that change how a run plays. Runs end on a top-out; meta progression carries between runs. Target platform: Windows PC (to be confirmed). Input: keyboard, with gamepad support planned. Claude: fill in and keep current as the design firms up.

## Tech stack
- Unity 6000.6.9f1, URP 17.6.0 (2D renderer), Input System 1.20.1, 2D Tilemap + Tilemap Extras, 2D Sprite, Shader Graph, UI Toolkit, Unity Test Framework 1.8.0.
- VFX Graph is not installed yet; add `com.unity.visualeffectgraph` before building particle effects.
- Pipeline package (`com.unity.pipeline` 1.0.0) so the `unity` CLI drives the Editor.
- Editor settings: Run In Background on; Enter Play Mode is "Reload Scene only" (domain reload off), so reset static state on play (for example with `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`).

## Working with me
- Before non-trivial or ambiguous work, ask clarifying questions one at a time with multiple-choice answers. Small, clear edits need no questions.
- Humans edit and maintain this project: make editable prefabs, scenes and ScriptableObject data, and build editor tools (UI Toolkit) for human use.
- Never write builders that overwrite or discard human edits. Generators may only write derived output (icons, VFX, atlases) into folders marked as generated.
- Send a push notification when blocked, when you need a decision, or when a task is finished, not for each step.
- Editor dialogs: accept the API Updater and reimport prompts; in Safe Mode, enter it and fix the compile errors. Never accept discard, revert or delete prompts; ask me instead.
- If the Editor hangs or crashes, restart it and say so (see the unity-editor-cli-recovery skill).

## Project layout
- The Unity project is the repository root.
- Organize by feature: `Assets/_Project/<Feature>/{Scripts,Prefabs,Data,UI,Tests}`, each feature with its own asmdef.
- Art and audio go in `Assets/_Project/<Feature>/Art|Audio/` or, when shared, `Assets/_Project/Shared/<Area>/Art|Audio/` (areas such as UI or Gameplay, created as needed). Only these folders are picked up by the asset tracker.
- Larger features get their own CLAUDE.md; list them here. Cross-cutting rules stay in this file.

| Folder | Contents |
|---|---|
| `Assets/_Project/` | Game code and content, one subfolder per feature |
| `Assets/_Project/AssetTracking/` | Editor-only art/audio tracker synced with a Google Sheet |
| `Assets/Scenes/` | Scenes (template `SampleScene` for now) |
| `Assets/Settings/` | URP pipeline and 2D renderer assets |
| `Assets/Editor/` | Template editor scripts |
| `Assets/Welcome/` | Unity template welcome content; safe to remove later |

Feature CLAUDE.md files:
- [Assets/_Project/AssetTracking/CLAUDE.md](Assets/_Project/AssetTracking/CLAUDE.md): asset tracker and Google Sheet sync.

## Unity rules
- Input only through the Input System (via a single wrapper class), never UnityEngine.Input.
- UI Toolkit for all UI, HUD and editor tools.
- VFX Graph for particles, Shader Graph for transparent effects.
- One SRP Batcher compatible opaque shader, UberLit. Add features to it (properties, the UnityPerMaterial CBUFFER, shader_feature_local in each pass) instead of writing new shaders.
- Per-object material effects use instanced materials, never MaterialPropertyBlocks (they break SRP batching).
- Custom rendering goes through ScriptableRendererFeature with the Render Graph API.
- Pool anything spawned per frame (UnityEngine.Pool). No Find* calls or allocations in Update.

## Assets
- Every file and folder under Assets/ has a .meta with a GUID: move, rename and delete them together, never change a GUID.
- Asset serialization is Force Text. Prefer letting the Editor create and change assets over hand-editing YAML.

## C# style
- Standard C# conventions; private fields `_camelCase`; namespace `BlockDiggerz.<Feature>`.
- One MonoBehaviour or ScriptableObject per file, file named after the class.
- Expose tunables with `[SerializeField]` / `[field: SerializeField]` plus `[Tooltip]`.
- No magic strings or numbers: use named constants, serialized fields or ScriptableObjects.
- No inline comments; use XML `<summary>` on public types and non-obvious members.
- Never use em or en dashes in code, docs, UI text or commit messages.
- Prefer plain, readable code over clever patterns.

## Verification (a clean compile is NOT done)
- After gameplay, input, UI or flow changes: run the PlayMode tests and add or extend a test for the change.
- Input tests drive real Input System devices (queued state events), never autopilots or movement overrides.
- Set up and read game state through the debug menu and its CLI, not one-off script evals.
- Always exit Play mode after a manual test, because recompiles are deferred while playing.
- If something can't be tested automatically, say so and list what must be checked by hand.

## Unity CLI and shell
- Write custom CLI commands instead of script eval, and put repeated tasks in reusable scripts under Tools/.
- Never use heredocs (quotes and apostrophes break them). Write files with Write, change them with Edit, and run longer scripts from a file by path.
- On Windows PowerShell, pass C# to `unity command script eval` with `--file <path>`, not inline code: PowerShell splits quoted inline code into separate arguments.

## Git
- The project is not a git repository yet; initialize it (with a Unity .gitignore, LFS and the merge driver below) before the first commit.
- Assume multiple contributors. Short-lived feature branches off main; merge main in before finishing.
- Split scenes and prefabs per feature so two people rarely edit the same file.
- Commit .meta files with their assets. Never commit Library/, Temp/, Logs/, UserSettings/.
- Set up LFS by extension in .gitattributes (textures, models, audio, video, fonts) before adding binaries. Also check every commit for files over 10 MB.
- UnityYAMLMerge is the merge driver for scenes, prefabs and assets. At the start of a session, check it is configured and set it up if it is missing:
  - Check: `git config --get merge.unityyamlmerge.driver` points at a file that exists, and `git check-attr merge -- <any .unity file>` prints `unityyamlmerge`.
  - Find the binary in the Editor version from `ProjectSettings/ProjectVersion.txt` (Unity Hub's install folder): `<Editor>/Data/Tools/UnityYAMLMerge.exe` on Windows, `Unity.app/Contents/Tools/UnityYAMLMerge` on macOS, `<Editor>/Data/Tools/UnityYAMLMerge` on Linux.
  - .gitattributes (committed): `merge=unityyamlmerge eol=lf` for `*.unity *.prefab *.asset *.mat *.anim *.controller *.overrideController *.physicMaterial *.playable *.mask *.mixer *.spriteatlas *.terrainlayer *.renderTexture *.preset`. Ask before changing a committed .gitattributes.
  - Local config (per clone, never committed): `git config merge.unityyamlmerge.name "Unity SmartMerge"`, `git config merge.unityyamlmerge.driver "'<path>' merge -h -p --force %O %B %A %A"` and `git config merge.unityyamlmerge.recursive binary`.
  - Never commit a machine-specific Editor path. If the matching Editor isn't installed, say so instead of guessing a path.
  - If a merge still leaves conflicts, don't hand-merge the YAML; stop and ask.

## Builds
No build pipeline yet. Claude: add build menu items, CLI entry points (`unity build ... --execute-method`) and a smoke-test command here once they exist.
