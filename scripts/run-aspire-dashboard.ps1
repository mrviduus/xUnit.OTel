#Requires -Version 5.1

<#
.SYNOPSIS
    Aspire Dashboard Local Runner for Windows PowerShell
    
.DESCRIPTION
    This script runs the .NET Aspire Dashboard for OpenTelemetry observability.
    Supports both Docker and Podman with automatic detection.
    
.PARAMETER Command
    The command to execute (start, stop, restart, logs, status, cleanup)
    
.PARAMETER Engine
    Container engine to use (docker or podman)
    
.PARAMETER Port
    Dashboard port (default: 18888)
    
.PARAMETER OtlpPort
    OTLP port (default: 4317)
    
.PARAMETER Foreground
    Run in foreground mode instead of detached
    
.PARAMETER Help
    Show help information
    
.EXAMPLE
    .\run-aspire-dashboard.ps1
    Start the Aspire Dashboard with auto-detected engine
    
.EXAMPLE
    .\run-aspire-dashboard.ps1 -Engine docker -Port 19999
    Start with Docker on custom port
    
.EXAMPLE
    .\run-aspire-dashboard.ps1 stop
    Stop the running container
    
.NOTES
    Author: xUnit.OTel Project
    Requires: Docker or Podman
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet('start', 'stop', 'restart', 'logs', 'status', 'cleanup')]
    [string]$Command = 'start',
    
    [Parameter()]
    [ValidateSet('docker', 'podman')]
    [string]$Engine,
    
    [Parameter()]
    [int]$Port = 18888,
    
    [Parameter()]
    [int]$OtlpPort = 4317,
    
    [Parameter()]
    [switch]$Foreground,
    
    [Parameter()]
    [switch]$Help
)

# Script configuration
$Script:ContainerName = 'aspire-dashboard'
$Script:ImageName = 'mcr.microsoft.com/dotnet/nightly/aspire-dashboard:8.0.0'
$Script:ContainerOtlpPort = 18889

# Override ports from environment variables if set
if ($env:ASPIRE_DASHBOARD_PORT) { $Port = [int]$env:ASPIRE_DASHBOARD_PORT }
if ($env:ASPIRE_OTLP_PORT) { $OtlpPort = [int]$env:ASPIRE_OTLP_PORT }

# Color functions
function Write-ColorOutput {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message,
        
        [Parameter()]
        [ValidateSet('Info', 'Success', 'Warning', 'Error')]
        [string]$Type = 'Info'
    )
    
    $colors = @{
        'Info'    = 'Cyan'
        'Success' = 'Green'
        'Warning' = 'Yellow'
        'Error'   = 'Red'
    }
    
    $prefix = "[$Type]".ToUpper()
    Write-Host $prefix -ForegroundColor $colors[$Type] -NoNewline
    Write-Host " $Message"
}

function Show-Help {
    $helpText = @"
run-aspire-dashboard.ps1 - Run .NET Aspire Dashboard for OpenTelemetry

USAGE:
    .\run-aspire-dashboard.ps1 [OPTIONS] [COMMAND]

COMMANDS:
    start       Start the Aspire Dashboard (default)
    stop        Stop and remove the container
    restart     Restart the container
    logs        Show container logs
    status      Show container status
    cleanup     Remove container and image

OPTIONS:
    -Engine ENGINE          Container engine to use (docker|podman)
    -Port PORT              Dashboard port (default: $Port)
    -OtlpPort PORT          OTLP port (default: $OtlpPort)
    -Foreground             Run in foreground mode
    -Help                   Show this help message

EXAMPLES:
    .\run-aspire-dashboard.ps1                              # Start with auto-detected engine
    .\run-aspire-dashboard.ps1 -Engine docker              # Force Docker usage
    .\run-aspire-dashboard.ps1 -Port 19999                 # Use custom dashboard port
    .\run-aspire-dashboard.ps1 stop                        # Stop the container
    .\run-aspire-dashboard.ps1 logs                        # View logs

ENVIRONMENT VARIABLES:
    CONTAINER_ENGINE                        # Preferred container engine
    ASPIRE_DASHBOARD_PORT                   # Dashboard port override
    ASPIRE_OTLP_PORT                        # OTLP port override

PORTS:
    $Port     - Aspire Dashboard web interface
    $OtlpPort      - OpenTelemetry Protocol (OTLP) endpoint

ACCESS:
    Dashboard: http://localhost:$Port
    OTLP:      http://localhost:$OtlpPort

"@
    Write-Host $helpText
}

