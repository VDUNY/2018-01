#pragma once

#include "mfhdf.h"

namespace LLE {
namespace HDF4 {

ref class SDAttributes;

public ref class SDDataSet
{
public:
	SDDataSet(int32 aSdsId);
	virtual ~SDDataSet(void);

	String^ GetName();
	int GetType();

	SDAttributes^ GetAttributes();

	int32 GetRef();
	array<int32>^ GetDimensions();

	comp_coder_t GetCompressType();

	void SetCompressNone();
	void SetCompressRLE();
	void SetCompressNBit(int32 aType, bool aSignExtend, bool aFillOne, intn aStartBit, intn aBitLength);
	void SetCompressSkpHuff(intn aSkipSize);
	void SetCompressDeflate(intn aLevel);	//GZIP
	void SetCompressDeflate();

	void GetCompressNBit(int32& aType, bool& aSignExtend, bool& aFillOne, intn& aStartBit, intn& aBitLength);
	void GetCompressSkpHuff(intn& aSkipSize);
	void GetCompressDeflate(intn& aLevel);

	generic<class T> void WriteData(
		array<T>^ aData,
		array<int32>^ aStart,
		array<int32>^ aEdges,
		array<int32>^ aStride
	);

	generic<class T> void WriteData(
		array<T, 2>^ aData,
		array<int32>^ aStart,
		array<int32>^ aEdges,
		array<int32>^ aStride
	);

	generic<class T> void WriteData(
		array<T, 3>^ aData,
		array<int32>^ aStart,
		array<int32>^ aEdges,
		array<int32>^ aStride
	);

	void WriteData(
		const void* aData,
		array<int32>^ aStart,
		array<int32>^ aEdges,
		array<int32>^ aStride
	);

	generic<class T> void WriteData(
		array<T>^ aData,
		array<int32>^ aStart,
		array<int32>^ aEdges
	);

	generic<class T> void WriteData(
		array<T, 2>^ aData,
		array<int32>^ aStart,
		array<int32>^ aEdges
	);

	generic<class T> void WriteData(
		array<T, 3>^ aData,
		array<int32>^ aStart,
		array<int32>^ aEdges
	);

	void WriteData(
		const void* aData,
		array<int32>^ aStart,
		array<int32>^ aEdges
	);

	generic<class T> void ReadData(
		array<T>^ aData,
		array<int32>^ aStart,
		array<int32>^ aEdges,
		array<int32>^ aStride
	);

	generic<class T> void ReadData(
		array<T, 2>^ aData,
		array<int32>^ aStart,
		array<int32>^ aEdges,
		array<int32>^ aStride
	);

	generic<class T> void ReadData(
		array<T, 3>^ aData,
		array<int32>^ aStart,
		array<int32>^ aEdges,
		array<int32>^ aStride
	);

	void ReadData(
		void* aData,
		array<int32>^ aStart,
		array<int32>^ aEdges,
		array<int32>^ aStride
	);

	generic<class T> void ReadData(
		array<T>^ aData,
		array<int32>^ aStart,
		array<int32>^ aEdges
	);

	generic<class T> void ReadData(
		array<T, 2>^ aData,
		array<int32>^ aStart,
		array<int32>^ aEdges
	);

	generic<class T> void ReadData(
		array<T, 3>^ aData,
		array<int32>^ aStart,
		array<int32>^ aEdges
	);

	void ReadData(
		void* aData,
		array<int32>^ aStart,
		array<int32>^ aEdges
	);

protected:
	SDAttributes^ m_attributes;
	String^ m_name;
	int32 m_dataType;
	int32 m_sdsId;

	!SDDataSet(void);
	void SetCompress(comp_coder_t aCompType, comp_info* aCompInfo);
	void GetCompress(comp_coder_t* aCompType, comp_info* aCompInfo);
};

}
}