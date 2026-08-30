#if UNITY_EDITOR && ODIN_INSPECTOR
using System;
using UnityEngine;

namespace MartinCalander.OdinSequence.Editor
{
    internal struct SequenceStripItemData
    {
        public SequenceStripItemData(int index, double start, double duration, int lane, string label, Color color)
        {
            Index = index;
            Start = start;
            Duration = duration;
            Lane = lane;
            Label = label;
            Color = color;
        }

        public int Index { get; }

        public double Start { get; }

        public double Duration { get; }

        public int Lane { get; }

        public string Label { get; }

        public Color Color { get; }

        public double End => Start + Math.Max(0d, Duration);
    }

    internal struct SequenceStripTimeRange
    {
        public SequenceStripTimeRange(double start, double end)
        {
            Start = start;
            End = end;
        }

        public double Start { get; }

        public double End { get; }

        public double Span => End - Start;
    }
}
#endif
