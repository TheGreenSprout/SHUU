using UnityEngine;

[CreateAssetMenu(fileName = "DialoguePortrait", menuName = "SHUU/Dialogue/DialoguePortrait")]
public class DialoguePortrait : ScriptableObject
{
    public Sprite idlePortrait;

    public Sprite talkingPortrait;


    public float idleSize = 1f;
    public float talkSize = 1f;

    public Vector2 idleOffset = new Vector2(0f, 0f);
    public Vector2 talkOffset = new Vector2(0f, 0f);
}
