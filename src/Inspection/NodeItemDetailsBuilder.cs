using System.Collections.Generic;
using Assets.Source.Adventure;
using Assets.Source.Flow.Elements;
using Assets.Source.Items;
using Assets.Source.Items.Data;
using Assets.Source.Items.Type;
using Assets.Source.Util;
using UnityEngine;

namespace KingdomNodeInspector.Inspection
{
    internal static class NodeItemDetailsBuilder
    {
        public static NodeItemDetails Build(InventoryItem item)
        {
            if (item == null)
                return null;

            string typeId = item.Type?.Identifier ?? "";
            string displayName = SafeTranslate(item.DisplayName, typeId);
            string description = SafeTranslate(item.Description, "");
            Sprite icon = item.Icon;
            float quality = item.Quality;
            bool canSell = item.Type != null && Shop.CanSellItem(item);
            long sellValue = canSell ? item.SellValue : 0L;
            int manaValue = item.ManaValue;

            var stats = new List<NodeItemStatEntry>();
            var enhancements = new List<NodeItemEnhancementInfo>();
            var traits = new List<NodeItemTraitInfo>();
            var equipped = new List<NodeItemEquipmentSummary>();
            var sources = CollectSources(item.Type);

            bool isCombatant = false;
            bool isHuman = false;
            bool isEquipment = false;
            int currentHealth = 0;
            int maxHealth = 0;
            float health01 = 0f;
            int currentEnergy = 0;
            int maxEnergy = 0;
            float energy01 = 0f;
            int combatPower = 0;
            int armor = 0;
            int level = 0;
            bool isDead = false;
            bool isExhausted = false;
            bool hasLimitBreak = false;
            int trainingCount = 0;
            int questCount = 0;
            int arenaCount = 0;
            int specialCount = 0;
            string archetype = "";
            string slotDescription = "";

            if (item.Data is CombatantItemData combatant)
            {
                isCombatant = true;
                currentHealth = combatant.CurrentHealth;
                maxHealth = combatant.MaxHealth;
                health01 = combatant.Health01;
                currentEnergy = combatant.CurrentEnergy;
                maxEnergy = combatant.MaxEnergy;
                energy01 = combatant.Energy01;
                combatPower = combatant.GetCombatPower();
                armor = combatant.GetStat(EquipStat.Armor);
                level = combatant.Level;
                isDead = combatant.Dead;
                isExhausted = combatant.Exhausted;
                hasLimitBreak = combatant.LimitBreak;

                CollectCombatantStats(combatant, stats);
                CollectCombatantTraits(combatant, traits);

                if (combatant is HumanItemData human)
                {
                    isHuman = true;
                    trainingCount = human.TrainingCount;
                    questCount = human.QuestCount;
                    arenaCount = human.ArenaCount;
                    specialCount = human.SpecialCount;
                    archetype = ResolveArchetypeName(human);
                    CollectHumanEquipment(human, equipped);
                }
            }
            else if (item.Type is EquipmentItemType equipType && item.Data is EquipmentItemData equipData)
            {
                isEquipment = true;
                slotDescription = SafeTranslate(equipType.GetSlotDescription(), "");
                CollectEquipmentStats(equipType, item, stats);
                CollectEnhancements(equipData, enhancements);
                CollectEquipmentTrait(equipType, item, traits);
            }

            return new NodeItemDetails(
                typeId,
                displayName,
                description,
                icon,
                quality,
                sellValue,
                manaValue,
                canSell,
                isCombatant,
                isHuman,
                isEquipment,
                currentHealth,
                maxHealth,
                health01,
                currentEnergy,
                maxEnergy,
                energy01,
                combatPower,
                armor,
                level,
                isDead,
                isExhausted,
                hasLimitBreak,
                trainingCount,
                questCount,
                arenaCount,
                specialCount,
                archetype,
                slotDescription,
                stats.AsReadOnly(),
                enhancements.AsReadOnly(),
                traits.AsReadOnly(),
                equipped.AsReadOnly(),
                sources.AsReadOnly());
        }

        public static string SafeTranslate(string token, string fallback)
        {
            if (string.IsNullOrEmpty(token))
                return fallback;

            try
            {
                string translated = Translation.Translate(token);
                if (!string.IsNullOrEmpty(translated) && !(translated.StartsWith("@") && translated == token))
                    return translated;
            }
            catch
            {
            }

            return string.IsNullOrEmpty(fallback) ? token : fallback;
        }

