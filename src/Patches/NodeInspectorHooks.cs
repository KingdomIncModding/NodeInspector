using System;
using System.Collections.Generic;
using KingdomFoundation.Api;
using KingdomNodeInspector.Core;

namespace KingdomNodeInspector.Patches
{
    internal sealed class NodeInspectorHooks : IDisposable
    {
        private readonly NodeInspectorPlugin plugin;
        private readonly List<IDisposable> subscriptions = new List<IDisposable>();

        public NodeInspectorHooks(NodeInspectorPlugin plugin)
        {
            this.plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        }

        public void Install()
        {
            var hooks = FoundationApi.GameHooks;
            string modId = NodeInspectorPlugin.PluginId;
            subscriptions.Add(hooks.Subscribe(modId, GameHookPoints.Escape, OnEscape, after: null, priority: 100));
            subscriptions.Add(hooks.Subscribe(modId, GameHookPoints.Cancel, OnCancel, after: null, priority: 100));
            subscriptions.Add(hooks.Subscribe(modId, GameHookPoints.WorkspaceShown, before: null, after: OnWorkspaceChanged, priority: 0));
            subscriptions.Add(hooks.Subscribe(modId, GameHookPoints.UiVisibility, before: null, after: OnUiVisibilityChanged, priority: 0));
        }

        public void Dispose()
        {
            for (int i = 0; i < subscriptions.Count; i++)
                subscriptions[i]?.Dispose();
            subscriptions.Clear();
        }

        private void OnEscape(GameHookCall call)
        {
            if (call.IsCancelled)
                return;

            if (plugin.HandleEscapePressed())
                call.Cancel();
        }

        private void OnCancel(GameHookCall call)
        {
            if (call.IsCancelled)
                return;

            if (plugin.IsPopupOpen)
            {
                plugin.CloseInspector();
                call.Cancel();
            }
        }

        private void OnWorkspaceChanged(GameHookCall call)
        {
            plugin.CloseInspector();
        }

        private void OnUiVisibilityChanged(GameHookCall call)
        {
            if (!FoundationApi.UiAccess.IsWorkspaceVisible)
                plugin.CloseInspector();
        }
    }
}
