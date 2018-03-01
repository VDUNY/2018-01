#----------------------------------------------------------------
# Generated CMake target import file for configuration "Release".
#----------------------------------------------------------------

# Commands may need to know the format version.
set(CMAKE_IMPORT_FILE_VERSION 1)

# Import target "xdr" for configuration "Release"
set_property(TARGET xdr APPEND PROPERTY IMPORTED_CONFIGURATIONS RELEASE)
set_target_properties(xdr PROPERTIES
  IMPORTED_IMPLIB_RELEASE "${_IMPORT_PREFIX}/lib/xdr.lib"
  IMPORTED_LINK_INTERFACE_LIBRARIES_RELEASE "ws2_32.lib"
  IMPORTED_LOCATION_RELEASE "${_IMPORT_PREFIX}/bin/xdr.dll"
  )

list(APPEND _IMPORT_CHECK_TARGETS xdr )
list(APPEND _IMPORT_CHECK_FILES_FOR_xdr "${_IMPORT_PREFIX}/lib/xdr.lib" "${_IMPORT_PREFIX}/bin/xdr.dll" )

# Import target "hdf" for configuration "Release"
set_property(TARGET hdf APPEND PROPERTY IMPORTED_CONFIGURATIONS RELEASE)
set_target_properties(hdf PROPERTIES
  IMPORTED_IMPLIB_RELEASE "${_IMPORT_PREFIX}/lib/hdf.lib"
  IMPORTED_LINK_INTERFACE_LIBRARIES_RELEASE "jpeg;zlib;szip"
  IMPORTED_LOCATION_RELEASE "${_IMPORT_PREFIX}/bin/hdf.dll"
  )

list(APPEND _IMPORT_CHECK_TARGETS hdf )
list(APPEND _IMPORT_CHECK_FILES_FOR_hdf "${_IMPORT_PREFIX}/lib/hdf.lib" "${_IMPORT_PREFIX}/bin/hdf.dll" )

# Import target "mfhdf" for configuration "Release"
set_property(TARGET mfhdf APPEND PROPERTY IMPORTED_CONFIGURATIONS RELEASE)
set_target_properties(mfhdf PROPERTIES
  IMPORTED_IMPLIB_RELEASE "${_IMPORT_PREFIX}/lib/mfhdf.lib"
  IMPORTED_LINK_INTERFACE_LIBRARIES_RELEASE "xdr;hdf"
  IMPORTED_LOCATION_RELEASE "${_IMPORT_PREFIX}/bin/mfhdf.dll"
  )

list(APPEND _IMPORT_CHECK_TARGETS mfhdf )
list(APPEND _IMPORT_CHECK_FILES_FOR_mfhdf "${_IMPORT_PREFIX}/lib/mfhdf.lib" "${_IMPORT_PREFIX}/bin/mfhdf.dll" )

# Import target "mfhdf_fcstub" for configuration "Release"
set_property(TARGET mfhdf_fcstub APPEND PROPERTY IMPORTED_CONFIGURATIONS RELEASE)
set_target_properties(mfhdf_fcstub PROPERTIES
  IMPORTED_IMPLIB_RELEASE "${_IMPORT_PREFIX}/lib/mfhdf_fcstub.lib"
  IMPORTED_LINK_INTERFACE_LIBRARIES_RELEASE "xdr;mfhdf;hdf"
  IMPORTED_LOCATION_RELEASE "${_IMPORT_PREFIX}/bin/mfhdf_fcstub.dll"
  )

list(APPEND _IMPORT_CHECK_TARGETS mfhdf_fcstub )
list(APPEND _IMPORT_CHECK_FILES_FOR_mfhdf_fcstub "${_IMPORT_PREFIX}/lib/mfhdf_fcstub.lib" "${_IMPORT_PREFIX}/bin/mfhdf_fcstub.dll" )

# Import target "mfhdf_fortran" for configuration "Release"
set_property(TARGET mfhdf_fortran APPEND PROPERTY IMPORTED_CONFIGURATIONS RELEASE)
set_target_properties(mfhdf_fortran PROPERTIES
  IMPORTED_IMPLIB_RELEASE "${_IMPORT_PREFIX}/lib/mfhdf_fortran.lib"
  IMPORTED_LINK_INTERFACE_LIBRARIES_RELEASE "mfhdf_fcstub;jpeg;zlib;szip"
  IMPORTED_LOCATION_RELEASE "${_IMPORT_PREFIX}/bin/mfhdf_fortran.dll"
  )

list(APPEND _IMPORT_CHECK_TARGETS mfhdf_fortran )
list(APPEND _IMPORT_CHECK_FILES_FOR_mfhdf_fortran "${_IMPORT_PREFIX}/lib/mfhdf_fortran.lib" "${_IMPORT_PREFIX}/bin/mfhdf_fortran.dll" )

# Import target "hdf_fcstub" for configuration "Release"
set_property(TARGET hdf_fcstub APPEND PROPERTY IMPORTED_CONFIGURATIONS RELEASE)
set_target_properties(hdf_fcstub PROPERTIES
  IMPORTED_IMPLIB_RELEASE "${_IMPORT_PREFIX}/lib/hdf_fcstub.lib"
  IMPORTED_LINK_INTERFACE_LIBRARIES_RELEASE "hdf"
  IMPORTED_LOCATION_RELEASE "${_IMPORT_PREFIX}/bin/hdf_fcstub.dll"
  )

list(APPEND _IMPORT_CHECK_TARGETS hdf_fcstub )
list(APPEND _IMPORT_CHECK_FILES_FOR_hdf_fcstub "${_IMPORT_PREFIX}/lib/hdf_fcstub.lib" "${_IMPORT_PREFIX}/bin/hdf_fcstub.dll" )

# Import target "hdf_fortran" for configuration "Release"
set_property(TARGET hdf_fortran APPEND PROPERTY IMPORTED_CONFIGURATIONS RELEASE)
set_target_properties(hdf_fortran PROPERTIES
  IMPORTED_IMPLIB_RELEASE "${_IMPORT_PREFIX}/lib/hdf_fortran.lib"
  IMPORTED_LINK_INTERFACE_LIBRARIES_RELEASE "hdf_fcstub;jpeg;zlib;szip"
  IMPORTED_LOCATION_RELEASE "${_IMPORT_PREFIX}/bin/hdf_fortran.dll"
  )

list(APPEND _IMPORT_CHECK_TARGETS hdf_fortran )
list(APPEND _IMPORT_CHECK_FILES_FOR_hdf_fortran "${_IMPORT_PREFIX}/lib/hdf_fortran.lib" "${_IMPORT_PREFIX}/bin/hdf_fortran.dll" )

# Commands beyond this point should not need to know the version.
set(CMAKE_IMPORT_FILE_VERSION)
