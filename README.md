# Time Loop Puzzle Game

A puzzle game where each loop, your past actions repeat as "ghosts" helping (or hindering) your progress.

## Concept
In this game, you control a character trying to reach a goal. Each time you complete a loop or restart, your previous actions are recorded and played back as "ghost" characters in the next loop. You need to use these ghosts strategically to solve puzzles and reach the goal.

## Features
- Beautiful particle effects and animations
- Interactive elements like levers and pressure plates
- Multiple ghost characters with different colors
- Sound effects for all actions
- Smooth transitions between loops
- Intuitive UI with help screen

## How to Play
- Use arrow keys to move your character (blue square)
- Press 'R' to restart the current loop
- Reach the green goal area to complete a loop
- Your past actions will appear as purple ghost characters in subsequent loops
- Use your ghosts to help you reach areas you couldn't access alone
- Activate levers to move walls
- Step on pressure plates to temporarily activate mechanisms

## Requirements
- Python 3.x
- Pygame

## Installation
1. Make sure you have Python installed
2. Set up a virtual environment (recommended):
   ```
   python3 -m venv venv
   source venv/bin/activate  # On Windows: venv\Scripts\activate
   ```
3. Install Pygame: `pip install pygame`
4. Run the game: `python time_loop_game.py`

## Quick Start
For convenience, you can use the provided shell script:
```
./run_game.sh
```

## Game Mechanics
- Each loop records your movements
- Up to 3 ghosts (past loops) can exist at once
- Strategically plan your movements to help future loops
- Use levers to move walls and open paths
- Coordinate with your ghost characters to activate multiple pressure plates

## Future Enhancements
- More complex puzzles requiring multiple ghosts
- Additional interactive elements that ghosts can trigger
- Time-based challenges
- Multiple levels with increasing difficulty
- Custom level editor
