using System;
using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Samples.Blobatar.InnerWorkings
{
    /// <summary>Mutable working description of the creature's core silhouette while it's being built.</summary>
    public class Body
    {
        public float cx, cy, rx, ry, n, rot;
        public List<float> radii;
        public int sides = -1;
        public float round = 0.3f;
    }

    public struct Ellipse
    {
        public float cx, cy, rx, ry;
        public Ellipse(float cx, float cy, float rx, float ry) { this.cx = cx; this.cy = cy; this.rx = rx; this.ry = ry; }
    }

    public struct Petal { public float cx, cy, r; }

    public struct ExtraTaper { public float cx, cy, rx, ry, tip; }

    /// <summary>Extra decorations (petals/nubs/tail) a shape can add alongside its core body.</summary>
    public class Deco
    {
        public List<Petal> petals = new List<Petal>();
        public List<ExtraTaper> extra = new List<ExtraTaper>();
    }

    public class ShapeDef
    {
        public string name;
        public float core;
        public Action<Traits, Body> body;
        public Func<Body, Ellipse> face;
        public Action<Traits, Body, Deco> decorate;
        public Func<Body, List<Vector2>> path;
    }

    /// <summary>
    /// The ten-shape vocabulary: round, organic, boxy, capsule, nub, cloud,
    /// droplet, hexagon, sun, triangle. Ported from styles/shapes.ts, with the
    /// weighted band thresholds from styles/blob.ts.
    /// </summary>
    public static class BlobatarShapes
    {
        static Func<Body, Ellipse> Shrunk(float k) => b => new Ellipse(b.cx, b.cy, b.rx * k, b.ry * k);

        static Ellipse SplineFace(Body b)
        {
            float m = float.MaxValue;
            foreach (var r in b.radii) if (r < m) m = r;
            return Shrunk(m * 0.95f)(b);
        }

        static Ellipse PolyFace(Body b) => Shrunk(0.84f)(b);

        static List<Vector2> Spline(Body b) => BlobatarPaths.BlobPath(b.cx, b.cy, b.rx, b.ry, b.radii, b.rot);
        static List<Vector2> Poly(Body b) => BlobatarPaths.Polygon(b.cx, b.cy, b.rx, b.ry, b.sides, b.round, b.rot);

        public static readonly ShapeDef Round = new ShapeDef { name = "round", core = 1f };

        public static readonly ShapeDef Organic = new ShapeDef
        {
            name = "organic",
            core = 0.98f,
            path = Spline,
            face = SplineFace,
        };

        public static readonly ShapeDef Boxy = new ShapeDef
        {
            name = "boxy",
            core = 0.86f,
            body = (t, b) => { b.n = t.Num("body.n", 3.4f, 6f); b.rot = t.Num("body.rot", -20f, 20f); },
        };

        public static readonly ShapeDef Capsule = new ShapeDef
        {
            name = "capsule",
            core = 1.02f,
            body = (t, b) => { b.ry *= t.Num("capsule.squat", 0.55f, 0.68f); },
            face = Shrunk(0.94f),
            decorate = (t, b, o) =>
            {
                foreach (var s in new[] { -1f, 1f })
                    o.petals.Add(new Petal { cx = b.cx + s * (b.rx - b.ry), cy = b.cy, r = b.ry });
            },
            path = b => BlobatarPaths.Box(b.cx, b.cy, b.rx - b.ry, b.ry),
        };

        public static readonly ShapeDef Nub = new ShapeDef
        {
            name = "nub",
            core = 0.88f,
            decorate = (t, b, o) =>
            {
                int count = t.Int("nub.n", 1, 2);
                for (int i = 0; i < count; i++)
                {
                    float a = t.Num($"nub.a{i}", 0f, 2f * Mathf.PI);
                    o.petals.Add(new Petal
                    {
                        cx = b.cx + Mathf.Cos(a) * b.rx * 0.88f,
                        cy = b.cy + Mathf.Sin(a) * b.rx * 0.88f,
                        r = b.rx * t.Num($"nub.r{i}", 0.24f, 0.4f),
                    });
                }
            },
        };

        public static readonly ShapeDef Cloud = new ShapeDef
        {
            name = "cloud",
            core = 0.78f,
            face = SplineFace,
            path = Spline,
            decorate = (t, b, o) =>
            {
                int count = t.Int("cloud.n", 4, 6);
                for (int i = 0; i < count; i++)
                {
                    float a = Mathf.PI + (Mathf.PI * (i + 0.5f)) / count;
                    o.petals.Add(new Petal
                    {
                        cx = b.cx + Mathf.Cos(a) * b.rx * 0.8f,
                        cy = b.cy + Mathf.Sin(a) * b.rx * 0.5f,
                        r = b.rx * t.Num($"cloud.r{i}", 0.44f, 0.62f),
                    });
                }
            },
        };

        public static readonly ShapeDef Droplet = new ShapeDef
        {
            name = "droplet",
            core = 0.78f,
            body = (t, b) => { b.cy += 0.22f * b.ry; b.n = 2f; },
            face = b => new Ellipse(b.cx, b.cy + b.ry * 0.05f, b.rx * 0.88f, b.ry * 0.88f),
            decorate = (t, b, o) =>
            {
                o.extra.Add(new ExtraTaper { cx = b.cx, cy = b.cy, rx = b.rx, ry = b.ry, tip = t.Num("droplet.tip", 1.4f, 1.65f) });
            },
        };

        public static readonly ShapeDef Hexagon = new ShapeDef
        {
            name = "hexagon",
            core = 1.05f,
            path = Poly,
            face = PolyFace,
            body = (t, b) => { b.sides = 6; b.rot = t.Num("body.rot", -12f, 12f); b.round = t.Num("poly.round", 0.24f, 0.5f); },
        };

        public static readonly ShapeDef Sun = new ShapeDef
        {
            name = "sun",
            core = 0.7f,
            decorate = (t, b, o) =>
            {
                int count = t.Int("sun.n", 6, 9);
                float dist = b.rx * t.Num("sun.dist", 1.0f, 1.08f);
                float pr = b.rx * t.Num("sun.r", 0.2f, 0.26f);
                float off = t.Num("sun.rot", 0f, 2f * Mathf.PI);
                for (int i = 0; i < count; i++)
                {
                    float a = off + (2f * Mathf.PI * i) / count;
                    o.petals.Add(new Petal { cx = b.cx + Mathf.Cos(a) * dist, cy = b.cy + Mathf.Sin(a) * dist, r = pr });
                }
            },
        };

        public static readonly ShapeDef Triangle = new ShapeDef
        {
            name = "triangle",
            core = 1.15f,
            path = Poly,
            body = (t, b) => { b.sides = 3; b.rot = t.Num("body.rot", -5f, 5f); b.round = t.Num("poly.round", 0.24f, 0.5f); },
            face = b => new Ellipse(b.cx, b.cy + b.ry * 0.1f, b.rx * 0.54f, b.ry * 0.36f),
        };

        /// <summary>Weighted shape bands: value in [0, upTo) picks that shape. Round and organic are common; the rest are rarer.</summary>
        public static readonly (ShapeDef shape, float upTo)[] Bands =
        {
            (Round, 0.22f), (Organic, 0.48f), (Boxy, 0.6f), (Capsule, 0.7f), (Nub, 0.79f),
            (Cloud, 0.86f), (Droplet, 0.915f), (Hexagon, 0.95f), (Sun, 0.98f), (Triangle, 1f),
        };
    }
}