function Get-ContainerEngine {
    if ($Engine) {
        if (Get-Command $Engine -ErrorAction SilentlyContinue) {
            return $Engine
        }
        else {
            Write-ColorOutput "Specified engine '$Engine' not found, falling back to auto-detection" -Type Warning
        }
    }
    
    if ($env:CONTAINER_ENGINE -and (Get-Command $env:CONTAINER_ENGINE -ErrorAction SilentlyContinue)) {
        return $env:CONTAINER_ENGINE
    }
    
    if (Get-Command podman -ErrorAction SilentlyContinue) {
        return 'podman'
    }
    elseif (Get-Command docker -ErrorAction SilentlyContinue) {
        return 'docker'
    }
    else {
        Write-ColorOutput "Neither Docker nor Podman found. Please install one of them." -Type Error
        exit 1
    }
}

function Test-ContainerExists {
    param([string]$ContainerEngine)
    
    try {
        $result = & $ContainerEngine ps -a --format "table {{.Names}}" 2>$null
        return $result -match "^$Script:ContainerName$"
    }
    catch {
        return $false
    }
}

function Test-ContainerRunning {
    param([string]$ContainerEngine)
    
    try {
        $result = & $ContainerEngine ps --format "table {{.Names}}" 2>$null
        return $result -match "^$Script:ContainerName$"
    }
    catch {
        return $false
    }
}

function Invoke-PullImage {
    param([string]$ContainerEngine)
    
    Write-ColorOutput "Pulling latest image: $Script:ImageName" -Type Info
    
    try {
        & $ContainerEngine pull $Script:ImageName
        if ($LASTEXITCODE -ne 0) {
            throw "Pull command failed"
        }
        Write-ColorOutput "Image pulled successfully" -Type Success
    }
    catch {
        Write-ColorOutput "Failed to pull image: $_" -Type Error
        exit 1
    }
}

function Start-Container {
    param([string]$ContainerEngine)
    
    # Check if container already exists
    if (Test-ContainerExists $ContainerEngine) {
        if (Test-ContainerRunning $ContainerEngine) {
            Write-ColorOutput "Container '$Script:ContainerName' is already running" -Type Warning
            Show-AccessInfo
            return
        }
        else {
            Write-ColorOutput "Starting existing container '$Script:ContainerName'" -Type Info
            & $ContainerEngine start $Script:ContainerName
            Write-ColorOutput "Container started" -Type Success
            Show-AccessInfo
            return
        }
    }
    
    # Pull latest image
    Invoke-PullImage $ContainerEngine
    
    # Prepare run arguments
    $runArgs = @(
        'run'
        '--name', $Script:ContainerName
        '--publish', "$Port`:18888"
        '--publish', "$OtlpPort`:$Script:ContainerOtlpPort"
        '--env', 'DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true'
        '--env', 'ASPNETCORE_ENVIRONMENT=Development'
        '--rm'
    )
    
    if (-not $Foreground) {
        $runArgs += '--detach'
    }
    else {
        $runArgs += '--interactive', '--tty'
    }
    
    $runArgs += $Script:ImageName
    
    Write-ColorOutput "Starting Aspire Dashboard with $ContainerEngine" -Type Info
    Write-ColorOutput "Command: $ContainerEngine $($runArgs -join ' ')" -Type Info
    
    try {
        & $ContainerEngine @runArgs
        if ($LASTEXITCODE -ne 0) {
            throw "Container start failed"
        }
        
        if (-not $Foreground) {
            # Wait a moment for container to start
            Start-Sleep -Seconds 2
            
            if (Test-ContainerRunning $ContainerEngine) {
                Write-ColorOutput "Aspire Dashboard started successfully" -Type Success
                Show-AccessInfo
            }
            else {
                Write-ColorOutput "Container failed to start properly" -Type Error
                & $ContainerEngine logs $Script:ContainerName
                exit 1
            }
        }
    }
    catch {
        Write-ColorOutput "Failed to start container: $_" -Type Error
        exit 1
    }
}

