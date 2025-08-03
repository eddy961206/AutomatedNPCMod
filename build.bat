@echo off
REM 강제 종료(실패해도 다음 단계 강행)
taskkill /f /im "StardewModdingAPI.exe" 2>nul
taskkill /f /im "Stardew Valley.exe" 2>nul

REM 빌드
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo 빌드 실패! 오류를 확인하세요.
    pause
    exit /b
)

REM 복사
copy /Y "bin\Debug\net6.0\AutomatedNPCMod.dll" "AutomatedNPCMod.dll"
if %ERRORLEVEL% NEQ 0 (
    echo DLL 복사 실패! 파일이 사용 중일 수 있습니다.
    pause
    exit /b
)

REM 게임 재실행 - 스팀 런처가 알아서 자동 실행이므로 따로 start 불필요
echo 모든 작업 완료. Steam에서 게임을 직접 실행하세요!
pause
