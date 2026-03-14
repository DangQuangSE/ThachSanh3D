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
6. `Assets/ThachSanhGeneral/Phat/Scenes/SnakeBossIntro.unity` (Snake Boss Cutscene)
7. `Assets/ThachSanhGeneral/Phat/Scenes/Outro.unity` (Ending/Outro Scene)

*Make sure all these scenes are enabled (ticked) in the "Scenes in Build" manager before exporting the standalone executable.*

## 📂 Project Structure & Usage Notes
- **`Assets/ThachSanhGeneral/`**: The main directory where active development happens. It is divided into personal subfolders for each developer to minimize merge conflicts:
  - `Bach/`
  - `HuuAnh/`
  - `Phat/`
  - `Quang/`
- **Reference Guides**: The root directory contains numerous detailed Markdown files. Consult these specialized guides for maintenance tasks:
  - **Bosses**: `BOSS_SETUP_GUIDE.md`, `VISUAL_BOSS_GUIDE.md`, `QUICK_BOSS_SETUP.md`
  - **Dialogue & Quests**: `QUEST_DIALOGUE_SETUP_GUIDE.md`, `GENSHIN_DIALOGUE_COMPLETE_GUIDE.md`, `DIALOGUE_BOX_GENSHIN_STYLE_GUIDE.md`, `QUICK_DIALOGUE_REFERENCE.md`
  - **Mechanics**: `HEALTH_DAMAGE_SETUP_GUIDE.md`, `DAY_NIGHT_DARKNESS_GUIDE.md`
  - **UI**: `CONFIRM_PANEL_REDESIGN.md`, `FIX_CONFIRM_PANEL_NOT_SHOWING.md`

---
**Note for Rebuilding**: Before committing or deploying a new release, test the flow starting smoothly from the `Main Menu.unity` scene and verify scene transition linkages. Always coordinate asset changes within personal folders to prevent prefab merge conflicts.
