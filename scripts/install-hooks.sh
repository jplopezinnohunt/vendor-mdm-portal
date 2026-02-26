#!/bin/bash
#
# Install git hooks for Vendor MDM Portal
# Run this once after cloning the repo
#

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
HOOKS_DIR="$REPO_ROOT/.git/hooks"

echo "Installing git hooks..."

# Pre-commit hook
if [ -f "$SCRIPT_DIR/hooks/pre-commit" ]; then
    cp "$SCRIPT_DIR/hooks/pre-commit" "$HOOKS_DIR/pre-commit"
    chmod +x "$HOOKS_DIR/pre-commit"
    echo "✓ Installed pre-commit hook"
else
    echo "✗ pre-commit hook not found at $SCRIPT_DIR/hooks/pre-commit"
    exit 1
fi

echo ""
echo "Git hooks installed successfully!"
echo "Pre-commit hook will now enforce Brain Rules Section 8."
