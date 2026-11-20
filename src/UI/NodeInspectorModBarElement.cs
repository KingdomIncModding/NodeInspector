using System;
using KingdomFoundation.Api;
using KingdomNodeInspector.Core;

namespace KingdomNodeInspector.UI
{
    internal sealed class NodeInspectorModBarElement : IModBarElement
    {
        private readonly NodeInspectorPlugin plugin;
        private readonly NodeInspectorFoundationUi ui;

        public string Id => "urakka.kingdominc.nodeinspector.bar";
        public string ModId => NodeInspectorPlugin.PluginId;
        public float Width => 116f;
        public int Order => 62;
        public ModBarAlignment Alignment => ModBarAlignment.Left;
        public bool IsVisible { get; set; } = true;

        public NodeInspectorModBarElement(NodeInspectorPlugin plugin, NodeInspectorFoundationUi ui)
        {
            this.plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            this.ui = ui ?? throw new ArgumentNullException(nameof(ui));
        }

        public IModBarView CreateView(IModBarContext context)
        {
            var view = new NodeInspectorModBarView(context, plugin, ui);
            ui.SetActiveBarView(view);
            return view;
        }
    }
}
