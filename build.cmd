@echo off
.paket\paket.bootstrapper.exe
IF %ERRORLEVEL% NEQ 0 (
  exit /b %ERRORLEVEL%
)

.paket\paket restore
IF %ERRORLEVEL% NEQ 0 (
  exit /b %ERRORLEVEL%
)

"packages\FAKE\tools\Fake.exe" build.fsx "%1"
IF %ERRORLEVEL% NEQ 0 (
  exit /b %ERRORLEVEL%
)