/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Audio;

using SproutPackage._Editor;

using SHUU.UserSide.Addons.AudioSystem.ScriptableObjects;

using SETB;
using SETB.SuperClasses;

using static SETB.EditorGUI_Base;
using static SETB.HandyEditorFunctions;



#if UNITY_6000_0_OR_NEWER
using TreeViewItemInt = UnityEditor.IMGUI.Controls.TreeViewItem<int>;
using TreeViewStateInt = UnityEditor.IMGUI.Controls.TreeViewState<int>;
using TreeViewInt = UnityEditor.IMGUI.Controls.TreeView<int>;
#else
using TreeViewItemInt = UnityEditor.IMGUI.Controls.TreeViewItem;
using TreeViewStateInt = UnityEditor.IMGUI.Controls.TreeViewState;
using TreeViewInt = UnityEditor.IMGUI.Controls.TreeView;
#endif

namespace SHUU.UserSide.Addons.AudioSystem._Editor
{
    #region Tree

    #region Tree Item
    internal class AudioChannelTreeViewItem : TreeViewItemInt
    {
        public const string RootChannelKey = "ROOT";

        public bool isGroupRoot;


        public string propertyPath;

        public string parentArrayPath;

        
        public int arrayIndex;

        
        public string PersistenceKey => isGroupRoot ? RootChannelKey : propertyPath;
    }
    #endregion




    #region Tree View
    internal class AudioChannelTreeView : TreeViewInt
    {
        #region Variables
        private readonly AudioChannelEditorWindow owner;
        private readonly SerializedObject serializedGroup;
        private readonly string assetGuid;
        private int idCounter;
        #endregion




        #region Main
        public AudioChannelTreeView(TreeViewStateInt state, SerializedObject serializedGroup, AudioChannelEditorWindow owner) : base(state)
        {
            this.owner = owner;
            this.serializedGroup = serializedGroup;
            assetGuid = serializedGroup != null && serializedGroup.targetObject != null
                ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(serializedGroup.targetObject))
                : null;

            showAlternatingRowBackgrounds = false;
            Reload();

            if (rootItem != null && rootItem.hasChildren && GetSelection().Count == 0)
            {
                SetSelection(new List<int> { 1 });
                SetExpanded(1, true);
            }
        }
        #endregion



        #region Logic
        public AudioChannelTreeViewItem GetSelectedChannelItem()
        {
            var selection = GetSelection();
            if (selection.Count == 0) return null;

            return FindItem(selection[0], rootItem) as AudioChannelTreeViewItem;
        }

        protected override bool CanRename(TreeViewItemInt item) => item is AudioChannelTreeViewItem;

        //  Right-clicking a channel does the same thing as its "Icon" button in the inspector.
        protected override void ContextClickedItem(int id)
        {
            var item = FindItem(id, rootItem) as AudioChannelTreeViewItem;
            if (item == null) return;

            SetSelection(new List<int> { id });
            owner.ShowIconPickerPopup(item.PersistenceKey, new Rect(Event.current.mousePosition, Vector2.zero));
        }


        protected override void DoubleClickedItem(int id)
        {
            var item = FindItem(id, rootItem);
            if (item is AudioChannelTreeViewItem) BeginRename(item);
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (!args.acceptedRename || string.IsNullOrEmpty(args.newName)) return;

            var item = FindItem(args.itemID, rootItem) as AudioChannelTreeViewItem;
            if (item == null) return;

            var idProp = item.isGroupRoot
                ? serializedGroup.FindProperty("ID")
                : serializedGroup.FindProperty(item.propertyPath)?.FindPropertyRelative("ID");

            if (idProp == null) return;

            idProp.stringValue = args.newName;
            serializedGroup.ApplyModifiedProperties();
            Reload();
        }

        protected override TreeViewItemInt BuildRoot()
        {
            idCounter = 0;

            var root = new TreeViewItemInt { id = 0, depth = -1, displayName = "Root" };
            var allItems = new List<TreeViewItemInt>();

            if (serializedGroup != null && serializedGroup.targetObject != null)
            {
                serializedGroup.Update();

                // Synthetic node for the AudioChannelGroup asset itself.
                var groupIdProp = serializedGroup.FindProperty("ID");
                var groupItem = new AudioChannelTreeViewItem
                {
                    id = ++idCounter, // always 1
                    depth = 0,
                    displayName = groupIdProp != null ? groupIdProp.stringValue : serializedGroup.targetObject.name,
                    isGroupRoot = true
                };
                allItems.Add(groupItem);

                var rootChildrenProp = serializedGroup.FindProperty("children");
                BuildChannelItems(groupItem, rootChildrenProp, "children", allItems);
            }

            SetupParentsAndChildrenFromDepths(root, allItems);

            if (allItems.Count == 0)
                root.AddChild(new TreeViewItemInt(int.MaxValue, -1, "(no target assigned)"));

            return root;
        }

