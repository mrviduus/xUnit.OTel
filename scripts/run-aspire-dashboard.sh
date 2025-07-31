#!/bin/bash

# Aspire Dashboard Local Runner
# This script runs the .NET Aspire Dashboard for OpenTelemetry observability
# Supports both Docker and Podman with automatic detection

set -euo pipefail  # Exit on error, undefined vars, and pipe failures

# Script configuration
readonly SCRIPT_NAME="$(basename "$0")"
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Container configuration
readonly CONTAINER_NAME="aspire-dashboard"
readonly IMAGE_NAME="mcr.microsoft.com/dotnet/nightly/aspire-dashboard:8.0.0"
readonly DASHBOARD_PORT="18888"
readonly OTLP_PORT="4317"
readonly CONTAINER_OTLP_PORT="18889"

# Colors for output
readonly RED='\033[0;31m'
readonly GREEN='\033[0;32m'
readonly YELLOW='\033[1;33m'
readonly BLUE='\033[0;34m'
readonly NC='\033[0m' # No Color

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $*" >&2
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $*" >&2
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $*" >&2
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $*" >&2
}

# Help function
show_help() {
    cat << EOF
${SCRIPT_NAME} - Run .NET Aspire Dashboard for OpenTelemetry

USAGE:
    ${SCRIPT_NAME} [OPTIONS] [COMMAND]

COMMANDS:
    start       Start the Aspire Dashboard (default)
    stop        Stop and remove the container
    restart     Restart the container
    logs        Show container logs
    status      Show container status
    cleanup     Remove container and image

OPTIONS:
    -e, --engine ENGINE     Container engine to use (docker|podman)
    -p, --port PORT         Dashboard port (default: ${DASHBOARD_PORT})
    -o, --otlp-port PORT    OTLP port (default: ${OTLP_PORT})
    -d, --detach            Run in detached mode (default)
    -f, --foreground        Run in foreground mode
    -h, --help              Show this help message

EXAMPLES:
    ${SCRIPT_NAME}                          # Start with auto-detected engine
    ${SCRIPT_NAME} --engine docker          # Force Docker usage
    ${SCRIPT_NAME} --port 19999             # Use custom dashboard port
    ${SCRIPT_NAME} stop                     # Stop the container
    ${SCRIPT_NAME} logs                     # View logs

ENVIRONMENT VARIABLES:
    CONTAINER_ENGINE                        # Preferred container engine
    ASPIRE_DASHBOARD_PORT                   # Dashboard port override
    ASPIRE_OTLP_PORT                        # OTLP port override

PORTS:
    ${DASHBOARD_PORT}     - Aspire Dashboard web interface
    ${OTLP_PORT}      - OpenTelemetry Protocol (OTLP) endpoint

ACCESS:
    Dashboard: http://localhost:${DASHBOARD_PORT}
    OTLP:      http://localhost:${OTLP_PORT}

EOF
}

# Detect available container engine
detect_container_engine() {
    local engine="${CONTAINER_ENGINE:-}"
    
    if [[ -n "$engine" ]]; then
        if command -v "$engine" >/dev/null 2>&1; then
            echo "$engine"
            return 0
        else
            log_warning "Specified engine '$engine' not found, falling back to auto-detection"
        fi
    fi
    
    if command -v podman >/dev/null 2>&1; then
        echo "podman"
    elif command -v docker >/dev/null 2>&1; then
        echo "docker"
    else
        log_error "Neither Docker nor Podman found. Please install one of them."
        exit 1
    fi
}

# Check if container exists
container_exists() {
    local engine="$1"
    $engine ps -a --format "table {{.Names}}" | grep -q "^${CONTAINER_NAME}$"
}

# Check if container is running
container_running() {
    local engine="$1"
    $engine ps --format "table {{.Names}}" | grep -q "^${CONTAINER_NAME}$"
}

# Pull latest image
pull_image() {
    local engine="$1"
    
    log_info "Pulling latest image: ${IMAGE_NAME}"
    if ! $engine pull "$IMAGE_NAME"; then
        log_error "Failed to pull image"
        exit 1
    fi
    log_success "Image pulled successfully"
}

# Start container
start_container() {
    local engine="$1"
    local dashboard_port="${2:-$DASHBOARD_PORT}"
    local otlp_port="${3:-$OTLP_PORT}"
    local detached="${4:-true}"
    
    # Check if container already exists
    if container_exists "$engine"; then
        if container_running "$engine"; then
            log_warning "Container '$CONTAINER_NAME' is already running"
            show_access_info "$dashboard_port" "$otlp_port"
            return 0
        else
            log_info "Starting existing container '$CONTAINER_NAME'"
            $engine start "$CONTAINER_NAME"
            log_success "Container started"
            show_access_info "$dashboard_port" "$otlp_port"
            return 0
        fi
    fi
    
    # Pull latest image
    pull_image "$engine"
    
    # Prepare run command
    local run_args=(
        "run"
        "--name" "$CONTAINER_NAME"
        "--publish" "${dashboard_port}:18888"
        "--publish" "${otlp_port}:${CONTAINER_OTLP_PORT}"
        "--env" "DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true"
        "--env" "ASPNETCORE_ENVIRONMENT=Development"
        "--rm"  # Auto-remove when stopped
    )
    
    if [[ "$detached" == "true" ]]; then
        run_args+=("--detach")
    else
        run_args+=("--interactive" "--tty")
    fi
    
    run_args+=("$IMAGE_NAME")
    
    log_info "Starting Aspire Dashboard with $engine"
    log_info "Command: $engine ${run_args[*]}"
    
    if ! $engine "${run_args[@]}"; then
        log_error "Failed to start container"
        exit 1
    fi
    
    if [[ "$detached" == "true" ]]; then
        # Wait a moment for container to start
        sleep 2
        
        if container_running "$engine"; then
            log_success "Aspire Dashboard started successfully"
            show_access_info "$dashboard_port" "$otlp_port"
        else
            log_error "Container failed to start properly"
            $engine logs "$CONTAINER_NAME"
            exit 1
        fi
    fi
}

