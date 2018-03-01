#-----------------------------------------------------------------------------
# JPEG Config file for compiling against JPEG install directory
#-----------------------------------------------------------------------------

GET_FILENAME_COMPONENT (SELF_DIR "${CMAKE_CURRENT_LIST_FILE}" PATH)
GET_FILENAME_COMPONENT(_IMPORT_PREFIX "${SELF_DIR}" PATH)
GET_FILENAME_COMPONENT(_IMPORT_PREFIX "${_IMPORT_PREFIX}" PATH)
if (NOT WIN32)
  GET_FILENAME_COMPONENT(_IMPORT_PREFIX "${_IMPORT_PREFIX}" PATH)
endif (NOT WIN32)

GET_FILENAME_COMPONENT (JPEG_INCLUDE_DIRS "${_IMPORT_PREFIX}/include")

#-----------------------------------------------------------------------------
# Version Strings
#-----------------------------------------------------------------------------
set (JPEG_VERSION_STRING 8.0)
set (JPEG_VERSION_MAJOR  8.0)
set (JPEG_VERSION_MINOR  2)

#-----------------------------------------------------------------------------
# Don't include targets if this file is being picked up by another
# project which has already build JPEG as a subproject
#-----------------------------------------------------------------------------
if (NOT TARGET "jpeg")
  include (${SELF_DIR}/jpeg-targets.cmake)
  set (JPEG_LIBRARIES "jpeg")
  set (JPEG_LIBRARY "jpeg")
endif (NOT TARGET "jpeg")
