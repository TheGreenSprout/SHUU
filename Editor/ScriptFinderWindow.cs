/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

using SETB.SuperClasses;

using static SETB.EditorGUI_Base;

namespace SHUU._Editor
{
    public class ScriptFinderWindow : EditorWindow_Base<ScriptFinderWindow>
    {
        // ── State ──────────────────────────────────────────────────────────────
        private MonoScript _targetScript;
        private readonly List<ResultEntry> _results = new();
        private Vector2 _scrollPos;
        private string _emptyMessage = string.Empty;
        private string _lastSearchName;
        private int _selectedIndex = -1;

        // ── Colours ────────────────────────────────────────────────────────────
        private static readonly Color ColAccent      = new Color32(82,  139, 255, 255);
        private static readonly Color ColAccentDim   = new Color32(82,  139, 255,  40);
        private static readonly Color ColRowEven     = new Color32(42,  42,  42,  255);
        private static readonly Color ColRowOdd      = new Color32(38,  38,  38,  255);
        private static readonly Color ColRowHover    = new Color32(55,  55,  55,  255);
        private static readonly Color ColRowSelected = new Color32(34,  68,  130, 255);
        private static readonly Color ColSeparator   = new Color32(25,  25,  25,  255);
        private static readonly Color ColBadgeBg     = new Color32(30,  60,  120, 255);
        private static readonly Color ColBadgeText   = new Color32(140, 180, 255, 255);
        private static readonly Color ColDimText     = new Color32(130, 130, 130, 255);
        private static readonly Color ColIcon        = new Color32(160, 200, 255, 255);
        private static readonly Color ColHeaderBg    = new Color32(30,  30,  30,  255);

        // ── Styles (lazy) ──────────────────────────────────────────────────────
        private GUIStyle _styleName;
        private GUIStyle _stylePath;
        private GUIStyle _styleBadge;
        private GUIStyle _styleHeader;
        private GUIStyle _styleCount;
        private GUIStyle _styleEmpty;
        private bool _stylesInitialized;

        // ── Textures ───────────────────────────────────────────────────────────
        private Texture2D _texBadgeBg;

        private const float RowHeight = 48f;

        // ── Entry point ────────────────────────────────────────────────────────
        [MenuItem("Tools/Sprout's Handy Unity Utils/Script Finder")]
        public static void ShowWindow()
            => CreateWindow("Script Finder", centered: true, minWidth: 360, minHeight: 300);

        // ── Lifecycle ──────────────────────────────────────────────────────────
        protected override void OnEnable()
        {
            base.OnEnable(); // loads [EditorPref] attributes
            titleContent = new GUIContent("  Script Finder", EditorGUIUtility.IconContent("Search Icon").image);
        }

        protected override void OnDisable()
        {
            base.OnDisable(); // saves [EditorPref] attributes
            DestroyImmediate(_texBadgeBg);
            _stylesInitialized = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DestroyImmediate(_texBadgeBg);
        }

        // ── Main GUI ───────────────────────────────────────────────────────────
        private void OnGUI()
        {
            EnsureStyles();
            DrawTopBar();
            DrawSearchRow();
            DrawResultsPanel();
        }

        // ── Top bar ────────────────────────────────────────────────────────────
        private void DrawTopBar()
        {
            Rect barRect = GetControlRect(GUILayout.Height(38f));
            DrawRect(barRect, ColHeaderBg);

            // Accent line (left edge)
            DrawRect(new Rect(barRect.x, barRect.y, 3f, barRect.height), ColAccent);

            // Title
            GUI.Label(new Rect(barRect.x + 14f, barRect.y, barRect.width - 14f, barRect.height),
                "Script Finder", _styleHeader);

            // Result count badge
            if (_results.Count > 0)
            {
                string countText = $"{_results.Count} found";
                Vector2 countSize = _styleBadge.CalcSize(new GUIContent(countText));
                Rect badgeRect = new Rect(
                    barRect.xMax - countSize.x - 12f,
                    barRect.y + (barRect.height - countSize.y) * 0.5f,
                    countSize.x, countSize.y);
                GUI.Label(badgeRect, countText, _styleBadge);
            }
        }

