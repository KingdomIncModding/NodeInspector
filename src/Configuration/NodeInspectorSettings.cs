using System;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomNodeInspector.Configuration
{
    public sealed class NodeInspectorSettings
    {
        private readonly ConfigFile config;

        public ConfigEntry<Key> InspectKeyEntry { get; }
        public ConfigEntry<bool> ShowModBarButtonEntry { get; }
        public ConfigEntry<float> RefreshIntervalEntry { get; }
        public ConfigEntry<bool> ConfirmSingleDeleteEntry { get; }

        public Key InspectKey => InspectKeyEntry.Value;
        public bool ShowModBarButton => ShowModBarButtonEntry.Value;
        public float RefreshInterval => Mathf.Clamp(RefreshIntervalEntry.Value, 0.2f, 2.0f);
        public bool ConfirmSingleDelete => ConfirmSingleDeleteEntry.Value;

        public NodeInspectorSettings(ConfigFile config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));

            InspectKeyEntry = config.Bind(
                "Input",
                "InspectKey",
                Key.F6,
                "Keyboard shortcut to inspect the building currently under the mouse cursor.");

            ShowModBarButtonEntry = config.Bind(
                "General",
                "ShowModBarButton",
                true,
                "Show a quick Inspect button in the top Mod Bar.");

            RefreshIntervalEntry = config.Bind(
                "General",
                "RefreshIntervalSeconds",
                0.5f,
                new ConfigDescription(
                    "How frequently the inspector refreshes live node contents while open (seconds).",
                    new AcceptableValueRange<float>(0.2f, 2.0f)));

            ConfirmSingleDeleteEntry = config.Bind(
                "Safety",
                "ConfirmSingleDelete",
                false,
                "Require confirmation before deleting a single item. Mass purge always asks for confirmation.");
        }

        public void SetInspectKey(Key key)
        {
            if (InspectKeyEntry.Value == key)
                return;

            InspectKeyEntry.Value = key;
            Save();
        }

        public void SetShowModBarButton(bool visible)
        {
            if (ShowModBarButtonEntry.Value == visible)
                return;

            ShowModBarButtonEntry.Value = visible;
            Save();
        }

        public void SetRefreshInterval(float seconds)
        {
            float clamped = Mathf.Clamp(seconds, 0.2f, 2.0f);
            if (Mathf.Approximately(RefreshIntervalEntry.Value, clamped))
                return;

            RefreshIntervalEntry.Value = clamped;
            Save();
        }

        public void SetConfirmSingleDelete(bool confirm)
        {
            if (ConfirmSingleDeleteEntry.Value == confirm)
                return;

            ConfirmSingleDeleteEntry.Value = confirm;
            Save();
        }

        public string FormatHotkeyLabel()
        {
            return InspectKey == Key.None ? "[ None ]" : $"[ {InspectKey} ]";
        }

        private void Save()
        {
            config.Save();
        }
    }
}
