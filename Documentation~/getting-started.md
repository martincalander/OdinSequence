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
public List<FighterTimelineEvent> Attack;
```

The values may use any built-in numeric type. Integer fields work, but their
timeline edits are rounded to whole values.

## 4. Add presentation paths

Lane, label, and color are optional:

```csharp
[SequenceStrip(
    nameof(FighterTimelineEvent.Start),
    nameof(FighterTimelineEvent.Duration),
    LaneMember = nameof(FighterTimelineEvent.Track),
    LabelMember = nameof(FighterTimelineEvent.Label),
    ColorMember = nameof(FighterTimelineEvent.Color),
    SnapInterval = 1d / 60d)]
public List<FighterTimelineEvent> Attack;
```

Open the component in Odin Inspector. The strip appears above the normal list.
If configuration is incomplete, the Inspector reports the issue and leaves the
normal list available for repair.

The **Fighter Move Sequence** Package Manager sample uses lanes for animation,
hitboxes, hurtboxes, camera, VFX, and audio. Its records show the startup,
active, and recovery phases of a fighting-game attack.

## 5. Connect runtime playback

Odin Sequence is the authoring surface. Your runtime code owns the clock,
evaluates the timed records, and invokes the appropriate animation, hitbox,
hurtbox, camera, VFX, or audio system. This keeps the data model and playback
rules specific to your game.
