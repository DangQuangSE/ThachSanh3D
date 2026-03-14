# ThachSanh3D

ThachSanh3D is a 3D Unity project developed collaboratively. This document serves as a comprehensive guide for understanding, maintaining, and rebuilding the project.

## ⚙️ Setup Instructions
- **Unity Version**: Ensure you are using Unity `6000.3.6f1`. The project is locked to this version.
  - Verification check: View `ProjectSettings/ProjectVersion.txt`.
- **Render Pipeline**: Universal Render Pipeline (URP) `17.3.0`.

## 📦 Core Dependencies
The project leverages the following major Unity packages (refer to `Packages/manifest.json` for the complete list):
- `com.unity.inputsystem`: `1.18.0` - New Input System for game controls.
- `com.unity.cinemachine`: `3.1.5` - Advanced camera system and transitions.
- `com.unity.render-pipelines.universal`: `17.3.0` - URP Graphics layer.
- `com.unity.timeline`: `1.8.10` - Cutscenes and animation sequencing.
- `com.unity.ai.navigation`: `2.0.9` - AI Pathfinding and NavMesh capabilities.
- `com.unity.visualscripting`: `1.9.9` - Visual scripting tools.
- `com.unity.shadergraph`: `17.3.0` - Developer visual shader creation.

## 🏗️ Export Settings & Build Sequences
The build settings are configured in `ProjectSettings/EditorBuildSettings.asset`. 
The game flows through these scenes, which must be enabled in the final build:
1. `Assets/ThachSanhGeneral/Bach/scenes/Main Menu.unity` (Main Menu & Entry Point)
2. `Assets/ThachSanhGeneral/Bach/scenes/Quest Scene.unity` (Quest Hub/Interaction)
3. `Assets/ThachSanhGeneral/Phat/Scenes/SnakeBossMap.unity` (Snake Boss Gameplay Area)
4. `Assets/ThachSanhGeneral/HuuAnh/scenes/Map_EagleBoss.unity` (Eagle Boss Gameplay Area)
5. `Assets/ThachSanhGeneral/Phat/Scenes/InstructionScene.unity` (Tutorial/Instructions)
6. `Assets/ThachSanhGeneral/Phat/Scenes/SnakeBossIntro.unity` (Snake Boss Intro)
7. `Assets/ThachSanhGeneral/Phat/Scenes/Outro.unity` (Ending/Outro Scene)
8. `Assets/ThachSanhGeneral/HuuAnh/scenes/MapEagleBoss_Intro.unity` (Eagle Boss Intro)

*Make sure all these scenes are enabled (ticked) in the "Scenes in Build" manager before exporting the standalone executable.*

## 📂 Project Structure & Usage Notes
- **`Assets/ThachSanhGeneral/`**: The main directory where active development happens. It is divided into personal subfolders for each developer to minimize merge conflicts:
  - `Bach/`
  - `HuuAnh/`
  - `Phat/`
  - `Quang/`
- **Reference Guides**: The `Documentation/` directory contains numerous detailed Markdown files. Consult these specialized guides for maintenance tasks:
  - **Bosses**: `Documentation/BOSS_SETUP_GUIDE.md`, `Documentation/VISUAL_BOSS_GUIDE.md`, `Documentation/QUICK_BOSS_SETUP.md`
  - **Dialogue & Quests**: `Documentation/QUEST_DIALOGUE_SETUP_GUIDE.md`, `Documentation/GENSHIN_DIALOGUE_COMPLETE_GUIDE.md`, `Documentation/DIALOGUE_BOX_GENSHIN_STYLE_GUIDE.md`, `Documentation/QUICK_DIALOGUE_REFERENCE.md`
  - **Mechanics**: `Documentation/HEALTH_DAMAGE_SETUP_GUIDE.md`, `Documentation/DAY_NIGHT_DARKNESS_GUIDE.md`
  - **UI**: `Documentation/CONFIRM_PANEL_REDESIGN.md`, `Documentation/FIX_CONFIRM_PANEL_NOT_SHOWING.md`

---
## 🚀 Transfer, Continuation, & Deployment
To guarantee a successful transfer and continuation of development by another team or system:

### Moving the Project
1. **Zipping/Archiving**: Simply compress the root folder *excluding* the `Library/`, `Logs/`, `Temp/`, and `Obj/` folders to save space (these are automatically generated upon opening Unity).
2. **Git Versioning**: The `.gitignore` file is already properly configured to maintain necessary and ignore unnecessary files for Unity projects.

### First-Time Setup (Continuation)
1. Ensure the Unity Editor installed is EXACTLY **`6000.3.6f1`**.
2. Add the project through Unity Hub and open it.
3. Upon first launch, Unity will automatically restore all `Packages` specified in `manifest.json`.
4. Open the `Main Menu.unity` scene in `Assets/ThachSanhGeneral/Bach/scenes/` to guarantee correct flow execution.

### Deployment / Building
When you are ready to build the final executable for production:
1. Ensure the `Export Settings & Build Sequences` (listed above) match identically in the Build Settings (`File > Build Settings`).
2. Only activate necessary scenes to minimize build size.
3. Build for your target platform (e.g., Windows PC). Ensure your build includes the `ThachSanh3D_Data/` folder exactly where the `.exe` resides.

---
**Note for Rebuilding**: Before committing or deploying a new release, test the flow starting smoothly from the `Main Menu.unity` scene and verify scene transition linkages. Always coordinate asset changes within personal folders to prevent prefab merge conflicts.
