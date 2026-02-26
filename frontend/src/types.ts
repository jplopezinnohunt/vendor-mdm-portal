
// Enums matching the Azure Architecture / PDF
export enum ApplicationStatus {
  Draft = 'Draft',
  Submitted = 'Submitted',
  PendingReview = 'PendingReview',
  Approved = 'Approved',
  Rejected = 'Rejected',
}

export enum ChangeRequestStatus {
  Draft = 'Draft',
  Submitted = 'Submitted',
  UnderReview = 'UnderReview',
  InformationRequested = 'InformationRequested',
  Approved = 'Approved',
  Rejected = 'Rejected',
  Cancelled = 'Cancelled',
  Expired = 'Expired',
  Applied = 'Applied',
  Error = 'Error',
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

  // Expanded UNESCO Fields
  companyCode?: string;
  accountGroup?: string;
  contactPerson?: string;
  contactEmail?: string;
  contactPhone?: string;

  // New Fields
  birthDate?: string;
  gender?: string;
  profession?: string;
  birthCountry?: string;
  events?: { name: string, date: string }[];
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
  attributes?: any; // Dynamic attributes from JSON
  registrationType?: string; // Invitation, SelfRegistration
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


// Form DTOs
export interface VendorProfileFormData {
  name: string;
  email: string;
  street: string;
  city: string;
  postalCode: string;
  country: string;
  taxNumber1: string;

  // Bank Data
  bankCountry: string;
  bankAccount: string;
  bankKey: string;
  iban: string;
  swift: string; // SWIFT/BIC

  // New Spec Fields
  companyCode: string;
  contactPerson: string;
  contactPhone: string;
  accountGroup: string;

  // Individual Fields
  birthDate?: string;
  gender?: string;
  profession?: string;
  birthCountry?: string;

  // Event Fields
  eventDate?: string;

  // Bank Specific
  controlKey?: string;
}
