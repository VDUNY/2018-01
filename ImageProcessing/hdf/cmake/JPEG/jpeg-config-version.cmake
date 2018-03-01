#-----------------------------------------------------------------------------
# JPEG Version file for install directory
#-----------------------------------------------------------------------------

set (PACKAGE_VERSION 8.0)

if ("${PACKAGE_FIND_VERSION_MAJOR}" EQUAL 8.0)

  # exact match for version 8.0.2
  if ("${PACKAGE_FIND_VERSION_MINOR}" EQUAL 2)

    # compatible with any version 8.0.2.x
    set (PACKAGE_VERSION_COMPATIBLE 1) 
    
    if ("${PACKAGE_FIND_VERSION_PATCH}" EQUAL )
      set (PACKAGE_VERSION_EXACT 1)    

      if ("${PACKAGE_FIND_VERSION_TWEAK}" EQUAL )
        # not using this yet
      endif ("${PACKAGE_FIND_VERSION_TWEAK}" EQUAL )
      
    endif ("${PACKAGE_FIND_VERSION_PATCH}" EQUAL )
    
  endif ("${PACKAGE_FIND_VERSION_MINOR}" EQUAL 2)
endif ("${PACKAGE_FIND_VERSION_MAJOR}" EQUAL 8.0)


