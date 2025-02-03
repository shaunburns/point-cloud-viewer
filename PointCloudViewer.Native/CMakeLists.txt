cmake_minimum_required(VERSION 3.25.1)
project(PointCloudViewer)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED True)
set(CMAKE_COMPILE_WARNING_AS_ERROR ON)

include(cmake/TargetProperties.cmake)

include(${CMAKE_TOOLCHAIN_FILE})

add_subdirectory(src)
