#pragma once

#include "mfhdf.h"

namespace LLE {
namespace HDF4 {

public ref class DFACC
{
public:
	static const int READ=DFACC_READ;
	static const int WRITE=DFACC_WRITE;
	static const int CREATE=DFACC_CREATE;
	static const int ALL=DFACC_ALL;
	static const int RDONLY=DFACC_RDONLY;
	static const int RDWR=DFACC_RDWR;
	static const int CLOBBER=DFACC_CLOBBER;
	static const int BUFFER=DFACC_BUFFER;
	static const int APPENDABLE=DFACC_APPENDABLE;
	static const int CURRENT=DFACC_CURRENT;
	static const int OLD=DFACC_OLD;
};

}
}
