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
        public ConfigEntry<bool> AutoRefreshEntry { get; }
        public ConfigEntry<float> RefreshIntervalEntry { get; }
        public ConfigEntry<bool> ConfirmSingleDeleteEntry { get; }

        public Key InspectKey => InspectKeyEntry.Value;
        public bool AutoRefresh => AutoRefreshEntry.Value;
        public float RefreshInterval => Mathf.Clamp(RefreshIntervalEntry.Value, 1.0f, 10.0f);
        public bool ConfirmSingleDelete => ConfirmSingleDeleteEntry.Value;

        public NodeInspectorSettings(ConfigFile config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));

            InspectKeyEntry = config.Bind(
                "Input",
                "InspectKey",
                Key.F6,
                "Keyboard shortcut to inspect the building currently under the mouse cursor.");

            AutoRefreshEntry = config.Bind(
                "General",
                "AutoRefreshWhileOpen",
                false,
                "Automatically refresh the open inspector list on a timer. Disabled by default so entries stay steady while viewing; use the Refresh button anytime.");

            RefreshIntervalEntry = config.Bind(
                "General",
                "AutoRefreshIntervalSeconds",
                3.0f,
                new ConfigDescription(
                    "Interval between automatic refreshes when Auto-Refresh is enabled (seconds).",
                    new AcceptableValueRange<float>(1.0f, 10.0f)));

            ConfirmSingleDeleteEntry = config.Bind(
                "Safety",
                "ConfirmItemDelete",
                true,
                "Require confirmation before deleting a single item. Mass purge always asks for confirmation.");
        }

        public void SetInspectKey(Key key)
        {
            if (InspectKeyEntry.Value == key)
                return;

            InspectKeyEntry.Value = key;
            Save();
        }

        public void SetAutoRefresh(bool enabled)
        {
            if (AutoRefreshEntry.Value == enabled)
                return;

            AutoRefreshEntry.Value = enabled;
            Save();
        }

        public void SetRefreshInterval(float seconds)
        {
            float clamped = Mathf.Clamp(seconds, 1.0f, 10.0f);
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
