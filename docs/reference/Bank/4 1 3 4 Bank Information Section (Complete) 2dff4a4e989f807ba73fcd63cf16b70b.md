# 4.1.3.4 Bank Information Section (Complete)

Bank Section have 2 subsections

Bank and Account

**Critical Validation Message:**

```
┌────────────────────────────────────────────────────────────┐
│ ⓘ First choose bank country to unlock other fields        │
└────────────────────────────────────────────────────────────┘
```

![image.png](4%201%203%204%20Bank%20Information%20Section%20(Complete)/image.png)

![image.png](4%201%203%204%20Bank%20Information%20Section%20(Complete)/image%201.png)

**Section Header:** “Bank Information”

**Controls:**

- “Collapse all” link
- “Expand all” link
- “- Account n°:” subsection (expandable/collapsible)
- “REMOVE BANK” button (top right)

**Bank Group Fields (Initial State - ALL READONLY):**

| Field | Type | Required | Initial State | Note |
| --- | --- | --- | --- | --- |
| Name | Text | Yes (*) | readonly | Bank name |
| Abbreviation | Text | No | readonly | Short name |
| Bank agency | Text | Yes (*) | readonly | Branch name |
| Agency Address | Text | Yes (*) | readonly | Branch address |
| City and Postal Code | Text | Yes (*) | readonly | Branch city/postal |
| **Country** | **Dropdown** | **Yes (*)** | **ENABLED** | **UNLOCKING FIELD** |
| Account Holder Name | Text | Yes (*) | readonly | Beneficiary name |
| Account Currency | Dropdown | Yes (*) | disabled | Unlocked after country |
| Additional bank information | Textarea | No | readonly | Free text |

**Account Group Fields (Initial State - ALL READONLY):**

![image.png](4%201%203%204%20Bank%20Information%20Section%20(Complete)/63b24fe8-1c1a-4b38-aeba-c715f648ba74.png)

| Field | Type | Required | Initial State | Visibility |
| --- | --- | --- | --- | --- |
| Account Number | Text | Yes (*) | readonly | Always visible |
| Control Key | Text | No | readonly | Country-dependent (DE, AT) |
| IBAN | Text | Conditional | readonly | SEPA countries |
| SWIFT/BIC Code | Text | Yes (*) | readonly | Always required |
| Bank number and Branch code | Text | Conditional | readonly | Country-dependent |

**Bank Document Group:**

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| Bank documents | File upload | Yes (*) | Max 2 files |
| Display | Static text | N/A | “0 file(s) - (confidential information)” |

