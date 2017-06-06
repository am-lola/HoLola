#include "pch.h"
#include "LolaCommsNative.h"

extern "C"
{
	BSTR SampleFunc(float f)
	{
		return ::SysAllocString(std::to_wstring(f).c_str());
	}
}