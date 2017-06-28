#include "pch.h"
#include "LolaCommsNative.h"
#include <Windows.h>
#include "LolaCommsNativeETW.h"

extern "C"
{

    bool Init()
    {
        auto reg = EventRegisterLolaCommsNative();
        LogInfo(std::wstring(L"ETW Registration: ") + std::to_wstring(reg));
        WSADATA wsaData = { 0 };
        int res = WSAStartup(MAKEWORD(2, 2), &wsaData);

        if (res != 0) // couldn't start WSA
        {
            return false;
        }
        return true;
    }

    bool DeInit()
    {
        int res = WSACleanup();
        if (res != 0)
        {
            return false; // cleanup failed
        }
        EventUnregisterLolaCommsNative();
        return true;
    }

    void __stdcall RegisterInfoCallback(VL_INFOCALLBACK callback)
    {
        SetInfoCB([callback](std::wstring text) {callback(::SysAllocString(text.c_str())); });
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

    void __stdcall VisionListener_OnConnect(VisionListener* vl, VL_ONCONNECTCALLBACK callback)
    {
        vl->onConnect([callback](std::wstring hostname) {callback(::SysAllocString(hostname.c_str())); });
    }

    void __stdcall VisionListener_OnDisconnect(VisionListener* vl, VL_ONDISCONNECTCALLBACK callback)
    {
        vl->onDisconnect([callback](std::wstring hostname) {callback(::SysAllocString(hostname.c_str())); });
    }

    void __stdcall VisionListener_OnObstacleMessage(VisionListener* vl, VL_ONOBSTACLEMESSAGECALLBACK callback)
    {
        vl->onObstacleMessage([callback](am2b_iface::ObstacleMessage* obstacle) { callback(*obstacle); });
    }
}
