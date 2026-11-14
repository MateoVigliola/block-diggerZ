using NUnit.Framework;

namespace BlockDiggerz.Blocks.Tests
{
    public class PieceStoreTests
    {
        private PieceStore _store;
        private PieceDefinition _t;
        private PieceDefinition _i;

        [SetUp]
        public void SetUp()
        {
            _store = new PieceStore();
            _t = BlocksTestAssets.Piece("T");
            _i = BlocksTestAssets.Piece("I");
        }

        [Test]
        public void FirstStore_KeepsPiece_AndReleasesNothing()
        {
            Assert.IsTrue(_store.TryStore(_t, out PieceDefinition released));
            Assert.IsNull(released, "nothing stored yet, so the next piece comes from the bag");
            Assert.AreEqual(_t, _store.Stored);
        }

        [Test]
        public void StoringAgain_BeforeAPieceLocks_IsRefused()
        {
            _store.TryStore(_t, out _);

            Assert.IsFalse(_store.CanStore);
            Assert.IsFalse(_store.TryStore(_i, out PieceDefinition released));
            Assert.IsNull(released);
            Assert.AreEqual(_t, _store.Stored, "refused store leaves the slot alone");
        }

        [Test]
        public void AfterAPieceLocks_StoreSwaps()
        {
            _store.TryStore(_t, out _);
            _store.OnPieceLocked();

            Assert.IsTrue(_store.TryStore(_i, out PieceDefinition released));
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
