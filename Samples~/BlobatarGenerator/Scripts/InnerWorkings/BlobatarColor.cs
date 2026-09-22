using UnityEngine;

namespace SHUU.Samples.Blobatar.InnerWorkings
{
    /// <summary>A color in the OKLCH perceptual color space (lightness, chroma, hue in degrees).</summary>
    public struct Oklch
    {
        public float l, c, h;
        public Oklch(float l, float c, float h) { this.l = l; this.c = c; this.h = h; }
        public Oklch With(float? l = null, float? c = null, float? h = null) => new Oklch(l ?? this.l, c ?? this.c, h ?? this.h);
    }

    /// <summary>A target hue/lightness/chroma an expression can pull the palette toward (e.g. anger -> red).</summary>
    public struct Tint
    {
        public float h, l, pull, c;
        public Tint(float h, float l, float pull, float c) { this.h = h; this.l = l; this.pull = pull; this.c = c; }
    }

    /// <summary>
    /// OKLCH-based palette generation with WCAG-style contrast enforcement, and
    /// the mood-tinting math used by colorful expressions (mad/love/shy/sick).
    /// Ported from color.ts. Unlike the original, this works in OKLCH/linear-sRGB
    /// floats throughout rather than round-tripping through hex strings, since
    /// there's no CSS to hand off to here — the result is the same palette, just
    /// without the incidental 8-bit quantization of the web version.
    /// </summary>
    public static class BlobatarColor
    {
        public static readonly Tint HOT = new Tint(27f, 0.58f, 0.6f, 0.18f);
        public static readonly Tint ROSE = new Tint(358f, 0.72f, 0.55f, 0.16f);
        public static readonly Tint BLUSH = new Tint(12f, 0.84f, 0.4f, 0.1f);
        public static readonly Tint BILE = new Tint(142f, 0.66f, 0.6f, 0.13f);

        const float TintFloor = 4.55f;
        static readonly Oklch DarkSurface = new Oklch(0.145f, 0f, 0f);
        const float SurfaceFloor = 1.5f;

        static Vector3 ToLinear(Oklch o)
        {
            float r = o.h * Mathf.Deg2Rad;
            float a = o.c * Mathf.Cos(r);
            float b = o.c * Mathf.Sin(r);
            float l_ = o.l + 0.3963377774f * a + 0.2158037573f * b;
            float m_ = o.l - 0.1055613458f * a - 0.0638541728f * b;
            float s_ = o.l - 0.0894841775f * a - 1.291485548f * b;
            float L = l_ * l_ * l_, M = m_ * m_ * m_, S = s_ * s_ * s_;
            return new Vector3(
                4.0767416621f * L - 3.3077115913f * M + 0.2309699292f * S,
                -1.2684380046f * L + 2.6097574011f * M - 0.3413193965f * S,
                -0.0041960863f * L - 0.7034186147f * M + 1.707614701f * S);
        }

        static bool InGamut(Vector3 rgb) =>
            rgb.x >= -1e-4f && rgb.x <= 1f + 1e-4f &&
            rgb.y >= -1e-4f && rgb.y <= 1f + 1e-4f &&
            rgb.z >= -1e-4f && rgb.z <= 1f + 1e-4f;

        /// <summary>Resolves an OKLCH color to displayable linear RGB, reducing chroma if needed to stay in gamut.</summary>
        static Vector3 Resolve(Oklch o)
        {
            Vector3 rgb = ToLinear(o);
            if (!InGamut(rgb))
            {
                float lo = 0f, hi = o.c;
                for (int i = 0; i < 12; i++)
                {
                    float mid = (lo + hi) / 2f;
                    if (InGamut(ToLinear(o.With(c: mid)))) lo = mid; else hi = mid;
                }
                rgb = ToLinear(o.With(c: lo));
            }
            return new Vector3(Mathf.Clamp01(rgb.x), Mathf.Clamp01(rgb.y), Mathf.Clamp01(rgb.z));
        }

        static float Luminance(Oklch o)
        {
            var rgb = Resolve(o);
            return 0.2126f * rgb.x + 0.7152f * rgb.y + 0.0722f * rgb.z;
        }

        /// <summary>WCAG-style contrast ratio between two colors (1 = identical, 21 = max).</summary>
        public static float Contrast(Oklch a, Oklch b)
        {
            float x = Luminance(a), y = Luminance(b);
            return (Mathf.Max(x, y) + 0.05f) / (Mathf.Min(x, y) + 0.05f);
        }

        /// <summary>Nudges fg's lightness (away from bg, then toward it, then to pure black/white) until the contrast ratio is met.</summary>
        public static Oklch EnsureContrast(Oklch fg, Oklch bg, float min)
        {
            if (Contrast(fg, bg) >= min) return fg;
            float lean = fg.l >= bg.l ? 1f : -1f;
            foreach (var dir in new[] { lean, -lean })
            {
                Oklch probe = fg;
                for (int i = 0; i < 60; i++)
                {
                    float newL = Mathf.Clamp01(probe.l + dir * 0.02f);
                    probe = probe.With(l: newL);
                    if (Contrast(probe, bg) >= min) return probe;
                    if (newL <= 0f || newL >= 1f) break;
                }
            }
            Oklch black = fg.With(l: 0f, c: 0f);
            Oklch white = fg.With(l: 1f, c: 0f);
            return Contrast(black, bg) >= Contrast(white, bg) ? black : white;
        }

