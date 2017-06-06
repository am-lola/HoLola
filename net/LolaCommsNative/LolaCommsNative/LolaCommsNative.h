#pragma once

#ifndef _LOLA_COMMS_NATIVE_DLL_H
#define _LOLA_COMMS_NATIVE_DLL_H
///
// This provides an unmanaged DLL interface to the networking 
// interface & libraries needed to communicate with lola.
//
// NOTE: When importing this DLL into Unity, you will NOT be able
//       to call into it from the editor.
///

#ifdef LOLACOMMDLL_EXPORT
#define LOLACOMMDLL_API __declspec(dllexport)
#else
#define LOLACOMMMDLL_API __declspec(dllimport)
#endif

#include <string>
#include <comutil.h>

#include "VisionListener.h"

extern "C"
{
	LOLACOMMMDLL_API BSTR SampleFunc(float f);
    
    LOLACOMMMDLL_API bool init();

    LOLACOMMMDLL_API VisionListener* VisionListener_Create(int port);
    LOLACOMMMDLL_API void VisionListener_Destroy(VisionListener* vl);
    LOLACOMMMDLL_API void VisionListener_Listen(VisionListener* vl);
    LOLACOMMMDLL_API bool VisionListener_IsListening(VisionListener* vl);
    LOLACOMMMDLL_API void VisionListener_Stop(VisionListener* vl);

    typedef void(__stdcall *VL_ONERRORCALLBACK)(BSTR errstr);
    LOLACOMMMDLL_API void __stdcall VisionListener_OnError(VisionListener* vl, VL_ONERRORCALLBACK callback);
}

#endif // _LOLA_COMMS_NATIVE_DLL_H
