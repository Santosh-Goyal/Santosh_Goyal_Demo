## 📋 Quick Navigation

- [Prerequisites](#prerequisites)
- [Part 1: Project Initialization](#part-1-project-initialization)
- [Part 2: Folder Structure](#part-2-folder-structure)
- [Part 3: Scene Setup](#part-3-scene-setup)
- [Part 4: Prefab Creation](#part-4-prefab-creation)
- [Part 5: Audio System](#part-5-audio-system)
- [Part 6: GameConfiguration](#part-6-gameconfiguration)
- [Part 7: Main Menu Scene UI](#part-7-main-menu-scene-ui)
- [Part 8: Gameplay Scene UI](#part-8-gameplay-scene-ui)
- [Part 9: Script References](#part-9-script-references)
- [Part 10: Testing](#part-10-testing)

---

## 🔟 Prerequisites

### System Requirements
- **Unity Version**: 2021.3 LTS (or newer Unity 6)
- **Operating System**: Windows 10/11, macOS, or Linux
- **RAM**: Minimum 8GB
- **Disk Space**: 5GB free space

### Required Assets (if not included)
- 8 unique card front sprites (512×512 or higher)
- 1 card back sprite (512×512 or higher)
- Menu BGM audio file (WAV or MP3, 128kbps+)
- Gameplay BGM audio file (WAV or MP3, 128kbps+)
- 6 SFX audio files: click, match, mismatch, combo, win, gameover (WAV, 44.1kHz)

### Game Assets Size
- Sprites: ~500KB total
- Audio: ~5MB total
- Ready after setup: ~50MB

---

## ✅ Part 1: Project Initialization

### Step 1.1: Create New Unity Project

1. **Open Unity Hub**
2. **Click "New Project"**
3. **Configure Project Settings**:
   - **Template**: 2D
   - **Editor Version**: 2021.3 LTS (or latest 2022+)
   - **Project Name**: `Card Demo`
   - **Location**: Choose your development folder
   - **Organization**: Your organization name
   - **2D URP Rendering**: Optional (standard 2D works fine)
4. **Click "Create Project"** and wait for import

### Step 1.2: Verify Project Settings

1. **Go to**: Edit → Project Settings
2. **Player Section**:
   - **Resolution and Presentation**:
     - Default Width: 1024
     - Default Height: 768
     - Full Screen: Off
   - **Android/iOS** (if targeting mobile):
     - Aspect Ratio: 16:9
3. **Graphics Section**:
   - Quality: High-Quality
   - Anti-Aliasing: 2x or 4x
4. **Audio Section**:
   - Default Speaker Mode: Stereo
   - System Sample Rate: 44100 Hz

### Step 1.3: Set Scene Background

1. **Open Main Camera settings** (if creating new scene)
2. **Set Background Color**:
   - R: 230, G: 200, B: 167 (Beige/Tan)
3. **Clear Flags**: Solid Color

---

## 📁 Part 2: Folder Structure

### Step 2.1: Create Folder Hierarchy

In your **Assets** folder, create this structure:

```
Assets/
├── Scripts/
├── Scenes/
├── Prefabs/
├── Audio/
│   ├── BGM/
│   └── SFX/
├── Sprites/
│   └── CardFronts/
├── Resources/
└── StreamingAssets/
    └── Savedata/
```

**Detailed Instructions**:

1. **Right-click in Project window** → Folder → Name it `Scripts`
2. **Repeat for**: `Scenes`, `Prefabs`, `Audio`, `Sprites`, `Resources`, `StreamingAssets`
3. **Inside Audio**: Create `BGM` and `SFX` folders
4. **Inside Sprites**: Create `CardFronts` folder
5. **Inside StreamingAssets**: Create `Savedata` folder

### Step 2.2: Move/Create Script Files

1. **In Scripts folder**, create files OR move existing:
   - GameManager.cs
   - GameSessionManager.cs
   - GameConfiguration.cs
   - CardController.cs
   - AudioManager.cs
   - SaveLoadManager.cs
   - MenuManager.cs
   - UIManager.cs

2. **Wait for compilation** (bar empties at top-right)
3. **Verify no errors** in Console

---

## 🎬 Part 3: Scene Setup

### Step 3.1: Create Main Menu Scene

1. **Right-click in Scenes folder** → Create New Scene
2. **Name it**: `MainMenu`
3. **Save the scene**: Ctrl+S (save in Scenes folder)
4. **Scene should have**:
   - Main Camera (default)
   - Canvas for UI (we'll create this next)
   - EventSystem (auto-created)

### Step 3.2: Create Gameplay Scene

1. **Right-click in Scenes folder** → Create New Scene
2. **Name it**: `Gameplay`
3. **Save the scene**: Ctrl+S
4. **Scene should have**:
   - Main Camera
   - Canvas for UI
   - Empty GameObject named `CardContainer`

### Step 3.3: Register Scenes in Build Settings

1. **Go to**: File → Build Settings
2. **Drag MainMenu scene** to slot 0
3. **Drag Gameplay scene** to slot 1
4. **Result**:
   - Scene 0: MainMenu
   - Scene 1: Gameplay
5. **Close Build Settings**

---

## 🎨 Part 4: Prefab Creation

### Step 4.1: Create Card Prefab

#### 4.1.1: Create Card GameObject

1. **In MainMenu/Gameplay scene**, right-click in Hierarchy
2. **UI → Image** (create a UI Image)
3. **Rename it**: `Card`
4. **Set RectTransform**:
   - Width: 100 px
   - Height: 150 px
   - Anchors: Center
5. **Set Image**:
   - Image Component → Source Image: Assign card back sprite
   - Preserve Aspect: ON
6. **Add Button Component**:
   - Right-click Card → Add Component → Button
   - Transition: Color Tint
   - Target Graphic: The Image component
7. **Add CardController Script**:
   - Add Component → CardController

#### 4.1.2: Configure Button

1. **Select Card** (with Button component)
2. **In Inspector** (Button section):
   - **Transition**: Color Tint
   - **Normal Color**: White (255, 255, 255)
   - **Highlighted Color**: Light Blue (200, 220, 255)
   - **Pressed Color**: Gray (150, 150, 150)
   - **Disabled Color**: Dark Gray (100, 100, 100)

#### 4.1.3: Save as Prefab

1. **Drag the Card from Hierarchy** into Prefabs folder
2. **Name it**: `Card.prefab`
3. **Delete original** from scene
4. **Verify**: Card.prefab now in Prefabs folder

---

## 🎵 Part 5: Audio System

### Step 5.1: Import Audio Files

1. **Navigate to**: Assets → Audio → BGM
2. **Import/Create audio files**:
   - **MenuBGM.wav** (or .mp3)
   - **GameplayBGM.wav** (or .mp3)
3. **Navigate to**: Assets → Audio → SFX
4. **Import/Create SFX files**:
   - ButtonClick.wav
   - CardMatch.wav
   - CardMismatch.wav
   - Combo.wav
   - GameWin.wav
   - GameOver.wav

### Step 5.2: Configure Audio Clips

**For each audio file:**

1. **Select the audio file** in Project
2. **In Inspector**, set:
   - **Load Type**: Streaming (for BGM), Decompress On Load (for SFX)
   - **Sample Rate Setting**: Optimize
   - **Apply**

### Step 5.3: Create AudioManager

1. **In MainMenu scene**, right-click in Hierarchy
2. **Create Empty** → Name: `AudioManager`
3. **Add Component**:
   - **Audio Source** (for BGM)
   - **AudioManager** script
4. **Configure Audio Source**:
   - **Spatial Blend**: 0 (2D)
   - **Volume**: 1
   - **Loop**: ON
5. **Don't destroy on load**: NO (we'll set this in code)
6. **In MainMenu Scene**, do NOT delete this object (it'll persist)

---

## ⚙️ Part 6: GameConfiguration

### Step 6.1: Create GameConfiguration Asset

1. **Right-click in Resources folder**
2. **Create → Memory Game → Game Configuration**
3. **Name it**: `GameConfiguration`

### Step 6.2: Configure Card Definitions

1. **Select GameConfiguration** in Inspector
2. **Scroll to**: Card Definitions section
3. **Set Array Size**: 8 (for 8 unique card types)
4. **For each Card Definition (0-7)**:
   - **Card ID**: 0, 1, 2, 3, 4, 5, 6, 7
   - **Card Name**: "Heart", "Diamond", "Club", "Spade", etc.
   - **Card Front**: Assign card sprite (Card1, Card2, etc.)
   - **Match Value**: 0, 1, 2, 3, 4, 5, 6, 7
   - **Description**: Optional text
5. **Card Back Sprite**: Assign your card back image

### Step 6.3: Configure Difficulty Levels

1. **In GameConfiguration Inspector**
2. **Scroll to**: Difficulty Settings
3. **Set Array Size**: 4 (Easy, Medium, Hard, Expert)

**Easy (Index 0)**:
- Level Name: Easy
- Grid Rows: 2
- Grid Columns: 3
- Time Limit Seconds: 60
- Card Flip Duration Modifier: 1.0

**Medium (Index 1)**:
- Level Name: Medium
- Grid Rows: 4
- Grid Columns: 3
- Time Limit Seconds: 120
- Card Flip Duration Modifier: 1.0

**Hard (Index 2)**:
- Level Name: Hard
- Grid Rows: 4
- Grid Columns: 4
- Time Limit Seconds: 180
- Card Flip Duration Modifier: 0.9

**Expert (Index 3)**:
- Level Name: Expert
- Grid Rows: 5
- Grid Columns: 4
- Time Limit Seconds: 240
- Card Flip Duration Modifier: 0.8

### Step 6.4: Configure Audio & Scoring

1. **Animation Settings**:
   - Card Flip Duration: 0.25
   - Mismatch Reset Delay: 1.5
   - Flip Ease: Default curve

2. **Scoring System**:
   - Points Per Match: 100
   - Points Per Mismatch: -10
   - Combo Multiplier: 1
   - Max Combo: 10

---

## 🎨 Part 7: Main Menu Scene UI

### Step 7.1: Setup Canvas

1. **Select MainMenu scene**
2. **Right-click in Hierarchy**
3. **UI → Canvas**
4. **Configure Canvas**:
   - **Canvas Scaler**:
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1024 × 768
   - **Graphic Raycaster**: Check if enabled
5. **Set background**: 
   - Add Image component to Canvas
   - Color: Beige (230, 200, 167)

### Step 7.2: Create Title Text

1. **Right-click Canvas** → UI → Text (TextMeshPro)
2. **Rename**: `TitleText`
3. **Set properties**:
   - Text: "Memory Card Match"
   - Font Size: 60
   - Alignment: Center, Top
   - Color: Black or Dark Blue
4. **Position**: Top-center of screen

### Step 7.3: Create Main Buttons

Create 4 buttons in a 2×2 grid:

1. **Right-click Canvas** → UI → Button (TextMeshPro)
2. **Configure each button**:

**Button 1 (Start)**:
- Name: StartButton
- Text: "Start"
- Position: Left-Center
- Size: 200×60

**Button 2 (Continue)**:
- Name: ContinueButton
- Text: "Continue"
- Position: Right-Center
- Size: 200×60

**Button 3 (Settings)**:
- Name: SettingsButton
- Text: "Settings"
- Position: Left-Center (below Start)
- Size: 200×60

**Button 4 (Quit)**:
- Name: QuitButton
- Text: "Quit"
- Position: Right-Center (below Continue)
- Size: 200×60

### Step 7.4: Create Statistics Panel

1. **Right-click Canvas** → UI → Panel
2. **Name**: `StatisticsPanel`
3. **Position**: Bottom-center
4. **Add 2 TextMeshPro texts**:
   - **BestScoreText**: "Best Score: 0"
   - **GamesPlayedText**: "Games Played: 0"

### Step 7.5: Create Difficulty Buttons

1. **Right-click Canvas** → UI → Panel
2. **Name**: `DifficultyPanel`
3. **Position**: Below Statistics
4. **Add Horizontal Layout Group**:
   - Child Force Expand: False
   - Spacing: 10
5. **Create 4 buttons**:
   - Name each: EasyButton, MediumButton, HardButton, ExpertButton
   - Text: "Easy 2x3", "Medium 4x3", "Hard 4x4", "Expert 5x4"
   - Size: 100×60 each

### Step 7.6: Add Managers & References

1. **Add Empty GameObject**: `MenuManager`
2. **Add Component**: MenuManager script
3. **In Inspector**, assign:
   - Start Button → newGameButton
   - Continue Button → continueGameButton
   - Settings Button → settingsButton
   - Quit Button → quitButton
   - Best Score Text → bestScoreText
   - Games Played Text → gamesPlayedText
   - Difficulty Buttons Array (4 slots):
     - [0] EasyButton
     - [1] MediumButton
     - [2] HardButton
     - [3] ExpertButton
4. **Add AudioManager reference** (if separate AudioManager exists)
5. **Save scene**: Ctrl+S

---

## 🎮 Part 8: Gameplay Scene UI

### Step 8.1: Setup Canvas

1. **Select Gameplay scene**
2. **Right-click in Hierarchy** → UI → Canvas
3. **Configure same as MainMenu**:
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1024×768
   - Background color: Beige

### Step 8.2: Create Game HUD

1. **Right-click Canvas** → UI → Panel
2. **Name**: `HUDPanel`
3. **Position**: Top-center
4. **Add TextMeshPro texts**:
   - **ScoreText**: "Score: 0" (left side)
   - **ComboText**: "Combo: x1" (center)
   - **MatchesText**: "Matches: 0/3" (right side)
   - **TimerText**: "Time: 01:00" (center-bottom)

### Step 8.3: Create Card Container

1. **Right-click in Hierarchy** → Create Empty
2. **Name**: `CardContainer`
3. **Add Component**: RectTransform
4. **Set properties**:
   - Anchors: Center
   - Width: 600
   - Height: 400
   - Position: Center of screen

### Step 8.4: Create Pause Menu

1. **Right-click Canvas** → UI → Panel
2. **Name**: `PausePanel`
3. **Set to inactive**: Uncheck in Hierarchy
4. **Add Semi-transparent background**:
   - Image component
   - Color: Black with 0.5 alpha
   - Full screen coverage
5. **Add CanvasGroup component**:
   - Alpha: 1
6. **Add 3 buttons**:
   - **ResumeButton**: "Resume"
   - **PauseMainMenuButton**: "Main Menu"
   - **SettingsCloseButton**: "Settings"

### Step 8.5: Create Game Over Panel

1. **Right-click Canvas** → UI → Panel
2. **Name**: `GameOverPanel`
3. **Set to inactive**: Uncheck
4. **Add content**:
   - Title: "Game Over!"
   - Final Score display
   - Matches display
   - Accuracy display
   - Time display
5. **Add 2 buttons**:
   - **RestartGameButton**: "Restart"
   - **MainMenuButton**: "Main Menu"
6. **Add CanvasGroup component**

### Step 8.6: Setup UI References

1. **Add Empty GameObject**: `UIManager`
2. **Add Component**: UIManager script
3. **Assign in Inspector**:
   - Score Text → scoreText
   - Combo Text → comboText
   - Matches Text → matchesText
   - Timer Text → timerText
   - Pause Panel → pausePanel
   - Game Over Panel → gameOverPanel
   - All buttons → respective fields
   - Volume sliders (if present)
   - Audio toggles (if present)

### Step 8.7: Create CardContainer Handler

1. **Select CardContainer** in Hierarchy
2. **Verify**: RectTransform is properly configured
3. **Note**: GameManager will populate this with card prefabs

### Step 8.8: Add GameManager

1. **Add Empty GameObject**: `GameManager`
2. **Add Component**: GameManager script
3. **Assign in Inspector**:
   - Game Config → GameConfiguration asset (from Resources)
   - Card Prefab → Card prefab
   - Card Container → CardContainer
   - Game Session Manager → (will be found via FindObjectOfType)
   - AudioManager → (will be found via singleton)
   - UIManager → (will be found via FindObjectOfType)

### Step 8.9: Add GameSessionManager

1. **Add Empty GameObject**: `GameSessionManager`
2. **Add Component**: GameSessionManager script
3. **Assign in Inspector**:
   - Game Config → GameConfiguration asset

---

## 📋 Part 9: Script References

### Step 9.1: Verify All Scripts Exist

**In Assets → Scripts**, ensure all files present:
- ✅ GameManager.cs
- ✅ GameSessionManager.cs
- ✅ GameConfiguration.cs
- ✅ CardController.cs
- ✅ AudioManager.cs
- ✅ SaveLoadManager.cs
- ✅ MenuManager.cs
- ✅ UIManager.cs

### Step 9.2: Button Click Handlers

**MainMenu Scene - MenuManager Events**:

1. **Start Button**:
   - Select StartButton
   - Button Component → On Click ()+
   - Drag MenuManager → MenuManager.OnNewGameClicked()

2. **Continue Button**:
   - Select ContinueButton
   - Button Component → On Click ()+
   - Drag MenuManager → MenuManager.OnContinueGameClicked()

3. **Settings Button**:
   - Select SettingsButton
   - Button Component → On Click ()+
   - Drag MenuManager → MenuManager.OnSettingsClicked()

4. **Quit Button**:
   - Select QuitButton
   - Button Component → On Click ()+
   - Drag MenuManager → MenuManager.OnQuitClicked()

5. **Difficulty Buttons** (Easy, Medium, Hard, Expert):
   - Each button already connected in MenuManager.cs
   - **OR manually connect**:
     - Select button
     - Button → On Click ()+
     - Drag MenuManager → MenuManager.OnDifficultySelected(0/1/2/3)

**Gameplay Scene - UIManager Events**:

1. **Resume Button**:
   - Drag UIManager → UIManager.OnResumeClicked()

2. **Pause Main Menu Button**:
   - Drag UIManager → UIManager.OnPauseMainMenuClicked()
   - (This saves and returns to main menu)

3. **Restart Button**:
   - Drag UIManager → UIManager.OnRestartGameClicked()

4. **Main Menu Button** (Game Over):
   - Drag UIManager → UIManager.OnMainMenuClicked()

### Step 9.3: Audio Manager Setup

1. **In MainMenu scene**, select AudioManager object
2. **In Inspector**:
   - Add Audio Source component (if not present)
   - Drag MenuBGM into Audio Clip
   - Volume: 0.8
   - Loop: ON
   - Spatial Blend: 0 (2D)

3. **In code**, ensure singleton starts BGM on menu init

---

## ✅ Part 10: Testing

### Step 10.1: Run Basic Tests

1. **Open MainMenu scene** (if not already open)
2. **Press Play** in Editor
3. **Test buttons**:
   - ✅ Title displays
   - ✅ All buttons clickable
   - ✅ Start button works (loads Gameplay)
   - ✅ Continue button disabled (no save)
   - ✅ Quit button closes game
   - ✅ Difficulty buttons selectable (visual feedback)

### Step 10.2: Test Difficulty Selection

1. **From Main Menu**:
   - Click each difficulty button
   - Verify visual feedback (green highlight)
   - Click Start with different difficulties
2. **Verify board size**:
   - Easy: 2×3 grid ✅
   - Medium: 4×3 grid ✅
   - Hard: 4×4 grid ✅
   - Expert: 5×4 grid ✅

### Step 10.3: Test Gameplay

1. **Start game on Easy difficulty**
2. **Test card mechanics**:
   - ✅ Click cards to flip them
   - ✅ Cards show front image
   - ✅ Cards flip back if mismatch
   - ✅ Matched cards lock and stay flipped
3. **Test HUD**:
   - ✅ Score updates on match
   - ✅ Combo increments
   - ✅ Matches display updates
   - ✅ Timer counts down
4. **Test pause**:
   - ✅ Pause button works
   - ✅ Game freezes
   - ✅ Pause menu appears
   - ✅ Resume button works

### Step 10.4: Test Audio

1. **Enable game volume** (not muted)
2. **Play game and listen for**:
   - ✅ Menu BGM playing
   - ✅ Click sound on button press
   - ✅ Card match sound
   - ✅ Combo sound (every 3rd combo)
   - ✅ Gameplay BGM transition
   - ✅ Win sound at game end

### Step 10.5: Test Save/Load

1. **On Easy (3 matches to win)**:
   - Play game, match 1-2 pairs
   - Click Pause → Main Menu button
   - Verify small beige save file in StreamingAssets/Savedata/
2. **Back to Main Menu**:
   - Verify Continue button is now ENABLED
3. **Click Continue**:
   - Game loads with exact same board
   - Score restored ✅
   - Matched cards locked ✅
   - Combo restored ✅
   - Timer restored ✅
4. **Finish game**:
   - Complete remaining matches
   - Win panel displays
   - Save file disappears ✅

### Step 10.6: Debug Checklist

**If something doesn't work:**

1. **Check Console** for errors
   - Fix any compilation errors
   - Watch for NullReferenceExceptions

2. **Verify Inspector assignments**:
   - Select problematic object
   - Check all serialized fields are assigned
   - Look for empty/missing references (red X)

3. **Verify Button Connections**:
   - Select button
   - Check "On Click ()" in Button component
   - Should show connected method

4. **Audio Issues**:
   - Verify AudioManager exists in MainMenu
   - Check AudioClips are assigned
   - Verify audio files are in correct format

5. **Save/Load Issues**:
   - Check StreamingAssets/Savedata/ folder exists
   - Check permissions on folder
   - Look for save file after pause→mainmenu

---
