# Autonomous art pipeline

The built-in OpenAI image generation tool produced the canonical character sheet,
the reference-based three-pose master, and the independent effect sheet. Their
final prompts are recorded in `prompts.json`. No API credentials are needed to rebuild.

`Assets/Character/Rick/reference/canonical.png` establishes the identity.
`base_poses.png` is its production derivative. Two attempts to generate additional
seated masters were rejected by the image service; seated and facial poses are
therefore derived automatically from the approved master using the cutout rig.

Run `python tools/art_pipeline/build.py` from any working directory. Processing:
chroma key → connected-component cleanup → crop → nearest-neighbor sizing →
24-color quantization → bottom-center placement → articulated animation →
RGBA sheets and metadata → review GIFs/contact sheet → technical validation.

Character frames are 128×128; the neutral character is 92 pixels tall. The ground
anchor is (64,120). Airborne animation offsets are allowed within a bounded range.
The runtime handles direction flipping, portal opacity, and effect positioning.
Effect sheets share the 128×128 coordinate system so they can be independently layered.

Technical checks reject empty, clipped, misaligned, out-of-palette, incorrectly
sized or non-RGBA frames. Visual review additionally identified and repaired
disconnected side-pose specks, eyelid geometry, and seated trouser color.
Source identity is retained rather than asking a generator to reinvent each frame.

`python -m unittest discover -s tools/art_pipeline -p 'test_*.py'` checks final
assets, frame counts, previews, icon sizes, and reproducibility of extracted masters.
