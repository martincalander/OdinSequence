# Getting started

## 1. Install requirements

Use Unity 6000.4 or newer and install Odin Inspector 4.0 or newer. The runtime
attribute can compile without Odin, but the sequence strip drawer is enabled
only while Odin's `ODIN_INSPECTOR` symbol is active.

## 2. Install the package

Add the Git URL in Unity Package Manager:

```text
https://github.com/martincalander/OdinSequence.git
```

## 3. Add the attribute

Apply `SequenceStripAttribute` to a field or property whose value implements
`IList`. Give it the element member paths for start and duration:

```csharp
[SequenceStrip("Start", "Duration")]
public List<Cue> Cues;
```

The values may use any built-in numeric type. Integer fields work, but their
timeline edits are rounded to whole values.

## 4. Add presentation paths

Lane, label, and color are optional:

```csharp
[SequenceStrip(
    "Timing.Start",
    "Timing.Duration",
    LaneMember = "Track",
    LabelMember = "DisplayName",
    ColorMember = "Tint")]
public List<Cue> Cues;
```

Open the component in Odin Inspector. The strip appears above the normal list.
If configuration is incomplete, the Inspector reports the issue and leaves the
normal list available for repair.
