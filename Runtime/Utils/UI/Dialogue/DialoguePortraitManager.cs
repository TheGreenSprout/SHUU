using System;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePortraitManager : MonoBehaviour
{
    public Image portraitImage;

    public UIfloat uiFloat;


    public DialoguePortrait startPortrait = null;



    private bool talking { get; set; }

    public float talkSpd = 1f;



    public DialoguePortrait currentPortrait = null;



    private Action talkingAction = null;




    private void Start() {
        if (startPortrait != null)
        {
            ChangeCurrentPortrait(startPortrait);
        }
    }



    public void ChangeCurrentPortrait(DialoguePortrait portrait = null)
    {
        currentPortrait = portrait;

        IdlePortrait();
    }



    public void IdlePortrait()
    {
        talkingAction = TalkPortrait;



        portraitImage.sprite = currentPortrait.idlePortrait;


        RectTransform transfrm = portraitImage.gameObject.GetComponent<RectTransform>();

        transfrm.localScale = new Vector3(currentPortrait.idleSize, currentPortrait.idleSize, 1f);
        transfrm.localPosition = new Vector3(currentPortrait.idleOffset.x, currentPortrait.idleOffset.y, 0f);
    }

    public void TalkPortrait()
    {
        talkingAction = IdlePortrait;
        


        portraitImage.sprite = currentPortrait.talkingPortrait;


        RectTransform transfrm = portraitImage.gameObject.GetComponent<RectTransform>();

        transfrm.localScale = new Vector3(currentPortrait.talkSize, currentPortrait.talkSize, 1f);
        transfrm.localPosition = new Vector3(currentPortrait.talkOffset.x, currentPortrait.talkOffset.y, 0f);
    }


    public void StartTalking()
    {
        talking = true;


        ChangeSpr();
    }
    public void StopTalking()
    {
        talking = false;
        

        IdlePortrait();
    }


    private void ChangeSpr()
    {
        if (!talking)
        {
            return;
        }



        talkingAction();


        Invoke(nameof(ChangeSpr), talkSpd);
    }
}
