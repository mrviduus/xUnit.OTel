@echo off
setlocal enabledelayedexpansion

REM Aspire Dashboard Local Runner for Windows
REM This script runs the .NET Aspire Dashboard for OpenTelemetry observability
REM Supports both Docker and Podman with automatic detection

REM Script configuration
set "SCRIPT_NAME=%~nx0"
set "SCRIPT_DIR=%~dp0"
set "PROJECT_ROOT=%SCRIPT_DIR%.."

REM Container configuration
set "CONTAINER_NAME=aspire-dashboard"
set "IMAGE_NAME=mcr.microsoft.com/dotnet/nightly/aspire-dashboard:8.0.0"
set "DASHBOARD_PORT=18888"
set "OTLP_PORT=4317"
set "CONTAINER_OTLP_PORT=18889"

REM Default values
set "COMMAND=start"
set "ENGINE="
set "DETACHED=true"
set "CUSTOM_DASHBOARD_PORT=%DASHBOARD_PORT%"
set "CUSTOM_OTLP_PORT=%OTLP_PORT%"

REM Parse command line arguments
:parse_args
if "%~1"=="" goto :args_done
if /i "%~1"=="--help" goto :show_help
if /i "%~1"=="-h" goto :show_help
if /i "%~1"=="--engine" (
    set "ENGINE=%~2"
    shift
    shift
    goto :parse_args
)
if /i "%~1"=="-e" (
    set "ENGINE=%~2"
    shift
    shift
    goto :parse_args
)
if /i "%~1"=="--port" (
    set "CUSTOM_DASHBOARD_PORT=%~2"
    shift
    shift
    goto :parse_args
)
if /i "%~1"=="-p" (
    set "CUSTOM_DASHBOARD_PORT=%~2"
    shift
    shift
    goto :parse_args
)
if /i "%~1"=="--otlp-port" (
    set "CUSTOM_OTLP_PORT=%~2"
    shift
    shift
    goto :parse_args
)
if /i "%~1"=="-o" (
    set "CUSTOM_OTLP_PORT=%~2"
    shift
    shift
    goto :parse_args
)
if /i "%~1"=="--foreground" (
    set "DETACHED=false"
    shift
    goto :parse_args
)
if /i "%~1"=="-f" (
    set "DETACHED=false"
    shift
    goto :parse_args
)
if /i "%~1"=="start" (
    set "COMMAND=start"
    shift
    goto :parse_args
)
if /i "%~1"=="stop" (
    set "COMMAND=stop"
    shift
    goto :parse_args
)
if /i "%~1"=="restart" (
    set "COMMAND=restart"
    shift
    goto :parse_args
)
if /i "%~1"=="logs" (
    set "COMMAND=logs"
    shift
    goto :parse_args
)
if /i "%~1"=="status" (
    set "COMMAND=status"
    shift
    goto :parse_args
)
if /i "%~1"=="cleanup" (
    set "COMMAND=cleanup"
    shift
    goto :parse_args
)
echo [ERROR] Unknown option: %~1
echo Use --help for usage information
exit /b 1

:args_done

REM Override ports from environment variables if set
if defined ASPIRE_DASHBOARD_PORT set "CUSTOM_DASHBOARD_PORT=%ASPIRE_DASHBOARD_PORT%"
if defined ASPIRE_OTLP_PORT set "CUSTOM_OTLP_PORT=%ASPIRE_OTLP_PORT%"

REM Detect container engine if not specified
if "%ENGINE%"=="" call :detect_engine

echo [INFO] Using container engine: %ENGINE%

REM Execute command
if /i "%COMMAND%"=="start" call :start_container
if /i "%COMMAND%"=="stop" call :stop_container
if /i "%COMMAND%"=="restart" call :restart_container
if /i "%COMMAND%"=="logs" call :show_logs
if /i "%COMMAND%"=="status" call :show_status
if /i "%COMMAND%"=="cleanup" call :cleanup

goto :eof

:show_help
echo %SCRIPT_NAME% - Run .NET Aspire Dashboard for OpenTelemetry
echo.
echo USAGE:
echo     %SCRIPT_NAME% [OPTIONS] [COMMAND]
echo.
echo COMMANDS:
echo     start       Start the Aspire Dashboard (default)
echo     stop        Stop and remove the container
echo     restart     Restart the container
echo     logs        Show container logs
echo     status      Show container status
echo     cleanup     Remove container and image
echo.
echo OPTIONS:
echo     -e, --engine ENGINE     Container engine to use (docker^|podman)
echo     -p, --port PORT         Dashboard port (default: %DASHBOARD_PORT%)
echo     -o, --otlp-port PORT    OTLP port (default: %OTLP_PORT%)
echo     -f, --foreground        Run in foreground mode
echo     -h, --help              Show this help message
echo.
echo EXAMPLES:
echo     %SCRIPT_NAME%                          # Start with auto-detected engine
echo     %SCRIPT_NAME% --engine docker          # Force Docker usage
echo     %SCRIPT_NAME% --port 19999             # Use custom dashboard port
echo     %SCRIPT_NAME% stop                     # Stop the container
echo     %SCRIPT_NAME% logs                     # View logs
echo.
echo ENVIRONMENT VARIABLES:
echo     CONTAINER_ENGINE                        # Preferred container engine
echo     ASPIRE_DASHBOARD_PORT                   # Dashboard port override
echo     ASPIRE_OTLP_PORT                        # OTLP port override
echo.
echo PORTS:
echo     %DASHBOARD_PORT%     - Aspire Dashboard web interface
echo     %OTLP_PORT%      - OpenTelemetry Protocol (OTLP) endpoint
echo.
echo ACCESS:
echo     Dashboard: http://localhost:%DASHBOARD_PORT%
echo     OTLP:      http://localhost:%OTLP_PORT%
echo.
goto :eof

