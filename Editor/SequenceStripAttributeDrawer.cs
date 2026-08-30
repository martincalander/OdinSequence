#if UNITY_EDITOR && ODIN_INSPECTOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace MartinCalander.OdinSequence.Editor
{
    public sealed class SequenceStripAttributeDrawer : OdinAttributeDrawer<SequenceStripAttribute>
    {
        private const float GutterWidth = 54f;
        private const float RulerHeight = 22f;
        private const float ScrollbarSize = 14f;
        private const float ResizeHandleWidth = 5f;
        private const int MaximumIssueCount = 4;

        private static readonly int TimelineControlHint = "OdinSequenceTimeline".GetHashCode();

        private readonly List<SequenceStripBinding> bindings = new List<SequenceStripBinding>();
        private readonly List<SequenceStripItemData> itemData = new List<SequenceStripItemData>();
        private readonly List<ItemVisual> visuals = new List<ItemVisual>();
        private readonly List<string> issues = new List<string>();

        private bool stateInitialized;
        private bool showList;
        private bool snapEnabled;
        private bool viewInitialized;
        private bool fitView = true;
        private double zoom = 1d;
        private double viewStart;
        private float verticalScroll;
        private int selectedIndex = -1;

        private DragMode dragMode;
        private int dragControlId;
        private double dragPointerTime;
        private double dragStart;
        private double dragDuration;
        private SequenceStripBinding dragBinding;

        private GUIStyle blockLabelStyle;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            EnsureState();

            if (Property.ValueEntry == null ||
                Property.ValueEntry.TypeOfValue == null ||
                !typeof(IList).IsAssignableFrom(Property.ValueEntry.TypeOfValue))
            {
                EditorGUILayout.HelpBox(
                    "[SequenceStrip] only works on IList fields and properties.",
                    MessageType.Error);
                CallNextDrawer(label);
                return;
            }

            if (Property.ValueEntry.ValueCount != 1)
            {
                DrawToolbar(label, new SequenceStripTimeRange(0d, 1d));
                EditorGUILayout.HelpBox(
                    "The timeline only supports one selected object at a time. You can still edit multiple objects in the list below.",
                    MessageType.Info);
                CallNextDrawer(label);
                return;
            }

            var list = Property.ValueEntry.WeakSmartValue as IList;
            if (list == null)
            {
                DrawToolbar(label, new SequenceStripTimeRange(0d, 1d));
                EditorGUILayout.HelpBox(
                    "The list is null. Create it in the list below.",
                    MessageType.Warning);
                CallNextDrawer(label);
                return;
            }

            BuildBindings(list);
            SequenceStripTimeRange range = SequenceStripLayout.CalculateRange(itemData, true);
            DrawToolbar(label, range);

            bool showFallbackList = false;
            if (list.Count == 0)
            {
                DrawEmptyState("The list is empty. Add an item below to get started.");
                showFallbackList = true;
            }
            else if (bindings.Count == 0)
            {
                DrawEmptyState("None of the items have readable start and duration values.");
                showFallbackList = true;
            }
            else
            {
                DrawTimeline(range);
            }

            if (issues.Count > 0)
                EditorGUILayout.HelpBox(string.Join("\n", issues), MessageType.Warning);

            if (showList || showFallbackList)
                CallNextDrawer(label);
        }

        private void EnsureState()
        {
            if (stateInitialized)
                return;

            showList = Attribute.ShowList;
            snapEnabled = Attribute.SnapInterval > 0d;
            stateInitialized = true;
        }

        private void BuildBindings(IList list)
        {
            bindings.Clear();
            itemData.Clear();
            issues.Clear();

            if (string.IsNullOrWhiteSpace(Attribute.StartMember) ||
                string.IsNullOrWhiteSpace(Attribute.DurationMember))
            {
                AddIssue("StartMember and DurationMember must both be set.");
                return;
            }

            Property.Children.Update();
            int resolvedCount = Math.Min(list.Count, Property.Children.Count);
            if (resolvedCount < list.Count)
                AddIssue("Odin hasn't loaded every list item yet. You can still edit the list below.");

            for (int index = 0; index < resolvedCount; index++)
            {
                InspectorProperty element = Property.Children[index];
                if (element == null || element.ValueEntry == null || element.ValueEntry.WeakSmartValue == null)
                {
                    AddIssue($"Item {index} is null, so it was skipped.");
                    continue;
                }

                InspectorProperty startProperty = OdinMemberPath.Resolve(element, Attribute.StartMember);
                InspectorProperty durationProperty = OdinMemberPath.Resolve(element, Attribute.DurationMember);
                if (!TryReadRequiredNumber(startProperty, Attribute.StartMember, index, out double start) ||
                    !TryReadRequiredNumber(durationProperty, Attribute.DurationMember, index, out double duration))
                {
                    continue;
                }

                if (duration < 0d)
                {
                    AddIssue($"Item {index} has a negative duration, so it is drawn at zero length.");
                    duration = 0d;
                }

                InspectorProperty laneProperty = null;
                int lane = 0;
                if (!string.IsNullOrWhiteSpace(Attribute.LaneMember))
                {
                    laneProperty = OdinMemberPath.Resolve(element, Attribute.LaneMember);
                    if (laneProperty == null || laneProperty.ValueEntry == null)
                    {
                        AddIssue($"Lane member '{Attribute.LaneMember}' was not found on item {index}. Lane 0 is used.");
                        laneProperty = null;
                    }
                    else if (!SequenceValueConverter.TryReadLane(laneProperty.ValueEntry.WeakSmartValue, out lane))
                    {
                        AddIssue($"Lane member '{Attribute.LaneMember}' on item {index} isn't numeric. Lane 0 is used.");
                        laneProperty = null;
                        lane = 0;
                    }
                }

                string defaultLabel = $"Item {index}";
                string itemLabel = defaultLabel;
                if (!string.IsNullOrWhiteSpace(Attribute.LabelMember))
                {
                    InspectorProperty labelProperty = OdinMemberPath.Resolve(element, Attribute.LabelMember);
                    if (labelProperty == null || labelProperty.ValueEntry == null)
                        AddIssue($"Label member '{Attribute.LabelMember}' was not found on item {index}.");
                    else
                        itemLabel = SequenceValueConverter.ReadLabel(labelProperty.ValueEntry.WeakSmartValue, defaultLabel);
                }

                Color color = DefaultColor(index);
                if (!string.IsNullOrWhiteSpace(Attribute.ColorMember))
                {
                    InspectorProperty colorProperty = OdinMemberPath.Resolve(element, Attribute.ColorMember);
                    if (colorProperty == null || colorProperty.ValueEntry == null)
                        AddIssue($"Color member '{Attribute.ColorMember}' was not found on item {index}.");
                    else if (!SequenceValueConverter.TryReadColor(colorProperty.ValueEntry.WeakSmartValue, out color))
                        AddIssue($"Color member '{Attribute.ColorMember}' on item {index} must be a Color or Color32.");
                }

                color.a = Mathf.Max(0.35f, color.a);
                var data = new SequenceStripItemData(index, start, duration, lane, itemLabel, color);
                var binding = new SequenceStripBinding(
                    data,
                    element,
                    startProperty,
                    durationProperty);
                bindings.Add(binding);
                itemData.Add(data);
            }

            if (selectedIndex >= list.Count)
                selectedIndex = -1;
        }

        private bool TryReadRequiredNumber(
            InspectorProperty member,
            string path,
            int itemIndex,
            out double value)
        {
            value = 0d;
            if (member == null || member.ValueEntry == null)
            {
                AddIssue($"Member '{path}' was not found on item {itemIndex}.");
                return false;
            }

            if (!SequenceValueConverter.TryReadNumber(member.ValueEntry.WeakSmartValue, out value))
            {
                AddIssue($"Member '{path}' on item {itemIndex} must be a finite numeric value.");
                return false;
            }

            return true;
        }

        private void DrawToolbar(GUIContent label, SequenceStripTimeRange range)
        {
            GUIContent title = label ?? new GUIContent(Property.NiceName);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(title, EditorStyles.miniBoldLabel, GUILayout.MinWidth(48f));
            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(Attribute.SnapInterval <= 0d))
            {
                string snapText = Attribute.SnapInterval > 0d
                    ? $"Snap {FormatTime(Attribute.SnapInterval)}"
                    : "Snap off";
                var snapContent = new GUIContent(
                    snapText,
                    "Hold Control or Command while dragging to bypass snapping.");
                snapEnabled = GUILayout.Toggle(
                    snapEnabled,
                    snapContent,
                    EditorStyles.toolbarButton,
                    GUILayout.Width(72f));
            }

            if (GUILayout.Button(new GUIContent("−", "Zoom out"), EditorStyles.toolbarButton, GUILayout.Width(24f)))
                ZoomAround(range, zoom / 1.5d, VisibleCenter(range));

            if (GUILayout.Button(new GUIContent("Fit", "Fit all items"), EditorStyles.toolbarButton, GUILayout.Width(30f)))
                Fit(range);

            if (GUILayout.Button(new GUIContent("+", "Zoom in"), EditorStyles.toolbarButton, GUILayout.Width(24f)))
                ZoomAround(range, zoom * 1.5d, VisibleCenter(range));

            GUILayout.Label($"{zoom:0.#}x", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(36f));
            showList = GUILayout.Toggle(
                showList,
                new GUIContent("List", "Show Odin's normal list drawer"),
                EditorStyles.toolbarButton,
                GUILayout.Width(38f));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEmptyState(string message)
        {
            Rect rect = GUILayoutUtility.GetRect(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.Height(44f),
                GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, PanelColor);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, rect.height - 12f),
                message,
                EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawTimeline(SequenceStripTimeRange range)
        {
            if (!viewInitialized || fitView)
            {
                viewStart = range.Start;
                viewInitialized = true;
            }

            zoom = SequenceStripLayout.ClampZoom(zoom);
            double visibleSpan = Math.Max(0.000001d, range.Span / zoom);
            List<int> lanes = SequenceStripLayout.BuildLaneOrder(itemData);
            float laneHeight = Mathf.Clamp(Attribute.LaneHeight, 22f, 60f);
            float maximumHeight = Mathf.Clamp(Attribute.MaximumHeight, laneHeight, 520f);
            float laneContentHeight = Math.Max(laneHeight, lanes.Count * laneHeight);
            float laneViewportHeight = Math.Min(laneContentHeight, maximumHeight);
            bool hasVerticalScrollbar = laneContentHeight > laneViewportHeight + 0.5f;
            float totalHeight = RulerHeight + laneViewportHeight + ScrollbarSize;

            Rect outer = GUILayoutUtility.GetRect(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.Height(totalHeight),
                GUILayout.ExpandWidth(true));

            EditorGUI.DrawRect(outer, PanelColor);
            DrawBorder(outer, BorderColor, 1f);

            float contentWidth = outer.width - (hasVerticalScrollbar ? ScrollbarSize : 0f);
            Rect rulerRect = new Rect(outer.x, outer.y, contentWidth, RulerHeight);
            Rect laneViewport = new Rect(outer.x, rulerRect.yMax, contentWidth, laneViewportHeight);
            Rect timeRect = new Rect(
                laneViewport.x + GutterWidth,
                laneViewport.y,
                Math.Max(1f, laneViewport.width - GutterWidth),
                laneViewport.height);
            Rect horizontalScrollbar = new Rect(
                outer.x + GutterWidth,
                laneViewport.yMax,
                Math.Max(1f, contentWidth - GutterWidth),
                ScrollbarSize);

            DrawRuler(rulerRect, timeRect, visibleSpan);
            DrawLanes(laneViewport, timeRect, lanes, laneHeight, visibleSpan);

            double scrollPadding = Math.Max(range.Span, visibleSpan);
            double scrollMinimum = range.Start - scrollPadding;
            double scrollMaximum = range.End + scrollPadding;
            float oldViewStart = (float)viewStart;
            float newViewStart = GUI.HorizontalScrollbar(
                horizontalScrollbar,
                oldViewStart,
                (float)visibleSpan,
                (float)scrollMinimum,
                (float)(scrollMaximum + visibleSpan));
            if (!Mathf.Approximately(oldViewStart, newViewStart))
            {
                viewStart = newViewStart;
                fitView = false;
            }

            if (hasVerticalScrollbar)
            {
                Rect verticalScrollbar = new Rect(
                    laneViewport.xMax,
                    laneViewport.y,
                    ScrollbarSize,
                    laneViewport.height);
                verticalScroll = GUI.VerticalScrollbar(
                    verticalScrollbar,
                    verticalScroll,
                    laneViewportHeight,
                    0f,
                    laneContentHeight);
            }
            else
            {
                verticalScroll = 0f;
            }

            HandleInput(
                outer,
                laneViewport,
                timeRect,
                laneHeight,
                laneContentHeight,
                laneViewportHeight,
                visibleSpan,
                range);
        }

        private void DrawRuler(Rect rulerRect, Rect timeRect, double visibleSpan)
        {
            EditorGUI.DrawRect(rulerRect, RulerColor);
            GUI.Label(
                new Rect(rulerRect.x + 5f, rulerRect.y + 2f, GutterWidth - 8f, rulerRect.height - 4f),
                "Lane",
                EditorStyles.centeredGreyMiniLabel);

            Rect rulerTimeRect = new Rect(timeRect.x, rulerRect.y, timeRect.width, rulerRect.height);
            double tickSpacing = SequenceStripLayout.NiceTickSpacing(visibleSpan, timeRect.width);
            double firstTick = Math.Ceiling(viewStart / tickSpacing) * tickSpacing;
            double viewEnd = viewStart + visibleSpan;

            for (int index = 0; index < 512; index++)
            {
                double tick = firstTick + index * tickSpacing;
                if (tick > viewEnd + tickSpacing * 0.001d)
                    break;

                float x = SequenceStripLayout.TimeToPixel(tick, viewStart, visibleSpan, rulerTimeRect);
                EditorGUI.DrawRect(new Rect(x, rulerRect.yMax - 5f, 1f, 5f), GridColor);
                GUI.Label(
                    new Rect(x + 3f, rulerRect.y + 2f, 64f, rulerRect.height - 4f),
                    FormatTime(tick),
                    EditorStyles.miniLabel);
            }

            EditorGUI.DrawRect(new Rect(rulerRect.x, rulerRect.yMax - 1f, rulerRect.width, 1f), BorderColor);
        }

        private void DrawLanes(
            Rect laneViewport,
            Rect globalTimeRect,
            IReadOnlyList<int> lanes,
            float laneHeight,
            double visibleSpan)
        {
            visuals.Clear();

            GUI.BeginClip(laneViewport);
            Rect localViewport = new Rect(0f, 0f, laneViewport.width, laneViewport.height);
            Rect localTimeRect = new Rect(GutterWidth, 0f, globalTimeRect.width, laneViewport.height);

            for (int laneRow = 0; laneRow < lanes.Count; laneRow++)
            {
                float y = laneRow * laneHeight - verticalScroll;
                if (y > localViewport.yMax || y + laneHeight < localViewport.yMin)
                    continue;

                Color rowColor = laneRow % 2 == 0 ? LaneColorA : LaneColorB;
                EditorGUI.DrawRect(new Rect(0f, y, localViewport.width, laneHeight), rowColor);
                EditorGUI.DrawRect(new Rect(0f, y + laneHeight - 1f, localViewport.width, 1f), BorderColor);
                GUI.Label(
                    new Rect(3f, y, GutterWidth - 6f, laneHeight),
                    lanes[laneRow].ToString(CultureInfo.InvariantCulture),
                    EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUI.DrawRect(new Rect(GutterWidth - 1f, 0f, 1f, localViewport.height), BorderColor);
            DrawGrid(localTimeRect, visibleSpan);

            for (int index = 0; index < bindings.Count; index++)
            {
                SequenceStripBinding binding = bindings[index];
                int laneRow = IndexOfLane(lanes, binding.Data.Lane);
                if (laneRow < 0)
                    continue;

                Rect body = SequenceStripLayout.CalculateItemRect(
                    binding.Data,
                    laneRow,
                    viewStart,
                    visibleSpan,
                    localTimeRect,
                    laneHeight,
                    verticalScroll);

                if (body.yMax < 0f || body.y > localViewport.height ||
                    body.xMax < GutterWidth || body.x > localViewport.width)
                {
                    continue;
                }

                DrawItem(binding, body);

                Rect globalBody = Offset(body, laneViewport.position);
                var visual = new ItemVisual(
                    binding,
                    globalBody,
                    new Rect(globalBody.xMax - ResizeHandleWidth, globalBody.y, ResizeHandleWidth, globalBody.height));
                visuals.Add(visual);
            }

            GUI.EndClip();

            for (int index = 0; index < visuals.Count; index++)
            {
                ItemVisual visual = visuals[index];
                Rect clippedBody = Intersect(visual.Body, laneViewport);
                if (clippedBody.width <= 0f || clippedBody.height <= 0f)
                    continue;

                EditorGUIUtility.AddCursorRect(clippedBody, MouseCursor.MoveArrow);
                if (visual.Binding.CanResize)
                    EditorGUIUtility.AddCursorRect(Intersect(visual.RightHandle, laneViewport), MouseCursor.ResizeHorizontal);
            }
        }

        private void DrawGrid(Rect localTimeRect, double visibleSpan)
        {
            double tickSpacing = SequenceStripLayout.NiceTickSpacing(visibleSpan, localTimeRect.width);
            double firstTick = Math.Ceiling(viewStart / tickSpacing) * tickSpacing;
            double viewEnd = viewStart + visibleSpan;

            for (int index = 0; index < 512; index++)
            {
                double tick = firstTick + index * tickSpacing;
                if (tick > viewEnd + tickSpacing * 0.001d)
                    break;

                float x = SequenceStripLayout.TimeToPixel(tick, viewStart, visibleSpan, localTimeRect);
                Color color = Math.Abs(tick) < tickSpacing * 0.001d ? ZeroLineColor : GridColor;
                EditorGUI.DrawRect(new Rect(x, 0f, 1f, localTimeRect.height), color);
            }
        }

        private void DrawItem(SequenceStripBinding binding, Rect body)
        {
            bool selected = binding.Data.Index == selectedIndex;
            Color color = binding.Data.Color;
            if (selected)
                color = Color.Lerp(color, Color.white, 0.12f);

            EditorGUI.DrawRect(body, color);
            DrawBorder(body, selected ? SelectionColor : Color.black.WithAlpha(0.45f), selected ? 2f : 1f);

            if (body.width >= ResizeHandleWidth * 2f + 2f)
            {
                Color handleColor = Color.white.WithAlpha(selected ? 0.55f : 0.28f);
                EditorGUI.DrawRect(new Rect(body.xMax - ResizeHandleWidth, body.y + 2f, ResizeHandleWidth, body.height - 4f), handleColor);
            }

            EnsureStyles();
            Rect labelRect = new Rect(
                body.x + 4f,
                body.y,
                Math.Max(0f, body.width - ResizeHandleWidth - 8f),
                body.height);
            GUI.Label(labelRect, binding.Data.Label, blockLabelStyle);

            string tooltip = $"{binding.Data.Label}\nStart {FormatTime(binding.Data.Start)}\n" +
                             $"Duration {FormatTime(binding.Data.Duration)}\nLane {binding.Data.Lane}";
            GUI.Label(body, new GUIContent(string.Empty, tooltip), GUIStyle.none);
        }

        private void HandleInput(
            Rect outer,
            Rect laneViewport,
            Rect timeRect,
            float laneHeight,
            float laneContentHeight,
            float laneViewportHeight,
            double visibleSpan,
            SequenceStripTimeRange range)
        {
            Event current = Event.current;
            int controlId = GUIUtility.GetControlID(TimelineControlHint, FocusType.Passive, outer);
            EventType eventType = current.GetTypeForControl(controlId);

            if (eventType == EventType.ScrollWheel && outer.Contains(current.mousePosition))
            {
                bool overLaneGutter = laneViewport.Contains(current.mousePosition) &&
                                      current.mousePosition.x < timeRect.x;
                if (overLaneGutter && laneContentHeight > laneViewportHeight)
                {
                    verticalScroll = Mathf.Clamp(
                        verticalScroll + current.delta.y * laneHeight,
                        0f,
                        laneContentHeight - laneViewportHeight);
                }
                else
                {
                    double anchor = timeRect.Contains(current.mousePosition)
                        ? SequenceStripLayout.PixelToTime(current.mousePosition.x, viewStart, visibleSpan, timeRect)
                        : viewStart + visibleSpan * 0.5d;
                    ZoomAround(range, zoom * Math.Pow(1.12d, -current.delta.y), anchor);
                }

                current.Use();
                GUI.changed = true;
                return;
            }

            if (eventType == EventType.MouseDown && outer.Contains(current.mousePosition))
            {
                if (current.button != 0 || !laneViewport.Contains(current.mousePosition) ||
                    GUIUtility.hotControl != 0)
                {
                    return;
                }

                ItemVisual hit = FindVisual(current.mousePosition, laneViewport);
                if (hit.Binding == null)
                {
                    selectedIndex = -1;
                    current.Use();
                    return;
                }

                selectedIndex = hit.Binding.Data.Index;
                hit.Binding.Element.State.Expanded = true;
                DragMode nextDragMode = DragMode.None;
                if (hit.Binding.CanResize && Intersect(hit.RightHandle, laneViewport).Contains(current.mousePosition))
                    nextDragMode = DragMode.ResizeRight;
                else if (hit.Binding.CanMove)
                    nextDragMode = DragMode.Move;

                if (nextDragMode != DragMode.None)
                {
                    Property.RecordForUndo("Edit sequence item");
                    dragMode = nextDragMode;
                    dragControlId = controlId;
                    dragBinding = hit.Binding;
                    dragPointerTime = SequenceStripLayout.PixelToTime(
                        current.mousePosition.x,
                        viewStart,
                        visibleSpan,
                        timeRect);
                    dragStart = hit.Binding.Data.Start;
                    dragDuration = hit.Binding.Data.Duration;
                    GUIUtility.hotControl = controlId;
                }

                current.Use();
                GUI.changed = true;
                return;
            }

            if (eventType == EventType.MouseDrag && GUIUtility.hotControl == controlId &&
                dragControlId == controlId)
            {
                if (dragBinding != null)
                {
                    ApplyDrag(current, timeRect, visibleSpan);
                }

                current.Use();
                GUI.changed = true;
                return;
            }

            if ((eventType == EventType.MouseUp || eventType == EventType.Ignore) &&
                GUIUtility.hotControl == controlId && dragControlId == controlId)
            {
                GUIUtility.hotControl = 0;
                dragMode = DragMode.None;
                dragControlId = 0;
                dragBinding = null;
                current.Use();
                GUI.changed = true;
            }
        }

        private void ApplyDrag(
            Event current,
            Rect timeRect,
            double visibleSpan)
        {
            double pointerTime = SequenceStripLayout.PixelToTime(
                current.mousePosition.x,
                viewStart,
                visibleSpan,
                timeRect);
            double delta = pointerTime - dragPointerTime;
            bool bypassSnap = current.control || current.command;
            double snap = snapEnabled && !bypassSnap ? Math.Max(0d, Attribute.SnapInterval) : 0d;
            Type durationType = dragBinding.DurationProperty?.ValueEntry?.TypeOfValue;
            double minimumDuration = SequenceValueConverter.ResolveMinimumDuration(
                Attribute.MinimumDuration,
                durationType);

            switch (dragMode)
            {
                case DragMode.Move:
                {
                    double start = SequenceStripLayout.Snap(dragStart + delta, snap);
                    if (!Attribute.AllowNegativeTime)
                        start = Math.Max(0d, start);
                    SetNumber(dragBinding.StartProperty, start);
                    break;
                }
                case DragMode.ResizeRight:
                {
                    double end = SequenceStripLayout.Snap(dragStart + dragDuration + delta, snap);
                    double duration = Math.Max(minimumDuration, end - dragStart);
                    SetNumber(dragBinding.DurationProperty, duration);
                    break;
                }
            }

            Property.MarkSerializationRootDirty();
            Property.Tree.ApplyChanges();
        }

        private static bool SetNumber(InspectorProperty property, double value)
        {
            if (property == null || property.ValueEntry == null || !property.ValueEntry.IsEditable)
                return false;

            if (!SequenceValueConverter.TryConvertNumber(value, property.ValueEntry.TypeOfValue, out object converted))
                return false;

            property.ValueEntry.WeakSmartValue = converted;
            property.ValueEntry.ApplyChanges();
            return true;
        }

        private void Fit(SequenceStripTimeRange range)
        {
            zoom = 1d;
            viewStart = range.Start;
            viewInitialized = true;
            fitView = true;
        }

        private void ZoomAround(SequenceStripTimeRange range, double requestedZoom, double anchor)
        {
            double previousZoom = SequenceStripLayout.ClampZoom(zoom);
            double nextZoom = SequenceStripLayout.ClampZoom(requestedZoom);
            double previousSpan = Math.Max(0.000001d, range.Span / previousZoom);
            double nextSpan = Math.Max(0.000001d, range.Span / nextZoom);
            double anchorRatio = (anchor - viewStart) / previousSpan;

            zoom = nextZoom;
            viewStart = anchor - anchorRatio * nextSpan;
            viewInitialized = true;
            fitView = Math.Abs(zoom - 1d) < 0.000001d &&
                      Math.Abs(viewStart - range.Start) < 0.000001d;
        }

        private double VisibleCenter(SequenceStripTimeRange range)
        {
            if (!viewInitialized)
                return (range.Start + range.End) * 0.5d;

            return viewStart + range.Span / SequenceStripLayout.ClampZoom(zoom) * 0.5d;
        }

        private ItemVisual FindVisual(Vector2 mousePosition, Rect laneViewport)
        {
            for (int index = visuals.Count - 1; index >= 0; index--)
            {
                ItemVisual visual = visuals[index];
                if (Intersect(visual.Body, laneViewport).Contains(mousePosition))
                    return visual;
            }

            return default;
        }

        private void AddIssue(string issue)
        {
            if (issues.Count >= MaximumIssueCount || issues.Contains(issue))
                return;

            issues.Add(issue);
        }

        private void EnsureStyles()
        {
            if (blockLabelStyle != null)
                return;

            blockLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(0, 0, 0, 0)
            };
            blockLabelStyle.normal.textColor = Color.white;
        }

        private static int IndexOfLane(IReadOnlyList<int> lanes, int lane)
        {
            for (int index = 0; index < lanes.Count; index++)
            {
                if (lanes[index] == lane)
                    return index;
            }

            return -1;
        }

        private static string FormatTime(double value)
        {
            double absolute = Math.Abs(value);
            string format = absolute >= 100d ? "0" : absolute >= 10d ? "0.#" : "0.##";
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        private static Color DefaultColor(int index)
        {
            float hue = Mathf.Repeat(index * 0.618034f + 0.08f, 1f);
            return Color.HSVToRGB(hue, 0.58f, EditorGUIUtility.isProSkin ? 0.82f : 0.72f);
        }

        private static Rect Offset(Rect rect, Vector2 offset)
        {
            rect.position += offset;
            return rect;
        }

        private static Rect Intersect(Rect first, Rect second)
        {
            float xMin = Math.Max(first.xMin, second.xMin);
            float yMin = Math.Max(first.yMin, second.yMin);
            float xMax = Math.Min(first.xMax, second.xMax);
            float yMax = Math.Min(first.yMax, second.yMax);
            return xMax <= xMin || yMax <= yMin
                ? new Rect(xMin, yMin, 0f, 0f)
                : Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        private static void DrawBorder(Rect rect, Color color, float width)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, width), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - width, rect.width, width), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, width, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - width, rect.y, width, rect.height), color);
        }

        private static Color PanelColor => EditorGUIUtility.isProSkin
            ? new Color(0.12f, 0.12f, 0.12f, 1f)
            : new Color(0.84f, 0.84f, 0.84f, 1f);

        private static Color RulerColor => EditorGUIUtility.isProSkin
            ? new Color(0.16f, 0.16f, 0.16f, 1f)
            : new Color(0.78f, 0.78f, 0.78f, 1f);

        private static Color LaneColorA => EditorGUIUtility.isProSkin
            ? new Color(0.135f, 0.135f, 0.135f, 1f)
            : new Color(0.88f, 0.88f, 0.88f, 1f);

        private static Color LaneColorB => EditorGUIUtility.isProSkin
            ? new Color(0.15f, 0.15f, 0.15f, 1f)
            : new Color(0.83f, 0.83f, 0.83f, 1f);

        private static Color BorderColor => EditorGUIUtility.isProSkin
            ? new Color(0f, 0f, 0f, 0.55f)
            : new Color(0f, 0f, 0f, 0.28f);

        private static Color GridColor => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.1f)
            : new Color(0f, 0f, 0f, 0.12f);

        private static Color ZeroLineColor => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.32f)
            : new Color(0f, 0f, 0f, 0.3f);

        private static Color SelectionColor => new Color(1f, 0.72f, 0.18f, 1f);

        private sealed class SequenceStripBinding
        {
            public SequenceStripBinding(
                SequenceStripItemData data,
                InspectorProperty element,
                InspectorProperty startProperty,
                InspectorProperty durationProperty)
            {
                Data = data;
                Element = element;
                StartProperty = startProperty;
                DurationProperty = durationProperty;
            }

            public SequenceStripItemData Data { get; }

            public InspectorProperty Element { get; }

            public InspectorProperty StartProperty { get; }

            public InspectorProperty DurationProperty { get; }

            public bool CanMove => StartProperty?.ValueEntry?.IsEditable == true;

            public bool CanResize => DurationProperty?.ValueEntry?.IsEditable == true;
        }

        private struct ItemVisual
        {
            public ItemVisual(
                SequenceStripBinding binding,
                Rect body,
                Rect rightHandle)
            {
                Binding = binding;
                Body = body;
                RightHandle = rightHandle;
            }

            public SequenceStripBinding Binding { get; }

            public Rect Body { get; }

            public Rect RightHandle { get; }
        }

        private enum DragMode
        {
            None,
            Move,
            ResizeRight
        }
    }

    internal static class SequenceStripColorExtensions
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
#endif
