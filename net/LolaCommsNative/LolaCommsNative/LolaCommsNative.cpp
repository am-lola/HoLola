#include "pch.h"
#include "LolaCommsNative.h"
#include <Windows.h>
#include "LolaCommsNativeETW.h"

extern "C"
{

    bool __stdcall Init()
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

    bool __stdcall DeInit()
    {
        int res = WSACleanup();
        if (res != 0)
        {
            return false; // cleanup failed
        }
        EventUnregisterLolaCommsNative();
        return true;
    }

    void __stdcall RegisterInfoCallback(INFOCALLBACK callback)
    {
        SetInfoCB([callback](std::wstring text) {callback(::SysAllocString(text.c_str())); });
    }

#pragma region VisionListener
    VisionListener* __stdcall VisionListener_Create(int port)
    {
        return new VisionListener(port);
    }

    void __stdcall VisionListener_Destroy(VisionListener* vl)
    {
        delete vl;
    }

    void __stdcall VisionListener_Listen(VisionListener* vl)
    {
        vl->listen();
    }

    bool __stdcall VisionListener_IsListening(VisionListener* vl)
    {
        return vl->listening();
    }

    void __stdcall VisionListener_Stop(VisionListener* vl)
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

    void __stdcall VisionListener_OnSurfaceMessage(VisionListener* vl, VL_ONSURFACEMESSAGECALLBACK callback)
    {
        vl->onSurfaceMessage([callback](am2b_iface::SurfaceMessage* surface) { callback(*surface); });
    }
#pragma endregion

#pragma region PoseListener
    PoseListener* __stdcall PoseListener_Create(int port)
    {
        return new PoseListener(port);
    }

    void __stdcall PoseListener_Destroy(PoseListener* pl)
    {
        delete pl;
    }

    void __stdcall PoseListener_Listen(PoseListener* pl)
    {
        pl->listen();
    }

    bool __stdcall PoseListener_IsListening(PoseListener* pl)
    {
        return pl->listening();
    }

    void __stdcall PoseListener_Stop(PoseListener* pl)
    {
        pl->stop();
    }

    void __stdcall PoseListener_OnError(PoseListener* pl, PL_ONERRORCALLBACK callback)
    {
        pl->onError([callback](std::wstring errstr) {callback(::SysAllocString(errstr.c_str())); });
    }

    void __stdcall PoseListener_OnNewPose(PoseListener* pl, PL_ONNEWPOSECALLBACK callback)
    {
        pl->onNewPose([callback](HR_Pose_Red* pose) {callback(*pose); });
    }
#pragma endregion

#pragma region FootstepListener
    FootstepListener* __stdcall FootstepListener_Create(int port, std::wstring host)
    {
        return new FootstepListener(port, host);
    }

    void __stdcall FootstepListener_Destroy(FootstepListener* fl)
    {
        delete fl;
    }

    void __stdcall FootstepListener_Listen(FootstepListener* fl)
    {
        fl->listen();
    }

    bool __stdcall FootstepListener_IsListening(FootstepListener* fl)
    {
        return fl->listening();
    }

    void __stdcall FootstepListener_Stop(FootstepListener* fl)
    {
        fl->stop();
    }

    void __stdcall FootstepListener_OnError(FootstepListener* fl, FL_ONERRORCALLBACK callback)
    {
        fl->onError([callback](std::wstring errstr) {callback(::SysAllocString(errstr.c_str())); });
    }

    void __stdcall FootstepListener_OnConnect(FootstepListener* fl, FL_ONCONNECTCALLBACK callback)
    {
        fl->onConnect([callback](std::wstring host) {callback(::SysAllocString(host.c_str())); });
    }

    void __stdcall FootstepListener_OnDisconnect(FootstepListener* fl, FL_ONDISCONNECTCALLBACK callback)
    {
        fl->onDisconnect([callback](std::wstring host) {callback(::SysAllocString(host.c_str())); });
    }

    void __stdcall FootstepListener_OnNewStep(FootstepListener* fl, FL_ONNEWSTEPCALLBACK callback)
    {
        fl->onNewStep([callback](FootstepListener::Footstep step) {callback(step); });
    }
#pragma endregion

}
