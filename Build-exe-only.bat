@echo off
echo Publishing HedgeConfig as single executable...
echo.

echo Cleaning previous builds...
dotnet clean --configuration Release

echo.
echo Publishing...
dotnet publish HedgeConfig/HedgeConfig.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output "./publish" ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true ^
    -p:PublishReadyToRun=true

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Success! Single executable created at:
    echo   .\publish\HedgeConfig.exe
    echo.
    dir /s ".\publish\HedgeConfig.exe"
) else (
    echo.
    echo Build failed!
)

pause

