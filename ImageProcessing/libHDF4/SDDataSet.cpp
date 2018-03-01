#include <msclr/lock.h>

#include "SDLock.h"
#include "SDDataSetAttributes.h"
#include "SDDataSet.h"

using namespace LLE::HDF4;
using namespace LLE::Util;
using namespace msclr;

SDDataSet::SDDataSet(int32 aSdsId) :
	m_sdsId(aSdsId)
{
	char name[MAX_NC_NAME];
	int32 dimensions[MAX_VAR_DIMS];
	int32 rank;
	int32 dataType;
	int32 attributeCount;
	intn status = SDgetinfo(m_sdsId, name, &rank, dimensions, &dataType, &attributeCount);
	if (status == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDfileinfo failed", "SDDataSetAttributes::GetName");
	}
	m_name = gcnew String(name);
	m_dataType = dataType;
	m_attributes = gcnew SDDataSetAttributes(aSdsId);
}

SDDataSet::~SDDataSet(void)
{
	lock myLock(SDLock::Instance);

	if (m_attributes != nullptr) {
		delete m_attributes;
		m_attributes = nullptr;
	}
	if (m_sdsId != FAIL) {
		SDendaccess(m_sdsId);
		m_sdsId = FAIL;
	}
}

SDDataSet::!SDDataSet(void)
{
	lock myLock(SDLock::Instance);

	if (m_attributes != nullptr) {
		delete m_attributes;
		m_attributes = nullptr;
	}
	if (m_sdsId != FAIL) {
		SDendaccess(m_sdsId);
		m_sdsId = FAIL;
	}
}

String^ SDDataSet::GetName()
{
	return m_name;
}

int SDDataSet::GetType()
{
	return m_dataType;
}

SDAttributes^ SDDataSet::GetAttributes()
{
	return m_attributes;
}

int32 SDDataSet::GetRef()
{
	lock myLock(SDLock::Instance);

	return SDidtoref(m_sdsId);
}

array<int32>^ SDDataSet::GetDimensions()
{
	lock myLock(SDLock::Instance);

	char name[MAX_NC_NAME];
	int32 dimensions[MAX_VAR_DIMS];
	int32 rank;
	int32 dataType;
	int32 attributeCount;
	intn status = SDgetinfo(m_sdsId, name, &rank, dimensions, &dataType, &attributeCount);
	if (status == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDfileinfo failed", "SDDataSetAttributes::GetAttributeCount");
	}

	array<int32>^ tmpDimensions = gcnew array<int32>(rank);
	int i;
	for (i=0; i<rank; i++) {
		tmpDimensions[i] = dimensions[i];
	}

	return tmpDimensions;
}

generic<class T> void SDDataSet::WriteData(
	array<T>^ aData,
	array<int32>^ aStart,
	array<int32>^ aEdges,
	array<int32>^ aStride
)
{
	pin_ptr<T> data = &aData[0];
	WriteData(data, aStart, aEdges, aStride);
}

generic<class T> void SDDataSet::WriteData(
	array<T, 2>^ aData,
	array<int32>^ aStart,
	array<int32>^ aEdges,
	array<int32>^ aStride
)
{
	pin_ptr<T> data = &aData[0, 0];
	WriteData(data, aStart, aEdges, aStride);
}

generic<class T> void SDDataSet::WriteData(
	array<T, 3>^ aData,
	array<int32>^ aStart,
	array<int32>^ aEdges,
	array<int32>^ aStride
)
{
	pin_ptr<T> data = &aData[0, 0, 0];
	WriteData(data, aStart, aEdges, aStride);
}

void SDDataSet::WriteData(
	const void* aData,
	array<int32>^ aStart,
	array<int32>^ aEdges,
	array<int32>^ aStride
)
{
	lock myLock(SDLock::Instance);

	// make sure all the array dimensions are correct
	if (aEdges->Length != aStart->Length) {
		throw gcnew LLEException(LogLevel::Err, "Invalid edges parameter", "SDDataSet::WriteData");
	}
	if (aStride->Length != aStart->Length) {
		throw gcnew LLEException(LogLevel::Err, "Invalid stride parameter", "SDDataSet::WriteData");
	}

	// write the data
	pin_ptr<int32> start = &aStart[0];
	pin_ptr<int32> edges = &aEdges[0];
	pin_ptr<int32> stride = &aStride[0];
	intn status = SDwritedata(m_sdsId, (int32*)start, (int32*)stride, (int32*)edges, (void*)aData);
	if (status == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDwritedata failed", "SDDataSet::WriteData");
	}
}

