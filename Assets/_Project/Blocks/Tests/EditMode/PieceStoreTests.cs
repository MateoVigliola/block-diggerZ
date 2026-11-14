using NUnit.Framework;

namespace BlockDiggerz.Blocks.Tests
{
    public class PieceStoreTests
    {
        private PieceStore _store;
        private PaintedPiece _t;
        private PaintedPiece _i;

        [SetUp]
        public void SetUp()
        {
            _store = new PieceStore();
            _t = new PaintedPiece(BlocksTestAssets.Piece("T"), System.Array.Empty<BlockColor>());
            _i = new PaintedPiece(BlocksTestAssets.Piece("I"), System.Array.Empty<BlockColor>());
        }

        [Test]
        public void FirstStore_KeepsPiece_AndReleasesNothing()
        {
            Assert.IsTrue(_store.TryStore(_t, out PaintedPiece released));
            Assert.IsNull(released, "nothing stored yet, so the next piece comes from the bag");
            Assert.AreEqual(_t, _store.Stored);
        }

        [Test]
        public void StoringAgain_BeforeAPieceLocks_IsRefused()
        {
            _store.TryStore(_t, out _);

            Assert.IsFalse(_store.CanStore);
            Assert.IsFalse(_store.TryStore(_i, out PaintedPiece released));
            Assert.IsNull(released);
            Assert.AreEqual(_t, _store.Stored, "refused store leaves the slot alone");
        }

        [Test]
        public void AfterAPieceLocks_StoreSwaps()
        {
            _store.TryStore(_t, out _);
            _store.OnPieceLocked();

            Assert.IsTrue(_store.TryStore(_i, out PaintedPiece released));
            Assert.AreEqual(_t, released);
            Assert.AreEqual(_i, _store.Stored);
            Assert.IsFalse(_store.CanStore);
        }

        [Test]
        public void Reset_EmptiesSlotAndUnlocks()
        {
            _store.TryStore(_t, out _);

            _store.Reset();

            Assert.IsNull(_store.Stored);
            Assert.IsTrue(_store.CanStore);
        }
    }
}
