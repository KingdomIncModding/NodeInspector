using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomNodeInspector.UI
{
    internal sealed class NodeInspectorConfirmDialog
    {
        private readonly GameObject overlay;
        private readonly TextMeshProUGUI titleText;
        private readonly TextMeshProUGUI bodyText;
        private readonly TextMeshProUGUI confirmLabel;

        private Action pendingConfirm;

        public bool IsVisible => overlay && overlay.activeSelf;

        public NodeInspectorConfirmDialog(Transform popupRoot, NodeInspectorTheme theme)
        {
            overlay = new GameObject("ConfirmOverlay", typeof(RectTransform), typeof(Image), typeof(Button));
            overlay.transform.SetParent(popupRoot, worldPositionStays: false);
            NodeInspectorTheme.StretchFull((RectTransform)overlay.transform);

            var dim = overlay.GetComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.72f);

            var dimButton = overlay.GetComponent<Button>();
            dimButton.transition = Selectable.Transition.None;
            dimButton.onClick.AddListener(Hide);

            var card = new GameObject("ConfirmCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(overlay.transform, worldPositionStays: false);
            var cardRect = (RectTransform)card.transform;
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(430f, 210f);

            theme.ApplyPanelChrome(cardRect, card.GetComponent<Image>());

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
            content.transform.SetParent(card.transform, worldPositionStays: false);
            var contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(20f, 18f);
            contentRect.offsetMax = new Vector2(-20f, -18f);

            var layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            titleText = theme.CreateText(content.transform, "Title", "Confirm Purge", 20f, NodeInspectorTheme.Gold);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            var titleLe = titleText.gameObject.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 28f;
            titleLe.minHeight = 28f;

            bodyText = theme.CreateText(content.transform, "Body", "", 15f, NodeInspectorTheme.Ink);
            bodyText.alignment = TextAlignmentOptions.Center;
            bodyText.textWrappingMode = TextWrappingModes.Normal;
            bodyText.overflowMode = TextOverflowModes.Overflow;
            var bodyLe = bodyText.gameObject.AddComponent<LayoutElement>();
            bodyLe.flexibleHeight = 1f;
            bodyLe.minHeight = 76f;

            var actions = new GameObject("Actions", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            actions.transform.SetParent(content.transform, worldPositionStays: false);
            var actionsLe = actions.GetComponent<LayoutElement>();
            actionsLe.preferredHeight = 38f;
            actionsLe.minHeight = 38f;

            var row = actions.GetComponent<HorizontalLayoutGroup>();
            row.spacing = 14f;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = true;
            row.childForceExpandHeight = true;

            theme.CreateTextButton(actions.transform, "CancelBtn", "Cancel", 16f, isDanger: false, Hide);
            var confirmBtn = theme.CreateTextButton(actions.transform, "ConfirmBtn", "Purge", 16f, isDanger: true, Confirm);
            confirmLabel = confirmBtn.GetComponentInChildren<TextMeshProUGUI>();

            overlay.SetActive(false);
        }

        public void Show(string title, string message, string confirmText, Action onConfirm)
        {
            pendingConfirm = onConfirm;
            titleText.text = string.IsNullOrWhiteSpace(title) ? "Confirm Action" : title;
            bodyText.text = message ?? "";
            if (confirmLabel)
                confirmLabel.text = string.IsNullOrWhiteSpace(confirmText) ? "Confirm" : confirmText;

            overlay.transform.SetAsLastSibling();
            overlay.SetActive(true);
        }

        public void Hide()
        {
            pendingConfirm = null;
            if (overlay)
                overlay.SetActive(false);
        }

        private void Confirm()
        {
            Action callback = pendingConfirm;
            Hide();
            callback?.Invoke();
        }
    }
}
