import pygame
import sys
import math
import random
from collections import deque
import os

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

# Particle effect class
class Particle:
    def __init__(self, x, y, color, size=3, lifetime=30):
        self.x = x
        self.y = y
        self.color = color
        self.size = size
        self.lifetime = lifetime
        self.max_lifetime = lifetime
        self.dx = random.uniform(-1.5, 1.5)
        self.dy = random.uniform(-1.5, 1.5)
        
    def update(self):
        self.x += self.dx
        self.y += self.dy
        self.lifetime -= 1
        self.size = max(0, self.size * (self.lifetime / self.max_lifetime))
        
    def draw(self, screen):
        alpha = int(255 * (self.lifetime / self.max_lifetime))
        particle_color = (*self.color[:3], alpha) if len(self.color) > 3 else (*self.color, alpha)
        particle_surface = pygame.Surface((self.size*2, self.size*2), pygame.SRCALPHA)
        pygame.draw.circle(particle_surface, particle_color, (self.size, self.size), self.size)
        screen.blit(particle_surface, (self.x - self.size, self.y - self.size))

# Player class
class Player:
    def __init__(self, x, y, size=20):
        self.x = x
        self.y = y
        self.size = size
        self.speed = 5
        self.actions = []  # Store actions for this loop
        self.particles = []
        self.trail = []
        self.trail_timer = 0
        self.trail_interval = 5  # Frames between trail points
        self.is_moving = False
        self.animation_frame = 0
        self.animation_speed = 0.2
        
    def move(self, dx, dy):
        self.is_moving = dx != 0 or dy != 0
        
        # Check boundaries
        new_x = self.x + dx * self.speed
        new_y = self.y + dy * self.speed
        
        if 0 <= new_x <= SCREEN_WIDTH - self.size:
            self.x = new_x
        if 0 <= new_y <= SCREEN_HEIGHT - self.size:
            self.y = new_y
            
        # Record the action
        self.actions.append((self.x, self.y))
        
        # Add trail effect
        self.trail_timer += 1
        if self.trail_timer >= self.trail_interval:
            self.trail.append((self.x + self.size/2, self.y + self.size/2))
            if len(self.trail) > 10:  # Limit trail length
                self.trail.pop(0)
            self.trail_timer = 0
            
        # Add movement particles
        if self.is_moving and random.random() < 0.3:
            self.particles.append(
                Particle(
                    self.x + random.uniform(0, self.size),
                    self.y + random.uniform(0, self.size),
                    PLAYER_COLOR,
                    size=random.uniform(1, 3),
                    lifetime=random.randint(10, 20)
                )
            )
            
    def update(self):
        # Update animation
        if self.is_moving:
            self.animation_frame += self.animation_speed
            if self.animation_frame >= 4:
                self.animation_frame = 0
                
        # Update particles
        for particle in self.particles[:]:
            particle.update()
            if particle.lifetime <= 0:
                self.particles.remove(particle)
                
    def draw(self, screen):
        # Draw trail
        for i, (tx, ty) in enumerate(self.trail):
            alpha = int(255 * (i / len(self.trail)) * 0.5)
            trail_color = (*PLAYER_COLOR[:3], alpha) if len(PLAYER_COLOR) > 3 else (*PLAYER_COLOR, alpha)
            trail_size = int(self.size * 0.5 * (i / len(self.trail)))
            trail_surface = pygame.Surface((trail_size*2, trail_size*2), pygame.SRCALPHA)
            pygame.draw.circle(trail_surface, trail_color, (trail_size, trail_size), trail_size)
            screen.blit(trail_surface, (tx - trail_size, ty - trail_size))
            
        # Draw particles
        for particle in self.particles:
            particle.draw(screen)
            
        # Draw player with pulsing effect
        pulse = math.sin(pygame.time.get_ticks() * 0.005) * 0.1 + 0.9
        size_pulse = int(self.size * pulse)
        offset = (self.size - size_pulse) // 2
        
        # Draw player shadow
        shadow_surface = pygame.Surface((self.size, self.size), pygame.SRCALPHA)
        pygame.draw.rect(shadow_surface, (0, 0, 0, 50), (2, 2, self.size, self.size), border_radius=5)
        screen.blit(shadow_surface, (self.x + 2, self.y + 2))
        
        # Draw player with rounded corners
        pygame.draw.rect(screen, PLAYER_COLOR, (self.x, self.y, self.size, self.size), border_radius=5)
        
        # Draw highlight
        highlight_color = (min(255, PLAYER_COLOR[0] + 50), min(255, PLAYER_COLOR[1] + 50), min(255, PLAYER_COLOR[2] + 50))
        pygame.draw.rect(screen, highlight_color, (self.x, self.y, self.size, self.size), width=2, border_radius=5)
        
    def reset_position(self, x, y):
        self.x = x
        self.y = y
        self.actions = []
        self.trail = []
        self.particles = []
        
        # Create reset effect
        for _ in range(20):
            self.particles.append(
                Particle(
                    self.x + random.uniform(0, self.size),
                    self.y + random.uniform(0, self.size),
                    PLAYER_COLOR,
                    size=random.uniform(2, 5),
                    lifetime=random.randint(20, 40)
                )
            )

