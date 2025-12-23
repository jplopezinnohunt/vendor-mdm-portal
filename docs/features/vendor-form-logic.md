# Vendor Form Logic & Dynamic Rendering
## Overview
The vendor management portal utilizes a centralized **Dynamic Form Engine** (`DynamicFormHelper.tsx`) to render vendor profile forms. This engine ensures consistency across different workflows (Invitation, Registration, Change Requests) and adapts the UI based on the **Vendor Type** and **Flow Context**.

## Core Components
*   **`DynamicFormHelper.tsx`**: The main component that dictates which fields are shown, hidden, optional, or mandatory.
*   **`ChangeRequestForm.tsx`**: The parent container that manages form state (using `react-hook-form`) and submits data to the backend.
*   **`types.ts`**: Defines the `VendorProfileFormData` structure, including all possible fields across vendor types.

## Vendor Types & Field Rules
The form adapts to the `accountGroup` (Vendor Type):

### 1. Individual (INDV)
*   **Purpose**: Physical persons, consultants, experts.
*   **General Section**:
    *   **Full Name**: Combined Family Name & Given Name.
    *   **Personal Details**: Gender, Date of Birth, Country of Birth, Profession (Mandatory).
    *   **Identification**: ID Document upload required (PDF/JPG).
*   **Address**: Full address required (Street, City, Postal Code, Country).

### 2. Company / Organization (HQSU, NGOS, INSO)
*   **Purpose**: Legal entities, NGOs, Institutes.
*   **General Section**:
    *   **Name**: Company / Organization Name.
    *   **Personal Details**: Hidden (DoB, Gender, etc. are not applicable).
    *   **Identification**: Registration Certificate upload required.
*   **Address**: Full address required.

### 3. Event / Conference (EVNT, CONF)
*   **Purpose**: One-time venues, catering, conference centers.
*   **General Section**:
    *   **Name**: Event Name.
    *   **Event Details**: Event Date (Mandatory).
*   **Address**: Full address required (Venue location).

### 4. Participant (PART)
*   **Purpose**: Workshop attendees, trainees (Bank transfer only).
*   **General Section**: Minimal info (Name, Email). ID Documents NOT required.
*   **Address**: **Simplified**. ONLY Country is required. Street/City hidden.
*   **Bank**: **Mandatory**. Strict validation applied.

## Bank Validation Logic
The `DynamicFormHelper` implements strict ISO and country-specific rules for bank account data. The form observes the `Bank Country` selection (`watch('bankCountry')`) and adjusts fields dynamically.

| Country | Code | Fields Visible | Labels / Notes |
| :--- | :--- | :--- | :--- |
| **France** | FR | IBAN, Swift, Account Number | Standard SEPA. IBAN is 27 chars. |
| **Germany** | DE | IBAN, Swift, Account Number, **Control Key** | Control Key is mandatory for DE. |
| **USA** | US | Bank Key, Account Number, Swift | **Bank Key** labeled "ABA Routing Number". No IBAN. |
| **Argentina** | AR | IBAN (as CBU), Bank Key, Swift | **IBAN** field used for CBU (22 digits). **Bank Key** labeled "Bank Code". |
| **United Kingdom**| GB | IBAN, Swift, Sort Code | (Planned) Sort Code validation. |
| **Default** | * | IBAN, Swift, Bank Key, Account Number | Generic fallback. |

## Flow Context Logic
The form behavior changes based on who is viewing it (`flowType`):

1.  **INVITATION**: Vendor is registering for the first time. All fields editable.
2.  **CHANGE_VENDOR**: Existing vendor initiating a change.
    *   **Restricted Fields**: Name, Tax ID are read-only (Master Data Team must change these).
    *   **Sensitive Fields**: Bank data changes trigger specific warnings.
3.  **CHANGE_INTERNAL**: Internal staff (Approver/Admin) editing.
    *   **Full Access**: Can edit all fields including Name and Tax ID.

## Technical Implementation Details
*   **State Management**: `react-hook-form` is used for value tracking and validation.
*   **Reactive Logic**: The `watch` function is passed to `DynamicFormSection` to allow real-time toggling of bank fields.
*   **Types**: `VendorProfileFormData` in `types.ts` is the superset of all fields.

### Example: Bank Config Object
```typescript
const CountryBankConfigs = {
    'US': {
        showIBAN: false,
        showBankNumber: true,
        bankNumberLabel: "ABA Routing Number",
        // ...
    }
};
```
