#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;

using SHUU.Utils.BaseScripts.ScriptableObjs.Audio;

using SETB.SuperClasses;
using static SETB.EditorGUI_Base;
using static SETB.HandyEditorFunctions;

namespace SHUU._Editor
{
	[CustomEditor(typeof(Music_Storage), true)]
	public class Music_Storage_Drawer : Editor_Base<Music_Storage_Drawer>
	{
		#region Variables
		SerializedProperty allMusicProp;
        #endregion




        #region Main
        protected override void OnEnable()
        {
            base.OnEnable();

			allMusicProp = Prop("allMusic");
        }

		
        protected override void DrawInspector()
	    {
			Space();

			DrawLabel("Music Storage", EditorStyles.boldLabel);

			Space();

			DrawInputProperty(null, allMusicProp.FindPropertyRelative("Array.size"));

			Space();

			for (int i = 0; i < allMusicProp.arraySize; i++)
			{
				SerializedProperty element = allMusicProp.GetArrayElementAtIndex(i);

				SerializedProperty idProp = PropRelative(element, "IDENTIFIER");
				SerializedProperty clipProp = PropRelative(element, "music");


				bool ret = false;
				Vertical(() =>
				{
					bool open = false;

					Horizontal(() =>
					{
						string label = string.IsNullOrEmpty(idProp.stringValue) ? $"Element {i}" : idProp.stringValue;

						QuickFoldout(GetFoldKey("MusicStorageDrawer", "allMusicElement", i), label, ref open);


						DrawButton("Edit Loop", () => AudioLoopEditor.ShowWindow(serializedObject.targetObject, element.propertyPath), null, GUILayout.Width(90));

						DrawButton("X", () =>
						{
							allMusicProp.DeleteArrayElementAtIndex(i);
							ret = true;
						}, null, GUILayout.Width(20));
					});

					if (ret) return;


					if (open)
					{
						Space(5f);

						Indent(() =>
						{
							DrawInputProperty(null, idProp);
							DrawInputProperty(null, clipProp);

							SerializedProperty loopProp = PropRelative(element, "loopSlices");
							if (loopProp != null)
							{
								var start = PropRelative(loopProp, "startPoint");
								var end = PropRelative(loopProp, "endPoint");

								start.floatValue = DrawInputFloat("Loop start", start.floatValue);
								end.floatValue = DrawInputFloat("Loop end", end.floatValue);
							}
						});
					}
				}, "box");

				if (ret) continue;


				Space(4f);
			}

			Space();

			DrawButton("Add Music", () => allMusicProp.arraySize++);
	    }
		#endregion



		#region Logic
		private string GetFoldKey(string type, string name, int index) => $"{target.GetInstanceID()}_{type}_{index}_{name}";

		private bool QuickFoldout(string key, string label, ref bool open, Action logic = null)
		{
			open = GetEditorPref(key, false);

			bool newOpen = open;
			DrawFoldout(label, ref newOpen, logic, null, true);


			if (newOpen != open) SetEditorPref(key, newOpen);

			return newOpen;
		}
		#endregion
	}
}
#endif
