using System;
using KingdomFoundation.Api;
using KingdomNodeInspector.Core;

namespace KingdomNodeInspector.UI
{
    internal sealed class NodeInspectorFoundationUi : IDisposable
    {
        private readonly NodeInspectorPlugin plugin;
        private FoundationModContext context;
        private IModConfigButton hotkeyButton;

        public NodeInspectorFoundationUi(NodeInspectorPlugin plugin)
        {
            this.plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        }

        public void Register()
        {
            try
            {
                context = FoundationApi.ForMod(NodeInspectorPlugin.PluginId);
                RegisterSettings();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void UpdateHotkeyButtonText(string text)
        {
            if (hotkeyButton != null)
                hotkeyButton.ButtonText = text;
        }

        public void Dispose()
        {
            context?.Dispose();
            context = null;
            hotkeyButton = null;
        }

        private void RegisterSettings()
        {
            var settings = plugin.Settings;
            var section = context.Settings(
                "Node Inspector",
                "Hover any building and press the Inspect hotkey (default F6) to view and manage its internal inventory, buffers, soldiers, stats, and equipment.");

            section.AddText("Input", isHeader: true);

            hotkeyButton = section.AddButton(
                "Inspect Hotkey",
                () => plugin.StartListeningForHotkey(),
                buttonText: settings.FormatHotkeyLabel());

            section.AddText("General", isHeader: true);

            section.AddToggle(
                "Auto-Refresh While Open",
                settings.AutoRefresh,
                val => settings.SetAutoRefresh(val),
                getter: () => settings.AutoRefresh);

            section.AddSlider(
                "Auto-Refresh Interval",
                1.0f,
                10.0f,
                settings.RefreshInterval,
                val => settings.SetRefreshInterval(val),
                step: 0.5f,
                getter: () => settings.RefreshInterval,
                formatValue: val => $"{val:0.0}s");

            section.AddText("Safety", isHeader: true);

            section.AddToggle(
                "Confirm Single-Item Delete",
                settings.ConfirmSingleDelete,
                val => settings.SetConfirmSingleDelete(val),
                getter: () => settings.ConfirmSingleDelete);
        }
    }
}
