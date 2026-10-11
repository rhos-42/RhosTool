cmake_minimum_required(VERSION 3.21)
project(RhosToolCpp VERSION 1.0.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_AUTOMOC ON)

find_package(Qt6 REQUIRED COMPONENTS Widgets)

qt_add_executable(RhosToolCpp
  WIN32
  src/main.cpp
  src/AdbClient.h
  src/AdbClient.cpp
  src/CatalogStore.h
  src/CatalogStore.cpp
  src/MainWindow.h
  src/MainWindow.cpp
  src/FlowLayout.h
)

target_link_libraries(RhosToolCpp PRIVATE Qt6::Widgets)
set_target_properties(RhosToolCpp PROPERTIES OUTPUT_NAME RhosTool)

# Copie les catalogues JSON à côté de l'exe
add_custom_command(TARGET RhosToolCpp POST_BUILD
  COMMAND ${CMAKE_COMMAND} -E copy_directory
          "${CMAKE_CURRENT_SOURCE_DIR}/data"
          "$<TARGET_FILE_DIR:RhosToolCpp>/data"
)
