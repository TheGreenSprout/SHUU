using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

using SHUU.Samples.Blobatar;
using SHUU.Samples.Blobatar.InnerWorkings;

namespace SHUU.Samples.Blobatar._Editor
{
    /// <summary>
    /// The ten body shapes, mirrored here as a plain enum so the custom builder
    /// tab can drive shape selection without going through the seed/trait system.
    /// </summary>
    public enum BlobatarCustomShape
    {
        Round, Organic, Boxy, Capsule, Nub, Cloud, Droplet, Hexagon, Sun, Triangle
    }

    /// <summary>
    /// Editor window for the Blobatar package. "Generate From Seed" reproduces
    /// exactly what <see cref="BlobatarGenerator.GenerateBlobatar"/> does. "Custom
    /// Builder" bypasses the seed/hash system entirely and builds a blobatar
    /// straight from sliders the user controls directly, reusing the same
    /// geometry/color/expression/raster primitives the runtime uses. Either tab
    /// can save its current preview to a Texture2D (.png) asset in the project.
    /// </summary>
    public class BlobatarEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Sprout's Handy Unity Utils/Blobatar Generator")]
        public static void Open()
        {
            var win = GetWindow<BlobatarEditorWindow>("Blobatar Editor");
            win.minSize = new Vector2(560, 480);
        }

        int tab;
        Vector2 scroll;

        const int PreviewBoxSize = 300;

        void OnEnable()
        {
            RegenerateSeedPreview();
            RegenerateCustomPreview();
        }

        void OnGUI()
        {
            tab = GUILayout.Toolbar(tab, new[] { "Generate From Seed", "Custom Builder" });
            EditorGUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();

            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(position.width - PreviewBoxSize - 24));
            if (tab == 0) DrawSeedTab();
            else DrawCustomTab();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginVertical(GUILayout.Width(PreviewBoxSize));
            DrawPreview(tab == 0 ? seedPreview : customPreview);
            EditorGUILayout.Space(6);
            if (GUILayout.Button("Regenerate"))
            {
                if (tab == 0) RegenerateSeedPreview(); else RegenerateCustomPreview();
            }
            if (GUILayout.Button("Save As Texture Asset..."))
            {
                SaveTextureAsset(tab == 0 ? seedPreview : customPreview, tab == 0 ? SuggestedSeedName() : "CustomBlobatar");
            }

