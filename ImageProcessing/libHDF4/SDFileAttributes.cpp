#include <msclr/lock.h>

#include "SDLock.h"
#include "SDFileAttributes.h"

using namespace LLE::HDF4;
using namespace LLE::Util;
using namespace msclr;

SDFileAttributes::SDFileAttributes(int32 aId) :
	SDAttributes(aId)
{
}

int32 SDFileAttributes::GetAttributeCount()
{
	lock myLock(SDLock::Instance);

	int32 dataSetCount;
	int32 attributeCount;
	intn status = SDfileinfo(m_id, &dataSetCount, &attributeCount);
	if (status == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDfileinfo failed", "SDFileAttributes::GetAttributeCount");
	}

	return attributeCount;
}