generic<class T> void SDDataSet::WriteData(
	array<T>^ aData,
	array<int32>^ aStart,
	array<int32>^ aEdges
)
{
	pin_ptr<T> data = &aData[0];
	WriteData(data, aStart, aEdges);
}

generic<class T> void SDDataSet::WriteData(
	array<T, 2>^ aData,
	array<int32>^ aStart,
	array<int32>^ aEdges
)
{
	pin_ptr<T> data = &aData[0, 0];
	WriteData(data, aStart, aEdges);
}

generic<class T> void SDDataSet::WriteData(
	array<T, 3>^ aData,
	array<int32>^ aStart,
	array<int32>^ aEdges
)
{
	pin_ptr<T> data = &aData[0, 0, 0];
	WriteData(data, aStart, aEdges);
}

void SDDataSet::WriteData(
	const void* aData,
	array<int32>^ aStart,
	array<int32>^ aEdges
)
{
	lock myLock(SDLock::Instance);

	// make sure all the array dimensions are correct
	if (aEdges->Length != aStart->Length) {
		throw gcnew LLEException(LogLevel::Err, "Invalid edges parameter", "SDDataSet::WriteData");
	}

	// write the data
	pin_ptr<int32> start = &aStart[0];
	pin_ptr<int32> edges = &aEdges[0];
	intn status = SDwritedata(m_sdsId, (int32*)start, NULL, (int32*)edges, (void*)aData);
	if (status == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDwritedata failed", "SDDataSet::WriteData");
	}
}

generic<class T> void SDDataSet::ReadData(
	array<T>^ aData,
	array<int32>^ aStart,
	array<int32>^ aEdges,
	array<int32>^ aStride
)
{
	pin_ptr<T> data = &aData[0];
	ReadData(data, aStart, aEdges, aStride);
}

generic<class T> void SDDataSet::ReadData(
	array<T, 2>^ aData,
	array<int32>^ aStart,
	array<int32>^ aEdges,
	array<int32>^ aStride
)
{
	pin_ptr<T> data = &aData[0, 0];
	ReadData(data, aStart, aEdges, aStride);
}

generic<class T> void SDDataSet::ReadData(
	array<T, 3>^ aData,
	array<int32>^ aStart,
	array<int32>^ aEdges,
	array<int32>^ aStride
)
{
	pin_ptr<T> data = &aData[0, 0, 0];
	ReadData(data, aStart, aEdges, aStride);
}

void SDDataSet::ReadData(
	void* aData,
	array<int32>^ aStart,
	array<int32>^ aEdges,
	array<int32>^ aStride
)
{
	lock myLock(SDLock::Instance);

	// make sure all the array dimensions are correct
	if (aEdges->Length != aStart->Length) {
		throw gcnew LLEException(LogLevel::Err, "Invalid edges parameter", "SDDataSet::ReadData");
	}
	if (aStride->Length != aStart->Length) {
		throw gcnew LLEException(LogLevel::Err, "Invalid stride parameter", "SDDataSet::ReadData");
	}

	// write the data
	pin_ptr<int32> start = &aStart[0];
	pin_ptr<int32> edges = &aEdges[0];
	pin_ptr<int32> stride = &aStride[0];
	intn status = SDreaddata(m_sdsId, (int32*)start, (int32*)stride, (int32*)edges, aData);
	if (status == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDreaddata failed", "SDDataSet::ReadData");
	}
}

generic<class T> void SDDataSet::ReadData(
	array<T>^ aData,
	array<int32>^ aStart,
	array<int32>^ aEdges
)
{
	pin_ptr<T> data = &aData[0];
	ReadData(data, aStart, aEdges);
}

generic<class T> void SDDataSet::ReadData(
	array<T, 2>^ aData,
	array<int32>^ aStart,
	array<int32>^ aEdges
)
{
	pin_ptr<T> data = &aData[0, 0];
	ReadData(data, aStart, aEdges);
}

generic<class T> void SDDataSet::ReadData(
	array<T, 3>^ aData,
	array<int32>^ aStart,
	array<int32>^ aEdges
)
{
	pin_ptr<T> data = &aData[0, 0, 0];
	ReadData(data, aStart, aEdges);
}

