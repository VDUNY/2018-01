#include <msclr/lock.h>

#include "SDFileAttributes.h"
#include "SDDataSet.h"
#include "SDLock.h"
#include "SD.h"

using namespace LLE::HDF4;
using namespace LLE::Util;
using namespace msclr;

SD::SD(String^ aPath, int32 aAccess) :
	m_sdId(FAIL)
{
	lock myLock(SDLock::Instance);

	try {
		StringAnsi path(aPath);

		// create the file
		m_sdId = SDstart((char*)path.Ptr(), aAccess);
		if (m_sdId == FAIL) {
			throw gcnew LLEException(LogLevel::Err, "SDstart failed", "SD::SD");
		}

		// create an attribute group
		m_attributes = gcnew SDFileAttributes(m_sdId);
	}
	catch (Exception^) {
		if (m_sdId != FAIL) {
			SDend(m_sdId);
			m_sdId = FAIL;
		}
		if (m_attributes != nullptr) {
			delete m_attributes;
			m_attributes = nullptr;
		}
		throw;
	}
}

SD::~SD(void)
{
	lock myLock(SDLock::Instance);

	if (m_attributes != nullptr) {
		delete m_attributes;
		m_attributes = nullptr;
	}
	if (m_sdId != FAIL) {
		SDend(m_sdId);
		m_sdId = FAIL;
	}
}

SD::!SD(void)
{
	lock myLock(SDLock::Instance);

	if (m_attributes != nullptr) {
		delete m_attributes;
		m_attributes = nullptr;
	}
	if (m_sdId != FAIL) {
		SDend(m_sdId);
		m_sdId = FAIL;
	}
}

int32 SD::GetDataSetCount()
{
	lock myLock(SDLock::Instance);

	int32 dataSetCount;
	int32 attributeCount;
	intn status = SDfileinfo(m_sdId, &dataSetCount, &attributeCount);
	if (status == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDfileinfo failed", "SD::GetDataSetCount");
	}

	return dataSetCount;
}

SDAttributes^ SD::GetAttributes()
{
	lock myLock(SDLock::Instance);

	return m_attributes;
}

SDDataSet^ SD::CreateDataSet(String^ aName, int32 aDataType, array<int32>^ aDimensions)
{
	lock myLock(SDLock::Instance);

	StringAnsi name(aName);
	pin_ptr<int32> dimensions = &aDimensions[0];

	// create a data set
	int32 sdsId = SDcreate(m_sdId, (char*)name.Ptr(), aDataType, aDimensions->Length, (int32*)dimensions);
	if (sdsId == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDcreate failed", "SD::CreateDataSet");
	}

	// return a wrapper object
	return gcnew SDDataSet(sdsId);
}

SDDataSet^ SD::GetDataSet(String^ aName)
{
	lock myLock(SDLock::Instance);

	StringAnsi name(aName);

	// get the data set index associated with the data set name
	int32 index = SDnametoindex(m_sdId, (char*)name.Ptr());
	if (index == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDnametoindex failed", "SD::SelectDataSetByName");
	}

	// return the data set
	return GetDataSet(index);
}

SDDataSet^ SD::GetDataSet(int32 aIndex)
{
	lock myLock(SDLock::Instance);

	// select a data set
	int32 sdsId = SDselect(m_sdId, aIndex);
	if (sdsId == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDselect failed", "SD::SelectDataSetByIndex");
	}

	// return a wrapper object
	return gcnew SDDataSet(sdsId);
}

SDDataSet^ SD::GetDataSetByRef(int32 aRef)
{
	lock myLock(SDLock::Instance);

	// get the data set index associated with the data set name
	int32 index = SDreftoindex(m_sdId, aRef);
	if (index == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDreftoindex failed", "SD::SelectDataSetByRef");
	}

	// return the data set
	return GetDataSet(index);
}
