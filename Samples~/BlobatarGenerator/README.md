# Blobatar for Unity

A C# port of [blobatar](https://github.com/) — deterministic, seeded blob
avatars — targeting Unity. Instead of emitting SVG, this generates a
`Texture2D` directly via a small built-in rasterizer, so it works anywhere
Unity runs (including builds with no browser/SVG support).

Same seed in → same avatar out, always. It covers everything the original
does **except CSS-driven idle animation** (blinking, breathing, gaze drift),
since the ask here was a single static image. A facial expression is still
fully supported — it's just baked into one fixed pose instead of animated.

## Quick start

```csharp
using Blobatar;

Texture2D avatar = BlobatarGenerator.GenerateBlobatar("alice@example.com", background: true);
myRawImage.texture = avatar;
```

That's the exact signature you asked for: `GenerateBlobatar(string seed, bool background)`.
There are also optional parameters if you want more control:

```csharp
Texture2D avatar = BlobatarGenerator.GenerateBlobatar(
    seed: "alice@example.com",
    background: true,           // opaque backdrop plate, or transparent if false
    size: 512,                  // texture is size x size pixels
    expression: BlobatarExpr.Happy,
    hue: null,                  // override 0-360 to lock a color, or leave null to derive from the seed
    tone: null,                 // override 0-1 (pale -> ink) to lock a tone, or leave null
    normalize: true,            // trims/lowercases the seed so "Alice" == " alice "
    supersample: 3              // edge anti-aliasing quality (raise for smoother edges, costs perf)
);
```

## What you get

- **10 body shapes**: round, organic, boxy, capsule, nub, cloud, droplet,
  hexagon, sun, triangle — weighted so round/organic are common and the
  louder shapes are rarer, exactly matching the original distribution.
- **Full OKLCH color palette generation** with WCAG-style contrast
  enforcement, so head/eye/background always stay readable against each other.
- **14 expressions**: Idle, Happy, Sad, Mad, Surprised, Wink, Sleepy, Smug,
  Unsure, Scared, Love, Shy, Sick, Thinking. Four of them (Mad, Love, Shy,
  Sick) also tint the palette (angry-red, love-rose, shy-blush, sick-green).
- **Optional background plate** via the `background` bool.
- Pixel-identical, cross-platform determinism — the hashing and math are
  implemented with fixed-width integer/float ops so results don't depend on
  .NET version or platform.

## How it works (file guide)

| File | Purpose |
|---|---|
| `BlobatarHash.cs` | Seed string → deterministic hash state → named trait streams. |
| `BlobatarTraits.cs` | Ergonomic accessors over hash streams (ranges, ints, picks, jitter, overrides). |
| `BlobatarPaths.cs` | Geometry generators (superellipse "squircle", smooth blob spline, rounded polygon, box, teardrop taper) — each flattened straight to a polyline instead of an SVG path string. |
| `BlobatarShapes.cs` | The 10-shape vocabulary: each shape's body tweak, face region, and extra decorations (petals/nubs/tail). |
| `BlobatarCompose.cs` | Picks a shape band from the seed, builds the body, and fits two eyes onto the face without ever overflowing it. |
| `BlobatarColor.cs` | OKLCH color math: gamut mapping, contrast ratio, palette ramp, expression tinting. |
| `BlobatarExpression.cs` | The 14 expression poses and the math that bakes a pose onto eye geometry + a body bob offset. |
| `BlobatarRaster.cs` | The bit that replaces the SVG renderer: fills polygons into a coverage buffer with supersampled AA, then alpha-composites flat colors into pixels. |
| `BlobatarGenerator.cs` | Public API: wires all of the above together into `GenerateBlobatar(...)`. |

## Performance notes

Generation does real rasterization work on the CPU (no GPU/shader path), so
it's meant for "generate once, cache/save the result" use — e.g. when a
profile is created, or on first load, not every frame. At `size: 256` with
the default `supersample: 3` it should take low tens of milliseconds on a
desktop machine; drop `supersample` to 2 or `size` to 128 for faster/rougher
results (e.g. list thumbnails), and consider caching generated textures
(e.g. keyed by seed) rather than regenerating on every UI redraw.

## Determinism / porting notes

- Trait hashing uses the same integer mixer as the original library (ported
  bit-for-bit, using `unchecked` 32-bit arithmetic), so results are stable
  across platforms and .NET versions.
- Color math is done directly in OKLCH/linear-sRGB floats rather than via
  hex-string round-tripping — there's no CSS output here to match byte-for-
  byte, so this avoids incidental 8-bit quantization while producing the same
  palettes.
- Shapes are rasterized as simple (non-self-intersecting) closed polygons;
  overlapping same-color shapes (e.g. capsule end-caps, sun rays) are combined
  by taking the max coverage per pixel, so anti-aliased edges never double up
  and darken a seam.
