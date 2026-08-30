#if UNITY_EDITOR && ODIN_INSPECTOR
using MartinCalander.OdinSequence.Editor;
using NUnit.Framework;
using UnityEngine;

namespace MartinCalander.OdinSequence.Tests
{
    public sealed class SequenceStripItemDataTests
    {
        [Test]
        public void End_UsesStartWhenDurationIsNegative()
        {
            var item = new SequenceStripItemData(3, 8d, -2d, 1, "Invalid", Color.red);

            Assert.That(item.End, Is.EqualTo(8d));
        }

        [Test]
        public void Constructor_AssignsEveryValue()
        {
            Color color = new Color(0.1f, 0.2f, 0.3f, 0.4f);
            var item = new SequenceStripItemData(7, 1.25d, 0.5d, -3, "Cue", color);

            Assert.That(item.Index, Is.EqualTo(7));
            Assert.That(item.Start, Is.EqualTo(1.25d));
            Assert.That(item.Duration, Is.EqualTo(0.5d));
            Assert.That(item.Lane, Is.EqualTo(-3));
            Assert.That(item.Label, Is.EqualTo("Cue"));
            Assert.That(item.Color, Is.EqualTo(color));
        }
    }
}
#endif
