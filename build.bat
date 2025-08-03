@echo off 
echo Building mod... 
dotnet build 
if %0% EQU 0 ( 
  copy "bin\Debug\net6.0\AutomatedNPCMod.dll" "AutomatedNPCMod.dll" 
  echo Mod built successfully! Restart game to see changes. 
) else ( 
  echo Build failed! Check errors above. 
) 
pause 
