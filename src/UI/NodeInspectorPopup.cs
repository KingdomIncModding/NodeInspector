using System;
using System.Collections.Generic;
using Assets.Source.Flow;
using KingdomFoundation.Api;
using KingdomNodeInspector.Configuration;
using KingdomNodeInspector.Inspection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KingdomNodeInspector.UI
{
    internal sealed class NodeInspectorPopup : MonoBehaviour
    {
        private static readonly NodeStorageSection[] SectionOrder =
        {
            NodeStorageSection.Inputs,
            NodeStorageSection.Outputs,
            NodeStorageSection.Internal
        };

        private readonly HashSet<NodeStorageSection> expandedSections = new HashSet<NodeStorageSection>();
        private readonly HashSet<string> expandedKeys = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<GameObject> spawnedEntries = new List<GameObject>();

        private NodeInspectorSettings settings;
        private NodeInspectorTheme theme;
        private NodeInspectorConfirmDialog confirmDialog;
        private Action onClosed;

        private FlowElement targetElement;
        private float nextRefreshTime;

        private Image buildingIcon;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI subtitleText;
        private TextMeshProUGUI countText;
        private RectTransform listContent;
        private ScrollRect scrollRect;
        private Button expandCollapseBtn;
        private TextMeshProUGUI expandCollapseLabel;
        private Button purgeAllBtn;

        public FlowElement TargetElement => targetElement;
        public bool IsConfirmOpen => confirmDialog != null && confirmDialog.IsVisible;

        public static NodeInspectorPopup Open(FlowElement element, NodeInspectorSettings settings, Action onClosed)
        {
            Canvas rootCanvas = FindRootCanvas();
            if (!rootCanvas || element == null)
                return null;

            var backdrop = new GameObject(
                "NodeInspectorPopup",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            backdrop.transform.SetParent(rootCanvas.transform, worldPositionStays: false);
            backdrop.transform.SetAsLastSibling();

            NodeInspectorTheme.StretchFull((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = NodeInspectorTheme.Backdrop;

            var popup = backdrop.AddComponent<NodeInspectorPopup>();
            popup.onClosed = onClosed;
            popup.Initialize(element, settings);

            var backdropButton = backdrop.GetComponent<Button>();
            backdropButton.transition = Selectable.Transition.None;
            backdropButton.onClick.AddListener(popup.Close);

            FoundationApi.NativeUiFactory?.BlockWorkspaceZoom(backdrop);
            PlayWindowSound(open: true);
            return popup;
        }

        public void InspectTarget(FlowElement element)
        {
            if (element == null)
                return;

            confirmDialog?.Hide();
            targetElement = element;
            expandedSections.Clear();
            expandedKeys.Clear();
            RefreshNow();
            if (scrollRect)
                scrollRect.verticalNormalizedPosition = 1f;
        }

        public bool HandleEscape()
        {
            if (IsConfirmOpen)
            {
                confirmDialog.Hide();
                return true;
            }

            Close();
            return true;
        }

        public void Close()
        {
            PlayWindowSound(open: false);
            Action callback = onClosed;
            onClosed = null;
            callback?.Invoke();
            Destroy(gameObject);
        }

        public void RefreshNow()
        {
            if (targetElement == null || !NodeInventoryAccessor.IsInActiveWorkspace(targetElement))
            {
                Close();
                return;
            }

            nextRefreshTime = Time.unscaledTime + (settings != null ? settings.RefreshInterval : 3.0f);

            UpdateHeader();
            var items = NodeInventoryAccessor.CollectItems(targetElement);
            var grouped = GroupItemsBySection(items);
            PruneExpandedState(items, grouped);
            UpdateMassEditBar(items, grouped);
            RebuildList(items, grouped);
        }

        private void Update()
        {
            if (targetElement == null)
                return;

            if (!NodeInventoryAccessor.IsInActiveWorkspace(targetElement))
            {
                Close();
                return;
            }

            if (settings == null || !settings.AutoRefresh)
                return;

            if (IsConfirmOpen)
                return;

            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
                return;

            if (Time.unscaledTime >= nextRefreshTime)
                RefreshNow();
        }

        private void Initialize(FlowElement element, NodeInspectorSettings modSettings)
        {
            targetElement = element;
            settings = modSettings;
            theme = new NodeInspectorTheme();
            theme.ResolveNativeAssets();

            BuildWindow();
            RefreshNow();
        }

        private void BuildWindow()
        {
            var panel = new GameObject("Window", typeof(RectTransform), typeof(Image), typeof(Button));
            panel.transform.SetParent(transform, worldPositionStays: false);

            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(700f, 620f);

            var swallow = panel.GetComponent<Button>();
            swallow.transition = Selectable.Transition.None;
            swallow.navigation = new Navigation { mode = Navigation.Mode.None };

            theme.ApplyPanelChrome(panelRect, panel.GetComponent<Image>());

            var body = new GameObject("WindowBody", typeof(RectTransform), typeof(VerticalLayoutGroup));
            body.transform.SetParent(panel.transform, worldPositionStays: false);
            var bodyRect = (RectTransform)body.transform;
            bodyRect.anchorMin = Vector2.zero;
            bodyRect.anchorMax = Vector2.one;
            bodyRect.offsetMin = new Vector2(14f, 14f);
            bodyRect.offsetMax = new Vector2(-14f, -14f);

            var bodyLayout = body.GetComponent<VerticalLayoutGroup>();
            bodyLayout.spacing = 8f;
            bodyLayout.childControlWidth = true;
            bodyLayout.childControlHeight = true;
            bodyLayout.childForceExpandWidth = true;
            bodyLayout.childForceExpandHeight = false;

            BuildHeaderBar(body.transform);
            BuildMassEditBar(body.transform);
            BuildScrollList(body.transform);

            confirmDialog = new NodeInspectorConfirmDialog(panel.transform, theme);
        }

        private void BuildHeaderBar(Transform parent)
        {
            var bar = new GameObject("HeaderBar", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            bar.transform.SetParent(parent, worldPositionStays: false);
            var le = bar.GetComponent<LayoutElement>();
            le.preferredHeight = 62f;
            le.minHeight = 62f;
            le.flexibleHeight = 0f;

            bar.GetComponent<Image>().color = NodeInspectorTheme.HeaderBar;

            var layout = bar.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(12, 10, 8, 8);
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var iconFrame = new GameObject("BuildingIconFrame", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconFrame.transform.SetParent(bar.transform, worldPositionStays: false);
            var iconFrameLe = iconFrame.GetComponent<LayoutElement>();
            iconFrameLe.preferredWidth = 42f;
            iconFrameLe.minWidth = 42f;
            iconFrameLe.preferredHeight = 42f;
            iconFrameLe.minHeight = 42f;
            iconFrame.GetComponent<Image>().color = NodeInspectorTheme.Well;

            var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(iconFrame.transform, worldPositionStays: false);
            var iconRect = (RectTransform)iconObj.transform;
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            buildingIcon = iconObj.GetComponent<Image>();
            buildingIcon.preserveAspect = true;
            buildingIcon.raycastTarget = false;

            var titleCol = new GameObject("TitleCol", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            titleCol.transform.SetParent(bar.transform, worldPositionStays: false);
            var titleColLe = titleCol.GetComponent<LayoutElement>();
            titleColLe.flexibleWidth = 1f;
            titleColLe.preferredHeight = 46f;
            titleColLe.minHeight = 46f;

            var titleLayout = titleCol.GetComponent<VerticalLayoutGroup>();
            titleLayout.spacing = 2f;
            titleLayout.childAlignment = TextAnchor.MiddleLeft;
            titleLayout.childControlWidth = true;
            titleLayout.childControlHeight = true;
            titleLayout.childForceExpandWidth = true;
            titleLayout.childForceExpandHeight = false;

            titleText = theme.CreateText(titleCol.transform, "Title", "Node Inspector", 18f, NodeInspectorTheme.Gold);
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.textWrappingMode = TextWrappingModes.NoWrap;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            var titleLe = titleText.gameObject.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 26f;
            titleLe.minHeight = 24f;

            subtitleText = theme.CreateText(titleCol.transform, "Subtitle", "", 12.5f, NodeInspectorTheme.SecondaryInk);
            subtitleText.alignment = TextAlignmentOptions.MidlineLeft;
            subtitleText.textWrappingMode = TextWrappingModes.NoWrap;
            subtitleText.overflowMode = TextOverflowModes.Ellipsis;
            var subLe = subtitleText.gameObject.AddComponent<LayoutElement>();
            subLe.preferredHeight = 18f;
            subLe.minHeight = 16f;

            var refreshBtn = theme.CreateIconButton(
                bar.transform,
                "RefreshBtn",
                NodeInspectorIcons.Refresh,
                NodeInspectorTheme.ButtonNeutral,
                NodeInspectorTheme.Ink,
                NodeInspectorTheme.Brass,
                RefreshNow);
            var refLe = refreshBtn.gameObject.AddComponent<LayoutElement>();
            refLe.preferredWidth = 32f;
            refLe.minWidth = 32f;
            refLe.preferredHeight = 32f;
            refLe.minHeight = 32f;

            var closeBtn = theme.CreateIconButton(
                bar.transform,
                "CloseBtn",
                NodeInspectorIcons.Close,
                NodeInspectorTheme.ButtonNeutral,
                NodeInspectorTheme.Gold,
                NodeInspectorTheme.Brass,
                Close);
            var closeLe = closeBtn.gameObject.AddComponent<LayoutElement>();
            closeLe.preferredWidth = 32f;
            closeLe.minWidth = 32f;
            closeLe.preferredHeight = 32f;
            closeLe.minHeight = 32f;
        }

        private void BuildMassEditBar(Transform parent)
        {
            var bar = new GameObject("MassEditBar", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            bar.transform.SetParent(parent, worldPositionStays: false);
            var le = bar.GetComponent<LayoutElement>();
            le.preferredHeight = 42f;
            le.minHeight = 42f;
            le.flexibleHeight = 0f;

            bar.GetComponent<Image>().color = NodeInspectorTheme.Toolbar;

            var layout = bar.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 6, 6);
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            countText = theme.CreateText(bar.transform, "ItemCount", "0 items", 14f, NodeInspectorTheme.SecondaryInk);
            countText.alignment = TextAlignmentOptions.MidlineLeft;
            countText.textWrappingMode = TextWrappingModes.NoWrap;
            var countLe = countText.gameObject.AddComponent<LayoutElement>();
            countLe.preferredWidth = 110f;
            countLe.minWidth = 80f;
            countLe.preferredHeight = 30f;
            countLe.minHeight = 30f;

            var spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(bar.transform, worldPositionStays: false);
            var spacerLe = spacer.GetComponent<LayoutElement>();
            spacerLe.flexibleWidth = 1f;
            spacerLe.preferredHeight = 30f;

            expandCollapseBtn = theme.CreateTextButton(bar.transform, "ExpandAllBtn", "Expand All", 13f, isDanger: false, ToggleExpandAll);
            expandCollapseLabel = expandCollapseBtn.GetComponentInChildren<TextMeshProUGUI>();
            var expLe = expandCollapseBtn.gameObject.AddComponent<LayoutElement>();
            expLe.preferredWidth = 112f;
            expLe.minWidth = 104f;
            expLe.preferredHeight = 30f;
            expLe.minHeight = 30f;
            expLe.flexibleHeight = 0f;

            purgeAllBtn = theme.CreateTextButton(bar.transform, "PurgeAllBtn", "Purge All", 13.5f, isDanger: true, RequestPurgeAll);
            var purgeLe = purgeAllBtn.gameObject.AddComponent<LayoutElement>();
            purgeLe.preferredWidth = 106f;
            purgeLe.minWidth = 96f;
            purgeLe.preferredHeight = 30f;
            purgeLe.minHeight = 30f;
            purgeLe.flexibleHeight = 0f;
        }

        private void BuildScrollList(Transform parent)
        {
            var well = new GameObject("ListWell", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            well.transform.SetParent(parent, worldPositionStays: false);
            var wellLe = well.GetComponent<LayoutElement>();
            wellLe.flexibleHeight = 1f;
            wellLe.minHeight = 200f;

            theme.ApplyWellChrome((RectTransform)well.transform, well.GetComponent<Image>());

            var scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
            scrollObj.transform.SetParent(well.transform, worldPositionStays: false);
            var scrollRectTransform = (RectTransform)scrollObj.transform;
            scrollRectTransform.anchorMin = Vector2.zero;
            scrollRectTransform.anchorMax = Vector2.one;
            scrollRectTransform.offsetMin = new Vector2(4f, 4f);
            scrollRectTransform.offsetMax = new Vector2(-18f, -4f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewport.transform.SetParent(scrollObj.transform, worldPositionStays: false);
            NodeInspectorTheme.StretchFull((RectTransform)viewport.transform);
            viewport.GetComponent<Image>().color = Color.clear;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, worldPositionStays: false);
            listContent = (RectTransform)content.transform;
            listContent.anchorMin = new Vector2(0f, 1f);
            listContent.anchorMax = new Vector2(1f, 1f);
            listContent.pivot = new Vector2(0.5f, 1f);
            listContent.anchoredPosition = Vector2.zero;
            listContent.sizeDelta = Vector2.zero;

            var contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 4f;
            contentLayout.padding = new RectOffset(4, 4, 4, 4);
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            var fitter = content.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var scrollbar = BuildScrollbar(well.transform);

            scrollRect = scrollObj.GetComponent<ScrollRect>();
            scrollRect.viewport = (RectTransform)viewport.transform;
            scrollRect.content = listContent;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 36f;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        }

        private Scrollbar BuildScrollbar(Transform wellParent)
        {
            var sbObj = new GameObject("Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            sbObj.transform.SetParent(wellParent, worldPositionStays: false);
            var sbRect = (RectTransform)sbObj.transform;
            sbRect.anchorMin = new Vector2(1f, 0f);
            sbRect.anchorMax = new Vector2(1f, 1f);
            sbRect.pivot = new Vector2(1f, 0.5f);
            sbRect.offsetMin = new Vector2(-14f, 6f);
            sbRect.offsetMax = new Vector2(-4f, -6f);

            sbObj.GetComponent<Image>().color = new Color(18f / 255f, 12f / 255f, 18f / 255f, 1f);

            var slideArea = new GameObject("SlidingArea", typeof(RectTransform));
            slideArea.transform.SetParent(sbObj.transform, worldPositionStays: false);
            NodeInspectorTheme.StretchFull((RectTransform)slideArea.transform);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(slideArea.transform, worldPositionStays: false);
            NodeInspectorTheme.StretchFull((RectTransform)handle.transform);

            var handleImg = handle.GetComponent<Image>();
            if (theme.ScrollHandleSprite)
            {
                handleImg.sprite = theme.ScrollHandleSprite;
                handleImg.type = Image.Type.Sliced;
                handleImg.color = Color.white;
            }
            else
            {
                handleImg.color = NodeInspectorTheme.Brass;
            }

            var scrollbar = sbObj.GetComponent<Scrollbar>();
            scrollbar.handleRect = (RectTransform)handle.transform;
            scrollbar.targetGraphic = handleImg;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            return scrollbar;
        }

        private void UpdateHeader()
        {
            if (titleText)
                titleText.text = NodeInventoryAccessor.GetBuildingTitle(targetElement);

            if (subtitleText)
                subtitleText.text = NodeInventoryAccessor.GetBuildingSubtitle(targetElement);

            if (buildingIcon)
            {
                Sprite icon = targetElement.Icon;
                buildingIcon.sprite = icon;
                buildingIcon.enabled = icon != null;
            }
        }

        private void UpdateMassEditBar(
            IReadOnlyList<NodeStoredItem> items,
            Dictionary<NodeStorageSection, List<NodeStoredItem>> grouped)
        {
            if (countText)
                countText.text = items.Count == 1 ? "1 item" : $"{items.Count} items";

            if (purgeAllBtn)
                purgeAllBtn.interactable = items.Count > 0;

            if (expandCollapseBtn)
                expandCollapseBtn.interactable = items.Count > 0;

            if (expandCollapseLabel)
            {
                int activeSections = CountActiveSections(grouped);
                bool allSectionsOpen = activeSections > 0 && expandedSections.Count >= activeSections;
                expandCollapseLabel.text = allSectionsOpen ? "Collapse All" : "Expand All";
            }
        }

        private void RebuildList(
            IReadOnlyList<NodeStoredItem> items,
            Dictionary<NodeStorageSection, List<NodeStoredItem>> grouped)
        {
            float savedScroll = scrollRect ? scrollRect.verticalNormalizedPosition : 1f;

            for (int i = 0; i < spawnedEntries.Count; i++)
            {
                if (spawnedEntries[i])
                    Destroy(spawnedEntries[i]);
            }

            spawnedEntries.Clear();

            if (items.Count == 0)
            {
                var emptyObj = new GameObject("EmptyState", typeof(RectTransform), typeof(LayoutElement));
                emptyObj.transform.SetParent(listContent, worldPositionStays: false);
                var le = emptyObj.GetComponent<LayoutElement>();
                le.preferredHeight = 140f;
                le.minHeight = 140f;

                var emptyText = theme.CreateText(
                    emptyObj.transform,
                    "EmptyText",
                    "This node has no stored items right now.",
                    16f,
                    NodeInspectorTheme.MutedInk);
                emptyText.alignment = TextAlignmentOptions.Center;
                NodeInspectorTheme.StretchFull(emptyText.rectTransform);
                spawnedEntries.Add(emptyObj);
            }
            else
            {
                for (int s = 0; s < SectionOrder.Length; s++)
                {
                    NodeStorageSection section = SectionOrder[s];
                    List<NodeStoredItem> sectionItems = grouped[section];
                    if (sectionItems.Count == 0)
                        continue;

                    string sectionLabel = NodeInventoryAccessor.GetSectionLabel(section);
                    bool sectionExpanded = expandedSections.Contains(section);

                    var sectionHeader = new NodeInspectorSectionView(
                        listContent,
                        theme,
                        sectionLabel,
                        sectionItems.Count,
                        sectionExpanded,
                        () => ToggleSection(section),
                        () => RequestPurgeSection(section, sectionLabel, sectionItems.Count));

                    spawnedEntries.Add(sectionHeader.Root);

                    if (!sectionExpanded)
                        continue;

                    for (int i = 0; i < sectionItems.Count; i++)
                    {
                        NodeStoredItem entry = sectionItems[i];
                        NodeItemDetails details = NodeItemDetailsBuilder.Build(entry.Item);
                        bool isExpanded = expandedKeys.Contains(entry.StableKey);

                        var view = new NodeInspectorEntryView(
                            listContent,
                            theme,
                            entry,
                            details,
                            i,
                            isExpanded,
                            () => ToggleEntry(entry.StableKey),
                            () => RequestDeleteSingle(entry, details));

                        spawnedEntries.Add(view.Root);
                    }
                }
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(listContent);
            if (scrollRect)
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(savedScroll);
        }

        private void ToggleSection(NodeStorageSection section)
        {
            if (expandedSections.Contains(section))
                expandedSections.Remove(section);
            else
                expandedSections.Add(section);

            RefreshNow();
        }

        private void ToggleEntry(string stableKey)
        {
            if (expandedKeys.Contains(stableKey))
                expandedKeys.Remove(stableKey);
            else
                expandedKeys.Add(stableKey);

            RefreshNow();
        }

        private void ToggleExpandAll()
        {
            var items = NodeInventoryAccessor.CollectItems(targetElement);
            if (items.Count == 0)
                return;

            var grouped = GroupItemsBySection(items);
            int activeSections = CountActiveSections(grouped);
            bool allSectionsOpen = activeSections > 0 && expandedSections.Count >= activeSections;

            expandedSections.Clear();
            expandedKeys.Clear();

            if (!allSectionsOpen)
            {
                for (int i = 0; i < SectionOrder.Length; i++)
                {
                    if (grouped[SectionOrder[i]].Count > 0)
                        expandedSections.Add(SectionOrder[i]);
                }
            }

            RefreshNow();
        }

        private void RequestDeleteSingle(NodeStoredItem entry, NodeItemDetails details)
        {
            if (settings == null || settings.ConfirmSingleDelete)
            {
                string itemName = details?.DisplayName ?? entry.Item?.Type?.Identifier ?? "this item";
                confirmDialog.Show(
                    "Delete Item",
                    $"Permanently delete <color=#E2BD77>{itemName}</color> from <color=#E2BD77>{entry.SlotLabel}</color>?",
                    "Delete",
                    () => PerformDeleteSingle(entry));
                return;
            }

            PerformDeleteSingle(entry);
        }

        private void PerformDeleteSingle(NodeStoredItem entry)
        {
            if (targetElement == null)
                return;

            if (NodeInventoryAccessor.TryRemove(targetElement, entry))
                expandedKeys.Remove(entry.StableKey);

            RefreshNow();
        }

        private void RequestPurgeAll()
        {
            if (targetElement == null)
                return;

            var items = NodeInventoryAccessor.CollectItems(targetElement);
            if (items.Count == 0)
                return;

            string nodeName = NodeInventoryAccessor.GetBuildingTitle(targetElement);
            string noun = items.Count == 1 ? "item" : "items";
            confirmDialog.Show(
                "Confirm Mass Purge",
                $"Permanently purge all <color=#E86C60>{items.Count} {noun}</color> from <color=#E2BD77>{nodeName}</color>?\n\nThis removes every input, output, and internal entry in this node and cannot be undone.",
                $"Purge All ({items.Count})",
                PerformPurgeAll);
        }

        private void PerformPurgeAll()
        {
            if (targetElement == null)
                return;

            int removed = NodeInventoryAccessor.PurgeAll(targetElement);
            expandedSections.Clear();
            expandedKeys.Clear();
            RefreshNow();
            ShowStatus($"Node Inspector: Purged {removed} {(removed == 1 ? "item" : "items")} from {NodeInventoryAccessor.GetBuildingTitle(targetElement)}.");
        }

        private void RequestPurgeSection(NodeStorageSection section, string sectionLabel, int count)
        {
            if (targetElement == null || count <= 0)
                return;

            string nodeName = NodeInventoryAccessor.GetBuildingTitle(targetElement);
            string noun = count == 1 ? "item" : "items";
            confirmDialog.Show(
                $"Confirm Purge {sectionLabel}",
                $"Permanently purge all <color=#E86C60>{count} {noun}</color> in <color=#E2BD77>{sectionLabel}</color> from <color=#E2BD77>{nodeName}</color>?\n\nThis action cannot be undone.",
                $"Purge {sectionLabel}",
                () => PerformPurgeSection(section, sectionLabel));
        }

        private void PerformPurgeSection(NodeStorageSection section, string sectionLabel)
        {
            if (targetElement == null)
                return;

            int removed = NodeInventoryAccessor.PurgeBySection(targetElement, section);
            expandedSections.Remove(section);
            RefreshNow();
            ShowStatus($"Node Inspector: Purged {removed} {sectionLabel.ToLowerInvariant()} {(removed == 1 ? "item" : "items")} from {NodeInventoryAccessor.GetBuildingTitle(targetElement)}.");
        }

        private static Dictionary<NodeStorageSection, List<NodeStoredItem>> GroupItemsBySection(IReadOnlyList<NodeStoredItem> items)
        {
            var grouped = new Dictionary<NodeStorageSection, List<NodeStoredItem>>
            {
                [NodeStorageSection.Inputs] = new List<NodeStoredItem>(),
                [NodeStorageSection.Outputs] = new List<NodeStoredItem>(),
                [NodeStorageSection.Internal] = new List<NodeStoredItem>()
            };

            for (int i = 0; i < items.Count; i++)
            {
                NodeStorageSection section = NodeInventoryAccessor.GetSection(items[i].Kind);
                grouped[section].Add(items[i]);
            }

            return grouped;
        }

        private static int CountActiveSections(Dictionary<NodeStorageSection, List<NodeStoredItem>> grouped)
        {
            int count = 0;
            for (int i = 0; i < SectionOrder.Length; i++)
            {
                if (grouped[SectionOrder[i]].Count > 0)
                    count++;
            }

            return count;
        }

        private void PruneExpandedState(
            IReadOnlyList<NodeStoredItem> items,
            Dictionary<NodeStorageSection, List<NodeStoredItem>> grouped)
        {
            for (int i = 0; i < SectionOrder.Length; i++)
            {
                if (grouped[SectionOrder[i]].Count == 0)
                    expandedSections.Remove(SectionOrder[i]);
            }

            if (expandedKeys.Count == 0)
                return;

            var live = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < items.Count; i++)
                live.Add(items[i].StableKey);

            expandedKeys.IntersectWith(live);
        }

        private static Canvas FindRootCanvas()
        {
            if (FoundationApi.UiAccess.Root)
            {
                var fromRoot = FoundationApi.UiAccess.Root.GetComponentInParent<Canvas>()?.rootCanvas;
                if (fromRoot)
                    return fromRoot;
            }

            return FindAnyObjectByType<Canvas>()?.rootCanvas;
        }

        private static void PlayWindowSound(bool open)
        {
            try
            {
                FoundationApi.UiAccess.PlaySound(open ? GameUiSound.WindowOpen : GameUiSound.WindowClose);
            }
            catch
            {
            }
        }

        private static void ShowStatus(string message)
        {
            try
            {
                FoundationApi.UiAccess.ShowStatusMessage(message);
            }
            catch
            {
            }
        }
    }
}
