# HoLola - Holographic Data Visualization for Lola

## Build Instructions

### Building & Deploying the Unity Project

If the following steps don't work, please follow the [complete instructions here](https://developer.microsoft.com/en-us/windows/mixed-reality/exporting_and_building_a_unity_visual_studio_solution).

1. Open the Unity project in Unity
2. `File->Build Settings...
3. Double-check that the **PLatform** is set to **Windows Store** and that at least one scene is listed under **Scenes In Build**
4. Click the **Build** button. You will be asked to choose a directory to put the binaries in (e.g `.\Build\`).
5. After the build completes, navigate to the build directory you selected and open `HoLola.sln` in Visual Studio (*NOTE: This is **not** the same `HoLola.sln` as you will find in the Unity project root directory!)
6. Set your CPU architecture to `x86`, then set the target (next to the green arrow) to either `Device` or `Hololens Emulator`.
7. Press `F5` to deploy and launch the application.