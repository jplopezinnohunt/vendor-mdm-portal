# Install .NET SDK for Local Development

## Quick Install (macOS)

### Option 1: Direct Download (Recommended)
1. Visit: https://dotnet.microsoft.com/download/dotnet/8.0
2. Download the **.NET 8.0 SDK** for macOS
3. Run the installer package
4. Verify installation:
   ```bash
   dotnet --version
   # Should show: 8.0.x
   ```

### Option 2: Using Homebrew (if you have it)
```bash
brew install --cask dotnet-sdk
```

### Option 3: Using Installer Script
```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0
```

## Verify Installation

After installation, verify:
```bash
dotnet --version
# Should output: 8.0.x

dotnet --list-sdks
# Should show installed SDKs
```

## Add to PATH (if needed)

If `dotnet` command is not found after installation, add to your `~/.zshrc`:
```bash
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$PATH:$HOME/.dotnet:$HOME/.dotnet/tools"
```

Then reload:
```bash
source ~/.zshrc
```

## After Installation

Once .NET SDK is installed, you can run the backend:

```bash
cd backend/VendorMdm.Api
dotnet restore
dotnet run
```

---

*The frontend is already running. Once .NET is installed, start the backend in a new terminal.*

