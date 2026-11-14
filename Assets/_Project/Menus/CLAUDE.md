# Menus (main menu, pause menu, settings)

Namespace `BlockDiggerz.Menus`, asmdef `BlockDiggerz.Menus` (references `BlockDiggerz.Input`, `Unity.InputSystem`). Nothing here references Blocks.

## Scenes and prefabs
- `Assets/Scenes/MainMenu.unity` (build index 0): camera, `GameInput`, `MainMenu` prefab, `DebugMenu` prefab.
- `Assets/Scenes/Game.unity` (build index 1) holds a `PauseMenu` prefab instance wired to its `GameInput`.
- Prefabs in `Prefabs/`: `MainMenu` (UIDocument + `MainMenuView`), `PauseMenu` (UIDocument + `PauseMenuView`). Panel settings `UI/MenusPanelSettings.asset` (sort order 100: above the HUD at 0, below the debug menu at 1000).

## Behavior
- `MainMenuView`: Play loads `_gameScene`, Settings opens the shared panel, Quit calls `Application.Quit`. Gameplay keys off while the menu is up.
- `PauseMenuView`: the Menu/Pause action (Esc, gamepad Start, not rebindable) pauses: `Time.timeScale` 0 and `GameInput.SetGameplayEnabled(false)`. Esc goes back one page (`Core/MenuNavigator`); Esc on the root page continues. Works on the game over screen. Main menu resumes, then loads `_mainMenuScene`.
- `SettingsPanelController` (plain class, `UI/SettingsPanel.uxml`, instanced in both menu layouts): Controls tab has a header row, then one row per `GameInput.GameplayActions` entry with a Primary and an Alternate key button (the action's first two keyboard bindings; unbound shows a dash). Clicking starts an interactive rebind (keyboard only, Esc cancels, Backspace clears the slot). `Core/BindingSwapper` applies the key and swaps it with any other keyboard slot that used it; when a Primary ends up empty and its Alternate has a key, the Alternate moves up. Actions with one default key have an empty-path Keyboard binding in GameControls as their Alternate slot; new gameplay actions need one too. Empty overrides (cleared slots) are saved like any other. Sound tab: Music and SFX sliders 0 to 100. Each tab has Reset to defaults. While rebinding (and on the frame it ends) Esc belongs to the rebind, not to navigation.
- `Core/GameSettings`: volumes and binding override JSON in an `IKeyValueStore` (`PlayerPrefsKeyValueStore` in game). Both menu screens load saved bindings in `Awake`, so the Game scene works when played directly. After a rebind, `GameInput.NotifyBindingsChanged` refreshes key hints (the HUD store key).
- Volumes are placeholders: saved, not applied to audio yet. Audio code should read `GameSettings.MusicVolume` / `SfxVolume`.

## Tests
- `Tests/EditMode`: navigator, binding swap, key slots (empty, clear, move up), settings save and load, action labels. Run with `Tools/run-editmode-tests.sh MenuNavigatorTests BindingSwapperTests KeySlotTests GameSettingsTests ActionDisplayNamesTests`.
