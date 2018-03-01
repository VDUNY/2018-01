#include <msclr/lock.h>

#include "SDLock.h"
#include "SDAttributes.h"

using namespace LLE::HDF4;
using namespace LLE::Util;
using namespace msclr;

SDAttributes::SDAttributes(int32 aId) :
	m_id(aId)
{
}

SDAttributes::~SDAttributes(void)
{
}

void SDAttributes::SetAttribute(String^ aName, int32 aDataType, int32 aLength, void* aValue)
{
	lock myLock(SDLock::Instance);

	StringAnsi name(aName);

	intn status = SDsetattr(
		m_id,
		(char*)name.Ptr(),
		aDataType,
		aLength,
		aValue
	);
	if (status == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDsetattr failed", "SDAttributes::SetAttribute");
	}
}

void SDAttributes::Set(String^ aName, String^ aValue)
{
	// HDF library doesn't like empty attribute values, so make sure something
	// is in the string
	if (aValue == nullptr || aValue->Length == 0) {
		aValue = " ";
	}

	StringAnsi value(aValue);

	SetAttribute(aName, DFNT_CHAR8, (int) strlen((char*)value.Ptr()), (char*)value.Ptr());
}

void SDAttributes::Set(String^ aName, int8 aValue)
{
	array<int8>^ values = { aValue };
	SetArray(aName, values);
}

void SDAttributes::Set(String^ aName, int16 aValue)
{
	array<int16>^ values = { aValue };
	SetArray(aName, values);
}

void SDAttributes::Set(String^ aName, int32 aValue)
{
	array<int32>^ values = { aValue };
	SetArray(aName, values);
}

void SDAttributes::Set(String^ aName, uint8 aValue)
{
	array<uint8>^ values = { aValue };
	SetArray(aName, values);
}

void SDAttributes::Set(String^ aName, uint16 aValue)
{
	array<uint16>^ values = { aValue };
	SetArray(aName, values);
}

void SDAttributes::Set(String^ aName, uint32 aValue)
{
	array<uint32>^ values = { aValue };
	SetArray(aName, values);
}

void SDAttributes::Set(String^ aName, float aValue)
{
	array<float>^ values = { aValue };
	SetArray(aName, values);
}

void SDAttributes::Set(String^ aName, double aValue)
{
	array<double>^ values = { aValue };
	SetArray(aName, values);
}

void SDAttributes::SetArray(String^ aName, array<int8>^ aValues)
{
	pin_ptr<int8> values = &aValues[0];
	SetAttribute(aName, DFNT_INT8, aValues->Length, values);
}

void SDAttributes::SetArray(String^ aName, array<int16>^ aValues)
{
	pin_ptr<int16> values = &aValues[0];
	SetAttribute(aName, DFNT_INT16, aValues->Length, values);
}

void SDAttributes::SetArray(String^ aName, array<int32>^ aValues)
{
	pin_ptr<int32> values = &aValues[0];
	SetAttribute(aName, DFNT_INT32, aValues->Length, values);
}

void SDAttributes::SetArray(String^ aName, array<uint8>^ aValues)
{
	pin_ptr<uint8> values = &aValues[0];
	SetAttribute(aName, DFNT_UINT8, aValues->Length, values);
}

void SDAttributes::SetArray(String^ aName, array<uint16>^ aValues)
{
	pin_ptr<uint16> values = &aValues[0];
	SetAttribute(aName, DFNT_UINT16, aValues->Length, values);
}

void SDAttributes::SetArray(String^ aName, array<uint32>^ aValues)
{
	pin_ptr<uint32> values = &aValues[0];
	SetAttribute(aName, DFNT_UINT32, aValues->Length, values);
}

void SDAttributes::SetArray(String^ aName, array<float>^ aValues)
{
	pin_ptr<float> values = &aValues[0];
	SetAttribute(aName, DFNT_FLOAT, aValues->Length, values);
}

void SDAttributes::SetArray(String^ aName, array<double>^ aValues)
{
	pin_ptr<double> values = &aValues[0];
	SetAttribute(aName, DFNT_DOUBLE, aValues->Length, values);
}

void SDAttributes::ReadAttribute(String^ aName, int32 aDataType, int32 aLength, void* aValue)
{
	lock myLock(SDLock::Instance);

	StringAnsi name(aName);

	// find the attribute
	int32 index = SDfindattr(m_id, (char*)name.Ptr());
	if (index == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDfindattr failed", "SDAttributes::ReadAttribute");
	}

	// get the attribute's info
	int32 dataType;
	int32 length;
	intn status = SDattrinfo(m_id, index, (char*)name.Ptr(), &dataType, &length);
	if(dataType == DFNT_CHAR8)  dataType = DFNT_UINT8;
	if (status == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDattrinfo failed", "SDAttributes::ReadAttribute");
	}
	if (dataType != aDataType) {
		throw gcnew LLEException(LogLevel::Err, "Incorrect data type", "SDAttributes::ReadAttribute");
	}
	if (length != aLength) {
		throw gcnew LLEException(LogLevel::Err, "Incorrect length", "SDAttributes::ReadAttribute");
	}

	// read the attribute
	status = SDreadattr(m_id, index, aValue);
	if (status == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDreadattr failed", "SDAttributes::ReadAttribute");
	}
}

