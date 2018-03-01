#-----------------------------------------------------------------------------
# ZLIB Config file for compiling against ZLIB install directory
#-----------------------------------------------------------------------------

GET_FILENAME_COMPONENT (SELF_DIR "${CMAKE_CURRENT_LIST_FILE}" PATH)
GET_FILENAME_COMPONENT(_IMPORT_PREFIX "${SELF_DIR}" PATH)
GET_FILENAME_COMPONENT(_IMPORT_PREFIX "${_IMPORT_PREFIX}" PATH)
if (NOT WIN32)
  GET_FILENAME_COMPONENT(_IMPORT_PREFIX "${_IMPORT_PREFIX}" PATH)
endif (NOT WIN32)

GET_FILENAME_COMPONENT (ZLIB_INCLUDE_DIRS "${_IMPORT_PREFIX}/include")

#-----------------------------------------------------------------------------
# Version Strings
#-----------------------------------------------------------------------------
set (ZLIB_VERSION_STRING 1.2)
set (ZLIB_VERSION_MAJOR  1.2)
set (ZLIB_VERSION_MINOR  8)

#-----------------------------------------------------------------------------
# Don't include targets if this file is being picked up by another
# project which has already build ZLIB as a subproject
#-----------------------------------------------------------------------------
if (NOT TARGET "zlib")
  include (${SELF_DIR}/zlib-targets.cmake)
  set (ZLIB_LIBRARIES "zlib")
endif (NOT TARGET "zlib")
