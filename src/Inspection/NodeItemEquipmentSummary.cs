using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomNodeInspector.Inspection
{
    internal sealed class NodeItemEquipmentSummary
    {
        public string DisplayName { get; }
        public string SlotDescription { get; }
        public Sprite Icon { get; }
        public float Quality { get; }
        public long SellValue { get; }
        public IReadOnlyList<NodeItemStatEntry> Stats { get; }
        public IReadOnlyList<NodeItemEnhancementInfo> Enhancements { get; }

        public NodeItemEquipmentSummary(
            string displayName,
            string slotDescription,
            Sprite icon,
            float quality,
            long sellValue,
            IReadOnlyList<NodeItemStatEntry> stats,
            IReadOnlyList<NodeItemEnhancementInfo> enhancements)
        {
            DisplayName = displayName ?? "";
            SlotDescription = slotDescription ?? "";
            Icon = icon;
            Quality = quality;
            SellValue = sellValue;
            Stats = stats ?? Array.Empty<NodeItemStatEntry>();
            Enhancements = enhancements ?? Array.Empty<NodeItemEnhancementInfo>();
        }
    }
}
