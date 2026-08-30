using System;
using System.Collections.Generic;
using UnityEngine;

namespace MartinCalander.OdinSequence.Samples
{
    public sealed class Fighter : MonoBehaviour
    {
        [SequenceStrip(
            nameof(FighterTimelineEvent.Start),
            nameof(FighterTimelineEvent.Duration),
            LaneMember = nameof(FighterTimelineEvent.Track),
            LabelMember = nameof(FighterTimelineEvent.Label),
            ColorMember = nameof(FighterTimelineEvent.Color),
            SnapInterval = 1d / 60d)]
        [SerializeField]
        private List<FighterTimelineEvent> attack = new List<FighterTimelineEvent>
        {
            new FighterTimelineEvent("Startup", 0f, 0.18f, FighterTrack.Animation, new Color(0.24f, 0.58f, 0.95f)),
            new FighterTimelineEvent("Sword Whoosh", 0.10f, 0.08f, FighterTrack.Audio, new Color(0.32f, 0.78f, 0.48f)),
            new FighterTimelineEvent("Active Hitbox", 0.18f, 0.10f, FighterTrack.Hitbox, new Color(0.96f, 0.30f, 0.24f)),
            new FighterTimelineEvent("Counterable Hurtbox", 0.18f, 0.22f, FighterTrack.Hurtbox, new Color(0.95f, 0.58f, 0.20f)),
            new FighterTimelineEvent("Slash VFX", 0.18f, 0.14f, FighterTrack.Vfx, new Color(0.72f, 0.42f, 0.92f)),
            new FighterTimelineEvent("Camera Impact", 0.20f, 0.08f, FighterTrack.Camera, new Color(0.95f, 0.82f, 0.28f)),
            new FighterTimelineEvent("Hit Audio", 0.20f, 0.08f, FighterTrack.Audio, new Color(0.32f, 0.78f, 0.48f)),
            new FighterTimelineEvent("Recovery", 0.28f, 0.32f, FighterTrack.Animation, new Color(0.24f, 0.58f, 0.95f))
        };
    }

    [Serializable]
    public sealed class FighterTimelineEvent
    {
        public FighterTimelineEvent(string label, float start, float duration, int track, Color color)
        {
            Label = label;
            Start = start;
            Duration = duration;
            Track = track;
            Color = color;
        }

        public string Label;
        public float Start;
        public float Duration;
        public int Track;
        public Color Color = Color.white;
    }

    public static class FighterTrack
    {
        public const int Animation = 0;
        public const int Hitbox = 1;
        public const int Hurtbox = 2;
        public const int Camera = 3;
        public const int Vfx = 4;
        public const int Audio = 5;
    }
}