# Stop container
stop_container() {
    local engine="$1"
    
    if ! container_exists "$engine"; then
        log_warning "Container '$CONTAINER_NAME' does not exist"
        return 0
    fi
    
    if container_running "$engine"; then
        log_info "Stopping container '$CONTAINER_NAME'"
        $engine stop "$CONTAINER_NAME"
        log_success "Container stopped"
    else
        log_warning "Container '$CONTAINER_NAME' is not running"
    fi
}

# Show logs
show_logs() {
    local engine="$1"
    
    if ! container_exists "$engine"; then
        log_error "Container '$CONTAINER_NAME' does not exist"
        exit 1
    fi
    
    log_info "Showing logs for '$CONTAINER_NAME'"
    $engine logs --follow "$CONTAINER_NAME"
}

# Show status
show_status() {
    local engine="$1"
    
    log_info "Container status for '$CONTAINER_NAME':"
    
    if container_exists "$engine"; then
        $engine ps -a --filter "name=${CONTAINER_NAME}" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
        
        if container_running "$engine"; then
            echo
            log_info "Container is running and accessible at:"
            show_access_info
        fi
    else
        echo "Container does not exist"
    fi
}

# Cleanup
cleanup() {
    local engine="$1"
    
    log_info "Cleaning up Aspire Dashboard resources"
    
    # Stop and remove container
    if container_exists "$engine"; then
        if container_running "$engine"; then
            stop_container "$engine"
        fi
        
        log_info "Removing container '$CONTAINER_NAME'"
        $engine rm "$CONTAINER_NAME" 2>/dev/null || true
    fi
    
    # Remove image
    log_info "Removing image '$IMAGE_NAME'"
    $engine rmi "$IMAGE_NAME" 2>/dev/null || true
    
    log_success "Cleanup completed"
}

# Show access information
show_access_info() {
    local dashboard_port="${1:-$DASHBOARD_PORT}"
    local otlp_port="${2:-$OTLP_PORT}"
    
    cat << EOF

${GREEN}Aspire Dashboard is ready!${NC}

📊 Dashboard: ${BLUE}http://localhost:${dashboard_port}${NC}
🔗 OTLP Endpoint: ${BLUE}http://localhost:${otlp_port}${NC}

Environment Variables for your tests:
${YELLOW}export OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:${otlp_port}${NC}
${YELLOW}export OTEL_EXPORTER_OTLP_ENABLED=true${NC}

To stop: ${SCRIPT_NAME} stop
To view logs: ${SCRIPT_NAME} logs

EOF
}

# Main function
main() {
    local command="start"
    local engine=""
    local dashboard_port="${ASPIRE_DASHBOARD_PORT:-$DASHBOARD_PORT}"
    local otlp_port="${ASPIRE_OTLP_PORT:-$OTLP_PORT}"
    local detached="true"
    
    # Parse arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            -e|--engine)
                engine="$2"
                shift 2
                ;;
            -p|--port)
                dashboard_port="$2"
                shift 2
                ;;
            -o|--otlp-port)
                otlp_port="$2"
                shift 2
                ;;
            -d|--detach)
                detached="true"
                shift
                ;;
            -f|--foreground)
                detached="false"
                shift
                ;;
            -h|--help)
                show_help
                exit 0
                ;;
            start|stop|restart|logs|status|cleanup)
                command="$1"
                shift
                ;;
            *)
                log_error "Unknown option: $1"
                echo "Use --help for usage information"
                exit 1
                ;;
        esac
    done
    
    # Detect container engine if not specified
    if [[ -z "$engine" ]]; then
        engine=$(detect_container_engine)
    fi
    
    log_info "Using container engine: $engine"
    
    # Execute command
    case $command in
        start)
            start_container "$engine" "$dashboard_port" "$otlp_port" "$detached"
            ;;
        stop)
            stop_container "$engine"
            ;;
        restart)
            stop_container "$engine"
            sleep 1
            start_container "$engine" "$dashboard_port" "$otlp_port" "$detached"
            ;;
        logs)
            show_logs "$engine"
            ;;
        status)
            show_status "$engine"
            ;;
        cleanup)
            cleanup "$engine"
            ;;
    esac
}

# Run main function with all arguments
main "$@"
