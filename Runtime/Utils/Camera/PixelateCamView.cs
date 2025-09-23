using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PixelateCamView : MonoBehaviour
{
    private Camera cam;

    private GameObject camObj;


    public RenderTexture camTexture;



    public int scaleMultiplyAmmount = 50;


    private int[] screenValues;




    private void Awake()
    {
        RefreshScreenValues();



        cam = this.GetComponent<Camera>();

        camObj = this.gameObject;
    }

    private void OnEnable()
    {
        ReSizeRenderTexture();
    }


    public void RefreshScreenValues()
    {
        screenValues = new int[] { Screen.width, Screen.height };
    }



    public bool reSize = false;
    private void Update()
    {
        if (reSize)
        {
            reSize = false;

            ReSizeRenderTexture();
        }
    }



    public void ReSizeRenderTexture()
    {
        if (cam.targetTexture != null)
        {
            cam.targetTexture = null;
        }
        camObj.SetActive(false);

        camTexture.Release();


        /*int gcd = GCD(screenValues[0], screenValues[1]);
        int aspectWidth;
        int aspectHeight;
        if (gcd != 1)
        {
            aspectWidth = screenValues[0] / gcd;
            aspectHeight = screenValues[1] / gcd;
        }
        else{
            int[] closestRatio = GetClosestAspectRatio();

            aspectWidth = closestRatio[0];
            aspectHeight = closestRatio[1];
        }*/

        int[] closestRatio = GetClosestAspectRatio();

        int aspectWidth = closestRatio[0];
        int aspectHeight = closestRatio[1];

        camTexture.width = aspectWidth * scaleMultiplyAmmount;
        camTexture.height = aspectHeight * scaleMultiplyAmmount;


        camTexture.Create();

        cam.targetTexture = camTexture;
        camObj.SetActive(true);
    }

    /*int GCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }*/

    int[] GetClosestAspectRatio()
    {
        float aspectRatio = (float)screenValues[0] / screenValues[1];


        int[][] commonRatios = new int[][]
        {
            new int[] { 1, 1 },  // 1:1
            new int[] { 1, 2 },  // 1:2
            new int[] { 2, 3 },  // 2:3
            new int[] { 3, 4 },  // 3:4
            new int[] { 4, 5 },  // 4:5
            new int[] { 5, 6 },  // 5:6
            new int[] { 6, 7 },  // 6:7
            new int[] { 7, 8 },  // 7:8
            new int[] { 8, 9 },  // 8:9
            new int[] { 9, 10 }, // 9:10
            new int[] { 1, 3 },  // 1:3
            new int[] { 2, 5 },  // 2:5
            new int[] { 3, 7 },  // 3:7
            new int[] { 4, 9 },  // 4:9
            new int[] { 5, 8 },  // 5:8
            new int[] { 7, 10 }, // 7:10
            new int[] { 3, 5 },  // 3:5
            new int[] { 2, 7 },  // 2:7
            new int[] { 5, 9 },  // 5:9
            new int[] { 6, 11 }, // 6:11
            new int[] { 11, 13 }, // 11:13
            new int[] { 13, 14 }, // 13:14
            new int[] { 16, 9 }, // 16:9 (HD, Full HD, etc.)
            new int[] { 21, 9 }, // 21:9 (Ultrawide)
            new int[] { 16, 10 }, // 16:10 (Common for some displays)
            new int[] { 4, 3 },  // 4:3 (Traditional CRT aspect ratio)
            new int[] { 3, 2 },  // 3:2 (Common for cameras, such as 35mm film)
            new int[] { 2, 1 },  // 2:1 (Some cinematic aspect ratios)
            new int[] { 5, 4 },  // 5:4 (Common in older computer monitors)
            new int[] { 1, 4 },  // 1:4 (Tall aspect ratio, used for some phones)
        };


        // Find the closest match
        int[] closestRatio = new int[2];
        float closestDifference = float.MaxValue;

        for (int i = 0; i < commonRatios.Length; i++)
        {
            float difference = Mathf.Abs(aspectRatio - ((float)commonRatios[i][0] / commonRatios[i][1]));

            if (difference < closestDifference)
            {
                closestDifference = difference;
                closestRatio = commonRatios[i];
            }
        }

        while (closestRatio[0] + closestRatio[1] < 19)
        {
            closestRatio[0] *= 2;
            closestRatio[1] *= 2;
        }

        return closestRatio;
    }
}
