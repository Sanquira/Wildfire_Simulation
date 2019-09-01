# Wildfire_Simulation

Wildfire simulation in Unity. Made as test project for Bohemia Interactive.

The goal of this task was to create a simple system simulating the spreading of fire among plants while taking into account the direction and speed of the wind.

Visual experience quality was not part of the test.

## Performance

This simulation can handle 20000 objects with ease. 

It's optimized both performance and memory wise.

It does not use physics to handle fire simulation due to performance issues.

It uses Quad tree for fast access to trees in forrest.

Caching for fire spread.

Unity terrain features for tree rendering and handling.

Coroutines for performance heavy features (wind changes, generating map)


## Control

* WASD - move
* QE - rotate camera
* Mouse wheel - Zoom camera
* LMB - Action depends on dropdown menu
* Generate - Clear map and generate new random forrest
* Clear - Clear map
* Start simulation - Pause/Unpause simulation time
* Fire - Generate a few random placed fires
* Dropdown menu: 
  * Add plant - Add tree to mouse selected position
  * Remove plant - Remove selected tree
  * Toggle fire - Set fire at mouse position or extinguish selected one. When tree is selected, set tree into fire and otherwise.
* Wind speed - Set windspeed which deform area to spread fire in wind direction
* Wind direction - Set wind direction
* Quit - Exit the program

## Colors:

* GREEN - normal tree
* YELLOW - heating up tree
* RED - burning tree
* CYAN - cooling down tree
* BLACK - burned out tree

## Heating a cooling mechanism:
Each fire distribute it's heat to near (aka heating area) not
burning/not burned out trees which are marked as heating up. When
accumulated temperature rises above threshold, tree is lighted up. When
the wind changes, the heating areas of all the fires are changed thus
some trees can be heated up, but not enough to light up. These trees
will start to cool down. When they cool down to default temperature,
they are marked as normal tree again. Each fire adds tempretature
separately so when there are more fires around tree it will light up faster.
