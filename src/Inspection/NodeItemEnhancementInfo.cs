using UnityEngine;

namespace KingdomNodeInspector.Inspection
{
    internal sealed class NodeItemEnhancementInfo
    {
        public string Text { get; }
        public Sprite Icon { get; }
        public float Quality { get; }
        public long SellValue { get; }

        public NodeItemEnhancementInfo(string text, Sprite icon, float quality, long sellValue)
        {
            Text = text ?? "";
            Icon = icon;
            Quality = quality;
            SellValue = sellValue;
        }
    }
}
