# Networking DLLs

The projects here contain all of the networking log and data marshalling necessary to get information received from robot components (e.g. lepp3) and make it available to components in Unity.

The `LolaComms.sln` references both projects, so you can build everything in one step from there.

### LolaCommsNative

A C++ DLL which handles the TCP and UDP communication necessary to receive data from the robot. Each component provides an asynchronous callback interface which allows the actual processing to be done on a background thread. 

### LolaComms

A C# UWP DLL which wraps LolaCommsNative and provides a managed interface usable by Unity components. Marshalling through the native-managed layer is hidden here, so any Unity component should not need to worry about any implementation details.
