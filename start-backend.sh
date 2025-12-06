#!/bin/bash
# Start Backend API Script
# This script will start the backend API on port 5001

echo "🚀 Starting Vendor MDM Backend API..."
echo ""

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK is not installed!"
    echo ""
    echo "Please install .NET 8 SDK first:"
    echo "1. Visit: https://dotnet.microsoft.com/download/dotnet/8.0"
    echo "2. Download and install the macOS installer"
    echo "3. Open a new terminal and run this script again"
    echo ""
    echo "Or see: INSTALL_DOTNET_QUICK.md for detailed instructions"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version 2>/dev/null)
echo "✅ .NET SDK found: $DOTNET_VERSION"
echo ""

# Navigate to API project
cd "$(dirname "$0")/backend/VendorMdm.Api" || exit 1

echo "📦 Restoring dependencies..."
dotnet restore

if [ $? -ne 0 ]; then
    echo "❌ Failed to restore dependencies"
    exit 1
fi

echo ""
echo "✅ Dependencies restored"
echo ""
echo "🌐 Starting API server on http://localhost:5001"
echo "📖 Swagger UI will be available at: http://localhost:5001/swagger"
echo ""
echo "Press Ctrl+C to stop the server"
echo ""

# Start the API
dotnet run

