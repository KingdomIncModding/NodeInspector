using Assets.Source.Flow;
using BepInEx;
using KingdomFoundation.Api;
using KingdomNodeInspector.Configuration;
using KingdomNodeInspector.Inspection;
using KingdomNodeInspector.Patches;
using KingdomNodeInspector.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KingdomNodeInspector.Core
{
    [BepInPlugin(PluginId, PluginName, PluginVersion)]
    [BepInDependency("urakka.kingdominc.foundation", "0.14.4")]
    public sealed class NodeInspectorPlugin : BaseUnityPlugin
    {
        public const string PluginId = "urakka.kingdominc.nodeinspector";
        public const string PluginName = "Node Inspector";
        public const string PluginVersion = "0.1.1";

        public static NodeInspectorPlugin Instance { get; private set; }
        public NodeInspectorSettings Settings { get; private set; }

        public bool IsPopupOpen => activePopup != null;

        private NodeInspectorHooks hooks;
        private NodeInspectorFoundationUi foundationUi;
        private NodeInspectorPopup activePopup;
        private bool listeningForHotkey;
        private float lastHotkeyTime;

        private void Awake()
        {
            Instance = this;
            Settings = new NodeInspectorSettings(Config);

            hooks = new NodeInspectorHooks(this);
            hooks.Install();

            foundationUi = new NodeInspectorFoundationUi(this);
            foundationUi.Register();

            Logger.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }

        private void Update()
        {
            if (listeningForHotkey)
            {
                HandleHotkeyCapture();
                return;
            }

            if (activePopup && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                HandleEscapePressed();
                return;
            }

            HandleInspectHotkey();
        }

        private void OnDestroy()
        {
            CloseInspector();

            hooks?.Dispose();
            hooks = null;

            foundationUi?.Dispose();
            foundationUi = null;

            NodeInspectorIcons.Release();

            if (Instance == this)
                Instance = null;
        }

        public void StartListeningForHotkey()
        {
            listeningForHotkey = true;
            foundationUi?.UpdateHotkeyButtonText("[ Press Key... ]");
        }

        public void OpenInspector(FlowElement element)
        {
            if (element == null)
                return;

            if (activePopup)
                activePopup.InspectTarget(element);
            else
                activePopup = NodeInspectorPopup.Open(element, Settings, OnPopupClosed);
        }

        public void CloseInspector()
        {
            if (activePopup)
            {
                activePopup.Close();
                activePopup = null;
            }
        }

        public bool HandleEscapePressed()
        {
            return activePopup && activePopup.HandleEscape();
        }

        private void OnPopupClosed()
        {
            activePopup = null;
        }

        private void HandleInspectHotkey()
        {
            if (Settings.InspectKey == Key.None || Keyboard.current == null)
                return;

            if (!Application.isFocused || IsTextInputFocused())
                return;

            var control = Keyboard.current[Settings.InspectKey];
            if (control == null || !control.wasPressedThisFrame)
                return;

            if (Time.unscaledTime - lastHotkeyTime < 0.2f)
                return;

            lastHotkeyTime = Time.unscaledTime;

            if (activePopup)
            {
                CloseInspector();
                return;
            }

            if (!CanOpenInspectorNow())
                return;

            FlowElement hovered = NodeInventoryAccessor.FindHoveredBuilding();
            if (hovered == null)
            {
                ShowStatus("Node Inspector: Hover over a building and press " + Settings.InspectKey + ".");
                return;
            }

            OpenInspector(hovered);
        }

        private bool CanOpenInspectorNow()
        {
            var ui = FoundationApi.UiAccess;
            if (!ui.IsGameInputEnabled || !ui.IsWorkspaceVisible || ui.IsMenuVisible || ui.IsAlertVisible || ui.HasPopup || ui.IsTutorialActive)
                return false;

            return true;
        }

        private void HandleHotkeyCapture()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                FinishHotkeyCapture();
                return;
            }

            if (Keyboard.current.backspaceKey.wasPressedThisFrame || Keyboard.current.deleteKey.wasPressedThisFrame)
            {
                Settings.SetInspectKey(Key.None);
                FinishHotkeyCapture();
                return;
            }

            foreach (var key in Keyboard.current.allKeys)
            {
                if (key == null || !key.wasPressedThisFrame)
                    continue;

                if (key.keyCode == Key.None || key.keyCode == Key.Escape)
                    continue;

                Settings.SetInspectKey(key.keyCode);
                FinishHotkeyCapture();
                return;
            }
        }

        private void FinishHotkeyCapture()
        {
            listeningForHotkey = false;
            foundationUi?.UpdateHotkeyButtonText(Settings.FormatHotkeyLabel());
        }

        private static bool IsTextInputFocused()
        {
            var selected = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
            if (!selected)
                return false;

            var tmp = selected.GetComponentInParent<TMP_InputField>();
            if (tmp && tmp.isFocused)
                return true;

            var legacy = selected.GetComponentInParent<InputField>();
            return legacy && legacy.isFocused;
        }

        private static void ShowStatus(string message)
        {
            try
            {
                FoundationApi.UiAccess.ShowStatusMessage(message);
            }
            catch
            {
            }
        }
    }
}
