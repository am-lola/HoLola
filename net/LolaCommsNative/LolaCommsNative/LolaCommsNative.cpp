#include "pch.h"
#include "LolaCommsNative.h"

extern "C"
{
    BSTR SampleFunc(float f)
    {
        return ::SysAllocString(std::to_wstring(f).c_str());
    }

    bool init()
    {
        WSADATA wsaData = { 0 };
        int res = WSAStartup(MAKEWORD(2, 2), &wsaData);

        if (res != 0) // couldn't start WSA
        {
            return false;
        }
        return true;
    }

    VisionListener* VisionListener_Create(int port)
    {
        return new VisionListener(port);
    }

    void VisionListener_Destroy(VisionListener* vl)
    {
        delete vl;
    }

    void VisionListener_Listen(VisionListener* vl)
    {
        vl->listen();
    }

    bool VisionListener_IsListening(VisionListener* vl)
    {
        return vl->listening();
    }

    void VisionListener_Stop(VisionListener* vl)
    {
        vl->stop();
    }

    void __stdcall VisionListener_OnError(VisionListener* vl, VL_ONERRORCALLBACK callback)
    {
        vl->onError([callback](std::wstring errstr) {callback(::SysAllocString(errstr.c_str())); });
    }
}
