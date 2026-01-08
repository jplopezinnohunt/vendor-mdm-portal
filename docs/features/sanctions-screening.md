# Sanctions Screening

The Vendor MDM Portal enforces sanctions screening to prevent doing business with blocked entities.

## Screening Strategy

The system supports a **Hybrid Screening Model** that can switch between different providers based on configuration.

### 1. OFAC Source (Default - Free)
This provider uses the **Official US Treasury SDN List** (Specially Designated Nationals).
*   **Source**: Downloads `sdn.csv` directly from `treasury.gov`.
*   **Cost**: Free (Public Government Data).
*   **API Key**: **Not Required**.
*   **Mechanism**:
    1.  On startup (and every 24h), the service downloads the latest `sdn.csv`.
    2.  Screening performs a **Local Fuzzy Match** against this in-memory list.
    3.  Privacy: Vendor names are **never sent** to any external API; checking is done 100% locally.

### 2. OpenSanctions (Commercial)
This provider uses the [OpenSanctions API](https://www.opensanctions.org).
*   **Source**: Aggregates 300+ lists (OFAC, UN, EU, UK, etc.).
*   **Cost**: Paid (requires API Key).
*   **Mechanism**: Sends vendor data to OpenSanctions API for screening.

## Configuration

The provider is selected in `appsettings.json` (or Azure App Service Configuration).

```json
"SanctionsScreening": {
    "UseMock": false,            // Set to true to simulate matches (Dev only)
    "RealProvider": "OfacSource", // Options: "OfacSource" (Free) or "OpenSanctions" (Paid)
    "MatchThreshold": 0.75,      // Sensitivity (0.1 to 1.0)
    
    // OFAC Settings
    "OfacSettings": {
        "SourceUrl": "https://sanctionslistservice.ofac.treas.gov/api/publicationpreview/exports/sdn.csv"
    },

    // OpenSanctions Settings
    "RealSettings": {
        "ApiKey": "YOUR_KEY_HERE"
    }
}
```

## Screening Points

Screening occurs automatically at two key points:
1.  **Invitation**: When inviting a new vendor (`POST /invitation`).
2.  **Vendor Creation**: When creating a direct vendor (`POST /vendor`).

## Handling Matches & False Positives

If a match is found (Risk Level: `High` or `Critical`):
*   The API returns `409 Conflict`.
*   The response includes the matched entity name (e.g., "Matched with 'PUTIN, Vladimir'").

### Overrule (Force Creation)
For false positives, authorized users can bypass the check using the **Force Creation** flag.

*   **API**: Add `?force=true` query parameter.
*   **Audit**: The bypass is logged as a **Warning** in the system logs, noting that a potential sanctions match was willfully ignored.

----
*Last Updated: 2026-01-08 (Added OFAC Source)*
