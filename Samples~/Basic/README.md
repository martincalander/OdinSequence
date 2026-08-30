# Fighter Move Sequence

Add `Fighter` to a GameObject and open it in Odin Inspector. The sample lays out
a fighting-game attack across animation, hitbox, hurtbox, camera, VFX, and audio
lanes. Its records cover startup, the active hitbox and counterable hurtbox
window, impact feedback, and recovery with 1/60-second snapping.

Odin Sequence edits these records but does not play them. Connect the list to
your project's combat, animation, camera, VFX, and audio runtime systems.
The sample assembly is compiled only while `ODIN_INSPECTOR` is active.

Try these interactions:

- Drag the middle of a block to change its start time.
- Drag the right edge to resize it.
- Hold Control or Command while dragging to bypass snapping.
- Use the mouse wheel over the strip to zoom.
- Adjust startup, active, and recovery durations to tune the move's feel.
