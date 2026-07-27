using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using SHUU.Utils.Helpers;
using SHUU.Utils.Globals;
using System;

namespace SHUU.Utils.Developer.Console
{

    public class DevConsoleUI : MonoBehaviour
    {
        #region Variables
        [Header("References")]
        private DevConsoleManager controller;

        [SerializeField] private TMP_InputField inputField;
        public ScrollRect scrollRect;
        public float scrollSensitivity
        {
            get => scrollRect.scrollSensitivity;
            set => scrollRect.scrollSensitivity = value;
        }
        [SerializeField] private TMP_Text outputText;


        [HideInInspector] public bool inputFieldActive => inputField.isFocused;



        // Internal
        private static List<string> previousCommands = new();

        private static int previousCommandIndex = -1;


        private bool _printing = false;
        private bool printing
        {
            get => _printing;
            set
            {
                bool prev = _printing;
                _printing = value;

                if (!_printing && prev && printingQueue != null && printingQueue.Count > 0) printingQueue.Dequeue().Invoke();
            }
        }
        private Queue<Action> printingQueue = new();
        #endregion




        #region Main
        private void Awake()
        {
            controller = HandyFunctions.SearchComponent_InSelfAndParents<DevConsoleManager>(this.transform);

            inputField.onSubmit.AddListener(SubmitCommand);
        }


        private void OnEnable()
        {
            controller.inputModule.previousCommand += PreviousCommand;
            controller.inputModule.nextCommand += NextCommand;
        }
        
        private void OnDisable()
        {
            controller.inputModule.previousCommand -= PreviousCommand;
            controller.inputModule.nextCommand -= NextCommand;
        }


        private void Update()
        {
            if (previousCommands.Count == 0) return;
        }
        #endregion



        #region Logic
        
        #region General
        public void Toggle()
        {
            gameObject.SetActive(!gameObject.activeInHierarchy);


            inputField.text = "";

            if (gameObject.activeInHierarchy) inputField.ActivateInputField();
        }
        #endregion



        #region Command recalling
        private void NextCommand()
        {
            if (previousCommands.Count == 0) return;

            if (previousCommandIndex > 0)
            {
                previousCommandIndex--;
                inputField.text = previousCommands[previousCommands.Count - 1 - previousCommandIndex];
                inputField.caretPosition = inputField.text.Length;
            }
            else ResetPreviousCommandIndex();
        }

        private void PreviousCommand()
        {
            if (previousCommands.Count == 0) return;

            if (previousCommandIndex < previousCommands.Count - 1)
            {
                previousCommandIndex++;
                inputField.text = previousCommands[previousCommands.Count - 1 - previousCommandIndex];
                inputField.caretPosition = inputField.text.Length;
            }
        }


        private void ResetPreviousCommandIndex()
        {
            if (previousCommandIndex != -1) previousCommandIndex = -1;
            inputField.text = "";
        }
        #endregion



        #region Command submission
        public void Button_SubmitCommand() => SubmitCommand(inputField.text);
        public void SubmitCommand(string text)
        {
            if (string.IsNullOrEmpty(text)) return;


            if (controller.ProcessInput(text))
            {
                if (previousCommands.Contains(text)) previousCommands.Remove(text);

                previousCommands.Add(text);
            }

            ResetPreviousCommandIndex();
            inputField.ActivateInputField();
        }
        
        
        public void Print(string message, Color? textColor = null, bool forcedCanvasUpdate = true)
        {
            if (printing)
            {
                printingQueue.Enqueue(() => Print(message, textColor, forcedCanvasUpdate));
                return;
            }

            printing = true;

            PrintInternal(message, textColor, forcedCanvasUpdate);

            printing = false;
        }
        private void PrintInternal(string message, Color? textColor = null, bool forcedCanvasUpdate = true)
        {
            if (message == null) return;


            (string, string) colortag = ("", "");
            if (textColor != null) colortag = (textColor.Value.GetColorOpenTag_RichText(), "</color>");

            outputText.text += $"{colortag.Item1}{message}{colortag.Item2}\n";

            
            if (forcedCanvasUpdate) FixScrollRect();
        }

        public void Print(Color? textColor = null, params string[] message)
        {
            if (printing)
            {
                printingQueue.Enqueue(() => Print(textColor, message));
                return;
            }

            printing = true;

            foreach (string line in message)
                PrintInternal(line, textColor, false);

            FixScrollRect();

            printing = false;
        }
        public void Print(params (string, Color?)[] message)
        {
            if (printing)
            {
                printingQueue.Enqueue(() => Print(message));
                return;
            }

            printing = true;

            foreach ((string line, Color? color) input in message)
                PrintInternal(input.line, input.color);

            FixScrollRect();

            printing = false;
        }

        public void PrintGradually(float delay, int index, AudioClip clip, Color? textColor = null, params string[] message)
            => _PrintGradually(false, delay, index, clip, textColor, message);
        private void _PrintGradually(bool printPass, float delay, int index, AudioClip clip, Color? textColor = null, params string[] message)
        {
            if (printing && !printPass)
            {
                printingQueue.Enqueue(() => PrintGradually(delay, index, clip, textColor, message));
                return;
            }

            if (index < 0 || index >= message.Length)
            {
                if (printing) printing = false;

                return;
            }

            if (!printing) printing = true;

            PrintInternal(message[index], textColor, false);
            FixScrollRect();

            if (clip != null && controller.gameObject.activeInHierarchy) SHUU_Audio.PlaySfxAt(Camera.main.transform, clip, new SFX_Options { spatialBlend = 0f });

            index++;
            if (controller.gameObject.activeInHierarchy) SHUU_Time.Timer(delay, () => _PrintGradually(true, delay, index, clip, textColor, message), true);
            else _PrintGradually(true, delay, index, clip, textColor, message);
        }
        public void PrintGradually(float delay, int index, AudioClip clip, params (string, Color?)[] message) => _PrintGradually(false, delay, index, clip, message);
        private void _PrintGradually(bool printPass, float delay, int index, AudioClip clip, params (string, Color?)[] message)
        {
            if (printing && !printPass)
            {
                printingQueue.Enqueue(() => PrintGradually(delay, index, clip, message));
                return;
            }

            if (index < 0 || index >= message.Length)
            {
                if (printing) printing = false;

                return;
            }

            if (!printing) printing = true;

            PrintInternal(message[index].Item1, message[index].Item2, false);
            FixScrollRect();

            if (clip != null && controller.gameObject.activeInHierarchy) SHUU_Audio.PlaySfxAt(Camera.main.transform, clip, new SFX_Options { spatialBlend = 0f });

            index++;
            if (controller.gameObject.activeInHierarchy) SHUU_Time.Timer(delay, () => _PrintGradually(true, delay, index++, clip, message), true);
            else _PrintGradually(true, delay, index, clip, message);
        }

        private void FixScrollRect()
        {
            SHUU_Time.onNextFrame += () =>
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            };
        }
        #endregion
    
        #endregion
    }

}
