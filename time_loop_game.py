import pygame
import sys
import math
import random
from collections import deque

# Initialize pygame
pygame.init()
pygame.mixer.init()  # For sound effects

# Constants
SCREEN_WIDTH = 800
SCREEN_HEIGHT = 600
FPS = 60

# Colors
WHITE = (255, 255, 255)
BLACK = (0, 0, 0)
RED = (255, 0, 0)
GREEN = (0, 255, 0)
BLUE = (0, 0, 255)
LIGHT_BLUE = (173, 216, 230)
DARK_BLUE = (0, 0, 139)
PLAYER_COLOR = (0, 128, 255)
GHOST_COLOR = (128, 0, 255, 180)  # Semi-transparent purple
WALL_COLOR = (50, 50, 50)
GOAL_COLOR = (0, 200, 0)
BUTTON_COLOR = (100, 100, 200)
BUTTON_HOVER_COLOR = (120, 120, 220)
BACKGROUND_COLOR = (240, 240, 245)

# Player class
class Player:
    def __init__(self, x, y, size=20):
        self.x = x
        self.y = y
        self.size = size
        self.speed = 5
        self.actions = []  # Store actions for this loop
        
    def move(self, dx, dy):
        # Check boundaries
        new_x = self.x + dx * self.speed
        new_y = self.y + dy * self.speed
        
        if 0 <= new_x <= SCREEN_WIDTH - self.size:
            self.x = new_x
        if 0 <= new_y <= SCREEN_HEIGHT - self.size:
            self.y = new_y
            
        # Record the action
        self.actions.append((self.x, self.y))
        
    def draw(self, screen):
        pygame.draw.rect(screen, PLAYER_COLOR, (self.x, self.y, self.size, self.size))
        
    def reset_position(self, x, y):
        self.x = x
        self.y = y
        self.actions = []

# Ghost class (represents past player actions)
class Ghost:
    def __init__(self, actions, size=20):
        self.actions = actions.copy()  # Copy of player's actions from previous loop
        self.size = size
        self.current_action = 0
        
    def update(self):
        if self.current_action < len(self.actions) - 1:
            self.current_action += 1
            
    def draw(self, screen):
        if self.actions and self.current_action < len(self.actions):
            x, y = self.actions[self.current_action]
            ghost_surface = pygame.Surface((self.size, self.size), pygame.SRCALPHA)
            pygame.draw.rect(ghost_surface, GHOST_COLOR, (0, 0, self.size, self.size))
            screen.blit(ghost_surface, (x, y))

# Level class
class Level:
    def __init__(self):
        self.walls = []
        self.goal = (700, 500, 50, 50)  # (x, y, width, height)
        self.start_pos = (50, 50)
        self.setup_level()
        
    def setup_level(self):
        # Add some walls (x, y, width, height)
        self.walls = [
            (100, 100, 600, 20),   # Top wall
            (100, 300, 600, 20),   # Middle wall
            (100, 500, 400, 20),   # Bottom partial wall
            (300, 200, 20, 100),   # Vertical wall 1
            (500, 320, 20, 180),   # Vertical wall 2
        ]
        
    def draw(self, screen):
        # Draw walls
        for wall in self.walls:
            pygame.draw.rect(screen, BLACK, wall)
        
        # Draw goal
        pygame.draw.rect(screen, GREEN, self.goal)
        
    def check_collision(self, player):
        player_rect = pygame.Rect(player.x, player.y, player.size, player.size)
        
        # Check wall collisions
        for wall in self.walls:
            if player_rect.colliderect(pygame.Rect(wall)):
                return True
                
        return False
        
    def check_goal(self, player):
        player_rect = pygame.Rect(player.x, player.y, player.size, player.size)
        goal_rect = pygame.Rect(self.goal)
        
        return player_rect.colliderect(goal_rect)

# Game class
class TimeLoopGame:
    def __init__(self):
        self.screen = pygame.display.set_mode((SCREEN_WIDTH, SCREEN_HEIGHT))
        pygame.display.set_caption("Time Loop Puzzle Game")
        self.clock = pygame.time.Clock()
        self.level = Level()
        self.player = Player(*self.level.start_pos)
        self.ghosts = []  # List of ghosts from previous loops
        self.loop_count = 0
        self.max_loops = 3  # Maximum number of loops/ghosts
        self.font = pygame.font.SysFont(None, 36)
        
    def handle_events(self):
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                return False
            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_r:
                    self.restart_loop()
                    
        keys = pygame.key.get_pressed()
        dx, dy = 0, 0
        
        if keys[pygame.K_LEFT]:
            dx = -1
        if keys[pygame.K_RIGHT]:
            dx = 1
        if keys[pygame.K_UP]:
            dy = -1
        if keys[pygame.K_DOWN]:
            dy = 1
            
        if dx != 0 or dy != 0:
            self.player.move(dx, dy)
            
            # Check for collisions with walls
            if self.level.check_collision(self.player):
                # Move back (simple collision resolution)
                self.player.move(-dx, -dy)
                
        return True
        
    def update(self):
        # Update all ghosts
        for ghost in self.ghosts:
            ghost.update()
            
        # Check if player reached the goal
        if self.level.check_goal(self.player):
            self.restart_loop()
            
    def draw(self):
        self.screen.fill(WHITE)
        
        # Draw level
        self.level.draw(self.screen)
        
        # Draw ghosts
        for ghost in self.ghosts:
            ghost.draw(self.screen)
            
        # Draw player
        self.player.draw(self.screen)
        
        # Draw UI
        loop_text = self.font.render(f"Loop: {self.loop_count + 1}", True, BLACK)
        self.screen.blit(loop_text, (10, 10))
        
        ghosts_text = self.font.render(f"Ghosts: {len(self.ghosts)}/{self.max_loops}", True, BLACK)
        self.screen.blit(ghosts_text, (10, 50))
        
        pygame.display.flip()
        
    def restart_loop(self):
        # Create a new ghost from player's actions
        if self.player.actions:
            self.ghosts.append(Ghost(self.player.actions))
            
        # Limit the number of ghosts
        if len(self.ghosts) > self.max_loops:
            self.ghosts.pop(0)  # Remove oldest ghost
            
        # Reset player position
        self.player.reset_position(*self.level.start_pos)
        self.loop_count += 1
        
    def run(self):
        running = True
        while running:
            running = self.handle_events()
            self.update()
            self.draw()
            self.clock.tick(FPS)
            
        pygame.quit()
        sys.exit()

# Main function
def main():
    game = TimeLoopGame()
    game.run()

if __name__ == "__main__":
    main()
