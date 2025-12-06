# 🚀 Quick .NET 8 SDK Installation Guide

## Option 1: Direct Download (Recommended - Easiest)

1. **Visit**: https://dotnet.microsoft.com/download/dotnet/8.0
2. **Download**: Click "Download .NET SDK 8.0.x" for macOS
3. **Install**: 
   - Open the downloaded `.pkg` file
   - Follow the installation wizard
   - This will install to `/usr/local/share/dotnet`
4. **Verify**: Open a new terminal and run:
   ```bash
   dotnet --version
   ```
   Should show: `8.0.x`

## Option 2: Install Homebrew First (Then .NET)

If you want to use Homebrew:

```bash
# Install Homebrew (if not installed)
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Then install .NET
brew install --cask dotnet-sdk
```

## After Installation

1. **Open a NEW terminal** (to refresh PATH)
2. **Verify installation**:
   ```bash
   dotnet --version
   ```
3. **Start the backend**:
   ```bash
   cd backend/VendorMdm.Api
   dotnet restore
   dotnet run
   ```

## Troubleshooting

### "dotnet: command not found" after installation
Add to your `~/.zshrc`:
```bash
export PATH="$PATH:/usr/local/share/dotnet"
export DOTNET_ROOT="/usr/local/share/dotnet"
```

Then reload:
```bash
source ~/.zshrc
```

### Check installation location
```bash
# Check if .NET is installed but not in PATH
ls -la /usr/local/share/dotnet/dotnet
ls -la ~/.dotnet/dotnet
```

---

**Once .NET is installed, I can help you start the backend!**

