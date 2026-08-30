#if UNITY_EDITOR && ODIN_INSPECTOR
using System;
using MartinCalander.OdinSequence.Editor;
using NUnit.Framework;
using UnityEngine;

namespace MartinCalander.OdinSequence.Tests
{
    public sealed class SequenceValueConverterTests
    {
        [Test]
        public void TryReadNumber_AcceptsSupportedNumericTypes()
        {
            object[] values =
            {
                (sbyte)-2,
                (byte)2,
                (short)-3,
                (ushort)3,
                -4,
                4u,
                -5L,
                5UL,
                1.25f,
                2.5d,
                3.75m
            };

            foreach (object value in values)
            {
                Assert.That(SequenceValueConverter.TryReadNumber(value, out double result), Is.True,
                    value.GetType().Name);
                Assert.That(double.IsNaN(result), Is.False);
            }
        }

        [Test]
        public void TryReadNumber_RejectsNonNumericAndNonFiniteValues()
        {
            Assert.That(SequenceValueConverter.TryReadNumber(true, out _), Is.False);
            Assert.That(SequenceValueConverter.TryReadNumber("2", out _), Is.False);
            Assert.That(SequenceValueConverter.TryReadNumber(double.NaN, out _), Is.False);
            Assert.That(SequenceValueConverter.TryReadNumber(float.PositiveInfinity, out _), Is.False);
        }

        [Test]
        public void TryConvertNumber_PreservesDestinationKindAndRoundsIntegers()
        {
            Assert.That(SequenceValueConverter.TryConvertNumber(1.5d, typeof(int), out object positive), Is.True);
            Assert.That(positive, Is.TypeOf<int>().And.EqualTo(2));

            Assert.That(SequenceValueConverter.TryConvertNumber(-1.5d, typeof(long), out object negative), Is.True);
            Assert.That(negative, Is.TypeOf<long>().And.EqualTo(-2L));

            Assert.That(SequenceValueConverter.TryConvertNumber(1.25d, typeof(float?), out object single), Is.True);
            Assert.That(single, Is.TypeOf<float>().And.EqualTo(1.25f));
        }

        [Test]
        public void TryConvertNumber_RejectsOverflowAndUnsupportedTypes()
        {
            Assert.That(SequenceValueConverter.TryConvertNumber(-1d, typeof(byte), out _), Is.False);
            Assert.That(SequenceValueConverter.TryConvertNumber(double.MaxValue, typeof(float), out _), Is.False);
            Assert.That(SequenceValueConverter.TryConvertNumber(1d, typeof(bool), out _), Is.False);
        }

        [Test]
        public void LaneAndColorConversions_AcceptExpectedSerializedShapes()
        {
            Assert.That(SequenceValueConverter.TryReadLane(2.5f, out int lane), Is.True);
            Assert.That(lane, Is.EqualTo(3));

            var packed = new Color32(10, 20, 30, 40);
            Assert.That(SequenceValueConverter.TryReadColor(packed, out Color color), Is.True);
            Assert.That((Color32)color, Is.EqualTo(packed));
        }

        [Test]
        public void ReadLabel_UsesFallbackForNullAndEmptyValues()
        {
            Assert.That(SequenceValueConverter.ReadLabel(null, "Fallback"), Is.EqualTo("Fallback"));
            Assert.That(SequenceValueConverter.ReadLabel(string.Empty, "Fallback"), Is.EqualTo("Fallback"));
            Assert.That(SequenceValueConverter.ReadLabel(42, "Fallback"), Is.EqualTo("42"));
        }
    }
}
#endif
