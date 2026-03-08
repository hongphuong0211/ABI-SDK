if(NOT TARGET games-frame-pacing::swappy)
add_library(games-frame-pacing::swappy SHARED IMPORTED)
set_target_properties(games-frame-pacing::swappy PROPERTIES
    IMPORTED_LOCATION "C:/Users/ADMIN/.gradle/caches/8.13/transforms/9ea8195bb03c8dc133ac46a98b29868f/transformed/jetified-games-frame-pacing-2.1.2/prefab/modules/swappy/libs/android.armeabi-v7a/libswappy.so"
    INTERFACE_INCLUDE_DIRECTORIES "C:/Users/ADMIN/.gradle/caches/8.13/transforms/9ea8195bb03c8dc133ac46a98b29868f/transformed/jetified-games-frame-pacing-2.1.2/prefab/modules/swappy/include"
    INTERFACE_LINK_LIBRARIES ""
)
endif()

if(NOT TARGET games-frame-pacing::swappy_static)
add_library(games-frame-pacing::swappy_static STATIC IMPORTED)
set_target_properties(games-frame-pacing::swappy_static PROPERTIES
    IMPORTED_LOCATION "C:/Users/ADMIN/.gradle/caches/8.13/transforms/9ea8195bb03c8dc133ac46a98b29868f/transformed/jetified-games-frame-pacing-2.1.2/prefab/modules/swappy_static/libs/android.armeabi-v7a/libswappy_static.a"
    INTERFACE_INCLUDE_DIRECTORIES "C:/Users/ADMIN/.gradle/caches/8.13/transforms/9ea8195bb03c8dc133ac46a98b29868f/transformed/jetified-games-frame-pacing-2.1.2/prefab/modules/swappy_static/include"
    INTERFACE_LINK_LIBRARIES ""
)
endif()

