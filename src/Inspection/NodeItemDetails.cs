using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomNodeInspector.Inspection
{
    internal sealed class NodeItemDetails
    {
        public string TypeId { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public Sprite Icon { get; }
        public float Quality { get; }
        public long SellValue { get; }
        public int ManaValue { get; }
        public bool CanSell { get; }

        public bool IsCombatant { get; }
        public bool IsHuman { get; }
        public bool IsEquipment { get; }

        public int CurrentHealth { get; }
        public int MaxHealth { get; }
        public float Health01 { get; }
        public int CurrentEnergy { get; }
        public int MaxEnergy { get; }
        public float Energy01 { get; }
        public int CombatPower { get; }
        public int Armor { get; }
        public int Level { get; }
        public bool IsDead { get; }
        public bool IsExhausted { get; }
        public bool HasLimitBreak { get; }
        public int TrainingCount { get; }
        public int QuestCount { get; }
        public int ArenaCount { get; }
        public int SpecialCount { get; }
        public string Archetype { get; }

        public string SlotDescription { get; }
        public IReadOnlyList<NodeItemStatEntry> Stats { get; }
        public IReadOnlyList<NodeItemEnhancementInfo> Enhancements { get; }
        public IReadOnlyList<NodeItemTraitInfo> Traits { get; }
        public IReadOnlyList<NodeItemEquipmentSummary> EquippedItems { get; }
        public IReadOnlyList<string> Sources { get; }

        public NodeItemDetails(
            string typeId,
            string displayName,
            string description,
            Sprite icon,
            float quality,
            long sellValue,
            int manaValue,
            bool canSell,
            bool isCombatant,
            bool isHuman,
            bool isEquipment,
            int currentHealth,
            int maxHealth,
            float health01,
            int currentEnergy,
            int maxEnergy,
            float energy01,
            int combatPower,
            int armor,
            int level,
            bool isDead,
            bool isExhausted,
            bool hasLimitBreak,
            int trainingCount,
            int questCount,
            int arenaCount,
            int specialCount,
            string archetype,
            string slotDescription,
            IReadOnlyList<NodeItemStatEntry> stats,
            IReadOnlyList<NodeItemEnhancementInfo> enhancements,
            IReadOnlyList<NodeItemTraitInfo> traits,
            IReadOnlyList<NodeItemEquipmentSummary> equippedItems,
            IReadOnlyList<string> sources)
        {
            TypeId = typeId ?? "";
            DisplayName = displayName ?? "";
            Description = description ?? "";
            Icon = icon;
            Quality = quality;
            SellValue = sellValue;
            ManaValue = manaValue;
            CanSell = canSell;
            IsCombatant = isCombatant;
            IsHuman = isHuman;
            IsEquipment = isEquipment;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            Health01 = health01;
            CurrentEnergy = currentEnergy;
            MaxEnergy = maxEnergy;
            Energy01 = energy01;
            CombatPower = combatPower;
            Armor = armor;
            Level = level;
            IsDead = isDead;
            IsExhausted = isExhausted;
            HasLimitBreak = hasLimitBreak;
            TrainingCount = trainingCount;
            QuestCount = questCount;
            ArenaCount = arenaCount;
            SpecialCount = specialCount;
            Archetype = archetype ?? "";
            SlotDescription = slotDescription ?? "";
            Stats = stats ?? Array.Empty<NodeItemStatEntry>();
            Enhancements = enhancements ?? Array.Empty<NodeItemEnhancementInfo>();
            Traits = traits ?? Array.Empty<NodeItemTraitInfo>();
            EquippedItems = equippedItems ?? Array.Empty<NodeItemEquipmentSummary>();
            Sources = sources ?? Array.Empty<string>();
        }
    }
}
