import React from 'react';
import { UseFormRegister, FieldErrors } from 'react-hook-form';
import { VendorProfileFormData } from '../types';
import { Input, Card } from './ui/Elements';

interface DynamicFormSectionProps {
    section: string;
    register: UseFormRegister<VendorProfileFormData>;
    errors: FieldErrors<VendorProfileFormData>;
    watch?: any; // Using any for simplicity with complex generic types, or UseFormWatch<VendorProfileFormData> if imported
    vendorType: string; // 'INDV', 'HQSU', 'EVNT', etc.
    flowType: 'INVITATION' | 'CHANGE_VENDOR' | 'CHANGE_INTERNAL';
    originalData?: VendorProfileFormData;
}

// Bank Configuration Types
interface BankFieldConfig {
    showIBAN: boolean;
    showControlKey: boolean;
    showBankNumber: boolean; // Routing Number / Bank Code
    showSwiftBIC: boolean;
    showAccountNumber: boolean;
    accountNumberLabel?: string;
    bankNumberLabel?: string; // "Routing Number", "Bank Key", "Bank Code"
    ibanLabel?: string; // "IBAN", "CBU"
}

const CountryBankConfigs: Record<string, BankFieldConfig> = {
    // France (SEPA)
    'FR': {
        showIBAN: true,
        showControlKey: false,
        showBankNumber: false,
        showSwiftBIC: true,
        showAccountNumber: true,
        ibanLabel: "IBAN (27 chars)",
        accountNumberLabel: "Account Number"
    },
    // Germany (SEPA with Control Key)
    'DE': {
        showIBAN: true,
        showControlKey: true, // Unique
        showBankNumber: false,
        showSwiftBIC: true,
        showAccountNumber: true,
        ibanLabel: "IBAN (22 chars)",
        accountNumberLabel: "Account Number"
    },
    // United States
    'US': {
        showIBAN: false,
        showControlKey: false,
        showBankNumber: true,
        showSwiftBIC: true,
        showAccountNumber: true,
        bankNumberLabel: "ABA Routing Number (9 digits)",
        accountNumberLabel: "Account Number"
    },
    // Argentina
    'AR': {
        showIBAN: false, // Uses CBU instead
        showControlKey: false,
        showBankNumber: true,
        showSwiftBIC: true, // Optional usually but good for intl
        showAccountNumber: true,
        bankNumberLabel: "Bank Code (3 digits)",
        ibanLabel: "CBU (22 digits)" // Using IBAN field for CBU as they are fulfilling similar roles in UI
    },
    // Default / fallback
    'DEFAULT': {
        showIBAN: true,
        showControlKey: false,
        showBankNumber: true,
        showSwiftBIC: true,
        showAccountNumber: true,
        bankNumberLabel: "Bank Key",
        ibanLabel: "IBAN",
        accountNumberLabel: "Account Number"
    }
};

