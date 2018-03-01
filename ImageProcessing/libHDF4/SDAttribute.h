#pragma once

#include "mfhdf.h"
#include "DFNT.h"

namespace LLE {
namespace HDF4 {

using namespace System;
using namespace System::Collections;

public ref class SDAttribute abstract
{
public:
	String^ name;
	SDAttribute(String^ name) : name(name) {};
};

public ref class SDInt8Attribute : public SDAttribute
{
public:
	int8 value;
	SDInt8Attribute(String^ name, int8 val) : SDAttribute(name), value(val) {};
};

public ref class SDInt16Attribute : public SDAttribute
{
public:
	int16 value;
	SDInt16Attribute(String^ name, int16 val) : SDAttribute(name), value(val) {};
};

public ref class SDInt32Attribute : public SDAttribute
{
public:
	int32 value;
	SDInt32Attribute(String^ name, int32 val) : SDAttribute(name), value(val) {};
};

public ref class SDUint8Attribute : public SDAttribute
{
public:
	uint8 value;
	SDUint8Attribute(String^ name, uint8 val) : SDAttribute(name), value(val) {};
};

public ref class SDUint16Attribute : public SDAttribute
{
public:
	uint16 value;
	SDUint16Attribute(String^ name, uint16 val) : SDAttribute(name), value(val) {};
};

public ref class SDUint32Attribute : public SDAttribute
{
public:
	uint32 value;
	SDUint32Attribute(String^ name, uint32 val) : SDAttribute(name), value(val) {};
};

public ref class SDFloatAttribute : public SDAttribute
{
public:
	float value;
	SDFloatAttribute(String^ name, float val) : SDAttribute(name), value(val) {};
};

public ref class SDDoubleAttribute : public SDAttribute
{
public:
	double value;
	SDDoubleAttribute(String^ name, double val) : SDAttribute(name), value(val) {};
};

public ref class SDStringAttribute : public SDAttribute
{
public:
	String^ value;
	SDStringAttribute(String^ name, String^ val) : SDAttribute(name), value(val) {};
};

public ref class SDInt8ArrayAttribute : public SDAttribute
{
public:
	array<int8>^ value;
	SDInt8ArrayAttribute(String^ name, array<int8>^ val) : SDAttribute(name), value(val) {};
};

public ref class SDInt16ArrayAttribute : public SDAttribute
{
public:
	array<int16>^ value;
	SDInt16ArrayAttribute(String^ name, array<int16>^ val) : SDAttribute(name), value(val) {};
};

public ref class SDInt32ArrayAttribute : public SDAttribute
{
public:
	array<int32>^ value;
	SDInt32ArrayAttribute(String^ name, array<int32>^ val) : SDAttribute(name), value(val) {};
};

public ref class SDUint8ArrayAttribute : public SDAttribute
{
public:
	array<uint8>^ value;
	SDUint8ArrayAttribute(String^ name, array<uint8>^ val) : SDAttribute(name), value(val) {};
};

public ref class SDUint16ArrayAttribute : public SDAttribute
{
public:
	array<uint16>^ value;
	SDUint16ArrayAttribute(String^ name, array<uint16>^ val) : SDAttribute(name), value(val) {};
};

public ref class SDUint32ArrayAttribute : public SDAttribute
{
public:
	array<uint32>^ value;
	SDUint32ArrayAttribute(String^ name, array<uint32>^ val) : SDAttribute(name), value(val) {};
};

public ref class SDFloatArrayAttribute : public SDAttribute
{
public:
	array<float>^ value;
	SDFloatArrayAttribute(String^ name, array<float>^ val) : SDAttribute(name), value(val) {};
};

public ref class SDDoubleArrayAttribute : public SDAttribute
{
public:
	array<double>^ value;
	SDDoubleArrayAttribute(String^ name, array<double>^ val) : SDAttribute(name), value(val) {};
};

}
}