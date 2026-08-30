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
            SnapInterval = 1d)]
        [SerializeField]
        private List<FighterTimelineEvent> attack = new List<FighterTimelineEvent>
        {
            new FighterTimelineEvent("Startup", 0, 11, FighterTrack.Animation, new Color(0.24f, 0.58f, 0.95f)),
            new FighterTimelineEvent("Sword Whoosh", 6, 5, FighterTrack.Audio, new Color(0.32f, 0.78f, 0.48f)),
            new FighterTimelineEvent("Active Hitbox", 11, 6, FighterTrack.Hitbox, new Color(0.96f, 0.30f, 0.24f)),
            new FighterTimelineEvent("Counterable Hurtbox", 11, 13, FighterTrack.Hurtbox, new Color(0.95f, 0.58f, 0.20f)),
            new FighterTimelineEvent("Slash VFX", 11, 8, FighterTrack.Vfx, new Color(0.72f, 0.42f, 0.92f)),
            new FighterTimelineEvent("Camera Impact", 12, 5, FighterTrack.Camera, new Color(0.95f, 0.82f, 0.28f)),
            new FighterTimelineEvent("Hit Audio", 12, 5, FighterTrack.Audio, new Color(0.32f, 0.78f, 0.48f)),
            new FighterTimelineEvent("Recovery", 17, 19, FighterTrack.Animation, new Color(0.24f, 0.58f, 0.95f))
        };
    }

    [Serializable]
    public sealed class FighterTimelineEvent
    {
        public FighterTimelineEvent(string label, int start, int duration, int track, Color color)
        {
            Label = label;
            Start = start;
            Duration = duration;
            Track = track;
            Color = color;
        }

        public string Label;
        public int Start;
        public int Duration;
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
