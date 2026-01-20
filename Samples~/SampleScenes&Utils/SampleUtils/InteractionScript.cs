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


    private MeshRenderer meshRenderer;




    protected override void Awake()
    {
        base.Awake();


        meshRenderer = GetComponent<MeshRenderer>();
    }



    protected override void InteractLogic()
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
