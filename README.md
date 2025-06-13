# unreal_engine_conan
Shows how to use conan2 together with unreal engine 5

# Windows Setup
## Prequesits
- Ninja
- Cmake
- Conan 2.x.y
- visual studio code
- visual studio community 2022
- unreal engine


## Setup
### Create a unreal engine c++ project
- start unreal editor
- select the games tab
- select some game type (tested with 'Blank' and 'First Person')
- select C++
- type the project name
- select project location
- press create

### setup the unreal engine c++ project
- unreal editor should open itself
- in the unreal editor menu bar select 'Edit' -> 'Editor Preferences'
- under 'General' click on 'Source Code'
- select set 'Visual Studio Code' as 'Source Code Editor'
- close 'Editor Preferences'
- in the menu bar select 'Tools' 'Refresh Visual Studio Code Project' (it is under the 'PROGRAMMING' group)
- go to 'Platforms' (it is in the Top Center of the screen next to the play button)
- select 'Windows' -> 'Package Project'
- select 'select Folder' in the Package project dialog (this can take 20 minutes)

### setup visual studio code
- if unreal editor opened visual studio code close it
- in windows open 'x64 Native Tools Command Prompt for VS 2022'
- in there type 'code' to open visual studio code with the correct environment
- in visual studio code open your project
- open the 'MyProject.code-workspace' (where MyProject is the name of your project)
- search for '"program": "C:\\workspace\\MyProject\\Binaries\\Win64\\UnrealGame.exe",'
- replace UnrealGame.exe with the exe of your game in my case it is MyProject.exe
- press ctrl + shift + b to open the 'select the build task to run' combobox
- select 'MyProject Win64 Development Rebuild' (where MyProject is the name of your project)
- go to 'Run and Debug' (it is the play button with the bug icon on the left)
- open the run and debug combobox and select 'Launch my Project Development'
- press the play button
- your game should start now in debug mode

### add conan support
- in power shell run conan profile detect
- go to the profile created from conan
- paste this at the end
```
[conf]
tools.build:cxxflags=["-DWINVER=0x0A00", "-D_WIN32_WINNT=0x0A00", "/EHsc"]
```
- copy the conanfile.py file from this project into your project root
in the project root run:
```
conan install . --output-folder conanbuild --build missing --deployer full_deploy -s tools.cmake.cmaketoolchain:generator=Ninja
```
- now copy the MyProject.Build.cs file from this project into the 'Source/MyProject' folder of your project and change the name so it matches the name of your project
- press ctrl + shift + b to open the 'select the build task to run' combobox
- select 'MyProject Win64 Development Rebuild' (where MyProject is the name of your project)
- go to 'Run and Debug' and press the play button to start the game


### problems compiling and linking unreal engine source with your dependencies
#### unreal engine defines macros which make it hard to compile your dependencies with it
unreal engine defines lots of macros which can interfere with your dependencies so use 'UndefineMacros_UE_4.17.h' and 'RedefineMacros_UE_4.17.h'
```
#include "UndefineMacros_UE_4.17.h"
#include "Your/Dependency/yourDep.h"
#include "RedefineMacros_UE_4.17.h"
```
#### unreal engine defines 'UI' as namespace and this clashes with the 'UI'-Symbol from openssl/ui.h also there are some warnings coming from openssl
```
#pragma warning(push, 0)
#pragma push_macro("UI")
#undef UI
#define UI OpenSSL_UI
#include <openssl/ui.h>
#pragma pop_macro("UI")
#pragma warning(pop)
```



