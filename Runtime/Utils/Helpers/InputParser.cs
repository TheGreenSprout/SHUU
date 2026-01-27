using SHUU.Utils.InputSystem;
using UnityEngine;

namespace SHUU.Utils.Helpers
{
    public static class InputParser
    {
        public static (KeyCode?, int?, string) ParseInput(string input)
        {
            input = input.ToLower();


            if (input.StartsWith("mouse"))
            {
                if (int.TryParse(input.Substring(5), out int button) && button >= 0) return (null, button, null);
            }
            else if (input.StartsWith("axis_"))
            {
                return (null, null, input.Substring(5));
            }
            else
            {
                AxisNames? axis = ParseAxisEnum(input);
                if (axis != null) return (null, null, GetAxis(axis.Value));
                else if (System.Enum.TryParse(input, true, out KeyCode parsed)) return (parsed, null, null);
            }


            Debug.LogWarning($"InputParser: Unknown input '{input}'");
            return (null, null, null);
        }


        public static string InputToString(KeyCode key) => key.ToString();

        public static string InputToString(int mouse) => "Mouse" + mouse.ToString();

        public static string InputToString(string axis) => "Axis_" + axis;



        public enum AxisNames
        {
            Horizontal,
            Vertical,

            MouseX,
            MouseY,
            MouseScrollwheel,

            LeftJoystickX,
            LeftJoystickY,
            RightJoystickX,
            RightJoystickY,
            RightTrigger,
            LeftTrigger,
        }

        public static string GetAxis(AxisNames name)
        {
            return name switch
            {
                AxisNames.Horizontal => "Horizontal",
                AxisNames.Vertical => "Vertical",


                AxisNames.MouseX => "Mouse X",
                AxisNames.MouseY => "Mouse Y",
                AxisNames.MouseScrollwheel => "Mouse ScrollWheel",


                AxisNames.LeftJoystickX => "Joystick X",
                AxisNames.LeftJoystickY => "Joystick Y",

                AxisNames.RightJoystickX => "Joystick 3",
                AxisNames.RightJoystickY => "Joystick 4",

                AxisNames.LeftTrigger => "Joystick 6",
                AxisNames.RightTrigger => "Joystick 5",

                _ => null
            };
        }
        public static string GetAxis_WithEnumName(AxisNames name)
        {
            return name switch
            {
                AxisNames.Horizontal => "General_Horizontal",
                AxisNames.Vertical => "General_Vertical",


                AxisNames.MouseX => "KeyboardMouse_Mouse X",
                AxisNames.MouseY => "KeyboardMouse_Mouse Y",
                AxisNames.MouseScrollwheel => "KeyboardMouse_Mouse ScrollWheel",


                AxisNames.LeftJoystickX => "Gamepad_Joystick X",
                AxisNames.LeftJoystickY => "Gamepad_Joystick Y",

                AxisNames.RightJoystickX => "Gamepad_Joystick 3",
                AxisNames.RightJoystickY => "Gamepad_Joystick 4",

                AxisNames.LeftTrigger => "Gamepad_Joystick 6",
                AxisNames.RightTrigger => "Gamepad_Joystick 5",

                _ => null
            };
        }
        public static (string, InputDeviceType?) GetEnumNameAxisData(string axis)
        {
            if (axis.StartsWith("General_")) return (axis.Substring(8), InputDeviceType.General);
            else if (axis.StartsWith("KeyboardMouse_")) return (axis.Substring(14), InputDeviceType.KeyboardMouse);
            else if (axis.StartsWith("Gamepad_")) return (axis.Substring(8), InputDeviceType.Gamepad);
            else return (null, null);
        }

        public static AxisNames? ParseAxisEnum(string axis)
        {
            return axis switch
            {
                "Horizontal" => AxisNames.Horizontal,
                "Vertical" => AxisNames.Vertical,


                "Mouse X" => AxisNames.MouseX,
                "Mouse Y" => AxisNames.MouseY,
                "Mouse ScrollWheel" => AxisNames.MouseScrollwheel,


                "Joystick X" => AxisNames.LeftJoystickX,
                "Joystick Y" => AxisNames.LeftJoystickY,

                "Joystick 3" => AxisNames.RightJoystickX,
                "Joystick 4" => AxisNames.RightJoystickY,

                "Joystick 5" => AxisNames.LeftTrigger,
                "Joystick 6" => AxisNames.RightTrigger,

                _ => null
            };
        }

        
        
        /*public static bool Parse_GetInput(string input)
        {
            var (key, mouse, axis) = ParseInput(input);

            if (key != null) return Input.GetKey(key.Value);
            else if (mouse != null) return Input.GetMouseButton(mouse.Value);
            else if (mouse != null) return Input.GetMouseButton(mouse.Value);
            
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
        }*/
    }
}
