# Node Inspector

[![Support on Ko-fi](https://raw.githubusercontent.com/KingdomIncModding/.github/main/assets/kofi-button.png)](https://ko-fi.com/urakkaamyx/tip)

**Node Inspector** lets you open any building in Kingdom Inc and inspect its live internal inventory in a game-themed popup. See what is backed up inside a full Mine, check the soldiers currently stationed in a War Effort, review a Dungeon party's health and gear, or clear stuck items with individual delete buttons and confirmed mass-purge controls.

## Features

- **Inspect Any Building**: Hover over a building and press **F6** (customizable) to open its internal inventory popup, or click the optional **Inspect** button in the top Mod Bar to pick a building directly.
- **Full Internals Coverage**:
  - Input and output port buffers on every building (Mines, Farms, Forests, crafters, junctions, and more).
  - **War Effort** soldiers (kept in sync with active war progress when removed).
  - **Stockpile** storage, **Dungeon** party members, **Quest Board** adventurers, **Arena** fighters, **Inn** guests and food, **Training Grounds** trainees and dummy, **Outfitter** active human, in-progress **Recipe** crafting inputs, **Armory** channel storage, and **Portal** channel storage.
- **Retractable Entry List**: Each stored item appears in a clean expandable row with its icon, name, storage slot, delete button, and expand/collapse chevron. Expanding a row reveals quality, sell and mana values, descriptions, equipment stats and enhancements, and full combatant breakdowns (HP/Energy bars, combat power, armor, stats, active traits, and equipped gear).
- **Mass Edit & Purge With Confirmation**: Use the top-of-list Mass Edit toolbar to **Expand All / Collapse All**, purge a specific storage group (such as Soldiers, Outputs, or Inputs), or **Purge All** items in the node. Every mass purge requires confirmation before removing items.
- **Live Refresh**: Entries update automatically while the popup is open, preserving expanded drawers and scroll position.

## Configuration

Open **Options -> Mods -> Node Inspector** in-game to customize:
- **Inspect Hotkey**: Default `F6`. Click the button and press any key to rebind, press `Backspace` or `Delete` to unbind, or press `Esc` to cancel.
- **Show Mod Bar Button**: Toggle the quick-inspect button in the top Mod Bar.
- **Live Refresh Interval**: Adjust how often the open popup refreshes live building state (`0.2s` - `2.0s`).
- **Confirm Single-Item Delete**: Optionally require confirmation when deleting individual entries (mass purge always requires confirmation).

## Installation

1. Install **BepInEx 5** for Kingdom Inc.
2. Install **Foundation** (`0.14.4` or later) into `KingdomInc/BepInEx/plugins/Foundation/`.
3. Extract **Node Inspector** into `KingdomInc/BepInEx/plugins/NodeInspector/` so `NodeInspector.dll` and `mod.json` sit inside that folder.
4. Launch Kingdom Inc.

## Compatibility

| Class.Method | Hook Type | Impact |
|---|---|---|
| Foundation.GameHooks (Escape) | Before | Closes the Node Inspector popup or active confirmation dialog when pressing Escape. |
| Foundation.GameHooks (Cancel) | Before | Cancels click-to-inspect mode or closes the open Node Inspector popup on cancel input. |
| Foundation.GameHooks (BuildingClick) | Before | Opens the inspector for the clicked building only while click-to-inspect mode is active. |
| Foundation.GameHooks (WorkspaceShown) | After | Closes the Node Inspector popup when switching between workspaces. |
| Foundation.GameHooks (UiVisibility) | After | Closes the Node Inspector popup when the workspace view is hidden. |

## Support

If you enjoy these Kingdom Inc mods and tools, consider supporting development on [**Ko-fi**](https://ko-fi.com/urakkaamyx/tip)!
