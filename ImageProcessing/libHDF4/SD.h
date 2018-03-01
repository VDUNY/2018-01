#pragma once

#include "DFACC.h"
#include "DFNT.h"

namespace LLE {
namespace HDF4 {

ref class SDDataSet;
ref class SDAttributes;

using namespace System;

public ref class SD
{
public:
	SD(String^ aPath, int32 aAccess);
	virtual ~SD(void);

	int32 GetDataSetCount();
	SDAttributes^ GetAttributes();

	SDDataSet^ CreateDataSet(String^ aName, int32 aDataType, array<int32>^ aDimensions);
	SDDataSet^ GetDataSet(String^ aName);
	SDDataSet^ GetDataSet(int32 aIndex);
	SDDataSet^ GetDataSetByRef(int32 aRef);

protected:
	SDAttributes^ m_attributes;
	int32 m_sdId;
	!SD(void);
};

}
}