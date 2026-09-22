using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

using SHUU.Utils.Globals;
using SHUU.Utils.Helpers;
using SHUU.InnerWorkings.Preferences;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.Utils.InputSystem
{
    public static class SHUU_Input
    {
        #region Variables
        public static InputActionAsset InputActionAsset => SHUUPreferences_InputSystem.Instance.actionAsset ?? UnityEngine.InputSystem.InputSystem.actions;


        private static DualDictionary<int, Gamepad, bool> GamepadRumble = new();

        public static bool IsGamepadRumbling(int deviceID)
        {
            if (GamepadRumble.TryGetValue(deviceID, out bool ret)) return ret;
            else return false;
        }
        public static bool IsGamepadRumbling(Gamepad gamepad)
        {
            if (GamepadRumble.TryGetValue(gamepad, out bool ret)) return ret;
            else return false;
        }
        public static bool IsGamepadRumbling(int deviceID, Gamepad gamepad)
        {
            if (GamepadRumble.TryGetValue(deviceID, gamepad, out bool ret)) return ret;
            else return false;
        }



        private static Dictionary<string, InputActionMap> MapCache = new();
        private static Dictionary<string, InputAction> ActionCache = new();

        private static Dictionary<string, ActionHooks> Hooks = new();


        private const float defaultBufferTime = 0.15f;



        private static bool DebugLogEmission => SHUUPreferences_InputSystem.Instance != null && SHUUPreferences_InputSystem.Instance.debugLogEmission;
        private static bool DisabledWarning_debugLogEmission => SHUUPreferences_InputSystem.Instance != null && SHUUPreferences_InputSystem.Instance.mapDisabledWarning_debugLogEmission;



        #region Map Context Stack
        private static readonly Stack<string> ContextStack = new();

        public static string CurrentContext => ContextStack.Count > 0 ? ContextStack.Peek() : null;
        public static int ContextDepth => ContextStack.Count;
        #endregion

        #endregion




        #region Main
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            RebuildCache();

            if (InputActionAsset != null) InputActionAsset.Enable();
            else Debug.LogError("SHUU_Input: No InputActionAsset assigned in SHUU_Preferences nor InputActionAsset assigned as project-wide..");

            SHUU_Time.OnUpdate += Update;


            foreach (var pad in Gamepad.all)
                GamepadRumble[pad.deviceId, pad] = false;
            
            UnityEngine.InputSystem.InputSystem.onDeviceChange += (device, change) =>
            {
                if (device is not Gamepad pad) return;

                if (change == InputDeviceChange.Added) GamepadRumble[pad.deviceId, pad] = false;
                else if (change == InputDeviceChange.Removed) GamepadRumble.Remove(pad.deviceId, pad);
            };
        }


        private static void Update()
        {
            foreach (var hook in Hooks.Values)
                hook.Tick();
        }


        public static IEnumerable<string> GetAllMapNames()
            => InputActionAsset != null ? InputActionAsset.actionMaps.Select(m => m.name) : Enumerable.Empty<string>();
        #endregion



        #region Logic

        #region Cache
        public static void RebuildCache()
        {
            MapCache.Clear();
            ActionCache.Clear();

            if (InputActionAsset == null) return;

            foreach (var map in InputActionAsset.actionMaps)
            {
                MapCache[map.name] = map;

                foreach (var action in map.actions)
                    ActionCache[$"{map.name}/{action.name}"] = action;
            }
        }


        public static InputActionMap GetMap(string mapName)
        {
            if (string.IsNullOrEmpty(mapName)) return null;


            if (MapCache.TryGetValue(mapName, out var cached)) return cached;

            InputActionMap found = InputActionAsset?.FindActionMap(mapName);

            if (found != null)
            {
                MapCache[mapName] = found;

                return found;
            }


            if (DebugLogEmission) Debug.LogWarning($"SHUU_Input: ActionMap '{mapName}' not found.");

            return null;
        }
        
        public static InputAction GetAction(string actionPath)
        {
            if (string.IsNullOrEmpty(actionPath)) return null;


            if (ActionCache.TryGetValue(actionPath, out var cached)) return cached;

            InputAction found = InputActionAsset?.FindAction(actionPath);

            if (found != null)
            {
                ActionCache[actionPath] = found;

                return found;
            }


            if (DebugLogEmission) Debug.LogWarning($"SHUU_Input: Action '{actionPath}' not found.");

            return null;
        }
        #endregion



        #region Map Control
        public static void EnableMap(string mapName) => GetMap(mapName)?.Enable();
        public static void DisableMap(string mapName) => GetMap(mapName)?.Disable();

        public static bool IsMapEnabled(string mapName) => GetMap(mapName)?.enabled ?? false;


        public static void EnableMaps(params string[] mapNames)
        {
            if (mapNames == null) return;

            foreach (var mapName in mapNames)
                EnableMap(mapName);
        }

        public static void DisableMaps(params string[] mapNames)
        {
            if (mapNames == null) return;

            foreach (var mapName in mapNames)
                DisableMap(mapName);
        }


        public static void EnableMapsDisableRest(params string[] mapNames)
        {
            if (InputActionAsset == null) return;


            var keep = new HashSet<string>(mapNames ?? Array.Empty<string>(), StringComparer.Ordinal);

            foreach (var map in InputActionAsset.actionMaps)
            {
                if (keep.Contains(map.name)) map.Enable();
                else map.Disable();
            }
        }

        public static void DisableMapsEnableRest(params string[] mapNames)
        {
            if (InputActionAsset == null) return;


            var keep = new HashSet<string>(mapNames ?? Array.Empty<string>(), StringComparer.Ordinal);

            foreach (var map in InputActionAsset.actionMaps)
            {
                if (keep.Contains(map.name)) map.Disable();
                else map.Enable();
            }
        }
        #endregion



        #region Map Context Stack
        public static void PushContext(string mapName)
        {
            if (string.IsNullOrEmpty(mapName)) return;


            if (ContextStack.Count > 0) DisableMap(ContextStack.Peek());

            ContextStack.Push(mapName);

            EnableMap(mapName);
        }


        public static string PopContext()
        {
            if (ContextStack.Count == 0) return null;


            string popped = ContextStack.Pop();
            DisableMap(popped);

            if (ContextStack.Count > 0) EnableMap(ContextStack.Peek());

            return popped;
        }

        public static string PeekContext() => CurrentContext;


        public static void ClearContext()
        {
            while (ContextStack.Count > 0)
                DisableMap(ContextStack.Pop());
        }


        public static string PrintContextStack()
        {
            if (ContextStack.Count == 0) return "[]";

            string stack = string.Join(", ", ContextStack.ToArray());
            return $"[{stack}]";
        }
        #endregion



        #region Input Retrieval
        public static bool GetInput(string actionPath)
        {
            InputAction action = GetAction(actionPath);
            if (action == null) return false;

            if (!action.enabled)
            {
                if (DisabledWarning_debugLogEmission) Debug.LogWarning($"SHUU_Input: Action '{actionPath}' not enabled.");

                return false;
            }

            return action.IsPressed();
        }

        public static bool GetInputDown(string actionPath)
        {
            InputAction action = GetAction(actionPath);
            if (action == null) return false;

            if (!action.enabled)
            {
                if (DisabledWarning_debugLogEmission) Debug.LogWarning($"SHUU_Input: Action '{actionPath}' not enabled.");

                return false;
            }

            return action.WasPressedThisFrame();
        }

        public static bool GetInputUp(string actionPath)
        {
            InputAction action = GetAction(actionPath);
            if (action == null) return false;

            if (!action.enabled)
            {
                if (DisabledWarning_debugLogEmission) Debug.LogWarning($"SHUU_Input: Action '{actionPath}' not enabled.");

                return false;
            }

            return action.WasReleasedThisFrame();
        }


        public static T GetInputValue<T>(string actionPath) where T : struct
        {
            InputAction action = GetAction(actionPath);
            if (action == null) return default;

            if (!action.enabled)
            {
                if (DisabledWarning_debugLogEmission) Debug.LogWarning($"SHUU_Input: Action '{actionPath}' not enabled.");

                return default;
            }


            return action.ReadValue<T>();
        }

        public static float GetInputValue(string actionPath) => GetInputValue<float>(actionPath);
        public static Vector2 GetInputValue2D(string actionPath) => GetInputValue<Vector2>(actionPath);
        #endregion



        #region Buffered Inputs
        public static void RegisterBufferInput_Down(string actionPath, float bufferTime = defaultBufferTime, bool ignoreTimeScale = false)
        {
            if (string.IsNullOrEmpty(actionPath) || bufferTime <= 0f) return;


            ActionHooks hook = GetOrCreateHooks(actionPath);
            if (hook == null) return;

            if (hook.downBuffer == null)
            {
                hook.downBuffer = new BufferedInput(bufferTime, ignoreTimeScale);
                hook.EnsureSubscribed();
            }
        }

        public static void UnregisterBufferInput_Down(string actionPath)
        {
            if (!Hooks.TryGetValue(actionPath, out var hook)) return;

            hook.downBuffer = null;

            CleanupIfEmpty(actionPath, hook);
        }


        public static void RegisterBufferInput_Up(string actionPath, float bufferTime = defaultBufferTime, bool ignoreTimeScale = false)
        {
            if (string.IsNullOrEmpty(actionPath) || bufferTime <= 0f) return;


            ActionHooks hook = GetOrCreateHooks(actionPath);
            if (hook == null) return;

            if (hook.upBuffer == null)
            {
                hook.upBuffer = new BufferedInput(bufferTime, ignoreTimeScale);
                hook.EnsureSubscribed();
            }
        }

        public static void UnregisterBufferInput_Up(string actionPath)
        {
            if (!Hooks.TryGetValue(actionPath, out var hook)) return;

            hook.upBuffer = null;

            CleanupIfEmpty(actionPath, hook);
        }


        public static bool GetBufferedInput_Down(string actionPath, bool consume = true)
        {
            if (!Hooks.TryGetValue(actionPath, out var hook)) return false;

            BufferedInput buffer = hook.downBuffer;
            if (buffer == null || !buffer.isActive) return false;

            if (consume) buffer.Consume();

            return true;
        }

        public static bool GetBufferedInput_Up(string actionPath, bool consume = true)
        {
            if (!Hooks.TryGetValue(actionPath, out var hook)) return false;

            BufferedInput buffer = hook.upBuffer;
            if (buffer == null || !buffer.isActive) return false;

            if (consume) buffer.Consume();

            return true;
        }
        #endregion



        #region Listeners
        public static void RegisterListener_Down(string actionPath, Action callback)
        {
            if (string.IsNullOrEmpty(actionPath) || callback == null) return;


            ActionHooks hook = GetOrCreateHooks(actionPath);
            if (hook == null) return;

            if (hook.downListeners.Contains(callback)) return;

            hook.downListeners.Add(callback);
            hook.EnsureSubscribed();
        }

        public static void UnregisterListener_Down(string actionPath, Action callback = null)
        {
            if (!Hooks.TryGetValue(actionPath, out var hook)) return;

            if (callback != null) hook.downListeners.Remove(callback);
            else hook.downListeners.Clear();

            CleanupIfEmpty(actionPath, hook);
        }


        public static void RegisterListener_Up(string actionPath, Action callback)
        {
            if (string.IsNullOrEmpty(actionPath) || callback == null) return;


            ActionHooks hook = GetOrCreateHooks(actionPath);
            if (hook == null) return;

            if (hook.upListeners.Contains(callback)) return;

            hook.upListeners.Add(callback);
            hook.EnsureSubscribed();
        }

        public static void UnregisterListener_Up(string actionPath, Action callback = null)
        {
            if (!Hooks.TryGetValue(actionPath, out var hook)) return;

            if (callback != null) hook.upListeners.Remove(callback);
            else hook.upListeners.Clear();

            CleanupIfEmpty(actionPath, hook);
        }
        #endregion



        #region Hooks
        private static ActionHooks GetOrCreateHooks(string actionPath)
        {
            if (Hooks.TryGetValue(actionPath, out var existing)) return existing;

            InputAction action = GetAction(actionPath);
            if (action == null) return null;

            var hook = new ActionHooks(action);
            Hooks[actionPath] = hook;

            return hook;
        }


        private static void CleanupIfEmpty(string actionPath, ActionHooks hook)
        {
            if (hook.hasAnyHooks) return;

            hook.Unsubscribe();
            Hooks.Remove(actionPath);
        }

        public static void CleanupAllHooks()
        {
            foreach (var hook in Hooks.Values)
                hook.Unsubscribe();

            Hooks.Clear();
        }
        #endregion



        /*
        ⚠️‼️ AI ASSISTED SNIPPET

        This snippet was written with the assistance of AI.
        */
        #region Rebinding & Defaults
        public static string SaveOverrides() => InputActionAsset?.SaveBindingOverridesAsJson();

        public static void LoadOverrides(string json)
        {
            if (InputActionAsset == null || string.IsNullOrEmpty(json)) return;

            InputActionAsset.LoadBindingOverridesFromJson(json);
        }


        public static void ResetToDefaults(string mapName = null)
        {
            if (string.IsNullOrEmpty(mapName))
            {
                InputActionAsset?.RemoveAllBindingOverrides();

                return;
            }

            GetMap(mapName)?.RemoveAllBindingOverrides();
        }


        public static string FindBindingConflict(string actionPath, string candidateControlPath, string bindingGroup = null)
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


        public static int GetCompositePartBindingIndex(string actionPath, CompositeBindPart part) => GetCompositePartBindingIndex(actionPath, GetEnumName(part));
        public static int GetCompositePartBindingIndex(string actionPath, string partName)
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


        public static InputActionRebindingExtensions.RebindingOperation StartInteractiveRebind(string actionPath, int bindingIndex, Action onComplete = null, Action onCancel = null, string schemeFilter = null, Action<string> onConflict = null, bool blockOnConflict = false)
        {
            InputAction action = GetAction(actionPath);
            if (action == null) return null;

            action.Disable();

            var rebind = action.PerformInteractiveRebinding(bindingIndex);

            if (!string.IsNullOrEmpty(schemeFilter) && InputActionAsset != null)
            {
                InputControlScheme? scheme = InputActionAsset.FindControlScheme(schemeFilter);

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
        public static string GetBindingDisplayString(string actionPath, int bindingIndex)
        {
            InputAction action = GetAction(actionPath);
            if (action == null) return string.Empty;

            return action.GetBindingDisplayString(bindingIndex);
        }


        public static string GetBindingDisplayString(string actionPath, string schemeName)
        {
            InputAction action = GetAction(actionPath);
            if (action == null || InputActionAsset == null) return string.Empty;

            InputControlScheme? scheme = InputActionAsset.FindControlScheme(schemeName);
            if (!scheme.HasValue) return string.Empty;

            var mask = InputBinding.MaskByGroup(scheme.Value.bindingGroup);

            return action.GetBindingDisplayString(mask);
        }


        public static string GetBindingDisplayString(string actionPath, string schemeName, int schemeBindingIndex)
        {
            List<string> all = GetBindingDisplayStrings(actionPath, schemeName);
            if (all == null || schemeBindingIndex < 0 || schemeBindingIndex >= all.Count) return string.Empty;

            return all[schemeBindingIndex];
        }


        public static List<string> GetBindingDisplayStrings(string actionPath, string schemeName)
        {
            InputAction action = GetAction(actionPath);
            if (action == null || InputActionAsset == null) return null;

            InputControlScheme? scheme = InputActionAsset.FindControlScheme(schemeName);
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


        public static Dictionary<int, string> GetAllBindingDisplayStrings(string actionPath)
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
        public static void SetRumble(float lowFrequency, float highFrequency)
        {
            Gamepad pad = Gamepad.current;
            if (pad == null) return;

            pad.SetMotorSpeeds(Mathf.Clamp01(lowFrequency), Mathf.Clamp01(highFrequency));
            GamepadRumble[pad.deviceId, pad] = true;
        }

        public static void StopRumble()
        {
            Gamepad pad = Gamepad.current;
            if (pad == null) return;

            pad.SetMotorSpeeds(0f, 0f);
            GamepadRumble[pad.deviceId, pad] = false;
        }

        public static void StopAllRumble()
        {
            foreach (var pad in Gamepad.all)
            {
                pad.SetMotorSpeeds(0f, 0f);
                GamepadRumble[pad.deviceId, pad] = false;
            }
        }


        public static SHUU_Timer RumbleTimer(float lowFrequency, float highFrequency, float duration)
        {
            SetRumble(lowFrequency, highFrequency);

            return SHUU_Time.Timer(duration, StopRumble);
        }

        public static Coroutine RumbleFor(float lowFrequency, float highFrequency, float duration) => SHUU_Time.StartCoroutineStatic(RumbleRoutine(lowFrequency, highFrequency, duration));
        public static IEnumerator RumbleRoutine(float lowFrequency, float highFrequency, float duration)
        {
            SetRumble(lowFrequency, highFrequency);

            yield return new WaitForSeconds(duration);

            StopRumble();
        }
        #endregion

        #endregion
    }
}