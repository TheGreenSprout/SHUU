using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SHUU.Utils.Helpers
{
    public class CodeListener : MonoBehaviour
    {
        #region Variables
        // External
        [SerializeField] private int resetThreshold = 10;

        [SerializeField] private List<CodeData> codes = new();



        // Internal
        private List<InnerCodeData> data = new();

        [SerializeField] private List<KeyCode> currentInput = new();


        #region Info
        private static readonly Dictionary<char, KeyCode> SpecialKeys = new()
        {
            { ' ', KeyCode.Space },
            { '\n', KeyCode.Return },
            { '\t', KeyCode.Tab },
            { '-', KeyCode.Minus },
            { '=', KeyCode.Equals },
            { '[', KeyCode.LeftBracket },
            { ']', KeyCode.RightBracket },
            { '\\', KeyCode.Backslash },
            { ';', KeyCode.Semicolon },
            { '\'', KeyCode.Quote },
            { ',', KeyCode.Comma },
            { '.', KeyCode.Period },
            { '/', KeyCode.Slash }
        };

        private static readonly KeyCode[] SupportedKeys =
        {
            // Letters
            KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E,
            KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J,
            KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O,
            KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T,
            KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y,
            KeyCode.Z,

            // Numbers
            KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2,
            KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5,
            KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8,
            KeyCode.Alpha9,

            // Other
            KeyCode.Space,
            KeyCode.Return,
            KeyCode.Tab,
            KeyCode.Minus,
            KeyCode.Equals,
            KeyCode.LeftBracket,
            KeyCode.RightBracket,
            KeyCode.Backslash,
            KeyCode.Semicolon,
            KeyCode.Quote,
            KeyCode.Comma,
            KeyCode.Period,
            KeyCode.Slash
        };
        #endregion

        #endregion




        #region Main
        private void Awake()
        {
            foreach (var code in codes)
                data.Add(new InnerCodeData(ConvertStringToKeys(code.Code), code.Event));
        }


        private void Update()
        {
            foreach (KeyCode keyCode in SupportedKeys)
            {
                if (Input.GetKeyDown(keyCode))
                {
                    currentInput.Add(keyCode);

                    break;
                }
            }


            bool check = false;
            
            foreach (var code in data)
                if (code.Update(currentInput)) check = true;

            if (!check)
            {
                currentInput.Clear();

                return;
            }


            if (currentInput.Count == resetThreshold) currentInput.Clear();
        }
        #endregion



        #region Logic
        private List<KeyCode> ConvertStringToKeys(string str)
        {
            List<KeyCode> ret = new();

            foreach (char c in str.ToUpper())
            {
                if (c >= 'A' && c <= 'Z') ret.Add(KeyCode.A + (c - 'A'));
                else if (c >= '0' && c <= '9') ret.Add(KeyCode.Alpha0 + (c - '0'));
                else if (SpecialKeys.TryGetValue(c, out KeyCode key)) ret.Add(key);
            }

            return ret;
        }
        #endregion




        #region Data class
        [Serializable]
        private class CodeData
        {
            #region Variables
            public string Code;

            public UnityEvent Event;
            #endregion
        }



        private class InnerCodeData
        {
            #region Variables
            private List<KeyCode> code;

            private UnityEvent callback;
            #endregion



            #region Main
            public InnerCodeData(List<KeyCode> code, UnityEvent callback)
            {
                this.code = code;

                this.callback = callback;
            }

            public bool Update(List<KeyCode> current)
            {
                var count = current.Count;
                if (count == 0) return false;

                if (count < code.Count) return CheckEqualKeyCodes(current);
                else
                {
                    if (CheckExactEqualKeyCodes(current)) callback?.Invoke();

                    return false;
                }
            }
            #endregion


            #region Logic
            private bool CheckEqualKeyCodes(List<KeyCode> currentInput)
            {
                if (currentInput.Count > code.Count) return false;
                
                for (int i = 0; i < currentInput.Count; i++)
                    if (currentInput[i] != code[i]) return false;

                return true;
            }

            private bool CheckExactEqualKeyCodes(List<KeyCode> currentInput)
            {
                if (currentInput.Count != code.Count) return false;
                
                for (int i = 0; i < currentInput.Count; i++)
                    if (currentInput[i] != code[i]) return false;

                return true;
            }
            #endregion
        }
        #endregion 
    }
}
