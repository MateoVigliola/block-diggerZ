using System.Collections;
using BlockDiggerz.Testing;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace BlockDiggerz.Blocks.Tests
{
    /// <summary>
    /// Plays the Game scene through the real keyboard: moving, auto repeat, soft drop, hard drop, rotation, line
    /// clears, gravity, top-out and restart. Setup (spawning a given piece, pausing gravity, filling a row) goes
    /// through the Blocks debug menu section.
    /// </summary>
    public class BlocksInputTests : InputTestBase
    {
        private const string GameScene = "Game";
        private const float SceneLoadTimeout = 15f;
        private const string Section = BlocksDebugControls.SectionTitle;
        private const int MaxDropsBeforeTopOut = 40;

        private BlocksGame _game;

        [UnitySetUp]
        public IEnumerator LoadGame()
        {
            yield return SceneManager.LoadSceneAsync(GameScene, LoadSceneMode.Single);
            yield return WaitFor(() =>
            {
                _game = Object.FindAnyObjectByType<BlocksGame>();
                return _game != null && _game.Board != null && _game.ActivePiece != null;
            }, SceneLoadTimeout, "Game scene with a falling piece");
            DebugMenuDriver.Set(Section, BlocksDebugControls.PauseGravityLabel, true);
            DebugMenuDriver.Press(Section, BlocksDebugControls.ClearBoardLabel);
        }

        [UnityTest]
        public IEnumerator TapLeftAndRight_MovesOneColumn_WithLettersAndArrows()
        {
            Spawn("T");
            int startX = _game.ActivePosition.x;

            yield return Tap(Keyboard.aKey);
            Assert.AreEqual(startX - 1, _game.ActivePosition.x, "A moves left");

            yield return Tap(Keyboard.dKey);
            Assert.AreEqual(startX, _game.ActivePosition.x, "D moves right");

            yield return Tap(Keyboard.leftArrowKey);
            Assert.AreEqual(startX - 1, _game.ActivePosition.x, "Left arrow moves left");

            yield return Tap(Keyboard.rightArrowKey);
            Assert.AreEqual(startX, _game.ActivePosition.x, "Right arrow moves right");
        }

        [UnityTest]
        public IEnumerator HoldingRight_AutoRepeats_UntilTheWall()
        {
            Spawn("T");
            int wallX = _game.Rules.BoardWidth - _game.ActivePiece.BoxSize;

            yield return Hold(Keyboard.dKey, FramesFor(0.7f));

            Assert.AreEqual(wallX, _game.ActivePosition.x, "T piece pressed against the right wall");
        }

        [UnityTest]
        public IEnumerator PausedGravity_KeepsPieceStill()
        {
            Spawn("T");
            int startY = _game.ActivePosition.y;

            yield return Frames(FramesFor(2f));

            Assert.AreEqual(startY, _game.ActivePosition.y);
        }

        [UnityTest]
        public IEnumerator HoldingSoftDrop_FallsFast_WithSAndDownArrow()
        {
            Spawn("T");
            int startY = _game.ActivePosition.y;

            yield return Hold(Keyboard.sKey, FramesFor(0.25f));
            int afterS = _game.ActivePosition.y;
            Assert.GreaterOrEqual(startY - afterS, 3, "S should drop several rows in a quarter second");

            yield return Hold(Keyboard.downArrowKey, FramesFor(0.25f));
            Assert.GreaterOrEqual(afterS - _game.ActivePosition.y, 3, "Down arrow should drop several rows in a quarter second");
        }

        [UnityTest]
        public IEnumerator W_HardDropsAndLocksAtTheBottom()
        {
            Spawn("O");
            int locked = _game.PiecesLocked;
            PieceDefinition o = _game.ActivePiece;

            yield return Tap(Keyboard.wKey);

            Assert.AreEqual(locked + 1, _game.PiecesLocked);
            Assert.AreEqual(o, _game.Board.Get(4, 0));
            Assert.AreEqual(o, _game.Board.Get(5, 0));
            Assert.AreEqual(o, _game.Board.Get(4, 1));
            Assert.AreEqual(o, _game.Board.Get(5, 1));
        }

        [UnityTest]
        public IEnumerator RotateKeys_TurnPieceClockwise()
        {
            Spawn("T");

            yield return Tap(Keyboard.eKey);
            Assert.AreEqual(1, _game.ActiveRotation, "E rotates clockwise");

            yield return Tap(Keyboard.upArrowKey);
            Assert.AreEqual(2, _game.ActiveRotation, "Up arrow rotates clockwise");
        }

        [UnityTest]
        public IEnumerator Rotation_AgainstWall_PushesPieceAwayFromWall()
        {
            Spawn("I");
            yield return Tap(Keyboard.eKey);
            Assert.AreEqual(1, _game.ActiveRotation, "I piece stands up");

            for (int i = 0; i < _game.Rules.BoardWidth; i++)
            {
                yield return Tap(Keyboard.aKey);
            }

            Vector2Int atWall = _game.ActivePosition;
            yield return Tap(Keyboard.eKey);

            Assert.AreEqual(2, _game.ActiveRotation, "lying flat is allowed by pushing");
            Assert.AreEqual(atWall + new Vector2Int(2, 0), _game.ActivePosition, "pushed 2 cells right, away from the left wall");
        }

        [UnityTest]
        public IEnumerator FullRow_IsCleared_AndHudCountsIt()
        {
            DebugMenuDriver.Press(Section, BlocksDebugControls.FillBottomRowLabel);
            Spawn("I");

            yield return Tap(Keyboard.wKey);
            yield return null;

            Assert.AreEqual(1, _game.TotalLinesCleared);
            for (int x = 0; x < _game.Board.Width; x++)
            {
                Assert.IsNull(_game.Board.Get(x, 0), $"bottom row cell {x} should be cleared");
            }

            Label lines = HudRoot().Q<Label>(BlocksHud.LinesValueName);
            Assert.AreEqual("1", lines.text);
        }

        [UnityTest]
        public IEnumerator Gravity_PullsPieceDownOnItsOwn()
        {
            DebugMenuDriver.Set(Section, BlocksDebugControls.PauseGravityLabel, false);
            Spawn("T");
            int startY = _game.ActivePosition.y;
            int expectedRows = Mathf.FloorToInt(2.5f / _game.Rules.GravityInterval);

            yield return Frames(FramesFor(2.5f));

            Assert.LessOrEqual(_game.ActivePosition.y, startY - expectedRows);
        }

        [UnityTest]
        public IEnumerator StackReachingTheTop_EndsRun_AndRRestarts()
        {
            for (int i = 0; i < MaxDropsBeforeTopOut && _game.State == BlocksGameState.Playing; i++)
            {
                yield return Tap(Keyboard.wKey);
            }

            Assert.AreEqual(BlocksGameState.GameOver, _game.State, "stacking in the middle must top out");
            yield return Frames(2);
            BlocksHud hud = Object.FindAnyObjectByType<BlocksHud>();
            Assert.IsTrue(hud.IsGameOverShown, "game over panel visible");

            yield return Tap(Keyboard.rKey);
            yield return Frames(2);

            Assert.AreEqual(BlocksGameState.Playing, _game.State);
            Assert.AreEqual(0, _game.TotalLinesCleared);
            Assert.AreEqual(0, _game.PiecesLocked);
            Assert.IsNotNull(_game.ActivePiece);
            Assert.IsFalse(hud.IsGameOverShown, "game over panel hidden after restart");
        }

        private static void Spawn(string pieceId)
        {
            DebugMenuDriver.Press(Section, BlocksDebugControls.SpawnLabelPrefix + pieceId);
        }

        private static int FramesFor(float gameSeconds)
        {
            return Mathf.CeilToInt(gameSeconds / FrameTime);
        }

        private static VisualElement HudRoot()
        {
            return Object.FindAnyObjectByType<BlocksHud>().GetComponent<UIDocument>().rootVisualElement;
        }
    }
}
