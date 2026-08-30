# Odin Sequence

Odin Sequence adds an editable lane timeline above a normal Odin list. It is a
good fit for short cinematics, fighting-game moves, hitbox and hurtbox windows,
combat abilities, animation phases, camera shots, VFX, audio regions, spawn
schedules, tutorial steps, and any other records with a start and duration.

## Topics

- [Getting started](getting-started.md)
- [Configuration and member paths](configuration.md)

## Compatibility

The package targets Unity 6000.4 or newer and Odin Inspector 4.0 or newer. It
uses the public Odin drawer and property-tree APIs. Odin is required in the
project but is not redistributed by this package.

The runtime, Editor, test, and sample assemblies compile only while Odin's
`ODIN_INSPECTOR` symbol is active. Without Odin installed, the package adds no
compiled assemblies to the project.

## Scope

Odin Sequence edits start and duration values and uses the optional lane value
to arrange records. It authors timing data but does not run a sequence. Runtime
playback belongs to the project, which can evaluate the records and route them
to animation, combat, camera, VFX, audio, or cinematic systems.

The normal Odin list remains the detailed editor for all other fields and for
structural operations such as adding, removing, and reordering records.
