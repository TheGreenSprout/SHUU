using System;
using SHUU.Utils.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

/*
⚠️ Script requires something to be done:
    - Go to [Project Settings > Input System Package]
    - Assing an "Input Action Asset" to "Project-wide Actions"


Terminology:
    + iv_ : Input Value (Vector2, float...)
    + it_ : Input Toggle (boolean)
    + icb_ : Input Callback [Pressed/Released] (Action)
    + iref_ : Input Reference (InputAction)
*/

public class CustomInputManager : SceneSensitiveScript
{
    public static CustomInputManager InputManager_Instance { get; private set; }


    public InputActionAsset input;



    #region <AUTO_GENERATED_INPUT_FIELDS>
        /* Auto-generated Input fields.
        This block is managed by the Input Action Sync tool.
        DO NOT EDIT MANUALLY — changes may impact the tool's functionality.*/

    // Player Map
        // Values
        public Vector2 iv_Player_Movement { get; private set; }

        // States
        public static bool it_Player_MovementDown { get; private set; }
        public static bool it_Player_SprintDown { get; private set; }

        // Events
        public static event Action icb_Player_SprintPressedAction;
        public static event Action icb_Player_SprintReleasedAction;
        public static event Action icb_Player_JumpPressedAction;

        // References
        private InputAction iref_Player_MovementInputAction;
        private InputAction iref_Player_SprintInputAction;
        private InputAction iref_Player_JumpInputAction;


    // Actions Map
        // States
        public static bool it_Actions_InteractDown { get; private set; }

        // Events
        public static event Action icb_Actions_PausePressedAction;
        public static event Action icb_Actions_InteractPressedAction;

        // References
        private InputAction iref_Actions_PauseInputAction;
        private InputAction iref_Actions_InteractInputAction;

    #endregion // </AUTO_GENERATED_INPUT_FIELDS>




