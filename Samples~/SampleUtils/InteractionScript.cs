using System;
using SHUU.Utils;
using UnityEngine;

#region XML doc
/// <summary>
/// Example script of how to code basic interaction logic using SproutsHUU's interaction system.
/// </summary>
#endregion
public class InteractionScript : Interactable
{
    public Material mat1;
    public Material mat2;


    public GameObject hoverIndicator;



    private MeshRenderer meshRenderer;




    protected override void Awake()
    {
        base.Awake();


        meshRenderer = GetComponent<MeshRenderer>();
    }


    public override void Interact()
    {
        if (meshRenderer.sharedMaterial == mat1)
        {
            meshRenderer.material = mat2;
        }
        else
        {
            meshRenderer.material = mat1;
        }
    }

    public override void HoverStart()
    {
        hoverIndicator.SetActive(true);
    }

    public override void HoverEnd()
    {
        hoverIndicator.SetActive(false);
    }



    public void SetMaterialFromBool(bool b)
    {
        if (b)
        {
            meshRenderer.material = mat1;
        }
        else
        {
            meshRenderer.material = mat2;
        }
    }

    public bool GetBooleanFromCurrentMaterial()
    {
        if (meshRenderer.sharedMaterial == mat1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
