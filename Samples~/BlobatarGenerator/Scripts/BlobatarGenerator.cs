using System.Collections.Generic;
using UnityEngine;

using SHUU.Samples.Blobatar.InnerWorkings;

namespace SHUU.Samples.Blobatar
{
    public static class BlobatarGenerator
    {
        public static Texture2D GenerateBlobatar(
            string seed,
            bool background = true,
            int size = 512,
            BlobatarExpr expression = BlobatarExpr.Idle,
            float? hue = null,
            float? tone = null,
            bool normalize = true,
            int supersample = 3)
        {
            var t = new Traits(seed, normalize);
            var l = BlobatarCompose.BuildLayout(t);

            float hueV = hue ?? t.Num("hue", 0f, 360f);
            float toneV = tone ?? t["tone"];
            var pal = BlobatarColor.Ramp(hueV, true, toneV);

            var def = BlobatarExpression.Roster[expression];
            Oklch head = pal.head, eye = pal.eye;
            if (def.tint.HasValue && def.pose.heat > 0f)
            {
                var (hotHead, hotEye) = BlobatarColor.Tinted(head, eye, def.tint.Value);
                head = BlobatarColor.Mix(head, hotHead, def.pose.heat);
                eye = BlobatarColor.Mix(eye, hotEye, def.pose.heat);
            }

            var (bakedEyes, bdy) = BlobatarExpression.BakePose(l.eyes, def.pose);

            var pixels = new Color32[size * size];

            if (background)
            {
                var bgPolys = new List<List<Vector2>> { BlobatarPaths.Superellipse(50f, 50f, 50f, 50f, 6f) };
                var covBg = BlobatarRaster.RasterizeLayer(size, bgPolys, supersample);
                BlobatarRaster.Composite(pixels, covBg, BlobatarColor.ToColor(pal.bg));
            }

            var bodyPolys = BuildBodyPolys(l, bdy);
            var covBody = BlobatarRaster.RasterizeLayer(size, bodyPolys, supersample);
            BlobatarRaster.Composite(pixels, covBody, BlobatarColor.ToColor(head));

            var eyePolys = BuildEyePolys(bakedEyes, bdy);
            var covEye = BlobatarRaster.RasterizeLayer(size, eyePolys, supersample);
            BlobatarRaster.Composite(pixels, covEye, BlobatarColor.ToColor(eye));

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        static List<List<Vector2>> BuildBodyPolys(Layout l, float bdy)
        {
            var polys = new List<List<Vector2>>();
            foreach (var p in l.petals)
                polys.Add(BlobatarPaths.Superellipse(p.cx, p.cy, p.r, p.r, 2f));
            foreach (var e in l.extra)
                polys.Add(BlobatarPaths.Taper(e.cx, e.cy, e.rx, e.ry, e.tip));

            var body = l.body;
            polys.Add(l.draw != null
                ? l.draw(body)
                : BlobatarPaths.Superellipse(body.cx, body.cy, body.rx, body.ry, body.n, body.rot));

            if (bdy != 0f)
                for (int i = 0; i < polys.Count; i++)
                    polys[i] = ShiftY(polys[i], bdy);
            return polys;
        }

        static List<List<Vector2>> BuildEyePolys(List<Eye> eyes, float bdy)
        {
            var polys = new List<List<Vector2>>();
            foreach (var e in eyes)
                polys.Add(BlobatarPaths.Superellipse(e.cx, e.cy, e.rx, e.ry, e.n, e.rot));

            if (bdy != 0f)
                for (int i = 0; i < polys.Count; i++)
                    polys[i] = ShiftY(polys[i], bdy);
            return polys;
        }

        static List<Vector2> ShiftY(List<Vector2> poly, float dy)
        {
            var outp = new List<Vector2>(poly.Count);
            foreach (var p in poly) outp.Add(new Vector2(p.x, p.y + dy));
            return outp;
        }
    }
}
