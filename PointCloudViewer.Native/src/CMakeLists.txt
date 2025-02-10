add_executable(PointCloudViewer)
target_set_common_properties(PointCloudViewer)

target_sources(PointCloudViewer
    PRIVATE
    main.cpp
)

find_package(SDL2 CONFIG REQUIRED)
find_package(glad REQUIRED)

target_link_libraries(PointCloudViewer PRIVATE
	SDL2::SDL2
    glad::glad
)

if (EMSCRIPTEN)
    set_target_properties(PointCloudViewer PROPERTIES SUFFIX ".html")
    target_link_options(PointCloudViewer PRIVATE
        --emit-tsd "$<TARGET_FILE_DIR:PointCloudViewer>/PointCloudViewer.d.ts"
        -sEXPORTED_RUNTIME_METHODS=["cwrap","ccall","UTF8ToString","getValue"]
        "-sUSE_SDL=2"
        "-sALLOW_MEMORY_GROWTH=1"
        "-sUSE_WEBGL2=1" # Enables WebGL 2 for SDL2, avoiding compatibility issues.
        "-sMODULARIZE=1" # Wraps the output in a function, preventing global namespace pollution.
        "-sEXPORT_ES6=1" # Enables ES6 modules, required for modern JavaScript interoperability.
    )
endif (EMSCRIPTEN)
