#if UNITY_EDITOR && ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MartinCalander.OdinSequence.Editor
{
    internal static class SequenceStripLayout
    {
        private const double SmallestSpan = 0.000001d;
        private const double LargestTimelineCoordinate = 1e30d;

        public static SequenceStripTimeRange CalculateRange(
            IReadOnlyList<SequenceStripItemData> items,
            bool includeZero,
            double minimumSpan = 1d,
            double paddingFraction = 0.08d)
        {
            minimumSpan = Math.Max(SmallestSpan, minimumSpan);
            double minimum = includeZero ? 0d : double.PositiveInfinity;
            double maximum = includeZero ? 0d : double.NegativeInfinity;

            if (items != null)
            {
                for (int index = 0; index < items.Count; index++)
                {
                    SequenceStripItemData item = items[index];
                    if (!IsFinite(item.Start) || !IsFinite(item.End))
                        continue;

                    double itemStart = ClampTimelineCoordinate(item.Start);
                    double itemEnd = ClampTimelineCoordinate(item.End);
                    minimum = Math.Min(minimum, Math.Min(itemStart, itemEnd));
                    maximum = Math.Max(maximum, Math.Max(itemStart, itemEnd));
                }
            }

            if (!IsFinite(minimum) || !IsFinite(maximum))
            {
                minimum = 0d;
                maximum = minimumSpan;
            }

            double span = maximum - minimum;
            if (span < minimumSpan)
            {
                double center = (minimum + maximum) * 0.5d;
                minimum = center - minimumSpan * 0.5d;
                maximum = center + minimumSpan * 0.5d;
                span = minimumSpan;
            }

            double padding = Math.Max(0d, paddingFraction) * span;
            if (!IsFinite(padding) || !IsFinite(minimum - padding) || !IsFinite(maximum + padding))
                padding = 0d;

            return new SequenceStripTimeRange(minimum - padding, maximum + padding);
        }

        public static List<int> BuildLaneOrder(IReadOnlyList<SequenceStripItemData> items)
        {
            var lanes = new List<int>();
            if (items == null)
                return lanes;

            for (int index = 0; index < items.Count; index++)
            {
                int lane = items[index].Lane;
                if (!lanes.Contains(lane))
                    lanes.Add(lane);
            }

            lanes.Sort();
            return lanes;
        }

        public static Rect CalculateItemRect(
            SequenceStripItemData item,
            int laneRow,
            double viewStart,
            double viewSpan,
            Rect timeArea,
            float laneHeight,
            float verticalOffset,
            float minimumWidth = 6f)
        {
            float x = TimeToPixel(item.Start, viewStart, viewSpan, timeArea);
            float endX = TimeToPixel(item.End, viewStart, viewSpan, timeArea);
            float width = Mathf.Max(minimumWidth, endX - x);
            float y = laneRow * laneHeight - verticalOffset + 3f;
            return new Rect(x, y, width, Mathf.Max(4f, laneHeight - 6f));
        }

        public static float TimeToPixel(double time, double viewStart, double viewSpan, Rect area)
        {
            if (viewSpan <= SmallestSpan || area.width <= 0f)
                return area.x;

            double normalized = (time - viewStart) / viewSpan;
            normalized = Math.Max(-10000d, Math.Min(10000d, normalized));
            return area.x + (float)normalized * area.width;
        }

        public static double PixelToTime(float pixel, double viewStart, double viewSpan, Rect area)
        {
            if (area.width <= 0f)
                return viewStart;

            return viewStart + (pixel - area.x) / area.width * viewSpan;
        }

        public static double Snap(double value, double interval)
        {
            if (!IsFinite(value) || !IsFinite(interval) || interval <= 0d)
                return value;

            return Math.Round(value / interval, MidpointRounding.AwayFromZero) * interval;
        }

        public static double NiceTickSpacing(double visibleSpan, float pixelWidth, float targetPixels = 80f)
        {
            if (!IsFinite(visibleSpan) || visibleSpan <= 0d || pixelWidth <= 0f)
                return 1d;

            double rough = visibleSpan / Math.Max(1d, pixelWidth / Math.Max(20f, targetPixels));
            double magnitude = Math.Pow(10d, Math.Floor(Math.Log10(rough)));
            double normalized = rough / magnitude;
            double step;

            if (normalized <= 1d)
                step = 1d;
            else if (normalized <= 2d)
                step = 2d;
            else if (normalized <= 5d)
                step = 5d;
            else
                step = 10d;

            return step * magnitude;
        }

        public static double ClampZoom(double zoom)
        {
            return Math.Max(0.25d, Math.Min(64d, zoom));
        }

        public static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static double ClampTimelineCoordinate(double value)
        {
            return Math.Max(-LargestTimelineCoordinate, Math.Min(LargestTimelineCoordinate, value));
        }
    }
}
#endif
