project(bootstrapperdll)

add_definitions(-DNATIVEAOT_DLL)

set(SOURCES
    ../main.cpp
)

add_library(bootstrapperdll STATIC ${SOURCES})

install_bootstrapper_object(bootstrapperdll aotsdk)

if (CLR_CMAKE_TARGET_WIN32)
  add_library(bootstrapperdll.GuardCF STATIC ${SOURCES})
  install_bootstrapper_object(bootstrapperdll.GuardCF aotsdk)
  target_compile_options(bootstrapperdll.GuardCF PRIVATE $<$<OR:$<COMPILE_LANGUAGE:C>,$<COMPILE_LANGUAGE:CXX>>:/guard:cf>)
endif()
