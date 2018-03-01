#-----------------------------------------------------------------------------
# SZIP Config file for compiling against SZIP install directory
#-----------------------------------------------------------------------------

GET_FILENAME_COMPONENT (SELF_DIR "${CMAKE_CURRENT_LIST_FILE}" PATH)
GET_FILENAME_COMPONENT(_IMPORT_PREFIX "${SELF_DIR}" PATH)
GET_FILENAME_COMPONENT(_IMPORT_PREFIX "${_IMPORT_PREFIX}" PATH)
if (NOT WIN32)
  GET_FILENAME_COMPONENT(_IMPORT_PREFIX "${_IMPORT_PREFIX}" PATH)
endif (NOT WIN32)

GET_FILENAME_COMPONENT (SZIP_INCLUDE_DIRS "${_IMPORT_PREFIX}/include")

#-----------------------------------------------------------------------------
# Version Strings
#-----------------------------------------------------------------------------
set (SZIP_VERSION_STRING 2.1)
set (SZIP_VERSION_MAJOR  2)
set (SZIP_VERSION_MINOR  1)

#-----------------------------------------------------------------------------
# Don't include targets if this file is being picked up by another
# project which has already build SZIP as a subproject
#-----------------------------------------------------------------------------
if (NOT TARGET "szip")
  include (${SELF_DIR}/szip-targets.cmake)
  set (SZIP_LIBRARIES "szip")
endif (NOT TARGET "szip")