void SDDataSet::ReadData(
	void* aData,
	array<int32>^ aStart,
	array<int32>^ aEdges
)
{
	lock myLock(SDLock::Instance);

	// make sure all the array dimensions are correct
	if (aEdges->Length != aStart->Length) {
		throw gcnew LLEException(LogLevel::Err, "Invalid edges parameter", "SDDataSet::ReadData");
	}

	// write the data
	pin_ptr<int32> start = &aStart[0];
	pin_ptr<int32> edges = &aEdges[0];
	intn status = SDreaddata(m_sdsId, (int32*)start, NULL, (int32*)edges, aData);
	if (status == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDreaddata failed", "SDDataSet::ReadData");
	}
}

comp_coder_t SDDataSet::GetCompressType()
{
	comp_coder_t compType;
	comp_info compInfo;

	GetCompress(&compType, &compInfo);

	return compType;
}

void SDDataSet::SetCompressNone()
{
	SetCompress(COMP_CODE_NONE, 0);
}

void SDDataSet::SetCompressRLE()
{
	SetCompress(COMP_CODE_RLE, 0);
}

void SDDataSet::SetCompressNBit(int32 aType, bool aSignExtend, bool aFillOne, intn aStartBit, intn aBitLength)
{
	comp_info compInfo;
	compInfo.nbit.nt = aType;
	compInfo.nbit.sign_ext = aSignExtend ? 1 : 0;
	compInfo.nbit.fill_one = aFillOne ? 1 : 0;
	compInfo.nbit.start_bit = aStartBit;
	compInfo.nbit.bit_len = aBitLength;

	SetCompress(COMP_CODE_NBIT, &compInfo);
}

void SDDataSet::SetCompressSkpHuff(intn aSkipSize)
{
	comp_info compInfo;
	compInfo.skphuff.skp_size = aSkipSize;

	SetCompress(COMP_CODE_SKPHUFF, &compInfo);
}

void SDDataSet::SetCompressDeflate()
{
	SetCompressDeflate(6);
}

void SDDataSet::SetCompressDeflate(intn aLevel)
{
	comp_info compInfo;
	compInfo.deflate.level = aLevel;

	SetCompress(COMP_CODE_DEFLATE, &compInfo);
}

void SDDataSet::GetCompressNBit(int32& aType, bool& aSignExtend, bool& aFillOne, intn& aStartBit, intn& aBitLength)
{
	comp_coder_t compType;
	comp_info compInfo;

	GetCompress(&compType, &compInfo);
	if (compType != COMP_CODE_NBIT) {
		throw gcnew LLEException(LogLevel::Err, "Compression type mismatch", "SDDataSet::GetCompressNBit");
	}
	aType = compInfo.nbit.nt;
	aSignExtend = (compInfo.nbit.sign_ext != 0);
	aFillOne = (compInfo.nbit.fill_one != 0);
	aStartBit = compInfo.nbit.start_bit;
	aBitLength = compInfo.nbit.bit_len;
}

void SDDataSet::GetCompressSkpHuff(intn& aSkipSize)
{
	comp_coder_t compType;
	comp_info compInfo;

	GetCompress(&compType, &compInfo);
	if (compType != COMP_CODE_SKPHUFF) {
		throw gcnew LLEException(LogLevel::Err, "Compression type mismatch", "SDDataSet::GetCompressSkpHuff");
	}
	aSkipSize = compInfo.skphuff.skp_size;
}

void SDDataSet::GetCompressDeflate(intn& aLevel)
{
	comp_coder_t compType;
	comp_info compInfo;

	GetCompress(&compType, &compInfo);
	if (compType != COMP_CODE_DEFLATE) {
		throw gcnew LLEException(LogLevel::Err, "Compression type mismatch", "SDDataSet::GetCompressDeflate");
	}
	aLevel = compInfo.deflate.level;
}

void SDDataSet::SetCompress(comp_coder_t aCompType, comp_info* aCompInfo)
{
	lock myLock(SDLock::Instance);

	intn status = SDsetcompress(m_sdsId, aCompType, aCompInfo);
	if (status == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDsetcompress failed", "SDDataSet::SetCompress");
	}
}

void SDDataSet::GetCompress(comp_coder_t* aCompType, comp_info* aCompInfo)
{
	lock myLock(SDLock::Instance);

	intn status = SDgetcompress(m_sdsId, aCompType, aCompInfo);
	if (status == FAIL) {
		throw gcnew LLEException(LogLevel::Err, "SDgetcompress failed", "SDDataSet::GetCompress");
	}
}
