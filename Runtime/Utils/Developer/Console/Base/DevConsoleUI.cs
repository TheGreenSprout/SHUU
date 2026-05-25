using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using SHUU.Utils.Helpers;

namespace SHUU.Utils.Developer.Console
{

    public class DevConsoleUI : MonoBehaviour
    {
        #region Variables
        [Header("References")]
        private DevConsoleManager controller;

        [SerializeField] private TMP_InputField inputField;
        public ScrollRect scrollRect;
        [SerializeField] private TMP_Text outputText;


        [HideInInspector] public bool inputFieldActive => inputField.isFocused;



        // Internal
        private static List<string> previousCommands = new();


        private static int previousCommandIndex = -1;
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
        
        
        public void Print(string message, Color? textColor = null)
        {
            if (message == null) return;

            
            (string, string) colortag = ("", "");
            if (textColor != null) colortag = (textColor.Value.GetColorOpenTag_RichText(), "</color>");


            outputText.text += $"{colortag.Item1}{message}{colortag.Item2}\n";

            
            Canvas.ForceUpdateCanvases();

            
            scrollRect.verticalNormalizedPosition = 0f;
        }
        public void Print(Color? textColor = null, params string[] message)
        {
            foreach (string line in message)
            {
                Print(line, textColor);
            }
        }
        public void Print(params (string, Color?)[] message)
        {
            foreach ((string line, Color? color) input in message)
            {
                Print(input.line, input.color);
            }
        }
        #endregion
    
        #endregion
    }

}
