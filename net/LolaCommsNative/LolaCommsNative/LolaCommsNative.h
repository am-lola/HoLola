#pragma once

#ifndef _LOLA_COMMS_NATIVE_DLL_H
#define _LOLA_COMMS_NATIVE_DLL_H
///
// This provides an unmanaged DLL interface to the networking 
// interface & libraries needed to communicate with Lola.
//
// NOTE: When importing this DLL into Unity, you will NOT be able
//       to call into it from the editor.
///

#ifdef LOLACOMMDLL_EXPORT
#define LOLACOMMSDLL_API __declspec(dllexport)
#else
#define LOLACOMMSDLL_API __declspec(dllimport)
#endif

#include <string>
#include <comutil.h>

#include "Logging.h"
#include "VisionListener.h"

extern "C"
{

    LOLACOMMSDLL_API bool __stdcall Init();
    LOLACOMMSDLL_API bool __stdcall DeInit();

    LOLACOMMSDLL_API void __stdcall RegisterInfoCallback(VL_INFOCALLBACK callback);

    LOLACOMMSDLL_API VisionListener* __stdcall VisionListener_Create(int port);
    LOLACOMMSDLL_API void __stdcall VisionListener_Destroy(VisionListener* vl);
    LOLACOMMSDLL_API void __stdcall VisionListener_Listen(VisionListener* vl);
    LOLACOMMSDLL_API bool __stdcall VisionListener_IsListening(VisionListener* vl);
    LOLACOMMSDLL_API void __stdcall VisionListener_Stop(VisionListener* vl);

    typedef void(__stdcall *VL_ONERRORCALLBACK)(BSTR errstr);
    LOLACOMMSDLL_API void __stdcall VisionListener_OnError(VisionListener* vl, VL_ONERRORCALLBACK callback);

    typedef void(__stdcall *VL_ONCONNECTCALLBACK)(BSTR errstr);
    LOLACOMMSDLL_API void __stdcall VisionListener_OnConnect(VisionListener* vl, VL_ONCONNECTCALLBACK callback);

    typedef void(__stdcall *VL_ONDISCONNECTCALLBACK)(BSTR errstr);
    LOLACOMMSDLL_API void __stdcall VisionListener_OnDisconnect(VisionListener* vl, VL_ONDISCONNECTCALLBACK callback);

    typedef void(__stdcall *VL_ONOBSTACLEMESSAGECALLBACK)(am2b_iface::ObstacleMessage obstacle);
    LOLACOMMSDLL_API void __stdcall VisionListener_OnObstacleMessage(VisionListener* vl, VL_ONOBSTACLEMESSAGECALLBACK callback);

    typedef void(__stdcall *VL_ONSURFACEMESSAGECALLBACK)(am2b_iface::SurfaceMessage surface);
    LOLACOMMSDLL_API void __stdcall VisionListener_OnSurfaceMessage(VisionListener* vl, VL_ONSURFACEMESSAGECALLBACK callback);
}

#endif // _LOLA_COMMS_NATIVE_DLL_H