# Ghost class (represents past player actions)
class Ghost:
    def __init__(self, actions, size=20, color=None):
        self.actions = actions.copy()  # Copy of player's actions from previous loop
        self.size = size
        self.current_action = 0
        self.particles = []
        self.trail = []
        self.trail_timer = 0
        self.trail_interval = 5
        self.color = color or GHOST_COLOR
        self.loop_number = 0  # Will be set by the game
        
    def update(self):
        if self.current_action < len(self.actions) - 1:
            self.current_action += 1
            
            # Add trail effect
            self.trail_timer += 1
            if self.trail_timer >= self.trail_interval and self.actions:
                x, y = self.actions[self.current_action]
                self.trail.append((x + self.size/2, y + self.size/2))
                if len(self.trail) > 8:  # Limit trail length
                    self.trail.pop(0)
                self.trail_timer = 0
                
            # Add ghost particles
            if random.random() < 0.1 and self.actions:
                x, y = self.actions[self.current_action]
                self.particles.append(
                    Particle(
                        x + random.uniform(0, self.size),
                        y + random.uniform(0, self.size),
                        self.color,
                        size=random.uniform(1, 2),
                        lifetime=random.randint(10, 20)
                    )
                )
                
        # Update particles
        for particle in self.particles[:]:
            particle.update()
            if particle.lifetime <= 0:
                self.particles.remove(particle)
            
    def draw(self, screen):
        if not self.actions or self.current_action >= len(self.actions):
            return
            
        # Draw trail
        for i, (tx, ty) in enumerate(self.trail):
            alpha = int(180 * (i / len(self.trail)) * 0.5) if self.trail else 0
            trail_color = (*self.color[:3], alpha) if len(self.color) > 3 else (*self.color, alpha)
            trail_size = int(self.size * 0.4 * (i / len(self.trail))) if self.trail else 0
            if trail_size > 0:
                trail_surface = pygame.Surface((trail_size*2, trail_size*2), pygame.SRCALPHA)
                pygame.draw.circle(trail_surface, trail_color, (trail_size, trail_size), trail_size)
                screen.blit(trail_surface, (tx - trail_size, ty - trail_size))
            
        # Draw particles
        for particle in self.particles:
            particle.draw(screen)
            
        # Draw ghost with fading effect
        x, y = self.actions[self.current_action]
        pulse = math.sin(pygame.time.get_ticks() * 0.003 + self.loop_number) * 0.1 + 0.9
        
        # Draw ghost with rounded corners and transparency
        ghost_surface = pygame.Surface((self.size, self.size), pygame.SRCALPHA)
        pygame.draw.rect(ghost_surface, self.color, (0, 0, self.size, self.size), border_radius=5)
        screen.blit(ghost_surface, (x, y))
        
        # Draw loop number
        font = pygame.font.SysFont(None, 20)
        loop_text = font.render(str(self.loop_number), True, WHITE)
        text_rect = loop_text.get_rect(center=(x + self.size//2, y + self.size//2))
        screen.blit(loop_text, text_rect)

# Lever class
class Lever:
    def __init__(self, x, y, linked_wall=None):
        self.x = x
        self.y = y
        self.width = 30
        self.height = 15
        self.activated = False
        self.linked_wall = linked_wall
        self.cooldown = 0
        self.cooldown_max = 30  # Half second at 60 FPS
        self.particles = []
        
    def update(self):
        if self.cooldown > 0:
            self.cooldown -= 1
            
        # Update particles
        for particle in self.particles[:]:
            particle.update()
            if particle.lifetime <= 0:
                self.particles.remove(particle)
                
    def draw(self, screen):
        # Draw lever base
        pygame.draw.rect(screen, (100, 100, 100), 
                        (self.x - 5, self.y + 10, self.width + 10, 5), border_radius=2)
        
        # Draw lever handle
        if self.activated:
            # Activated position (right)
            pygame.draw.rect(screen, (200, 200, 100), 
                            (self.x + self.width//2, self.y - 5, self.width//2, self.height + 10), 
                            border_radius=5)
        else:
            # Deactivated position (left)
            pygame.draw.rect(screen, (200, 200, 100), 
                            (self.x, self.y - 5, self.width//2, self.height + 10), 
                            border_radius=5)
                            
        # Draw particles
        for particle in self.particles:
            particle.draw(screen)
            
    def check_collision(self, player):
        player_rect = pygame.Rect(player.x, player.y, player.size, player.size)
        lever_rect = pygame.Rect(self.x - 5, self.y - 5, self.width + 10, self.height + 15)
        return player_rect.colliderect(lever_rect)
        
    def activate(self):
        if self.cooldown <= 0:
            self.activated = not self.activated
            self.cooldown = self.cooldown_max
            
            # Create particles
            for _ in range(10):
                self.particles.append(
                    Particle(
                        self.x + random.uniform(0, self.width),
                        self.y + random.uniform(0, self.height),
                        (255, 255, 100),
                        size=random.uniform(1, 3),
                        lifetime=random.randint(10, 30)
                    )
                )
            return True
        return False

# Level class
class Level:
    def __init__(self):
        self.walls = []
        self.movable_walls = []  # Walls that can be moved by levers
        self.goal = (700, 500, 50, 50)  # (x, y, width, height)
        self.start_pos = (50, 50)
        self.levers = []
        self.pressure_plates = []
        self.particles = []
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
        
        # Add a movable wall
        movable_wall = pygame.Rect(400, 320, 20, 180)
        self.movable_walls.append({
            'rect': movable_wall,
            'active': True,
            'target_y': movable_wall.y,
            'current_y': movable_wall.y,
            'speed': 2
        })
        
        # Add a lever linked to the movable wall
        self.levers.append(Lever(200, 350, linked_wall=0))  # 0 is the index of the movable wall
        
        # Add pressure plates
        self.pressure_plates.append({
            'rect': pygame.Rect(600, 400, 40, 40),
            'activated': False,
            'color': RED,
            'active_color': GREEN,
            'linked_wall': None,
            'timer': 0,
            'duration': 180  # 3 seconds at 60 FPS
        })
        
    def update(self):
        # Update movable walls
        for wall in self.movable_walls:
            if wall['active']:
                wall['target_y'] = wall['rect'].y
            else:
                wall['target_y'] = SCREEN_HEIGHT + 50  # Move below screen
                
            # Smooth movement
            if wall['current_y'] < wall['target_y']:
                wall['current_y'] = min(wall['current_y'] + wall['speed'], wall['target_y'])
            elif wall['current_y'] > wall['target_y']:
                wall['current_y'] = max(wall['current_y'] - wall['speed'], wall['target_y'])
                
            wall['rect'].y = wall['current_y']
            
        # Update levers
        for lever in self.levers:
            lever.update()
            
        # Update pressure plates
        for plate in self.pressure_plates:
            if plate['activated']:
                plate['timer'] -= 1
                if plate['timer'] <= 0:
                    plate['activated'] = False
                    
                # Add particles
                if random.random() < 0.1:
                    self.particles.append(
                        Particle(
                            random.uniform(plate['rect'].left, plate['rect'].right),
                            random.uniform(plate['rect'].top, plate['rect'].bottom),
                            plate['active_color'],
                            size=random.uniform(1, 3),
                            lifetime=random.randint(10, 20)
                        )
                    )
                    
        # Update particles
        for particle in self.particles[:]:
            particle.update()
            if particle.lifetime <= 0:
                self.particles.remove(particle)
        
    def draw(self, screen):
        # Draw particles
        for particle in self.particles:
            particle.draw(screen)
            
        # Draw goal with glow effect
        goal_rect = pygame.Rect(self.goal)
        pulse = math.sin(pygame.time.get_ticks() * 0.005) * 0.2 + 0.8
        glow_size = int(max(goal_rect.width, goal_rect.height) * 1.2 * pulse)
        glow_surface = pygame.Surface((glow_size*2, glow_size*2), pygame.SRCALPHA)
        
        # Create radial gradient for glow
        for i in range(glow_size, 0, -1):
            alpha = int(100 * (i / glow_size))
            pygame.draw.circle(glow_surface, (*GOAL_COLOR[:3], alpha), (glow_size, glow_size), i)
            
        screen.blit(glow_surface, 
                   (goal_rect.centerx - glow_size, goal_rect.centery - glow_size))
        
        # Draw goal
        pygame.draw.rect(screen, GOAL_COLOR, goal_rect, border_radius=10)
        pygame.draw.rect(screen, (255, 255, 255), goal_rect, width=2, border_radius=10)
        
        # Draw walls with shadow effect
        for wall in self.walls:
            wall_rect = pygame.Rect(wall)
            # Shadow
            shadow_rect = pygame.Rect(wall_rect.x + 3, wall_rect.y + 3, wall_rect.width, wall_rect.height)
            pygame.draw.rect(screen, (30, 30, 30), shadow_rect, border_radius=3)
            # Wall
            pygame.draw.rect(screen, WALL_COLOR, wall_rect, border_radius=3)
            # Highlight
            pygame.draw.rect(screen, (80, 80, 80), wall_rect, width=2, border_radius=3)
            
        # Draw movable walls
        for wall in self.movable_walls:
            # Shadow
            shadow_rect = pygame.Rect(wall['rect'].x + 3, wall['rect'].y + 3, 
                                     wall['rect'].width, wall['rect'].height)
            pygame.draw.rect(screen, (30, 30, 30, 150), shadow_rect, border_radius=3)
            
            # Wall with slight transparency
            wall_surface = pygame.Surface((wall['rect'].width, wall['rect'].height), pygame.SRCALPHA)
            wall_color = (100, 100, 200, 200) if wall['active'] else (200, 100, 100, 200)
            pygame.draw.rect(wall_surface, wall_color, 
                            (0, 0, wall['rect'].width, wall['rect'].height), border_radius=3)
            screen.blit(wall_surface, (wall['rect'].x, wall['rect'].y))
            
            # Highlight
            pygame.draw.rect(screen, (150, 150, 255) if wall['active'] else (255, 150, 150), 
                            wall['rect'], width=2, border_radius=3)
            
        # Draw levers
        for lever in self.levers:
            lever.draw(screen)
            
        # Draw pressure plates
        for plate in self.pressure_plates:
            color = plate['active_color'] if plate['activated'] else plate['color']
            pygame.draw.rect(screen, color, plate['rect'], border_radius=5)
            pygame.draw.rect(screen, (255, 255, 255), plate['rect'], width=2, border_radius=5)
            
            # Draw activation indicator
            if plate['activated']:
                progress = plate['timer'] / plate['duration']
                indicator_width = plate['rect'].width * progress
                indicator_rect = pygame.Rect(plate['rect'].x, plate['rect'].y - 5, 
                                           indicator_width, 3)
                pygame.draw.rect(screen, plate['active_color'], indicator_rect)
        
    def check_collision(self, player):
        player_rect = pygame.Rect(player.x, player.y, player.size, player.size)
        
        # Check wall collisions
        for wall in self.walls:
            if player_rect.colliderect(pygame.Rect(wall)):
                return True
                
        # Check movable wall collisions
        for wall in self.movable_walls:
            if wall['active'] and player_rect.colliderect(wall['rect']):
                return True
                
        return False
        
    def check_goal(self, player):
        player_rect = pygame.Rect(player.x, player.y, player.size, player.size)
        goal_rect = pygame.Rect(self.goal)
        
        return player_rect.colliderect(goal_rect)
        
    def check_lever_interactions(self, player):
        player_rect = pygame.Rect(player.x, player.y, player.size, player.size)
        
        for i, lever in enumerate(self.levers):
            if lever.check_collision(player):
                if lever.activate():
                    # Toggle linked wall if exists
                    if lever.linked_wall is not None and lever.linked_wall < len(self.movable_walls):
                        self.movable_walls[lever.linked_wall]['active'] = not self.movable_walls[lever.linked_wall]['active']
                    return True
        return False
        
    def check_pressure_plate_interactions(self, player, ghosts):
        player_rect = pygame.Rect(player.x, player.y, player.size, player.size)
        
        for plate in self.pressure_plates:
            # Check player collision
            if player_rect.colliderect(plate['rect']):
                plate['activated'] = True
                plate['timer'] = plate['duration']
                return True
                
            # Check ghost collisions
            for ghost in ghosts:
                if ghost.actions and ghost.current_action < len(ghost.actions):
                    x, y = ghost.actions[ghost.current_action]
                    ghost_rect = pygame.Rect(x, y, ghost.size, ghost.size)
                    if ghost_rect.colliderect(plate['rect']):
                        plate['activated'] = True
                        plate['timer'] = plate['duration']
                        return True
                        
        return False

# HUD class
class HUD:
    def __init__(self):
        self.font = pygame.font.SysFont(None, 24)
        self.buttons = []
        self.setup_buttons()
        
    def setup_buttons(self):
        # Restart button
        self.buttons.append({
            'rect': pygame.Rect(SCREEN_WIDTH - 100, 10, 80, 30),
            'text': 'Restart',
            'action': 'restart',
            'hover': False
        })
        
    def update(self, mouse_pos):
        for button in self.buttons:
            button['hover'] = button['rect'].collidepoint(mouse_pos)
            
    def handle_events(self, event):
        if event.type == pygame.MOUSEBUTTONDOWN and event.button == 1:  # Left click
            for button in self.buttons:
                if button['hover']:
                    return button['action']
        return None
        
    def draw(self, screen, loop_count, ghost_count, max_ghosts):
        # Draw loop counter
        loop_text = self.font.render(f"Loop: {loop_count}", True, (50, 50, 50))
        screen.blit(loop_text, (10, 10))
        
        # Draw ghost counter
        ghost_text = self.font.render(f"Ghosts: {ghost_count}/{max_ghosts}", True, (50, 50, 50))
        screen.blit(ghost_text, (10, 40))
        
        # Draw buttons
        for button in self.buttons:
            color = BUTTON_HOVER_COLOR if button['hover'] else BUTTON_COLOR
            pygame.draw.rect(screen, color, button['rect'], border_radius=5)
            pygame.draw.rect(screen, (255, 255, 255), button['rect'], width=2, border_radius=5)
            
            text = self.font.render(button['text'], True, (255, 255, 255))
            text_rect = text.get_rect(center=button['rect'].center)
            screen.blit(text, text_rect)

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
        self.hud = HUD()
        self.particles = []
        self.game_state = "playing"  # playing, level_complete, game_over
        self.transition_alpha = 255
        self.fade_direction = -1  # -1 for fade in, 1 for fade out
        
        # Load sounds
        self.sounds = {}
        self.load_sounds()
        
    def load_sounds(self):
        """Load sound effects from assets directory"""
        sound_files = {
            'move': {'file': 'move.wav', 'volume': 0.2},
            'restart': {'file': 'restart.wav', 'volume': 0.4},
            'goal': {'file': 'goal.wav', 'volume': 0.5},
            'lever': {'file': 'lever.wav', 'volume': 0.4},
            'button': {'file': 'button.wav', 'volume': 0.3}
        }
        
        # Create assets directory if it doesn't exist
        assets_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'assets')
        if not os.path.exists(assets_dir):
            os.makedirs(assets_dir)
            print(f"Created assets directory at {assets_dir}")
            
        # Check if sound files exist, if not create placeholder sounds
        for sound_name, sound_info in sound_files.items():
            sound_path = os.path.join(assets_dir, sound_info['file'])
            if not os.path.exists(sound_path):
                self.create_placeholder_sound(sound_path)
                
            try:
                sound = pygame.mixer.Sound(sound_path)
                sound.set_volume(sound_info['volume'])
                self.sounds[sound_name] = sound
            except:
                print(f"Warning: Could not load sound {sound_path}")
                self.sounds[sound_name] = None
                
    def create_placeholder_sound(self, path):
        """Create a simple placeholder sound file"""
        try:
            import wave
            import struct
            
            # Create a simple beep sound
            duration = 0.1  # seconds
            frequency = 440  # Hz
            sample_rate = 44100  # samples per second
            num_samples = int(duration * sample_rate)
            
            # Open file for writing
            with wave.open(path, 'w') as wav_file:
                # Set parameters
                wav_file.setparams((1, 2, sample_rate, num_samples, 'NONE', 'not compressed'))
                
                # Generate sine wave
                samples = []
                for i in range(num_samples):
                    sample = 32767 * 0.3 * math.sin(2 * math.pi * frequency * i / sample_rate)
                    samples.append(struct.pack('h', int(sample)))
                    
                # Write samples to file
                wav_file.writeframes(b''.join(samples))
                
            print(f"Created placeholder sound at {path}")
        except:
            print(f"Warning: Could not create placeholder sound at {path}")
            
    def play_sound(self, sound_name):
        """Play a sound if it exists"""
        if sound_name in self.sounds and self.sounds[sound_name]:
            self.sounds[sound_name].play()
        
    def handle_events(self):
        mouse_pos = pygame.mouse.get_pos()
        self.hud.update(mouse_pos)
        
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                return False
                
            # Handle HUD events
            result = self.hud.handle_events(event)
            if result == "restart":
                self.restart_loop()
                self.play_sound('restart')
                
            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_r:
                    self.restart_loop()
                    self.play_sound('restart')
                        
        if self.game_state != "playing":
            return True
                    
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
            prev_x, prev_y = self.player.x, self.player.y
            self.player.move(dx, dy)
            
            # Check for collisions with walls
            if self.level.check_collision(self.player):
                # Move back (simple collision resolution)
                self.player.move(-dx, -dy)
            elif prev_x != self.player.x or prev_y != self.player.y:
                # Only play sound if actually moved
                self.play_sound('move')
                
            # Check for lever interactions
            if self.level.check_lever_interactions(self.player):
                self.play_sound('lever')
                    
        # Check for pressure plate interactions
        self.level.check_pressure_plate_interactions(self.player, self.ghosts)
                
        return True
        
    def update(self):
        # Handle screen transitions
        if self.transition_alpha > 0 and self.fade_direction == -1:
            self.transition_alpha = max(0, self.transition_alpha - 10)
        elif self.transition_alpha < 255 and self.fade_direction == 1:
            self.transition_alpha = min(255, self.transition_alpha + 10)
        elif self.transition_alpha == 255 and self.fade_direction == 1:
            # Transition complete, reset game state
            self.fade_direction = -1
            if self.game_state == "level_complete":
                self.restart_loop()
                self.game_state = "playing"
        
        # Update player
        self.player.update()
        
        # Update level
        self.level.update()
        
        # Update all ghosts
        for ghost in self.ghosts:
            ghost.update()
            
        # Update particles
        for particle in self.particles[:]:
            particle.update()
            if particle.lifetime <= 0:
                self.particles.remove(particle)
            
        # Check if player reached the goal
        if self.game_state == "playing" and self.level.check_goal(self.player):
            self.game_state = "level_complete"
            self.fade_direction = 1
            self.play_sound('goal')
                
            # Add celebration particles
            for _ in range(50):
                self.particles.append(
                    Particle(
                        random.uniform(self.player.x, self.player.x + self.player.size),
                        random.uniform(self.player.y, self.player.y + self.player.size),
                        (random.randint(0, 255), random.randint(0, 255), random.randint(0, 255)),
                        size=random.uniform(2, 5),
                        lifetime=random.randint(30, 60)
                    )
                )
            
    def draw(self):
        # Draw background with subtle pattern
        self.screen.fill(BACKGROUND_COLOR)
        
        # Draw subtle grid pattern
        for x in range(0, SCREEN_WIDTH, 40):
            pygame.draw.line(self.screen, (230, 230, 235), (x, 0), (x, SCREEN_HEIGHT), 1)
        for y in range(0, SCREEN_HEIGHT, 40):
            pygame.draw.line(self.screen, (230, 230, 235), (0, y), (SCREEN_WIDTH, y), 1)
        
        # Draw level
        self.level.draw(self.screen)
        
        # Draw ghosts
        for i, ghost in enumerate(self.ghosts):
            ghost.loop_number = i + 1
            ghost.draw(self.screen)
            
        # Draw player
        self.player.draw(self.screen)
        
        # Draw particles
        for particle in self.particles:
            particle.draw(self.screen)
        
        # Draw HUD
        self.hud.draw(self.screen, self.loop_count, len(self.ghosts), self.max_loops)
        
        # Draw level complete message
        if self.game_state == "level_complete":
            self.draw_level_complete()
            
        # Draw transition overlay
        if self.transition_alpha > 0:
            overlay = pygame.Surface((SCREEN_WIDTH, SCREEN_HEIGHT), pygame.SRCALPHA)
            overlay.fill((0, 0, 0, self.transition_alpha))
            self.screen.blit(overlay, (0, 0))
        
        pygame.display.flip()
        
    def draw_level_complete(self):
        if self.transition_alpha < 100:  # Only show when fade is mostly complete
            font = pygame.font.SysFont(None, 60)
            text = font.render("Loop Complete!", True, (255, 255, 255))
            text_rect = text.get_rect(center=(SCREEN_WIDTH//2, SCREEN_HEIGHT//2 - 50))
            
            # Draw text background
            bg_rect = text_rect.copy()
            bg_rect.inflate_ip(40, 20)
            pygame.draw.rect(self.screen, (0, 0, 0, 150), bg_rect, border_radius=10)
            
            self.screen.blit(text, text_rect)
            
            # Draw smaller instruction
            small_font = pygame.font.SysFont(None, 30)
            small_text = small_font.render("Starting next loop...", True, (200, 200, 200))
            small_rect = small_text.get_rect(center=(SCREEN_WIDTH//2, SCREEN_HEIGHT//2 + 10))
            self.screen.blit(small_text, small_rect)
        
    def restart_loop(self):
        # Create a new ghost from player's actions
        if self.player.actions:
            # Assign different colors to different ghosts
            ghost_colors = [
                (128, 0, 255, 180),  # Purple
                (255, 0, 128, 180),  # Pink
                (0, 128, 255, 180),  # Blue
            ]
            color_index = len(self.ghosts) % len(ghost_colors)
            new_ghost = Ghost(self.player.actions, color=ghost_colors[color_index])
            self.ghosts.append(new_ghost)
            
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
