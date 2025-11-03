@echo off
setlocal EnableExtensions EnableDelayedExpansion

set "REPO_URL=https://github.com/EM20080/HedgeConfig.git"
set "REMOTE_NAME=origin"
set "DEFAULT_BRANCH=main"

if "%~1"=="" ( set "MSG=update" ) else ( set "MSG=%*" )

git --version >nul 2>&1 || (echo Git not found in PATH & exit /b 1)

git rev-parse --is-inside-work-tree >nul 2>&1
if errorlevel 1 (
  echo Initializing new git repository...
  git init || exit /b 1
  git checkout -b "%DEFAULT_BRANCH%" || exit /b 1
  git remote add "%REMOTE_NAME%" "%REPO_URL%" 2>nul
)

for /f "delims=" %%b in ('git rev-parse --abbrev-ref HEAD') do set "BRANCH=%%b"
if /i "!BRANCH!"=="HEAD" (
  echo Detached HEAD; creating "%DEFAULT_BRANCH%" branch...
  git checkout -b "%DEFAULT_BRANCH%" || exit /b 1
  set "BRANCH=%DEFAULT_BRANCH%"
)

git remote get-url "%REMOTE_NAME%" >nul 2>&1
if errorlevel 1 (
  echo Adding remote %REMOTE_NAME% -> %REPO_URL%
  git remote add "%REMOTE_NAME%" "%REPO_URL%" || exit /b 1
)

git add -A
git diff --cached --quiet
if errorlevel 1 (
  git commit -m "%MSG%" || exit /b 1
) else (
  echo No changes to commit.
)

git rev-parse --abbrev-ref --symbolic-full-name @{u} >nul 2>&1
if errorlevel 1 (
  echo Pushing and setting upstream: %REMOTE_NAME% %BRANCH%
  git push -u "%REMOTE_NAME%" "%BRANCH%" || exit /b 1
) else (
  echo Pushing to tracked upstream...
  git push || exit /b 1
)