        // ── Search row ─────────────────────────────────────────────────────────
        private void DrawSearchRow()
        {
            Space(6f);
            Horizontal(() =>
            {
                GUILayout.Space(10f);

                if (ChangeCheck(() =>
                    _targetScript = DrawInputObject(null, _targetScript,
                        allowSceneObjects: false, GUILayout.Height(24f))))
                {
                    ClearResults();
                }

                GUILayout.Space(8f);

                using (new EditorGUI.DisabledGroupScope(_targetScript == null))
                {
                    var oldBg = GUI.backgroundColor;
                    GUI.backgroundColor = ColAccent;
                    DrawButton("Search", RunSearch, null, GUILayout.Width(70f), GUILayout.Height(24f));
                    GUI.backgroundColor = oldBg;
                }

                GUILayout.Space(10f);
            });
        }

        // ── Results panel ──────────────────────────────────────────────────────
        private void DrawResultsPanel()
        {
            Space(6f);

            // Separator
            DrawSeparator();

            // Empty state
            if (_results.Count == 0)
            {
                FlexibleSpace();
                DrawLabel(
                    string.IsNullOrEmpty(_emptyMessage)
                        ? "Select a script and press Search"
                        : _emptyMessage,
                    _styleEmpty);
                FlexibleSpace();
                return;
            }

            // Column headers
            DrawColumnHeaders();

            float panelHeight = position.height - 38f - 24f - 6f - 1f - 24f - 10f;

            ScrollView(ref _scrollPos, () =>
            {
                for (int i = 0; i < _results.Count; i++)
                    DrawResultRow(i);
            }, GUILayout.Height(panelHeight));
        }

        private void DrawColumnHeaders()
        {
            Rect rect = GetControlRect(GUILayout.Height(24f));
            DrawRect(rect, new Color32(25, 25, 30, 255));

            float x = rect.x + 12f;
            GUI.Label(new Rect(x,         rect.y, 180f,              rect.height), "OBJECT",         _styleCount);
            GUI.Label(new Rect(x + 180f,  rect.y, rect.width - 200f, rect.height), "HIERARCHY PATH", _styleCount);
        }

        private void DrawResultRow(int index)
        {
            ResultEntry entry = _results[index];

            Rect rowRect  = GetControlRect(GUILayout.Height(RowHeight));
            bool selected = index == _selectedIndex;
            bool hover    = rowRect.Contains(Event.current.mousePosition);

            Color bg = selected ? ColRowSelected
                     : hover    ? ColRowHover
                     : index % 2 == 0 ? ColRowEven : ColRowOdd;

            DrawRect(rowRect, bg);

            if (selected)
                DrawRect(new Rect(rowRect.x, rowRect.y, 3f, rowRect.height), ColAccent);

            DrawRect(new Rect(rowRect.x, rowRect.yMax - 1f, rowRect.width, 1f), new Color32(20, 20, 20, 255));

            float cx    = rowRect.x + 12f;
            float cy    = rowRect.y + RowHeight * 0.5f;

            // GameObject icon
            Texture goIcon = EditorGUIUtility.IconContent("GameObject Icon").image;
            if (goIcon != null)
            {
                GUI.DrawTexture(new Rect(cx, cy - 8f, 16f, 16f), goIcon,
                    ScaleMode.ScaleToFit, true, 0f, ColIcon, 0f, 0f);
                cx += 22f;
            }

            GUI.Label(new Rect(cx, rowRect.y + 8f, 160f, 20f), entry.ObjectName, _styleName);

            if (entry.ComponentCount > 1)
            {
                string badge    = $"×{entry.ComponentCount}";
                Vector2 badgeSize = _styleBadge.CalcSize(new GUIContent(badge));
                GUI.Label(new Rect(
                    cx + 164f,
                    rowRect.y + (RowHeight - badgeSize.y) * 0.5f,
                    badgeSize.x, badgeSize.y), badge, _styleBadge);
            }

            GUI.Label(new Rect(cx, rowRect.y + 28f, 220f, 16f), entry.HierarchyPath, _stylePath);

            // Active/inactive tag
            string activeTag  = entry.IsActive ? "active" : "inactive";
            Color  activeCol  = entry.IsActive ? ColAccent : ColDimText;
            var    tagStyle   = new GUIStyle(_stylePath) { normal = { textColor = activeCol } };
            GUI.Label(new Rect(rowRect.xMax - 70f, cy - 8f, 64f, 16f), activeTag, tagStyle);

            // Click
            if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
            {
                _selectedIndex = index;
                if (entry.Object != null)
                {
                    Selection.activeGameObject = entry.Object;
                    EditorGUIUtility.PingObject(entry.Object);
                }
                Event.current.Use();
            }

            if (hover && Event.current.type == EventType.MouseMove)
                Repaint();
        }

