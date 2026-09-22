using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Samples.Blobatar.InnerWorkings
{
    public struct Eye { public float cx, cy, rx, ry, n, rot; }

    /// <summary>The fully-resolved, not-yet-posed description of one blobatar.</summary>
    public class Layout
    {
        public string shape;
        public Body body;
        public Ellipse face;
        public List<Petal> petals;
        public List<ExtraTaper> extra;
        public List<Eye> eyes;
        public System.Func<Body, List<Vector2>> draw;
    }

    /// <summary>Ported from styles/compose.ts: picks a shape, builds its body, and fits two eyes onto its face.</summary>
    public static class BlobatarCompose
    {
        /// <summary>
        /// Sizes and places two eyes onto a face ellipse so they never overflow it,
        /// then adds gaze offset, a size/stretch asymmetry between the eyes, and a
        /// gentle correlated tilt. Ported from compose.ts's faceFit.
        /// </summary>
        public static List<Eye> FaceFit(Traits t, Body b, Ellipse face)
        {
            float rx = b.rx;
            float er0 = t.Num("eye.rx", 0.075f, 0.105f) * rx;
            float ratio = t.Num("eye.ratio", 1.9f, 3.2f);
            float scale = t.Num("eye.scale", 0.78f, 1.24f);
            float stretch = t.Num("eye.stretch", 0.85f, 1.18f);
            float clearance = t.Num("eye.gap", 0.1f, 0.24f) * rx;
            float wide = er0 * Mathf.Max(1f, scale);
            float tall = er0 * ratio * Mathf.Max(1f, scale * stretch);
            float gap0 = wide + rx * 0.03f + clearance;

            float gx = t.Jitter("gaze.x", 0.09f) * face.rx;
            float gy = t.Num("gaze.y", -0.2f, 0.08f) * face.ry;
            float dy = t.Jitter("eye.dy", 0.04f) * face.ry;
            float reach = Mathf.Sqrt(wide * wide + tall * tall);
            float needX = (Mathf.Abs(gx) + gap0 + reach) / face.rx;
            float needY = (Mathf.Abs(gy) + Mathf.Abs(dy) + reach) / face.ry;
            float need = Mathf.Sqrt(needX * needX + needY * needY);
            float fit = need > 0.9f ? 0.9f / need : 1f;

            float er = er0 * fit;
            float eyeRy = er * ratio;
            float gap = gap0 * fit;
            float room = Mathf.Clamp01(clearance / tall);
            float bound = Mathf.Min(12f, Mathf.Asin(room) * Mathf.Rad2Deg);
            float lean = t.Num("eye.lean", -1f, 1f) * bound;
            float lean2 = Mathf.Clamp(lean + t.Jitter("eye.lean2", 3.5f), -12f, 12f);

            float cx = face.cx + gx * fit;
            float cy = face.cy + gy * fit;

            var eyes = new List<Eye>(2)
            {
                new Eye { cx = cx - gap, cy = cy, rx = er, ry = eyeRy, n = t.Num("eye.n", 3.5f, 6f), rot = lean },
                new Eye { cx = cx + gap, cy = cy + dy * fit, rx = er * scale, ry = eyeRy * scale * stretch, n = t.Num("eye.n", 3.5f, 6f), rot = lean2 },
            };
            return eyes;
        }

        static ShapeDef PickShape(float v)
        {
            foreach (var band in BlobatarShapes.Bands)
                if (v < band.upTo) return band.shape;
            return BlobatarShapes.Bands[BlobatarShapes.Bands.Length - 1].shape;
        }

        /// <summary>Builds the full, un-posed layout for a set of traits.</summary>
        public static Layout BuildLayout(Traits t)
        {
            var shape = PickShape(t["shape"]);
            float r = t.Num("body.r", 31f, 38f) * shape.core;
            var body = new Body
            {
                cx = 50f + t.Jitter("body.x", 1.5f),
                cy = 50f + t.Jitter("body.y", 1.5f),
                rx = r,
                ry = r * t.Num("body.ratio", 0.92f, 1.08f),
                n = t.Num("body.n", 1.9f, 2.5f),
                rot = 0f,
            };

            int pts = t.Int("body.pts", 6, 8);
            body.radii = new List<float>(pts);
            for (int i = 0; i < pts; i++) body.radii.Add(1f + t.Jitter($"body.r{i}", 0.16f));

            shape.body?.Invoke(t, body);

            Ellipse face = shape.face != null ? shape.face(body) : new Ellipse(body.cx, body.cy, body.rx, body.ry);
            var deco = new Deco();
            shape.decorate?.Invoke(t, body, deco);

            var eyes = FaceFit(t, body, face);

            return new Layout
            {
                shape = shape.name,
                body = body,
                face = face,
                petals = deco.petals,
                extra = deco.extra,
                eyes = eyes,
                draw = shape.path,
            };
        }
    }
}
