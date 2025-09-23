using UnityEngine;
using System.Collections;

public class UIfloat : MonoBehaviour
{
    public float floatStrength = 1.0f;
    public float floatSpeed = 1.0f;
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 5.0f;
    public float scaleMagnitude = 1.5f;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private RectTransform rectTransform;
    private bool isShaking = false;

    private float randomOffsetX;  // Phase offset for X-axis
    private float randomOffsetY;  // Phase offset for Y-axis

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("RectTransform component is not found!");
            enabled = false;
            return;
        }

        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;

        // Generate random offsets for the sine wave to make each object unique
        randomOffsetX = Random.Range(0f, 2f * Mathf.PI);
        randomOffsetY = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        if (!isShaking)
        {
            FloatObject();
        }
    }

    void FloatObject()
    {
        // Add random phase offsets so that each object moves differently
        float x = originalPosition.x + Mathf.Sin(Time.time * floatSpeed + randomOffsetX) * floatStrength;
        float y = originalPosition.y + Mathf.Cos(Time.time * floatSpeed + randomOffsetY) * floatStrength;
        rectTransform.anchoredPosition = new Vector3(x, y, originalPosition.z);
    }

    public void ShakeAndScale()
    {
        StartCoroutine(ShakeAndScaleCoroutine());
    }

    IEnumerator ShakeAndScaleCoroutine()
    {
        isShaking = true;

        float elapsedTime = 0.0f;

        while (elapsedTime < shakeDuration)
        {
            float x = Random.Range(-shakeMagnitude, shakeMagnitude) + originalPosition.x;
            float y = Random.Range(-shakeMagnitude, shakeMagnitude) + originalPosition.y;
            rectTransform.anchoredPosition = new Vector3(x, y, originalPosition.z);

            float scale = Mathf.Lerp(originalScale.x, scaleMagnitude, elapsedTime / shakeDuration);
            rectTransform.localScale = new Vector3(scale, scale, originalScale.z);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = originalScale;
        isShaking = false;
    }
}
