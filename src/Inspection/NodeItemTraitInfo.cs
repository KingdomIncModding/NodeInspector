using UnityEngine;

namespace KingdomNodeInspector.Inspection
{
    internal sealed class NodeItemTraitInfo
    {
        public string Name { get; }
        public string Description { get; }
        public Sprite Icon { get; }

        public NodeItemTraitInfo(string name, string description, Sprite icon)
        {
            Name = name ?? "";
            Description = description ?? "";
            Icon = icon;
        }
    }
}
