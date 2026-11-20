# Node Inspector

[![Support on Ko-fi](https://raw.githubusercontent.com/KingdomIncModding/.github/main/assets/kofi-button.png)](https://ko-fi.com/urakkaamyx/tip)

**Node Inspector** lets you open any building in Kingdom Inc and inspect its live internal inventory in a game-themed popup. See what is backed up inside a full Mine, check the soldiers currently stationed in a War Effort, review a Dungeon party's health and gear, or clear stuck items with individual delete buttons and confirmed mass-purge controls.

## Features

- **Inspect Any Building**: Hover over a building and press **F6** (customizable) to open its internal inventory popup.
- **Full Internals Coverage**:
  - Input and output port buffers on every building (Mines, Farms, Forests, crafters, junctions, and more).
  - **War Effort** soldiers (kept in sync with active war progress when removed).
  - **Stockpile** storage, **Dungeon** party members, **Quest Board** adventurers, **Arena** fighters, **Inn** guests and food, **Training Grounds** trainees and dummy, **Outfitter** active human, in-progress **Recipe** crafting inputs, **Armory** channel storage, and **Portal** channel storage.
- **Retractable Category & Entry List**: Items are organized into collapsible **Inputs**, **Outputs**, and **Internal** sections (retracted by default) with per-section purge controls. Expanding a section reveals each stored item's row with its icon, name, delete button, and expand/collapse chevron. Expanding an item reveals its exact slot, quality, sell and mana values, descriptions, equipment stats and enhancements, and full combatant breakdowns (HP/Energy bars, combat power, armor, stats, active traits, and equipped gear).
- **Mass Edit & Purge With Confirmation**: Use the top-of-list Mass Edit toolbar to **Expand All / Collapse All** or **Purge All** items in the node, or purge **Inputs**, **Outputs**, or **Internal** individually from each section header. Every mass purge and single-item delete asks for confirmation by default.
- **Manual & Optional Auto-Refresh**: Snapshots stay steady while you expand drawers, inspect gear, and delete items; click the **Refresh** button anytime or enable optional **Auto-Refresh** in settings.

## Configuration

Open **Options -> Mods -> Node Inspector** in-game to customize:
- **Inspect Hotkey**: Default `F6`. Click the button and press any key to rebind, press `Backspace` or `Delete` to unbind, or press `Esc` to cancel.
- **Auto-Refresh While Open**: Disabled by default so rows stay steady while viewing; enable to refresh automatically on a timer.
- **Auto-Refresh Interval**: Adjust the auto-refresh interval (`1.0s` - `10.0s`) when Auto-Refresh is enabled.
- **Confirm Single-Item Delete**: Enabled by default so both single-item deletes and mass purges ask for confirmation.

## Installation

1. Install **BepInEx 5** for Kingdom Inc.
2. Install **Foundation** (`0.14.4` or later) into `KingdomInc/BepInEx/plugins/Foundation/`.
3. Extract **Node Inspector** into `KingdomInc/BepInEx/plugins/NodeInspector/` so `NodeInspector.dll` and `mod.json` sit inside that folder.
4. Launch Kingdom Inc.

## Compatibility

| Class.Method | Hook Type | Impact |
|---|---|---|
| Foundation.GameHooks (Escape) | Before | Closes the Node Inspector popup or active confirmation dialog when pressing Escape. |
| Foundation.GameHooks (Cancel) | Before | Closes the open Node Inspector popup on cancel input. |
| Foundation.GameHooks (WorkspaceShown) | After | Closes the Node Inspector popup when switching between workspaces. |
| Foundation.GameHooks (UiVisibility) | After | Closes the Node Inspector popup when the workspace view is hidden. |

## Support

If you enjoy these Kingdom Inc mods and tools, consider supporting development on [**Ko-fi**](https://ko-fi.com/urakkaamyx/tip)!
