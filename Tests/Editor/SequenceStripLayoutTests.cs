#if UNITY_EDITOR && ODIN_INSPECTOR
using System.Collections.Generic;
using MartinCalander.OdinSequence.Editor;
using NUnit.Framework;
using UnityEngine;

namespace MartinCalander.OdinSequence.Tests
{
    public sealed class SequenceStripLayoutTests
    {
        [Test]
        public void CalculateRange_ContainsEveryItemAndZeroWithPadding()
        {
            var items = new List<SequenceStripItemData>
            {
                Item(0, -2d, 1d, 0),
                Item(1, 3d, 2d, 1)
            };

            SequenceStripTimeRange range = SequenceStripLayout.CalculateRange(items, true);

            Assert.That(range.Start, Is.LessThan(-2d));
            Assert.That(range.End, Is.GreaterThan(5d));
            Assert.That(range.Start, Is.LessThanOrEqualTo(0d));
            Assert.That(range.End, Is.GreaterThanOrEqualTo(0d));
        }

        [Test]
        public void CalculateRange_UsesDefaultSpanForEmptyList()
        {
            SequenceStripTimeRange range = SequenceStripLayout.CalculateRange(
                new List<SequenceStripItemData>(),
                true);

            Assert.That(range.Span, Is.GreaterThanOrEqualTo(1d));
            Assert.That(range.Start, Is.LessThan(0d));
            Assert.That(range.End, Is.GreaterThan(0d));
        }

        [Test]
        public void CalculateRange_ExtremeFiniteValuesStayFinite()
        {
            var items = new List<SequenceStripItemData>
            {
                Item(0, -1e308d, 0d, 0),
                Item(1, 1e308d, 0d, 0)
            };

            SequenceStripTimeRange range = SequenceStripLayout.CalculateRange(items, true);

            Assert.That(SequenceStripLayout.IsFinite(range.Start), Is.True);
            Assert.That(SequenceStripLayout.IsFinite(range.End), Is.True);
            Assert.That(SequenceStripLayout.IsFinite(range.Span), Is.True);
            Assert.That(range.Span, Is.GreaterThan(0d));
        }

        [Test]
        public void BuildLaneOrder_ReturnsSortedDistinctLanes()
        {
            var items = new List<SequenceStripItemData>
            {
                Item(0, 0d, 1d, 4),
                Item(1, 1d, 1d, -2),
                Item(2, 2d, 1d, 4),
                Item(3, 3d, 1d, 1)
            };

            Assert.That(SequenceStripLayout.BuildLaneOrder(items), Is.EqualTo(new[] { -2, 1, 4 }));
        }

        [Test]
        public void CalculateItemRect_PlacesItemAtExpectedTimeAndLane()
        {
            SequenceStripItemData item = Item(0, 2d, 3d, 8);
            Rect rect = SequenceStripLayout.CalculateItemRect(
                item,
                1,
                0d,
                10d,
                new Rect(10f, 0f, 100f, 100f),
                30f,
                5f);

            Assert.That(rect.x, Is.EqualTo(30f).Within(0.001f));
            Assert.That(rect.width, Is.EqualTo(30f).Within(0.001f));
            Assert.That(rect.y, Is.EqualTo(28f).Within(0.001f));
            Assert.That(rect.height, Is.EqualTo(24f).Within(0.001f));
        }

        [Test]
        public void Snap_UsesAwayFromZeroAtHalfSteps()
        {
            Assert.That(SequenceStripLayout.Snap(1.125d, 0.25d), Is.EqualTo(1.25d).Within(0.000001d));
            Assert.That(SequenceStripLayout.Snap(-1.125d, 0.25d), Is.EqualTo(-1.25d).Within(0.000001d));
            Assert.That(SequenceStripLayout.Snap(1.125d, 0d), Is.EqualTo(1.125d));
        }

        [Test]
        public void NiceTickSpacing_UsesReadableOneTwoFiveSteps()
        {
            Assert.That(SequenceStripLayout.NiceTickSpacing(10d, 800f), Is.EqualTo(1d));
            Assert.That(SequenceStripLayout.NiceTickSpacing(24d, 800f), Is.EqualTo(5d));
        }

        private static SequenceStripItemData Item(int index, double start, double duration, int lane)
        {
            return new SequenceStripItemData(index, start, duration, lane, $"Item {index}", Color.white);
        }
    }
}
#endif
