using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace BlockDiggerz.Blocks.Tests
{
    public class BlockColorTests
    {
        private BlockColor _color;
        private Texture2D _texture;
        private Sprite _sprite;

        [SetUp]
        public void SetUp()
        {
            _color = ScriptableObject.CreateInstance<BlockColor>();
            var so = new SerializedObject(_color);
            so.FindProperty("<Tint>k__BackingField").colorValue = Color.red;
            so.ApplyModifiedPropertiesWithoutUndo();
            _texture = new Texture2D(4, 4);
            _sprite = Sprite.Create(_texture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_sprite);
            Object.DestroyImmediate(_texture);
            Object.DestroyImmediate(_color);
        }

        [Test]
        public void WithoutSprite_DrawsWithTint()
        {
            Assert.AreEqual(Color.red, _color.DrawTint);
        }

        [Test]
        public void WithSprite_DrawsUntinted()
        {
            var so = new SerializedObject(_color);
            so.FindProperty("<Sprite>k__BackingField").objectReferenceValue = _sprite;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(Color.white, _color.DrawTint);
            Assert.AreEqual(Color.red, _color.Tint, "the ghost still uses the tint");
        }
    }
}
