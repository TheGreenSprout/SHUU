using UnityEngine;
using UnityEditor;

public class DialogueAssistantWindow : EditorWindow
{
    private GUIStyle titleStyle;




    [MenuItem("Tools/Sprout's Handy Unity Utils/Dialogue Assistant")]
    public static void ShowWindow()
    {
        GetWindow<DialogueAssistantWindow>("Dialogue Instance Creator");
    }


    void OnGUI()
    {
        // Title
        GUILayout.Label("Dialogue Assistant");
        GUILayout.Space(20);

        // Tool buttons
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Dialogue Creator", GUILayout.Width(300), GUILayout.Height(50)))
        {
            //DialogueCreator.Open();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Dialogue Portrait Creator", GUILayout.Width(300), GUILayout.Height(50)))
        {
            //DialoguePortraitCreator.Open();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Dialogue Sounds Creator", GUILayout.Width(300), GUILayout.Height(50)))
        {
            DialogueSoundsCreator.Open();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }
}
