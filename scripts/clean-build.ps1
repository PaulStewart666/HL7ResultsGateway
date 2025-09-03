# PowerShell script to clean and build without file locking issues
# This closes VS Code temporarily to release file handles

# Function to clean and build with proper file handle management
function Clean-And-Build-Project {
    Write-Host "Stopping potential file watchers..."

    # Kill any running dotnet processes that might be locking files
    Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

    # Wait a moment for processes to fully terminate
    Start-Sleep -Seconds 2

    Write-Host "Running dotnet clean..."
    dotnet clean

    # Wait a moment before building
    Start-Sleep -Seconds 1

    Write-Host "Running dotnet build..."
    dotnet build
}

# Run the function
Clean-And-Build-Project
