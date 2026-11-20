using System;
using System.Text;
using KingdomFoundation.Api;
using KingdomNodeInspector.Inspection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomNodeInspector.UI
{
    internal sealed class NodeInspectorEntryView
    {
        public GameObject Root { get; }

        public NodeInspectorEntryView(
            Transform parent,
            NodeInspectorTheme theme,
            NodeStoredItem entry,
            NodeItemDetails details,
            int rowIndex,
            bool isExpanded,
            Action onToggleExpand,
            Action onDelete)
        {
            Root = new GameObject("Entry_" + rowIndex, typeof(RectTransform), typeof(VerticalLayoutGroup));
            Root.transform.SetParent(parent, worldPositionStays: false);

            var rootLayout = Root.GetComponent<VerticalLayoutGroup>();
            rootLayout.spacing = 0f;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            BuildHeaderRow(theme, entry, details, rowIndex, isExpanded, onToggleExpand, onDelete);

            if (isExpanded)
                BuildDetailDrawer(theme, entry, details);
        }

        private void BuildHeaderRow(
            NodeInspectorTheme theme,
            NodeStoredItem entry,
            NodeItemDetails details,
            int rowIndex,
            bool isExpanded,
            Action onToggleExpand,
            Action onDelete)
        {
            var header = new GameObject("Header", typeof(RectTransform), typeof(Image), typeof(Button), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            header.transform.SetParent(Root.transform, worldPositionStays: false);

            var headerLe = header.GetComponent<LayoutElement>();
            headerLe.preferredHeight = 42f;
            headerLe.minHeight = 42f;

            var bg = header.GetComponent<Image>();
            bg.color = isExpanded
                ? NodeInspectorTheme.RowExpanded
                : rowIndex % 2 == 0 ? NodeInspectorTheme.RowEven : NodeInspectorTheme.RowOdd;

            var rowButton = header.GetComponent<Button>();
            rowButton.targetGraphic = bg;
            rowButton.navigation = new Navigation { mode = Navigation.Mode.None };
            rowButton.transition = Selectable.Transition.ColorTint;
            var colors = rowButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.16f, 1.12f, 1.08f, 1f);
            colors.pressedColor = new Color(0.92f, 0.88f, 0.86f, 1f);
            colors.selectedColor = colors.highlightedColor;
            rowButton.colors = colors;
            rowButton.onClick.AddListener(() =>
            {
                NodeInspectorTheme.PlayClickSound();
                onToggleExpand?.Invoke();
            });

            var hLayout = header.GetComponent<HorizontalLayoutGroup>();
            hLayout.padding = new RectOffset(22, 8, 5, 5);
            hLayout.spacing = 10f;
            hLayout.childAlignment = TextAnchor.MiddleLeft;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = false;

            BuildItemIcon(header.transform, details?.Icon);
            BuildNameBlock(header.transform, theme, entry, details);
            BuildRowActions(header.transform, theme, isExpanded, onToggleExpand, onDelete);

            var divider = new GameObject("Divider", typeof(RectTransform), typeof(Image));
            divider.transform.SetParent(header.transform, worldPositionStays: false);
            var divLe = divider.AddComponent<LayoutElement>();
            divLe.ignoreLayout = true;
            var divRect = (RectTransform)divider.transform;
            divRect.anchorMin = new Vector2(0f, 0f);
            divRect.anchorMax = new Vector2(1f, 0f);
            divRect.pivot = new Vector2(0.5f, 0f);
            divRect.sizeDelta = new Vector2(0f, 1f);
            var divImg = divider.GetComponent<Image>();
            divImg.color = NodeInspectorTheme.Divider;
            divImg.raycastTarget = false;
        }

        private static void BuildItemIcon(Transform parent, Sprite iconSprite)
        {
            var iconHolder = new GameObject("IconFrame", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconHolder.transform.SetParent(parent, worldPositionStays: false);
            var le = iconHolder.GetComponent<LayoutElement>();
            le.preferredWidth = 30f;
            le.minWidth = 30f;
            le.preferredHeight = 30f;
            le.minHeight = 30f;

            var frameImg = iconHolder.GetComponent<Image>();
            frameImg.color = NodeInspectorTheme.Well;
            frameImg.raycastTarget = false;

            var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(iconHolder.transform, worldPositionStays: false);
            var iconRect = (RectTransform)iconObj.transform;
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            var img = iconObj.GetComponent<Image>();
            img.sprite = iconSprite;
            img.enabled = iconSprite != null;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }

        private static void BuildNameBlock(Transform parent, NodeInspectorTheme theme, NodeStoredItem entry, NodeItemDetails details)
        {
            var block = new GameObject("NameBlock", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            block.transform.SetParent(parent, worldPositionStays: false);
            var le = block.GetComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            le.preferredHeight = 30f;

            var layout = block.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            string displayName = details != null && !string.IsNullOrWhiteSpace(details.DisplayName)
                ? details.DisplayName
                : entry.Item?.Type?.Identifier ?? "Unknown Item";

            var nameText = theme.CreateText(block.transform, "Name", displayName, 16f, NodeInspectorTheme.Ink);
            nameText.alignment = TextAlignmentOptions.MidlineLeft;
            nameText.textWrappingMode = TextWrappingModes.NoWrap;
            var nameLe = nameText.gameObject.AddComponent<LayoutElement>();
            nameLe.flexibleWidth = 1f;
            nameLe.minWidth = 100f;
        }

        private static void BuildRowActions(
            Transform parent,
            NodeInspectorTheme theme,
            bool isExpanded,
            Action onToggleExpand,
            Action onDelete)
        {
            var deleteBtn = theme.CreateIconButton(
                parent,
                "DeleteBtn",
                NodeInspectorIcons.Trash,
                NodeInspectorTheme.DangerBg,
                NodeInspectorTheme.DangerInk,
                new Color(204f / 255f, 92f / 255f, 82f / 255f, 0.9f),
                onDelete);
            var delLe = deleteBtn.gameObject.AddComponent<LayoutElement>();
            delLe.preferredWidth = 28f;
            delLe.minWidth = 28f;
            delLe.preferredHeight = 28f;
            delLe.minHeight = 28f;

            var chevronBtn = theme.CreateIconButton(
                parent,
                "ChevronBtn",
                isExpanded ? NodeInspectorIcons.ChevronDown : NodeInspectorIcons.ChevronRight,
                NodeInspectorTheme.Well,
                NodeInspectorTheme.Gold,
                NodeInspectorTheme.Brass,
                onToggleExpand);
            var chevLe = chevronBtn.gameObject.AddComponent<LayoutElement>();
            chevLe.preferredWidth = 28f;
            chevLe.minWidth = 28f;
            chevLe.preferredHeight = 28f;
            chevLe.minHeight = 28f;
        }

        private void BuildDetailDrawer(NodeInspectorTheme theme, NodeStoredItem entry, NodeItemDetails details)
        {
            var drawer = new GameObject("DetailDrawer", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            drawer.transform.SetParent(Root.transform, worldPositionStays: false);
            drawer.GetComponent<Image>().color = NodeInspectorTheme.Drawer;

            var hLayout = drawer.GetComponent<HorizontalLayoutGroup>();
            hLayout.padding = new RectOffset(34, 14, 10, 12);
            hLayout.spacing = 14f;
            hLayout.childAlignment = TextAnchor.UpperLeft;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = true;

            var guide = new GameObject("GuideLine", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            guide.transform.SetParent(drawer.transform, worldPositionStays: false);
            var guideLe = guide.GetComponent<LayoutElement>();
            guideLe.preferredWidth = 3f;
            guideLe.minWidth = 3f;
            var guideImg = guide.GetComponent<Image>();
            guideImg.color = NodeInspectorTheme.Brass;
            guideImg.raycastTarget = false;

            var body = new GameObject("DetailsBody", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            body.transform.SetParent(drawer.transform, worldPositionStays: false);
            var bodyLe = body.GetComponent<LayoutElement>();
            bodyLe.flexibleWidth = 1f;

            var bodyLayout = body.GetComponent<VerticalLayoutGroup>();
            bodyLayout.spacing = 6f;
            bodyLayout.childControlWidth = true;
            bodyLayout.childControlHeight = true;
            bodyLayout.childForceExpandWidth = true;
            bodyLayout.childForceExpandHeight = false;

            BuildMetadataLine(body.transform, theme, entry, details);

            if (details == null)
                return;

            if (details.IsCombatant)
                BuildCombatantDetails(body.transform, theme, details);
            else if (details.IsEquipment)
                BuildEquipmentDetails(body.transform, theme, details);
            else
                BuildStandardItemDetails(body.transform, theme, details);
        }

        private static void BuildMetadataLine(Transform parent, NodeInspectorTheme theme, NodeStoredItem entry, NodeItemDetails details)
        {
            var sb = new StringBuilder();
            sb.Append($"<color=#E2BD77>Location:</color> {entry.SlotLabel}");

            if (!string.IsNullOrEmpty(entry.ProgressSummary))
                sb.Append($"  •  <color=#E2BD77>{entry.ProgressSummary}</color>");

            if (details != null)
            {
                int qualityPct = Mathf.RoundToInt(details.Quality * 100f);
                string qualityHex = qualityPct > 100 ? "#8AD680" : qualityPct < 100 ? "#E86C60" : "#EADFCB";
                sb.Append($"  •  Quality: <color={qualityHex}>{qualityPct}%</color>");

                if (details.CanSell && details.SellValue > 0)
                    sb.Append($"  •  Value: <color=#E2BD77>{FormatValue(details.SellValue)}g</color>");

                if (details.ManaValue > 0)
                    sb.Append($"  •  Mana: <color=#8CD6F2>{FormatValue(details.ManaValue)}</color>");
            }

            AddWrappedLine(parent, theme, sb.ToString(), 13.5f, NodeInspectorTheme.SecondaryInk);
        }

        private static void BuildCombatantDetails(Transform parent, NodeInspectorTheme theme, NodeItemDetails details)
        {
            BuildProgressBarRow(
                parent,
                theme,
                "Health",
                details.Health01,
                NodeInspectorTheme.GetHealthColor(details.Health01),
                $"{FormatValue(details.CurrentHealth)} / {FormatValue(details.MaxHealth)} ({Mathf.RoundToInt(details.Health01 * 100f)}%)");

            if (details.MaxEnergy > 0)
            {
                BuildProgressBarRow(
                    parent,
                    theme,
                    "Energy",
                    details.Energy01,
                    NodeInspectorTheme.EnergyBar,
                    $"{FormatValue(details.CurrentEnergy)} / {FormatValue(details.MaxEnergy)} ({Mathf.RoundToInt(details.Energy01 * 100f)}%)");
            }

            var status = new StringBuilder();
            status.Append($"<color=#E2BD77>Power:</color> <color=#8AD680>{FormatValue(details.CombatPower)}</color>");
            status.Append($"   <color=#E2BD77>Armor:</color> {FormatValue(details.Armor)}");
            if (details.Level > 0)
                status.Append($"   <color=#E2BD77>Level:</color> {details.Level}");
            if (!string.IsNullOrEmpty(details.Archetype))
                status.Append($"   <color=#E2BD77>Class:</color> {details.Archetype}");

            if (details.IsHuman)
            {
                if (details.TrainingCount > 0)
                    status.Append($"   Training: {details.TrainingCount}");
                if (details.QuestCount > 0)
                    status.Append($"   Quests: {details.QuestCount}");
                if (details.ArenaCount > 0)
                    status.Append($"   Arena: {details.ArenaCount}");
            }

            if (details.IsDead)
                status.Append("   <color=#E86C60>[DEAD]</color>");
            if (details.IsExhausted)
                status.Append("   <color=#E8AE54>[EXHAUSTED]</color>");
            if (details.HasLimitBreak)
                status.Append("   <color=#C792EA>[LIMIT BREAK]</color>");

            AddWrappedLine(parent, theme, status.ToString(), 14f, NodeInspectorTheme.Ink);

            if (details.Stats.Count > 0)
            {
                var statLine = new StringBuilder("<color=#C4B59B>Stats:</color> ");
                for (int i = 0; i < details.Stats.Count; i++)
                {
                    if (i > 0)
                        statLine.Append("  •  ");
                    statLine.Append(details.Stats[i].FormattedLine);
                }

                AddWrappedLine(parent, theme, statLine.ToString(), 13f, NodeInspectorTheme.SecondaryInk);
            }

            if (details.Traits.Count > 0)
            {
                AddSectionLabel(parent, theme, "Traits");
                for (int i = 0; i < details.Traits.Count; i++)
                {
                    var trait = details.Traits[i];
                    string text = string.IsNullOrWhiteSpace(trait.Description)
                        ? $"<color=#E2BD77>{trait.Name}</color>"
                        : $"<color=#E2BD77>{trait.Name}:</color> {trait.Description}";
                    BuildIconDetailLine(parent, theme, trait.Icon, text, 13f, NodeInspectorTheme.Ink);
                }
            }

            if (details.IsHuman)
            {
                AddSectionLabel(parent, theme, $"Equipped Gear ({details.EquippedItems.Count})");
                if (details.EquippedItems.Count == 0)
                {
                    AddWrappedLine(parent, theme, "No equipment worn.", 13f, NodeInspectorTheme.MutedInk);
                }
                else
                {
                    for (int i = 0; i < details.EquippedItems.Count; i++)
                    {
                        var eq = details.EquippedItems[i];
                        var eqText = new StringBuilder();
                        eqText.Append(eq.DisplayName);
                        int eqQual = Mathf.RoundToInt(eq.Quality * 100f);
                        if (eqQual != 100)
                        {
                            string qHex = eqQual > 100 ? "#8AD680" : "#E86C60";
                            eqText.Append($" <color={qHex}>({eqQual}%)</color>");
                        }

                        if (!string.IsNullOrWhiteSpace(eq.SlotDescription))
                            eqText.Append($" <color=#988B7C>— {eq.SlotDescription}</color>");

                        if (eq.Enhancements.Count > 0)
                        {
                            eqText.Append("  <color=#8AD680>[");
                            for (int e = 0; e < eq.Enhancements.Count; e++)
                            {
                                if (e > 0)
                                    eqText.Append(", ");
                                eqText.Append(eq.Enhancements[e].Text);
                            }

                            eqText.Append("]</color>");
                        }

                        BuildIconDetailLine(parent, theme, eq.Icon, eqText.ToString(), 13f, NodeInspectorTheme.Ink);
                    }
                }
            }
        }

        private static void BuildEquipmentDetails(Transform parent, NodeInspectorTheme theme, NodeItemDetails details)
        {
            if (!string.IsNullOrWhiteSpace(details.SlotDescription))
                AddWrappedLine(parent, theme, $"<color=#E2BD77>{details.SlotDescription}</color>", 14f, NodeInspectorTheme.Gold);

            if (details.Stats.Count > 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < details.Stats.Count; i++)
                {
                    if (i > 0)
                        sb.Append("   •   ");
                    sb.Append($"<color=#8AD680>{details.Stats[i].FormattedLine}</color>");
                }

                AddWrappedLine(parent, theme, sb.ToString(), 13.5f, NodeInspectorTheme.Good);
            }

            if (details.Enhancements.Count > 0)
            {
                AddSectionLabel(parent, theme, "Enhancements");
                for (int i = 0; i < details.Enhancements.Count; i++)
                {
                    var enh = details.Enhancements[i];
                    BuildIconDetailLine(parent, theme, enh.Icon, enh.Text, 13f, NodeInspectorTheme.Good);
                }
            }

            if (details.Traits.Count > 0)
            {
                AddSectionLabel(parent, theme, "Trait");
                for (int i = 0; i < details.Traits.Count; i++)
                {
                    var trait = details.Traits[i];
                    string text = string.IsNullOrWhiteSpace(trait.Description)
                        ? $"<color=#E2BD77>{trait.Name}</color>"
                        : $"<color=#E2BD77>{trait.Name}:</color> {trait.Description}";
                    BuildIconDetailLine(parent, theme, trait.Icon, text, 13f, NodeInspectorTheme.Ink);
                }
            }

            BuildStandardItemDetails(parent, theme, details);
        }

        private static void BuildStandardItemDetails(Transform parent, NodeInspectorTheme theme, NodeItemDetails details)
        {
            if (!string.IsNullOrWhiteSpace(details.Description))
                AddWrappedLine(parent, theme, details.Description, 13.5f, NodeInspectorTheme.Ink);

            if (details.Sources.Count > 0)
                AddWrappedLine(parent, theme, $"<color=#988B7C>Sources: {string.Join(", ", details.Sources)}</color>", 12.5f, NodeInspectorTheme.MutedInk);
        }

        private static void BuildProgressBarRow(
            Transform parent,
            NodeInspectorTheme theme,
            string label,
            float fill01,
            Color fillColor,
            string valueText)
        {
            var row = new GameObject(label + "BarRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, worldPositionStays: false);
            var rowLe = row.GetComponent<LayoutElement>();
            rowLe.preferredHeight = 20f;
            rowLe.minHeight = 20f;

            var hLayout = row.GetComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 8f;
            hLayout.childAlignment = TextAnchor.MiddleLeft;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = true;

            var lbl = theme.CreateText(row.transform, "Label", label + ":", 13f, NodeInspectorTheme.SecondaryInk);
            lbl.alignment = TextAlignmentOptions.MidlineLeft;
            var lblLe = lbl.gameObject.AddComponent<LayoutElement>();
            lblLe.preferredWidth = 58f;
            lblLe.minWidth = 58f;

            var track = new GameObject("Track", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement));
            track.transform.SetParent(row.transform, worldPositionStays: false);
            var trackLe = track.GetComponent<LayoutElement>();
            trackLe.flexibleWidth = 1f;
            trackLe.preferredHeight = 16f;
            track.GetComponent<Image>().color = NodeInspectorTheme.Well;
            var trackOutline = track.GetComponent<Outline>();
            trackOutline.effectColor = NodeInspectorTheme.Divider;
            trackOutline.effectDistance = new Vector2(1f, -1f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(track.transform, worldPositionStays: false);
            var fillRect = (RectTransform)fill.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(Mathf.Clamp01(fill01), 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImg = fill.GetComponent<Image>();
            fillImg.color = fillColor;
            fillImg.raycastTarget = false;

            var val = theme.CreateText(track.transform, "Value", valueText, 12f, Color.white);
            val.alignment = TextAlignmentOptions.Center;
            NodeInspectorTheme.StretchFull(val.rectTransform);
        }

        private static void BuildIconDetailLine(
            Transform parent,
            NodeInspectorTheme theme,
            Sprite icon,
            string text,
            float fontSize,
            Color color)
        {
            var row = new GameObject("IconLine", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, worldPositionStays: false);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            if (icon != null)
            {
                var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                iconObj.transform.SetParent(row.transform, worldPositionStays: false);
                var iconLe = iconObj.GetComponent<LayoutElement>();
                iconLe.preferredWidth = 18f;
                iconLe.minWidth = 18f;
                iconLe.preferredHeight = 18f;
                iconLe.minHeight = 18f;

                var img = iconObj.GetComponent<Image>();
                img.sprite = icon;
                img.preserveAspect = true;
                img.raycastTarget = false;
            }

            var txt = theme.CreateText(row.transform, "Text", text, fontSize, color);
            txt.alignment = TextAlignmentOptions.TopLeft;
            txt.textWrappingMode = TextWrappingModes.Normal;
            txt.overflowMode = TextOverflowModes.Overflow;
            var txtLe = txt.gameObject.AddComponent<LayoutElement>();
            txtLe.flexibleWidth = 1f;
        }

        private static void AddSectionLabel(Transform parent, NodeInspectorTheme theme, string label)
        {
            var text = theme.CreateText(parent, "Section_" + label, label, 13.5f, NodeInspectorTheme.Gold);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            var le = text.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 20f;
            le.minHeight = 20f;
        }

        private static void AddWrappedLine(Transform parent, NodeInspectorTheme theme, string value, float fontSize, Color color)
        {
            var text = theme.CreateText(parent, "Line", value, fontSize, color);
            text.alignment = TextAlignmentOptions.TopLeft;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
        }

        private static string FormatValue(long value)
        {
            try
            {
                return FoundationApi.UiAccess.FormatNumber(value);
            }
            catch
            {
                return value.ToString("N0");
            }
        }
    }
}
