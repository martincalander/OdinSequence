# Configuration and member paths

All paths are relative to one list element. Separate nested members with dots.
Path matching is case-sensitive and follows the members exposed in Odin's
public property tree.

## Attribute options

| Option | Default | Purpose |
| --- | --- | --- |
| `StartMember` | Required | Numeric start-time path |
| `DurationMember` | Required | Numeric duration path |
| `LaneMember` | None | Numeric lane path |
| `LabelMember` | None | Text shown on each block |
| `ColorMember` | None | `Color` or `Color32` block tint |
| `SnapInterval` | `0.1` | Drag and resize grid; zero disables it |
| `MinimumDuration` | `0.01` | Smallest duration produced by resize; rounded up for integer duration members |
| `LaneHeight` | `28` | Height of one lane in pixels |
| `MaximumHeight` | `220` | Maximum visible lane area before scrolling |
| `ShowList` | `true` | Initial state of the normal list drawer |
| `AllowNegativeTime` | `true` | Whether blocks may be moved before zero |

The toolbar can temporarily change snapping and list visibility without
modifying the attribute. Fit and zoom are local Inspector view state.

## Editing rules

- Move writes the start member.
- Right resize writes duration.
- Unity undo is recorded once when an edit gesture starts.

Multi-object selection is intentionally left to Odin's normal list because
different targets may have different element counts and paths.
