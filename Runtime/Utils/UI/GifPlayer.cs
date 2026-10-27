using UnityEngine;
using UnityEngine.UI;
using System;

using Alchemy.Inspector;

namespace SHUU.Utils.UI
{
    [RequireComponent(typeof(Image))]
    public class GifPlayer : MonoBehaviour
    {
        #region Variables
        // Inspector
        [Title("Settings")]
        [SerializeField] private Image targetImage;

        [SerializeField] private Sprite[] frames;
        [SerializeField, Min(0.01f)] private float frameRate = 8f;

        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool useUnscaledTime = false;



        // Internal
        private int currentFrame;
        private float timer;

        [HideInInspector] public bool isPlaying;

        public event Action onComplete;
        #endregion




        #region Main
        private void OnEnable()
        {
            if (targetImage == null) targetImage = GetComponent<Image>();
            if (playOnEnable) Play(); else ApplyFrame();
        }


        private void Update()
        {
            if (!isPlaying || targetImage == null || frames == null || frames.Length == 0) return;

            timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float frameTime = 1f / frameRate;

            while (timer >= frameTime)
            {
                timer -= frameTime;
                currentFrame++;

                if (currentFrame >= frames.Length)
                {
                    if (loop) currentFrame = 0;
                    else
                    {
                        currentFrame = frames.Length - 1;
                        isPlaying = false;

                        ApplyFrame();

                        onComplete?.Invoke();

                        return;
                    }
                }
            }
            ApplyFrame();
        }


        private void Reset() => targetImage = GetComponent<Image>();

        private void OnValidate() => frameRate = Mathf.Max(0.01f, frameRate);
        #endregion



        #region Logic

        #region Public API
        public void Play()
        {
            isPlaying = true;
            
            ApplyFrame();
        }

        public void Pause() => isPlaying = false;

        public void Stop()
        {
            isPlaying = false;
            currentFrame = 0;
            timer = 0f;

            ApplyFrame();
        }


        public void SetFrames(Sprite[] newFrames, bool restart = true)
        {
            frames = newFrames;
            if (restart) Stop();
        }
        #endregion



        #region Internal
        private void ApplyFrame()
        {
            if (targetImage == null || frames == null || frames.Length == 0) return;
            currentFrame = Mathf.Clamp(currentFrame, 0, frames.Length - 1);
            targetImage.sprite = frames[currentFrame];
        }
        #endregion

        #endregion
    }
}
