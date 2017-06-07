#pragma once

#include <functional>
#include <comutil.h>

///
// Some simple utils to make passing log data back up 
// to managed land a little easier so it can be displayed
// to users in Unity or logged with Unity's logger.
///

typedef void(__stdcall *VL_INFOCALLBACK)(BSTR txt);
typedef std::function<void(std::wstring)> LogInfoFn;
static LogInfoFn LogInfo_cb = [](std::wstring txt) { OutputDebugString(txt.c_str()); };

static void LogInfo(std::wstring text)
{
    if (LogInfo_cb)
        LogInfo_cb(text);
}

static void SetInfoCB(std::function<void(std::wstring)> cb)
{
    LogInfo_cb = cb;
}