        static float GammaEncode(float v) => v <= 0.0031308f ? 12.92f * v : 1.055f * Mathf.Pow(v, 1f / 2.4f) - 0.055f;

        /// <summary>Converts to a Unity sRGB Color (alpha = 1).</summary>
        public static Color ToColor(Oklch o)
        {
            var rgb = Resolve(o);
            return new Color(
                Mathf.Clamp01(GammaEncode(rgb.x)),
                Mathf.Clamp01(GammaEncode(rgb.y)),
                Mathf.Clamp01(GammaEncode(rgb.z)), 1f);
        }

        /// <summary>Linear interpolation of two OKLCH colors in their (L, a, b) cartesian form.</summary>
        public static Oklch Mix(Oklch a, Oklch b, float t)
        {
            float ar = a.h * Mathf.Deg2Rad, br = b.h * Mathf.Deg2Rad;
            float ax = a.c * Mathf.Cos(ar), ay = a.c * Mathf.Sin(ar);
            float bx = b.c * Mathf.Cos(br), by = b.c * Mathf.Sin(br);
            float x = ax + (bx - ax) * t;
            float y = ay + (by - ay) * t;
            return new Oklch(a.l + (b.l - a.l) * t, Mathf.Sqrt(x * x + y * y), Mathf.Atan2(y, x) * Mathf.Rad2Deg);
        }

        /// <summary>
        /// Computes the "fully tinted" head/eye colors for a mood (e.g. anger),
        /// guaranteeing the eye stays readable against the head at every point
        /// along the base-color -&gt; tinted-color mix (not just the endpoints).
        /// </summary>
        public static (Oklch head, Oklch eye) Tinted(Oklch head, Oklch eye, Tint tgt)
        {
            Oklch hotHead = new Oklch(head.l + (tgt.l - head.l) * tgt.pull, Mathf.Max(head.c, tgt.c), tgt.h);
            hotHead = EnsureContrast(hotHead, DarkSurface, SurfaceFloor);
            Oklch hotEye = EnsureContrast(eye, hotHead, TintFloor);

            float dir = hotEye.l >= hotHead.l ? 1f : -1f;
            for (int pass = 0; pass < 40; pass++)
            {
                float worst = float.PositiveInfinity;
                for (int i = 0; i <= 10; i++)
                {
                    float tt = i / 10f;
                    float c = Contrast(Mix(eye, hotEye, tt), Mix(head, hotHead, tt));
                    if (c < worst) worst = c;
                }
                if (worst >= TintFloor) return (hotHead, hotEye);
                float newL = Mathf.Clamp01(hotEye.l + dir * 0.02f);
                if (Mathf.Approximately(newL, hotEye.l)) return (hotHead, hotEye);
                hotEye = hotEye.With(l: newL);
            }
            return (hotHead, hotEye);
        }

        static readonly (float edge, float l, float c)[] Tones =
        {
            (0.2f, 0.86f, 0.085f),
            (0.36f, 0.9f, 0.028f),
            (0.62f, 0.73f, 0.135f),
            (0.8f, 0.62f, 0.165f),
            (0.93f, 0.87f, 0.16f),
            (1.0f, 0.34f, 0.035f),
        };

        static (float l, float c) ToneAt(float v)
        {
            foreach (var t in Tones) if (v < t.edge) return (t.l, t.c);
            return (Tones[0].l, Tones[0].c);
        }

        /// <summary>
        /// The valid "tone" values (0-1) that actually correspond to a distinct
        /// shade via <see cref="Ramp"/> — the midpoint of each preset band —
        /// paired with a short label for UI pickers. Tone isn't continuous:
        /// every value inside a band produces the exact same shade, so these
        /// are the only settings worth exposing to a slider or dropdown.
        /// </summary>
        public static readonly (string label, float tone)[] ToneOptions = BuildToneOptions();

        static (string label, float tone)[] BuildToneOptions()
        {
            var opts = new (string, float)[Tones.Length];
            float lo = 0f;
            for (int i = 0; i < Tones.Length; i++)
            {
                float hi = Tones[i].edge;
                opts[i] = ($"Shade {i + 1} (L {Tones[i].l:0.00})", (lo + hi) * 0.5f);
                lo = hi;
            }
            return opts;
        }

        public struct Palette { public Oklch bg, head, eye; }

        /// <summary>Builds a background/head/eye palette from a hue (0-360) and tone (0-1, pale to ink), with contrast guarantees enforced.</summary>
        public static Palette Ramp(float hue, bool enforce, float tone)
        {
            var (tl, tc) = ToneAt(tone);
            Oklch head = EnsureContrast(new Oklch(tl, tc, hue), DarkSurface, SurfaceFloor);
            Oklch bg = new Oklch(0.965f, 0.01f, hue);
            Oklch eye = head.l >= 0.5f ? new Oklch(0.17f, 0.02f, hue) : new Oklch(0.97f, 0.012f, hue);
            if (enforce)
            {
                head = EnsureContrast(head, bg, 1.25f);
                eye = EnsureContrast(eye, head, 4.5f);
            }
            return new Palette { bg = bg, head = head, eye = eye };
        }
    }
}