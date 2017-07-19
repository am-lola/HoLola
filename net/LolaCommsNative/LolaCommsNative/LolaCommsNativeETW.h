//**********************************************************************`
//* This is an include file generated by Message Compiler.             *`
//*                                                                    *`
//* Copyright (c) Microsoft Corporation. All Rights Reserved.          *`
//**********************************************************************`
#pragma once
#include <wmistr.h>
#include <evntrace.h>
#include "evntprov.h"
//
//  Initial Defs
//
#if !defined(ETW_INLINE)
#define ETW_INLINE DECLSPEC_NOINLINE __inline
#endif

#if defined(__cplusplus)
extern "C" {
#endif

//
// Allow disabling of code generation
//
#ifndef MCGEN_DISABLE_PROVIDER_CODE_GENERATION
#if  !defined(McGenDebug)
#define McGenDebug(a,b)
#endif 


#if !defined(MCGEN_TRACE_CONTEXT_DEF)
#define MCGEN_TRACE_CONTEXT_DEF
typedef struct _MCGEN_TRACE_CONTEXT
{
    TRACEHANDLE            RegistrationHandle;
    TRACEHANDLE            Logger;
    ULONGLONG              MatchAnyKeyword;
    ULONGLONG              MatchAllKeyword;
    ULONG                  Flags;
    ULONG                  IsEnabled;
    UCHAR                  Level; 
    UCHAR                  Reserve;
    USHORT                 EnableBitsCount;
    PULONG                 EnableBitMask;
    const ULONGLONG*       EnableKeyWords;
    const UCHAR*           EnableLevel;
} MCGEN_TRACE_CONTEXT, *PMCGEN_TRACE_CONTEXT;
#endif

#if !defined(MCGEN_LEVEL_KEYWORD_ENABLED_DEF)
#define MCGEN_LEVEL_KEYWORD_ENABLED_DEF
FORCEINLINE
BOOLEAN
McGenLevelKeywordEnabled(
    _In_ PMCGEN_TRACE_CONTEXT EnableInfo,
    _In_ UCHAR Level,
    _In_ ULONGLONG Keyword
    )
{
    //
    // Check if the event Level is lower than the level at which
    // the channel is enabled.
    // If the event Level is 0 or the channel is enabled at level 0,
    // all levels are enabled.
    //

    if ((Level <= EnableInfo->Level) || // This also covers the case of Level == 0.
        (EnableInfo->Level == 0)) {

        //
        // Check if Keyword is enabled
        //

        if ((Keyword == (ULONGLONG)0) ||
            ((Keyword & EnableInfo->MatchAnyKeyword) &&
             ((Keyword & EnableInfo->MatchAllKeyword) == EnableInfo->MatchAllKeyword))) {
            return TRUE;
        }
    }

    return FALSE;

}
#endif

#if !defined(MCGEN_EVENT_ENABLED_DEF)
#define MCGEN_EVENT_ENABLED_DEF
FORCEINLINE
BOOLEAN
McGenEventEnabled(
    _In_ PMCGEN_TRACE_CONTEXT EnableInfo,
    _In_ PCEVENT_DESCRIPTOR EventDescriptor
    )
{

    return McGenLevelKeywordEnabled(EnableInfo, EventDescriptor->Level, EventDescriptor->Keyword);

}
#endif


//
// EnableCheckMacro
//
#ifndef MCGEN_ENABLE_CHECK
#define MCGEN_ENABLE_CHECK(Context, Descriptor) (Context.IsEnabled &&  McGenEventEnabled(&Context, &Descriptor))
#endif

#if !defined(MCGEN_CONTROL_CALLBACK)
#define MCGEN_CONTROL_CALLBACK

DECLSPEC_NOINLINE __inline
VOID
__stdcall
McGenControlCallbackV2(
    _In_ LPCGUID SourceId,
    _In_ ULONG ControlCode,
    _In_ UCHAR Level,
    _In_ ULONGLONG MatchAnyKeyword,
    _In_ ULONGLONG MatchAllKeyword,
    _In_opt_ PEVENT_FILTER_DESCRIPTOR FilterData,
    _Inout_opt_ PVOID CallbackContext
    )
/*++

Routine Description:

    This is the notification callback for Windows Vista and later.

Arguments:

    SourceId - The GUID that identifies the session that enabled the provider. 

    ControlCode - The parameter indicates whether the provider 
                  is being enabled or disabled.

    Level - The level at which the event is enabled.

    MatchAnyKeyword - The bitmask of keywords that the provider uses to 
                      determine the category of events that it writes.

    MatchAllKeyword - This bitmask additionally restricts the category 
                      of events that the provider writes. 

    FilterData - The provider-defined data.

    CallbackContext - The context of the callback that is defined when the provider 
                      called EtwRegister to register itself.

Remarks:

    ETW calls this function to notify provider of enable/disable

--*/
{
    PMCGEN_TRACE_CONTEXT Ctx = (PMCGEN_TRACE_CONTEXT)CallbackContext;
    ULONG Ix;
#ifndef MCGEN_PRIVATE_ENABLE_CALLBACK_V2
    UNREFERENCED_PARAMETER(SourceId);
    UNREFERENCED_PARAMETER(FilterData);
#endif

    if (Ctx == NULL) {
        return;
    }

    switch (ControlCode) {

        case EVENT_CONTROL_CODE_ENABLE_PROVIDER:
            Ctx->Level = Level;
            Ctx->MatchAnyKeyword = MatchAnyKeyword;
            Ctx->MatchAllKeyword = MatchAllKeyword;
            Ctx->IsEnabled = EVENT_CONTROL_CODE_ENABLE_PROVIDER;

            for (Ix = 0; Ix < Ctx->EnableBitsCount; Ix += 1) {
                if (McGenLevelKeywordEnabled(Ctx, Ctx->EnableLevel[Ix], Ctx->EnableKeyWords[Ix]) != FALSE) {
                    Ctx->EnableBitMask[Ix >> 5] |= (1 << (Ix % 32));
                } else {
                    Ctx->EnableBitMask[Ix >> 5] &= ~(1 << (Ix % 32));
                }
            }
            break;

        case EVENT_CONTROL_CODE_DISABLE_PROVIDER:
            Ctx->IsEnabled = EVENT_CONTROL_CODE_DISABLE_PROVIDER;
            Ctx->Level = 0;
            Ctx->MatchAnyKeyword = 0;
            Ctx->MatchAllKeyword = 0;
            if (Ctx->EnableBitsCount > 0) {
                RtlZeroMemory(Ctx->EnableBitMask, (((Ctx->EnableBitsCount - 1) / 32) + 1) * sizeof(ULONG));
            }
            break;
 
        default:
            break;
    }

#ifdef MCGEN_PRIVATE_ENABLE_CALLBACK_V2
    //
    // Call user defined callback
    //
    MCGEN_PRIVATE_ENABLE_CALLBACK_V2(
        SourceId,
        ControlCode,
        Level,
        MatchAnyKeyword,
        MatchAllKeyword,
        FilterData,
        CallbackContext
        );
#endif
   
    return;
}

#endif
#endif // MCGEN_DISABLE_PROVIDER_CODE_GENERATION
//+
// Provider LolaCommsNative Event Count 19
//+
EXTERN_C __declspec(selectany) const GUID LolaCommsNative = {0x8eb119a9, 0x2fe5, 0x46f5, {0x99, 0x8b, 0xa3, 0x96, 0xca, 0x3f, 0x74, 0xb7}};

//
// Channel
//
#define LolaCommsNative_CHANNEL_Application 0x9

//
// Event Descriptors
//
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Vision_OnConnectionOpened = {0x65, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Vision_OnConnectionOpened_value 0x65
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Vision_OnConnectionClosed = {0x66, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Vision_OnConnectionClosed_value 0x66
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Vision_OnConnectionError = {0x67, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Vision_OnConnectionError_value 0x67
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Vision_OnVisionMessageReceived = {0x68, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Vision_OnVisionMessageReceived_value 0x68
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Vision_OnObstacleMessageReceived = {0x69, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Vision_OnObstacleMessageReceived_value 0x69
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Vision_OnSurfaceMessageReceived = {0x6a, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Vision_OnSurfaceMessageReceived_value 0x6a
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Vision_OnRGBImageMessageReceived = {0x6b, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Vision_OnRGBImageMessageReceived_value 0x6b
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Vision_OnPointCloudMessageReceived = {0x6c, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Vision_OnPointCloudMessageReceived_value 0x6c
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Vision_OnUnknownVisionMessageReceived = {0x6d, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Vision_OnUnknownVisionMessageReceived_value 0x6d
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Pose_OnConnectionOpened = {0xc9, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Pose_OnConnectionOpened_value 0xc9
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Pose_OnConnectionClosed = {0xca, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Pose_OnConnectionClosed_value 0xca
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Pose_OnConnectionError = {0xcb, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Pose_OnConnectionError_value 0xcb
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Pose_OnPoseMessageReceived = {0xcc, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Pose_OnPoseMessageReceived_value 0xcc
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Footsteps_OnConnectionOpened = {0x12d, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Footsteps_OnConnectionOpened_value 0x12d
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Footsteps_OnConnectionClosed = {0x12e, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Footsteps_OnConnectionClosed_value 0x12e
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Footsteps_OnConnectionError = {0x12f, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Footsteps_OnConnectionError_value 0x12f
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Footsteps_OnConnectionHandshakeComplete = {0x130, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Footsteps_OnConnectionHandshakeComplete_value 0x130
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Footsteps_OnFootstepMessageReceived = {0x131, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Footsteps_OnFootstepMessageReceived_value 0x131
EXTERN_C __declspec(selectany) const EVENT_DESCRIPTOR Footsteps_OnOtherMessageReceived = {0x132, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0};
#define Footsteps_OnOtherMessageReceived_value 0x132

//
// Note on Generate Code from Manifest for Windows Vista and above
//
//Structures :  are handled as a size and pointer pairs. The macro for the event will have an extra 
//parameter for the size in bytes of the structure. Make sure that your structures have no extra padding.
//
//Strings: There are several cases that can be described in the manifest. For array of variable length 
//strings, the generated code will take the count of characters for the whole array as an input parameter. 
//
//SID No support for array of SIDs, the macro will take a pointer to the SID and use appropriate 
//GetLengthSid function to get the length.
//

//
// Allow disabling of code generation
//
#ifndef MCGEN_DISABLE_PROVIDER_CODE_GENERATION

//
// Globals 
//


//
// Event Enablement Bits
//

EXTERN_C __declspec(selectany) DECLSPEC_CACHEALIGN ULONG LolaCommsNativeEnableBits[1];
EXTERN_C __declspec(selectany) const ULONGLONG LolaCommsNativeKeywords[1] = {0x0};
EXTERN_C __declspec(selectany) const UCHAR LolaCommsNativeLevels[1] = {0};
EXTERN_C __declspec(selectany) MCGEN_TRACE_CONTEXT LolaCommsNative_Context = {0, 0, 0, 0, 0, 0, 0, 0, 1, LolaCommsNativeEnableBits, LolaCommsNativeKeywords, LolaCommsNativeLevels};

EXTERN_C __declspec(selectany) REGHANDLE LolaCommsNativeHandle = (REGHANDLE)0;

#if !defined(McGenEventRegisterUnregister)
#define McGenEventRegisterUnregister
#pragma warning(push)
#pragma warning(disable:6103)
DECLSPEC_NOINLINE __inline
ULONG __stdcall
McGenEventRegister(
    _In_ LPCGUID ProviderId,
    _In_opt_ PENABLECALLBACK EnableCallback,
    _In_opt_ PVOID CallbackContext,
    _Inout_ PREGHANDLE RegHandle
    )
/*++

Routine Description:

    This function registers the provider with ETW USER mode.

Arguments:
    ProviderId - Provider ID to be register with ETW.

    EnableCallback - Callback to be used.

    CallbackContext - Context for this provider.

    RegHandle - Pointer to registration handle.

Remarks:

    If the handle != NULL will return ERROR_SUCCESS

--*/
{
    ULONG Error;


    if (*RegHandle) {
        //
        // already registered
        //
        return ERROR_SUCCESS;
    }

    Error = EventRegister( ProviderId, EnableCallback, CallbackContext, RegHandle); 

    return Error;
}
#pragma warning(pop)


DECLSPEC_NOINLINE __inline
ULONG __stdcall
McGenEventUnregister(_Inout_ PREGHANDLE RegHandle)
/*++

Routine Description:

    Unregister from ETW USER mode

Arguments:
            RegHandle this is the pointer to the provider context
Remarks:
            If provider has not been registered, RegHandle == NULL,
            return ERROR_SUCCESS
--*/
{
    ULONG Error;


    if(!(*RegHandle)) {
        //
        // Provider has not registerd
        //
        return ERROR_SUCCESS;
    }

    Error = EventUnregister(*RegHandle); 
    *RegHandle = (REGHANDLE)0;
    
    return Error;
}
#endif
//
// Register with ETW Vista +
//
#ifndef EventRegisterLolaCommsNative
#define EventRegisterLolaCommsNative() McGenEventRegister(&LolaCommsNative, McGenControlCallbackV2, &LolaCommsNative_Context, &LolaCommsNativeHandle) 
#endif

//
// UnRegister with ETW
//
#ifndef EventUnregisterLolaCommsNative
#define EventUnregisterLolaCommsNative() McGenEventUnregister(&LolaCommsNativeHandle) 
#endif

//
// Enablement check macro for Vision_OnConnectionOpened
//

#define EventEnabledVision_OnConnectionOpened() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Vision_OnConnectionOpened
//
#define EventWriteVision_OnConnectionOpened(hostname)\
        EventEnabledVision_OnConnectionOpened() ?\
        Template_z(LolaCommsNativeHandle, &Vision_OnConnectionOpened, hostname)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Vision_OnConnectionClosed
//

#define EventEnabledVision_OnConnectionClosed() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Vision_OnConnectionClosed
//
#define EventWriteVision_OnConnectionClosed(hostname)\
        EventEnabledVision_OnConnectionClosed() ?\
        Template_z(LolaCommsNativeHandle, &Vision_OnConnectionClosed, hostname)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Vision_OnConnectionError
//

#define EventEnabledVision_OnConnectionError() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Vision_OnConnectionError
//
#define EventWriteVision_OnConnectionError(hostname)\
        EventEnabledVision_OnConnectionError() ?\
        Template_z(LolaCommsNativeHandle, &Vision_OnConnectionError, hostname)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Vision_OnVisionMessageReceived
//

#define EventEnabledVision_OnVisionMessageReceived() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Vision_OnVisionMessageReceived
//
#define EventWriteVision_OnVisionMessageReceived(info)\
        EventEnabledVision_OnVisionMessageReceived() ?\
        Template_z(LolaCommsNativeHandle, &Vision_OnVisionMessageReceived, info)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Vision_OnObstacleMessageReceived
//

#define EventEnabledVision_OnObstacleMessageReceived() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Vision_OnObstacleMessageReceived
//
#define EventWriteVision_OnObstacleMessageReceived(type, model_id, part_id, action, radius, surface, coeffs)\
        EventEnabledVision_OnObstacleMessageReceived() ?\
        Template_dqqqfdF9(LolaCommsNativeHandle, &Vision_OnObstacleMessageReceived, type, model_id, part_id, action, radius, surface, coeffs)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Vision_OnSurfaceMessageReceived
//

#define EventEnabledVision_OnSurfaceMessageReceived() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Vision_OnSurfaceMessageReceived
//
#define EventWriteVision_OnSurfaceMessageReceived(info)\
        EventEnabledVision_OnSurfaceMessageReceived() ?\
        Template_z(LolaCommsNativeHandle, &Vision_OnSurfaceMessageReceived, info)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Vision_OnRGBImageMessageReceived
//

#define EventEnabledVision_OnRGBImageMessageReceived() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Vision_OnRGBImageMessageReceived
//
#define EventWriteVision_OnRGBImageMessageReceived(info)\
        EventEnabledVision_OnRGBImageMessageReceived() ?\
        Template_z(LolaCommsNativeHandle, &Vision_OnRGBImageMessageReceived, info)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Vision_OnPointCloudMessageReceived
//

#define EventEnabledVision_OnPointCloudMessageReceived() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Vision_OnPointCloudMessageReceived
//
#define EventWriteVision_OnPointCloudMessageReceived(info)\
        EventEnabledVision_OnPointCloudMessageReceived() ?\
        Template_z(LolaCommsNativeHandle, &Vision_OnPointCloudMessageReceived, info)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Vision_OnUnknownVisionMessageReceived
//

#define EventEnabledVision_OnUnknownVisionMessageReceived() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Vision_OnUnknownVisionMessageReceived
//
#define EventWriteVision_OnUnknownVisionMessageReceived(info)\
        EventEnabledVision_OnUnknownVisionMessageReceived() ?\
        Template_z(LolaCommsNativeHandle, &Vision_OnUnknownVisionMessageReceived, info)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Pose_OnConnectionOpened
//

#define EventEnabledPose_OnConnectionOpened() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Pose_OnConnectionOpened
//
#define EventWritePose_OnConnectionOpened(hostname)\
        EventEnabledPose_OnConnectionOpened() ?\
        Template_z(LolaCommsNativeHandle, &Pose_OnConnectionOpened, hostname)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Pose_OnConnectionClosed
//

#define EventEnabledPose_OnConnectionClosed() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Pose_OnConnectionClosed
//
#define EventWritePose_OnConnectionClosed(hostname)\
        EventEnabledPose_OnConnectionClosed() ?\
        Template_z(LolaCommsNativeHandle, &Pose_OnConnectionClosed, hostname)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Pose_OnConnectionError
//

#define EventEnabledPose_OnConnectionError() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Pose_OnConnectionError
//
#define EventWritePose_OnConnectionError(hostname)\
        EventEnabledPose_OnConnectionError() ?\
        Template_z(LolaCommsNativeHandle, &Pose_OnConnectionError, hostname)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Pose_OnPoseMessageReceived
//

#define EventEnabledPose_OnPoseMessageReceived() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Pose_OnPoseMessageReceived
//
#define EventWritePose_OnPoseMessageReceived(info)\
        EventEnabledPose_OnPoseMessageReceived() ?\
        Template_z(LolaCommsNativeHandle, &Pose_OnPoseMessageReceived, info)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Footsteps_OnConnectionOpened
//

#define EventEnabledFootsteps_OnConnectionOpened() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Footsteps_OnConnectionOpened
//
#define EventWriteFootsteps_OnConnectionOpened(hostname)\
        EventEnabledFootsteps_OnConnectionOpened() ?\
        Template_z(LolaCommsNativeHandle, &Footsteps_OnConnectionOpened, hostname)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Footsteps_OnConnectionClosed
//

#define EventEnabledFootsteps_OnConnectionClosed() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Footsteps_OnConnectionClosed
//
#define EventWriteFootsteps_OnConnectionClosed(hostname)\
        EventEnabledFootsteps_OnConnectionClosed() ?\
        Template_z(LolaCommsNativeHandle, &Footsteps_OnConnectionClosed, hostname)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Footsteps_OnConnectionError
//

#define EventEnabledFootsteps_OnConnectionError() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Footsteps_OnConnectionError
//
#define EventWriteFootsteps_OnConnectionError(hostname)\
        EventEnabledFootsteps_OnConnectionError() ?\
        Template_z(LolaCommsNativeHandle, &Footsteps_OnConnectionError, hostname)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Footsteps_OnConnectionHandshakeComplete
//

#define EventEnabledFootsteps_OnConnectionHandshakeComplete() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Footsteps_OnConnectionHandshakeComplete
//
#define EventWriteFootsteps_OnConnectionHandshakeComplete(hostname)\
        EventEnabledFootsteps_OnConnectionHandshakeComplete() ?\
        Template_z(LolaCommsNativeHandle, &Footsteps_OnConnectionHandshakeComplete, hostname)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Footsteps_OnFootstepMessageReceived
//

#define EventEnabledFootsteps_OnFootstepMessageReceived() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Footsteps_OnFootstepMessageReceived
//
#define EventWriteFootsteps_OnFootstepMessageReceived(info)\
        EventEnabledFootsteps_OnFootstepMessageReceived() ?\
        Template_z(LolaCommsNativeHandle, &Footsteps_OnFootstepMessageReceived, info)\
        : ERROR_SUCCESS\

//
// Enablement check macro for Footsteps_OnOtherMessageReceived
//

#define EventEnabledFootsteps_OnOtherMessageReceived() ((LolaCommsNativeEnableBits[0] & 0x00000001) != 0)

//
// Event Macro for Footsteps_OnOtherMessageReceived
//
#define EventWriteFootsteps_OnOtherMessageReceived(info)\
        EventEnabledFootsteps_OnOtherMessageReceived() ?\
        Template_z(LolaCommsNativeHandle, &Footsteps_OnOtherMessageReceived, info)\
        : ERROR_SUCCESS\

#endif // MCGEN_DISABLE_PROVIDER_CODE_GENERATION


//
// Allow Diasabling of code generation
//
#ifndef MCGEN_DISABLE_PROVIDER_CODE_GENERATION

//
// Template Functions 
//
//
//Template from manifest : ConnectionStateChanged
//
#ifndef Template_z_def
#define Template_z_def
ETW_INLINE
ULONG
Template_z(
    _In_ REGHANDLE RegHandle,
    _In_ PCEVENT_DESCRIPTOR Descriptor,
    _In_opt_ PCWSTR  _Arg0
    )
{
#define ARGUMENT_COUNT_z 1

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_z];

    EventDataDescCreate(&EventData[0], 
                        (_Arg0 != NULL) ? _Arg0 : L"NULL",
                        (_Arg0 != NULL) ? (ULONG)((wcslen(_Arg0) + 1) * sizeof(WCHAR)) : (ULONG)sizeof(L"NULL"));

    return EventWrite(RegHandle, Descriptor, ARGUMENT_COUNT_z, EventData);
}
#endif

//
//Template from manifest : ObstacleMessage
//
#ifndef Template_dqqqfdF9_def
#define Template_dqqqfdF9_def
ETW_INLINE
ULONG
Template_dqqqfdF9(
    _In_ REGHANDLE RegHandle,
    _In_ PCEVENT_DESCRIPTOR Descriptor,
    _In_ const signed int  _Arg0,
    _In_ const unsigned int  _Arg1,
    _In_ const unsigned int  _Arg2,
    _In_ const unsigned int  _Arg3,
    _In_ const float  _Arg4,
    _In_ const signed int  _Arg5,
    _In_reads_(9) const float *_Arg6
    )
{
#define ARGUMENT_COUNT_dqqqfdF9 7

    EVENT_DATA_DESCRIPTOR EventData[ARGUMENT_COUNT_dqqqfdF9];

    EventDataDescCreate(&EventData[0], &_Arg0, sizeof(const signed int)  );

    EventDataDescCreate(&EventData[1], &_Arg1, sizeof(const unsigned int)  );

    EventDataDescCreate(&EventData[2], &_Arg2, sizeof(const unsigned int)  );

    EventDataDescCreate(&EventData[3], &_Arg3, sizeof(const unsigned int)  );

    EventDataDescCreate(&EventData[4], &_Arg4, sizeof(const float)  );

    EventDataDescCreate(&EventData[5], &_Arg5, sizeof(const signed int)  );

    EventDataDescCreate(&EventData[6],  _Arg6, sizeof(const float)*9);

    return EventWrite(RegHandle, Descriptor, ARGUMENT_COUNT_dqqqfdF9, EventData);
}
#endif

#endif // MCGEN_DISABLE_PROVIDER_CODE_GENERATION

#if defined(__cplusplus)
};
#endif

#define MSG_level_LogAlways                  0x50000000L
#define MSG_channel_Application              0x90000001L
#define MSG_LolaCommsNative_event_1_message  0xB0010065L
#define MSG_LolaCommsNative_event_2_message  0xB0010066L
