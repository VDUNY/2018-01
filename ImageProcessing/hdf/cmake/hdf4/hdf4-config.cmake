#-----------------------------------------------------------------------------
# HDF4 Config file for compiling against hdf4 install directory
#-----------------------------------------------------------------------------
GET_FILENAME_COMPONENT (SELF_DIR "${CMAKE_CURRENT_LIST_FILE}" PATH)
GET_FILENAME_COMPONENT(_IMPORT_PREFIX "${SELF_DIR}" PATH)
GET_FILENAME_COMPONENT(_IMPORT_PREFIX "${_IMPORT_PREFIX}" PATH)
if (NOT WIN32)
  GET_FILENAME_COMPONENT(_IMPORT_PREFIX "${_IMPORT_PREFIX}" PATH)
endif (NOT WIN32)

#-----------------------------------------------------------------------------
# User Options
#-----------------------------------------------------------------------------
set (HDF4_ENABLE_PARALLEL OFF)
set (HDF4_BUILD_FORTRAN   ON)
set (HDF4_BUILD_XDR_LIB   ON)
set (HDF4_BUILD_TOOLS     ON)
set (HDF4_BUILD_UTILS     ON)
set (HDF4_ENABLE_JPEG_LIB_SUPPORT ON)
set (HDF4_ENABLE_Z_LIB_SUPPORT ON)
set (HDF4_ENABLE_SZIP_SUPPORT  ON)
set (HDF4_ENABLE_SZIP_ENCODING ON)
set (HDF4_BUILD_SHARED_LIBS    ON)
set (HDF4_PACKAGE_EXTLIBS ON)

#-----------------------------------------------------------------------------
# Directories
#-----------------------------------------------------------------------------
set (HDF4_INCLUDE_DIR "${_IMPORT_PREFIX}/include")

if (HDF4_BUILD_FORTRAN)
  set (HDF4_INCLUDE_DIR_FORTRAN "${_IMPORT_PREFIX}/include")
endif (HDF4_BUILD_FORTRAN)
  
if (HDF4_BUILD_XDR_LIB)
  set (HDF4_INCLUDE_DIR_XDR "${_IMPORT_PREFIX}/include")
endif (HDF4_BUILD_XDR_LIB)

if (HDF4_BUILD_TOOLS)
  set (HDF4_INCLUDE_DIR_TOOLS "${_IMPORT_PREFIX}/include")
endif (HDF4_BUILD_TOOLS)

if (HDF4_BUILD_UTILS)
  set (HDF4_INCLUDE_DIR_UTILS "${_IMPORT_PREFIX}/include")
  set (HDF4_TOOLS_DIR "${_IMPORT_PREFIX}/bin" )
endif (HDF4_BUILD_UTILS)

#-----------------------------------------------------------------------------
# Version Strings
#-----------------------------------------------------------------------------
set (HDF4_VERSION_STRING 4.2.11)
set (HDF4_VERSION_MAJOR  4.2)
set (HDF4_VERSION_MINOR  11)

#-----------------------------------------------------------------------------
# Don't include targets if this file is being picked up by another
# project which has already built hdf4 as a subproject
#-----------------------------------------------------------------------------
if (NOT TARGET "hdf4")
  if (HDF4_ENABLE_JPEG_LIB_SUPPORT AND HDF4_PACKAGE_EXTLIBS AND NOT TARGET "jpeg")
    include (${SELF_DIR}/../JPEG/jpeg-targets.cmake)
  endif (HDF4_ENABLE_JPEG_LIB_SUPPORT AND HDF4_PACKAGE_EXTLIBS AND NOT TARGET "jpeg")
  if (HDF4_ENABLE_Z_LIB_SUPPORT AND HDF4_PACKAGE_EXTLIBS AND NOT TARGET "zlib")
    include (${SELF_DIR}/../ZLIB/zlib-targets.cmake)
  endif (HDF4_ENABLE_Z_LIB_SUPPORT AND HDF4_PACKAGE_EXTLIBS AND NOT TARGET "zlib")
  if (HDF4_ENABLE_SZIP_SUPPORT AND HDF4_PACKAGE_EXTLIBS AND NOT TARGET "szip")
    include (${SELF_DIR}/../SZIP/szip-targets.cmake)
  endif (HDF4_ENABLE_SZIP_SUPPORT AND HDF4_PACKAGE_EXTLIBS AND NOT TARGET "szip")
  include (${SELF_DIR}/hdf4-targets.cmake)
  set (HDF4_LIBRARIES "xdr;hdf;mfhdf;mfhdf_fcstub;mfhdf_fortran;hdf_fcstub;hdf_fortran")
endif (NOT TARGET "hdf4")
