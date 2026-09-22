using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Samples.Blobatar.InnerWorkings
{
    /// <summary>
    /// A minimal software rasterizer: fills simple (non-self-intersecting)
    /// closed polygons, given in the 0..100 design space, into a per-pixel
    /// coverage buffer with supersampled anti-aliasing, then alpha-composites
    /// a flat color over a pixel buffer using that coverage. This replaces the
    /// SVG renderer the original library targets, since Unity needs pixels.
    /// </summary>
    public static class BlobatarRaster
    {
        /// <summary>
        /// Rasterizes a set of simple closed polygons (each in 0..100 design
        /// space, sharing one fill color) into a coverage buffer already laid
        /// out in Unity's bottom-up pixel row order, so it can be composited
        /// directly. Overlapping polygons combine via max (not sum), so two
        /// anti-aliased edges overlapping doesn't darken the seam.
        ///
        /// Uses a scanline fill (edge crossings + a left-to-right sweep) instead
        /// of testing every supersample point against every polygon vertex.
        /// Produces identical coverage to the old brute-force point-in-polygon
        /// approach, but replaces an O(vertices) test per sample with an O(1)
        /// amortized sweep, which is what made large sizes / high supersampling
        /// so slow before.
        /// </summary>
        public static float[] RasterizeLayer(int size, List<List<Vector2>> polys, int supersample = 3)
        {
            var cov = new float[size * size];
            float scale = 100f / size;
            int ss = Mathf.Max(1, supersample);
            float invSS2 = 1f / (ss * ss);

            var crossings = new List<float>(32);

            foreach (var poly in polys)
            {
                if (poly == null || poly.Count < 3) continue;
                int n = poly.Count;

                float minx = float.MaxValue, maxx = float.MinValue, miny = float.MaxValue, maxy = float.MinValue;
                foreach (var p in poly)
                {
                    if (p.x < minx) minx = p.x;
                    if (p.x > maxx) maxx = p.x;
                    if (p.y < miny) miny = p.y;
                    if (p.y > maxy) maxy = p.y;
                }

                int px0 = Mathf.Max(0, Mathf.FloorToInt(minx / scale) - 1);
                int px1 = Mathf.Min(size - 1, Mathf.CeilToInt(maxx / scale) + 1);
                int py0 = Mathf.Max(0, Mathf.FloorToInt(miny / scale) - 1);
                int py1 = Mathf.Min(size - 1, Mathf.CeilToInt(maxy / scale) + 1);
                if (px1 < px0 || py1 < py0) continue;

                int width = px1 - px0 + 1;
                var hitCounts = new int[width];

                for (int py = py0; py <= py1; py++)
                {
                    System.Array.Clear(hitCounts, 0, width);

                    for (int sy = 0; sy < ss; sy++)
                    {
                        float wy = (py + (sy + 0.5f) / ss) * scale;

                        // Find where every edge crosses this sub-scanline (even-odd rule),
                        // same test as before, but done once per row instead of once per sample.
                        crossings.Clear();
                        int j = n - 1;
                        for (int i = 0; i < n; i++)
                        {
                            float yi = poly[i].y, yj = poly[j].y;
                            if ((yi > wy) != (yj > wy))
                            {
                                float xi = poly[i].x, xj = poly[j].x;
                                crossings.Add(xi + (wy - yi) * (xj - xi) / (yj - yi + 1e-12f));
                            }
                            j = i;
                        }
                        if (crossings.Count == 0) continue;
                        crossings.Sort();

                        // Sweep sub-samples across the row left-to-right, advancing past
                        // crossings as we go. wx is monotonically increasing across the
                        // combined (px, sx) loop, so this single pass is exact.
                        int ci = 0;
                        bool inside = false;
                        for (int px = 0; px < width; px++)
                        {
                            int hit = 0;
                            for (int sx = 0; sx < ss; sx++)
                            {
                                float wx = ((px0 + px) + (sx + 0.5f) / ss) * scale;
                                while (ci < crossings.Count && crossings[ci] <= wx) { inside = !inside; ci++; }
                                if (inside) hit++;
                            }
                            hitCounts[px] += hit;
                        }
                    }

                    // Flip here: py is top-down design space, but the output buffer
                    // must match Unity's bottom-up Texture2D pixel row order.
                    int texRow = size - 1 - py;
                    int rowBase = texRow * size;
                    for (int px = 0; px < width; px++)
                    {
                        float c = hitCounts[px] * invSS2;
                        int idx = rowBase + px0 + px;
                        if (c > cov[idx]) cov[idx] = c;
                    }
                }
            }
            return cov;
        }

        /// <summary>Standard "over" alpha compositing of a flat color onto a pixel buffer using per-pixel coverage as source alpha.</summary>
        public static void Composite(Color32[] pixels, float[] cov, Color color)
        {
            byte r = (byte)Mathf.RoundToInt(Mathf.Clamp01(color.r) * 255f);
            byte g = (byte)Mathf.RoundToInt(Mathf.Clamp01(color.g) * 255f);
            byte b = (byte)Mathf.RoundToInt(Mathf.Clamp01(color.b) * 255f);

            for (int i = 0; i < pixels.Length; i++)
            {
                float sa = cov[i];
                if (sa <= 0f) continue;
                var d = pixels[i];
                float da = d.a / 255f;
                float outA = sa + da * (1f - sa);
                if (outA <= 0f) { pixels[i] = new Color32(0, 0, 0, 0); continue; }
                float outR = (r * sa + d.r * da * (1f - sa)) / outA;
                float outG = (g * sa + d.g * da * (1f - sa)) / outA;
                float outB = (b * sa + d.b * da * (1f - sa)) / outA;
                pixels[i] = new Color32(
                    (byte)Mathf.RoundToInt(outR), (byte)Mathf.RoundToInt(outG),
                    (byte)Mathf.RoundToInt(outB), (byte)Mathf.RoundToInt(outA * 255f));
            }
        }
    }
}