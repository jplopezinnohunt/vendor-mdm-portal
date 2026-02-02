# Performance Optimization Standard: Generated Columns for JSONB Search

## Pattern: PostgreSQL/SQL Server Computed Columns

### Problem
Searching within JSONB `Attributes` columns is slow without indexes.

### Solution
Create computed columns that extract frequently-searched JSON keys, then index them.

### SQL Server Example
```sql
-- Add computed column
ALTER TABLE VendorInvitations 
ADD Currency_Computed AS JSON_VALUE(Attributes, '$.Currency') PERSISTED;

-- Index it
CREATE INDEX IX_VendorInvitations_Currency 
ON VendorInvitations(Currency_Computed) 
WHERE Currency_Computed IS NOT NULL;
```

### PostgreSQL Example
```sql
-- Add generated column
ALTER TABLE vendor_invitations 
ADD COLUMN currency_computed TEXT 
GENERATED ALWAYS AS (attributes->>'Currency') STORED;

-- Index it
CREATE INDEX idx_vendor_invitations_currency 
ON vendor_invitations(currency_computed) 
WHERE currency_computed IS NOT NULL;
```

### Recommended Fields
- `VendorInvitations`: `Currency`, `SapLanguage`
- `VendorApplications`: `VendorType`, `AccountGroup`

### Performance Impact
- **Before**: Full table scan + JSON parsing
- **After**: Index seek (100-1000x faster)

### When to Apply
- Field is searched/filtered frequently (>10% of queries)
- Table has >1000 rows
- Query performance <400ms (Doherty Threshold)
