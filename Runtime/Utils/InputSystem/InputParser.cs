using UnityEngine;

namespace SHUU.Utils.InputSystem
{

    public static class InputParser
    {
        public static (KeyCode?, int?) ParseInput(string input)
        {
            input = input.ToLower();


            if (input.StartsWith("mouse"))
            {
                if (int.TryParse(input.Substring(5), out int button) && button >= 0) return (null, button);
            }
            else if (System.Enum.TryParse(input, true, out KeyCode parsed)) return (parsed, null);


            Debug.LogWarning($"InputParser: Unknown input '{input}'");
            return (null, null);
        }

        
        
        public static bool Parse_GetInput(string input)
        {
            var (key, mouse) = ParseInput(input);

            if (key.HasValue) return Input.GetKey(key.Value);
            if (mouse.HasValue) return Input.GetMouseButton(mouse.Value);
            
            return false;
        }

        public static bool Parse_GetInputDown(string input)
        {
            var (key, mouse) = ParseInput(input);

            if (key.HasValue) return Input.GetKeyDown(key.Value);
            if (mouse.HasValue) return Input.GetMouseButtonDown(mouse.Value);
            
            return false;
        }

        public static bool Parse_GetInputUp(string input)
        {
            var (key, mouse) = ParseInput(input);

            if (key.HasValue) return Input.GetKeyUp(key.Value);
            if (mouse.HasValue) return Input.GetMouseButtonUp(mouse.Value);
            
            return false;
        }
    }

}