function Stop-Container {
    param([string]$ContainerEngine)
    
    if (-not (Test-ContainerExists $ContainerEngine)) {
        Write-ColorOutput "Container '$Script:ContainerName' does not exist" -Type Warning
        return
    }
    
    if (Test-ContainerRunning $ContainerEngine) {
        Write-ColorOutput "Stopping container '$Script:ContainerName'" -Type Info
        & $ContainerEngine stop $Script:ContainerName
        Write-ColorOutput "Container stopped" -Type Success
    }
    else {
        Write-ColorOutput "Container '$Script:ContainerName' is not running" -Type Warning
    }
}

function Show-Logs {
    param([string]$ContainerEngine)
    
    if (-not (Test-ContainerExists $ContainerEngine)) {
        Write-ColorOutput "Container '$Script:ContainerName' does not exist" -Type Error
        exit 1
    }
    
    Write-ColorOutput "Showing logs for '$Script:ContainerName'" -Type Info
    & $ContainerEngine logs --follow $Script:ContainerName
}

function Show-Status {
    param([string]$ContainerEngine)
    
    Write-ColorOutput "Container status for '$Script:ContainerName':" -Type Info
    
    if (Test-ContainerExists $ContainerEngine) {
        & $ContainerEngine ps -a --filter "name=$Script:ContainerName" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
        
        if (Test-ContainerRunning $ContainerEngine) {
            Write-Host
            Write-ColorOutput "Container is running and accessible at:" -Type Info
            Show-AccessInfo
        }
    }
    else {
        Write-Host "Container does not exist"
    }
}

function Invoke-Cleanup {
    param([string]$ContainerEngine)
    
    Write-ColorOutput "Cleaning up Aspire Dashboard resources" -Type Info
    
    # Stop and remove container
    if (Test-ContainerExists $ContainerEngine) {
        if (Test-ContainerRunning $ContainerEngine) {
            Stop-Container $ContainerEngine
        }
        
        Write-ColorOutput "Removing container '$Script:ContainerName'" -Type Info
        & $ContainerEngine rm $Script:ContainerName 2>$null
    }
    
    # Remove image
    Write-ColorOutput "Removing image '$Script:ImageName'" -Type Info
    & $ContainerEngine rmi $Script:ImageName 2>$null
    
    Write-ColorOutput "Cleanup completed" -Type Success
}

function Show-AccessInfo {
    $accessInfo = @"

Aspire Dashboard is ready!

📊 Dashboard: http://localhost:$Port
🔗 OTLP Endpoint: http://localhost:$OtlpPort

Environment Variables for your tests:
`$env:OTEL_EXPORTER_OTLP_ENDPOINT = "http://localhost:$OtlpPort"
`$env:OTEL_EXPORTER_OTLP_ENABLED = "true"

To stop: .\run-aspire-dashboard.ps1 stop
To view logs: .\run-aspire-dashboard.ps1 logs

"@
    Write-Host $accessInfo -ForegroundColor Green
}

# Main execution
if ($Help) {
    Show-Help
    exit 0
}

$containerEngine = Get-ContainerEngine
Write-ColorOutput "Using container engine: $containerEngine" -Type Info

switch ($Command) {
    'start' { Start-Container $containerEngine }
    'stop' { Stop-Container $containerEngine }
    'restart' { 
        Stop-Container $containerEngine
        Start-Sleep -Seconds 1
        Start-Container $containerEngine
    }
    'logs' { Show-Logs $containerEngine }
    'status' { Show-Status $containerEngine }
    'cleanup' { Invoke-Cleanup $containerEngine }
}
