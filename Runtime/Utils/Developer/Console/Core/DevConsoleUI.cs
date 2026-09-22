using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

using SHUU.Utils.Helpers;
using SHUU.Utils.Globals;

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

        [SerializeField] private AudioSource source;

         
        [HideInInspector] public bool inputFieldActive => inputField.isFocused;



        // Internal
        private static List<string> PreviousCommands = new();

        private static int PreviousCommandIndex = -1;


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
            if (PreviousCommands.Count == 0) return;
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
            if (PreviousCommands.Count == 0) return;

            if (PreviousCommandIndex > 0)
            {
                PreviousCommandIndex--;
                inputField.text = PreviousCommands[PreviousCommands.Count - 1 - PreviousCommandIndex];
                inputField.caretPosition = inputField.text.Length;
            }
            else ResetPreviousCommandIndex();
        }

        private void PreviousCommand()
        {
            if (PreviousCommands.Count == 0) return;

            if (PreviousCommandIndex < PreviousCommands.Count - 1)
            {
                PreviousCommandIndex++;
                inputField.text = PreviousCommands[PreviousCommands.Count - 1 - PreviousCommandIndex];
                inputField.caretPosition = inputField.text.Length;
            }
        }


        private void ResetPreviousCommandIndex()
        {
            if (PreviousCommandIndex != -1) PreviousCommandIndex = -1;
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
                if (PreviousCommands.Contains(text)) PreviousCommands.Remove(text);

                PreviousCommands.Add(text);
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

            if (clip != null && controller.gameObject.activeInHierarchy) source.PlayOneShot(clip);

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

            if (clip != null && controller.gameObject.activeInHierarchy) source.PlayOneShot(clip);

            index++;
            if (controller.gameObject.activeInHierarchy) SHUU_Time.Timer(delay, () => _PrintGradually(true, delay, index++, clip, message), true);
            else _PrintGradually(true, delay, index, clip, message);
        }

        private void FixScrollRect()
        {
            SHUU_Time.OnNextFrame += () =>
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            };
        }
        #endregion
    
        #endregion
    }

}
