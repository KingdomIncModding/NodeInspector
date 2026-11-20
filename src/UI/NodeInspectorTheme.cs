using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Behaviour.UI.MainMenu;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomNodeInspector.UI
{
    internal sealed class NodeInspectorTheme
    {
        public static readonly Color Backdrop = new Color(0f, 0f, 0f, 0.56f);
        public static readonly Color Panel = new Color(38f / 255f, 26f / 255f, 35f / 255f, 0.99f);
        public static readonly Color HeaderBar = new Color(49f / 255f, 32f / 255f, 42f / 255f, 1f);
        public static readonly Color Toolbar = new Color(31f / 255f, 21f / 255f, 29f / 255f, 1f);
        public static readonly Color Well = new Color(24f / 255f, 16f / 255f, 24f / 255f, 1f);
        public static readonly Color RowEven = new Color(46f / 255f, 36f / 255f, 44f / 255f, 1f);
        public static readonly Color RowOdd = new Color(39f / 255f, 30f / 255f, 38f / 255f, 1f);
        public static readonly Color RowExpanded = new Color(56f / 255f, 42f / 255f, 48f / 255f, 1f);
        public static readonly Color Drawer = new Color(29f / 255f, 21f / 255f, 28f / 255f, 1f);
        public static readonly Color Ink = new Color(234f / 255f, 223f / 255f, 203f / 255f, 1f);
        public static readonly Color SecondaryInk = new Color(196f / 255f, 181f / 255f, 155f / 255f, 1f);
        public static readonly Color MutedInk = new Color(152f / 255f, 139f / 255f, 124f / 255f, 1f);
        public static readonly Color Gold = new Color(226f / 255f, 189f / 255f, 119f / 255f, 1f);
        public static readonly Color Brass = new Color(174f / 255f, 130f / 255f, 73f / 255f, 1f);
        public static readonly Color Divider = new Color(84f / 255f, 65f / 255f, 66f / 255f, 0.9f);
        public static readonly Color DangerBg = new Color(128f / 255f, 36f / 255f, 36f / 255f, 1f);
        public static readonly Color DangerHover = new Color(165f / 255f, 48f / 255f, 46f / 255f, 1f);
        public static readonly Color DangerInk = new Color(252f / 255f, 222f / 255f, 216f / 255f, 1f);
        public static readonly Color Good = new Color(138f / 255f, 214f / 255f, 128f / 255f, 1f);
        public static readonly Color Warn = new Color(232f / 255f, 174f / 255f, 84f / 255f, 1f);
        public static readonly Color Bad = new Color(232f / 255f, 108f / 255f, 96f / 255f, 1f);
        public static readonly Color EnergyBar = new Color(236f / 255f, 158f / 255f, 64f / 255f, 1f);
        public static readonly Color ButtonNeutral = new Color(58f / 255f, 44f / 255f, 50f / 255f, 1f);

        public TMP_FontAsset Font { get; private set; }
        public Material FontMaterial { get; private set; }
        public Sprite PanelFrameSprite { get; private set; }
        public Sprite WellBorderSprite { get; private set; }
        public Sprite ButtonSprite { get; private set; }
        public Sprite ScrollHandleSprite { get; private set; }

        public void ResolveNativeAssets()
        {
            ResolveFont();
            ResolveSprites();
        }

        public void ApplyPanelChrome(RectTransform panelRect, Image panelImage)
        {
            panelImage.sprite = null;
            panelImage.type = Image.Type.Simple;
            panelImage.color = Panel;
            panelImage.raycastTarget = true;

            if (PanelFrameSprite)
            {
                var frameObj = new GameObject("NativeFrame", typeof(RectTransform), typeof(Image));
                frameObj.transform.SetParent(panelRect, worldPositionStays: false);
                var frameRect = (RectTransform)frameObj.transform;
                StretchFull(frameRect);
                var frameImage = frameObj.GetComponent<Image>();
                frameImage.sprite = PanelFrameSprite;
                frameImage.type = Image.Type.Sliced;
                frameImage.color = Color.white;
                frameImage.raycastTarget = false;
            }
            else
            {
                var outline = panelRect.gameObject.AddComponent<Outline>();
                outline.effectColor = Brass;
                outline.effectDistance = new Vector2(2f, -2f);
            }
        }

        public void ApplyWellChrome(RectTransform wellRect, Image wellImage)
        {
            wellImage.sprite = null;
            wellImage.type = Image.Type.Simple;
            wellImage.color = Well;
            wellImage.raycastTarget = true;

            if (WellBorderSprite)
            {
                var borderObj = new GameObject("WellBorder", typeof(RectTransform), typeof(Image));
                borderObj.transform.SetParent(wellRect, worldPositionStays: false);
                var borderRect = (RectTransform)borderObj.transform;
                StretchFull(borderRect);
                var borderImage = borderObj.GetComponent<Image>();
                borderImage.sprite = WellBorderSprite;
                borderImage.type = Image.Type.Sliced;
                borderImage.color = Color.white;
                borderImage.raycastTarget = false;
            }
            else
            {
                var outline = wellRect.gameObject.AddComponent<Outline>();
                outline.effectColor = Divider;
                outline.effectDistance = new Vector2(1f, -1f);
            }
        }

        public Button CreateTextButton(
            Transform parent,
            string name,
            string label,
            float fontSize,
            bool isDanger,
            Action onClick)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            obj.transform.SetParent(parent, worldPositionStays: false);

            var image = obj.GetComponent<Image>();
            image.sprite = null;
            image.type = Image.Type.Simple;
            image.color = isDanger ? DangerBg : ButtonNeutral;

            var outline = obj.GetComponent<Outline>();
            outline.effectColor = isDanger
                ? new Color(214f / 255f, 102f / 255f, 88f / 255f, 0.95f)
                : Brass;
            outline.effectDistance = new Vector2(1f, -1f);

            var button = obj.GetComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = isDanger ? new Color(1.22f, 1.14f, 1.14f, 1f) : new Color(1.22f, 1.14f, 1.02f, 1f);
            colors.pressedColor = isDanger ? new Color(0.82f, 0.72f, 0.72f, 1f) : new Color(0.82f, 0.74f, 0.64f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.55f, 0.52f, 0.50f, 0.45f);
            button.colors = colors;

            button.onClick.AddListener(() =>
            {
                ClearSelection();
                PlayClickSound();
                onClick?.Invoke();
            });

            var text = CreateText(obj.transform, "Label", label, fontSize, isDanger ? DangerInk : Ink);
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            StretchFull(text.rectTransform);
            text.margin = new Vector4(6f, 2f, 6f, 2f);

            return button;
        }

        public Button CreateIconButton(
            Transform parent,
            string name,
            Sprite iconSprite,
            Color backgroundColor,
            Color iconTint,
            Color borderColor,
            Action onClick)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            obj.transform.SetParent(parent, worldPositionStays: false);

            var bg = obj.GetComponent<Image>();
            bg.color = backgroundColor;

            var outline = obj.GetComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(1f, -1f);

            var button = obj.GetComponent<Button>();
            button.targetGraphic = bg;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.12f, 1.05f, 1f);
            colors.pressedColor = new Color(0.78f, 0.72f, 0.68f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.45f);
            button.colors = colors;

            button.onClick.AddListener(() =>
            {
                ClearSelection();
                PlayClickSound();
                onClick?.Invoke();
            });

            var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(obj.transform, worldPositionStays: false);
            var iconRect = (RectTransform)iconObj.transform;
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(16f, 16f);

            var iconImage = iconObj.GetComponent<Image>();
            iconImage.sprite = iconSprite;
            iconImage.color = iconTint;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            return button;
        }

        public TextMeshProUGUI CreateText(Transform parent, string name, string value, float fontSize, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, worldPositionStays: false);

            var text = obj.GetComponent<TextMeshProUGUI>();
            if (Font != null)
            {
                text.font = Font;
                if (FontMaterial != null)
                    text.fontSharedMaterial = FontMaterial;
            }

            text.fontSize = fontSize;
            text.color = color;
            text.text = value ?? "";
            text.raycastTarget = false;
            text.overflowMode = TextOverflowModes.Ellipsis;
            return text;
        }

        public static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static Color GetHealthColor(float health01)
        {
            if (health01 >= 0.65f)
                return Good;
            if (health01 >= 0.30f)
                return Warn;
            return Bad;
        }

        public static void PlayClickSound()
        {
            try
            {
                FoundationApi.UiAccess.PlaySound(GameUiSound.Button);
            }
            catch
            {
            }
        }

        private static void ClearSelection()
        {
            if (EventSystem.current)
                EventSystem.current.SetSelectedGameObject(null);
        }

        private void ResolveFont()
        {
            if (Font != null)
                return;

            var options = Resources.FindObjectsOfTypeAll<OptionsUI>().FirstOrDefault();
            if (options)
            {
                var sampleText = options.GetComponentInChildren<TMP_Text>(true);
                if (sampleText && sampleText.font)
                {
                    Font = sampleText.font;
                    FontMaterial = sampleText.fontSharedMaterial;
                    return;
                }
            }

            var anyText = Resources.FindObjectsOfTypeAll<TMP_Text>().FirstOrDefault(t => t && t.font);
            if (anyText)
            {
                Font = anyText.font;
                FontMaterial = anyText.fontSharedMaterial;
                return;
            }

            Font = TMP_Settings.defaultFontAsset;
        }

        private void ResolveSprites()
        {
            var spriteMap = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            foreach (Sprite sprite in Resources.FindObjectsOfTypeAll<Sprite>())
            {
                if (sprite && !spriteMap.ContainsKey(sprite.name))
                    spriteMap.Add(sprite.name, sprite);
            }

            PanelFrameSprite = FindNamedSprite(spriteMap, "DecoRectBorder")
                ?? FindNamedSprite(spriteMap, "DefaultBorderDark");
            WellBorderSprite = FindNamedSprite(spriteMap, "DefaultBorderDark")
                ?? FindNamedSprite(spriteMap, "SimpleBox")
                ?? FindNamedSprite(spriteMap, "SelectBox");
            ScrollHandleSprite = FindNamedSprite(spriteMap, "ScrollHandle");

            var options = Resources.FindObjectsOfTypeAll<OptionsUI>().FirstOrDefault();
            if (options)
            {
                var nativeButton = options.GetComponentsInChildren<Button>(true)
                    .FirstOrDefault(b => b && (b.name == "Apply" || b.name == "Cancel" || b.name == "KeyBindings"));
                var btnImg = nativeButton ? nativeButton.GetComponent<Image>() : null;
                if (btnImg && btnImg.sprite)
                    ButtonSprite = btnImg.sprite;
            }

            if (!ButtonSprite)
                ButtonSprite = FindNamedSprite(spriteMap, "Button02") ?? FindNamedSprite(spriteMap, "Button03");
        }

        private static Sprite FindNamedSprite(Dictionary<string, Sprite> spriteMap, string name)
        {
            if (spriteMap.TryGetValue(name, out Sprite found) && found)
                return found;

            try
            {
                return FoundationApi.Presentation.FindSprite(name);
            }
            catch
            {
                return null;
            }
        }
    }
}