        // Recursively mirrors the "children" SerializeReference lists on both
        // AudioChannelGroup (root) and AudioChannel (nested) — same field name at every level.
        private void BuildChannelItems(TreeViewItemInt parent, SerializedProperty arrayProp, string arrayPath, List<TreeViewItemInt> allItems)
        {
            if (arrayProp == null || !arrayProp.isArray) return;

            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                var elementProp = arrayProp.GetArrayElementAtIndex(i);
                var idProp = elementProp.FindPropertyRelative("ID");

                var item = new AudioChannelTreeViewItem
                {
                    id = ++idCounter,
                    depth = parent.depth + 1,
                    displayName = idProp != null ? idProp.stringValue : $"Channel {i}",
                    propertyPath = elementProp.propertyPath,
                    parentArrayPath = arrayPath,
                    arrayIndex = i
                };

                allItems.Add(item);

                var childArrayProp = elementProp.FindPropertyRelative("children");
                BuildChannelItems(item, childArrayProp, elementProp.propertyPath + ".children", allItems);
            }
        }

        // A soft, low-contrast stripe — Unity's default alternating background reads as
        // quite strong in both editor skins, so this uses a much lower alpha instead.
        private static readonly Color StripeColorDark = new Color(1f, 1f, 1f, 0.035f);
        private static readonly Color StripeColorLight = new Color(0f, 0f, 0f, 0.035f);

        // Unity doesn't expose the default editor window background color via public API —
        // these are the standard values for the dark/light skins.
        private static Color WindowBackgroundColor => EditorGUIUtility.isProSkin
            ? new Color(56f / 255f, 56f / 255f, 56f / 255f)
            : new Color(194f / 255f, 194f / 255f, 194f / 255f);

        // No background/border and no fixed height (miniButton's built-in fixed height is
        // taller than our small row button, which made it overflow into the row below and
        // squeeze the "+" glyph out entirely) — just the plain character, sized to the rect.
        private static GUIStyle plusButtonStyle;
        private static GUIStyle PlusButtonStyle => plusButtonStyle ??= new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 10,
            fixedHeight = 0,
            fixedWidth = 0,
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0)
        };

        protected override void RowGUI(RowGUIArgs args)
        {
            if (!IsSelected(args.item.id))
            {
                EditorGUI.DrawRect(args.rowRect, WindowBackgroundColor);
                if (args.row % 2 == 1)
                    EditorGUI.DrawRect(args.rowRect, EditorGUIUtility.isProSkin ? StripeColorDark : StripeColorLight);
            }

            var channelItem = args.item as AudioChannelTreeViewItem;

            var defaultIcon = EditorGUIUtility.IconContent("AudioSource Icon").image;
            var icon = (channelItem != null && assetGuid != null)
                ? ChannelIconPrefs.GetIcon(assetGuid, channelItem.PersistenceKey, defaultIcon)
                : defaultIcon;

            var rect = args.rowRect;
            rect.x += GetContentIndent(args.item);

            GUI.DrawTexture(new Rect(rect.x, rect.y + 1, 16, 16), icon, ScaleMode.ScaleToFit);

            const float plusButtonSize = 12f;
            var plusRect = new Rect(args.rowRect.xMax - plusButtonSize - 2f, args.rowRect.y + (args.rowRect.height - plusButtonSize) / 2f, plusButtonSize, plusButtonSize);

            var labelStyle = (channelItem != null && channelItem.isGroupRoot) ? EditorStyles.boldLabel : EditorStyles.label;
            float labelWidth = Mathf.Max(0f, plusRect.x - 4f - (rect.x + 18f));
            GUI.Label(new Rect(rect.x + 18, rect.y, labelWidth, rect.height), args.label, labelStyle);

            // Does the same thing as the toolbar's "Add Channel" button, but for this row specifically.
            if (channelItem != null && GUI.Button(plusRect, new GUIContent("+", "Add a child channel here"), PlusButtonStyle))
                owner.AddChannelUnder(channelItem);
        }
        #endregion
    }
    #endregion

    #endregion





    #region Channel Icon Prefs
    internal static class ChannelIconPrefs
    {
        #region Variables
        private const string KeyPrefix = "SHUU.AudioChannelEditor.Icon::";

        private const string BuiltinPrefix = "builtin:";


        private static EditorPrefID PrefID = new("AudioChannelGroup", "ChannelIcon");
        #endregion




        #region Logic
        private static string BuildKey(string assetGuid, string channelKey) => $"{KeyPrefix}{assetGuid}::{channelKey}";



        public static Texture GetIcon(string assetGuid, string channelKey, Texture fallback)
        {
            if (string.IsNullOrEmpty(assetGuid) || string.IsNullOrEmpty(channelKey)) return fallback;

            string key = BuildKey(assetGuid, channelKey);
            if (!HasEditorPref(key, PrefID)) return fallback;

            string stored = GetEditorPref<string>(key, id: PrefID);
            if (string.IsNullOrEmpty(stored)) return fallback;

            if (stored.StartsWith(BuiltinPrefix))
            {
                var content = EditorGUIUtility.IconContent(stored.Substring(BuiltinPrefix.Length));
                return content != null && content.image != null ? content.image : fallback;
            }

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(stored);
            return texture != null ? texture : fallback;
        }


        public static void SetBuiltinIcon(string assetGuid, string channelKey, string iconName)
            => SetEditorPref(BuildKey(assetGuid, channelKey), BuiltinPrefix + iconName, PrefID);

        public static void SetCustomIcon(string assetGuid, string channelKey, Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) return;

            SetEditorPref(BuildKey(assetGuid, channelKey), path, PrefID);
        }


        public static void ClearIcon(string assetGuid, string channelKey) => DeleteEditorPref(BuildKey(assetGuid, channelKey), PrefID);
        #endregion
    }
    #endregion





    #region Window
    public class AudioChannelEditorWindow : EditorWindow
    {
        #region Variables
        [SerializeField] private AudioChannelGroup targetGroup;
        [SerializeField] private TreeViewStateInt treeViewState;
        [SerializeField] private float treeWidth = 260f;

        private const float SplitterThickness = 4f;

        private SerializedObject serializedGroup;
        private AudioChannelTreeView treeView;
        private Vector2 inspectorScroll;

        private static GUIStyle headerNameFieldStyle;
        private static GUIStyle HeaderNameFieldStyle => headerNameFieldStyle ??= new GUIStyle(EditorStyles.textField)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 13
        };

        // A slight darkening so the channel list reads as its own panel next to the inspector.
        private static Color TreePaneBackgroundColor => EditorGUIUtility.isProSkin
            ? new Color(0f, 0f, 0f, 0.22f)
            : new Color(0f, 0f, 0f, 0.12f);

        // Category labels mirror the [Title(...)] groupings in SHUU_AudioInstance.Options,
        // used only to organize the "Add Option" menu and the already-added list. Any option
        // field not listed here still works — it just falls under "Other".
        private static readonly Dictionary<string, string> OptionCategories = new()
        {
            { "playOnAwake", "Playback" }, { "loop", "Playback" }, { "priority", "Playback" }, { "volume", "Playback" },
            { "pitch", "Playback" }, { "panStereo", "Playback" }, { "spatialBlend", "Playback" },

            { "dopplerLevel", "3D Sound" }, { "spread", "3D Sound" }, { "rolloffMode", "3D Sound" },
            { "minDistance", "3D Sound" }, { "maxDistance", "3D Sound" },

            { "mute", "Effects" }, { "bypassEffects", "Effects" }, { "bypassListenerEffects", "Effects" },
            { "bypassReverbZones", "Effects" }, { "reverbZoneMix", "Effects" },

            { "outputAudioMixerGroup", "Output" },

            { "onAudioEnd", "Custom" }, { "returnToPool_clipEnd", "Custom" }, { "returnToPool_sceneChange", "Custom" },
        };

        private static string GetCategory(string fieldName) => OptionCategories.TryGetValue(fieldName, out var cat) ? cat : "Other";
        #endregion




        #region Main
        public static void Open() => OpenFor(Selection.activeObject as AudioChannelGroup);

        [OnOpenAsset]
        public static bool OnOpenAsset(EntityId entityId, int line)
        {
            var asset = IdToObject(entityId) as AudioChannelGroup;
            if (asset == null) return false;

            OpenFor(asset);
            return true;
        }

        public static void OpenFor(AudioChannelGroup group)
        {
            var window = GetWindow<AudioChannelEditorWindow>("Audio Channel Editor");
            window.minSize = new Vector2(600, 380);
            window.SetTarget(group);
        }

        private void SetTarget(AudioChannelGroup group)
        {
            targetGroup = group;
            serializedGroup = targetGroup != null ? new SerializedObject(targetGroup) : null;

            treeViewState ??= new TreeViewStateInt();
            treeView = new AudioChannelTreeView(treeViewState, serializedGroup, this);
            treeView.Reload();

            Repaint();
        }


        #region Unity Events
        private void OnEnable()
        {
            if (targetGroup != null) SetTarget(targetGroup);
        }
        private void OnDisable()
        {
            if (targetGroup != null && UtilityIsDirty(targetGroup))
                ConfirmCancel_Changes("Emergency Save", $"Audio Channel Group has unsaved changes.\nSave before closing?", SaveTarget);
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (targetGroup == null)
            {
                EditorGUILayout.HelpBox("Select or assign an AudioChannelGroup asset to edit.", MessageType.Info);
                return;
            }

            serializedGroup.Update();

            bool hasSelection = treeView.GetSelectedChannelItem() != null;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (hasSelection)
                {
                    var treeRect = GUILayoutUtility.GetRect(treeWidth, treeWidth, 0, 100000, GUILayout.ExpandHeight(true));
                    treeView.OnGUI(treeRect);
                    EditorGUI.DrawRect(treeRect, TreePaneBackgroundColor);

                    var splitterRect = GUILayoutUtility.GetRect(SplitterThickness, SplitterThickness, 0, 100000, GUILayout.ExpandHeight(true));
                    EditorGUI.DrawRect(splitterRect, TreePaneBackgroundColor);
                    HandleSplitterDrag(splitterRect);

                    DrawInspector();
                }
                else
                {
                    // No channel selected — the tree takes the full window, no splitter or
                    // inspector pane at all, rather than showing an empty panel next to it.
                    var treeRect = GUILayoutUtility.GetRect(0, 100000, 0, 100000, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    treeView.OnGUI(treeRect);
                    EditorGUI.DrawRect(treeRect, TreePaneBackgroundColor);
                }
            }

            if (serializedGroup.ApplyModifiedProperties())
                treeView.Reload();
        }
        #endregion

        #endregion



        #region Logic

        #region Splitter
        // Draggable divider between the tree and inspector panes. Without this, giving the
        // tree an unbounded max width (as a naive GetRect call would) starves the inspector
        // pane down to almost nothing regardless of how big the window is.
        private void HandleSplitterDrag(Rect splitterRect)
        {
            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);
            int controlId = GUIUtility.GetControlID(FocusType.Passive, splitterRect);

            switch (Event.current.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    if (splitterRect.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId)
                    {
                        treeWidth = Mathf.Clamp(treeWidth + Event.current.delta.x, 150f, position.width - 250f);
                        Event.current.Use();
                        Repaint();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId)
                    {
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                    }
                    break;
            }
        }
        #endregion



        #region Toolbar
        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var newTarget = DrawInputObject(null, targetGroup, options: GUILayout.Width(220));
                if (newTarget != targetGroup) SetTarget(newTarget);

                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(targetGroup == null))
                {
                    bool isDirty = targetGroup != null && UtilityIsDirty(targetGroup);


                    DrawButton("Add Channel", AddChannel, EditorStyles.toolbarButton);

                    var selected = treeView?.GetSelectedChannelItem();
                    using (new EditorGUI.DisabledScope(selected == null || selected.isGroupRoot))
                        DrawButton("Delete", DeleteSelection, EditorStyles.toolbarButton);

                    var generateContent = new GUIContent(
                        "Generate Mixer",
                        AudioMixerReflection.IsAvailable
                            ? "Creates a new AudioMixer asset with one AudioMixerGroup per channel, mirroring this tree. Exposes each group's Volume, and assigns each channel to its group."
                            : "Unavailable: the internal Unity API this depends on wasn't found in this Editor version.");
                    using (new EditorGUI.DisabledScope(!AudioMixerReflection.IsAvailable))
                        DrawButton(generateContent, GenerateMixerFromChannelTree, EditorStyles.toolbarButton);

                    DrawButton("Generate Static References", () => AudioSystem_StaticReferencesGenerator.GenerateForGroup(targetGroup), EditorStyles.toolbarButton);

                    using (new EditorGUI.DisabledScope(!isDirty))
                        DrawButton(isDirty ? "Save*" : "Save", SaveTarget, EditorStyles.toolbarButton);
                }
            }
        }

        private void SaveTarget()
        {
            if (targetGroup == null) return;

            UtilitySetDirty(targetGroup);
            AssetDatabase.SaveAssetIfDirty(targetGroup);
        }
        #endregion



        #region Tree / Add / Delete
        private SerializedProperty GetChildrenProperty(AudioChannelTreeViewItem item)
        {
            if (item.isGroupRoot) return serializedGroup.FindProperty("children");
            return serializedGroup.FindProperty(item.propertyPath).FindPropertyRelative("children");
        }

        private void AddChannel()
        {
            var selected = treeView.GetSelectedChannelItem();
            AddChannelUnder(selected);
        }

        // Adds a new channel under the given item (or under the root if item is null),
        // matching what the toolbar's "Add Channel" button does for the current selection.
        // Shared with the per-row "+" button in the tree.
        internal void AddChannelUnder(AudioChannelTreeViewItem item)
        {
            var childrenProp = item != null ? GetChildrenProperty(item) : serializedGroup.FindProperty("children");

            int index = childrenProp.arraySize;
            childrenProp.InsertArrayElementAtIndex(index);

            var newElement = childrenProp.GetArrayElementAtIndex(index);

            // Only [SerializeReference] lists (e.g. AudioChannel.children) support assigning a
            // fresh managed reference here. AudioChannelGroup.children is currently a plain
            // [SerializeField] list, so Insert duplicates the previous element's data instead of
            // creating a blank one — add [SerializeReference] to that field to match AudioChannel
            // and get clean inserts there too. Either way we reset ID below so it's never ambiguous.
            if (newElement.propertyType == SerializedPropertyType.ManagedReference)
                newElement.managedReferenceValue = new AudioChannel();

            var idProp = newElement.FindPropertyRelative("ID");
            if (idProp != null) idProp.stringValue = "New Channel";

            serializedGroup.ApplyModifiedProperties();
            treeView.Reload();

            // Ids are reassigned in the same DFS order on Reload, and appending only adds a
            // node at the end of this parent's own children — everything before it keeps the
            // same id, so it's safe to reuse item.id (or 1, the root's fixed id) here.
            int parentId = item != null ? item.id : 1;
            treeView.SetExpanded(parentId, true);
        }

        private void DeleteSelection()
        {
            var selected = treeView.GetSelectedChannelItem();
            if (selected == null || selected.isGroupRoot) return; // root channel can't be deleted

            var arrayProp = serializedGroup.FindProperty(selected.parentArrayPath);
            if (selected.arrayIndex < arrayProp.arraySize)
                arrayProp.DeleteArrayElementAtIndex(selected.arrayIndex);

            serializedGroup.ApplyModifiedProperties();
            treeView.Reload();
        }

        // Creates a brand-new AudioMixer asset with one AudioMixerGroup per channel, mirroring
        // this tree (the root channel maps to the mixer's own Master group), exposes each
        // group's Volume parameter as "{GroupName}_Volume", and assigns each channel's
        // Options.outputAudioMixerGroup to its corresponding group. Always creates a new asset
        // rather than updating an existing one — running this again on the same
        // AudioChannelGroup produces a second, separate mixer.
        private void GenerateMixerFromChannelTree()
        {
            if (!AudioMixerReflection.IsAvailable)
            {
                SproutPackage_Editor.SproutPopup(
                    "The internal Unity API this depends on wasn't found in this Editor version. No changes were made.",
                    SproutPackage_Editor.PopupType.Error,
                    options : new() { Title = "Generate Mixer" });
                    
                return;
            }

            string defaultName = targetGroup.name + "_Mixer";
            string path = EditorUtility.SaveFilePanelInProject("Generate Audio Mixer", defaultName, "mixer",
                "Choose where to save the generated AudioMixer asset.");
            if (string.IsNullOrEmpty(path)) return; // cancelled

            var mixer = AudioMixerReflection.CreateMixerAsset(path);
            if (mixer == null)
            {
                SproutPackage_Editor.SproutPopup(
                    "Failed to create the AudioMixer asset. No changes were made.",
                    SproutPackage_Editor.PopupType.Error,
                    options : new() { Title = "Generate Mixer" });
                    
                return;
            }

            var masterGroup = AudioMixerReflection.GetMasterGroup(mixer);
            int groupCount = 1; // Master, for the root channel

            var rootIdProp = serializedGroup.FindProperty("ID");
            string rootName = rootIdProp != null && !string.IsNullOrEmpty(rootIdProp.stringValue) ? rootIdProp.stringValue : targetGroup.name;

            AssignMixerGroupOption(serializedGroup.FindProperty("options"), masterGroup);
            bool allExposed = AudioMixerReflection.ExposeVolumeParameter(mixer, masterGroup, $"{rootName}_Volume");

            GenerateGroupsRecursive(mixer, masterGroup, serializedGroup.FindProperty("children"), ref groupCount, ref allExposed);

            serializedGroup.ApplyModifiedProperties();
            treeView.Reload();

            UtilitySetDirty(mixer);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(mixer);

            string message = $"Created \"{path}\" with {groupCount} group(s) and assigned it to every channel.";
            if (!allExposed)
                message += "\n\nNote: some volume parameters could not be exposed automatically (internal API unavailable or shaped differently in this Unity version) — expose them manually via each group's context menu in the Audio Mixer window.";

            EditorUtility.DisplayDialog("Generate Mixer", message, "OK");
        }

        private void GenerateGroupsRecursive(AudioMixer mixer, AudioMixerGroup parentMixerGroup, SerializedProperty childrenArrayProp, ref int groupCount, ref bool allExposed)
        {
            if (childrenArrayProp == null || !childrenArrayProp.isArray) return;

            for (int i = 0; i < childrenArrayProp.arraySize; i++)
            {
                var channelProp = childrenArrayProp.GetArrayElementAtIndex(i);
                var idProp = channelProp.FindPropertyRelative("ID");
                string groupName = idProp != null && !string.IsNullOrEmpty(idProp.stringValue) ? idProp.stringValue : $"Channel {i}";

                var mixerGroup = AudioMixerReflection.CreateGroup(mixer, groupName);
                AudioMixerReflection.AddChildToParent(mixer, mixerGroup, parentMixerGroup);
                groupCount++;

                if (!AudioMixerReflection.ExposeVolumeParameter(mixer, mixerGroup, $"{groupName}_Volume"))
                    allExposed = false;

                AssignMixerGroupOption(channelProp.FindPropertyRelative("options"), mixerGroup);

                GenerateGroupsRecursive(mixer, mixerGroup, channelProp.FindPropertyRelative("children"), ref groupCount, ref allExposed);
            }
        }

        // Sets has_outputAudioMixerGroup = true and outputAudioMixerGroup = group on an Options
        // property, so the channel explicitly targets its new group rather than relying on the
        // usual has_/inherit fallback (which would otherwise walk up to the parent's group anyway,
        // but explicit is clearer immediately after generation).
        private void AssignMixerGroupOption(SerializedProperty optionsProp, AudioMixerGroup mixerGroup)
        {
            if (optionsProp == null) return;

            var hasProp = optionsProp.FindPropertyRelative("has_outputAudioMixerGroup");
            var valueProp = optionsProp.FindPropertyRelative("outputAudioMixerGroup");
            if (hasProp == null || valueProp == null) return;

            hasProp.boolValue = true;
            valueProp.objectReferenceValue = mixerGroup;
        }
        #endregion



        #region Inspector
        // Only ever called when a channel is selected (see OnGUI) — no nothing-selected
        // branch here, since in that case the whole pane isn't drawn at all.
        private void DrawInspector()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                var selected = treeView.GetSelectedChannelItem();
                if (selected == null) return;

                inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll);

                if (selected.isGroupRoot)
                {
                    DrawEditableName(serializedGroup.FindProperty("ID"), selected.PersistenceKey);
                    EditorGUILayout.LabelField("Root channel — lives on the AudioChannelGroup asset itself.", EditorStyles.miniLabel);
                    EditorGUILayout.Space();

                    DrawGroupRootFields();

                    var optionsProp = serializedGroup.FindProperty("options");
                    if (optionsProp != null) DrawOptionsSection(optionsProp, null);
                }
                else
                {
                    var property = serializedGroup.FindProperty(selected.propertyPath);
                    if (property != null)
                    {
                        DrawEditableName(property.FindPropertyRelative("ID"), selected.PersistenceKey);
                        EditorGUILayout.Space();

                        DrawChannelFields(property);

                        var optionsProp = property.FindPropertyRelative("options");
                        if (optionsProp != null) DrawOptionsSection(optionsProp, property.FindPropertyRelative("inheritNullOptions"));
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawEditableName(SerializedProperty idProp, string channelKey)
        {
            if (idProp == null) return;

            using (new EditorGUILayout.HorizontalScope())
            {
                idProp.stringValue = EditorGUILayout.TextField(idProp.stringValue, HeaderNameFieldStyle);

                var defaultIcon = EditorGUIUtility.IconContent("AudioSource Icon").image;
                var currentIcon = ChannelIconPrefs.GetIcon(GetAssetGuid(), channelKey, defaultIcon);

                var buttonContent = new GUIContent(" Icon", currentIcon, "Change this channel's icon");
                if (GUILayout.Button(buttonContent, GUILayout.Width(64), GUILayout.Height(20)))
                    ShowIconPickerPopup(channelKey, new Rect(Event.current.mousePosition, Vector2.zero));
            }
        }

        // Shared by the "Icon" button next to the name field and right-clicking a channel in
        // the tree — both open the same popup for the same channel.
        public void ShowIconPickerPopup(string channelKey, Rect activatorRect)
        {
            var popup = new IconPickerPopupContent(this, GetAssetGuid(), channelKey);
            UnityEditor.PopupWindow.Show(activatorRect, popup);
        }

        private string GetAssetGuid() =>
            targetGroup != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(targetGroup)) : null;

        // A handful of built-in icons relevant to audio, offered as quick presets alongside
        // the option to drag in any custom texture.
        private static readonly string[] IconPresets =
        {
            "AudioSource Icon",
            "AudioMixerController Icon",
            "AudioClip Icon",
            "AudioReverbZone Icon",
            "AudioListener Icon",
            "ScriptableObject Icon",
        };

        // Standalone popup opened from the "Icon" button next to the name field. Combines the
        // custom-texture drop target and the icon presets (each shown with its actual image, so
        // you can see what you're picking before you pick it — a plain GenericMenu can't render
        // images in its items) into one place, rather than an inline section in the inspector.
        private class IconPickerPopupContent : PopupWindowContent
        {
            private readonly AudioChannelEditorWindow owner;
            private readonly string assetGuid;
            private readonly string channelKey;

            public IconPickerPopupContent(AudioChannelEditorWindow owner, string assetGuid, string channelKey)
            {
                this.owner = owner;
                this.assetGuid = assetGuid;
                this.channelKey = channelKey;
            }

            public override Vector2 GetWindowSize() => new Vector2(190, 160 + IconPresets.Length * 22 + 30);

            public override void OnGUI(Rect rect)
            {
                EditorGUILayout.LabelField("Custom Texture", EditorStyles.miniBoldLabel);

                const float fieldWidth = 140f;
                var reservedRect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));
                var fieldRect = new Rect(reservedRect.x + (reservedRect.width - fieldWidth) / 2f, reservedRect.y, fieldWidth, reservedRect.height);
                var droppedTexture = EditorGUI.ObjectField(fieldRect, GUIContent.none, null, typeof(Texture2D), false) as Texture2D;

                if (droppedTexture != null)
                {
                    ChannelIconPrefs.SetCustomIcon(assetGuid, channelKey, droppedTexture);
                    owner.treeView.Reload();
                    owner.Repaint();
                    editorWindow.Close();
                }

                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Presets", EditorStyles.miniBoldLabel);

                foreach (var iconName in IconPresets)
                {
                    var iconContent = EditorGUIUtility.IconContent(iconName);
                    string label = ObjectNames.NicifyVariableName(iconName.Replace(" Icon", ""));
                    var buttonContent = new GUIContent(" " + label, iconContent != null ? iconContent.image : null);

                    if (GUILayout.Button(buttonContent, GUILayout.Height(20)))
                    {
                        ChannelIconPrefs.SetBuiltinIcon(assetGuid, channelKey, iconName);
                        owner.treeView.Reload();
                        owner.Repaint();
                        editorWindow.Close();
                    }
                }

                EditorGUILayout.Space(4);
                if (GUILayout.Button("Reset to Default", GUILayout.Height(20)))
                {
                    ChannelIconPrefs.ClearIcon(assetGuid, channelKey);
                    owner.treeView.Reload();
                    owner.Repaint();
                    editorWindow.Close();
                }
            }
        }

        private void DrawSectionHeader(string title)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        // AudioChannelGroup fields, split into "General" (misc) and "Object Pool" (it always
        // owns its own pool, instantiated fresh per scene from parentPrefab — there's
        // no useParentPool toggle at this level, and nothing to inherit audioLookup from since
        // it's the root). "ID" and "children" are handled elsewhere (editable header / the tree
        // itself); "options" gets its own section below.
        private void DrawGroupRootFields()
        {
            var audioLookupProp = serializedGroup.FindProperty("audioLookup");
            var poolInitialSizeProp = serializedGroup.FindProperty("initialSize");
            var poolParentPrefabProp = serializedGroup.FindProperty("parentPrefab");
            var poolPrefabProp = serializedGroup.FindProperty("prefab");
            var poolScenePersistantProp = serializedGroup.FindProperty("scenePersistant");

            DrawSectionHeader("General");
            if (audioLookupProp != null) EditorGUILayout.PropertyField(audioLookupProp);

            DrawSectionHeader("Object Pool");
            if (poolInitialSizeProp != null) EditorGUILayout.PropertyField(poolInitialSizeProp);
            if (poolParentPrefabProp != null) EditorGUILayout.PropertyField(poolParentPrefabProp);

            if (poolPrefabProp != null)
            {
                EditorGUILayout.PropertyField(poolPrefabProp);
                if (poolPrefabProp.objectReferenceValue == null)
                    EditorGUILayout.HelpBox("A prefab is required for the root channel's pool.", MessageType.Warning);
            }

            if (poolScenePersistantProp != null) EditorGUILayout.PropertyField(poolScenePersistantProp);
        }

        // AudioChannel fields, split into "General" (audioLookup, hidden while
        // inheritAudioLookup is true) and "Object Pool" (useParentPool plus its own pool
        // settings, hidden entirely while useParentPool is true since they're irrelevant then).
        // Within Object Pool: parentPrefab is hidden whenever EITHER useParentPool
        // or useParentPoolParentPrefab is true, and prefab is hidden whenever EITHER
        // useParentPool or useParentPoolPrefab is true — each mirrors its own [HideIf(...)]
        // combined condition on the actual field. A prefab warning shows only once
        // prefab is actually visible and unset. "ID" and "children" are handled
        // elsewhere; "options"/"inheritNullOptions" go under Options below.
        //
        // Note: plain PropertyFields don't understand Alchemy's [FoldoutGroup]/[HideIf] —
        // those render as flat fields here rather than the fancy default-inspector layout.
        private void DrawChannelFields(SerializedProperty property)
        {
            var inheritAudioLookupProp = property.FindPropertyRelative("inheritAudioLookup");
            var audioLookupProp = property.FindPropertyRelative("audioLookup");
            var useParentPoolProp = property.FindPropertyRelative("useParentPool");
            var poolInitialSizeProp = property.FindPropertyRelative("initialSize");
            var useParentPoolParentPrefabProp = property.FindPropertyRelative("useParentPoolParentPrefab");
            var poolParentPrefabProp = property.FindPropertyRelative("parentPrefab");
            var useParentPoolPrefabProp = property.FindPropertyRelative("useParentPoolPrefab");
            var poolPrefabProp = property.FindPropertyRelative("prefab");

            DrawSectionHeader("General");
            bool inheritAudioLookup = inheritAudioLookupProp != null && inheritAudioLookupProp.boolValue;
            if (inheritAudioLookupProp != null) EditorGUILayout.PropertyField(inheritAudioLookupProp);
            if (!inheritAudioLookup && audioLookupProp != null) EditorGUILayout.PropertyField(audioLookupProp);

            DrawSectionHeader("Object Pool");
            bool useParentPool = useParentPoolProp != null && useParentPoolProp.boolValue;
            if (useParentPoolProp != null) EditorGUILayout.PropertyField(useParentPoolProp);

            if (!useParentPool)
            {
                if (poolInitialSizeProp != null) EditorGUILayout.PropertyField(poolInitialSizeProp);

                bool useParentPoolParentPrefab = useParentPoolParentPrefabProp != null && useParentPoolParentPrefabProp.boolValue;
                if (useParentPoolParentPrefabProp != null) EditorGUILayout.PropertyField(useParentPoolParentPrefabProp);

                if (!useParentPoolParentPrefab && poolParentPrefabProp != null)
                    EditorGUILayout.PropertyField(poolParentPrefabProp);

                bool useParentPoolPrefab = useParentPoolPrefabProp != null && useParentPoolPrefabProp.boolValue;
                if (useParentPoolPrefabProp != null) EditorGUILayout.PropertyField(useParentPoolPrefabProp);

                if (!useParentPoolPrefab && poolPrefabProp != null)
                {
                    EditorGUILayout.PropertyField(poolPrefabProp);
                    if (poolPrefabProp.objectReferenceValue == null)
                        EditorGUILayout.HelpBox("A prefab is required when not using the parent pool.", MessageType.Warning);
                }
            }
        }
        #endregion



        #region Options Section
        // Finds every (has_X, X) field pair directly under an Options property, regardless
        // of which concrete option fields exist — new ones added to Options later just work.
        private List<(SerializedProperty hasProp, SerializedProperty valueProp, string fieldName)> FindOptionPairs(SerializedProperty optionsProp)
        {
            var propsByName = new Dictionary<string, SerializedProperty>();

            var iterator = optionsProp.Copy();
            var end = optionsProp.GetEndProperty();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;
                propsByName[iterator.name] = iterator.Copy();
            }

            var pairs = new List<(SerializedProperty, SerializedProperty, string)>();
            foreach (var kvp in propsByName)
            {
                if (!kvp.Key.StartsWith("has_")) continue;

                string fieldName = kvp.Key.Substring(4);
                if (propsByName.TryGetValue(fieldName, out var valueProp))
                    pairs.Add((kvp.Value, valueProp, fieldName));
            }

            return pairs;
        }

        private void DrawOptionsSection(SerializedProperty optionsProp, SerializedProperty inheritNullOptionsProp)
        {
            DrawSectionHeader("Options");
            EditorGUILayout.Space(2);

            if (inheritNullOptionsProp != null)
                EditorGUILayout.PropertyField(inheritNullOptionsProp);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Parameters");
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Add Option", EditorStyles.miniButton, GUILayout.Width(90)))
                {
                    var pairs = FindOptionPairs(optionsProp);
                    var menuRect = new Rect(Event.current.mousePosition, Vector2.zero);
                    BuildAddOptionMenu(pairs).DropDown(menuRect);
                }
            }

            var addedPairs = FindOptionPairs(optionsProp).Where(p => p.hasProp.boolValue).ToList();

            if (addedPairs.Count == 0)
            {
                EditorGUILayout.HelpBox("No options added — use \"Add Option\" to override specific AudioSource settings for this channel.", MessageType.None);
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                foreach (var group in addedPairs.GroupBy(p => GetCategory(p.fieldName)))
                {
                    EditorGUILayout.LabelField(group.Key, EditorStyles.miniBoldLabel);

                    using (new EditorGUI.IndentLevelScope())
                    {
                        foreach (var pair in group)
                            DrawOptionRow(pair.valueProp, pair.hasProp, pair.fieldName);
                    }
                }
            }
        }

        private void DrawOptionRow(SerializedProperty valueProp, SerializedProperty hasProp, string fieldName)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(fieldName));
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("x", GUILayout.Width(20)))
                        hasProp.boolValue = false;
                }

                EditorGUILayout.PropertyField(valueProp, GUIContent.none);
            }

            EditorGUILayout.Space(2);
        }

        private GenericMenu BuildAddOptionMenu(List<(SerializedProperty hasProp, SerializedProperty valueProp, string fieldName)> pairs)
        {
            var menu = new GenericMenu();
            var available = pairs.Where(p => !p.hasProp.boolValue).ToList();

            if (available.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("All options added"));
                return menu;
            }

            foreach (var pair in available)
            {
                string path = $"{GetCategory(pair.fieldName)}/{ObjectNames.NicifyVariableName(pair.fieldName)}";
                var hasProp = pair.hasProp;

                menu.AddItem(new GUIContent(path), false, () =>
                {
                    hasProp.boolValue = true;
                    serializedGroup.ApplyModifiedProperties();
                    Repaint();
                });
            }

            return menu;
        }
        #endregion
        
        #endregion
    }
    #endregion





    #region Mixer Generation (Reflection)
    internal static class AudioMixerReflection
    {
        private const BindingFlags AnyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags AnyStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        private static readonly Type ControllerType = typeof(Editor).Assembly.GetType("UnityEditor.Audio.AudioMixerController");
        private static readonly Type GroupControllerType = typeof(Editor).Assembly.GetType("UnityEditor.Audio.AudioMixerGroupController");
        private static readonly Type GroupParameterPathType = typeof(Editor).Assembly.GetType("UnityEditor.Audio.AudioGroupParameterPath");
        private static readonly Type ExposedParameterType = typeof(Editor).Assembly.GetType("UnityEditor.Audio.ExposedAudioParameter");
        private static readonly Type GuidType = typeof(Editor).Assembly.GetType("UnityEditor.GUID");

        private static readonly MethodInfo CreateAtPathMethod = ControllerType?.GetMethod("CreateMixerControllerAtPath", AnyStatic);
        private static readonly MethodInfo CreateNewGroupMethod = ControllerType?.GetMethod("CreateNewGroup", AnyInstance);
        private static readonly MethodInfo AddChildToParentMethod = ControllerType?.GetMethod("AddChildToParent", AnyInstance);
        private static readonly PropertyInfo MasterGroupProperty = ControllerType?.GetProperty("masterGroup", AnyInstance);

        // For exposing a group's Volume parameter under a custom name — the same thing
        // right-clicking Volume → "Expose 'Volume (of Group)' to script" does in the Mixer window.
        private static readonly MethodInfo GetGuidForVolumeMethod = GroupControllerType?.GetMethod("GetGUIDForVolume", AnyInstance);
        private static readonly MethodInfo AddExposedParameterMethod = ControllerType?.GetMethod("AddExposedParameter", AnyInstance);
        private static readonly ConstructorInfo GroupParameterPathConstructor =
            (GroupParameterPathType != null && GroupControllerType != null && GuidType != null)
                ? GroupParameterPathType.GetConstructor(AnyInstance, null, new[] { GroupControllerType, GuidType }, null)
                : null;
        private static readonly PropertyInfo ExposedParametersProperty = ControllerType?.GetProperty("exposedParameters", AnyInstance);
        private static readonly FieldInfo ExposedParamNameField = ExposedParameterType?.GetField("name", AnyInstance);
        private static readonly FieldInfo ExposedParamGuidField = ExposedParameterType?.GetField("guid", AnyInstance);

        public static bool IsAvailable =>
            ControllerType != null && CreateAtPathMethod != null && CreateNewGroupMethod != null &&
            AddChildToParentMethod != null && MasterGroupProperty != null;

        public static bool CanExposeVolume =>
            GroupControllerType != null && GetGuidForVolumeMethod != null && AddExposedParameterMethod != null &&
            GroupParameterPathConstructor != null && ExposedParametersProperty != null &&
            ExposedParamNameField != null && ExposedParamGuidField != null;

        // Creates a brand-new .mixer asset (with its default Master group) at a project-relative path.
        public static AudioMixer CreateMixerAsset(string assetPath) =>
            CreateAtPathMethod.Invoke(null, new object[] { assetPath }) as AudioMixer;

        public static AudioMixerGroup GetMasterGroup(AudioMixer mixer) =>
            MasterGroupProperty.GetValue(mixer) as AudioMixerGroup;

        // storeUndoState is deliberately false: this generation is a one-shot batch operation
        // over a brand-new asset, not something meant to be undoable step-by-step.
        public static AudioMixerGroup CreateGroup(AudioMixer mixer, string name) =>
            CreateNewGroupMethod.Invoke(mixer, new object[] { name, false }) as AudioMixerGroup;

        public static void AddChildToParent(AudioMixer mixer, AudioMixerGroup child, AudioMixerGroup parent) =>
            AddChildToParentMethod.Invoke(mixer, new object[] { child, parent });

        // Exposes a group's Volume parameter under exposedName (e.g. "Music_Volume") and returns
        // whether it succeeded. No-op (returns false) if CanExposeVolume is false, so a Unity
        // version where this internal shape has changed just silently skips exposing rather than
        // breaking mixer generation entirely.
        public static bool ExposeVolumeParameter(AudioMixer mixer, AudioMixerGroup group, string exposedName)
        {
            if (!CanExposeVolume) return false;

            object volumeGuid = GetGuidForVolumeMethod.Invoke(group, null);
            if (volumeGuid == null) return false;

            object path = GroupParameterPathConstructor.Invoke(new object[] { group, volumeGuid });
            AddExposedParameterMethod.Invoke(mixer, new object[] { path });

            // AddExposedParameter assigns a default generated name — find the entry we just
            // added (by matching guid) and rename it. Arrays of structs need to be rebuilt and
            // reassigned wholesale; mutating a GetValue() copy in place wouldn't persist.
            var exposedArray = ExposedParametersProperty.GetValue(mixer) as Array;
            if (exposedArray == null) return false;

            var newArray = Array.CreateInstance(ExposedParameterType, exposedArray.Length);
            bool found = false;

            for (int i = 0; i < exposedArray.Length; i++)
            {
                object entry = exposedArray.GetValue(i);
                if (!found && volumeGuid.Equals(ExposedParamGuidField.GetValue(entry)))
                {
                    ExposedParamNameField.SetValue(entry, exposedName);
                    found = true;
                }

                newArray.SetValue(entry, i);
            }

            ExposedParametersProperty.SetValue(mixer, newArray);
            return found;
        }
    }
    #endregion





    #region Inspector
    [CustomEditor(typeof(AudioChannelGroup))]
    internal class AudioChannelGroup_Editor : Editor_Base<AudioChannelGroup_Editor>
    {
        protected override void DrawInspector()
        {
            using (new EditorGUI.DisabledScope(true))
                DrawInputProperty("Root Channel Name", Prop("ID"));

            Space();

            DrawButton("Open Audio Channel Editor", () => AudioChannelEditorWindow.OpenFor((AudioChannelGroup)target));
        }
    }
    #endregion
}