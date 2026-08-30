using System;

namespace MartinCalander.OdinSequence
{
    /// <summary>
    /// Adds an editable timeline above Odin's regular list drawer.
    /// Paths are relative to each list item and can use dots for nested members.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class SequenceStripAttribute : Attribute
    {
        /// <summary>
        /// Sets up the item members used by the timeline.
        /// </summary>
        /// <param name="startMember">Start time field or property.</param>
        /// <param name="durationMember">Duration field or property.</param>
        /// <param name="laneMember">Optional field or property used to group items into lanes.</param>
        /// <param name="labelMember">Optional field or property used as the item label.</param>
        /// <param name="colorMember">Optional Color or Color32 field or property.</param>
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

        /// <summary>Time between snap points. Set to zero to turn snapping off.</summary>
        public double SnapInterval { get; set; } = 0.1d;

        /// <summary>Shortest duration allowed when resizing an item. Integral members round up to a whole unit.</summary>
        public double MinimumDuration { get; set; } = 0.01d;

        /// <summary>Height of each lane, in pixels.</summary>
        public float LaneHeight { get; set; } = 28f;

        /// <summary>Maximum height of the scrollable lane area, in pixels.</summary>
        public float MaximumHeight { get; set; } = 220f;

        /// <summary>Shows Odin's list drawer below the timeline.</summary>
        public bool ShowList { get; set; } = true;

        /// <summary>Allows items to be dragged before time zero.</summary>
        public bool AllowNegativeTime { get; set; } = true;
    }
}
