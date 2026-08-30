# Changelog

All notable changes to this package are documented here.

## [Unreleased]

### Changed

- Replaced the Basic Sequence sample with a Fighter Move Sequence covering startup, hitbox and hurtbox windows, recovery, camera, VFX, and audio timing.
- Expanded the documentation with short cinematic, fighting-game, and combat ability use cases and clarified that projects provide runtime playback.

## [0.1.0] - 2026-08-30

### Added

- `SequenceStripAttribute` for arbitrary `IList` record models.
- Start, duration, lane, label, and color member paths.
- Compact lane timeline with selection, horizontal zoom, scrolling, and fit controls.
- Drag-to-move, right-edge resize, and configurable snapping.
- Unity undo and Odin property-tree change application.
- Normal list fallback with inline configuration errors.
- Basic sample, package documentation, and focused EditMode tests.

### Changed

- Gated Odin-dependent Editor code and tests with the canonical `ODIN_INSPECTOR` symbol.
- Limited the Editor assembly to its required Sirenix precompiled references.