            if (tab == 1)
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Randomize All"))
                {
                    RandomizeCustom();
                    RegenerateCustomPreview();
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        void DrawPreview(Texture2D tex)
        {
            Rect r = GUILayoutUtility.GetRect(PreviewBoxSize, PreviewBoxSize, GUILayout.ExpandWidth(false));
            EditorGUI.DrawRect(r, new Color(0.16f, 0.16f, 0.16f));
            if (tex != null)
            {
                float s = Mathf.Min(r.width / tex.width, r.height / tex.height);
                float w = tex.width * s, h = tex.height * s;
                Rect inner = new Rect(r.x + (r.width - w) / 2f, r.y + (r.height - h) / 2f, w, h);
                GUI.DrawTexture(inner, tex, ScaleMode.ScaleToFit, true);
            }
            if (tex != null)
                EditorGUILayout.LabelField($"{tex.width} x {tex.height}", EditorStyles.centeredGreyMiniLabel);
        }

        // ============================================================
        // TAB 1 — Generate From Seed (thin wrapper over BlobatarGenerator)
        // ============================================================

        string seed = "hello world";
        bool seedBackground = true;
        int seedSize = 256;
        BlobatarExpr seedExpression = BlobatarExpr.Idle;
        bool seedOverrideHue;
        float seedHue = 210f;
        bool seedOverrideTone;
        int seedToneIndex = 2;
        bool seedNormalize = true;
        int seedSupersample = 3;
        Texture2D seedPreview;

        static readonly string[] ToneLabels = System.Array.ConvertAll(BlobatarColor.ToneOptions, o => o.label);

        string SuggestedSeedName() => "Blobatar_" + Sanitize(string.IsNullOrEmpty(seed) ? "seed" : seed);

        static string Sanitize(string s)
        {
            var chars = s.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                if (!char.IsLetterOrDigit(chars[i])) chars[i] = '_';
            return new string(chars);
        }

        void DrawSeedTab()
        {
            EditorGUILayout.LabelField("Generate From Seed", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Same seed always produces the same avatar. This mirrors BlobatarGenerator.GenerateBlobatar exactly.", MessageType.None);

            EditorGUI.BeginChangeCheck();

            seed = EditorGUILayout.TextField("Seed", seed);
            seedNormalize = EditorGUILayout.Toggle(new GUIContent("Normalize Seed", "Trims/lowercases the seed so \"Alice\" and \" alice \" match."), seedNormalize);
            seedBackground = EditorGUILayout.Toggle("Background Plate", seedBackground);
            seedSize = EditorGUILayout.IntPopup("Size", seedSize, new[] { "64", "128", "256", "512", "1024" }, new[] { 64, 128, 256, 512, 1024 });
            seedExpression = (BlobatarExpr)EditorGUILayout.EnumPopup("Expression", seedExpression);
            seedSupersample = EditorGUILayout.IntSlider(new GUIContent("Supersample (AA quality)", "Higher = smoother edges, slower."), seedSupersample, 1, 4);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Color Overrides", EditorStyles.boldLabel);
            seedOverrideHue = EditorGUILayout.Toggle("Override Hue", seedOverrideHue);
            using (new EditorGUI.DisabledScope(!seedOverrideHue))
                seedHue = EditorGUILayout.Slider("Hue", seedHue, 0f, 360f);
            seedOverrideTone = EditorGUILayout.Toggle("Override Tone", seedOverrideTone);
            using (new EditorGUI.DisabledScope(!seedOverrideTone))
                seedToneIndex = EditorGUILayout.Popup(new GUIContent("Tone", "Tone only has a handful of valid shades - picking between them directly instead of a free slider."), seedToneIndex, ToneLabels);

            if (EditorGUI.EndChangeCheck())
                RegenerateSeedPreview();
        }

        void RegenerateSeedPreview()
        {
            if (seedPreview != null) DestroyImmediate(seedPreview);
            seedPreview = BlobatarGenerator.GenerateBlobatar(
                seed,
                background: seedBackground,
                size: seedSize,
                expression: seedExpression,
                hue: seedOverrideHue ? (float?)seedHue : null,
                tone: seedOverrideTone ? (float?)BlobatarColor.ToneOptions[seedToneIndex].tone : null,
                normalize: seedNormalize,
                supersample: seedSupersample);
        }

        // ============================================================
        // TAB 2 — Custom Builder (fully manual, no hashing involved)
        // ============================================================

        BlobatarCustomShape shape = BlobatarCustomShape.Round;

        float bodyR = 34f, bodyRatio = 1f, bodyRotDeg = 0f, bodyN = 2.4f;
        float bodyOffsetX, bodyOffsetY;

        // shape-specific (capsule's "squat" reuses bodyRatio, relabeled — see DrawCustomTab)
        int nubCount = 1;
        float nubSize = 0.3f;
        float nubAngle0 = 45f, nubAngle1 = 225f;
        int cloudCount = 5;
        float cloudSize = 0.5f;
        float dropletTip = 1.5f;
        float hexRound = 0.35f;
        int sunCount = 7;
        float sunDist = 1.02f, sunSize = 0.22f, sunRotOffsetDeg;
        float triRound = 0.35f;

        // organic / cloud spline detail (the only bit that still uses the hash,
        // purely as a source of "texture" for the dent pattern)
        string detailSeed = "detail";
        float wobble = 0.16f;
        int ptsCount = 7;

        // eyes
        float eyeSizeFrac = 0.09f, eyeRatio = 2.5f, eyeScale = 1f, eyeStretch = 1f, eyeGapFrac = 0.16f;
        float gazeX, gazeY = -0.05f, eyeDy;
        float eyeLean, eyeLean2Delta;
        float eyeCornerN = 4.5f;

        // color / output
        float customHue = 210f, customTone = 0.55f;
        int customToneIndex = 2;
        bool customBackground = true;
        BlobatarExpr customExpression = BlobatarExpr.Idle;
        int customSize = 256;
        int customSupersample = 3;

        Texture2D customPreview;

        void DrawCustomTab()
        {
            EditorGUILayout.LabelField("Custom Builder", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Every value here is set directly by you — no seed hashing involved (except the optional dent pattern on Organic/Cloud).", MessageType.None);

            EditorGUI.BeginChangeCheck();

            var newShape = (BlobatarCustomShape)EditorGUILayout.EnumPopup("Shape", shape);
            if (newShape != shape)
            {
                shape = newShape;
                // Re-clamp values whose valid range depends on the shape, so
                // switching shapes doesn't leave an out-of-range value sitting
                // unseen until the user happens to touch that slider.
                float ratioMin = shape == BlobatarCustomShape.Capsule ? 0.3f : 0.6f;
                float ratioMax = shape == BlobatarCustomShape.Capsule ? 0.85f : 1.4f;
                bodyRatio = Mathf.Clamp(bodyRatio, ratioMin, ratioMax);
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Body", EditorStyles.boldLabel);
            bodyR = EditorGUILayout.Slider("Radius", bodyR, 12f, 48f);
            {
                float ratioMin = shape == BlobatarCustomShape.Capsule ? 0.3f : 0.6f;
                float ratioMax = shape == BlobatarCustomShape.Capsule ? 0.85f : 1.4f;
                bodyRatio = EditorGUILayout.Slider(shape == BlobatarCustomShape.Capsule ? "Squat" : "Height Ratio", bodyRatio, ratioMin, ratioMax);
            }
            bodyOffsetX = EditorGUILayout.Slider("Offset X", bodyOffsetX, -6f, 6f);
            bodyOffsetY = EditorGUILayout.Slider("Offset Y", bodyOffsetY, -6f, 6f);

            bool showRot = shape != BlobatarCustomShape.Capsule && shape != BlobatarCustomShape.Droplet;
            if (showRot)
                bodyRotDeg = EditorGUILayout.Slider("Rotation", bodyRotDeg, -45f, 45f);

            bool showN = shape == BlobatarCustomShape.Round || shape == BlobatarCustomShape.Boxy
                         || shape == BlobatarCustomShape.Nub || shape == BlobatarCustomShape.Sun;
            if (showN)
                bodyN = EditorGUILayout.Slider(new GUIContent("Squircle Exponent", "2 = ellipse, higher = squarer, lower = pointier."), bodyN, 1.2f, 8f);

            EditorGUILayout.Space(4);
            DrawShapeSpecificFields();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Eyes", EditorStyles.boldLabel);
            eyeSizeFrac = EditorGUILayout.Slider("Size", eyeSizeFrac, 0.05f, 0.14f);
            eyeRatio = EditorGUILayout.Slider("Height Ratio", eyeRatio, 1.4f, 3.6f);
            eyeScale = EditorGUILayout.Slider("Right Eye Scale", eyeScale, 0.7f, 1.3f);
            eyeStretch = EditorGUILayout.Slider("Right Eye Stretch", eyeStretch, 0.8f, 1.25f);
            eyeGapFrac = EditorGUILayout.Slider("Gap", eyeGapFrac, 0.05f, 0.3f);
            gazeX = EditorGUILayout.Slider("Gaze X", gazeX, -0.12f, 0.12f);
            gazeY = EditorGUILayout.Slider("Gaze Y", gazeY, -0.25f, 0.12f);
            eyeDy = EditorGUILayout.Slider("Right Eye Vertical Offset", eyeDy, -0.06f, 0.06f);
            eyeLean = EditorGUILayout.Slider("Tilt", eyeLean, -12f, 12f);
            eyeLean2Delta = EditorGUILayout.Slider("Right Eye Tilt Offset", eyeLean2Delta, -6f, 6f);
            eyeCornerN = EditorGUILayout.Slider("Corner Shape", eyeCornerN, 2f, 8f);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Color & Output", EditorStyles.boldLabel);
            customHue = EditorGUILayout.Slider("Hue", customHue, 0f, 360f);
            customToneIndex = EditorGUILayout.Popup(new GUIContent("Tone", "Tone only has a handful of valid shades - picking between them directly instead of a free slider."), customToneIndex, ToneLabels);
            customTone = BlobatarColor.ToneOptions[customToneIndex].tone;
            customBackground = EditorGUILayout.Toggle("Background Plate", customBackground);
            customExpression = (BlobatarExpr)EditorGUILayout.EnumPopup("Expression", customExpression);
            customSize = EditorGUILayout.IntPopup("Size", customSize, new[] { "64", "128", "256", "512", "1024" }, new[] { 64, 128, 256, 512, 1024 });
            customSupersample = EditorGUILayout.IntSlider("Supersample (AA quality)", customSupersample, 1, 4);

            if (EditorGUI.EndChangeCheck())
                RegenerateCustomPreview();
        }

        void DrawShapeSpecificFields()
        {
            switch (shape)
            {
                case BlobatarCustomShape.Organic:
                case BlobatarCustomShape.Cloud:
                    EditorGUILayout.LabelField("Dent Pattern", EditorStyles.boldLabel);
                    ptsCount = EditorGUILayout.IntSlider("Point Count", ptsCount, 6, 8);
                    wobble = EditorGUILayout.Slider("Wobble", wobble, 0f, 0.4f);
                    EditorGUILayout.BeginHorizontal();
                    detailSeed = EditorGUILayout.TextField("Detail Seed", detailSeed);
                    if (GUILayout.Button("Reroll", GUILayout.Width(60)))
                    {
                        detailSeed = System.Guid.NewGuid().ToString("N").Substring(0, 8);
                        RegenerateCustomPreview();
                    }
                    EditorGUILayout.EndHorizontal();
                    if (shape == BlobatarCustomShape.Cloud)
                    {
                        EditorGUILayout.Space(2);
                        EditorGUILayout.LabelField("Puffs", EditorStyles.boldLabel);
                        cloudCount = EditorGUILayout.IntSlider("Count", cloudCount, 3, 7);
                        cloudSize = EditorGUILayout.Slider("Size", cloudSize, 0.3f, 0.7f);
                    }
                    break;

                case BlobatarCustomShape.Capsule:
                    // squat reuses the generic "Height Ratio" slider above (relabeled).
                    break;

                case BlobatarCustomShape.Nub:
                    EditorGUILayout.LabelField("Nubs", EditorStyles.boldLabel);
                    nubCount = EditorGUILayout.IntSlider("Count", nubCount, 1, 2);
                    nubSize = EditorGUILayout.Slider("Size", nubSize, 0.15f, 0.5f);
                    nubAngle0 = EditorGUILayout.Slider("Angle 1", nubAngle0, 0f, 360f);
                    if (nubCount > 1)
                        nubAngle1 = EditorGUILayout.Slider("Angle 2", nubAngle1, 0f, 360f);
                    break;

                case BlobatarCustomShape.Droplet:
                    EditorGUILayout.LabelField("Tail", EditorStyles.boldLabel);
                    dropletTip = EditorGUILayout.Slider("Taper", dropletTip, 1.1f, 2f);
                    break;

                case BlobatarCustomShape.Hexagon:
                    EditorGUILayout.LabelField("Corners", EditorStyles.boldLabel);
                    hexRound = EditorGUILayout.Slider("Rounding", hexRound, 0.05f, 0.7f);
                    break;

                case BlobatarCustomShape.Sun:
                    EditorGUILayout.LabelField("Rays", EditorStyles.boldLabel);
                    sunCount = EditorGUILayout.IntSlider("Count", sunCount, 5, 10);
                    sunDist = EditorGUILayout.Slider("Distance", sunDist, 0.85f, 1.3f);
                    sunSize = EditorGUILayout.Slider("Size", sunSize, 0.12f, 0.3f);
                    sunRotOffsetDeg = EditorGUILayout.Slider("Rotation Offset", sunRotOffsetDeg, 0f, 360f);
                    break;

                case BlobatarCustomShape.Triangle:
                    EditorGUILayout.LabelField("Corners", EditorStyles.boldLabel);
                    triRound = EditorGUILayout.Slider("Rounding", triRound, 0.05f, 0.7f);
                    break;
            }
        }

        void RandomizeCustom()
        {
            var prev = Random.state;
            Random.InitState((int)(System.DateTime.Now.Ticks & 0x7fffffff));

            shape = (BlobatarCustomShape)Random.Range(0, 10);
            bodyR = Random.Range(24f, 42f);
            bodyRatio = Random.Range(0.75f, 1.2f);
            bodyRotDeg = Random.Range(-30f, 30f);
            bodyN = Random.Range(1.6f, 5f);
            bodyOffsetX = Random.Range(-3f, 3f);
            bodyOffsetY = Random.Range(-3f, 3f);
            nubCount = Random.Range(1, 3);
            nubSize = Random.Range(0.2f, 0.45f);
            nubAngle0 = Random.Range(0f, 360f);
            nubAngle1 = Random.Range(0f, 360f);
            cloudCount = Random.Range(3, 8);
            cloudSize = Random.Range(0.35f, 0.65f);
            dropletTip = Random.Range(1.2f, 1.8f);
            hexRound = Random.Range(0.1f, 0.6f);
            sunCount = Random.Range(5, 11);
            sunDist = Random.Range(0.9f, 1.2f);
            sunSize = Random.Range(0.15f, 0.28f);
            sunRotOffsetDeg = Random.Range(0f, 360f);
            triRound = Random.Range(0.1f, 0.6f);
            detailSeed = Random.Range(0, int.MaxValue).ToString();
            wobble = Random.Range(0.05f, 0.3f);
            ptsCount = Random.Range(6, 9);
            eyeSizeFrac = Random.Range(0.06f, 0.12f);
            eyeRatio = Random.Range(1.8f, 3.2f);
            eyeScale = Random.Range(0.85f, 1.2f);
            eyeStretch = Random.Range(0.9f, 1.15f);
            eyeGapFrac = Random.Range(0.08f, 0.24f);
            gazeX = Random.Range(-0.08f, 0.08f);
            gazeY = Random.Range(-0.18f, 0.08f);
            eyeDy = Random.Range(-0.03f, 0.03f);
            eyeLean = Random.Range(-10f, 10f);
            eyeLean2Delta = Random.Range(-4f, 4f);
            eyeCornerN = Random.Range(3f, 6.5f);
            customHue = Random.Range(0f, 360f);
            customToneIndex = Random.Range(0, BlobatarColor.ToneOptions.Length);
            customTone = BlobatarColor.ToneOptions[customToneIndex].tone;

            Random.state = prev;
        }

        void RegenerateCustomPreview()
        {
            if (customPreview != null) DestroyImmediate(customPreview);
            customPreview = BuildCustomBlobatar();
        }

        /// <summary>
        /// Builds a blobatar straight from the manual fields above. Mirrors
        /// BlobatarGenerator.GenerateBlobatar's pipeline (layout -> palette ->
        /// expression bake -> rasterize -> composite) but every geometric value
        /// comes directly from a slider instead of a hashed trait.
        /// </summary>
        Texture2D BuildCustomBlobatar()
        {
            float cx = 50f + bodyOffsetX, cy = 50f + bodyOffsetY;
            float rx = bodyR, ry = bodyR * bodyRatio;

            var petals = new List<Petal>();
            var extra = new List<ExtraTaper>();
            List<Vector2> bodyPoly;
            Ellipse face;

            switch (shape)
            {
                case BlobatarCustomShape.Organic:
                {
                    var radii = BuildDetailRadii();
                    bodyPoly = BlobatarPaths.BlobPath(cx, cy, rx, ry, radii, bodyRotDeg);
                    float k = MinRadius(radii) * 0.95f;
                    face = new Ellipse(cx, cy, rx * k, ry * k);
                    break;
                }
                case BlobatarCustomShape.Cloud:
                {
                    var radii = BuildDetailRadii();
                    bodyPoly = BlobatarPaths.BlobPath(cx, cy, rx, ry, radii, bodyRotDeg);
                    float k = MinRadius(radii) * 0.95f;
                    face = new Ellipse(cx, cy, rx * k, ry * k);
                    for (int i = 0; i < cloudCount; i++)
                    {
                        float a = Mathf.PI + (Mathf.PI * (i + 0.5f)) / cloudCount;
                        petals.Add(new Petal { cx = cx + Mathf.Cos(a) * rx * 0.8f, cy = cy + Mathf.Sin(a) * rx * 0.5f, r = rx * cloudSize });
                    }
                    break;
                }
                case BlobatarCustomShape.Boxy:
                    bodyPoly = BlobatarPaths.Superellipse(cx, cy, rx, ry, bodyN, bodyRotDeg);
                    face = new Ellipse(cx, cy, rx, ry);
                    break;
                case BlobatarCustomShape.Capsule:
                {
                    bodyPoly = BlobatarPaths.Box(cx, cy, rx - ry, ry);
                    face = new Ellipse(cx, cy, rx * 0.94f, ry * 0.94f);
                    foreach (var s in new[] { -1f, 1f })
                        petals.Add(new Petal { cx = cx + s * (rx - ry), cy = cy, r = ry });
                    break;
                }
                case BlobatarCustomShape.Nub:
                {
                    bodyPoly = BlobatarPaths.Superellipse(cx, cy, rx, ry, bodyN, bodyRotDeg);
                    face = new Ellipse(cx, cy, rx, ry);
                    float[] angles = { nubAngle0, nubAngle1 };
                    for (int i = 0; i < nubCount; i++)
                    {
                        float a = angles[i] * Mathf.Deg2Rad;
                        petals.Add(new Petal { cx = cx + Mathf.Cos(a) * rx * 0.88f, cy = cy + Mathf.Sin(a) * rx * 0.88f, r = rx * nubSize });
                    }
                    break;
                }
                case BlobatarCustomShape.Droplet:
                {
                    float dcy = cy + 0.22f * ry;
                    bodyPoly = BlobatarPaths.Superellipse(cx, dcy, rx, ry, 2f, 0f);
                    face = new Ellipse(cx, dcy + ry * 0.05f, rx * 0.88f, ry * 0.88f);
                    extra.Add(new ExtraTaper { cx = cx, cy = dcy, rx = rx, ry = ry, tip = dropletTip });
                    break;
                }
                case BlobatarCustomShape.Hexagon:
                    bodyPoly = BlobatarPaths.Polygon(cx, cy, rx, ry, 6, hexRound, bodyRotDeg);
                    face = new Ellipse(cx, cy, rx * 0.84f, ry * 0.84f);
                    break;
                case BlobatarCustomShape.Sun:
                {
                    bodyPoly = BlobatarPaths.Superellipse(cx, cy, rx, ry, bodyN, bodyRotDeg);
                    face = new Ellipse(cx, cy, rx, ry);
                    float off = sunRotOffsetDeg * Mathf.Deg2Rad;
                    float dist = rx * sunDist;
                    float pr = rx * sunSize;
                    for (int i = 0; i < sunCount; i++)
                    {
                        float a = off + (2f * Mathf.PI * i) / sunCount;
                        petals.Add(new Petal { cx = cx + Mathf.Cos(a) * dist, cy = cy + Mathf.Sin(a) * dist, r = pr });
                    }
                    break;
                }
                case BlobatarCustomShape.Triangle:
                    bodyPoly = BlobatarPaths.Polygon(cx, cy, rx, ry, 3, triRound, bodyRotDeg);
                    face = new Ellipse(cx, cy + ry * 0.1f, rx * 0.54f, ry * 0.36f);
                    break;
                default: // Round
                    bodyPoly = BlobatarPaths.Superellipse(cx, cy, rx, ry, bodyN, bodyRotDeg);
                    face = new Ellipse(cx, cy, rx, ry);
                    break;
            }

            var eyes = FaceFitDirect(rx, face);

            // Palette + expression (identical to BlobatarGenerator's pipeline).
            var pal = BlobatarColor.Ramp(customHue, true, customTone);
            var def = BlobatarExpression.Roster[customExpression];
            Oklch head = pal.head, eye = pal.eye;
            if (def.tint.HasValue && def.pose.heat > 0f)
            {
                var (hotHead, hotEye) = BlobatarColor.Tinted(head, eye, def.tint.Value);
                head = BlobatarColor.Mix(head, hotHead, def.pose.heat);
                eye = BlobatarColor.Mix(eye, hotEye, def.pose.heat);
            }
            var (bakedEyes, bdy) = BlobatarExpression.BakePose(eyes, def.pose);

            int size = customSize;
            var pixels = new Color32[size * size];

            if (customBackground)
            {
                var bgPolys = new List<List<Vector2>> { BlobatarPaths.Superellipse(50f, 50f, 50f, 50f, 6f) };
                var covBg = BlobatarRaster.RasterizeLayer(size, bgPolys, customSupersample);
                BlobatarRaster.Composite(pixels, covBg, BlobatarColor.ToColor(pal.bg));
            }

            var bodyPolys = new List<List<Vector2>>();
            foreach (var p in petals) bodyPolys.Add(BlobatarPaths.Superellipse(p.cx, p.cy, p.r, p.r, 2f));
            foreach (var e in extra) bodyPolys.Add(BlobatarPaths.Taper(e.cx, e.cy, e.rx, e.ry, e.tip));
            bodyPolys.Add(bodyPoly);
            if (bdy != 0f) ShiftAll(bodyPolys, bdy);
            var covBody = BlobatarRaster.RasterizeLayer(size, bodyPolys, customSupersample);
            BlobatarRaster.Composite(pixels, covBody, BlobatarColor.ToColor(head));

            var eyePolys = new List<List<Vector2>>();
            foreach (var e in bakedEyes) eyePolys.Add(BlobatarPaths.Superellipse(e.cx, e.cy, e.rx, e.ry, e.n, e.rot));
            if (bdy != 0f) ShiftAll(eyePolys, bdy);
            var covEye = BlobatarRaster.RasterizeLayer(size, eyePolys, customSupersample);
            BlobatarRaster.Composite(pixels, covEye, BlobatarColor.ToColor(eye));

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        List<float> BuildDetailRadii()
        {
            var t = new Traits(detailSeed, true);
            var radii = new List<float>(ptsCount);
            for (int i = 0; i < ptsCount; i++)
                radii.Add(1f + t.Jitter($"organic.r{i}", wobble));
            return radii;
        }

        static float MinRadius(List<float> radii)
        {
            float m = float.MaxValue;
            foreach (var r in radii) if (r < m) m = r;
            return m;
        }

        static void ShiftAll(List<List<Vector2>> polys, float dy)
        {
            for (int i = 0; i < polys.Count; i++)
            {
                var poly = polys[i];
                for (int j = 0; j < poly.Count; j++)
                    poly[j] = new Vector2(poly[j].x, poly[j].y + dy);
            }
        }

        /// <summary>
        /// Direct-value clone of BlobatarCompose.FaceFit: same auto-fit safety
        /// clamp (eyes never overflow the face), but every input is a slider
        /// value instead of a hashed trait.
        /// </summary>
        List<Eye> FaceFitDirect(float rx, Ellipse face)
        {
            float er0 = eyeSizeFrac * rx;
            float ratio = eyeRatio;
            float scale = eyeScale;
            float stretch = eyeStretch;
            float clearance = eyeGapFrac * rx;
            float wide = er0 * Mathf.Max(1f, scale);
            float tall = er0 * ratio * Mathf.Max(1f, scale * stretch);
            float gap0 = wide + rx * 0.03f + clearance;

            float gx = gazeX * face.rx;
            float gy = gazeY * face.ry;
            float dy = eyeDy * face.ry;
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
            float lean = Mathf.Clamp(eyeLean, -bound, bound);
            float lean2 = Mathf.Clamp(lean + eyeLean2Delta, -12f, 12f);

            float cx = face.cx + gx * fit;
            float cy = face.cy + gy * fit;

            return new List<Eye>(2)
            {
                new Eye { cx = cx - gap, cy = cy, rx = er, ry = eyeRy, n = eyeCornerN, rot = lean },
                new Eye { cx = cx + gap, cy = cy + dy * fit, rx = er * scale, ry = eyeRy * scale * stretch, n = eyeCornerN, rot = lean2 },
            };
        }

        // ============================================================
        // Shared: save the current preview texture into the project
        // ============================================================

        static void SaveTextureAsset(Texture2D tex, string suggestedName)
        {
            if (tex == null)
            {
                EditorUtility.DisplayDialog("Blobatar", "Nothing to save yet.", "OK");
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Blobatar Texture",
                suggestedName,
                "png",
                "Choose where to save the blobatar texture.");

            if (string.IsNullOrEmpty(path)) return;

            byte[] png = tex.EncodeToPNG();
            File.WriteAllBytes(path, png);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }

            AssetDatabase.SaveAssets();
            var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
        }
    }
}