:detect_engine
set "ENGINE="
if defined CONTAINER_ENGINE (
    where "%CONTAINER_ENGINE%" >nul 2>&1
    if !errorlevel! equ 0 (
        set "ENGINE=%CONTAINER_ENGINE%"
        goto :detect_done
    ) else (
        echo [WARNING] Specified engine '%CONTAINER_ENGINE%' not found, falling back to auto-detection
    )
)

where podman >nul 2>&1
if !errorlevel! equ 0 (
    set "ENGINE=podman"
    goto :detect_done
)

where docker >nul 2>&1
if !errorlevel! equ 0 (
    set "ENGINE=docker"
    goto :detect_done
)

echo [ERROR] Neither Docker nor Podman found. Please install one of them.
exit /b 1

:detect_done
goto :eof

:container_exists
%ENGINE% ps -a --format "table {{.Names}}" | findstr /r "^%CONTAINER_NAME%$" >nul 2>&1
goto :eof

:container_running
%ENGINE% ps --format "table {{.Names}}" | findstr /r "^%CONTAINER_NAME%$" >nul 2>&1
goto :eof

:pull_image
echo [INFO] Pulling latest image: %IMAGE_NAME%
%ENGINE% pull "%IMAGE_NAME%"
if !errorlevel! neq 0 (
    echo [ERROR] Failed to pull image
    exit /b 1
)
echo [SUCCESS] Image pulled successfully
goto :eof

:start_container
REM Check if container already exists
call :container_exists
if !errorlevel! equ 0 (
    call :container_running
    if !errorlevel! equ 0 (
        echo [WARNING] Container '%CONTAINER_NAME%' is already running
        call :show_access_info
        goto :eof
    ) else (
        echo [INFO] Starting existing container '%CONTAINER_NAME%'
        %ENGINE% start "%CONTAINER_NAME%"
        echo [SUCCESS] Container started
        call :show_access_info
        goto :eof
    )
)

REM Pull latest image
call :pull_image

REM Start new container
echo [INFO] Starting Aspire Dashboard with %ENGINE%

if /i "%DETACHED%"=="true" (
    set "RUN_MODE=-d"
) else (
    set "RUN_MODE=-it"
)

set "RUN_CMD=%ENGINE% run %RUN_MODE% --name %CONTAINER_NAME% -p %CUSTOM_DASHBOARD_PORT%:18888 -p %CUSTOM_OTLP_PORT%:%CONTAINER_OTLP_PORT% -e DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true -e ASPNETCORE_ENVIRONMENT=Development --rm %IMAGE_NAME%"

echo [INFO] Command: !RUN_CMD!
!RUN_CMD!

if !errorlevel! neq 0 (
    echo [ERROR] Failed to start container
    exit /b 1
)

if /i "%DETACHED%"=="true" (
    timeout /t 2 /nobreak >nul
    call :container_running
    if !errorlevel! equ 0 (
        echo [SUCCESS] Aspire Dashboard started successfully
        call :show_access_info
    ) else (
        echo [ERROR] Container failed to start properly
        %ENGINE% logs "%CONTAINER_NAME%"
        exit /b 1
    )
)
goto :eof

:stop_container
call :container_exists
if !errorlevel! neq 0 (
    echo [WARNING] Container '%CONTAINER_NAME%' does not exist
    goto :eof
)

call :container_running
if !errorlevel! equ 0 (
    echo [INFO] Stopping container '%CONTAINER_NAME%'
    %ENGINE% stop "%CONTAINER_NAME%"
    echo [SUCCESS] Container stopped
) else (
    echo [WARNING] Container '%CONTAINER_NAME%' is not running
)
goto :eof

:restart_container
call :stop_container
timeout /t 1 /nobreak >nul
call :start_container
goto :eof

:show_logs
call :container_exists
if !errorlevel! neq 0 (
    echo [ERROR] Container '%CONTAINER_NAME%' does not exist
    exit /b 1
)

echo [INFO] Showing logs for '%CONTAINER_NAME%'
%ENGINE% logs --follow "%CONTAINER_NAME%"
goto :eof

:show_status
echo [INFO] Container status for '%CONTAINER_NAME%':

call :container_exists
if !errorlevel! equ 0 (
    %ENGINE% ps -a --filter "name=%CONTAINER_NAME%" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    
    call :container_running
    if !errorlevel! equ 0 (
        echo.
        echo [INFO] Container is running and accessible at:
        call :show_access_info
    )
) else (
    echo Container does not exist
)
goto :eof

:cleanup
echo [INFO] Cleaning up Aspire Dashboard resources

call :container_exists
if !errorlevel! equ 0 (
    call :container_running
    if !errorlevel! equ 0 (
        call :stop_container
    )
    
    echo [INFO] Removing container '%CONTAINER_NAME%'
    %ENGINE% rm "%CONTAINER_NAME%" >nul 2>&1
)

echo [INFO] Removing image '%IMAGE_NAME%'
%ENGINE% rmi "%IMAGE_NAME%" >nul 2>&1

echo [SUCCESS] Cleanup completed
goto :eof

:show_access_info
echo.
echo Aspire Dashboard is ready!
echo.
echo Dashboard: http://localhost:%CUSTOM_DASHBOARD_PORT%
echo OTLP Endpoint: http://localhost:%CUSTOM_OTLP_PORT%
echo.
echo Environment Variables for your tests:
echo set OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:%CUSTOM_OTLP_PORT%
echo set OTEL_EXPORTER_OTLP_ENABLED=true
echo.
echo To stop: %SCRIPT_NAME% stop
echo To view logs: %SCRIPT_NAME% logs
echo.
goto :eof