        private static void CollectCombatantStats(CombatantItemData combatant, List<NodeItemStatEntry> target)
        {
            for (int i = 0; i < GameConstants.StatCount; i++)
            {
                var stat = (EquipStat)i;
                if (stat == EquipStat.Health || stat == EquipStat.Energy)
                    continue;

                int value = combatant.GetStat(stat);
                if (value == 0)
                    continue;

                string label = SafeTranslate(stat.GetDisplayName(), stat.ToString());
                target.Add(new NodeItemStatEntry(label, value, $"{label}: {GameMath.FormatNumber(value)}"));
            }
        }

        private static void CollectCombatantTraits(CombatantItemData combatant, List<NodeItemTraitInfo> target)
        {
            foreach (ActiveTrait active in combatant.GetTraits(mutable: false))
            {
                if (active.Trait == null)
                    continue;

                string name = SafeTranslate(active.Trait.DisplayName, "");
                string desc = SafeTranslate(active.Description, "");
                target.Add(new NodeItemTraitInfo(name, desc, active.Trait.Icon));
            }
        }

        private static string ResolveArchetypeName(HumanItemData human)
        {
            if (human.HumanType == null || human.HumanType.Archetype == AdventurerArchetype.None)
                return "";

            var trait = human.HumanType.Archetype.GetArchetypeTrait();
            return trait != null
                ? SafeTranslate(trait.DisplayName, human.HumanType.Archetype.ToString())
                : human.HumanType.Archetype.ToString();
        }

        private static void CollectHumanEquipment(HumanItemData human, List<NodeItemEquipmentSummary> target)
        {
            for (int i = 0; i < human.Equipment.Count; i++)
            {
                InventoryItem eq = human.Equipment[i];
                if (eq == null)
                    continue;

                var eqType = eq.Type as EquipmentItemType;
                var eqData = eq.Data as EquipmentItemData;
                var eqStats = new List<NodeItemStatEntry>();
                var eqEnhancements = new List<NodeItemEnhancementInfo>();

                if (eqType != null)
                    CollectEquipmentStats(eqType, eq, eqStats);
                if (eqData != null)
                    CollectEnhancements(eqData, eqEnhancements);

                target.Add(new NodeItemEquipmentSummary(
                    SafeTranslate(eq.DisplayName, eq.Type?.Identifier ?? ""),
                    eqType != null ? SafeTranslate(eqType.GetSlotDescription(), "") : "",
                    eq.Icon,
                    eq.Quality,
                    eq.SellValue,
                    eqStats.AsReadOnly(),
                    eqEnhancements.AsReadOnly()));
            }
        }

        private static void CollectEquipmentStats(EquipmentItemType equipType, InventoryItem item, List<NodeItemStatEntry> target)
        {
            var entries = new List<NodeItemStatEntry>();
            for (int i = 0; i < GameConstants.StatCount; i++)
            {
                var stat = (EquipStat)i;
                int value = equipType.GetStat(item, stat);
                if (value == 0)
                    continue;

                string label = SafeTranslate(stat.GetDisplayName(), stat.ToString());
                string formatted = SafeTranslate(
                    Translation.Translate("@UIStatLine", value, stat.GetDisplayName()),
                    $"+{value} {label}");
                entries.Add(new NodeItemStatEntry(label, value, formatted));
            }

            entries.Sort((a, b) => b.Value.CompareTo(a.Value));
            target.AddRange(entries);
        }

        private static void CollectEnhancements(EquipmentItemData equipData, List<NodeItemEnhancementInfo> target)
        {
            foreach (ItemEnhancementData enhancement in equipData.Enhancements)
            {
                if (enhancement?.Enhancement == null)
                    continue;

                string text = SafeTranslate(enhancement.TextLine, enhancement.Enhancement.Identifier);
                target.Add(new NodeItemEnhancementInfo(text, enhancement.Enhancement.Icon, enhancement.Quality, enhancement.SellValue));
            }
        }

        private static void CollectEquipmentTrait(EquipmentItemType equipType, InventoryItem item, List<NodeItemTraitInfo> target)
        {
            if (equipType.EquipTrait == null)
                return;

            var preview = equipType.PreviewActiveTrait(item);
            string name = SafeTranslate(equipType.EquipTrait.DisplayName, "");
            string desc = SafeTranslate(preview.Description, "");
            target.Add(new NodeItemTraitInfo(name, desc, equipType.EquipTrait.Icon));
        }

        private static List<string> CollectSources(ItemType type)
        {
            var result = new List<string>();
            if (type == null)
                return result;

            foreach (string raw in type.GetItemSources())
            {
                string translated = SafeTranslate(raw, "");
                if (!string.IsNullOrWhiteSpace(translated))
                    result.Add(translated);
            }

            return result;
        }
    }
}
