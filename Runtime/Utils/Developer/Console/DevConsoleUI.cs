using System.Collections.Generic;
using SHUU.UserSide;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SHUU.Utils.Developer.Console
{

    public class DevConsoleUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DevConsoleManager controller;

        [SerializeField] private GameObject logLinePrefab;
        [SerializeField] private Transform logLineContainer;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private ScrollRect scrollRect;


        [Header("Keys")]
        [SerializeField] private KeyCode previousCommand = KeyCode.UpArrow;
        [SerializeField] private KeyCode nextCommand = KeyCode.DownArrow;



        // Internal
        private List<string> previousCommands;


        private int previousCommandIndex = -1;




        private void Awake()
        {
            inputField.onSubmit.AddListener(SubmitCommand);


            previousCommands = new List<string>();
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


            if (Input.GetKeyDown(previousCommand))
            {
                if (previousCommandIndex < previousCommands.Count - 1)
                {
                    previousCommandIndex++;
                    inputField.text = previousCommands[previousCommands.Count - 1 - previousCommandIndex];
                    inputField.caretPosition = inputField.text.Length;
                }
            }
            if (Input.GetKeyDown(nextCommand))
            {
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
            if (textColor == null) textColor = Color.white;

            GameObject newLine = Instantiate(logLinePrefab, logLineContainer);

            LogLine logLine = newLine.GetComponent<LogLine>();
            logLine.Setup(message, textColor.Value);

            
            Canvas.ForceUpdateCanvases();

            
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

}
