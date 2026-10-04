/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SHUU._Editor.Utils.SceneSwitcher
{
    internal sealed class SceneSwitcherPopup : PopupWindowContent
    {
        #region Variables
        private const string FavouritesFileName = "SHUU_SceneSwitcherFavourites.txt";

        private const float SearchBarHeight = 30f;
        private const float BottomPadding = 4f;
        private const float RowHeight = 20f;
        private const float MaxListHeight = 340f;
        private const float EmptyListHeight = 40f;
        private const float MinWidth = 300f;
        private const float MaxWidth = 520f;
        private const float ScrollbarWidth = 15f;

        private const float StarSize = 14f;
        private const float StarColumn = 26f;
        private const float TextPadding = 6f;
        private const float FolderGap = 8f;
        private const int StarTextureSize = 32;

        private static readonly Color Gold = new Color(1f, 0.8f, 0.2f);


        private static SceneFavouritesStore favouritesStore;
        private static Texture2D starFilled;
        private static Texture2D starOutline;
        private static Styles styles;
        private static bool stylesAreForProSkin;


        private readonly SceneSwitcherModel model;
        private readonly string currentPath;
        private readonly Vector2 windowSize;
        private readonly SearchField searchField = new SearchField();

        private List<SceneRow> rows;
        private float contentHeight;

        private string search = "";
        private int selected = -1;
        private Vector2 scroll;
        private bool scrollToSelected;

        private string toggleGuid;
        private int activateEntry = -1;
        private bool closeRequested;


        private sealed class Styles
        {
            public readonly GUIStyle name;
            public readonly GUIStyle nameBold;
            public readonly GUIStyle nameSelected;
            public readonly GUIStyle nameBoldSelected;
            public readonly GUIStyle folder;
            public readonly GUIStyle folderSelected;
            public readonly GUIStyle header;

            public readonly Color selection;
            public readonly Color starIdle;
            public readonly Color starIdleSelected;
            public readonly Color starHover;


            public Styles()
            {
                bool pro = EditorGUIUtility.isProSkin;
                Color text = EditorStyles.label.normal.textColor;
                Color dim = pro ? new Color(0.62f, 0.62f, 0.62f) : new Color(0.38f, 0.38f, 0.38f);

                name = Text(FontStyle.Normal, text);
                nameBold = Text(FontStyle.Bold, text);
                nameSelected = Text(FontStyle.Normal, Color.white);
                nameBoldSelected = Text(FontStyle.Bold, Color.white);
                folder = Text(FontStyle.Normal, dim);
                folderSelected = Text(FontStyle.Normal, new Color(0.84f, 0.89f, 1f));
                header = Text(FontStyle.Bold, dim);
                header.fontSize = Mathf.Max(9, EditorStyles.label.fontSize - 2);

                selection = pro ? new Color32(44, 93, 135, 255) : new Color32(62, 125, 231, 255);
                starIdle = pro ? new Color(0.7f, 0.7f, 0.7f, 0.4f) : new Color(0.3f, 0.3f, 0.3f, 0.45f);
                starIdleSelected = new Color(1f, 1f, 1f, 0.6f);
                starHover = pro ? new Color(1f, 1f, 1f, 0.95f) : new Color(0.1f, 0.1f, 0.1f, 0.95f);
            }


            private static GUIStyle Text(FontStyle fontStyle, Color color)
            {
                GUIStyle style = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = fontStyle,
                    clipping = TextClipping.Clip,
                    wordWrap = false
                };

                style.normal.textColor = color;
                style.hover.textColor = color;
                style.active.textColor = color;
                style.focused.textColor = color;

                return style;
            }
        }
        #endregion




        #region Main
        public SceneSwitcherPopup(string currentPath, float anchorWidth)
        {
            this.currentPath = currentPath;

            List<SceneEntry> scenes = new List<SceneEntry>();

            foreach (string guid in AssetDatabase.FindAssets("t:SceneAsset", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!string.IsNullOrEmpty(path)) scenes.Add(new SceneEntry(guid, path));
            }

            model = new SceneSwitcherModel(scenes);

            Rebuild();
            SelectStart();

            float listHeight = rows.Count == 0 ? EmptyListHeight : Mathf.Min(contentHeight, MaxListHeight);

            windowSize = new Vector2(Mathf.Clamp(anchorWidth, MinWidth, MaxWidth), SearchBarHeight + listHeight + BottomPadding);
        }


        public override Vector2 GetWindowSize() => windowSize;

        public override void OnOpen() => searchField.SetFocus();


        public override void OnGUI(Rect rect)
        {
            if (styles == null || stylesAreForProSkin != EditorGUIUtility.isProSkin)
            {
                styles = new Styles();
                stylesAreForProSkin = EditorGUIUtility.isProSkin;
            }

            EnsureStarTextures();

            Event e = Event.current;

            toggleGuid = null;
            activateEntry = -1;
            closeRequested = false;


            Rect searchRect = new Rect(rect.x + 6f, rect.y + 6f, rect.width - 12f, 18f);
            Rect listRect = new Rect(rect.x, rect.y + SearchBarHeight, rect.width, rect.height - SearchBarHeight - BottomPadding);

            HandleKeys(e, listRect.height);

            string typed = searchField.OnGUI(searchRect, search);

            if (typed != search)
            {
                search = typed;

                OnSearchChanged();
            }

            DrawList(listRect, e);

            ApplyDecisions();
        }
        #endregion




        #region Input
        private void HandleKeys(Event e, float listHeight)
        {
            if (e.type != EventType.KeyDown) return;

            switch (e.keyCode)
            {
                case KeyCode.DownArrow:
                    MoveSelection(1, listHeight);
                    e.Use();
                    break;

                case KeyCode.UpArrow:
                    MoveSelection(-1, listHeight);
                    e.Use();
                    break;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (selected >= 0) activateEntry = selected;
                    e.Use();
                    break;

                case KeyCode.Escape:
                    if (search.Length > 0)
                    {
                        search = "";
                        OnSearchChanged();
                    }
                    else closeRequested = true;

                    e.Use();
                    break;
            }
        }


        private void MoveSelection(int direction, float listHeight)
        {
            int row = RowOf(selected);

            int next = row < 0
                ? (direction > 0 ? NextSceneRow(-1, 1) : NextSceneRow(rows.Count, -1))
                : NextSceneRow(row, direction);

            if (next < 0) return;

            selected = rows[next].entry;

            EnsureVisible(next, listHeight);
        }


        private void ApplyDecisions()
        {
            if (toggleGuid != null)
            {
                FavouritesStore.Toggle(toggleGuid);

                if (FavouritesStore.SaveError != null) Debug.LogWarning($"[SHUU] Couldn't save the scene favourites: {FavouritesStore.SaveError}");

                Rebuild();

                editorWindow.Repaint();
            }

            if (activateEntry >= 0)
            {
                string path = model.Entries[activateEntry].path;

                editorWindow.Close();

                if (path != currentPath) EditorApplication.delayCall += () => HierarchySceneSwitcher.SwitchTo(path);

                return;
            }

            if (closeRequested) editorWindow.Close();
        }
        #endregion




        #region List
        private void DrawList(Rect listRect, Event e)
        {
            if (rows.Count == 0)
            {
                GUI.Label(listRect, model.Entries.Count == 0 ? "There are no scenes in the project" : $"No scenes match '{search.Trim()}'", EditorStyles.centeredGreyMiniLabel);

                return;
            }


            bool needsScrollbar = contentHeight > listRect.height;
            Rect viewRect = new Rect(0f, 0f, listRect.width - (needsScrollbar ? ScrollbarWidth : 0f), contentHeight);

            scroll.y = Mathf.Clamp(scroll.y, 0f, Mathf.Max(0f, contentHeight - listRect.height));

            if (scrollToSelected)
            {
                scrollToSelected = false;

                int selectedRow = RowOf(selected);
                if (selectedRow >= 0) EnsureVisible(selectedRow, listRect.height);
            }


            bool mouseInList = listRect.Contains(e.mousePosition);

            scroll = GUI.BeginScrollView(listRect, scroll, viewRect, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

            float visibleTop = scroll.y;
            float visibleBottom = scroll.y + listRect.height;

            for (int i = 0; i < rows.Count; i++)
            {
                float top = i * RowHeight;

                if (top + RowHeight < visibleTop || top > visibleBottom) continue;

                Rect rowRect = new Rect(0f, top, viewRect.width, RowHeight);

                if (rows[i].IsHeader)
                {
                    if (e.type == EventType.Repaint) GUI.Label(new Rect(rowRect.x + 8f, rowRect.y, rowRect.width - 8f, rowRect.height), rows[i].title, styles.header);
                }
                else DrawScene(rowRect, rows[i].entry, e, mouseInList);
            }

            GUI.EndScrollView();
        }


        private void DrawScene(Rect rowRect, int entryIndex, Event e, bool mouseInList)
        {
            SceneEntry scene = model.Entries[entryIndex];

            bool mouseOver = mouseInList && rowRect.Contains(e.mousePosition);
            Rect starHit = new Rect(rowRect.x, rowRect.y, StarColumn, rowRect.height);

            switch (e.type)
            {
                case EventType.MouseMove:
                    if (mouseOver && selected != entryIndex)
                    {
                        selected = entryIndex;

                        editorWindow.Repaint();
                    }
                    break;

                case EventType.MouseDown:
                    if (e.button == 0 && mouseOver)
                    {
                        if (starHit.Contains(e.mousePosition)) toggleGuid = scene.guid;
                        else activateEntry = entryIndex;

                        e.Use();
                    }
                    break;

                case EventType.Repaint:
                    PaintScene(rowRect, scene, entryIndex == selected, mouseOver && starHit.Contains(e.mousePosition));
                    break;
            }
        }


        private void PaintScene(Rect rowRect, SceneEntry scene, bool isSelected, bool starHovered)
        {
            bool isCurrent = scene.path == currentPath;
            bool isFavourite = FavouritesStore.Contains(scene.guid);

            if (isSelected) EditorGUI.DrawRect(rowRect, styles.selection);


            Rect starRect = new Rect(rowRect.x + (StarColumn - StarSize) * 0.5f, rowRect.y + (rowRect.height - StarSize) * 0.5f, StarSize, StarSize);

            Color tint;
            if (isFavourite) tint = starHovered ? new Color(Gold.r, Gold.g, Gold.b, 0.65f) : Gold;
            else tint = starHovered ? styles.starHover : (isSelected ? styles.starIdleSelected : styles.starIdle);

            Color previousColor = GUI.color;
            GUI.color = tint;
            GUI.DrawTexture(starRect, isFavourite ? starFilled : starOutline, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;


            GUIStyle nameStyle = isSelected ? (isCurrent ? styles.nameBoldSelected : styles.nameSelected) : (isCurrent ? styles.nameBold : styles.name);

            Rect textRect = new Rect(rowRect.x + StarColumn, rowRect.y, rowRect.width - StarColumn - TextPadding, rowRect.height);
            GUIContent nameContent = new GUIContent(scene.name);

            GUI.Label(textRect, nameContent, nameStyle);

            if (scene.folder.Length == 0) return;

            float folderX = textRect.x + nameStyle.CalcSize(nameContent).x + FolderGap;
            Rect folderRect = new Rect(folderX, textRect.y, textRect.xMax - folderX, textRect.height);

            if (folderRect.width > 24f) GUI.Label(folderRect, scene.folder, isSelected ? styles.folderSelected : styles.folder);
        }
        #endregion




        #region Logic
        private static SceneFavouritesStore FavouritesStore
        {
            get
            {
                if (favouritesStore == null)
                {
                    string projectFolder = Path.GetDirectoryName(Application.dataPath);

                    favouritesStore = new SceneFavouritesStore(Path.Combine(projectFolder, "UserSettings", FavouritesFileName));
                }

                return favouritesStore;
            }
        }


        private void Rebuild()
        {
            rows = model.Build(search, guid => FavouritesStore.Contains(guid));

            contentHeight = rows.Count * RowHeight;
        }

        private void OnSearchChanged()
        {
            Rebuild();

            scroll = Vector2.zero;

            if (search.Trim().Length == 0) SelectStart();
            else
            {
                int first = NextSceneRow(-1, 1);

                selected = first < 0 ? -1 : rows[first].entry;
            }
        }

        private void SelectStart()
        {
            selected = -1;

            for (int i = 0; i < rows.Count; i++)
            {
                if (!rows[i].IsHeader && model.Entries[rows[i].entry].path == currentPath)
                {
                    selected = rows[i].entry;
                    break;
                }
            }

            if (selected < 0)
            {
                int first = NextSceneRow(-1, 1);

                if (first >= 0) selected = rows[first].entry;
            }

            scrollToSelected = true;
        }


        private int RowOf(int entry)
        {
            if (entry < 0) return -1;

            for (int i = 0; i < rows.Count; i++)
                if (rows[i].entry == entry) return i;

            return -1;
        }

        private int NextSceneRow(int from, int step)
        {
            for (int i = from + step; i >= 0 && i < rows.Count; i += step)
                if (!rows[i].IsHeader) return i;

            return -1;
        }

        private void EnsureVisible(int row, float listHeight)
        {
            float top = row * RowHeight;
            float bottom = top + RowHeight;

            if (row > 0 && rows[row - 1].IsHeader) top -= RowHeight;

            if (top < scroll.y) scroll.y = top;
            else if (bottom > scroll.y + listHeight) scroll.y = bottom - listHeight;
        }


        private static void EnsureStarTextures()
        {
            if (starFilled == null) starFilled = CreateStarTexture(true);
            if (starOutline == null) starOutline = CreateStarTexture(false);
        }

        private static Texture2D CreateStarTexture(bool filled)
        {
            byte[] bytes = SceneSwitcherStar.Render(StarTextureSize, filled);
            Color32[] pixels = new Color32[StarTextureSize * StarTextureSize];

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(bytes[i * 4], bytes[i * 4 + 1], bytes[i * 4 + 2], bytes[i * 4 + 3]);

            Texture2D texture = new Texture2D(StarTextureSize, StarTextureSize, TextureFormat.RGBA32, true, true)
            {
                name = filled ? "SHUU Scene Switcher Star (filled)" : "SHUU Scene Switcher Star (outline)",
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear
            };

            texture.SetPixels32(pixels);
            texture.Apply(true, true);

            return texture;
        }
        #endregion
    }
}
#endif
