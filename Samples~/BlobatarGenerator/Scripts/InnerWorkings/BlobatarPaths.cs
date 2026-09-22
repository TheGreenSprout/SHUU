using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Samples.Blobatar.InnerWorkings
{
    /// <summary>
    /// Low-level curve generators. The original library emits SVG path strings;
    /// here every shape is instead flattened directly into a closed polyline
    /// (a List&lt;Vector2&gt; in the 0..100 design space) ready for rasterizing.
    /// Ported from shape.ts.
    /// </summary>
    public static class BlobatarPaths
    {
        public static List<Vector2> CubicFlat(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int seg = 16)
        {
            var pts = new List<Vector2>(seg + 1);
            for (int i = 0; i <= seg; i++)
            {
                float t = (float)i / seg;
                float mt = 1f - t;
                float x = mt * mt * mt * p0.x + 3f * mt * mt * t * p1.x + 3f * mt * t * t * p2.x + t * t * t * p3.x;
                float y = mt * mt * mt * p0.y + 3f * mt * mt * t * p1.y + 3f * mt * t * t * p2.y + t * t * t * p3.y;
                pts.Add(new Vector2(x, y));
            }
            return pts;
        }

        public static List<Vector2> QuadFlat(Vector2 p0, Vector2 p1, Vector2 p2, int seg = 12)
        {
            var pts = new List<Vector2>(seg + 1);
            for (int i = 0; i <= seg; i++)
            {
                float t = (float)i / seg;
                float mt = 1f - t;
                float x = mt * mt * p0.x + 2f * mt * t * p1.x + t * t * p2.x;
                float y = mt * mt * p0.y + 2f * mt * t * p1.y + t * t * p2.y;
                pts.Add(new Vector2(x, y));
            }
            return pts;
        }

        /// <summary>
        /// A rounded-square-ish "squircle" shape. n=2 is an ellipse, n=4 a
        /// classic squircle, larger n approaches a rectangle.
        /// </summary>
        public static List<Vector2> Superellipse(float cx, float cy, float rx, float ry, float n = 4f, float rot = 0f, int segPerQuadrant = 16)
        {
            float k = Mathf.Min(1f, (8f * Mathf.Pow(2f, -1f / n) - 4f) / 3f);
            float a = rx, b = ry, ak = a * k, bk = b * k;
            Vector2[] ctrlLocal =
            {
                new Vector2(a, 0), new Vector2(a, bk), new Vector2(ak, b), new Vector2(0, b),
                new Vector2(-ak, b), new Vector2(-a, bk), new Vector2(-a, 0),
                new Vector2(-a, -bk), new Vector2(-ak, -b), new Vector2(0, -b),
                new Vector2(ak, -b), new Vector2(a, -bk), new Vector2(a, 0),
            };
            float t0 = rot * Mathf.Deg2Rad;
            float cos = Mathf.Cos(t0), sin = Mathf.Sin(t0);
            var ctrl = new Vector2[13];
            for (int i = 0; i < 13; i++)
            {
                var p = ctrlLocal[i];
                ctrl[i] = new Vector2(cx + p.x * cos - p.y * sin, cy + p.x * sin + p.y * cos);
            }
            var poly = new List<Vector2> { ctrl[0] };
            for (int i = 1; i < 13; i += 3)
            {
                var seg = CubicFlat(ctrl[i - 1], ctrl[i], ctrl[i + 1], ctrl[i + 2], segPerQuadrant);
                for (int j = 1; j < seg.Count; j++) poly.Add(seg[j]);
            }
            return poly;
        }

        /// <summary>A smooth closed spline through `radii.Count` control vertices (Catmull-Rom style).</summary>
        public static List<Vector2> BlobPath(float cx, float cy, float rx, float ry, IReadOnlyList<float> radii, float rot = 0f, int seg = 16)
        {
            int n = radii.Count;
            float t0 = rot * Mathf.Deg2Rad;
            var p = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                float a = t0 + (2f * Mathf.PI * i) / n;
                p[i] = new Vector2(cx + rx * radii[i] * Mathf.Cos(a), cy + ry * radii[i] * Mathf.Sin(a));
            }

            Vector2 At(int i) => p[((i % n) + n) % n];

            var poly = new List<Vector2> { At(0) };
            for (int i = 0; i < n; i++)
            {
                Vector2 p0 = At(i - 1), p1 = At(i), p2 = At(i + 1), p3 = At(i + 2);
                Vector2 c1 = new Vector2(p1.x + (p2.x - p0.x) / 6f, p1.y + (p2.y - p0.y) / 6f);
                Vector2 c2 = new Vector2(p2.x - (p3.x - p1.x) / 6f, p2.y - (p3.y - p1.y) / 6f);
                var segPts = CubicFlat(poly[poly.Count - 1], c1, c2, p2, seg);
                for (int j = 1; j < segPts.Count; j++) poly.Add(segPts[j]);
            }
            return poly;
        }

        /// <summary>A regular polygon with optionally rounded corners (round: 0 = sharp, 1 = fully rounded).</summary>
        public static List<Vector2> Polygon(float cx, float cy, float rx, float ry, int sides, float round = 0.3f, float rot = 0f, int seg = 12)
        {
            float k = 0f;
            if (round > 0f) k = round < 1f ? round / 2f : 0.5f;
            float t0 = rot * Mathf.Deg2Rad - Mathf.PI / 2f;
            var v = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float a = t0 + (2f * Mathf.PI * i) / sides;
                v[i] = new Vector2(cx + rx * Mathf.Cos(a), cy + ry * Mathf.Sin(a));
            }

            Vector2 At(int i) => v[((i % sides) + sides) % sides];
            Vector2 Cut(int i, int j)
            {
                var p0 = At(i);
                var p1 = At(j);
                return new Vector2(p0.x + (p1.x - p0.x) * k, p0.y + (p1.y - p0.y) * k);
            }

            var poly = new List<Vector2> { Cut(0, -1) };
            for (int i = 0; i < sides; i++)
            {
                var c = Cut(i, i + 1);
                var segPts = QuadFlat(poly[poly.Count - 1], At(i), c, seg);
                for (int j = 1; j < segPts.Count; j++) poly.Add(segPts[j]);
                if (k < 0.5f) poly.Add(Cut(i + 1, i));
            }
            return poly;
        }

        public static List<Vector2> Box(float cx, float cy, float rx, float ry)
        {
            float l = cx - rx, r = cx + rx, top = cy - ry, bot = cy + ry;
            return new List<Vector2> { new Vector2(l, top), new Vector2(r, top), new Vector2(r, bot), new Vector2(l, bot) };
        }

        /// <summary>A rounded base tapering to a point (used by the "droplet" shape's tail).</summary>
        public static List<Vector2> Taper(float cx, float cy, float rx, float ry, float tip)
        {
            float t = Mathf.Max(1.05f, tip);
            float tx = rx * Mathf.Sqrt(1f - 1f / (t * t));
            float ty = cy - ry / t;
            float apex = cy - t * ry;
            float px = tx * 0.14f;
            float py = ty + 0.86f * (apex - ty);
            Vector2 p0 = new Vector2(cx - tx, ty);
            Vector2 p1 = new Vector2(cx - px, py);
            Vector2 pq = new Vector2(cx, apex);
            Vector2 p2 = new Vector2(cx + px, py);
            Vector2 p3 = new Vector2(cx + tx, ty);

            var poly = new List<Vector2> { p0, p1 };
            var mid = QuadFlat(p1, pq, p2, 10);
            for (int i = 1; i < mid.Count; i++) poly.Add(mid[i]);
            poly.Add(p3);
            return poly;
        }
    }
}
