using System;
using System.Linq;
using SHUU.UserSide;
using SHUU.Utils.Helpers;
using UnityEngine;
using UnityEngine.Events;

namespace SHUU.Utils.Developer.Console
{

    public class CommonDevCommands_DevConsole : MonoBehaviour
    {
        [Header("Player interaction")]
        public Transform playerTransform;
        public Transform cameraTransform;

        public UnityEvent activatePlayer;
        public UnityEvent deactivatePlayer;



        [Header("Misc")]
        public GameObject noclip_module;



        // Internal
        public static CommonDevCommands_DevConsole instance;



        // For commands
        private float noclipSpeed = 10f;


        private GameObject noclipInstance;




        private void Awake()
        {
            instance = this;


            noclipInstance = null;
        }



        #region Commands
        [DevConsoleCommand("help", "Lists all commands")]
        public static (string[], Color?) Help()
        {
            var cmds = DevCommandRegistry.AllCommands();
            if (cmds.Count() == 0) return (new string[] { "No commands registered." }, Color.red);



            string[] commandList = new string[cmds.Count()];
            
            int i = 0;
            foreach (var (name, info) in cmds)
            {
                var parameters = info.Method.GetParameters();

                string paramString = parameters.Length == 0
                    ? ""
                    : " (" + string.Join(", ", parameters.Select(p =>
                        $"{HandyFunctions.GetTypeName(p.ParameterType)}")) + ")";

                commandList[i] = $"{name}{paramString} - {info.Description}";
                i++;
            }

            return (commandList, null);
        }


        [DevConsoleCommand("noclip", "Toggles noclip mode")]
        public static (string[], Color?) _ToggleNoclip_Int(int intToggle)
        {
            bool toggle = intToggle >= 1 ? true : false;


            return ToggleNoclip(toggle);
        }
        [DevConsoleCommand("togglenoclip", "Toggles noclip mode")]
        public static (string[], Color?) _ToggleNoclip_Bool()
        {
            bool toggle = instance.noclipInstance == null ? true : false;


            return ToggleNoclip(toggle);
        }
        public static (string[], Color?) ToggleNoclip(bool toggle)
        {
            bool noclipOn = instance.noclipInstance != null ? true : false;



            if (noclipOn == toggle) return (new string[] { $"Noclip is already {(noclipOn ? "enabled" : "disabled")}" }, Color.red);



            if (instance.noclipInstance == null)
            {
                instance.deactivatePlayer.Invoke();


                instance.noclipInstance = Instantiate(instance.noclip_module, instance.playerTransform.position, instance.playerTransform.rotation);
                NoclipController noclipController = instance.noclipInstance.GetComponent<NoclipController>();

                noclipController.SetupCamera(instance.cameraTransform.rotation);
                noclipController.speed = instance.noclipSpeed;
                noclipController.playerTransform = instance.playerTransform;
                noclipController.cameraTransform = instance.cameraTransform;
            }
            else
            {
                Destroy(instance.noclipInstance);
                instance.noclipInstance = null;


                instance.activatePlayer.Invoke();
            }



            return (new string[] { $"Noclip {(noclipOn ? "enabled" : "disabled")}" }, null);
        }

        [DevConsoleCommand("noclipspeed", "Sets noclip speed")]
        public static (string[], Color?) SetNoclipSpeed(float speed)
        {
            instance.noclipSpeed = speed;



            return (new string[] { $"Noclip speed set to {speed}" }, null);
        }


        [DevConsoleCommand("tp", "Teleports the player to specified coordinates")]
        public static (string[], Color?) Teleport(float x, float y, float z)
        {
            if (instance.noclipInstance != null) instance.noclipInstance.transform.position = new Vector3(x, y, z);
            else instance.playerTransform.position = new Vector3(x, y, z);


            
            if (instance.noclipInstance != null) return (new string[] { $"NoclipModule teleported to ({x}, {y}, {z})" }, null);
            else return (new string[] { $"Player teleported to ({x}, {y}, {z})" }, null);
        }
        #endregion
    }

}
