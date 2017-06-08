# HoLola - Holographic Data Visualization for Lola

## Dependencies

* [Visual Studio](https://developer.microsoft.com/en-us/windows/mixed-reality/install_the_tools), including:
  * .NET Framework 3.5 development tools
  * .NET Portable Library targeting pack
  * Visual C++ runtime for UWP
  * Visual Studio Tools for Unity (will be installed by Unity)
  * Windows 10 SDK for UWP: C# and C++
* [Unity 5.6](https://store.unity.com/) or greater

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

NOTE: Recent builds are checked in under the HoLola Unity project, so this should only be necessary if you are making changes to either DLL.

Both the native and managed DLLs can be build from `LolaComms.sln` under [/net/LolaComms](./net/LolaComms).

**For deployment to Hololens or the emulator** you *must* build `x86`, *not* `x64`.

Resulting DLLs should be automatically deployed to the Unity project:

* Managed DLLs need to be copied to `<Unity Project Root>/Assets/Plugins/`
* Unmanaged (C++) DLLs need to be copied to `<Unity Project Root>/Assets/`

Once the DLLs have been updated in the Unity project, build from Unity and follow the instructions above to deploy to the device or emulator.

## Known Issues

* Sometimes when deploying to the Hololens Emulator the app crashes on startup with a CLR error. Under a debugger this appears to be a fatal error in Windows.Mirage.dll, but there are also often bad references in Unty's D3D code (null textures, etc) which can lead to an early crash. Re-deploying without making any changes often fixes this.