using System;

namespace MartinCalander.OdinSequence
{
    /// <summary>
    /// Draws a list of timed records as an editable strip above its normal Odin list.
    /// Member paths are relative to each list element and may contain dots for nesting.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class SequenceStripAttribute : Attribute
    {
        /// <summary>
        /// Creates a sequence strip for a list.
        /// </summary>
        /// <param name="startMember">Numeric start-time member path.</param>
        /// <param name="durationMember">Numeric duration member path.</param>
        /// <param name="laneMember">Optional numeric lane member path.</param>
        /// <param name="labelMember">Optional label member path.</param>
        /// <param name="colorMember">Optional Color or Color32 member path.</param>
        public SequenceStripAttribute(
            string startMember,
            string durationMember,
            string laneMember = null,
            string labelMember = null,
            string colorMember = null)
        {
            StartMember = startMember;
            DurationMember = durationMember;
            LaneMember = laneMember;
            LabelMember = labelMember;
            ColorMember = colorMember;
        }

        public string StartMember { get; }

        public string DurationMember { get; }

        public string LaneMember { get; set; }

        public string LabelMember { get; set; }

        public string ColorMember { get; set; }

        /// <summary>Snap interval in sequence units. Set to zero to disable snapping.</summary>
        public double SnapInterval { get; set; } = 0.1d;

        /// <summary>Smallest duration produced by a resize operation.</summary>
        public double MinimumDuration { get; set; } = 0.01d;

        /// <summary>Height of one lane in pixels.</summary>
        public float LaneHeight { get; set; } = 28f;

        /// <summary>Maximum height of the scrollable lane area in pixels.</summary>
        public float MaximumHeight { get; set; } = 220f;

        /// <summary>Show Odin's normal list drawer below the strip.</summary>
        public bool ShowList { get; set; } = true;

        /// <summary>Allow items to be dragged before time zero.</summary>
        public bool AllowNegativeTime { get; set; } = true;
    }
}
