using System;
using System.Collections.Generic;
using System.Reflection;
using Assets.Behaviour.Flow;
using Assets.Behaviour.Flow.Elements;
using Assets.Behaviour.Input;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using Assets.Source.Flow.Elements;
using Assets.Source.Items;
using Assets.Source.Items.Data;
using Assets.Source.Player;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KingdomNodeInspector.Inspection
{
    internal static class NodeInventoryAccessor
    {
        private static readonly AccessTools.FieldRef<ArmoryContents, List<InventoryItem>[,]> ArmoryGrid =
            AccessTools.FieldRefAccess<ArmoryContents, List<InventoryItem>[,]>("_contents");

        private static readonly AccessTools.FieldRef<PortalContents, List<InventoryItem>[,]> PortalGrid =
            AccessTools.FieldRefAccess<PortalContents, List<InventoryItem>[,]>("_contents");

        private static readonly MethodInfo QuestBoardUpdateVisuals =
            AccessTools.Method(typeof(WorldFlowQuestBoard), "UpdateQuest");

        public static FlowElement FindHoveredBuilding()
        {
            var player = GamePlayer.Current;
            if (player == null || !WorldFlowManager.Instance || !WorldFlowManager.Visible)
                return null;

            FlowDiagram activeFlow = player.CurrentFlow >= 0 && player.CurrentFlow < player.Flows.Count
                ? player.Flows[player.CurrentFlow]
                : null;
            if (activeFlow == null)
                return null;

            if (EventSystem.current != null)
            {
                var direct = UIHelper.GetMouseOverUI<WorldFlowElement>();
                if (direct && direct.Element != null && activeFlow.Elements.Contains(direct.Element))
                    return direct.Element;

                foreach (GameObject hit in UIHelper.GetAllMouseOverUI())
                {
                    if (!hit)
                        continue;

                    var building = hit.GetComponentInParent<WorldFlowElement>();
                    if (building && building.Element != null && activeFlow.Elements.Contains(building.Element))
                        return building.Element;

                    if (!hit.GetComponentInParent<WorldFlowManager>())
                        return null;
                }
            }

            if (FlowColliderManager.Instance)
            {
                foreach (Collider2D collider in FlowColliderManager.Instance.GetOverlap(PlayerInput.MouseWorld, Vector2Int.one))
                {
                    if (!collider)
                        continue;

                    var flowCollider = collider.GetComponent<FlowCollider>();
                    var worldEl = collider.GetComponent<WorldFlowElement>() ?? flowCollider?.Element;
                    if (worldEl && worldEl.Element != null && activeFlow.Elements.Contains(worldEl.Element))
                        return worldEl.Element;
                }
            }

            return null;
        }

        public static bool IsInActiveWorkspace(FlowElement element)
        {
            if (element == null)
                return false;

            var player = GamePlayer.Current;
            if (player == null || player.CurrentFlow < 0 || player.CurrentFlow >= player.Flows.Count)
                return false;

            return player.Flows[player.CurrentFlow].Elements.Contains(element);
        }

        public static string GetBuildingTitle(FlowElement element)
        {
            if (element == null)
                return "Node";

            string name = NodeItemDetailsBuilder.SafeTranslate(element.Label, element.ElementType);
            if (element.StackSize > 1)
                name = $"{name} x{element.StackSize}";
            return name;
        }

        public static string GetBuildingSubtitle(FlowElement element)
        {
            if (element == null)
                return "";

            if (element is WarEffort war && war.WarType != null)
            {
                string warName = NodeItemDetailsBuilder.SafeTranslate(war.WarType.DisplayName, war.WarType.Identifier);
                return $"War Effort • {warName}";
            }

            if (element is Dungeon dungeon && dungeon.CurrentDungeon?.Dungeon != null)
            {
                string dungeonName = NodeItemDetailsBuilder.SafeTranslate(dungeon.CurrentDungeon.Dungeon.DisplayName, dungeon.CurrentDungeon.Dungeon.Identifier);
                return $"Dungeon • {dungeonName} (Stage {dungeon.CurrentDungeon.CurrentStage + 1})";
            }

            if (element is QuestBoard qb && qb.SelectedQuest?.Quest != null)
            {
                string questName = NodeItemDetailsBuilder.SafeTranslate(qb.SelectedQuest.Quest.DisplayName, qb.SelectedQuest.Quest.Identifier);
                return $"Quest Board • {questName}";
            }

            if (element is Stockpile sp)
                return $"Stockpile • Capacity {sp.Contents.Count}/{sp.Capacity}";

            if (element is Armory armory)
                return armory.OutputType != null
                    ? $"Armory Output • Channel {armory.Channel + 1}"
                    : $"Armory Input • Channel {armory.Channel + 1}";

            if (element is Portal portal)
                return portal.IsOutput
                    ? $"Portal Output • Ch {portal.Channel + 1}-{portal.ChannelIndex + 1}"
                    : $"Portal Input • Ch {portal.Channel + 1}-{portal.ChannelIndex + 1}";

            if (element is RecipeElement recipe && recipe.Recipe != null)
            {
                string recipeName = NodeItemDetailsBuilder.SafeTranslate(recipe.Recipe.DisplayName, recipe.Recipe.Identifier);
                return $"{element.ElementType} • {recipeName}";
            }

            if (element is ResourceElement res && res.Resource != null)
            {
                string resName = NodeItemDetailsBuilder.SafeTranslate(res.Resource.GetDisplayName(), res.Resource.Identifier);
                return $"{element.ElementType} • {resName}";
            }

            return $"{element.ElementType} • {element.Inputs.Count} in / {element.Outputs.Count} out";
        }

        public static IReadOnlyList<NodeStoredItem> CollectItems(FlowElement element)
        {
            var results = new List<NodeStoredItem>();
            if (element == null)
                return results.AsReadOnly();

            CollectSpecialStorage(element, results);
            CollectOutputBuffers(element, results);
            CollectInputBuffers(element, results);

            return results.AsReadOnly();
        }

        public static bool TryRemove(FlowElement element, NodeStoredItem entry)
        {
            if (element == null || entry == null)
                return false;

            switch (entry.Kind)
            {
                case NodeStoredItemKind.InputBuffer:
                case NodeStoredItemKind.OutputBuffer:
                    return RemoveFromPortBuffer(entry);
                case NodeStoredItemKind.Stockpile:
                    return element is Stockpile stockpile && entry.NativeEntry is InventoryItem stockItem && stockpile.Contents.Remove(stockItem);
                case NodeStoredItemKind.WarEffortSoldier:
                    return element is WarEffort warEffort && entry.NativeEntry is CombatantItemData soldier && RemoveWarEffortSoldier(warEffort, soldier);
                case NodeStoredItemKind.DungeonParty:
                    return element is Dungeon dungeon && entry.NativeEntry is HumanItemData member && RemoveDungeonPartyMember(dungeon, member);
                case NodeStoredItemKind.QuestBoardMember:
                    return element is QuestBoard questBoard && entry.NativeEntry is HumanItemData adventurer && RemoveQuestBoardMember(questBoard, adventurer);
                case NodeStoredItemKind.ArenaSlot:
                    return element is Arena arena && RemoveArenaOccupant(arena, entry.SlotIndex, entry.NativeEntry as CombatantItemData);
                case NodeStoredItemKind.InnOccupant:
                    return element is Inn inn && RemoveInnOccupant(inn, entry.SlotIndex, entry.NativeEntry as InnSlot);
                case NodeStoredItemKind.InnFood:
                    return element is Inn innFood && RemoveInnFood(innFood, entry.SlotIndex, entry.NativeEntry as InventoryItem);
                case NodeStoredItemKind.TrainingOccupant:
                    return element is TrainingGrounds tg && RemoveTrainingOccupant(tg, entry.SlotIndex, entry.NativeEntry as TrainingGroundsSlot);
                case NodeStoredItemKind.TrainingDummy:
                    return element is TrainingGrounds tgDummy && RemoveTrainingDummy(tgDummy, entry.NativeEntry as CombatantItemData);
                case NodeStoredItemKind.OutfitterHuman:
                    return element is Outfitter outfitter && RemoveOutfitterActive(outfitter, entry.NativeEntry as InventoryItem);
                case NodeStoredItemKind.CraftingInput:
                    return element is RecipeElement recipeElement && CancelActiveCraft(recipeElement);
                case NodeStoredItemKind.ArmoryStorage:
                case NodeStoredItemKind.PortalStorage:
                    return RemoveFromListContainer(entry);
                default:
                    return false;
            }
        }

        public static int PurgeAll(FlowElement element)
        {
            if (element == null)
                return 0;

            var snapshot = CollectItems(element);
            int removed = 0;
            for (int i = snapshot.Count - 1; i >= 0; i--)
            {
                if (TryRemove(element, snapshot[i]))
                    removed++;
            }

            return removed;
        }

        public static int PurgeByKind(FlowElement element, NodeStoredItemKind kind)
        {
            if (element == null)
                return 0;

            var snapshot = CollectItems(element);
            int removed = 0;
            for (int i = snapshot.Count - 1; i >= 0; i--)
            {
                if (snapshot[i].Kind == kind && TryRemove(element, snapshot[i]))
                    removed++;
            }

            return removed;
        }

        public static string GetKindGroupLabel(NodeStoredItemKind kind)
        {
            return kind switch
            {
                NodeStoredItemKind.WarEffortSoldier => "Soldiers",
                NodeStoredItemKind.Stockpile => "Stockpile",
                NodeStoredItemKind.DungeonParty => "Party",
                NodeStoredItemKind.QuestBoardMember => "Adventurers",
                NodeStoredItemKind.ArenaSlot => "Fighters",
                NodeStoredItemKind.InnOccupant => "Guests",
                NodeStoredItemKind.InnFood => "Inn Food",
                NodeStoredItemKind.TrainingOccupant => "Trainees",
                NodeStoredItemKind.TrainingDummy => "Dummy",
                NodeStoredItemKind.OutfitterHuman => "Active",
                NodeStoredItemKind.CraftingInput => "Crafting",
                NodeStoredItemKind.ArmoryStorage => "Armory",
                NodeStoredItemKind.PortalStorage => "Portal",
                NodeStoredItemKind.OutputBuffer => "Outputs",
                NodeStoredItemKind.InputBuffer => "Inputs",
                _ => "Items"
            };
        }

        private static void CollectSpecialStorage(FlowElement element, List<NodeStoredItem> results)
        {
            if (element is WarEffort warEffort)
                CollectWarEffort(warEffort, results);
            else if (element is Stockpile stockpile)
                CollectStockpile(stockpile, results);
            else if (element is Dungeon dungeon)
                CollectDungeon(dungeon, results);
            else if (element is QuestBoard questBoard)
                CollectQuestBoard(questBoard, results);
            else if (element is Arena arena)
                CollectArena(arena, results);
            else if (element is Inn inn)
                CollectInn(inn, results);
            else if (element is TrainingGrounds training)
                CollectTrainingGrounds(training, results);
            else if (element is Outfitter outfitter)
                CollectOutfitter(outfitter, results);
            else if (element is RecipeElement recipeElement)
                CollectCraftingInput(recipeElement, results);
            else if (element is Armory armory)
                CollectArmory(armory, results);
            else if (element is Portal portal)
                CollectPortal(portal, results);
        }

        private static void CollectWarEffort(WarEffort warEffort, List<NodeStoredItem> results)
        {
            for (int i = 0; i < warEffort.Soldiers.Count; i++)
            {
                CombatantItemData soldier = warEffort.Soldiers[i];
                if (soldier?.Item == null)
                    continue;

                string progress = ReferenceEquals(warEffort.CurrentSoldier, soldier) ? "Active Frontline" : null;
                results.Add(new NodeStoredItem(
                    warEffort,
                    NodeStoredItemKind.WarEffortSoldier,
                    $"Soldier #{i + 1}",
                    0,
                    i,
                    0,
                    soldier.Item,
                    progress,
                    warEffort.Soldiers,
                    soldier));
            }
        }

        private static void CollectStockpile(Stockpile stockpile, List<NodeStoredItem> results)
        {
            for (int i = 0; i < stockpile.Contents.Count; i++)
            {
                InventoryItem item = stockpile.Contents[i];
                if (item == null)
                    continue;

                results.Add(new NodeStoredItem(
                    stockpile,
                    NodeStoredItemKind.Stockpile,
                    $"Stockpile ({stockpile.Contents.Count}/{stockpile.Capacity})",
                    0,
                    i,
                    stockpile.Capacity,
                    item,
                    null,
                    stockpile.Contents,
                    item));
            }
        }

        private static void CollectDungeon(Dungeon dungeon, List<NodeStoredItem> results)
        {
            var party = dungeon.CurrentDungeon?.Party;
            if (party == null)
                return;

            for (int i = 0; i < party.Count; i++)
            {
                HumanItemData member = party[i];
                if (member?.Item == null)
                    continue;

                string progress = $"Stage {dungeon.CurrentDungeon.CurrentStage + 1}";
                results.Add(new NodeStoredItem(
                    dungeon,
                    NodeStoredItemKind.DungeonParty,
                    $"Party Slot #{i + 1}",
                    0,
                    i,
                    5,
                    member.Item,
                    progress,
                    party,
                    member));
            }
        }

        private static void CollectQuestBoard(QuestBoard questBoard, List<NodeStoredItem> results)
        {
            for (int i = 0; i < questBoard.Contents.Count; i++)
            {
                HumanItemData member = questBoard.Contents[i];
                if (member?.Item == null)
                    continue;

                string progress = questBoard.SelectedQuest != null
                    ? $"Quest {Math.Round(questBoard.SelectedQuest.Progress01 * 100f)}%"
                    : null;
                results.Add(new NodeStoredItem(
                    questBoard,
                    NodeStoredItemKind.QuestBoardMember,
                    $"Quest Slot #{i + 1}",
                    0,
                    i,
                    QuestBoard.SlotCount,
                    member.Item,
                    progress,
                    questBoard.Contents,
                    member));
            }
        }

        private static void CollectArena(Arena arena, List<NodeStoredItem> results)
        {
            string status = arena.Fighting ? $"Fighting ({arena.TicksRemaining} ticks left)" : "Waiting";
            if (arena.Slot1?.Item != null)
            {
                results.Add(new NodeStoredItem(
                    arena,
                    NodeStoredItemKind.ArenaSlot,
                    "Arena Slot #1",
                    0,
                    0,
                    1,
                    arena.Slot1.Item,
                    status,
                    arena,
                    arena.Slot1));
            }

            if (arena.Slot2?.Item != null)
            {
                results.Add(new NodeStoredItem(
                    arena,
                    NodeStoredItemKind.ArenaSlot,
                    "Arena Slot #2",
                    1,
                    0,
                    1,
                    arena.Slot2.Item,
                    status,
                    arena,
                    arena.Slot2));
            }
        }

        private static void CollectInn(Inn inn, List<NodeStoredItem> results)
        {
            for (int i = 0; i < inn.Contents.Length; i++)
            {
                InnSlot slot = inn.Contents[i];
                if (slot == null)
                    continue;

                if (slot.Human != null)
                {
                    results.Add(new NodeStoredItem(
                        inn,
                        NodeStoredItemKind.InnOccupant,
                        $"Inn Slot #{i + 1}",
                        i,
                        0,
                        1,
                        slot.Human,
                        $"Resting ({slot.TicksRemaining}/{slot.TickCount} ticks)",
                        inn.Contents,
                        slot));
                }

                if (slot.Food != null)
                {
                    results.Add(new NodeStoredItem(
                        inn,
                        NodeStoredItemKind.InnFood,
                        $"Inn Slot #{i + 1} Food",
                        i,
                        1,
                        1,
                        slot.Food,
                        null,
                        slot,
                        slot.Food));
                }
            }
        }

        private static void CollectTrainingGrounds(TrainingGrounds training, List<NodeStoredItem> results)
        {
            for (int i = 0; i < training.Contents.Length; i++)
            {
                TrainingGroundsSlot slot = training.Contents[i];
                if (slot?.Human == null)
                    continue;

                results.Add(new NodeStoredItem(
                    training,
                    NodeStoredItemKind.TrainingOccupant,
                    $"Training Slot #{i + 1}",
                    i,
                    0,
                    1,
                    slot.Human,
                    $"Training ({slot.TicksRemaining}/{slot.TickCount} ticks)",
                    training.Contents,
                    slot));
            }

            if (training.TrainingDummy?.Item != null)
            {
                results.Add(new NodeStoredItem(
                    training,
                    NodeStoredItemKind.TrainingDummy,
                    "Training Dummy",
                    0,
                    0,
                    1,
                    training.TrainingDummy.Item,
                    null,
                    training,
                    training.TrainingDummy));
            }
        }

        private static void CollectOutfitter(Outfitter outfitter, List<NodeStoredItem> results)
        {
            if (outfitter.CurrentHuman == null)
                return;

            results.Add(new NodeStoredItem(
                outfitter,
                NodeStoredItemKind.OutfitterHuman,
                "Outfitter Active",
                0,
                0,
                1,
                outfitter.CurrentHuman,
                $"Equipping ({Math.Round(outfitter.Progress * 100f)}%)",
                outfitter,
                outfitter.CurrentHuman));
        }

        private static void CollectCraftingInput(RecipeElement recipeElement, List<NodeStoredItem> results)
        {
            if (recipeElement.Input == null)
                return;

            int index = 0;
            foreach (InventoryItem item in recipeElement.Input.AllItems)
            {
                if (item == null)
                    continue;

                results.Add(new NodeStoredItem(
                    recipeElement,
                    NodeStoredItemKind.CraftingInput,
                    "In-Progress Craft",
                    0,
                    index,
                    0,
                    item,
                    $"Crafting ({Math.Round(recipeElement.Progress * 100f)}%)",
                    recipeElement.Input,
                    item));
                index++;
            }
        }

        private static void CollectArmory(Armory armory, List<NodeStoredItem> results)
        {
            var player = GamePlayer.Current;
            if (player?.Armory == null)
                return;

            var grid = ArmoryGrid(player.Armory);
            if (grid == null || armory.Channel < 0 || armory.Channel >= grid.GetLength(0))
                return;

            if (armory.OutputType != null)
            {
                int ordinal = armory.OutputType.Ordinal;
                if (ordinal >= 0 && ordinal < grid.GetLength(1))
                    CollectArmoryList(armory, armory.Channel, armory.MaxItems, grid[armory.Channel, ordinal], results);
                return;
            }

            int itemCount = grid.GetLength(1);
            for (int ordinal = 0; ordinal < itemCount; ordinal++)
            {
                List<InventoryItem> list = grid[armory.Channel, ordinal];
                if (list == null || list.Count == 0)
                    continue;

                ItemType itemType = ItemType.GetById(ordinal);
                if (itemType != null && armory.CanAcceptItem(itemType))
                    CollectArmoryList(armory, armory.Channel, armory.MaxItems, list, results);
            }
        }

        private static void CollectArmoryList(
            Armory armory,
            int channel,
            int maxItems,
            List<InventoryItem> list,
            List<NodeStoredItem> results)
        {
            if (list == null)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                InventoryItem item = list[i];
                if (item == null)
                    continue;

                results.Add(new NodeStoredItem(
                    armory,
                    NodeStoredItemKind.ArmoryStorage,
                    $"Armory Ch {channel + 1} ({list.Count}/{maxItems})",
                    channel,
                    i,
                    maxItems,
                    item,
                    null,
                    list,
                    item));
            }
        }

        private static void CollectPortal(Portal portal, List<NodeStoredItem> results)
        {
            var player = GamePlayer.Current;
            if (player?.Portal == null)
                return;

            var grid = PortalGrid(player.Portal);
            if (grid == null
                || portal.Channel < 0 || portal.Channel >= grid.GetLength(0)
                || portal.ChannelIndex < 0 || portal.ChannelIndex >= grid.GetLength(1))
                return;

            List<InventoryItem> list = grid[portal.Channel, portal.ChannelIndex];
            if (list == null)
                return;

            int max = player.Portal.MaxItemCount;
            for (int i = 0; i < list.Count; i++)
            {
                InventoryItem item = list[i];
                if (item == null)
                    continue;

                results.Add(new NodeStoredItem(
                    portal,
                    NodeStoredItemKind.PortalStorage,
                    $"Portal Ch {portal.Channel + 1}-{portal.ChannelIndex + 1} ({list.Count}/{max})",
                    portal.ChannelIndex,
                    i,
                    max,
                    item,
                    null,
                    list,
                    item));
            }
        }

        private static void CollectOutputBuffers(FlowElement element, List<NodeStoredItem> results)
        {
            for (int p = 0; p < element.Outputs.Count; p++)
            {
                FlowOutput output = element.Outputs[p];
                if (output?.BufferedItems == null)
                    continue;

                string label = element.Outputs.Count > 1
                    ? $"Output #{p + 1} ({output.BufferedItems.Count}/{output.BufferSize})"
                    : $"Output ({output.BufferedItems.Count}/{output.BufferSize})";

                for (int i = 0; i < output.BufferedItems.Count; i++)
                {
                    InventoryItem item = output.BufferedItems[i];
                    if (item == null)
                        continue;

                    results.Add(new NodeStoredItem(
                        element,
                        NodeStoredItemKind.OutputBuffer,
                        label,
                        p,
                        i,
                        output.BufferSize,
                        item,
                        null,
                        output,
                        item));
                }
            }
        }

        private static void CollectInputBuffers(FlowElement element, List<NodeStoredItem> results)
        {
            for (int p = 0; p < element.Inputs.Count; p++)
            {
                FlowInput input = element.Inputs[p];
                if (input?.BufferedItems == null)
                    continue;

                string label = element.Inputs.Count > 1
                    ? $"Input #{p + 1} ({input.BufferedItems.Count}/{input.BufferSize})"
                    : $"Input ({input.BufferedItems.Count}/{input.BufferSize})";

                for (int i = 0; i < input.BufferedItems.Count; i++)
                {
                    InventoryItem item = input.BufferedItems[i];
                    if (item == null)
                        continue;

                    results.Add(new NodeStoredItem(
                        element,
                        NodeStoredItemKind.InputBuffer,
                        label,
                        p,
                        i,
                        input.BufferSize,
                        item,
                        null,
                        input,
                        item));
                }
            }
        }

        private static bool RemoveFromPortBuffer(NodeStoredItem entry)
        {
            if (!(entry.NativeContainer is FlowIO port) || !(entry.NativeEntry is InventoryItem item))
                return false;

            return port.BufferedItems.Remove(item);
        }

        private static bool RemoveFromListContainer(NodeStoredItem entry)
        {
            if (!(entry.NativeContainer is List<InventoryItem> list) || !(entry.NativeEntry is InventoryItem item))
                return false;

            return list.Remove(item);
        }

        private static bool RemoveWarEffortSoldier(WarEffort warEffort, CombatantItemData soldier)
        {
            bool removed = warEffort.Soldiers.Remove(soldier);
            warEffort.WarType?.Progress?.Soldiers.Remove(soldier);
            if (ReferenceEquals(warEffort.CurrentSoldier, soldier))
                warEffort.CurrentSoldier = warEffort.Soldiers.Count > 0 ? warEffort.Soldiers[0] : null;
            return removed;
        }

        private static bool RemoveDungeonPartyMember(Dungeon dungeon, HumanItemData member)
        {
            var progress = dungeon.CurrentDungeon;
            if (progress == null || !progress.Party.Remove(member))
                return false;

            if (progress.Party.Count == 0)
                progress.ClearDungeonProgress();

            return true;
        }

        private static bool RemoveQuestBoardMember(QuestBoard questBoard, HumanItemData adventurer)
        {
            if (!questBoard.Contents.Remove(adventurer))
                return false;

            var view = questBoard.World ? questBoard.World.GetComponent<WorldFlowQuestBoard>() : null;
            if (view && QuestBoardUpdateVisuals != null)
                QuestBoardUpdateVisuals.Invoke(view, null);

            return true;
        }

        private static bool RemoveArenaOccupant(Arena arena, int slotIndex, CombatantItemData occupant)
        {
            bool removed = false;
            if (slotIndex == 0 && ReferenceEquals(arena.Slot1, occupant))
            {
                arena.Slot1 = null;
                removed = true;
            }
            else if (slotIndex == 1 && ReferenceEquals(arena.Slot2, occupant))
            {
                arena.Slot2 = null;
                removed = true;
            }

            if (removed)
            {
                arena.Fighting = false;
                arena.TicksRemaining = 0;
            }

            return removed;
        }

        private static bool RemoveInnOccupant(Inn inn, int slotIndex, InnSlot slot)
        {
            if (slotIndex < 0 || slotIndex >= inn.Contents.Length || !ReferenceEquals(inn.Contents[slotIndex], slot))
                return false;

            inn.Contents[slotIndex] = null;
            return true;
        }

        private static bool RemoveInnFood(Inn inn, int slotIndex, InventoryItem food)
        {
            if (slotIndex < 0 || slotIndex >= inn.Contents.Length)
                return false;

            InnSlot slot = inn.Contents[slotIndex];
            if (slot == null || !ReferenceEquals(slot.Food, food))
                return false;

            slot.Food = null;
            return true;
        }

        private static bool RemoveTrainingOccupant(TrainingGrounds training, int slotIndex, TrainingGroundsSlot slot)
        {
            if (slotIndex < 0 || slotIndex >= training.Contents.Length || !ReferenceEquals(training.Contents[slotIndex], slot))
                return false;

            training.Contents[slotIndex] = null;
            return true;
        }

        private static bool RemoveTrainingDummy(TrainingGrounds training, CombatantItemData dummy)
        {
            if (!ReferenceEquals(training.TrainingDummy, dummy))
                return false;

            training.TrainingDummy = null;
            return true;
        }

        private static bool RemoveOutfitterActive(Outfitter outfitter, InventoryItem human)
        {
            if (!ReferenceEquals(outfitter.CurrentHuman, human))
                return false;

            outfitter.CurrentHuman = null;
            outfitter.CurrentEquip = null;
            outfitter.Progress = 0f;
            outfitter.Queued = false;
            return true;
        }

        private static bool CancelActiveCraft(RecipeElement recipeElement)
        {
            if (recipeElement.Input == null)
                return false;

            recipeElement.Input = null;
            recipeElement.Progress = 0f;
            return true;
        }
    }
}
