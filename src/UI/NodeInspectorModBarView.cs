using System;
using KingdomFoundation.Api;
using KingdomNodeInspector.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomNodeInspector.UI
{
    internal sealed class NodeInspectorModBarView : IModBarView
    {
        private static readonly Color ActiveTint = new Color(0.95f, 0.78f, 0.36f, 1f);
        private static readonly Color IdleTint = new Color(0.84f, 0.80f, 0.74f, 1f);

        private readonly NodeInspectorPlugin plugin;
        private readonly NodeInspectorFoundationUi ui;
        private readonly GameObject root;
        private readonly Button button;
        private readonly Image iconImage;
        private readonly TMP_Text label;

        public RectTransform Root => root ? (RectTransform)root.transform : null;

        public NodeInspectorModBarView(IModBarContext context, NodeInspectorPlugin plugin, NodeInspectorFoundationUi ui)
        {
            this.plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            this.ui = ui ?? throw new ArgumentNullException(nameof(ui));

            button = context.CreateButton("Inspect", HandleClicked);
            root = button.gameObject;
            label = button.GetComponentInChildren<TMP_Text>(true);

            var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconObj.transform.SetParent(root.transform, worldPositionStays: false);
            iconObj.transform.SetAsFirstSibling();
            var le = iconObj.GetComponent<LayoutElement>();
            le.preferredWidth = 16f;
            le.preferredHeight = 16f;
            le.minWidth = 16f;
            le.minHeight = 16f;

            iconImage = iconObj.GetComponent<Image>();
            iconImage.sprite = NodeInspectorIcons.Inspect;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            Refresh();
        }

        public void Refresh()
        {
            bool active = plugin.IsInspectPickMode || plugin.IsPopupOpen;
            if (iconImage)
                iconImage.color = active ? ActiveTint : IdleTint;

            if (label)
            {
                string keyHint = plugin.Settings.InspectKey != UnityEngine.InputSystem.Key.None
                    ? $" ({plugin.Settings.InspectKey})"
                    : "";
                label.text = plugin.IsInspectPickMode ? "Click Node..." : "Inspect" + keyHint;
                label.color = active ? ActiveTint : IdleTint;
            }
        }

        public void Dispose()
        {
            ui.ClearActiveBarView(this);
            if (root)
                UnityEngine.Object.Destroy(root);
        }

        private void HandleClicked()
        {
            plugin.ToggleInspectPickMode(fromUser: true);
        }
    }
}
