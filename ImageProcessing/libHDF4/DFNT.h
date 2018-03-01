#pragma once

#include "mfhdf.h"

namespace LLE {
namespace HDF4 {

public ref class DFNT
{
public:
	static const int FLOAT32=DFNT_FLOAT32;
	static const int FLOAT=DFNT_FLOAT;
	static const int FLOAT64=DFNT_FLOAT64;
	static const int DOUBLE=DFNT_DOUBLE;
	static const int FLOAT128=DFNT_FLOAT128;
	static const int INT8=DFNT_INT8;
	static const int UINT8=DFNT_UINT8;
	static const int INT16=DFNT_INT16;
	static const int UINT16=DFNT_UINT16;
	static const int INT32=DFNT_INT32;
	static const int UINT32=DFNT_UINT32;
	static const int INT64=DFNT_INT64;
	static const int UINT64=DFNT_UINT64;
	static const int INT128=DFNT_INT128;
	static const int UINT128=DFNT_UINT128;
	static const int UCHAR8=DFNT_UCHAR8;
	static const int UCHAR=DFNT_UCHAR;
	static const int CHAR8=DFNT_CHAR8;
	static const int CHAR=DFNT_CHAR;
	static const int CHAR16=DFNT_CHAR16;
	static const int UCHAR16=DFNT_UCHAR16;
};

}
}