# furious-chicks
An Angry Birds clone made with Unity as its renderer, a made-from-scratch 3D physics engine and psudorandom levels.

## [Demo Video](https://youtu.be/nao3CMXM5Fw)

This project is a demo of the physics engine underneath, some stuff are left techinal (like the levels cannot be won or lost, and you have an infinite number of birds in hand).

## Requirements
### For running binaries
None
### For development
- Unity 2022.2.15 or later (using a later version could prevent compatablility with the mentioned version and hence pull requests cannot be accepted unless they accomodate to the mentioned version).
- Api Compatibility Level set to ".Net Framework" in Project Settings.

## Game Controls
### In Throwing Phase:
- Press the bird to enter throwing mode.
- Pull the bird to aim.
- Use up/down arrows to pull the bird further from the slingshot.
- Press Space or E to shoot.
- Press F to activate an ability while in air before hitting anything.
- While in throwing mode, press C to switch to level view mode.
- Press Q to force-destroy a bird.
### In Level View Mode:
- Use the mouse to orbit around the level.
- Scroll the mouse wheel to zoom in and out.
- Use WASD to orbit around the level.
- Use up/down arrows to zoom in and out.
- Press C to exit level view mode if the bird has not been launched yet.
- Prees V to lock/unlock the cursor.

## The Engine At A Glance
While made with extendablility in mind, the engine mainly supports boxes and spheres, but it provides the means to support anykind of shape be it convex or concave, using what we are calling boxilization algorithm, it approximates the mesh to a finite number of boxes that works as collision detectors for the logical container that represents the original shape.

Box-Box collision detection has been heavily inspired by [Dirk Gregorius publishing](http://media.steampowered.com/apps/valve/2015/DirkGregorius_Contacts.pdf)

Collision Resolution is based off [Allen Chou publishings](https://allenchou.net/), to be more specific, the engine uses Sequential Impulse with friction, bounciness and other tweaks.

## Contributions
We'd love to see more coming out of our engine, be it new colliders, collision solvers, or optimizations, so feel free to contribute, we'll do our best to review pull requests.

## License
Under [GPL 3.0](./LICENSE)

## Special Thanks For
The rest of FiveLittleTheives ( [RedWn](https://github.com/RedWn),[Tarek Al-Habbal](https://github.com/tarook0),[Iyad Al-Anssary](https://github.com/IyadAlanssary) ) for the support, feedback and testing.