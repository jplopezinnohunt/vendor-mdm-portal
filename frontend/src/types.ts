
// Enums matching the Azure Architecture / PDF
export enum ApplicationStatus {
  Draft = 'Draft',
  Submitted = 'Submitted',
  Approved = 'Approved',
  Rejected = 'Rejected',
}

export enum ChangeRequestStatus {
  New = 'NEW',
  InReview = 'IN_REVIEW',
  Approved = 'APPROVED',
  Rejected = 'REJECTED',
  Applied = 'APPLIED',
  Error = 'ERROR',
}

export enum RequestType {
  BankData = 'BANK_DATA',
  Address = 'ADDRESS',
  Tax = 'TAX',
  General = 'GENERAL',
}

// Entity: VendorMasterData (Mapped from ECC BAPI_VENDOR_GETDETAIL)
export interface VendorAddress {
  street: string;
  city: string;
  postalCode: string;
  country: string; // ISO Code
  region?: string;
}

export interface VendorBank {
  id: string; // Internal mapping ID
  bankCountry: string; // BANKS
  bankKey: string; // BANKL
  bankAccount: string; // BANKN
  accountHolder: string; // KOINH
  iban: string;
}

export interface VendorMasterData {
  sapVendorId: string; // LIFNR
  name: string; // NAME1
  legalForm?: string;
  taxNumber1?: string; // STCD1
  taxNumber2?: string; // STCD2
  address: VendorAddress;
  banks: VendorBank[];
  email: string;
  phone: string;
}

// Entity: ChangeRequest
export interface ChangeRequest {
  id: string; // UUID
  vendorId: string;
  requestType: RequestType;
  status: ChangeRequestStatus;
  createdAt: string;
  updatedAt: string;
  items: ChangeRequestItem[];
  attachments: Attachment[];
}

// Entity: ChangeRequestItem (The Delta)
export interface ChangeRequestItem {
  id: string; // UUID
  tableName: string; // LFA1, LFBK, etc.
  fieldName: string; // NAME1, STRAS, etc.
  oldValue: string;
  newValue: string;
  subKey1?: string; // e.g., Company Code or Bank Country
  isSensitive: boolean;
}

// Entity: VendorApplication (Onboarding)
export interface VendorApplication {
  id: string;
  companyName: string;
  taxId: string;
  contactName: string;
  email: string;
  status: ApplicationStatus;
  submittedAt: string;
  sanctionCheckStatus?: 'Passed' | 'Failed' | 'Pending'; // Automated check status
}

// Entity: Attachment
export interface Attachment {
  id: string;
  fileName: string;
  mimeType: string;
  uploadedAt: string;
  category: 'BANK_LETTER' | 'TAX_CERTIFICATE' | 'OTHER';
}

// Form DTOs
export interface VendorProfileFormData {
  name: string;
  email: string;
  street: string;
  city: string;
  postalCode: string;
  country: string;
  taxNumber1: string;
  bankAccount: string;
  bankKey: string;
  iban: string;
}
