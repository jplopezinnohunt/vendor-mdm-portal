#!/bin/bash
# Setup Azure Connection Strings for Local Development
# This script helps you configure User Secrets for Azure connections

echo "🔧 Setting up Azure Connection Strings for Local Development"
echo ""

# Check if user-secrets is initialized
if ! dotnet user-secrets list > /dev/null 2>&1; then
    echo "📦 Initializing User Secrets..."
    dotnet user-secrets init
    echo "✅ User Secrets initialized"
    echo ""
fi

echo "Please provide your Azure connection strings:"
echo ""

# SQL Connection String
read -p "SQL Connection String (or press Enter to skip): " SQL_CONN
if [ ! -z "$SQL_CONN" ]; then
    dotnet user-secrets set "ConnectionStrings:Sql" "$SQL_CONN"
    echo "✅ SQL connection string saved"
fi

# Cosmos Connection String
read -p "Cosmos DB Connection String (or press Enter to skip): " COSMOS_CONN
if [ ! -z "$COSMOS_CONN" ]; then
    dotnet user-secrets set "ConnectionStrings:Cosmos" "$COSMOS_CONN"
    echo "✅ Cosmos DB connection string saved"
fi

# Service Bus Connection String
read -p "Service Bus Connection String (or press Enter to skip): " SB_CONN
if [ ! -z "$SB_CONN" ]; then
    dotnet user-secrets set "ConnectionStrings:ServiceBus" "$SB_CONN"
    echo "✅ Service Bus connection string saved"
fi

echo ""
echo "🎉 Setup complete!"
echo ""
echo "To verify your secrets, run:"
echo "  dotnet user-secrets list"
echo ""
echo "To use Azure resources, ensure appsettings.Development.json has:"
echo "  \"UseLocalEmulators\": false"

