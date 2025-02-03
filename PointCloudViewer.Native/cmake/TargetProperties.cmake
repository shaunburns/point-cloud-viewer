function(target_set_common_properties Target)
	set_target_properties(${Target}
		PROPERTIES
		CXX_STANDARD 20
		CXX_STANDARD_REQUIRED ON
		CXX_EXTENSIONS OFF

		COMPILE_WARNING_AS_ERROR ON
	)

	if (MSVC)
		target_compile_options(${Target} PRIVATE /W4)
    elseif(APPLE)
        target_compile_options(${Target} PRIVATE -Wall -Wextra)
	else()
		target_compile_options(${Target} PRIVATE -Wall -Wextra -pedantic)
	endif()
endfunction(target_set_common_properties)
