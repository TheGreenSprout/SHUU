using UnityEngine;
using SHUU.Utils.Developer.Console;
using SHUU.Utils.InputSystem;

namespace SHUU.UserSide
{

    public class InputSystem_ToggleDevConsole : ToggleDevConsole
    {
        [SerializeField] private InputBindingMap map;

        [SerializeField] private string setName;




        private void Update()
        {
            if (SHUU_Input.GetInputDown(map, setName, true))
            {
                Toggle();
            }
        }
    }
    
}
