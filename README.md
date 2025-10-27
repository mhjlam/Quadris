# Quadris

Tetris clone inspired by the NES version.

<p align="center">
   <img src="media/quadris.gif" alt="Demo Animation" width="720"/>
</p>

Quadris features an authentic scoring system, level progression, and gravity rules, a clean UI, and persistent highscores.

## Features

- **NES-accurate scoring, level, and gravity**  
  Points, level-ups, and drop speed match the original NES version of Tetris.

- **Preview window**  
  See the next piece before it spawns.

- **Animations**  
  Line clearing and game over animations.

- **Highscore tracking**  
  Persistent highscores with date logging.

## Controls

| Action         | Key         |
|----------------|-------------|
| Move Left      | Left Arrow  |
| Move Right     | Right Arrow |
| Soft Drop      | Down Arrow  |
| Hard Drop      | X           |
| Rotate         | Z           |
| Select/Confirm | Enter       |
| Pause/Menu     | Escape      |

- In menus, use **Up/Down** to navigate and **Enter** to select.
- In the highscores view, press **Escape** to return to the main menu.

## User Guide

1. **Start the game:** Launch the application and select "Play" from the main menu.

2. **Gameplay:** Use the controls above to play. Clear lines to score points and advance levels. The game ends when the stack reaches the top.

3. **Highscores:** After game over, your score is saved if it’s a highscore. View highscores from the main menu.

4. **Window resizing:**
The playfield and UI will automatically scale and center.

## Implementation Overview

- **`Screen`** system manages and separates the various game states (`Menu`, `Game`, `GameOver`).

- **`Renderer`** handles all drawing, including the well, pieces, preview, overlays, and menus. Layout is dynamic and pixel-perfect.

- **`Input`** maps keyboard events to game commands, with context-sensitive command maps for each state.
  
- **`Session`** tracks score, level, lines, and gravity, and implements game rules.

- **`Tetromino`** defines piece shapes, rotation, and movement.
- **`Well`** manages the playfield grid and line clears.

- **`Highscore`** manages persistent storage and display of top scores.

## Build Requirements

- .NET 8 SDK
- MonoGame 3.8+
- Visual Studio 2022

## License

This software is licensed under the [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.html). See [COPYING](COPYING) for details.
