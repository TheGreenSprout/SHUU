#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

using SHUU.Utils.SettingsSystem;
using SHUU._Editor.CodeGeneration;

using SETB;
using SETB.SuperClasses;

using static SETB.EditorGUI_Base;
using static SETB.HandyEditorFunctions;
using static SETB.EditorGUI_RowList;

namespace SHUU._Editor.Drawers
{
    public class SettingsDataEditorWindow : EditorWindow_Base<SettingsDataEditorWindow>
    {
        #region Variables

        #region Layout constants
        private const float ToolbarH = 22f;
        private const float MapColW = 180f;
        private const float FieldColW = 220f;
        private const float DividerW = 2f;
        private const float RowH = 22f;
        private const float Padding = 6f;
        private const float FooterH = 24f;
        private const float LineH = 18f;
        private const float LineGap = 4f;
        #endregion



        #region Fields
        [SerializeField] private SettingsAtlas asset;

        [SerializeField] private bool isDirty;

        [SerializeField] private int selectedMapIndex = -1;
        [SerializeField] private int selectedGroupIndex = -1;
        [SerializeField] private int selectedFieldIndex = -1;

        [SerializeField] private Vector2 mapScroll;
        [SerializeField] private Vector2 fieldScroll;
        [SerializeField] private Vector2 detailScroll;
        [SerializeField] private float detailContentH = 200f;


        [SerializeField] private int renamingMapIndex = -1;
        [SerializeField] private int renamingGroupIndex = -1;
        [SerializeField] private int renamingFieldGroupIndex = -1;
        [SerializeField] private int renamingFieldIndex = -1;
        [SerializeField] private bool renamingAssetName;
        [SerializeField] private string renameBuffer = "";


        [SerializeField] private List<string> collapsedGroupKeys = new();


        private readonly DragReorderController mapDrag = new();
        private readonly DragReorderController fieldDrag = new();

        private int fieldDragGroupIndex;
        #endregion

        #endregion




        #region Main
        public static void Open(SettingsAtlas asset)
        {
            var window = CreateWindow("Settings Editor", minWidth: 640, minHeight: 420);

            if (!window.TryLoad(asset))
            {
                if (window.asset != null) window.titleContent = new GUIContent($"Settings — {window.asset.settingsName}");
                window.Focus();

                return;
            }

            window.Focus();
        }

        private bool TryLoad(SettingsAtlas newAsset)
        {
            if (newAsset == asset) return true;

            if (!ConfirmDiscardChanges("switching assets")) return false;

            Load(newAsset);
            return true;
        }

        private void Load(SettingsAtlas newAsset)
        {
            asset = newAsset;
            isDirty = false;
            selectedMapIndex = newAsset.maps.Count > 0 ? 0 : -1;
            selectedGroupIndex = -1;
            selectedFieldIndex = -1;
            renamingMapIndex = -1;
            renamingGroupIndex = -1;
            renamingFieldGroupIndex = -1;
            renamingFieldIndex = -1;
            renamingAssetName = false;
            titleContent = new GUIContent($"Settings — {newAsset.settingsName}");
        }


        protected override void OnEnable()
        {
            base.OnEnable();

            if (asset != null) titleContent = new GUIContent($"Settings — {asset.settingsName}");
        }

        protected override void OnDisable() => base.OnDisable();


        protected override void OnDestroy()
        {
            if (isDirty && asset != null)
            {
                bool save = EditorUtility.DisplayDialog(
                    "Unsaved Changes",
                    $"Settings asset \"{asset.settingsName}\" has unsaved changes.\nSave before closing?",
                    "Save", "Discard");

                if (save) SaveAsset();
            }
        }


        private bool ConfirmDiscardChanges(string actionDescription) => HandyEditorFunctions.ConfirmDiscardChanges(
            isDirty && asset != null,
            "Unsaved Changes",
            $"Settings asset \"{asset?.settingsName}\"",
            actionDescription, SaveAsset);


        private void OnGUI()
        {
            if (asset == null)
            {
                Space(20);
                DrawHelpBox("No asset loaded.\nOpen or double-click a SettingsAtlas asset.", MessageType.Info);

                return;
            }

            var bands = SplitRows(new Rect(0, 0, position.width, position.height), 0f, ToolbarH, Remaining, FooterH);
            Rect toolbarBand = bands[0];
            Rect bodyBand = bands[1];
            Rect footerBand = bands[2];

            DrawToolbar(toolbarBand);

            var cols = SplitColumns(bodyBand, DividerW, MapColW, FieldColW, Remaining);
            Rect mapCol = cols[0];
            Rect fieldCol = cols[1];
            Rect detailCol = cols[2];

            DrawRect(new Rect(mapCol.xMax, bodyBand.y, DividerW, bodyBand.height), new Color(0.1f, 0.1f, 0.1f, 1f));
            DrawRect(new Rect(fieldCol.xMax, bodyBand.y, DividerW, bodyBand.height), new Color(0.1f, 0.1f, 0.1f, 1f));

            DrawMapColumn(mapCol);
            DrawFieldColumn(fieldCol);
            DrawDetailColumn(detailCol);

            DrawFooter(footerBand);

            if (Event.current.type == EventType.MouseUp)
            {
                mapDrag.Cancel();
                fieldDrag.Cancel();

                Repaint();
            }
        }
        #endregion



        #region Logic

        #region Save
        private void SaveAsset()
        {
            if (asset == null) return;

            UtilitySetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
            isDirty = false;
            Repaint();

            if (asset.generateStaticReferences) SettingsSystem_StaticReferencesGenerator.GenerateForAtlas(asset);
        }

        private void MarkDirty()
        {
            isDirty = true;

            Repaint();
        }
        #endregion



        #region Toolbar
        private void DrawToolbar(Rect bar)
        {
            GUI.Box(bar, GUIContent.none, EditorStyles.toolbar);

            const float gap = 6f;
            const float saveW = 54f;
            const float genW = 190f;
            const float restoreDefW = 100f;
            const float saveDefW = 90f;
            const float dotW = 14f;

            var btn = PackRightToLeft(bar, gap, 1f, saveDefW, restoreDefW, genW, saveW);
            Rect saveDefRect = btn[0];
            Rect restoreDefRect = btn[1];
            Rect genRect = btn[2];
            Rect saveRect = btn[3];

            Rect dotRect = new Rect(saveDefRect.x - gap - dotW, 0, dotW, ToolbarH);
            Rect nameRect = new Rect(4f, 1f, dotRect.x - gap - 4f, ToolbarH - 2f);

            if (renamingAssetName) DrawInlineRename(nameRect, -1, RenameTarget.AssetName);
            else
            {
                bool hasName = !string.IsNullOrWhiteSpace(asset.settingsName);
                GUIStyle nameStyle = new GUIStyle(EditorStyles.toolbarTextField) { alignment = TextAnchor.MiddleLeft };

                if (hasName)
                {
                    nameStyle.fontStyle = FontStyle.Bold;
                    GUI.Label(nameRect, asset.settingsName, nameStyle);
                }
                else
                {
                    Color faded = nameStyle.normal.textColor;
                    faded.a *= 0.5f;
                    nameStyle.normal.textColor = faded;
                    GUI.Label(nameRect, "Set name", nameStyle);
                }

                if (Event.current.type == EventType.MouseDown && nameRect.Contains(Event.current.mousePosition))
                {
                    renamingAssetName = true;
                    renamingMapIndex = -1;
                    renamingFieldIndex = -1;
                    renameBuffer = asset.settingsName;

                    Event.current.Use();
                }
            }

            if (isDirty)
            {
                GUIStyle dot = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(1f, 0.55f, 0.1f) }, alignment = TextAnchor.MiddleCenter };
                GUI.Label(dotRect, "●", dot);
            }

