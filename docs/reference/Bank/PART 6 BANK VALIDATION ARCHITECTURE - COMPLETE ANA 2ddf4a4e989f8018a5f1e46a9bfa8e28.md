# PART 6: BANK VALIDATION ARCHITECTURE - COMPLETE ANALYSIS

*[This section contains the complete bank validation analysis already provided earlier - including all country configurations for SEPA, US, Argentina, UK, etc. - with the country selection service, field unlocking mechanism, IBAN validation, SWIFT validation, and bank duplicate check services]*

### 6.1 Bank Country Selection Mechanism

**Initial State:** All bank fields are readonly/disabled except the Country dropdown

**Critical Validation Message:**

```
ⓘ First choose bank country to unlock other fields
```

### 6.2 Country-Dependent Field Configuration Service

**Service Endpoint:**

```
POST /api/bank/configuration
Content-Type: application/json

Request:
{
  "countryCode": "FR",
  "companyCode": "UNES",
  "vendorType": "INDV"
}
```

### 6.3 Complete Country Configurations

**France (SEPA Country):**

```
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
    "swiftFormat": "^[A-Z]{6}[A-Z0-9]{2}([A-Z0-9]{3})?$"
  },
  "sapMapping": {
    "primaryBankKey": "IBAN",
    "bankCountry": "FR",
    "paymentMethod": "SEPA_DD"
  }
}
```

**Germany (SEPA with Control Key):**

```
{
  "countryCode": "DE",
  "region": "SEPA",
  "fieldConfiguration": {
    "showIBAN": true,
    "showControlKey": true,  // UNIQUE to Germany/Austria
    "showAccountNumber": true,
    "ibanMandatory": true,
    "controlKeyMandatory": true
  },
  "validationRules": {
    "ibanFormat": "DE[0-9]{20}",
    "ibanLength": 22,
    "controlKeyValues": ["01", "02", "03", "21", "51", "99"]
  }
}
```

**United States:**

```
{
  "countryCode": "US",
  "region": "North America",
  "fieldConfiguration": {
    "showIBAN": false,
    "showAccountNumber": true,
    "showBankNumber": true,  // ABA Routing Number
    "showSwiftBIC": true,
    "accountNumberMandatory": true,
    "routingNumberMandatory": true
  },
  "validationRules": {
    "routingNumberFormat": "^[0-9]{9}$",
    "routingNumberChecksum": "ABA algorithm",
    "accountNumberFormat": "Variable length"
  },
  "sapMapping": {
    "primaryBankKey": "RoutingNumber",
    "paymentMethod": "ACH"
  }
}
```

**Argentina:**

```
{
  "countryCode": "AR",
  "region": "Latin America",
  "fieldConfiguration": {
    "showIBAN": false,
    "showCBU": true,  // 22-digit CBU
    "showBankCode": true,
    "showBranchCode": true,
    "showAccountNumber": true,
    "cbuMandatory": true
  },
  "validationRules": {
    "cbuFormat": "^[0-9]{22}$",
    "cbuChecksum": "Verhoeff algorithm",
    "bankCodeFormat": "^[0-9]{3}$",
    "branchCodeFormat": "^[0-9]{4}$"
  }
}
```

### 6.4 IBAN Validation Service

```
POST /api/bank/validate-iban
Content-Type: application/json

Request:
{
  "iban": "FR7630006000011234567890189",
  "country": "FR"
}

Response: 200 OK
{
  "valid": true,
  "checksumValid": true,
  "bankCode": "30006",
  "branchCode": "00001",
  "accountNumber": "12345678901",
  "checkDigits": "89",
  "bankName": "BNP Paribas",
  "bic": "BNPAFRPPXXX",
  "country": "FR"
}
```

**IBAN Checksum Algorithm (MOD-97):**

```
public bool ValidateIBANChecksum(string iban)
{
    // Step 1: Move first 4 characters to end
    var rearranged = iban.Substring(4) + iban.Substring(0, 4);

    // Step 2: Replace letters with numbers (A=10, B=11, ..., Z=35)
    var numeric = string.Join("", rearranged.Select(

---

# continue without stop

c => char.IsLetter(c) ? (c - 'A' + 10).ToString() : c.ToString()));

    // Step 3: Calculate MOD 97
    var remainder = BigInteger.Parse(numeric) % 97;

    // Step 4: Check if remainder is 1
    return remainder == 1;
}
```

### 6.5 SWIFT/BIC Validation Service

```
POST /api/bank/validate-swift
Content-Type: application/json

Request:
{
  "swift": "BNPAFRPPXXX",
  "country": "FR"
}

Response: 200 OK
{
  "valid": true,
  "bankCode": "BNPA",
  "countryCode": "FR",
  "locationCode": "PP",
  "branchCode": "XXX",
  "bankName": "BNP Paribas",
  "bankAddress": "16 Boulevard des Italiens, 75009 Paris",
  "city": "Paris",
  "active": true,
  "iso9362Compliant": true
}
```

### 6.6 Bank Duplicate Check Service

```
POST /api/bank/check-duplicate
Content-Type: application/json

Request:
{
  "vendorName": "TESTUSER Analysis",
  "country": "FR",
  "iban": "FR7630006000011234567890189",
  "accountNumber": "12345678901",
  "swift": "BNPAFRPPXXX",
  "companyCode": "UNES"
}

Response: 200 OK (No Duplicate)
{
  "duplicateFound": false,
  "message": "No duplicate bank accounts found",
  "checkPerformed": [
    "IBAN exact match",
    "AccountNumber + SWIFT combination",
    "VendorName + BankAccount combination"
  ]
}

Response: 409 Conflict (Duplicate Found)
{
  "duplicateFound": true,
  "matchType": "IBAN_EXACT",
  "existingVendors": [
    {
      "sapId": "10198765",
      "vendorName": "TESTUSER Analysis",
      "iban": "FR7630006000011234567890189",
      "accountGroup": "INDV",
      "status": "Active",
      "companyCode": "UNES"
    }
  ],
  "message": "Vendor with identical IBAN already exists in SAP",
  "allowProceed": false,
  "recommendation": "Use existing vendor or contact Vendor Unit"
}
```

### 6.7 SAP Bank Data Table Mapping (LFBK)

**MoUV Field → SAP Field Mapping:**

| MoUV Field | SAP Table | SAP Field | Data Type | Length | Description |
| --- | --- | --- | --- | --- | --- |
| Bank Country | LFBK | BANKS | CHAR | 3 | Bank country key |
| Bank Key | LFBK | BANKL | CHAR | 15 | Bank key (routing number) |
| Account Number | LFBK | BANKN | CHAR | 18 | Bank account number |
| IBAN | LFBK | IBAN | CHAR | 34 | IBAN |
| SWIFT/BIC | LFBK | SWIFT | CHAR | 11 | SWIFT code |
| Control Key | LFBK | BKONT | CHAR | 2 | Bank control key |
| Account Currency | LFBK | WAERS | CUKY | 5 | Currency key |
| Account Holder | LFBK | KOINH | CHAR | 60 | Account holder name |