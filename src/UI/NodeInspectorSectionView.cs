using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomNodeInspector.UI
{
    internal sealed class NodeInspectorSectionView
    {
        public GameObject Root { get; }

        public NodeInspectorSectionView(
            Transform parent,
            NodeInspectorTheme theme,
            string title,
            int itemCount,
            bool isExpanded,
            Action onToggle,
            Action onPurgeSection)
        {
            Root = new GameObject("Section_" + title, typeof(RectTransform), typeof(Image), typeof(Button), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            Root.transform.SetParent(parent, worldPositionStays: false);

            var le = Root.GetComponent<LayoutElement>();
            le.preferredHeight = 36f;
            le.minHeight = 36f;
            le.flexibleHeight = 0f;

            var bg = Root.GetComponent<Image>();
            bg.color = isExpanded
                ? new Color(62f / 255f, 44f / 255f, 54f / 255f, 1f)
                : NodeInspectorTheme.HeaderBar;

            var outline = Root.AddComponent<Outline>();
            outline.effectColor = NodeInspectorTheme.Divider;
            outline.effectDistance = new Vector2(1f, -1f);

            var button = Root.GetComponent<Button>();
            button.targetGraphic = bg;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.16f, 1.10f, 1.06f, 1f);
            colors.pressedColor = new Color(0.90f, 0.86f, 0.84f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            button.onClick.AddListener(() =>
            {
                NodeInspectorTheme.PlayClickSound();
                onToggle?.Invoke();
            });

            var hLayout = Root.GetComponent<HorizontalLayoutGroup>();
            hLayout.padding = new RectOffset(10, 8, 4, 4);
            hLayout.spacing = 10f;
            hLayout.childAlignment = TextAnchor.MiddleLeft;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = false;

            var chevIcon = new GameObject("Chevron", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            chevIcon.transform.SetParent(Root.transform, worldPositionStays: false);
            var chevLe = chevIcon.GetComponent<LayoutElement>();
            chevLe.preferredWidth = 16f;
            chevLe.minWidth = 16f;
            chevLe.preferredHeight = 16f;
            chevLe.minHeight = 16f;
            var chevImg = chevIcon.GetComponent<Image>();
            chevImg.sprite = isExpanded ? NodeInspectorIcons.ChevronDown : NodeInspectorIcons.ChevronRight;
            chevImg.color = NodeInspectorTheme.Gold;
            chevImg.preserveAspect = true;
            chevImg.raycastTarget = false;

            string noun = itemCount == 1 ? "item" : "items";
            var label = theme.CreateText(Root.transform, "Title", $"{title}  ({itemCount} {noun})", 15f, NodeInspectorTheme.Gold);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            var labelLe = label.gameObject.AddComponent<LayoutElement>();
            labelLe.flexibleWidth = 1f;
            labelLe.preferredHeight = 26f;

            var purgeBtn = theme.CreateTextButton(
                Root.transform,
                "PurgeSectionBtn",
                $"Purge {title}",
                12.5f,
                isDanger: true,
                onPurgeSection);
            var purgeLe = purgeBtn.gameObject.AddComponent<LayoutElement>();
            purgeLe.preferredWidth = Mathf.Clamp((title.Length + 6) * 7.8f + 18f, 96f, 134f);
            purgeLe.minWidth = 92f;
            purgeLe.preferredHeight = 26f;
            purgeLe.minHeight = 26f;
            purgeLe.flexibleHeight = 0f;
        }
    }
}
