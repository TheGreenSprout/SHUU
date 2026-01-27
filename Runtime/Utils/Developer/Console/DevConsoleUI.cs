using System.Collections.Generic;
using SHUU.Utils.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SHUU.Utils.Developer.Console
{

    public class DevConsoleUI : MonoBehaviour
    {
        [Header("References")]
        private DevConsoleManager controller;

        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TMP_Text outputText;


        [HideInInspector] public bool inputFieldActive => inputField.isFocused;



        // Internal
        private List<string> previousCommands;


        private int previousCommandIndex = -1;




        private void Awake()
        {
            controller = HandyFunctions.SearchComponent_InSelfAndParents<DevConsoleManager>(this.transform);

            inputField.onSubmit.AddListener(SubmitCommand);


            previousCommands = new List<string>();
        }

        private void OnEnable()
        {
            controller.input.previousCommand += PreviousCommand;
            controller.input.nextCommand += NextCommand;
        }
        private void OnDisable()
        {
            controller.input.previousCommand -= PreviousCommand;
            controller.input.nextCommand -= NextCommand;
        }


        public void Toggle()
        {
            gameObject.SetActive(!gameObject.activeInHierarchy);


            inputField.text = "";

            if (gameObject.activeInHierarchy) inputField.ActivateInputField();
        }



        private void Update()
        {
            if (previousCommands.Count == 0) return;
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
        private void NextCommand()
        {
            if (previousCommands.Count == 0) return;

            if (previousCommandIndex > 0)
            {
                previousCommandIndex--;
                inputField.text = previousCommands[previousCommands.Count - 1 - previousCommandIndex];
                inputField.caretPosition = inputField.text.Length;
            }
            else
            {
                ResetPreviousCommandIndex();
            }
        }


        private void ResetPreviousCommandIndex()
        {
            if (previousCommandIndex != -1) previousCommandIndex = -1;
            inputField.text = "";
        }



        public void SubmitCommand(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;



            if (controller.ProcessInput(text))
            {
                if (previousCommands.Contains(text)) previousCommands.Remove(text);

                previousCommands.Add(text);
            }


            ResetPreviousCommandIndex();
            inputField.ActivateInputField();
        }
        public void Button_SubmitCommand()
        {
            SubmitCommand(inputField.text);
        }


        public void Print(string message, Color? textColor = null)
        {
            (string, string) colortag = ("", "");
            if (textColor != null) colortag = (textColor.Value.GetColorOpenTag_RichText(), "</color>");


            outputText.text += $"{colortag.Item1}{message}{colortag.Item2}\n";

            
            Canvas.ForceUpdateCanvases();

            
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

}