[Bank Country Validation - Service Analysis & Behavior](https://www.notion.so/Bank-Country-Validation-Service-Analysis-Behavior-2dcf4a4e989f80e6a561ca49a4a4ac53?pvs=21)

**Bank Country Selection Service Call:**

```
POST /api/bank/configuration
Content-Type: application/json

Request:
{
  "countryCode": "FR",
  "companyCode": "UNES",
  "vendorType": "INDV"
}

Response:
{
  "countryCode": "FR",
  "countryName": "France",
  "region": "SEPA",
  "fieldConfiguration": {
    "showIBAN": true,
    "showControlKey": false,
    "showBankNumber": false,
    "showSwiftBIC": true,
    "showAccountNumber": true,
    "ibanMandatory": true,
    "swiftMandatory": true,
    "accountNumberMandatory": false
  },
  "validationRules": {
    "ibanFormat": "FR[0-9]{2}[0-9]{10}[A-Z0-9]{11}[0-9]{2}",
    "ibanLength": 27,
    "ibanChecksum": true,
    "swiftFormat": "^[A-Z]{6}[A-Z0-9]{2}([A-Z0-9]{3})?$",
    "swiftLength": [8, 11]
  },
  "mandatoryFields": [
    "BankName",
    "BankAgency",
    "AgencyAddress",
    "IBAN",
    "SWIFT"
  ],
  "sapMapping": {
    "primaryBankKey": "IBAN",
    "bankCountry": "FR",
    "paymentMethod": "SEPA_DD"
  }
}
```

**Field Unlocking After Country Selection:**

**France (SEPA) Configuration:**

```jsx
// Fields become editable:
document.getElementById('BankName').removeAttribute('readonly');
document.getElementById('BankAgency').removeAttribute('readonly');
document.getElementById('AgencyAddress').removeAttribute('readonly');
document.getElementById('AccountHolderName').removeAttribute('readonly');
document.getElementById('IBAN').removeAttribute('readonly');
document.getElementById('SwiftBic').removeAttribute('readonly');
document.getElementById('AccountNumber').removeAttribute('readonly');
document.getElementById('AccountCurrency').removeAttribute('disabled');

// Show/hide fields based on country:
document.getElementById('divIBAN_0').style.display = 'block';      // Show IBAN
document.getElementById('divControlKey_0').style.display = 'none'; // Hide Control Key
```

**Country-Specific Configurations:**

**SEPA Countries (France, Germany, Spain, Italy, Netherlands, etc.):**

```json
{
  "region": "SEPA",
  "requiredFields": ["IBAN", "SWIFT", "BankName", "BankAgency"],
  "fieldVisibility": {
    "iban": true,
    "swift": true,
    "accountNumber": true,
    "controlKey": false,  // Except Germany/Austria
    "bankNumber": false
  },
  "validations": {
    "ibanFormat": "Country-specific (FR27, DE22, ES24, IT27, NL18)",
    "ibanChecksum": "MOD-97 validation",
    "swiftFormat": "ISO 9362 (8 or 11 characters)"
  },
  "sapPaymentMethod": "SEPA_DD"
}
```

**Germany/Austria Special Case:**

```json
{
  "region": "SEPA",
  "country": "DE",
  "requiredFields": ["IBAN", "SWIFT", "ControlKey", "BankName"],
  "fieldVisibility": {
    "iban": true,
    "swift": true,
    "accountNumber": true,
    "controlKey": true,     // UNIQUE to DE/AT
    "bankNumber": false
  },
  "controlKeyValues": ["01", "02", "03", "21", "51", "99"]
}
```

**United States Configuration:**

```json
{
  "region": "North America",
  "country": "US",
  "requiredFields": ["AccountNumber", "RoutingNumber", "SWIFT", "BankName"],
  "fieldVisibility": {
    "iban": false,          // Not used domestically
    "swift": true,          // For international payments
    "accountNumber": true,
    "controlKey": false,
    "bankNumber": true      // ABA Routing Number
  },
  "validations": {
    "routingNumberFormat": "^[0-9]{9}$",
    "routingNumberChecksum": "ABA checksum algorithm",
    "accountNumberFormat": "Variable length"
  },
  "sapPaymentMethod": "ACH"
}
```

**Argentina Configuration:**

```json
{
  "region": "Latin America",
  "country": "AR",
  "requiredFields": ["CBU", "SWIFT", "BankName", "BankCode"],
  "fieldVisibility": {
    "iban": false,
    "swift": true,
    "accountNumber": true,
    "cbu": true,            // Argentina-specific: 22-digit CBU
    "bankCode": true,
    "branchCode": true
  },
  "validations": {
    "cbuFormat": "^[0-9]{22}$",
    "cbuChecksum": "Verhoeff algorithm",
    "bankCodeFormat": "^[0-9]{3}$",
    "branchCodeFormat": "^[0-9]{4}$"
  },
  "sapPaymentMethod": "WIRE"
}
```

**United Kingdom Configuration:**

```json
{
  "region": "Europe (Non-SEPA)",
  "country": "GB",
  "requiredFields": ["IBAN", "SortCode", "AccountNumber", "SWIFT"],
  "fieldVisibility": {
    "iban": true,           // Still used post-Brexit
    "swift": true,
    "accountNumber": true,
    "sortCode": true,       // 6-digit sort code
    "bankNumber": false
  },
  "validations": {
    "ibanFormat": "GB[0-9]{2}[A-Z]{4}[0-9]{14}",
    "ibanLength": 22,
    "sortCodeFormat": "^[0-9]{6}$",
    "accountNumberFormat": "^[0-9]{8}$"
  },
  "sapPaymentMethod": "BACS"
}
```

**Bank Validation Service Calls:**

**IBAN Validation:**

```
POST /api/bank/validate-iban
Content-Type: application/json

Request:
{
  "iban": "FR7630006000011234567890189",
  "country": "FR"
}

Response:
{
  "valid": true,
  "checksumValid": true,
  "bankCode": "30006",
  "branchCode": "00001",
  "accountNumber": "12345678901",
  "checkDigits": "89",
  "bankName": "BNP Paribas",
  "bic": "BNPAFRPPXXX"
}
```

**SWIFT/BIC Validation:**

```
POST /api/bank/validate-swift
Content-Type: application/json

Request:
{
  "swift": "BNPAFRPPXXX",
  "country": "FR"
}

Response:
{
  "valid": true,
  "bankCode": "BNPA",
  "countryCode": "FR",
  "locationCode": "PP",
  "branchCode": "XXX",
  "bankName": "BNP Paribas",
  "city": "Paris",
  "active": true
}
```

**Bank Duplicate Check:**

```
POST /api/bank/check-duplicate
Content-Type: application/json

Request:
{
  "vendorName": "TESTUSER Analysis",
  "country": "FR",
  "iban": "FR7630006000011234567890189",
  "accountNumber": "12345678901",
  "companyCode": "UNES"
}

Response:
{
  "duplicateFound": true,
  "matchType": "IBAN_EXACT",
  "existingVendors": [
    {
      "sapId": "10198765",
      "vendorName": "TESTUSER Analysis",
      "iban": "FR7630006000011234567890189",
      "accountGroup": "INDV",
      "status": "Active"
    }
  ],
  "message": "Vendor with identical IBAN already exists",
  "allowProceed": false
}
```

---

[**PART 6: BANK VALIDATION ARCHITECTURE - COMPLETE ANALYSIS**](https://www.notion.so/PART-6-BANK-VALIDATION-ARCHITECTURE-COMPLETE-ANALYSIS-2ddf4a4e989f8018a5f1e46a9bfa8e28?pvs=21)