String^ SDAttributes::GetString(String^ aName)
{
	lock myLock(SDLock::Instance);

	StringAnsi name(aName);

	// find the attribute
	int32 index = SDfindattr(m_id, (char*)name.Ptr());
	if (index == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDfindattr failed", "SDAttributes::GetString");
	}

	// get the attribute's info
	int32 dataType;
	int32 length;
	intn status = SDattrinfo(m_id, index, (char*)name.Ptr(), &dataType, &length);
	if (status == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDattrinfo failed", "SDAttributes::GetString");
	}

	// create an array to receive the string
	char *data = new char[length];

	// read the attribute
	status = SDreadattr(m_id, index, data);
	if (status == FAIL) {
		delete [] data;
		throw gcnew LLEException(LogLevel::Err, "SDreadattr failed", "SDAttributes::GetString");
	}

	// return the string to the caller
	String^ value = gcnew String(data, 0, length);
	delete [] data;

	return value;
}

int8 SDAttributes::GetInt8(String^ aName)
{
	int8 value;
	pin_ptr<int8> ptr = &value;
	ReadAttribute(aName, DFNT_INT8, 1, ptr);
	return value;
}

int16 SDAttributes::GetInt16(String^ aName)
{
	int16 value;
	pin_ptr<int16> ptr = &value;
	ReadAttribute(aName, DFNT_INT16, 1, ptr);
	return value;
}

int32 SDAttributes::GetInt32(String^ aName)
{
	int32 value;
	pin_ptr<int32> ptr = &value;
	ReadAttribute(aName, DFNT_INT32, 1, ptr);
	return value;
}

uint8 SDAttributes::GetUint8(String^ aName)
{
	uint8 value;
	pin_ptr<uint8> ptr = &value;
	ReadAttribute(aName, DFNT_UINT8, 1, ptr);
	return value;
}

uint16 SDAttributes::GetUint16(String^ aName)
{
	uint16 value;
	pin_ptr<uint16> ptr = &value;
	ReadAttribute(aName, DFNT_UINT16, 1, ptr);
	return value;
}

uint32 SDAttributes::GetUint32(String^ aName)
{
	uint32 value;
	pin_ptr<uint32> ptr = &value;
	ReadAttribute(aName, DFNT_UINT32, 1, ptr);
	return value;
}

float SDAttributes::GetFloat(String^ aName)
{
	float value;
	pin_ptr<float> ptr = &value;
	ReadAttribute(aName, DFNT_FLOAT, 1, ptr);
	return value;
}

double SDAttributes::GetDouble(String^ aName)
{
	double value;
	pin_ptr<double> ptr = &value;
	ReadAttribute(aName, DFNT_DOUBLE, 1, ptr);
	return value;
}

void SDAttributes::GetArray(String^ aName, array<int8>^ aValues)
{
	pin_ptr<int8> values = &aValues[0];
	ReadAttribute(aName, DFNT_INT8, aValues->Length, values);
}

void SDAttributes::GetArray(String^ aName, array<int16>^ aValues)
{
	pin_ptr<int16> values = &aValues[0];
	ReadAttribute(aName, DFNT_INT16, aValues->Length, values);
}

void SDAttributes::GetArray(String^ aName, array<int32>^ aValues)
{
	pin_ptr<int32> values = &aValues[0];
	ReadAttribute(aName, DFNT_INT32, aValues->Length, values);
}

void SDAttributes::GetArray(String^ aName, array<uint8>^ aValues)
{
	pin_ptr<uint8> values = &aValues[0];
	ReadAttribute(aName, DFNT_UINT8, aValues->Length, values);
}

void SDAttributes::GetArray(String^ aName, array<uint16>^ aValues)
{
	pin_ptr<uint16> values = &aValues[0];
	ReadAttribute(aName, DFNT_UINT16, aValues->Length, values);
}

void SDAttributes::GetArray(String^ aName, array<uint32>^ aValues)
{
	pin_ptr<uint32> values = &aValues[0];
	ReadAttribute(aName, DFNT_UINT32, aValues->Length, values);
}

void SDAttributes::GetArray(String^ aName, array<float>^ aValues)
{
	pin_ptr<float> values = &aValues[0];
	ReadAttribute(aName, DFNT_FLOAT, aValues->Length, values);
}

