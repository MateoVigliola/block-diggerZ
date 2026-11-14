# BlockDiggerz

## Game overview
A 2D falling-block roguelike in the spirit of classic block-stacking puzzlers. The core loop: pieces fall onto a grid, the player moves and rotates them to complete and clear lines, and between rounds the player picks upgrades, special pieces or modifiers that change how a run plays. Runs end on a top-out; meta progression carries between runs. Target platform: Windows PC (to be confirmed). Input: keyboard, with gamepad support planned. Claude: fill in and keep current as the design firms up.
Built so far: the core block dropper (10 x 20 board, the 7 standard pieces from a 7-bag, A/D and arrows to move, E/Up and Q for simple clockwise and counter-clockwise rotation, S/Down soft drop, W hard drop, line clears, game over with R to restart) and a piece tag system with only a "Basic" tag for now.

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
| `Assets/_Project/Blocks/` | Core block dropper: pieces, tags, board, game loop, HUD |
| `Assets/_Project/Input/` | `GameInput` wrapper and the `GameControls` actions asset |
| `Assets/_Project/DebugTools/` | In-game debug menu (backquote) and its `unity command debugmenu list/do` CLI |
| `Assets/_Project/Playtest/` | Editor window (Tools > BlockDiggerz > Playtest Checklist) for `Docs/PlaytestChecklist.md` |
| `Assets/_Project/Shared/Testing/` | Shared test helpers (`InputTestBase`, `DebugMenuDriver`) |
| `Assets/Scenes/` | `Game` (main scene, build index 0) and template `SampleScene` |
| `Assets/Settings/` | URP pipeline and 2D renderer assets |
| `Assets/Editor/` | Template editor scripts |
| `Assets/Welcome/` | Unity template welcome content; safe to remove later |

Feature CLAUDE.md files:
- [Assets/_Project/AssetTracking/CLAUDE.md](Assets/_Project/AssetTracking/CLAUDE.md): asset tracker and Google Sheet sync.
- [Assets/_Project/Blocks/CLAUDE.md](Assets/_Project/Blocks/CLAUDE.md): core block dropper, pieces and tags.

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
- Test only what was just added. Put the rules of a new feature in plain code and cover them with EditMode tests, running only the new tests (filter by test class), not whole suites.
- Don't run PlayMode tests, and don't enter Play mode to check things, unless I ask. They take a long time and take over the Editor while I'm playtesting.
- Regression runs (whole suites, PlayMode) only when I report a problem I found. Then also add a test that reproduces it.
- If an existing test no longer matches a deliberate change, update it so it isn't left knowingly broken, but don't run it.
- Input tests drive real Input System devices (queued state events), never autopilots or movement overrides.
- Set up and read game state through the debug menu and its CLI, not one-off script evals.
- Always exit Play mode after a manual test, because recompiles are deferred while playing.
- If something can't be tested automatically, say so and list what must be checked by hand.

## Playtesting (mine, not Claude's)
- Leave all playtesting and game feel testing to me. Don't play the game to judge feel, balance, difficulty, pacing, juice, readability or fun, and don't tune those values on your own judgment.
- Instead, after each feature, add a section to `Docs/PlaytestChecklist.md` (create it if missing) with the feature name, date, and a checkbox list of what I should test: what to try, what should happen, and which tunables (with their asset or component path) to adjust if it feels off.
- Keep the item format the checklist window (`Assets/_Project/Playtest/`) parses: `## Feature, date` headings, items as `- [ ] **Title:** text` on one line, each followed by `  - Notes:`. Further notes lines are indented four spaces. I tick items in the window and press Confirm, which writes `[x]`; `[x]` items are done.
- Keep working on the next feature while I playtest. I write my notes under each item; read them at the start of the next session or when I point you at them, and turn them into fixes or tuning changes.
- Automated tests are still your job, within the limits in Verification above; this split covers only how the game plays and feels.

## Unity CLI and shell
- Write custom CLI commands instead of script eval, and put repeated tasks in reusable scripts under Tools/.
- Never use heredocs (quotes and apostrophes break them). Write files with Write, change them with Edit, and run longer scripts from a file by path.
- On Windows PowerShell, pass C# to `unity command script eval` with `--file <path>`, not inline code: PowerShell splits quoted inline code into separate arguments.

## Git
- The repository is on branch main with a Unity .gitignore, LFS and the merge driver below already configured. No remote yet.
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
