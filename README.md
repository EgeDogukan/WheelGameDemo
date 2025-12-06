# Wheel of Fortune - Unity Technical Case

[![Made with Unity](https://img.shields.io/badge/Made%20with-Unity-57b9d3.svg?style=flat&logo=unity)](https://unity3d.com)
![Language](https://img.shields.io/badge/Language-C%23-blue?style=flat&logo=csharp)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20Domain-green?style=flat)

This repository contains my implementation of the **“Wheel of Fortune”** minigame, developed as a technical case study. The goal was to build a scalable **risk–reward spin game** with clean architecture, SOLID principles, and a production-quality UI that respects strict technical constraints.

---

> **👀 Quick Reviewer Guide**
>
> If you are reviewing the code architecture, I recommend starting here:
> 1. **`WheelGame.Domain/WheelGameSession.cs`**: The core "brain" (Pure C#).
> 2. **`WheelGame.UI/WheelGameController.cs`**: The glue between Domain and Unity.
> 3. **`WheelGame.Adapters/ScriptableWheelDefinitionProvider.cs`**: How config data feeds the domain.

---

## 🎮 Game Overview

**The Core Loop:**
* **Zones:** The game progresses through infinite zones.
* **The Wheel:** Each zone presents a wheel with multiple **Reward Slices** and exactly one **Bomb Slice** (except in special zones).
* **Risk vs. Reward:**
    * **Spin:** Gain rewards (stacking by type) OR hit the bomb and lose everything.
    * **Safe Silver Zone (Every 5th):** No bomb, better rewards.
    * **Super Gold Zone (Every 30th):** No bomb, best rewards.
* **Leaving:** Players can only **Leave** (cash out) during Safe or Super zones.

**Polish Features:**
* Smooth **DOTween** wheel animations with easing.
* **Pointer Tick** animation and audio as slices pass the indicator.
* **Reward Fly** effect: icons fly from the wheel to the inventory list.
* **Reactive UI:** Popups for Bombs and Summary screens with fade/scale animations.

---

## 🛠 Tech Stack

* **Engine:** Unity 2021 LTS
* **Language:** C#
* **UI System:** Unity UI (UGUI) + TextMeshPro (TMP)
* **Animation:** DOTween (No Unity Animator components used)
* **Data:** ScriptableObjects

---

## 🏗 Architecture

The project follows a strict separation of concerns, divided into three assemblies/namespaces:

### 1. Domain Layer (`WheelGame.Domain`)
* **Pure C#:** No dependencies on `UnityEngine`. Fully unit-testable.
* **`WheelGameSession`:** Manages state (Zone Index, Total Rewards, IsBombHit).
* **Logic:** Handles `ChooseSliceIndex()` and `ResolveSpin()`.
* **Interfaces:** Relies on `IZoneTypeResolver`, `IWheelDefinitionProvider`, and `IRandomProvider` to remain decoupled from implementation details.

### 2. Adapter Layer (`WheelGame.Adapters`)
* Connects Unity-specific data to the Domain.
* **`ScriptableWheelDefinitionProvider`:** Converts ScriptableObject configs into Domain entities.
* **`LinearRewardProgressionStrategy`:** Calculates reward scaling based on zone depth.
* **`UnityRandomProvider`:** Wraps `UnityEngine.Random`.

### 3. Presentation Layer (`WheelGame.UI`)
* **`WheelGameController`:** The entry point. Orchestrates the `Session`, listens to UI events, and updates Views.
* **Views:** Passive components (`WheelView`, `HudView`, `RewardSummaryView`) that visualize state.
* **Auto-Binding:** Buttons and texts are bound in code (no `OnClick` set in Inspector).

---

## 📂 Project Structure

```text
Assets/Scripts/
├── Adapters/                   # Infrastructure & Unity Glue
│   ├── LinearRewardProgressionStrategy.cs
│   ├── ScriptableWheelDefinitionProvider.cs
│   ├── ScriptableZoneTypeResolver.cs
│   └── UnityRandomProvider.cs
│
├── Config/                     # Data Definitions (ScriptableObjects)
│   ├── RewardProgressionConfig.cs
│   ├── SliceConfig.cs
│   └── WheelLayoutConfig.cs
│
├── Controllers/                # Entry Points
│   ├── WheelGameController.cs  # Main Logic Glue
│   └── FrameRateManager.cs     # Bootstrap (60FPS unlock)
│
├── Domain/                     # Pure C# Business Logic
│   ├── WheelGameSession.cs
│   ├── CoreTypes.cs            # Enums & Structs
│   ├── GameResults.cs
│   └── Interfaces.cs
│
└── UI/                         # Visual Components
    ├── WheelView.cs
    ├── HudView.cs
    ├── RewardSummaryView.cs
    ├── BombPopupView.cs
    ├── LeaveSummaryPopupView.cs
    └── UIAutoBinder.cs

```
---
## 🧠 Design Decisions

**1. Domain-First Approach**
I avoided writing logic inside `Update()` or Button callbacks. By isolating the session logic, the code is much less prone to "spaghetti" dependencies, and it would be very easy to move this logic to a backend server later if we needed anti-cheat validation.

**2. Performance & Pooling**
To ensure smooth performance on mobile devices, I implemented a strict memory management strategy:
* **Smart UI Reuse:** When changing zones, the Wheel Slices are not destroyed. Instead, I reuse the existing GameObjects and update their data (Icon/Text). This avoids layout thrashing.
* **Fly Icon Pooling:** The animated reward icons use a `Stack<GameObject>` pool to prevent Garbage Collection spikes during the core loop.

**3. Two-Step Spin Logic**
I split the spin action into `ChooseSliceIndex` (Decide) and `ResolveSpin` (Commit). This prevents synchronization bugs where the UI might show one result while the internal state has already updated to the next.

**4. Code-Driven UI Binding**
I didn't use any `OnClick` events in the Inspector. All buttons are wired up in `WheelGameController` using `onClick.AddListener`. This makes it much easier to debug call stacks since you can see exactly where events are hooked up.

---

## 🚀 How to Run

### In Unity Editor
1.  Open the project in **Unity 2021 LTS**.
2.  Open the scene: `Assets/Scenes/MainGame`.
3.  Press **Play**.

### Building for Android
1.  Switch Platform to **Android** in Build Settings.
2.  Ensure the Package Name is set (e.g., `com.dev.wheelgame`).
3.  Click **Build**.

> **Note:** I included a bootstrap script that unlocks 60 FPS and disables VSync to ensure smooth animations on mobile devices.

---

## 🔮 Future Improvements

If I were to expand this into a production-ready feature, I would add:
* **Save System:** Persisting the player's inventory and best run stats between sessions (e.g., via `PlayerPrefs` or a JSON file).
* **Server Authority:** Moving the RNG and Session logic to a cloud function to prevent client-side tampering.
* **Revive Mechanics:** Adding an "Ad Watch" feature to the Bomb Popup to let players continue their run.