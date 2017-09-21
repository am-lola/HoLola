# HoLola - Holographic Data Visualization for Lola

NOTE: Please use `--recursive` when cloning this repo to pull in submodules.

## Dependencies

* [Visual Studio 2017](https://developer.microsoft.com/en-us/windows/mixed-reality/install_the_tools), including:
  * .NET Framework 3.5 development tools
  * .NET Portable Library targeting pack
  * Visual C++ runtime for UWP
  * Visual Studio Tools for Unity (will be installed by Unity)
  * Windows 10 SDK for UWP: C# and C++
* [Unity 5.6.2f1](https://store.unity.com/) or greater (earlier versions may build the project, but contain bugs in some APIs!)
* [am2b-iface]() (included as a submodule-- either clone this repository with `--recursive` or run `git submodule update --init` after cloning to obtain the latest compatible version)

## Build Instructions

### Building & Deploying the Unity Project

#### Normal build/deploy

If the following steps don't work, please follow the [complete instructions here](https://developer.microsoft.com/en-us/windows/mixed-reality/exporting_and_building_a_unity_visual_studio_solution).

1. Open the Unity project in Unity
2. `File->Build Settings...`
3. Double-check that the **PLatform** is set to **Windows Store** and that at least one scene is listed under **Scenes In Build**
4. Click the **Build** button. You will be asked to choose a directory to put the binaries in (e.g `.\Build\`).
5. After the build completes, navigate to the build directory you selected and open `HoLola.sln` in Visual Studio (*NOTE: This is **not** the same `HoLola.sln` as you will find in the Unity project root directory!)
6. Set your CPU architecture to `x86`, then set the target (next to the green arrow) to either `Device` or `Hololens Emulator`.
7. Press `F5` to deploy and launch the application.

#### Manual Deployment From Unity

This will be necessary if you do not have sufficient permissions on your build machine to use the deployment and remote debugging tools in Visual Sudio (requires [Developer Mode](https://docs.microsoft.com/en-us/windows/uwp/get-started/enable-your-device-for-development), which must be enabled by an admin).

**The Hololens should be connected to your PC via USB**

1. Open the Unity project in Unity
2. `HoloToolkit->Build Window`
3. Select `Build SLN, Build APPX`
4. When the build is complete, select `Open APPX Packages Location`
5. Open a web browser and navigate to `http://127.0.0.1:10080`
6. In the browser select `Apps` on the left
7. In the App Manager, under *Install app* use the `Choose File` button to select the `.appx` file in directory opened in step 4
8. Click `Add dependency`, and add all of the `.appx` files under `<APPX Dir>/Dependencies/x86'
9. Click `Go`
10. Your app should now be installed! Bloom to find it in the app list, or use the *Insalled apps* menu in the App Manager to start it.

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

## Debugging Tips

#### Log Files
Unity produces a log file containing all the output from `Debug.Log*` calls, callstacks if a Monobehavior hits an unhandled exception, etc. The log file is over-written every time the app launches, so make sure to download the logs after every test!

The log is located in: `User Files \ LocalAppData \ HoLola_x.x.x.x_x86__xxx \ TempState`

#### ETW Events
ETW Trace data is available for some native components, which may help track down issues in the LolaComms DLLs. To enable them and get trace data, open the `Logging` section of the device portal, then under *Custom Providers* enter the GUID `8EB119A9-2FE5-46F5-998B-A396CA3F74B7` and click *Enable*. You should now see live event data at the bottom of the page when the application is active.

See [net/LolaCommsNative/LolaCommsNative/LolaCommsNative.man](net/LolaCommsNative/LolaCommsNative/LolaCommsNative.man) for event definitions.

*Note:* For some reason, provider info and event payloads are not appearing in the device portal. This *may* be related to [this issue](https://wpdev.uservoice.com/forums/110705-universal-windows-platform/suggestions/18591439-loggingchannel-not-showing-string-message-content), meaning a future update on the Hololens (and possibly a matching SDK update) could fix it, but it might also be that the configuration is not quite right on our end...

#### Debugging the external DLLs

1. Enable mixed-mode debugging in the project properties to examine behavior in LolaCommsNative
2. To catch exceptions and breakpoints in both DLLs, ensure `Enable Just My Code` is **NOT** checked in your debugging Options.

*Unfortunate side-effects:* Doing both of the above will also cause you catch a lot of exceptions in Unity and other components you likely don't want to deal with. This can make debugging tedious since Unity often throws a bunch of exceptions during initialization, so the recommendation is to keep your debugging setup for managed-only and Just My Code unless you *need* to dig into the communications DLLs.

## Known Issues

* Sometimes when deploying to the Hololens Emulator the app crashes on startup with a CLR error. Under a debugger this appears to be a fatal error in Windows.Mirage.dll, but there are also often bad references in Unty's D3D code (null textures, etc) which can lead to an early crash. Re-deploying without making any changes often fixes this.
