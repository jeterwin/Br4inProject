# H0T BR4IN

***

## 1. Game Overview

**Game Title:** H0T BR4IN  
**Genre:** First-Person Shooter  
**Platform:** PC / BCI  
**Engine / Tools:** Unity, Blender  

**Elevator Pitch:**  
H0T BR4IN is a fast-paced first-person shooter, where two players work together: one uses keyboard and mouse controls to move through combat arenas, while the other uses a brain-computer interface to eliminate enemies.

***

## 2. Core Concept

**Core Gameplay Loop:**  
See threats → Move strategically → Eliminate enemies → Progress through the level → Repeat  

**Unique Selling Point (USP):**  
The game is built around cooperative play between two players, combining traditional controls with brain-based input through a BCI system.

***

## 3. Gameplay Mechanics

**Player Mechanics:**  
**Movement:** Walking and looking around using traditional keyboard and mouse controls.  
**Actions:** Shooting is performed through BCI input signals by the second player.  
**Camera Behavior:** First-person camera that follows the movement and view of the keyboard/mouse player.  

**Game Systems:**  
**Combat:** Fast-paced arena shooting focused on surviving as long as possible while eliminating enemies.  
**Progression:** The game currently uses an infinite wave system instead of level-to-level progression.  
**NPC Behavior:** Enemies detect the player, chase them, and attack. As the game continues, enemies spawn in greater numbers and attack more rapidly.

***

## 4. Input System (BCI Interface)

The game uses a hybrid input system shared between two players. One player handles movement and camera control using traditional input devices, while the second player handles shooting through a Brain-Computer Interface (BCI).

***

## 5. Level Design

**Level Structure:**  
The game is currently designed as an infinite survival arena.  

**Progression:**  
Instead of advancing through separate scenes, the player faces increasingly difficult waves of enemies. Over time, more enemies spawn and their attack speed increases. The main objective is to survive as long as possible and eliminate as many enemies as possible.

***

## 6. Art & Visual Style

**Style:** 3D, minimalistic  
The visual style is clean and stylized, without realistic textures or heavy visual effects.  
The main color palette uses red, blue, and purple, combined with white and gray environment elements.  

**References:**  
Superhot, Severed Steel, Pistol Whip

***

## 7. Team Roles

**[@jeterwin](https://www.github.com/jeterwin) - Kirichner Erwin - Gameplay Programmer**  
Responsible for gameplay systems, combat effects, and integrating BCI-based shooting into gameplay.  

**[@giurrgiu](https://www.github.com/giurrgiu) - Giurgiu Alexandru - Systems Programmer**  
Responsible for core game systems, technical implementation.  

**[@Pricher23](https://www.github.com/Pricher23) - Buia Theodor - Level Designer**  
Responsible for level design, layout & modeling and encounter design.  

**[@Miruna022](https://www.github.com/Miruna022) - Mesaroșiu Miruna - UI/UX Designer**  
Responsible for user interface design and implementation. BCI Training.  

**[@FlorianFlavius](https://www.github.com/FlorianFlavius) - Florian Flavius - UI Programmer**  
Responsible for UI implementation and menu systems.  

**[@stefars](https://www.github.com/stefars) - Mitrea Ștefan - 3D Artist / Technical Artist**  
Responsible for 3D models, asset integration and BCI input integration.  

**[@natalia-matiut](https://www.github.com/natalia-matiut) - Matiuț Natalia - UI/2D Artist**  
Responsible for UI design, 2D assets, documentation and overall user experience.  

**[@Tactful-Github](https://www.github.com/Tactful-Github) - Ica Dragoș - BCI Integration & Testing**  
Responsible for connecting the game with the BCI system, testing inputs, and supporting gameplay control integration.

***

## 8. Build & Run Instructions

Open the project or launch the game build.  
From the main menu, press PLAY to start.  
The game is played by two people: one using keyboard and mouse controls, and one using the BCI input system.  
Both players are required during normal gameplay.

***

## 9. Current Limitations

The biggest limitation of the project is the BCI training process. The system was trained using the G.tecUnityInterface, not specifically for our game, which limits how much we can adapt the controls to our own gameplay.  
Because of these constraints, the game had to remain visually simple and use limited (to no) textures, effects, and assets. The current version also uses hybrid controls, since full gameplay control through BCI was not feasible in the available time.

***

## 10. Future Improvements

With more time, the main improvement would be creating a BCI training process designed specifically for our game. This would help improve control accuracy, responsiveness, and the overall gameplay experience.  
We would also expand the visual design, add more assets, textures and polish, and further improve the integration between the game and the BCI system, as well as adding more levels and maps.
