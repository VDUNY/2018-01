#pragma once

namespace LLE {
namespace HDF4 {

public ref class SDLock
{
public:
	SDLock(void);

	static SDLock^ Instance = gcnew SDLock;
};

}
}