void SDAttributes::GetArray(String^ aName, array<double>^ aValues)
{
	pin_ptr<double> values = &aValues[0];
	ReadAttribute(aName, DFNT_DOUBLE, aValues->Length, values);
}

// reads all the attributes and creates and returns a name/value list
array<SDAttribute^>^ SDAttributes::GetList()
{
	lock myLock(SDLock::Instance);
	array<SDAttribute^>^ list;
	char name[MAX_NC_NAME];
	int32 dataType;
	int32 length;

	int count = GetAttributeCount();
	list = gcnew array<SDAttribute^>(count);

	// read in all the attributes and create the list
	for(int i = 0; i < count; i++)
	{
		intn status = SDattrinfo(m_id, i, name, &dataType, &length);
		if (status == FAIL) {
			throw gcnew LLEException(LogLevel::Err, "SDattrinfo failed", "SDAttributes::GetList");
		}

		SDAttribute^ attr;
		String^ sName = gcnew String(name); 

		// create single value attribute
		if(length == 1)
		{
			switch(dataType)
			{
			case DFNT_CHAR :
				attr = gcnew SDInt8Attribute(sName, GetUint8(sName));
				break;
			case DFNT_INT8 :
				attr = gcnew SDInt8Attribute(sName, GetInt8(sName));
				break;
			case DFNT_INT16 :
				attr = gcnew SDInt16Attribute(sName, GetInt16(sName));
				break;
			case DFNT_INT32 :
				attr = gcnew SDInt32Attribute(sName, GetInt32(sName));
				break;
			case DFNT_UINT8 :
				attr = gcnew SDUint8Attribute(sName, GetUint8(sName));
				break;
			case DFNT_UINT16 :
				attr = gcnew SDUint16Attribute(sName, GetUint16(sName));
				break;
			case DFNT_UINT32 :
				attr = gcnew SDUint32Attribute(sName, GetUint32(sName));
				break;
			case DFNT_FLOAT :
				attr = gcnew SDFloatAttribute(sName, GetFloat(sName));
				break;
			case DFNT_DOUBLE :
				attr = gcnew SDDoubleAttribute(sName, GetDouble(sName));
				break;
			default:
				throw gcnew LLEException(LogLevel::Err, "Unknown Attribute type", "SDAttributes::GetList");
			}
		} 
		// create array attribute
		else if(length > 1)
		{
			switch(dataType)
			{
			case DFNT_CHAR :
				{
				String^ str = GetString(sName);
				attr = gcnew SDStringAttribute(sName, str);
				}
				break;
			case DFNT_INT8 :
				{
				array<int8>^ vals = gcnew array<int8>(length);
				GetArray(sName, vals);
				attr = gcnew SDInt8ArrayAttribute(sName, vals);
				}
				break;
			case DFNT_INT16 :
				{
				array<int16>^ vals = gcnew array<int16>(length);
				GetArray(sName, vals);
				attr = gcnew SDInt16ArrayAttribute(sName, vals);
				}
				break;
			case DFNT_INT32 :
				{
				array<int32>^ vals = gcnew array<int32>(length);
				GetArray(sName, vals);
				attr = gcnew SDInt32ArrayAttribute(sName, vals);
				}
				break;
			case DFNT_UINT8 :
				{
				array<uint8>^ vals = gcnew array<uint8>(length);
				GetArray(sName, vals);
				attr = gcnew SDUint8ArrayAttribute(sName, vals);
				}
				break;
			case DFNT_UINT16 :
				{
				array<uint16>^ vals = gcnew array<uint16>(length);
				GetArray(sName, vals);
				attr = gcnew SDUint16ArrayAttribute(sName, vals);
				}
				break;
			case DFNT_UINT32 :
				{
				array<uint32>^ vals = gcnew array<uint32>(length);
				GetArray(sName, vals);
				attr = gcnew SDUint32ArrayAttribute(sName, vals);
				}
				break;
			case DFNT_FLOAT :
				{
				array<float>^ vals = gcnew array<float>(length);
				GetArray(sName, vals);
				attr = gcnew SDFloatArrayAttribute(sName, vals);
				}
				break;
			case DFNT_DOUBLE :
				{
				array<double>^ vals = gcnew array<double>(length);
				GetArray(sName, vals);
				attr = gcnew SDDoubleArrayAttribute(sName, vals);
				}
				break;
			default:
				throw gcnew LLEException(LogLevel::Err, "Unknown Attribute type", "SDAttributes::GetList");
			}
		} else
		{
			throw gcnew LLEException(LogLevel::Err, "Invalid Attribute length", "SDAttributes::GetList");
		}

		list[i] = attr;
	}

	return list;
}
