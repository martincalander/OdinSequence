# Odin Sequence

Odin Sequence turns any `IList` of timed records into a compact editable
timeline in the Inspector. It works with existing data models. No timeline base
class, interface, or custom editor window is required.

## Requirements

- Unity 6000.4 or newer
- Odin Inspector 4.0 or newer

Odin Inspector is a separately licensed Asset Store product and is not included
in this package.

The runtime attribute has no Odin assembly dependency. The Editor integration
is enabled only when Odin's canonical `ODIN_INSPECTOR` symbol is active.

## Install

In Unity Package Manager, choose **Add package from git URL** and enter:

```text
https://github.com/martincalander/OdinSequence.git
```

Install and activate Odin Inspector to enable the sequence strip drawer.

## Use

```csharp
using System;
using System.Collections.Generic;
using MartinCalander.OdinSequence;
using UnityEngine;

public sealed class Encounter : MonoBehaviour
{
    [SequenceStrip(
        nameof(Cue.Start),
        nameof(Cue.Duration),
        LaneMember = nameof(Cue.Track),
        LabelMember = nameof(Cue.Name),
        ColorMember = nameof(Cue.Color),
        SnapInterval = 0.25d)]
    public List<Cue> Cues = new List<Cue>();
}

[Serializable]
public sealed class Cue
{
    public string Name;
    public float Start;
    public float Duration = 1f;
    public int Track;
    public Color Color = Color.white;
}
```

The start and duration paths are required. Lane, label, and color paths are
optional. Paths are relative to each list element and may use dots, such as
`Timing.Start`.

## Interactions

- Click a block to select it.
- Drag a block to move it in time.
- Drag the right edge to resize it.
- Hold Control or Command while dragging to bypass snapping.
- Scroll over the strip or use the toolbar to zoom.
- Choose **Fit** to frame the complete sequence.
- Toggle **List** to show or hide Odin's normal list drawer.

Every timeline edit uses Odin's public property APIs, supports Unity undo, and
marks the serialization root dirty. Invalid paths and unsupported member types
are reported beside the strip while the normal list stays available.

## Supported members

| Role | Supported value |
| --- | --- |
| Start | Any built-in integral, floating-point, or decimal type |
| Duration | Any built-in integral, floating-point, or decimal type |
| Lane | Any supported numeric type, rounded to the nearest lane number |
| Label | String or any value with a useful invariant string conversion |
| Color | `Color` or `Color32` |

See [the documentation](Documentation~/index.md) and import the **Basic
Sequence** sample from Package Manager for a complete example.

## License

MIT. See [LICENSE.md](LICENSE.md).
