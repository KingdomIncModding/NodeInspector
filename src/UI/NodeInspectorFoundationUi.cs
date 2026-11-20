using System;
using KingdomFoundation.Api;
using KingdomNodeInspector.Core;
using UnityEngine;

namespace KingdomNodeInspector.UI
{
    internal sealed class NodeInspectorFoundationUi : IDisposable
    {
        private readonly NodeInspectorPlugin plugin;
        private FoundationModContext context;
        private NodeInspectorModBarElement barElement;
        private IModBarRegistration barRegistration;
        private NodeInspectorModBarView activeBarView;
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
                RegisterModBar();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void SyncModBar()
        {
            if (barElement != null)
                barElement.IsVisible = plugin.Settings.ShowModBarButton;

            activeBarView?.Refresh();
            barRegistration?.Refresh();
        }

        public void UpdateHotkeyButtonText(string text)
        {
            if (hotkeyButton != null)
                hotkeyButton.ButtonText = text;
            activeBarView?.Refresh();
        }

        internal void SetActiveBarView(NodeInspectorModBarView view)
        {
            activeBarView = view;
        }

        internal void ClearActiveBarView(NodeInspectorModBarView view)
        {
            if (ReferenceEquals(activeBarView, view))
                activeBarView = null;
        }

        public void Dispose()
        {
            context?.Dispose();
            context = null;
            barElement = null;
            barRegistration = null;
            activeBarView = null;
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
                "Show Mod Bar Button",
                settings.ShowModBarButton,
                val =>
                {
                    settings.SetShowModBarButton(val);
                    SyncModBar();
                },
                getter: () => settings.ShowModBarButton);

            section.AddSlider(
                "Live Refresh Interval",
                0.2f,
                2.0f,
                settings.RefreshInterval,
                val => settings.SetRefreshInterval(val),
                step: 0.1f,
                getter: () => settings.RefreshInterval,
                formatValue: val => $"{val:0.0}s");

            section.AddText("Safety", isHeader: true);

            section.AddToggle(
                "Confirm Single-Item Delete",
                settings.ConfirmSingleDelete,
                val => settings.SetConfirmSingleDelete(val),
                getter: () => settings.ConfirmSingleDelete);
        }

        private void RegisterModBar()
        {
            if (FoundationApi.ModBar == null)
                return;

            barElement = new NodeInspectorModBarElement(plugin, this)
            {
                IsVisible = plugin.Settings.ShowModBarButton
            };

            barRegistration = context.Register(barElement);
        }
    }
}
