#pragma once

#include "SDAttributes.h"

namespace LLE {
namespace HDF4 {

public ref class SDDataSetAttributes :
	public SDAttributes
{
public:
	SDDataSetAttributes(int32 aId);

	virtual int32 GetAttributeCount() override;
};

}
}