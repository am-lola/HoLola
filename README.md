# HoLola - Holographic Data Visualization for Lola

## Dependencies

* Visual Studio, including:
** .NET Framework 3.5 development tools
** .NET Portable Library targeting pack
** Visual C++ runtime for UWP
** Windows Universal CRT SDK
** Visual Studio Tools for Unity (will be installed by Unity)
** Windows 10 SDK for UWP: C# and C++

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

#### Using the Hololens Emulator

https://developer.microsoft.com/en-us/windows/mixed-reality/using_the_hololens_emulator

### Building & Deploying the External DLLs

Both the native and managed DLLs can be build from `LolaComms.sln` under [/net/LolaComms](./net/LolaComms).

**For deployment to Hololens or the emulator** you *must* build `x86`, *not* `x64`.

Managed DLLs need to be copied to `<Unity Project Root>/Assets/Plugins/`

Unmanaged (C++) DLLs need to be copied to `<Unity Project Root>/Assets/`

## Known Issues

* Sometimes when deploying to the Hololens Emulator the app crashes on startup with a CLR error indicating a stack overflow. Re-deploying without making any changes often fixes this. Unclear, yet, if this is an issue with how we're loading our DLLs or if it's just a general stability problem with the Emulator.