    private void Awake()
    {
        if (InputManager_Instance != null && InputManager_Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        InputManager_Instance = this;

        transform.parent = null;

        DontDestroyOnLoad(gameObject);



        #region <AUTO_GENERATED_INPUT_AWAKE>
            /* Auto-generated Awake bindings.
            This block is managed by the Input Action Sync tool.
            DO NOT EDIT MANUALLY — changes may impact the tool's functionality.*/

        // Player Map
            iref_Player_MovementInputAction = InputSystem.actions.FindAction("Movement");
            iref_Player_SprintInputAction = InputSystem.actions.FindAction("Sprint");
            iref_Player_JumpInputAction = InputSystem.actions.FindAction("Jump");


        // Actions Map
            iref_Actions_PauseInputAction = InputSystem.actions.FindAction("Pause");
            iref_Actions_InteractInputAction = InputSystem.actions.FindAction("Interact");

        #endregion // </AUTO_GENERATED_INPUT_AWAKE>
    }


    private void OnEnable()
    {
        #region <AUTO_GENERATED_INPUT_ONENABLE>
            /* Auto-generated OnEnable: enable all action maps.
            This block is managed by the Input Action Sync tool.
            DO NOT EDIT MANUALLY — changes may impact the tool's functionality.*/

        // Player Map
            input.FindActionMap("Player").Enable();

        // Actions Map
            input.FindActionMap("Actions").Enable();

        #endregion // </AUTO_GENERATED_INPUT_ONENABLE>
    }

    private void OnDisable()
    {
        #region <AUTO_GENERATED_INPUT_ONDISABLE>
            /* Auto-generated OnDisable: disable all action maps.
            This block is managed by the Input Action Sync tool.
            DO NOT EDIT MANUALLY — changes may impact the tool's functionality.*/

        // Player Map
            input.FindActionMap("Player").Disable();

        // Actions Map
            input.FindActionMap("Actions").Disable();

        #endregion // </AUTO_GENERATED_INPUT_ONDISABLE>
    }



    #region <AUTO_GENERATED_INPUT_CALLBACK_HANDLING>
        /* Auto-generated Callback Handling: easy way to add and remove methods to the "icb_"s.
        This block is managed by the Input Action Sync tool.
        DO NOT EDIT MANUALLY — changes may impact the tool's functionality.*/

    // Player Map
        public static void AddSprintPressedCallback(Action callback)
        {
            icb_Player_SprintPressedAction += callback;
        }
        public static void RemoveSprintPressedCallback(Action callback)
        {
            icb_Player_SprintPressedAction -= callback;
        }

        public static void AddSprintReleasedCallback(Action callback)
        {
            icb_Player_SprintReleasedAction += callback;
        }
        public static void RemoveSprintReleasedCallback(Action callback)
        {
            icb_Player_SprintReleasedAction -= callback;
        }


        public static void AddJumpPressedCallback(Action callback)
        {
            icb_Player_JumpPressedAction += callback;
        }
        public static void RemoveJumpPressedCallback(Action callback)
        {
            icb_Player_JumpPressedAction -= callback;
        }



    // Actions Map
        public static void AddPausePressedCallback(Action callback)
        {
            icb_Actions_PausePressedAction += callback;
        }
        public static void RemovePausePressedCallback(Action callback)
        {
            icb_Actions_PausePressedAction -= callback;
        }


        public static void AddInteractPressedCallback(Action callback)
        {
            icb_Actions_InteractPressedAction += callback;
        }
        public static void RemoveInteractPressedCallback(Action callback)
        {
            icb_Actions_InteractPressedAction -= callback;
        }

    #endregion // </AUTO_GENERATED_INPUT_CALLBACK_HANDLING>



    private void Update()
    {
        #region <AUTO_GENERATED_INPUT_UPDATE>
            /* Auto-generated Update input handling.
            This block is managed by the Input Action Sync tool.
            DO NOT EDIT MANUALLY — changes may impact the tool's functionality.*/

        // Player Map
            // Values
            iv_Player_Movement = iref_Player_MovementInputAction.ReadValue<Vector2>();

            // States
            it_Player_MovementDown = iref_Player_MovementInputAction.IsPressed();
            it_Player_SprintDown = iref_Player_SprintInputAction.IsPressed();

            // Events
            if (iref_Player_SprintInputAction.WasPressedThisFrame())
            {
                icb_Player_SprintPressedAction?.Invoke();
            }

            if (iref_Player_SprintInputAction.WasReleasedThisFrame())
            {
                icb_Player_SprintReleasedAction?.Invoke();
            }

            if (iref_Player_JumpInputAction.WasPressedThisFrame())
            {
                icb_Player_JumpPressedAction?.Invoke();
            }



        // Actions Map
            // States
            it_Actions_InteractDown = iref_Actions_InteractInputAction.IsPressed();
            // Events
            if (iref_Actions_PauseInputAction.WasPressedThisFrame())
            {
                icb_Actions_PausePressedAction?.Invoke();
            }

            if (iref_Actions_InteractInputAction.WasPressedThisFrame())
            {
                icb_Actions_InteractPressedAction?.Invoke();
            }


        #endregion // </AUTO_GENERATED_INPUT_UPDATE>
    }
}




#region <AUTO_GENERATED_INPUT_SUMMARY>
    /* Auto-generated Summary: lists all generated inputs and their parts.
    This block is managed by the Input Action Sync tool.
    DO NOT EDIT MANUALLY — changes may impact the tool's functionality.*/

/*
Player/Movement → Variable: Movement | Parts: Value, Bool
Player/Sprint → Variable: Sprint | Parts: Bool, Pressed, Released
Player/Jump → Variable: Jump | Parts: Pressed
Actions/Pause → Variable: Pause | Parts: Pressed
Actions/Interact → Variable: Interact | Parts: Bool, Pressed
*/

#endregion // </AUTO_GENERATED_INPUT_SUMMARY>