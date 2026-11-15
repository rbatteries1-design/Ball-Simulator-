# Bounce Back (Unity)

This project now has a small 3D bounce-course starter: move the ball, land on trampolines, and reach the goal platform.

## Run in Unity

1. Open this project in Unity 2021.3.16f1 or newer and wait for scripts to compile.
2. Choose **Bounce Back > Create or Reset Demo Scene**.
3. Press **Play**.

**WASD / arrow keys** move relative to the camera, **hold right mouse and drag** to orbit around the ball, and use the **mouse wheel** to zoom. **Space** jumps when grounded and **R** restarts. The ball bounces from its impact speed and gradually loses height like a real ball; trampolines add a stronger launch. You start with three lives; falling below the fall limit costs one, and losing all three ends the run.

## Goal and audio

The generated demo includes a trigger goal and an AudioSource on the game manager. On your own scene, choose **Bounce Back > Setup Current Scene Lives and Audio** to add the manager and connect it to the ball. Select your goal marker and choose **Bounce Back > Add Goal Trigger to Selected Object**. Assign your victory, life-lost, and game-over AudioClips in the manager Inspector. The main camera needs one AudioListener (the sample scene already has one).

## Build

Use the Unity menu **Bounce Back > Build > Windows 64-bit** or **Bounce Back > Build > WebGL**. Builds are written to `Builds/Windows` and `Builds/WebGL`. WebGL requires that the WebGL Build Support module is installed in Unity Hub.

The build commands also generate the demo scene if it does not exist and add it to Build Settings.

## Skyboxes

Three 2K panoramic HDR sky textures are in `Assets/Skyboxes` (Kloofendal clouds, Autumn Field sunset, and Sunflowers clear sky). They are CC0 assets from [Poly Haven](https://polyhaven.com/license).

After Unity imports them, choose **Bounce Back > Create Skybox Materials**. Then choose a material from `Assets/Materials/Skyboxes` in the Lighting window's **Environment > Skybox Material** field.

For command-line builds, run Unity with the project path and one of these methods:

```text
-executeMethod BounceBack.Editor.BounceBackBuild.BuildWindows
-executeMethod BounceBack.Editor.BounceBackBuild.BuildWebGL
```

Build generation leaves your currently open Editor scene alone. The **Create or Reset Demo Scene** menu item asks before replacing its dedicated demo scene, then opens that scene.
