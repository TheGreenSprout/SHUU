using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using SHUU.Utils.Globals;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.Utils.InputSystem
{
    [RequireComponent(typeof(PlayerInput))]
    public class SHUU_PlayerInput : MonoBehaviour
    {
        #region Static Registry
        private static readonly Dictionary<int, SHUU_PlayerInput> Registry = new();

        public static IReadOnlyDictionary<int, SHUU_PlayerInput> All => Registry;
        public static int Count => Registry.Count;

        public static event Action<SHUU_PlayerInput> OnPlayerJoined;
        public static event Action<SHUU_PlayerInput> OnPlayerLeft;


        public static bool TryGetPlayer(int playerIndex, out SHUU_PlayerInput player) => Registry.TryGetValue(playerIndex, out player);
        public static SHUU_PlayerInput GetPlayer(int playerIndex)
        {
            TryGetPlayer(playerIndex, out SHUU_PlayerInput player);
            return player;
        }
        #endregion



        #region Variables
        private PlayerInput playerInput;


        private Dictionary<string, InputAction> actionCache = new();
        private Dictionary<string, ActionHooks> hooks = new();

        private const float defaultBufferTime = 0.15f;



        public int playerIndex => playerInput != null ? playerInput.playerIndex : -1;

        public InputDevice device => playerInput != null && playerInput.devices.Count > 0 ? playerInput.devices[0] : null;
        public string controlScheme => playerInput != null ? playerInput.currentControlScheme : null;

        public bool isGamepad => controlScheme != null && controlScheme.Contains("Gamepad", StringComparison.OrdinalIgnoreCase);
        public bool isKeyboardMouse => controlScheme != null && controlScheme.Contains("Keyboard", StringComparison.OrdinalIgnoreCase);


        private bool _gamepadRumble = false;
        public bool gamepadRumble => isGamepad && _gamepadRumble;



        #region Map Context Stack
        private readonly Stack<string> contextStack = new();

        public string currentContext => contextStack.Count > 0 ? contextStack.Peek() : null;
        public int contextDepth => contextStack.Count;
        #endregion


        #region Device events
        public event Action<SHUU_PlayerInput> onControlsChanged;
        public event Action<SHUU_PlayerInput> onDeviceLost;
        public event Action<SHUU_PlayerInput> onDeviceRegained;
        #endregion

        #endregion




        #region Main
        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();

            if (playerInput == null)
            {
                Debug.LogError($"SHUU_PlayerInput on '{gameObject.name}': No PlayerInput component found on the same GameObject.");
                return;
            }

            BuildCache();
        }


        private void OnEnable()
        {
            if (playerInput == null) return;


            Registry[playerIndex] = this;

            playerInput.onActionTriggered += OnActionTriggered;
            playerInput.onControlsChanged += OnControlsChanged;
            playerInput.onDeviceLost += OnDeviceLost;
            playerInput.onDeviceRegained += OnDeviceRegained;

            OnPlayerJoined?.Invoke(this);
        }

        private void OnDisable()
        {
            if (playerInput == null) return;


            Registry.Remove(playerIndex);

            playerInput.onActionTriggered -= OnActionTriggered;
            playerInput.onControlsChanged -= OnControlsChanged;
            playerInput.onDeviceLost -= OnDeviceLost;
            playerInput.onDeviceRegained -= OnDeviceRegained;

            CleanupAllHooks();

            StopRumble();

            OnPlayerLeft?.Invoke(this);
        }


        private void Update()
        {
            foreach (var hook in hooks.Values)
                hook.Tick();
        }


        #region Device Events
        private void OnControlsChanged(PlayerInput _) => onControlsChanged?.Invoke(this);
        private void OnDeviceLost(PlayerInput _) => onDeviceLost?.Invoke(this);
        private void OnDeviceRegained(PlayerInput _) => onDeviceRegained?.Invoke(this);


        private void OnActionTriggered(InputAction.CallbackContext ctx)
        {
            string path = $"{ctx.action.actionMap.name}/{ctx.action.name}";

            if (!hooks.TryGetValue(path, out var hook)) return;

            if (ctx.phase == InputActionPhase.Performed)
            {
                hook.downBuffer?.ResetBuffer();

                foreach (var listener in hook.downListeners)
                    listener?.Invoke();
            }
            else if (ctx.phase == InputActionPhase.Canceled)
            {
                hook.upBuffer?.ResetBuffer();

                foreach (var listener in hook.upListeners)
                    listener?.Invoke();
            }
        }
        #endregion

        #endregion



        #region Logic

        #region Cache
        public void BuildCache()
        {
            actionCache.Clear();

            if (playerInput?.actions == null) return;

            foreach (var map in playerInput.actions.actionMaps)
                foreach (var action in map.actions)
                    actionCache[$"{map.name}/{action.name}"] = action;
        }


        public InputActionMap GetMap(string mapName)
        {
            if (string.IsNullOrEmpty(mapName)) return null;

            InputActionMap found = playerInput?.actions?.FindActionMap(mapName);

            if (found == null) Debug.LogWarning($"SHUU_PlayerInput [{playerIndex}]: ActionMap '{mapName}' not found.");

            return found;
        }

        public InputAction GetAction(string actionPath)
        {
            if (string.IsNullOrEmpty(actionPath)) return null;

            if (actionCache.TryGetValue(actionPath, out var cached)) return cached;

            InputAction found = playerInput?.actions?.FindAction(actionPath);

            if (found != null)
            {
                actionCache[actionPath] = found;
                return found;
            }

            Debug.LogWarning($"SHUU_PlayerInput [{playerIndex}]: Action '{actionPath}' not found.");

            return null;
        }
        #endregion



        #region Map Control
        public void EnableMap(string mapName) => GetMap(mapName)?.Enable();
        public void DisableMap(string mapName) => GetMap(mapName)?.Disable();

        public bool IsMapEnabled(string mapName) => GetMap(mapName)?.enabled ?? false;


        public void EnableMaps(params string[] mapNames)
        {
            if (mapNames == null) return;

            foreach (var mapName in mapNames)
                EnableMap(mapName);
        }

        public void DisableMaps(params string[] mapNames)
        {
            if (mapNames == null) return;

            foreach (var mapName in mapNames)
                DisableMap(mapName);
        }


        public void EnableMapsDisableRest(params string[] mapNames)
        {
            if (playerInput?.actions == null) return;

            var keep = new HashSet<string>(mapNames ?? Array.Empty<string>(), StringComparer.Ordinal);

            foreach (var map in playerInput.actions.actionMaps)
            {
                if (keep.Contains(map.name)) map.Enable();
                else map.Disable();
            }
        }

        public void DisableMapsEnableRest(params string[] mapNames)
        {
            if (playerInput?.actions == null) return;

            var keep = new HashSet<string>(mapNames ?? Array.Empty<string>(), StringComparer.Ordinal);

            foreach (var map in playerInput.actions.actionMaps)
            {
                if (keep.Contains(map.name)) map.Disable();
                else map.Enable();
            }
        }
        #endregion



        #region Map Context Stack
        public void PushContext(string mapName)
        {
            if (string.IsNullOrEmpty(mapName)) return;

            if (contextStack.Count > 0) DisableMap(contextStack.Peek());

            contextStack.Push(mapName);

            EnableMap(mapName);
        }


        public string PopContext()
        {
            if (contextStack.Count == 0) return null;

            string popped = contextStack.Pop();
            DisableMap(popped);

            if (contextStack.Count > 0) EnableMap(contextStack.Peek());

            return popped;
        }

        public string PeekContext() => currentContext;


        public void ClearContext()
        {
            while (contextStack.Count > 0)
                DisableMap(contextStack.Pop());
        }


        public string PrintContextStack()
        {
            if (contextStack.Count == 0) return "[]";

            string stack = string.Join(", ", contextStack.ToArray());
            return $"[{stack}]";
        }
        #endregion



        #region Input Retrieval
        public bool GetInput(string actionPath)
        {
            InputAction action = GetAction(actionPath);
            if (action == null) return false;

            return action.IsPressed();
        }

        public bool GetInputDown(string actionPath)
        {
            InputAction action = GetAction(actionPath);
            if (action == null) return false;

            return action.WasPressedThisFrame();
        }

        public bool GetInputUp(string actionPath)
        {
            InputAction action = GetAction(actionPath);
            if (action == null) return false;

            return action.WasReleasedThisFrame();
        }


        public T GetInputValue<T>(string actionPath) where T : struct
        {
            InputAction action = GetAction(actionPath);
            if (action == null) return default;

            return action.ReadValue<T>();
        }

        public float GetInputValue(string actionPath) => GetInputValue<float>(actionPath);
        public Vector2 GetInputValue2D(string actionPath) => GetInputValue<Vector2>(actionPath);
        #endregion



        #region Buffered Inputs
        public void RegisterBufferInput_Down(string actionPath, float bufferTime = defaultBufferTime, bool ignoreTimeScale = false)
        {
            if (string.IsNullOrEmpty(actionPath) || bufferTime <= 0f) return;

            ActionHooks hook = GetOrCreateHooks(actionPath);
            if (hook == null) return;

            if (hook.downBuffer == null) hook.downBuffer = new BufferedInput(bufferTime, ignoreTimeScale);
        }

        public void UnregisterBufferInput_Down(string actionPath)
        {
            if (!hooks.TryGetValue(actionPath, out var hook)) return;

            hook.downBuffer = null;

            CleanupIfEmpty(actionPath, hook);
        }


        public void RegisterBufferInput_Up(string actionPath, float bufferTime = defaultBufferTime, bool ignoreTimeScale = false)
        {
            if (string.IsNullOrEmpty(actionPath) || bufferTime <= 0f) return;

            ActionHooks hook = GetOrCreateHooks(actionPath);
            if (hook == null) return;

            if (hook.upBuffer == null) hook.upBuffer = new BufferedInput(bufferTime, ignoreTimeScale);
        }

        public void UnregisterBufferInput_Up(string actionPath)
        {
            if (!hooks.TryGetValue(actionPath, out var hook)) return;

            hook.upBuffer = null;

            CleanupIfEmpty(actionPath, hook);
        }


        public bool GetBufferedInput_Down(string actionPath, bool consume = true)
        {
            if (!hooks.TryGetValue(actionPath, out var hook)) return false;

            BufferedInput buffer = hook.downBuffer;
            if (buffer == null || !buffer.isActive) return false;

            if (consume) buffer.Consume();

            return true;
        }

        public bool GetBufferedInput_Up(string actionPath, bool consume = true)
        {
            if (!hooks.TryGetValue(actionPath, out var hook)) return false;

            BufferedInput buffer = hook.upBuffer;
            if (buffer == null || !buffer.isActive) return false;

            if (consume) buffer.Consume();

            return true;
        }
        #endregion



        #region Listeners
        public void RegisterListener_Down(string actionPath, Action callback)
        {
            if (string.IsNullOrEmpty(actionPath) || callback == null) return;

            ActionHooks hook = GetOrCreateHooks(actionPath);
            if (hook == null) return;

            if (hook.downListeners.Contains(callback)) return;

            hook.downListeners.Add(callback);
        }

        public void UnregisterListener_Down(string actionPath, Action callback)
        {
            if (!hooks.TryGetValue(actionPath, out var hook)) return;

            hook.downListeners.Remove(callback);

            CleanupIfEmpty(actionPath, hook);
        }

        public void UnregisterListener_Down(string actionPath)
        {
            if (!hooks.TryGetValue(actionPath, out var hook)) return;

            hook.downListeners.Clear();

            CleanupIfEmpty(actionPath, hook);
        }


        public void RegisterListener_Up(string actionPath, Action callback)
        {
            if (string.IsNullOrEmpty(actionPath) || callback == null) return;

            ActionHooks hook = GetOrCreateHooks(actionPath);
            if (hook == null) return;

            if (hook.upListeners.Contains(callback)) return;

            hook.upListeners.Add(callback);
        }

        public void UnregisterListener_Up(string actionPath, Action callback)
        {
            if (!hooks.TryGetValue(actionPath, out var hook)) return;

            hook.upListeners.Remove(callback);

            CleanupIfEmpty(actionPath, hook);
        }

        public void UnregisterListener_Up(string actionPath)
        {
            if (!hooks.TryGetValue(actionPath, out var hook)) return;

            hook.upListeners.Clear();

            CleanupIfEmpty(actionPath, hook);
        }
        #endregion



        #region Hooks
        private ActionHooks GetOrCreateHooks(string actionPath)
        {
            if (hooks.TryGetValue(actionPath, out var existing)) return existing;

            InputAction action = GetAction(actionPath);
            if (action == null) return null;

            var hook = new ActionHooks(action);
            hooks[actionPath] = hook;

            return hook;
        }


        private void CleanupIfEmpty(string actionPath, ActionHooks hook)
        {
            if (hook.hasAnyHooks) return;

            hooks.Remove(actionPath);
        }

        private void CleanupAllHooks() => hooks.Clear();
        #endregion



        /*
        ⚠️‼️ AI ASSISTED SNIPPET

        This snippet was written with the assistance of AI.
        */
        #region Rebinding & Defaults
        public string SaveOverrides() => playerInput?.actions?.SaveBindingOverridesAsJson();

        public void LoadOverrides(string json)
        {
            if (playerInput?.actions == null || string.IsNullOrEmpty(json)) return;

            playerInput.actions.LoadBindingOverridesFromJson(json);
        }

        public void ResetToDefaults(string mapName = null)
        {
            if (playerInput?.actions == null) return;

            if (string.IsNullOrEmpty(mapName))
            {
                playerInput.actions.RemoveAllBindingOverrides();
                return;
            }

            GetMap(mapName)?.RemoveAllBindingOverrides();
        }


        public string FindBindingConflict(string actionPath, string candidateControlPath, string bindingGroup = null)
        {
            InputAction action = GetAction(actionPath);
            if (action == null || string.IsNullOrEmpty(candidateControlPath)) return null;

            InputActionMap map = action.actionMap;
            if (map == null) return null;

            InputControl candidateControl = UnityEngine.InputSystem.InputSystem.FindControl(candidateControlPath);
            if (candidateControl == null) return null;


            foreach (var otherAction in map.actions)
            {
                if (otherAction == action) continue;

                foreach (var binding in otherAction.bindings)
                {
                    if (binding.isComposite || binding.isPartOfComposite) continue;
                    if (!string.IsNullOrEmpty(bindingGroup) && !string.IsNullOrEmpty(binding.groups) && !binding.groups.Contains(bindingGroup)) continue;

                    string boundPath = string.IsNullOrEmpty(binding.overridePath) ? binding.path : binding.overridePath;
                    if (string.IsNullOrEmpty(boundPath)) continue;

                    InputControl existingControl = UnityEngine.InputSystem.InputSystem.FindControl(boundPath);

                    if (existingControl == candidateControl) return otherAction.name;
                }
            }

            return null;
        }


        public int GetCompositePartBindingIndex(string actionPath, CompositeBindPart part) => GetCompositePartBindingIndex(actionPath, GetEnumName(part));
        public int GetCompositePartBindingIndex(string actionPath, string partName)
        {
            InputAction action = GetAction(actionPath);
            if (action == null || string.IsNullOrEmpty(partName)) return -1;

            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (binding.isPartOfComposite && string.Equals(binding.name, partName, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }


        public InputActionRebindingExtensions.RebindingOperation StartInteractiveRebind(string actionPath, int bindingIndex, Action onComplete = null, Action onCancel = null, bool filterToCurrentScheme = true, Action<string> onConflict = null, bool blockOnConflict = false)
        {
            InputAction action = GetAction(actionPath);
            if (action == null) return null;

            action.Disable();

            var rebind = action.PerformInteractiveRebinding(bindingIndex);

            string schemeFilter = null;

            if (filterToCurrentScheme && playerInput?.actions != null && !string.IsNullOrEmpty(controlScheme))
            {
                schemeFilter = controlScheme;

                InputControlScheme? scheme = playerInput.actions.FindControlScheme(controlScheme);

                if (scheme.HasValue)
                {
                    foreach (var deviceRequirement in scheme.Value.deviceRequirements)
                        rebind.WithControlsHavingToMatchPath(deviceRequirement.controlPath);
                }
            }

            if (onConflict != null || blockOnConflict)
            {
                rebind.OnPotentialMatch(op =>
                {
                    var candidate = op.selectedControl;
                    if (candidate == null) return;

                    string conflictingAction = FindBindingConflict(actionPath, candidate.path, schemeFilter);

                    if (conflictingAction != null)
                    {
                        onConflict?.Invoke(conflictingAction);

                        if (blockOnConflict) return;
                    }

                    op.Complete();
                });
            }

            rebind.OnComplete(op =>
                {
                    op.Dispose();
                    action.Enable();
                    onComplete?.Invoke();
                })
                .OnCancel(op =>
                {
                    op.Dispose();
                    action.Enable();
                    onCancel?.Invoke();
                })
                .Start();

            return rebind;
        }
        #endregion


        
        /*
        ⚠️‼️ AI ASSISTED SNIPPET

        This snippet was written with the assistance of AI.
        */
        #region Display Strings
        public string GetBindingDisplayString(string actionPath, int bindingIndex)
        {
            InputAction action = GetAction(actionPath);
            if (action == null) return string.Empty;

            return action.GetBindingDisplayString(bindingIndex);
        }


        public string GetBindingDisplayString(string actionPath)
        {
            InputAction action = GetAction(actionPath);
            if (action == null || playerInput?.actions == null || string.IsNullOrEmpty(controlScheme)) return string.Empty;

            InputControlScheme? scheme = playerInput.actions.FindControlScheme(controlScheme);
            if (!scheme.HasValue) return string.Empty;

            var mask = InputBinding.MaskByGroup(scheme.Value.bindingGroup);

            return action.GetBindingDisplayString(mask);
        }


        public string GetBindingDisplayStringInScheme(string actionPath, int schemeBindingIndex)
        {
            List<string> All = GetBindingDisplayStrings(actionPath, controlScheme);
            if (All == null || schemeBindingIndex < 0 || schemeBindingIndex >= All.Count) return string.Empty;

            return All[schemeBindingIndex];
        }


        public string GetBindingDisplayString(string actionPath, string schemeName)
        {
            InputAction action = GetAction(actionPath);
            if (action == null || playerInput?.actions == null) return string.Empty;

            InputControlScheme? scheme = playerInput.actions.FindControlScheme(schemeName);
            if (!scheme.HasValue) return string.Empty;

            var mask = InputBinding.MaskByGroup(scheme.Value.bindingGroup);

            return action.GetBindingDisplayString(mask);
        }


        public string GetBindingDisplayString(string actionPath, string schemeName, int schemeBindingIndex)
        {
            List<string> All = GetBindingDisplayStrings(actionPath, schemeName);
            if (All == null || schemeBindingIndex < 0 || schemeBindingIndex >= All.Count) return string.Empty;

            return All[schemeBindingIndex];
        }


        public List<string> GetBindingDisplayStrings(string actionPath, string schemeName)
        {
            InputAction action = GetAction(actionPath);
            if (action == null || playerInput?.actions == null || string.IsNullOrEmpty(schemeName)) return null;

            InputControlScheme? scheme = playerInput.actions.FindControlScheme(schemeName);
            if (!scheme.HasValue) return null;

            var result = new List<string>();
            var mask = InputBinding.MaskByGroup(scheme.Value.bindingGroup);

            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action.bindings[i].isComposite) continue;
                if (!mask.Matches(action.bindings[i])) continue;

                result.Add(action.GetBindingDisplayString(i));
            }

            return result;
        }


        public Dictionary<int, string> GetAllBindingDisplayStrings(string actionPath)
        {
            InputAction action = GetAction(actionPath);
            if (action == null) return null;

            var result = new Dictionary<int, string>();

            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action.bindings[i].isComposite) continue;

                result[i] = action.GetBindingDisplayString(i);
            }

            return result;
        }
        #endregion



        #region Haptics
        public void SetRumble(float lowFrequency, float highFrequency)
        {
            if (device is Gamepad gamepad)
            {
                gamepad.SetMotorSpeeds(Mathf.Clamp01(lowFrequency), Mathf.Clamp01(highFrequency));
                _gamepadRumble = true;
            }
        }

        public void StopRumble()
        {
            if (device is Gamepad gamepad)
            {
                gamepad.SetMotorSpeeds(0f, 0f);
                _gamepadRumble = false;
            }
        }


        public SHUU_Timer RumbleTimer(float lowFrequency, float highFrequency, float duration)
        {
            SetRumble(lowFrequency, highFrequency);

            return SHUU_Time.Timer(duration, StopRumble);
        }

        public Coroutine RumbleFor(float lowFrequency, float highFrequency, float duration) => StartCoroutine(RumbleRoutine(lowFrequency, highFrequency, duration));
        public System.Collections.IEnumerator RumbleRoutine(float lowFrequency, float highFrequency, float duration)
        {
            SetRumble(lowFrequency, highFrequency);

            yield return new WaitForSeconds(duration);

            StopRumble();
        }
        #endregion

        #endregion
    }
}