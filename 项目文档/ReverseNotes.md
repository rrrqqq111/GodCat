# Reverse Notes

This note maps the learning-demo implementation back to the Shepherd script index.

## M1 Mapping

| Demo script | Shepherd reference | Notes |
|---|---|---|
| `RanchManager` | `RanchManager.cs` | Owns the in-game day/stage loop, money, debt target, and map binding. |
| `RanchMap` | `RanchMap.cs`, `MapCell.cs` | Builds a small grid and exposes cell and neighbor lookup. |
| `MapCell` | `MapCell.cs` and derived cell classes | Stores one runtime animal and handles click selection. |
| `Animal` | `Animal.cs`, `AnimalBlueprint.cs` | Runtime animal state: data, coordinates, age, base money resolution. |
| `AnimalData` | `AnimalSO.cs` | ScriptableObject-compatible data shape for type/name/family/rarity/base money/icon. |
| `RanchHUD` | `GameForm.cs`, `DebtForm.cs`, simple UI forms | Minimal HUD for stage, day, gold, debt, selected cell, and next-day action. |
| `M1Bootstrap` | Project bootstrap/procedure classes | Temporary runtime scene generator for the first playable prototype. |

## Current Scope

- One auto-generated 5x5 ranch map.
- If 25 scene sprites from `Assets/Ranch Assets/地块` exist, M1 binds animals to those scene tile images.
- If no scene tiles exist, M1 auto-generates 25 cells with `Assets/Ranch Assets/地块/地块1.png`.
- Three starting animals based on the hoofed-family sheet: 家猪, 野猪, 马.
- A synchronous `Next Day` settlement loop.
- Stage/debt scaffolding for the next milestone.

## Next Notes

- M2 should move animal definitions into reusable assets or generated data files.
- M2 should add real animal sprites from `Assets/Ranch Assets/Animals`.
- M3 should split stage data into a `LevelData` or `StageData` asset.
