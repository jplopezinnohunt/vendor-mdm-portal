// TypeScript interfaces for vendor personal details enhancements

/**
 * Address details - supports extended SAP address structure
 */
export interface AddressDetails {
    houseNo?: string;
    streetName?: string;
    streetName2?: string;
    streetName3?: string;
    streetName4?: string;
    postalCode?: string;
    city?: string;
    country?: string;
}

/**
 * Contact information - extended with payment email and fax
 */
export interface ContactDetails {
    phone?: string;
    mobilePhone?: string;
    fax?: string;
    email?: string;
    paymentEmail?: string;
}

/**
 * Attachment metadata - references to Azure Blob Storage
 */
export interface AttachmentMetadata {
    fileName: string;
    blobName: string;
    contentType: string;
    sizeBytes: number;
    uploadedAt: string;
    category: string;
    uploadedBy?: string;
    scanStatus?: 'pending_scan' | 'clean' | 'infected';
}

/**
 * Vendor-type-specific "Other Details" fields
 */
export interface OtherDetails {
    // Physical & Participant
    emergencyContactName?: string;
    emergencyContactPhone?: string;

    // Company & Meeting
    websiteUrl?: string;
    linkedinProfile?: string;
    employeeCount?: number;

    // Meeting specific
    eventCapacity?: number;
    specialEquipment?: string;

    // Participant specific
    dietaryRestrictions?: string;
    accessibilityRequirements?: string;
}

/**
 * Extended vendor form attributes
 */
export interface VendorApplicationAttributes {
    vendorType?: string;
    accountGroup?: string;
    address?: AddressDetails;
    contactInfo?: ContactDetails;
    attachments?: AttachmentMetadata[];
    otherDetails?: OtherDetails;
    [key: string]: any; // Allow additional properties
}

/**
 * Full vendor form data
 */
export interface VendorFormData {
    // Canonical fields
    legalName?: string;
    taxId?: string;
    primaryContactEmail?: string;

    // Form fields
    name?: string;
    givenName?: string;
    familyName?: string;
    dateOfBirth?: string;
    country?: string;
    vendorType?: string;
    accountGroup?: string;
    companyCode?: string;

    // Extended attributes
    attributes: VendorApplicationAttributes;
}
