#!/bin/bash
# Install git hooks for SDAHymns

echo "Installing git hooks..."

# Create .git/hooks directory if it doesn't exist
mkdir -p .git/hooks

# Copy pre-commit hook
cp scripts/pre-commit .git/hooks/pre-commit
chmod +x .git/hooks/pre-commit

echo "✅ Git hooks installed successfully!"
echo ""
echo "The pre-commit hook will now run automatically before each commit."
echo "It will:"
echo "  - Build the solution (type checking)"
echo "  - Run all tests"
echo ""
echo "To skip the hook temporarily, use: git commit --no-verify"
