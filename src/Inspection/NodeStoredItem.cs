using System.Runtime.CompilerServices;
using Assets.Source.Flow;
using Assets.Source.Items;

namespace KingdomNodeInspector.Inspection
{
    internal sealed class NodeStoredItem
    {
        public FlowElement Owner { get; }
        public NodeStoredItemKind Kind { get; }
        public string SlotLabel { get; }
        public int SlotIndex { get; }
        public int ItemIndex { get; }
        public int SlotCapacity { get; }
        public InventoryItem Item { get; }
        public string ProgressSummary { get; }
        public object NativeContainer { get; }
        public object NativeEntry { get; }
        public string StableKey { get; }

        public NodeStoredItem(
            FlowElement owner,
            NodeStoredItemKind kind,
            string slotLabel,
            int slotIndex,
            int itemIndex,
            int slotCapacity,
            InventoryItem item,
            string progressSummary,
            object nativeContainer,
            object nativeEntry)
        {
            Owner = owner;
            Kind = kind;
            SlotLabel = slotLabel ?? "";
            SlotIndex = slotIndex;
            ItemIndex = itemIndex;
            SlotCapacity = slotCapacity;
            Item = item;
            ProgressSummary = progressSummary;
            NativeContainer = nativeContainer;
            NativeEntry = nativeEntry;
            StableKey = $"{(int)kind}:{slotIndex}:{RuntimeHelpers.GetHashCode(item)}";
        }
    }
}