            string lastSet = string.IsNullOrWhiteSpace(asset.lastDefaultSetDateTime) ? "Never" : asset.lastDefaultSetDateTime;
            string defaultsTooltip = $"Last saved: {lastSet}";

            if (GUI.Button(saveDefRect, new GUIContent("Save Defaults", defaultsTooltip), EditorStyles.toolbarButton)) SaveWholeAssetDefaults();

            using (new EditorGUI.DisabledScope(asset.defaultData == null || !asset.defaultData.hasValue))
                if (GUI.Button(restoreDefRect, new GUIContent("Restore Defaults", defaultsTooltip), EditorStyles.toolbarButton)) RestoreWholeAssetDefaults();

            if (GUI.Button(genRect, "Generate Static References", EditorStyles.toolbarButton)) SettingsSystem_StaticReferencesGenerator.GenerateForAtlas(asset);

            using (new EditorGUI.DisabledScope(!isDirty))
                if (GUI.Button(saveRect, "Save", EditorStyles.toolbarButton)) SaveAsset();
        }
        #endregion



        #region Map column (left)
        private void DrawMapColumn(Rect col)
        {
            EditorGUI.DrawRect(col, new Color(0.17f, 0.17f, 0.17f, 1f));

            float mouseY = DrawScrollableRowList(
                col, RowH, RowH, asset.maps.Count, ref mapScroll,
                DrawMapRow,
                "Setting Maps", AddMap,
                headerBgColor: new Color(0.13f, 0.13f, 0.13f, 1f),
                extraScrollContent: () =>
                {
                    if (mapDrag.Active) mapDrag.DrawInsertionLine(Event.current.mousePosition.y, (int)RowH, asset.maps.Count, col.width - 14f);
                });

            if (mapDrag.Active && Event.current.type == EventType.MouseUp)
            {
                if (mapDrag.TryEnd(mouseY, (int)RowH, asset.maps.Count, out int to)) ReorderMap(mapDrag.FromIndex, to);

                Event.current.Use();

                Repaint();
            }
        }

        private void DrawMapRow(RowContext ctx)
        {
            int i = ctx.index;
            if (i >= asset.maps.Count) return;
            float w = ctx.rect.width;
            bool selected = i == selectedMapIndex;

            DrawRowBackground(ctx.rect, selected, ctx.alternateStripe);

            if (mapDrag.Active && mapDrag.FromIndex == i) EditorGUI.DrawRect(ctx.rect, new Color(1f, 1f, 1f, 0.06f));

            float delW = 18f;
            Rect del = new Rect(w - delW - 2f, ctx.rect.y + 2f, delW, RowH - 4f);
            Rect name = new Rect(Padding, ctx.rect.y + 2f, w - Padding - delW - 6f, RowH - 4f);

            if (renamingMapIndex == i) DrawInlineRename(name, i, RenameTarget.Map);
            else
            {
                GUIStyle lbl = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = selected ? Color.white : GUI.skin.label.normal.textColor },
                    alignment = TextAnchor.MiddleLeft
                };
                GUI.Label(name, asset.maps[i].mapName, lbl);

                if (Event.current.type == EventType.MouseDown && ctx.rect.Contains(Event.current.mousePosition) && !del.Contains(Event.current.mousePosition))
                {
                    if (Event.current.clickCount == 2)
                    {
                        renamingMapIndex = i;
                        renameBuffer = asset.maps[i].mapName;
                    }
                    else
                    {
                        SelectMap(i);

                        mapDrag.Begin(i);
                    }

                    Event.current.Use();
                }
            }

            GUIStyle xBtn = new GUIStyle(EditorStyles.miniButton) { normal = { textColor = new Color(0.85f, 0.35f, 0.35f) }, padding = new RectOffset(0,0,0,0) };
            if (GUI.Button(del, "x", xBtn)) DeleteMap(i);
        }

        #endregion



        #region Field column (middle)
        private void DrawFieldColumn(Rect col)
        {
            EditorGUI.DrawRect(col, new Color(0.19f, 0.19f, 0.19f, 1f));

            SettingMap map = SelectedMap();

            Rect hdr = new Rect(col.x, col.y, col.width, RowH);
            EditorGUI.DrawRect(hdr, new Color(0.13f, 0.13f, 0.13f, 1f));

            float addFieldW = 20f;
            float addGroupW = 56f;
            Rect addFieldBtn = new Rect(hdr.xMax - addFieldW - 2f, hdr.y + 2f, addFieldW, RowH - 4f);
            Rect addGroupBtn = new Rect(addFieldBtn.x - 4f - addGroupW, hdr.y + 2f, addGroupW, RowH - 4f);

            GUI.Label(new Rect(hdr.x + Padding, hdr.y + 3f, addGroupBtn.x - hdr.x - Padding - 4f, RowH), map != null ? map.mapName : "Fields", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(map == null))
            {
                if (GUI.Button(addGroupBtn, "+Group", EditorStyles.miniButton)) AddGroup();

                if (GUI.Button(addFieldBtn, "+", EditorStyles.miniButton)) AddField();
            }

            if (map == null)
            {
                GUI.Label(new Rect(col.x, col.y + RowH + 10f, col.width, 30f), "← Select a map", EditorStyles.centeredGreyMiniLabel);

                return;
            }

            List<FieldRowInfo> rows = BuildFieldRows(map);

            Rect body = new Rect(col.x, col.y + RowH, col.width, col.height - RowH);
            float innerW = col.width - 14f;

            var perGroupCursor = new int[map.groups.Count];
            int ungroupedCursor = 0;

            float mouseY = DrawScrollableRowList(
                body, 0f, RowH, rows.Count, ref fieldScroll,
                ctx =>
                {
                    var info = rows[ctx.index];
                    if (info.IsHeader) DrawGroupHeaderRow(map, info.GroupIndex, ctx.index, innerW);
                    else if (info.GroupIndex < 0)
                    {
                        DrawFieldRow(map, -1, ungroupedCursor, ctx.index, innerW);
                        ungroupedCursor++;
                    }
                    else
                    {
                        DrawFieldRow(map, info.GroupIndex, perGroupCursor[info.GroupIndex], ctx.index, innerW);
                        perGroupCursor[info.GroupIndex]++;
                    }
                },
                extraScrollContent: () =>
                {
                    if (fieldDrag.Active && (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout || Event.current.type == EventType.MouseDrag))
                    {
                        float my = Event.current.mousePosition.y;
                        int rawRow = (int)(my / RowH);
                        int clamped = Mathf.Clamp(rawRow, 0, rows.Count - 1);
                        float lineY;
                        int lineGroup;
                        if (rows.Count == 0)
                        {
                            lineY = 0;
                            lineGroup = -1;
                        }
                        else if (rows[clamped].IsHeader)
                        {
                            float posInRow = my - clamped * RowH;
                            if (posInRow < RowH * 0.5f)
                            {
                                lineY = clamped * RowH;
                                lineGroup = -1;
                            }
                            else
                            {
                                lineY = (clamped + 1) * RowH;
                                lineGroup = rows[clamped].GroupIndex;
                            }
                        }
                        else
                        {
                            lineY = Mathf.Clamp(rawRow, 0, rows.Count) * RowH;
                            lineGroup = rows[clamped].GroupIndex;
                        }
                        float indent = lineGroup >= 0 ? 14f : 0f;
                        EditorGUI.DrawRect(new Rect(indent, lineY - 1f, innerW - indent, 2f), new Color(0.25f, 0.65f, 1f, 1f));
                    }
                });

            if (fieldDrag.Active && Event.current.type == EventType.MouseUp)
            {
                ResolveDropTarget(rows, mouseY, RowH, map, out int targetGroup, out int targetIndex);
                MoveOrReorderField(map, fieldDragGroupIndex, fieldDrag.FromIndex, targetGroup, targetIndex);
                fieldDrag.Cancel();

                Event.current.Use();

                Repaint();
            }
        }

        private struct FieldRowInfo
        {
            public bool IsHeader;
            public int GroupIndex;
        }

        private List<FieldRowInfo> BuildFieldRows(SettingMap map)
        {
            var rows = new List<FieldRowInfo>();

            for (int i = 0; i < map.fields.Count; i++)
                rows.Add(new FieldRowInfo { IsHeader = false, GroupIndex = -1 });

            for (int g = 0; g < map.groups.Count; g++)
            {
                rows.Add(new FieldRowInfo { IsHeader = true, GroupIndex = g });

                if (!IsGroupCollapsed(map, map.groups[g]))
                {
                    for (int i = 0; i < map.groups[g].fields.Count; i++)
                        rows.Add(new FieldRowInfo { IsHeader = false, GroupIndex = g });
                }
            }

            return rows;
        }

        private void DrawGroupHeaderRow(SettingMap map, int g, int visualRow, float w)
        {
            if (g >= map.groups.Count) return;
            SettingGroup group = map.groups[g];
            Rect row = new Rect(0, visualRow * RowH, w, RowH);

            EditorGUI.DrawRect(row, new Color(0.16f, 0.16f, 0.16f, 1f));

            bool collapsed = IsGroupCollapsed(map, group);

            float foldW = 16f;
            float delW = 18f;
            float addW = 20f;
            Rect fold = new Rect(2f, row.y + 1f, foldW, RowH - 2f);
            Rect del = new Rect(w - delW - 2f, row.y + 2f, delW, RowH - 4f);
            Rect add = new Rect(del.x - 2f - addW, row.y + 2f, addW, RowH - 4f);
            Rect name = new Rect(fold.xMax + 2f, row.y + 2f, add.x - fold.xMax - 6f, RowH - 4f);

            if (GUI.Button(fold, collapsed ? "▸" : "▾", EditorStyles.label)) ToggleGroupCollapsed(map, group);

            if (renamingGroupIndex == g) DrawInlineRename(name, g, RenameTarget.Group);
            else
            {
                GUI.Label(name, group.groupName, EditorStyles.boldLabel);

                if (Event.current.type == EventType.MouseDown && name.Contains(Event.current.mousePosition))
                {
                    if (Event.current.clickCount == 2)
                    {
                        renamingGroupIndex = g;
                        renameBuffer = group.groupName;
                    }
                    else ToggleGroupCollapsed(map, group);

                    Event.current.Use();
                }
            }

            GUIStyle addBtnSt = new GUIStyle(EditorStyles.miniButton) { padding = new RectOffset(0, 0, 0, 0) };
            if (GUI.Button(add, "+", addBtnSt)) AddFieldToGroup(map, g);

            GUIStyle xBtn = new GUIStyle(EditorStyles.miniButton) { normal = { textColor = new Color(0.85f, 0.35f, 0.35f) }, padding = new RectOffset(0, 0, 0, 0) };
            if (GUI.Button(del, "x", xBtn)) DeleteGroup(map, g);
        }

        private void DrawFieldRow(SettingMap map, int groupIndex, int i, int visualRow, float w)
        {
            List<SettingField> list = ContainerFields(map, groupIndex);
            if (list == null || i >= list.Count) return;

            Rect row = new Rect(0, visualRow * RowH, w, RowH);
            bool selected = groupIndex == selectedGroupIndex && i == selectedFieldIndex;

            DrawRowBackground(row, selected, i % 2 != 0);

            if (fieldDrag.Active && fieldDragGroupIndex == groupIndex && fieldDrag.FromIndex == i) EditorGUI.DrawRect(row, new Color(1f, 1f, 1f, 0.06f));

            float indent = groupIndex < 0 ? 0f : 14f;
            float delW = 18f;
            float badgeW = 48f;
            Rect del = new Rect(w - delW - 2f, row.y + 2f, delW, RowH - 4f);
            Rect badge = new Rect(w - delW - badgeW - 6f, row.y + 3f, badgeW, RowH - 6f);
            Rect name = new Rect(Padding + indent, row.y + 2f, w - Padding - indent - delW - badgeW - 10f, RowH - 4f);

            bool renaming = renamingFieldGroupIndex == groupIndex && renamingFieldIndex == i;

            if (renaming)
            {
                DrawInlineRename(name, i, RenameTarget.Field);
            }
            else
            {
                GUIStyle lbl = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = selected ? Color.white : GUI.skin.label.normal.textColor },
                    alignment = TextAnchor.MiddleLeft
                };
                string key = string.IsNullOrWhiteSpace(list[i].key) ? $"<Field {i}>" : list[i].key;
                GUI.Label(name, key, lbl);

                GUIStyle badgeSt = new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = TypeColor(list[i].type) },
                    alignment = TextAnchor.MiddleRight
                };
                GUI.Label(badge, list[i].type.ToString(), badgeSt);

                if (Event.current.type == EventType.MouseDown && row.Contains(Event.current.mousePosition) &&
                    !del.Contains(Event.current.mousePosition))
                {
                    if (Event.current.clickCount == 2)
                    {
                        SelectField(groupIndex, i);
                        renamingFieldGroupIndex = groupIndex;
                        renamingFieldIndex = i;
                        renameBuffer = list[i].key;
                    }
                    else
                    {
                        SelectField(groupIndex, i);
                        fieldDrag.Begin(i);
                        fieldDragGroupIndex = groupIndex;
                    }
                    Event.current.Use();
                }
            }

            GUIStyle xBtn = new GUIStyle(EditorStyles.miniButton)
                { normal = { textColor = new Color(0.85f, 0.35f, 0.35f) }, padding = new RectOffset(0,0,0,0) };
            if (GUI.Button(del, "x", xBtn)) DeleteField(map, groupIndex, i);
        }

        #endregion



        #region Detail column (right)

        private void DrawDetailColumn(Rect col)
        {
            EditorGUI.DrawRect(col, new Color(0.21f, 0.21f, 0.21f, 1f));

            SettingMap map = SelectedMap();
            SettingField field = SelectedField(map);

            Rect hdr = new Rect(col.x, col.y, col.width, RowH);
            EditorGUI.DrawRect(hdr, new Color(0.13f, 0.13f, 0.13f, 1f));
            string hdrTitle = field != null && !string.IsNullOrWhiteSpace(field.key) ? field.key : "Properties";
            GUI.Label(new Rect(hdr.x + Padding, hdr.y + 3f, hdr.width - Padding * 2, RowH), hdrTitle, EditorStyles.boldLabel);

            if (map == null)
            {
                GUI.Label(new Rect(col.x, col.y + RowH + 10f, col.width, 30f),
                    "← Select a map", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            Rect scroll = new Rect(col.x, col.y + RowH, col.width, col.height - RowH);
            float innerW = scroll.width - Padding * 2 - 14f;

            detailScroll = GUI.BeginScrollView(scroll, detailScroll,
                new Rect(0, 0, scroll.width - 14f, Mathf.Max(detailContentH, scroll.height)));

            float x = Padding;
            float y = Padding;
            float lw = 82f;

            if (field != null)
            {
                EditorGUI.LabelField(new Rect(x, y, lw, LineH), "Group");
                string[] groupOptions = new[] { "<None>" }.Concat(map.groups.Select(g => g.groupName)).ToArray();
                int currentOption = selectedGroupIndex < 0 ? 0 : selectedGroupIndex + 1;
                EditorGUI.BeginChangeCheck();
                int newOption = EditorGUI.Popup(new Rect(x + lw, y, innerW - lw, LineH), currentOption, groupOptions);
                if (EditorGUI.EndChangeCheck())
                    MoveFieldToGroup(map, selectedGroupIndex, selectedFieldIndex, newOption - 1);
                y += LineH + LineGap * 2;

                EditorGUI.LabelField(new Rect(x, y, lw, LineH), "Type");
                EditorGUI.BeginChangeCheck();
                var newType = (SettingType)EditorGUI.EnumPopup(new Rect(x + lw, y, innerW - lw, LineH), field.type);
                if (EditorGUI.EndChangeCheck()) { field.type = newType; MarkDirty(); }
                y += LineH + LineGap * 2;

                EditorGUI.LabelField(new Rect(x, y, innerW, LineH), "Value", EditorStyles.boldLabel);
                y += LineH + LineGap;
                y = DrawDetailValue(field, x, y, innerW, lw);

                if (field.type == SettingType.Int || field.type == SettingType.Float)
                {
                    y += LineGap;
                    EditorGUI.LabelField(new Rect(x, y, innerW, LineH), "Bounds", EditorStyles.boldLabel);
                    y += LineH + LineGap;
                    y = DrawDetailBounds(field, x, y, innerW, lw);
                }

                y += LineGap * 2;
            }
            else
            {
                GUI.Label(new Rect(x, y, innerW, 20f), "← Select a field to edit its properties",
                    EditorStyles.centeredGreyMiniLabel);
                y += 20f + LineGap * 2;
            }

            detailContentH = DrawDetailMapDefaults(map, x, y, innerW);

            GUI.EndScrollView();
        }

        private float DrawDetailValue(SettingField f, float x, float y, float w, float lw)
        {
            switch (f.type)
            {
                case SettingType.Bool:
                    EditorGUI.LabelField(new Rect(x, y, lw, LineH), "Value");
                    EditorGUI.BeginChangeCheck();
                    bool bv = EditorGUI.Toggle(new Rect(x + lw, y, w - lw, LineH), f.boolValue);
                    if (EditorGUI.EndChangeCheck()) { f.boolValue = bv; MarkDirty(); }
                    return y + LineH + LineGap;

                case SettingType.Int:
                {
                    bool slider = f.useMin && f.useMax && f.intMin < f.intMax;
                    EditorGUI.LabelField(new Rect(x, y, lw, LineH), "Value");
                    EditorGUI.BeginChangeCheck();
                    int iv = slider
                        ? EditorGUI.IntSlider(new Rect(x + lw, y, w - lw, LineH), f.intValue, f.intMin, f.intMax)
                        : EditorGUI.IntField(new Rect(x + lw, y, w - lw, LineH), f.intValue);
                    if (EditorGUI.EndChangeCheck()) { f.intValue = iv; MarkDirty(); }
                    return y + LineH + LineGap;
                }

                case SettingType.Float:
                {
                    bool slider = f.useMin && f.useMax && f.floatMin < f.floatMax;
                    EditorGUI.LabelField(new Rect(x, y, lw, LineH), "Value");
                    EditorGUI.BeginChangeCheck();
                    float fv = slider
                        ? EditorGUI.Slider(new Rect(x + lw, y, w - lw, LineH), f.floatValue, f.floatMin, f.floatMax)
                        : EditorGUI.FloatField(new Rect(x + lw, y, w - lw, LineH), f.floatValue);
                    if (EditorGUI.EndChangeCheck()) { f.floatValue = fv; MarkDirty(); }
                    return y + LineH + LineGap;
                }

                case SettingType.String:
                    EditorGUI.LabelField(new Rect(x, y, lw, LineH), "Value");
                    EditorGUI.BeginChangeCheck();
                    string sv = EditorGUI.TextField(new Rect(x + lw, y, w - lw, LineH), f.stringValue);
                    if (EditorGUI.EndChangeCheck()) { f.stringValue = sv; MarkDirty(); }
                    return y + LineH + LineGap;

                case SettingType.Enum:
                    return DrawDetailEnum(f, x, y, w, lw);

                default:
                    return y;
            }
        }

        private float DrawDetailEnum(SettingField f, float x, float y, float w, float lw)
        {
            EditorGUI.LabelField(new Rect(x, y, lw, LineH), "Enum Type");
            float btnW = 24f;
            float fieldW = w - lw - btnW - 4f;
            EditorGUI.BeginChangeCheck();
            string newName = EditorGUI.TextField(new Rect(x + lw, y, fieldW, LineH), f.enumTypeName);
            if (EditorGUI.EndChangeCheck()) { f.enumTypeName = newName; f.enumValue = 0; MarkDirty(); }
            if (GUI.Button(new Rect(x + lw + fieldW + 4f, y, btnW, LineH), "…"))
            {
                SettingField captured = f;
                EnumTypePickerWindow.ShowWithCallback(fullName =>
                {
                    Record(asset,"Set Enum Type");
                    captured.enumTypeName = fullName;
                    captured.enumValue = 0;
                    MarkDirty();
                    Repaint();
                }, f.enumTypeName);
            }
            y += LineH + LineGap;

            Type resolved = ResolveEnumType(f.enumTypeName);
            if (resolved != null)
            {
                string[] names = Enum.GetNames(resolved);
                Array values = Enum.GetValues(resolved);
                int[] intValues = new int[values.Length];
                for (int i = 0; i < values.Length; i++) intValues[i] = Convert.ToInt32(values.GetValue(i));

                int curIdx = Mathf.Max(0, Array.IndexOf(intValues, f.enumValue));
                EditorGUI.LabelField(new Rect(x, y, lw, LineH), "Value");
                EditorGUI.BeginChangeCheck();
                int newIdx = EditorGUI.Popup(new Rect(x + lw, y, w - lw, LineH), curIdx, names);
                if (EditorGUI.EndChangeCheck()) { f.enumValue = intValues[newIdx]; MarkDirty(); }
                y += LineH + LineGap;
            }
            else
            {
                EditorGUI.LabelField(new Rect(x, y, lw, LineH), "Value (int)");
                EditorGUI.BeginChangeCheck();
                int raw = EditorGUI.IntField(new Rect(x + lw, y, w - lw, LineH), f.enumValue);
                if (EditorGUI.EndChangeCheck()) { f.enumValue = raw; MarkDirty(); }
                y += LineH + LineGap;

                if (!string.IsNullOrWhiteSpace(f.enumTypeName))
                {
                    EditorGUI.HelpBox(new Rect(x, y, w, LineH * 2f),
                        $"Type \"{f.enumTypeName}\" not found. Enter the fully-qualified name.", MessageType.Warning);
                    y += LineH * 2f + LineGap;
                }
            }

            return y;
        }

        private float DrawDetailBounds(SettingField f, float x, float y, float w, float lw)
        {
            bool isInt = f.type == SettingType.Int;
            float half = (w - 8f) / 2f;

            EditorGUI.BeginChangeCheck();
            bool newUseMin = EditorGUI.ToggleLeft(new Rect(x, y, half, LineH), "Use Min", f.useMin);
            bool newUseMax = EditorGUI.ToggleLeft(new Rect(x + half + 8, y, half, LineH), "Use Max", f.useMax);
            if (EditorGUI.EndChangeCheck()) { f.useMin = newUseMin; f.useMax = newUseMax; MarkDirty(); }
            y += LineH + LineGap;

            if (isInt)
            {
                if (f.useMin)
                {
                    EditorGUI.LabelField(new Rect(x, y, lw, LineH), "Min");
                    EditorGUI.BeginChangeCheck();
                    int v = EditorGUI.IntField(new Rect(x + lw, y, w - lw, LineH), f.intMin);
                    if (EditorGUI.EndChangeCheck()) { f.intMin = v; MarkDirty(); }
                    y += LineH + LineGap;
                }
                if (f.useMax)
                {
                    EditorGUI.LabelField(new Rect(x, y, lw, LineH), "Max");
                    EditorGUI.BeginChangeCheck();
                    int v = EditorGUI.IntField(new Rect(x + lw, y, w - lw, LineH), f.intMax);
                    if (EditorGUI.EndChangeCheck()) { f.intMax = v; MarkDirty(); }
                    y += LineH + LineGap;
                }
                if (f.useMin && f.useMax && f.intMin >= f.intMax)
                {
                    EditorGUI.HelpBox(new Rect(x, y, w, LineH * 1.5f), "Min must be less than Max", MessageType.Warning);
                    y += LineH * 1.5f + LineGap;
                }
            }
            else
            {
                if (f.useMin)
                {
                    EditorGUI.LabelField(new Rect(x, y, lw, LineH), "Min");
                    EditorGUI.BeginChangeCheck();
                    float v = EditorGUI.FloatField(new Rect(x + lw, y, w - lw, LineH), f.floatMin);
                    if (EditorGUI.EndChangeCheck()) { f.floatMin = v; MarkDirty(); }
                    y += LineH + LineGap;
                }
                if (f.useMax)
                {
                    EditorGUI.LabelField(new Rect(x, y, lw, LineH), "Max");
                    EditorGUI.BeginChangeCheck();
                    float v = EditorGUI.FloatField(new Rect(x + lw, y, w - lw, LineH), f.floatMax);
                    if (EditorGUI.EndChangeCheck()) { f.floatMax = v; MarkDirty(); }
                    y += LineH + LineGap;
                }
                if (f.useMin && f.useMax && f.floatMin >= f.floatMax)
                {
                    EditorGUI.HelpBox(new Rect(x, y, w, LineH * 1.5f), "Min must be less than Max", MessageType.Warning);
                    y += LineH * 1.5f + LineGap;
                }
            }

            return y;
        }

        private float DrawDetailMapDefaults(SettingMap map, float x, float y, float w)
        {
            EditorGUI.LabelField(new Rect(x, y, w, LineH), "Map Defaults", EditorStyles.boldLabel);
            y += LineH + LineGap;

            string lastSet = string.IsNullOrWhiteSpace(map.lastDefaultSetDateTime)
                ? "Never" : map.lastDefaultSetDateTime;
            EditorGUI.LabelField(new Rect(x, y, w, LineH), $"Last saved: {lastSet}", EditorStyles.miniLabel);
            y += LineH + LineGap;

            float btnW = (w - 4f) / 2f;
            if (GUI.Button(new Rect(x, y, btnW, RowH), "Save Defaults"))
            {
                Record(asset,"Save Map Defaults");
                map.SaveAsDefaults();
                MarkDirty();
            }
            bool hasDefaults = map.defaultFields != null && map.defaultFields.hasValue;
            using (new EditorGUI.DisabledScope(!hasDefaults))
            {
                if (GUI.Button(new Rect(x + btnW + 4f, y, btnW, RowH), "Restore Defaults"))
                {
                    Record(asset,"Restore Map Defaults");
                    map.RestoreDefaults();
                    selectedGroupIndex = -1;
                    selectedFieldIndex = -1;
                    MarkDirty();
                }
            }
            return y + RowH + Padding;
        }

        #endregion



        #region Footer

        private void DrawFooter(Rect bar)
        {
            EditorGUI.DrawRect(bar, new Color(0.12f, 0.12f, 0.12f, 1f));

            string msg = isDirty ? "● Unsaved changes" : "✓ Saved";
            Color color = isDirty ? new Color(1f, 0.55f, 0.1f) : new Color(0.35f, 0.85f, 0.35f);

            GUI.Label(new Rect(bar.x + Padding, bar.y + 4f, 220f, 16f), msg,
                new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = color } });

            SettingMap map = SelectedMap();
            SettingField field = SelectedField(map);
            string breadcrumb;
            if (map == null) breadcrumb = "";
            else if (field == null) breadcrumb = map.mapName;
            else if (selectedGroupIndex >= 0 && selectedGroupIndex < map.groups.Count)
                breadcrumb = $"{map.mapName}  /  {map.groups[selectedGroupIndex].groupName}  /  {field.key}";
            else
                breadcrumb = $"{map.mapName}  /  {field.key}";
            GUI.Label(new Rect(bar.x + 230f, bar.y + 4f, bar.width - 240f, 16f), breadcrumb,
                new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = new Color(0.6f, 0.6f, 0.6f) },
                    alignment = TextAnchor.MiddleRight
                });
        }

        #endregion



        #region Inline rename

        private enum RenameTarget { Map, Group, Field, AssetName }

        private void DrawInlineRename(Rect r, int index, RenameTarget target)
        {
            string ctrlName = target switch
            {
                RenameTarget.Map => "MapRename",
                RenameTarget.Group => "GroupRename",
                RenameTarget.Field => "FieldRename",
                _ => "AssetNameRename"
            };

            var result = EditorGUI_Base.DrawInlineRename(r, ref renameBuffer, ctrlName);

            switch (result)
            {
                case EditorGUI_Base.InlineRenameResult.Cancel:
                    switch (target)
                    {
                        case RenameTarget.Map: renamingMapIndex = -1; break;
                        case RenameTarget.Group: renamingGroupIndex = -1; break;
                        case RenameTarget.Field: renamingFieldIndex = -1; renamingFieldGroupIndex = -1; break;
                        case RenameTarget.AssetName: renamingAssetName = false; break;
                    }
                    Repaint();
                    break;

                case EditorGUI_Base.InlineRenameResult.Commit:
                    switch (target)
                    {
                        case RenameTarget.Map: CommitMapRename(index); break;
                        case RenameTarget.Group: CommitGroupRename(index); break;
                        case RenameTarget.Field: CommitFieldRename(SelectedMap(), renamingFieldGroupIndex, index); break;
                        case RenameTarget.AssetName: CommitAssetNameRename(); break;
                    }
                    break;
            }
        }

        #endregion



        #region Drag helpers

        private static void ResolveDropTarget(List<FieldRowInfo> rows, float mouseY, float rowH,
            SettingMap map, out int targetGroupIndex, out int targetIndex)
        {
            if (rows.Count == 0) { targetGroupIndex = -1; targetIndex = 0; return; }

            int rawRow = (int)(mouseY / rowH);
            bool overshoot = rawRow >= rows.Count;
            int clamped = Mathf.Clamp(rawRow, 0, rows.Count - 1);
            var info = rows[clamped];

            if (info.IsHeader)
            {
                float posInRow = mouseY - clamped * rowH;
                if (posInRow < rowH * 0.5f)
                {
                    targetGroupIndex = -1;
                    targetIndex = map.fields.Count;
                }
                else
                {
                    targetGroupIndex = info.GroupIndex;
                    targetIndex = 0;
                }
                return;
            }

            targetGroupIndex = info.GroupIndex;

            int countBefore = 0;
            for (int k = 0; k < clamped; k++)
                if (!rows[k].IsHeader && rows[k].GroupIndex == targetGroupIndex) countBefore++;

            targetIndex = overshoot ? countBefore + 1 : countBefore;
        }

        #endregion



        #region Operations

        private void AddMap()
        {
            Record(asset,"Add Setting Map");
            string name = UniquifyName($"Map {asset.maps.Count + 1}", asset.maps.Select(m => m.mapName));
            asset.maps.Add(new SettingMap { mapName = name });
            selectedMapIndex = asset.maps.Count - 1;
            selectedGroupIndex = -1;
            selectedFieldIndex = -1;
            MarkDirty();
        }

        private void DeleteMap(int i)
        {
            if (!EditorUtility.DisplayDialog("Delete Map",
                    $"Delete map \"{asset.maps[i].mapName}\" and all its fields?", "Delete", "Cancel")) return;
            Record(asset,"Delete Setting Map");
            asset.maps.RemoveAt(i);
            if (i < selectedMapIndex) selectedMapIndex--;
            selectedMapIndex = asset.maps.Count > 0 ? Mathf.Clamp(selectedMapIndex, 0, asset.maps.Count - 1) : -1;
            selectedGroupIndex = -1;
            selectedFieldIndex = -1;
            MarkDirty();
        }

        private void ReorderMap(int from, int to)
        {
            Record(asset,"Reorder Maps");
            var item = asset.maps[from];
            asset.maps.RemoveAt(from);
            asset.maps.Insert(to, item);
            selectedMapIndex = to;
            MarkDirty();
        }

        private void SelectMap(int i)
        {
            selectedMapIndex = i;
            selectedGroupIndex = -1;
            selectedFieldIndex = -1;
            renamingMapIndex = -1;
            renamingGroupIndex = -1;
            renamingFieldGroupIndex = -1;
            renamingFieldIndex = -1;
            GUI.FocusControl(null);
            Repaint();
        }

        private void CommitMapRename(int i)
        {
            string t = renameBuffer.Trim();
            if (!string.IsNullOrWhiteSpace(t) && t != asset.maps[i].mapName)
            {
                string unique = UniquifyName(t, asset.maps.Where((m, idx) => idx != i).Select(m => m.mapName));
                Record(asset,"Rename Map");
                asset.maps[i].mapName = unique;
                MarkDirty();
            }
            renamingMapIndex = -1;
            Repaint();
        }

        private void CommitAssetNameRename()
        {
            string t = renameBuffer.Trim();
            if (!string.IsNullOrWhiteSpace(t) && t != asset.settingsName)
            {
                Record(asset,"Rename Settings Asset");
                asset.settingsName = t;
                titleContent = new GUIContent($"Settings — {asset.settingsName}");
                MarkDirty();
            }
            renamingAssetName = false;
            Repaint();
        }

        private void AddField()
        {
            var map = SelectedMap();
            if (map == null) return;
            AddFieldInternal(map, -1);
        }

        private void AddFieldToGroup(SettingMap map, int groupIndex)
        {
            AddFieldInternal(map, groupIndex);
        }

        private void AddFieldInternal(SettingMap map, int groupIndex)
        {
            var list = ContainerFields(map, groupIndex);
            if (list == null) return;

            Record(asset,"Add Setting Field");
            string name = UniquifyName($"Field{list.Count + 1}", AllFieldKeysInMap(map));
            list.Add(new SettingField { key = name });
            SelectField(groupIndex, list.Count - 1);
            MarkDirty();
        }

        private void DeleteField(SettingMap map, int groupIndex, int i)
        {
            var list = ContainerFields(map, groupIndex);
            if (list == null) return;

            Record(asset,"Delete Setting Field");
            list.RemoveAt(i);

            if (groupIndex == selectedGroupIndex)
            {
                if (i < selectedFieldIndex) selectedFieldIndex--;
                selectedFieldIndex = list.Count > 0 ? Mathf.Clamp(selectedFieldIndex, 0, list.Count - 1) : -1;
            }
            MarkDirty();
        }

        private void MoveOrReorderField(SettingMap map, int fromGroupIndex, int fromIndex, int toGroupIndex, int rawToIndex)
        {
            var fromList = ContainerFields(map, fromGroupIndex);
            if (fromList == null || fromIndex < 0 || fromIndex >= fromList.Count) return;

            SettingField movedRef = fromList[fromIndex];

            if (fromGroupIndex == toGroupIndex)
            {
                int to = Mathf.Clamp(rawToIndex, 0, fromList.Count - 1);
                if (to == fromIndex) return;

                Record(asset,"Reorder Fields");
                fromList.RemoveAt(fromIndex);
                fromList.Insert(to, movedRef);
            }
            else
            {
                var toList = ContainerFields(map, toGroupIndex);
                if (toList == null) return;

                Record(asset,"Move Field");
                fromList.RemoveAt(fromIndex);
                toList.Insert(Mathf.Clamp(rawToIndex, 0, toList.Count), movedRef);
            }

            ReselectField(map, movedRef);
            MarkDirty();
        }

        private void ReselectField(SettingMap map, SettingField target)
        {
            int idx = map.fields.IndexOf(target);
            if (idx >= 0) { selectedGroupIndex = -1; selectedFieldIndex = idx; return; }

            for (int g = 0; g < map.groups.Count; g++)
            {
                idx = map.groups[g].fields.IndexOf(target);
                if (idx >= 0) { selectedGroupIndex = g; selectedFieldIndex = idx; return; }
            }
        }

        private void SelectField(int groupIndex, int i)
        {
            selectedGroupIndex = groupIndex;
            selectedFieldIndex = i;
            renamingFieldIndex = -1;
            renamingFieldGroupIndex = -1;
            renamingMapIndex = -1;
            renamingGroupIndex = -1;
            GUI.FocusControl(null);
            Repaint();
        }

        private void CommitFieldRename(SettingMap map, int groupIndex, int i)
        {
            if (map == null) return;
            var list = ContainerFields(map, groupIndex);
            if (list == null) return;

            string current = list[i].key;
            string t = renameBuffer.Trim();
            if (!string.IsNullOrWhiteSpace(t) && t != current)
            {
                string unique = UniquifyName(t, AllFieldKeysInMap(map).Where(k => k != current));
                Record(asset,"Rename Field");
                list[i].key = unique;
                MarkDirty();
            }
            renamingFieldIndex = -1;
            renamingFieldGroupIndex = -1;
            Repaint();
        }

        private void MoveFieldToGroup(SettingMap map, int fromGroupIndex, int fieldIndex, int toGroupIndex)
        {
            if (fromGroupIndex == toGroupIndex) return;
            MoveOrReorderField(map, fromGroupIndex, fieldIndex, toGroupIndex, int.MaxValue);
        }

        private void AddGroup()
        {
            var map = SelectedMap();
            if (map == null) return;
            Record(asset,"Add Setting Group");
            string name = UniquifyName($"Group {map.groups.Count + 1}", map.groups.Select(g => g.groupName));
            map.groups.Add(new SettingGroup { groupName = name });
            MarkDirty();
        }

        private void DeleteGroup(SettingMap map, int g)
        {
            if (!EditorUtility.DisplayDialog("Delete Group",
                    $"Delete group \"{map.groups[g].groupName}\" and all its fields?", "Delete", "Cancel")) return;

            Record(asset,"Delete Setting Group");
            map.groups.RemoveAt(g);

            if (selectedGroupIndex == g) { selectedGroupIndex = -1; selectedFieldIndex = -1; }
            else if (selectedGroupIndex > g) selectedGroupIndex--;

            MarkDirty();
        }

        private void CommitGroupRename(int g)
        {
            var map = SelectedMap();
            if (map == null) return;
            string t = renameBuffer.Trim();
            if (!string.IsNullOrWhiteSpace(t) && t != map.groups[g].groupName)
            {
                string unique = UniquifyName(t, map.groups.Where((grp, idx) => idx != g).Select(grp => grp.groupName));
                Record(asset,"Rename Group");
                map.groups[g].groupName = unique;
                MarkDirty();
            }
            renamingGroupIndex = -1;
            Repaint();
        }

        private string GroupKey(SettingMap map, SettingGroup group) => $"{map.mapName}/{group.groupName}";

        private bool IsGroupCollapsed(SettingMap map, SettingGroup group) =>
            collapsedGroupKeys.Contains(GroupKey(map, group));

        private void ToggleGroupCollapsed(SettingMap map, SettingGroup group)
        {
            string key = GroupKey(map, group);
            if (!collapsedGroupKeys.Remove(key))
                collapsedGroupKeys.Add(key);
            Repaint();
        }

        private void SaveWholeAssetDefaults()
        {
            Record(asset,"Save Settings Defaults");
            asset.SaveAsDefaults();
            MarkDirty();
        }

        private void RestoreWholeAssetDefaults()
        {
            Record(asset,"Restore Settings Defaults");
            asset.RestoreDefaults();
            selectedMapIndex = asset.maps.Count > 0 ? Mathf.Clamp(selectedMapIndex, 0, asset.maps.Count - 1) : -1;
            selectedGroupIndex = -1;
            selectedFieldIndex = -1;
            MarkDirty();
        }

        private static string UniquifyName(string desired, IEnumerable<string> existingNames)
        {
            var taken = new HashSet<string>(existingNames, StringComparer.Ordinal);
            if (!taken.Contains(desired)) return desired;

            int n = 1;
            string candidate;
            do { candidate = $"{desired} ({n})"; n++; }
            while (taken.Contains(candidate));

            return candidate;
        }

        private static IEnumerable<string> AllFieldKeysInMap(SettingMap map)
        {
            foreach (var f in map.fields) yield return f.key;
            foreach (var g in map.groups)
                foreach (var f in g.fields) yield return f.key;
        }

        private static List<SettingField> ContainerFields(SettingMap map, int groupIndex)
        {
            if (groupIndex < 0) return map.fields;
            return groupIndex < map.groups.Count ? map.groups[groupIndex].fields : null;
        }

        #endregion



        #region Helpers

        private SettingMap SelectedMap() =>
            selectedMapIndex >= 0 && selectedMapIndex < asset?.maps.Count
                ? asset.maps[selectedMapIndex] : null;

        private SettingField SelectedField(SettingMap map)
        {
            if (map == null) return null;
            var list = ContainerFields(map, selectedGroupIndex);
            return list != null && selectedFieldIndex >= 0 && selectedFieldIndex < list.Count
                ? list[selectedFieldIndex] : null;
        }

        private static Type ResolveEnumType(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var t = Type.GetType(name);
            if (t != null && t.IsEnum) return t;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType(name);
                if (t != null && t.IsEnum) return t;
            }
            return null;
        }

        private static Color TypeColor(SettingType type) => type switch
        {
            SettingType.Bool => new Color(0.45f, 0.85f, 0.45f),
            SettingType.Int => new Color(0.45f, 0.68f, 1.00f),
            SettingType.Float => new Color(1.00f, 0.78f, 0.35f),
            SettingType.String => new Color(0.88f, 0.55f, 0.88f),
            SettingType.Enum => new Color(1.00f, 0.58f, 0.35f),
            _ => Color.gray
        };

        #endregion

        #endregion
    }




    public class EnumTypePickerWindow : EditorWindow
    {
        private static SerializedProperty s_enumTypeName;
        private static SerializedProperty s_enumValue;
        private static Action<string> s_callback;
        private static string s_currentValue = "";

        private string search = "";
        private Vector2 scroll;
        private List<Type> allTypes;
        private List<Type> filtered;
        private int hoveredIndex = -1;

        private const float WindowW = 420f;
        private const float WindowH = 340f;


        public static void Show(SerializedProperty enumTypeName, SerializedProperty enumValue)
        {
            s_enumTypeName = enumTypeName;
            s_enumValue = enumValue;
            s_callback = null;
            s_currentValue = enumTypeName.stringValue;
            Open();
        }

        public static void ShowWithCallback(Action<string> onSelect, string currentValue = "")
        {
            s_enumTypeName = null;
            s_enumValue = null;
            s_callback = onSelect;
            s_currentValue = currentValue ?? "";
            Open();
        }

        private static void Open()
        {
            var win = GetWindow<EnumTypePickerWindow>(true, "Select Enum Type", true);
            win.minSize = new Vector2(WindowW, WindowH);
            win.maxSize = new Vector2(WindowW * 2f, WindowH * 2f);
            win.Initialize();
            win.ShowUtility();
        }

        private void Initialize()
        {
            allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .Where(t => t.IsEnum && !t.IsSpecialName)
                .OrderBy(t => t.FullName)
                .ToList();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string lo = search.ToLowerInvariant();
            filtered = string.IsNullOrWhiteSpace(search)
                ? new List<Type>(allTypes)
                : allTypes.Where(t => t.FullName.ToLowerInvariant().Contains(lo)).ToList();
            hoveredIndex = -1;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            GUI.SetNextControlName("SearchField");
            EditorGUI.BeginChangeCheck();
            search = EditorGUILayout.TextField("Search", search);
            if (EditorGUI.EndChangeCheck()) ApplyFilter();
            EditorGUI.FocusTextInControl("SearchField");

            EditorGUILayout.LabelField($"{filtered.Count} result{(filtered.Count == 1 ? "" : "s")}", EditorStyles.miniLabel);
            EditorGUILayout.Space(2);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (int i = 0; i < filtered.Count; i++)
            {
                Type t = filtered[i];
                bool selected = t.FullName == s_currentValue;

                Rect row = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight + 2f);
                bool hovered = row.Contains(Event.current.mousePosition);

                if (hovered) { hoveredIndex = i; EditorGUI.DrawRect(row, new Color(0.2f, 0.5f, 0.9f, 0.25f)); Repaint(); }
                else if (selected) { EditorGUI.DrawRect(row, new Color(0.2f, 0.5f, 0.9f, 0.15f)); }

                string ns = t.Namespace ?? "";
                string label = string.IsNullOrEmpty(ns) ? t.Name
                    : $"{t.Name}  <color=#888888><size=10>{ns}</size></color>";

                GUIStyle st = new GUIStyle(selected
                    ? new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold }
                    : EditorStyles.label) { richText = true };
                EditorGUI.LabelField(row, label, st);

                if (Event.current.type == EventType.MouseDown && hovered)
                { Commit(t.FullName); Event.current.Use(); }
            }
            EditorGUILayout.EndScrollView();
            HandleKeyboard();
        }

        private void HandleKeyboard()
        {
            if (Event.current.type != EventType.KeyDown) return;
            switch (Event.current.keyCode)
            {
                case KeyCode.DownArrow: hoveredIndex = Mathf.Min(hoveredIndex + 1, filtered.Count - 1); ScrollTo(hoveredIndex); Event.current.Use(); Repaint(); break;
                case KeyCode.UpArrow: hoveredIndex = Mathf.Max(hoveredIndex - 1, 0); ScrollTo(hoveredIndex); Event.current.Use(); Repaint(); break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter: if (hoveredIndex >= 0 && hoveredIndex < filtered.Count) Commit(filtered[hoveredIndex].FullName); Event.current.Use(); break;
                case KeyCode.Escape: Close(); Event.current.Use(); break;
            }
        }

        private void ScrollTo(int i)
        {
            float rh = EditorGUIUtility.singleLineHeight + 2f;
            scroll.y = Mathf.Clamp(scroll.y, i * rh - WindowH * 0.6f, i * rh);
        }

        private void Commit(string fullName)
        {
            s_currentValue = fullName;
            if (s_callback != null)
                s_callback.Invoke(fullName);
            else if (s_enumTypeName != null)
            {
                s_enumTypeName.stringValue = fullName;
                s_enumValue.intValue = 0;
                s_enumTypeName.serializedObject.ApplyModifiedProperties();
            }
            Close();
        }
    }
}
#endif
