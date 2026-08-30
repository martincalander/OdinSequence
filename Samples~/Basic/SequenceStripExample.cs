using System;
using System.Collections.Generic;
using UnityEngine;

namespace MartinCalander.OdinSequence.Samples
{
    public sealed class SequenceStripExample : MonoBehaviour
    {
        [SequenceStrip(
            nameof(SequenceCue.Start),
            nameof(SequenceCue.Duration),
            LaneMember = nameof(SequenceCue.Lane),
            LabelMember = nameof(SequenceCue.Label),
            ColorMember = nameof(SequenceCue.Color),
            SnapInterval = 0.25d)]
        [SerializeField]
        private List<SequenceCue> cues = new List<SequenceCue>
        {
            new SequenceCue("Open", 0f, 1.5f, 0, new Color(0.24f, 0.58f, 0.95f)),
            new SequenceCue("Camera", 0.75f, 2f, 1, new Color(0.72f, 0.42f, 0.92f)),
            new SequenceCue("Impact", 3f, 0.5f, 0, new Color(0.96f, 0.46f, 0.24f))
        };
    }

    [Serializable]
    public sealed class SequenceCue
    {
        public SequenceCue(string label, float start, float duration, int lane, Color color)
        {
            Label = label;
            Start = start;
            Duration = duration;
            Lane = lane;
            Color = color;
        }

        public string Label;
        public float Start;
        public float Duration = 1f;
        public int Lane;
        public Color Color = Color.white;
    }
}
