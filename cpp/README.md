# RhosTool C++ (Qt 6)

Remplacement progressif de l'app WPF. Cette version couvre l'atelier local : marques, scan ADB, lecture d'infos, reboot, recovery, gestionnaire de périphériques, dossiers MediaFire.

## Prérequis

- CMake 3.21+
- Compilateur C++17 (MinGW UCRT ou MSVC)
- Qt 6 Widgets

Avec MSYS2 :

```bash
pacman -S mingw-w64-ucrt-x86_64-toolchain mingw-w64-ucrt-x86_64-cmake mingw-w64-ucrt-x86_64-ninja mingw-w64-ucrt-x86_64-qt6-base
```

## Build

```powershell
cmake -S cpp -B cpp/build -G Ninja -DCMAKE_PREFIX_PATH="C:/msys64/ucrt64"
cmake --build cpp/build
.\cpp\build\RhosTool.exe
```

ADB est cherché dans `%AppData%\RhosTool\platform-tools`, puis `tools\platform-tools`.
