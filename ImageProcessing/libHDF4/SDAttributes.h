#pragma once

#include "mfhdf.h"
#include "DFNT.h"
#include "SDAttribute.h"

namespace LLE {
namespace HDF4 {

using namespace System;
using namespace System::Collections;

public ref class SDAttributes abstract
{
public:
	SDAttributes(int32 aId);
	virtual ~SDAttributes(void);

	virtual int32 GetAttributeCount() = 0;

	void Set(String^ aName, String^ aValue);

	void Set(String^ aName, int8 aValue);
	void Set(String^ aName, int16 aValue);
	void Set(String^ aName, int32 aValue);

	void Set(String^ aName, uint8 aValue);
	void Set(String^ aName, uint16 aValue);
	void Set(String^ aName, uint32 aValue);

	void Set(String^ aName, float aValue);
	void Set(String^ aName, double aValue);

	void SetArray(String^ aName, array<int8>^ aValues);
	void SetArray(String^ aName, array<int16>^ aValues);
	void SetArray(String^ aName, array<int32>^ aValues);

	void SetArray(String^ aName, array<uint8>^ aValues);
	void SetArray(String^ aName, array<uint16>^ aValues);
	void SetArray(String^ aName, array<uint32>^ aValues);

	void SetArray(String^ aName, array<float>^ aValues);
	void SetArray(String^ aName, array<double>^ aValues);

	String^ GetString(String^ aName);

	int8 GetInt8(String^ aName);
	int16 GetInt16(String^ aName);
	int32 GetInt32(String^ aName);

	uint8 GetUint8(String^ aName);
	uint16 GetUint16(String^ aName);
	uint32 GetUint32(String^ aName);

	float GetFloat(String^ aName);
	double GetDouble(String^ aName);

	void GetArray(String^ aName, array<int8>^ aValues);
	void GetArray(String^ aName, array<int16>^ aValues);
	void GetArray(String^ aName, array<int32>^ aValues);

	void GetArray(String^ aName, array<uint8>^ aValues);
	void GetArray(String^ aName, array<uint16>^ aValues);
	void GetArray(String^ aName, array<uint32>^ aValues);

	void GetArray(String^ aName, array<float>^ aValues);
	void GetArray(String^ aName, array<double>^ aValues);

	array<SDAttribute^>^ GetList();

protected:
	int32 m_id;

	void SetAttribute(String^ aName, int32 aDataType, int32 aLength, void* aValue);
	void ReadAttribute(String^ aName, int32 aDataType, int32 aLength, void* aValue);
};

}
}