#include <msclr/lock.h>

#include "SDLock.h"
#include "SDDataSetAttributes.h"

using namespace LLE::HDF4;
using namespace LLE::Util;
using namespace msclr;

SDDataSetAttributes::SDDataSetAttributes(int32 aId) :
	SDAttributes(aId)
{
}

int32 SDDataSetAttributes::GetAttributeCount()
{
	lock myLock(SDLock::Instance);

	char name[MAX_NC_NAME];
	int32 dimensions[MAX_VAR_DIMS];
	int32 rank;
	int32 dataType;
	int32 attributeCount;
	intn status = SDgetinfo(m_id, name, &rank, dimensions, &dataType, &attributeCount);
	if (status == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDfileinfo failed", "SDDataSetAttributes::GetAttributeCount");
	}

	return attributeCount;
}
