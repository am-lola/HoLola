#pragma once
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

extern "C"
{
	LOLACOMMMDLL_API BSTR SampleFunc(float f);
}