export const DynamicFormSection: React.FC<DynamicFormSectionProps> = ({
    section,
    register,
    errors,
    watch,
    vendorType,
    flowType,
    originalData
}) => {

    // Rule Engine: Determines field visibility and editability
    const isEditable = (field: string) => {
        // Internal changes (Approver) can edit everything
        if (flowType === 'CHANGE_INTERNAL') return true;

        // Invitation: All fields editable
        if (flowType === 'INVITATION') return true;

        // Vendor Change: Basic fields restricted if they affect Tax/Identity
        if (flowType === 'CHANGE_VENDOR') {
            if (field === 'companyName' || field === 'taxId') return false;
        }
        return true;
    };

    // 1. General Data Section
    if (section === 'General') {
        const isIndividual = ['INDV', 'SCSA', 'FELL'].includes(vendorType);
        const isParticipant = vendorType === 'PART';
        const isEvent = ['EVNT', 'CONF'].includes(vendorType);

        return (
            <Card title="General Information">
                <div className="grid grid-cols-1 gap-y-6 gap-x-4 sm:grid-cols-6">
                    {/* Name Field - Label changes based on type */}
                    <div className="sm:col-span-4">
                        <Input
                            label={isIndividual ? "Full Name (Family Name, Given Name)" : isEvent ? "Event Name" : "Company / Organization Name"}
                            disabled={!isEditable('companyName')}
                            {...register('name', { required: 'Name is required' })}
                            error={errors.name?.message}
                        />
                    </div>

                    <div className="sm:col-span-2">
                        <Input
                            label="Account Group"
                            {...register('accountGroup')}
                            disabled={true}
                            className="bg-gray-50"
                        />
                    </div>

                    <div className="sm:col-span-3">
                        <Input
                            label="Email Address"
                            type="email"
                            {...register('email', { required: true })}
                        />
                    </div>

                    {/* Individual Specific Fields */}
                    {isIndividual && (
                        <>
                            <div className="sm:col-span-2">
                                <Input label="Date of Birth" placeholder="YYYY-MM-DD" {...register('birthDate')} />
                            </div>
                            <div className="sm:col-span-2">
                                <Input label="Gender" placeholder="Mr/Ms/Dr" {...register('gender')} />
                            </div>
                            <div className="sm:col-span-2">
                                <Input label="Profession" {...register('profession')} />
                            </div>
                            <div className="sm:col-span-3">
                                <Input label="Country of Birth" maxLength={2} placeholder="FR" {...register('birthCountry')} />
                            </div>
                        </>
                    )}

                    {/* Event Specific Fields */}
                    {isEvent && (
                        <div className="sm:col-span-3">
                            <Input label="Event Date" type="date" {...register('eventDate')} />
                        </div>
                    )}
                </div>
            </Card>
        );
    }

    // 2. Address Section
    if (section === 'Address') {
        const isEvent = ['EVNT', 'CONF'].includes(vendorType);
        const isParticipant = vendorType === 'PART';

        // Partial address for Event (Venue city/country important) or Participant (Country only)

        return (
            <Card title="Address Details">
                <div className="grid grid-cols-1 gap-y-6 gap-x-4 sm:grid-cols-6">
                    {!isParticipant && (
                        <div className="sm:col-span-6">
                            <Input label="Street" {...register('street')} />
                        </div>
                    )}

                    {!isParticipant && (
                        <div className="sm:col-span-3">
                            <Input label="City" {...register('city')} />
                        </div>
                    )}

                    {!isParticipant && (
                        <div className="sm:col-span-3">
                            <Input label="Postal Code" {...register('postalCode')} />
                        </div>
                    )}

                    <div className="sm:col-span-3">
                        <Input label="Country" {...register('country')} maxLength={2} placeholder="ISO 2-char code (e.g. FR)" />
                    </div>
                </div>
            </Card>
        );
    }

    // 3. Bank Section (Always Sensitive)
    if (section === 'Bank') {
        const bankCountry = watch ? watch('bankCountry') : (originalData?.bankCountry || '');
        const config = CountryBankConfigs[bankCountry] || CountryBankConfigs['DEFAULT'];

        // Special handling if using IBAN field for CBU (Argentina)
        const isArgentina = bankCountry === 'AR';

        return (
            <Card title="Bank Information (Sensitive)" className="border-l-4 border-l-orange-400">
                <div className="p-2 mb-4 bg-orange-50 text-orange-800 text-sm rounded">
                    Strict validation applied. Changes require proof documents.
                    {!bankCountry && <div className="font-bold mt-1">ⓘ Select a Bank Country to see specific fields.</div>}
                </div>
                <div className="grid grid-cols-1 gap-y-6 gap-x-4 sm:grid-cols-6">
                    <div className="sm:col-span-2">
                        <Input
                            label="Bank Country"
                            {...register('bankCountry', { required: 'Bank Country is required' })}
                            placeholder="e.g. FR, US, DE"
                            maxLength={2}
                            error={errors.bankCountry?.message}
                        />
                    </div>

                    {/* Dynamic Fields based on Country Config */}

                    {/* Bank Key / Routing Number / Bank Code */}
                    {config.showBankNumber && (
                        <div className="sm:col-span-2">
                            <Input
                                label={config.bankNumberLabel || "Bank Key"}
                                {...register('bankKey', { required: config.showBankNumber })}
                                error={errors.bankKey?.message}
                            />
                        </div>
                    )}

                    {/* Control Key (Germany only) */}
                    {config.showControlKey && (
                        <div className="sm:col-span-2">
                            <Input
                                label="Control Key"
                                {...register('controlKey')}
                                maxLength={2}
                            />
                        </div>
                    )}

                    {/* IBAN or CBU */}
                    {(config.showIBAN || isArgentina) && (
                        <div className="sm:col-span-4">
                            <Input
                                label={config.ibanLabel || "IBAN"}
                                {...register('iban', { required: true })}
                                placeholder={isArgentina ? "22 digits" : "FR76..."}
                                error={errors.iban?.message}
                            />
                        </div>
                    )}

                    {/* Account Number (if not covered by IBAN/CBU or required separately) */}
                    {config.showAccountNumber && (
                        <div className="sm:col-span-3">
                            <Input
                                label={config.accountNumberLabel || "Account Number"}
                                {...register('bankAccount', { required: config.showAccountNumber })}
                                error={errors.bankAccount?.message}
                            />
                        </div>
                    )}

                    {/* SWIFT / BIC */}
                    {config.showSwiftBIC && (
                        <div className="sm:col-span-3">
                            <Input
                                label="SWIFT / BIC"
                                {...register('swift', { required: config.showSwiftBIC })}
                                error={errors.swift?.message}
                            />
                        </div>
                    )}
                </div>
            </Card>
        );
    }

    return null;
};
