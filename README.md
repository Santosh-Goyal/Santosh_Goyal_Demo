# Santosh_Goyal_Demo

## 📋 Table of Contents

- [Features](#features)
- [Project Structure](#project-structure)
- [Architecture Overview](#architecture-overview)
- [Game Flow](#game-flow)
- [Difficulty Levels](#difficulty-levels)
- [Technical Specifications](#technical-specifications)
- [Quick Start](#quick-start)
- [Detailed Setup Guide](#detailed-setup-guide)

---

## ✨ Features

### Core Gameplay
- **Card Matching Mechanics**: Flip cards to find matching pairs
- **Combo System**: Consecutive matches increase combo multiplier
- **Scoring**: Dynamic points based on matches and combo
- **Object Pooling**: Optimized card management for performance
- **Event-Driven Architecture**: Decoupled UI updates via event system

### Save/Load System
- **Complete State Persistence**: Saves score, combo, pairs, time, and card positions
- **Encrypted Save Files**: Base64 encryption with anti-tampering prefix
- **Cross-Scene Data Transfer**: Static cache for seamless scene transitions
- **Single-Use Saves**: Auto-deleted after loading to prevent accidental replays
- **Card State Restoration**: All card positions and flip states preserved
- **Smart Save Logic**: Only saves from pause menu (not from game over)

### Difficulty System
- **4 Difficulty Levels**: Easy, Medium, Hard, Expert
- **Dynamic Grid Layouts**: 
  - Easy: 2×3 grid (6 cards, 3 pairs)
  - Medium: 4×3 grid (12 cards, 6 pairs)
  - Hard: 4×4 grid (16 cards, 8 pairs)
  - Expert: 5×4 grid (20 cards, 10 pairs)
- **Difficulty-Specific Timers**: 60s, 120s, 180s, 240s
- **Main Menu Selection**: Choose difficulty before playing
- **Auto-Adjusted Spacing**: Cards scale to fit difficulty size

### Audio System
- **Persistent AudioManager**: Survives scene transitions
- **BGM Management**: Menu and gameplay BGM with crossfading
- **Sound Effects**: 6 unique SFX (click, match, mismatch, combo, win, game over)
- **Volume Controls**: Master, SFX, and BGM sliders
- **Mute Toggles**: Individual audio channel muting
- **Volume Persistence**: Reset on menu start for consistency

### UI/UX
- **Main Menu**: Start, Continue, Settings, Quit, Difficulty Selection
- **Gameplay HUD**: Score, Combo, Matches (X/Y), Timer display
- **Pause Menu**: Resume, Main Menu, Settings access
- **Game Over Panel**: Final score, matches, accuracy, play time
- **Settings Panel**: Audio controls with visual feedback
- **Continue Button**: Smart enable/disable based on save existence
- **Responsive Design**: Works across different screen sizes

### Game Flow
- **Two-Scene Architecture**: Main Menu (Scene 0) → Gameplay (Scene 1)
- **Proper Cleanup**: BGM stops and restarts appropriately
- **State Management**: Difficulty and save data cached across transitions
- **Restart Functionality**: Completely fresh board with cleared cache
- **Pause System**: Full game pause with UI overlay

---

## 📁 Project Structure

```
Memory Card Match/
├── Assets/
│   ├── Scripts/
│   │   ├── GameManager.cs                 (Main game orchestrator)
│   │   ├── GameSessionManager.cs          (Session stats & events)
│   │   ├── GameConfiguration.cs           (Centralized config)
│   │   ├── CardController.cs              (Individual card behavior)
│   │   ├── AudioManager.cs                (Persistent audio)
│   │   ├── SaveLoadManager.cs             (Encryption & persistence)
│   │   ├── MenuManager.cs                 (Main menu logic)
│   │   └── UIManager.cs                   (Gameplay UI)
│   │
│   ├── Scenes/
│   │   ├── MainMenu.unity                 (Scene 0)
│   │   └── Gameplay.unity                 (Scene 1)
│   │
│   ├── Prefabs/
│   │   └── Card.prefab                    (Card with pooling)
│   │
│   ├── Audio/
│   │   ├── BGM/
│   │   │   ├── MenuBGM.wav
│   │   │   └── GameplayBGM.wav
│   │   └── SFX/
│   │       ├── ButtonClick.wav
│   │       ├── CardMatch.wav
│   │       ├── CardMismatch.wav
│   │       ├── Combo.wav
│   │       ├── GameWin.wav
│   │       └── GameOver.wav
│   │
│   ├── Sprites/
│   │   ├── CardBack.png
│   │   └── CardFronts/
│   │       ├── Card1.png
│   │       ├── Card2.png
│   │       ├── Card3.png
│   │       ├── Card4.png
│   │       ├── Card5.png
│   │       ├── Card6.png
│   │       ├── Card7.png
│   │       └── Card8.png
│   │
│   ├── Resources/
│   │   └── GameConfiguration.asset        (Difficulty settings)
│   │
│   └── StreamingAssets/
│       └── Savedata/                      (Runtime save files)
│
├── README.md                               (This file)
├── SETUP.md                                (Detailed setup guide)
└── .gitignore                              (Git ignore rules)
```

---

## 🏗️ Architecture Overview

### Design Patterns

1. **Singleton Pattern**
   - `AudioManager`: Persists across scenes
   - `SaveLoadManager`: Centralized save/load
   - `GameManager`: Gameplay orchestration (scene-specific)

2. **Event-Driven Architecture**
   - `OnScoreChanged`: UI updates score display
   - `OnComboChanged`: UI updates combo display
   - `OnTimeChanged`: UI updates timer display
   - `OnMatchOccurred`: UI updates matches count
   - `OnGameOver`: Game over sequence triggered

3. **Static Cache Pattern**
   - `GameManager.pendingSaveData`: Save data across scenes
   - `GameManager.pendingDifficulty`: Difficulty selection across scenes

4. **Object Pooling**
   - Card reuse reduces instantiation overhead
   - Pool size: 20 cards (enough for Expert difficulty)

### Data Flow

```
User Input (UI Buttons)
    ↓
MenuManager / UIManager
    ↓
GameManager (Main game logic)
    ↓
GameSessionManager (Stats tracking)
    ↓
Events fired (OnScoreChanged, etc.)
    ↓
UIManager (Updates display)
```

### Save/Load Flow

```
User Click Pause Menu → Main Menu
    ↓
GameManager.SaveGame() collects all state
    ↓
SaveLoadManager.SaveGame() encrypts data
    ↓
File written to StreamingAssets/Savedata/
    ↓
User Click Continue
    ↓
SaveLoadManager.LoadGame() decrypts data
    ↓
GameManager.PrepareSaveDataForLoading() caches it
    ↓
Scene loads
    ↓
GameManager detects cache, restores state
    ↓
Save file auto-deleted (single-use)
```

---

## 🎮 Game Flow

### Main Menu Flow
```
Main Menu Scene (Scene 0)
├── Title: "Memory Card Match"
├── Buttons: [Start] [Continue] [Settings] [Quit]
├── Difficulty Selection: [Easy] [Medium] [Hard] [Expert]
├── Statistics: Best Score, Games Played
└── Settings: Volume sliders & audio toggles
```

### Gameplay Flow
```
Click [Start] with selected difficulty
    ↓
Load Gameplay Scene (Scene 1)
    ↓
Create board grid (Easy: 2×3, Medium: 4×3, Hard: 4×4, Expert: 5×4)
    ↓
Spawn cards (pooled)
    ↓
Display HUD (Score, Combo, Matches, Timer)
    ↓
Player flips cards
    ↓
Match found?
├─ YES: Combo++, Score+=, Lock cards, Check win
├─ NO: Flip back after delay, Combo=1
    ↓
Win condition?
├─ YES: Show Game Over panel → Back to Main Menu
├─ NO: Continue playing
    ↓
Pause clicked?
├─ YES: Show pause menu (Resume, Main Menu, Settings)
│   └─ Main Menu → Save game → Load Scene 0
└─ NO: Keep playing
```

### Continue Game Flow
```
[Continue] button in Main Menu
    ↓
Check if save file exists
├─ NO: Keep button disabled
└─ YES: Enable button
    ↓
Player clicks Continue
    ↓
Load save data (decrypt)
    ↓
Cache data via static field
    ↓
Load Gameplay Scene
    ↓
Restore all stats and card positions
    ↓
Delete save file (single-use)
    ↓
Resume gameplay
```

---

## 🎯 Difficulty Levels

| Level | Grid | Cards | Pairs | Time | Spacing X | Spacing Y | Notes |
|-------|------|-------|-------|------|-----------|-----------|-------|
| **Easy** | 2×3 | 6 | 3 | 60s | 15px | 15px | Entry level |
| **Medium** | 4×3 | 12 | 6 | 120s | 15px | 15px | Balanced |
| **Hard** | 4×4 | 16 | 8 | 180s | 7.5px | 12px | Challenging |
| **Expert** | 5×4 | 20 | 10 | 240s | 7.5px | 10px | Extreme |

**Key Points:**
- Grid size defines board layout
- Spacing auto-adjusts to fit canvas
- Time limit from GameConfiguration.DifficultyLevel
- Win condition = Match all pairs (cards.Count / 2)

---

## 🔧 Technical Specifications

### Runtime Requirements
- **Unity Version**: 2021 LTS or newer
- **Target Platform**: Windows/WebGL/Mobile
- **Minimum Resolution**: 1024×768
- **Scripting Backend**: IL2CPP or Mono

### Performance
- **Card Pooling**: Max 20 cards in pool
- **Memory**: ~50MB base + save file (variable)
- **Draw Calls**: Optimized with UI canvas batching
- **Frame Target**: 60 FPS on most devices

### Save File Specifications
- **Format**: JSON (encrypted with Base64)
- **Location**: `StreamingAssets/Savedata/gamesave.mem`
- **Size**: ~2KB per save
- **Encryption**: Base64 + "MEM_GAME_" prefix anti-tampering
- **Persistence**: Single-use (deleted after load)

### Scoring System
- **Base points per match**: 100
- **Combo multiplier**: 1 per combo level
- **Formula**: `basePoints × (1 + combo)`
- **Max combo**: 10
- **Mismatch penalty**: -10 points (resets combo to 1)

---

## 🚀 Quick Start

1. **Clone the project**
   ```bash
   git clone <repository-url>
   cd "Card Demo"
   ```

2. **Open in Unity 2021 LTS+**

3. **Import audio and sprite files** (if not included)

4. **Configure GameConfiguration asset**
   - Set card definitions (8 unique cards)
   - Assign audio clips
   - Verify difficulty levels

5. **Play!**
   - Press Play in Editor
   - Select difficulty in Main Menu
   - Click Start to begin

---

## 📚 Detailed Setup Guide

See **SETUP.md** for comprehensive step-by-step instructions covering:
- Unity project initialization
- Folder structure setup
- Scene configuration
- Prefab setup
- UI hierarchy creation
- Audio system configuration
- GameConfiguration asset setup
- Button connections
- Testing procedures

---

## 🎵 Audio Details

### BGM Tracks
- **Menu BGM**: Plays in Main Menu scene, loops
- **Gameplay BGM**: Plays during gameplay, loops
- **Crossfade**: 0.5s transition when switching

### SFX Effects
- **Button Click**: Menu/UI interactions
- **Card Match**: When cards match successfully
- **Card Mismatch**: When cards don't match
- **Combo**: Every 3rd consecutive combo
- **Game Win**: When all pairs matched
- **Game Over**: When time runs out

### Volume Levels
- **Master Volume**: 0-100% (affects all)
- **SFX Volume**: 0-100% (independent)
- **BGM Volume**: 0-100% (independent)
- **Mute Toggles**: Silent mode for each channel

---

## 🔒 Save/Load Security

### Encryption
- **Method**: Base64 encoding
- **Prefix**: "MEM_GAME_" for anti-tampering
- **Protection**: Detects file corruption

### Data Included
- Score, combo, pairs, attempts, time
- Card positions and states (matched, flipped, facing)
- Difficulty level
- Timestamp of save
