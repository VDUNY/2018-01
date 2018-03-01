#pragma once

#include "SDAttributes.h"

namespace LLE {
namespace HDF4 {

public ref class SDFileAttributes :
	public SDAttributes
{
public:
	SDFileAttributes(int32 aId);

	virtual int32 GetAttributeCount() override;
};

}
}