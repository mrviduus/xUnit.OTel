# Aspire Dashboard Scripts

This directory contains scripts to run the .NET Aspire Dashboard locally for OpenTelemetry observability during development and testing.

## Available Scripts

### 1. PowerShell Script (Recommended for Windows)
- **File**: `run-aspire-dashboard.ps1`
- **Platform**: Windows (PowerShell 5.1+)
- **Features**: Full-featured with colored output and comprehensive error handling

### 2. Batch Script
- **File**: `run-aspire-dashboard.bat`
- **Platform**: Windows (Command Prompt)
- **Features**: Basic functionality for environments without PowerShell

### 3. Bash Script
- **File**: `run-aspire-dashboard.sh`
- **Platform**: Linux/macOS/WSL
- **Features**: Full-featured with colored output and comprehensive error handling

## Quick Start

### Windows (PowerShell)
```powershell
# Start the dashboard
.\scripts\run-aspire-dashboard.ps1

# Start with specific engine
.\scripts\run-aspire-dashboard.ps1 -Engine docker

# Stop the dashboard
.\scripts\run-aspire-dashboard.ps1 stop

# View logs
.\scripts\run-aspire-dashboard.ps1 logs
```

### Windows (Command Prompt)
```cmd
# Start the dashboard
scripts\run-aspire-dashboard.bat

# Stop the dashboard
scripts\run-aspire-dashboard.bat stop
```

### Linux/macOS/WSL
```bash
# Make executable (first time only)
chmod +x scripts/run-aspire-dashboard.sh

# Start the dashboard
./scripts/run-aspire-dashboard.sh

# Stop the dashboard
./scripts/run-aspire-dashboard.sh stop
```

## Commands

All scripts support the following commands:

- `start` (default) - Start the Aspire Dashboard
- `stop` - Stop and remove the container
- `restart` - Restart the container
- `logs` - Show container logs
- `status` - Show container status
- `cleanup` - Remove container and image

## Options

### PowerShell Script Options
- `-Engine` - Container engine to use (docker|podman)
- `-Port` - Dashboard port (default: 18888)
- `-OtlpPort` - OTLP port (default: 4317)
- `-Foreground` - Run in foreground mode
- `-Help` - Show help information

### Bash Script Options
- `-e, --engine` - Container engine to use (docker|podman)
- `-p, --port` - Dashboard port (default: 18888)
- `-o, --otlp-port` - OTLP port (default: 4317)
- `-f, --foreground` - Run in foreground mode
- `-h, --help` - Show help information

### Batch Script Options
- `--engine` - Container engine to use (docker|podman)
- `--port` - Dashboard port (default: 18888)
- `--otlp-port` - OTLP port (default: 4317)
- `--foreground` - Run in foreground mode
- `--help` - Show help information

## Environment Variables

You can override default settings using environment variables:

```powershell
# PowerShell
$env:CONTAINER_ENGINE = "docker"
$env:ASPIRE_DASHBOARD_PORT = "19999"
$env:ASPIRE_OTLP_PORT = "4318"
```

```bash
# Bash
export CONTAINER_ENGINE=docker
export ASPIRE_DASHBOARD_PORT=19999
export ASPIRE_OTLP_PORT=4318
```

```cmd
# Command Prompt
set CONTAINER_ENGINE=docker
set ASPIRE_DASHBOARD_PORT=19999
set ASPIRE_OTLP_PORT=4318
```

## Container Requirements

You need either **Docker** or **Podman** installed:

### Docker
- Download from: https://docs.docker.com/get-docker/
- Verify installation: `docker --version`

### Podman
- Download from: https://podman.io/getting-started/installation
- Verify installation: `podman --version`

The scripts will automatically detect which container engine is available.

## Access Information

Once started, the Aspire Dashboard will be available at:

- **Dashboard UI**: http://localhost:18888
- **OTLP Endpoint**: http://localhost:4317

## Integration with Tests

To configure your tests to send telemetry to the dashboard:

### PowerShell
```powershell
$env:OTEL_EXPORTER_OTLP_ENDPOINT = "http://localhost:4317"
$env:OTEL_EXPORTER_OTLP_ENABLED = "true"
```

### Bash
```bash
export OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
export OTEL_EXPORTER_OTLP_ENABLED=true
```

### Command Prompt
```cmd
set OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
set OTEL_EXPORTER_OTLP_ENABLED=true
```

## Troubleshooting

### Container Engine Not Found
```
[ERROR] Neither Docker nor Podman found. Please install one of them.
```
**Solution**: Install Docker or Podman from the links above.

### Port Already in Use
```
[ERROR] Failed to start container
```
**Solution**: Check if another service is using ports 18888 or 4317, or use custom ports:
```powershell
.\run-aspire-dashboard.ps1 -Port 19999 -OtlpPort 4318
```

### Container Already Running
```
[WARNING] Container 'aspire-dashboard' is already running
```
**Solution**: This is normal - the dashboard is already available. Use `stop` command if you need to restart.

### Permission Issues (Linux/macOS)
```
Permission denied
```
**Solution**: Make the script executable:
```bash
chmod +x scripts/run-aspire-dashboard.sh
```

## Features

### Auto-Detection
- Automatically detects available container engine (Docker/Podman)
- Falls back gracefully if preferred engine is not available

### Container Management
- Prevents duplicate containers
- Automatic cleanup on stop
- Health checking and status reporting

### Development Friendly
- Colored output for better readability
- Comprehensive error messages
- Environment variable support
- Multiple platform support

### Production Ready
- Proper error handling
- Logging and status commands
- Configurable ports for different environments
- Clean shutdown and cleanup

## Examples

### Development Workflow
```powershell
# Start dashboard for development
.\scripts\run-aspire-dashboard.ps1

# Run your tests (in another terminal)
dotnet test

# View telemetry in dashboard at http://localhost:18888

# When done, stop the dashboard
.\scripts\run-aspire-dashboard.ps1 stop
```

### CI/CD Integration
```bash
# Start dashboard in background
./scripts/run-aspire-dashboard.sh start

# Run tests with telemetry
export OTEL_EXPORTER_OTLP_ENABLED=true
dotnet test

# Cleanup
./scripts/run-aspire-dashboard.sh cleanup
```

### Custom Configuration
```powershell
# Use custom ports to avoid conflicts
.\scripts\run-aspire-dashboard.ps1 -Port 19999 -OtlpPort 4318

# Force specific container engine
.\scripts\run-aspire-dashboard.ps1 -Engine podman

# Run in foreground for debugging
.\scripts\run-aspire-dashboard.ps1 -Foreground
```