        // ── Search ─────────────────────────────────────────────────────────────
        private void RunSearch()
        {
            _results.Clear();
            _selectedIndex  = -1;
            _emptyMessage   = string.Empty;
            _lastSearchName = _targetScript != null ? _targetScript.name : string.Empty;

            Type componentType = _targetScript?.GetClass();
            if (componentType == null || !typeof(Component).IsAssignableFrom(componentType))
            {
                _emptyMessage = "This script is not a Component — nothing to find.";
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            var seen = new Dictionary<GameObject, int>();

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Component comp in root.GetComponentsInChildren(componentType, includeInactive: true))
                {
                    GameObject go = comp.gameObject;

                    if (seen.ContainsKey(go))
                    {
                        seen[go]++;
                        for (int i = 0; i < _results.Count; i++)
                        {
                            if (_results[i].Object != go) continue;
                            var e = _results[i];
                            e.ComponentCount = seen[go];
                            _results[i] = e;
                            break;
                        }
                    }
                    else
                    {
                        seen[go] = 1;
                        _results.Add(new ResultEntry
                        {
                            Object         = go,
                            ObjectName     = go.name,
                            HierarchyPath  = BuildPath(comp.transform),
                            IsActive       = go.activeInHierarchy,
                            ComponentCount = 1,
                        });
                    }
                }
            }

            if (_results.Count == 0)
                _emptyMessage = $"No GameObjects in {scene.name} have {_lastSearchName}.";

            Repaint();
        }

        // ── Helpers ────────────────────────────────────────────────────────────
        private static string BuildPath(Transform t)
        {
            var parts = new List<string>();
            while (t != null) { parts.Add(t.name); t = t.parent; }
            parts.Reverse();
            return string.Join(" › ", parts);
        }

        private void ClearResults()
        {
            _results.Clear();
            _selectedIndex  = -1;
            _emptyMessage   = string.Empty;
            _lastSearchName = null;
        }

        // ── Style setup ────────────────────────────────────────────────────────
        private void EnsureStyles()
        {
            if (_stylesInitialized && _styleName != null) return;

            _texBadgeBg = MakeTex(1, 1, ColBadgeBg);

            _styleHeader = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 13,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = Color.white },
            };

            _styleName = new GUIStyle(EditorStyles.label)
            {
                fontSize  = 12,
                fontStyle = FontStyle.Bold,
                normal    = { textColor = Color.white },
                clipping  = TextClipping.Clip,
            };

            _stylePath = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                normal   = { textColor = ColDimText },
                clipping = TextClipping.Clip,
            };

            _styleBadge = new GUIStyle(EditorStyles.label)
            {
                fontSize  = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding   = new RectOffset(6, 6, 2, 2),
                normal    = { textColor = ColBadgeText, background = _texBadgeBg },
            };

            _styleCount = new GUIStyle(EditorStyles.label)
            {
                fontSize  = 9,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = ColDimText },
            };

            _styleEmpty = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                fontSize  = 12,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = ColDimText },
            };

            _stylesInitialized = true;
        }

        private static Texture2D MakeTex(int w, int h, Color col)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.DontSave,
            };
            var pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        // ── Data ───────────────────────────────────────────────────────────────
        private struct ResultEntry
        {
            public GameObject Object;
            public string     ObjectName;
            public string     HierarchyPath;
            public bool       IsActive;
            public int        ComponentCount;
        }
    }
}
#endif
