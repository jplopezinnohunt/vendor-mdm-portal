import React, { useState, useEffect } from 'react';
import { UseFormRegister, FieldErrors, UseFormSetValue } from 'react-hook-form';
import { Input } from '../components/ui/Elements';
import { VendorFormData } from '../types/vendor';
import { Info, Landmark } from 'lucide-react';
import { validateIBAN, validateSWIFT } from '../utils/bankValidation';

// Updated: 2026-01-05 12:55 - Full SAP integration with all fields

interface BankInformationFormProps {
    register: UseFormRegister<VendorFormData>;
    errors: FieldErrors<VendorFormData>;
    setValue: UseFormSetValue<VendorFormData>;
}

interface BankCountryConfig {
    countryCode: string;
    countryName: string;
    region: string;
    showIBAN: boolean;
    showControlKey: boolean;
    showBankNumber: boolean;
    showSwiftBIC: boolean;
    showAccountNumber: boolean;
    ibanMandatory: boolean;
    swiftMandatory: boolean;
    accountNumberMandatory: boolean;
    controlKeyMandatory: boolean;
    bankNumberMandatory: boolean;
    controlKeyValues?: string[];
    bankNumberLabel?: string;
    primaryBankKey: string;
    paymentMethods: string[];
}

export const BankInformationForm: React.FC<BankInformationFormProps> = ({
    register,
    errors,
    setValue
}) => {
    const [selectedCountry, setSelectedCountry] = useState('');
    const [config, setConfig] = useState<BankCountryConfig | null>(null);

    const countries = [
        { code: 'FR', name: 'France' },
        { code: 'DE', name: 'Germany' },
        { code: 'AT', name: 'Austria' },
        { code: 'ES', name: 'Spain' },
        { code: 'IT', name: 'Italy' },
        { code: 'NL', name: 'Netherlands' },
        { code: 'BE', name: 'Belgium' },
        { code: 'PT', name: 'Portugal' },
        { code: 'GB', name: 'United Kingdom' },
        { code: 'CH', name: 'Switzerland' },
        { code: 'US', name: 'United States' },
        { code: 'CA', name: 'Canada' },
        { code: 'AR', name: 'Argentina' },
        { code: 'BR', name: 'Brazil' },
        { code: 'MX', name: 'Mexico' },
    ];

    useEffect(() => {
        if (selectedCountry) {
            fetchBankConfiguration(selectedCountry);
        } else {
            setConfig(null);
        }
    }, [selectedCountry]);

    const fetchBankConfiguration = async (countryCode: string) => {
        try {
            const response = await fetch('/api/bank/configuration', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    countryCode,
                    companyCode: 'UNES'
                })
            });

            if (response.ok) {
                const configData = await response.json();
                setConfig(configData);
                console.log('✓ SAP Configuration loaded:', configData);
            }
        } catch (error) {
            console.error('Error fetching bank configuration:', error);
        }
    };

    const handleCountryChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const country = e.target.value;
        setSelectedCountry(country);
        setValue('attributes.bankCountry', country);
    };

    const handleIBANBlur = (e: React.FocusEvent<HTMLInputElement>) => {
        const iban = e.target.value;
        if (iban && selectedCountry) {
            const result = validateIBAN(iban);
            if (result.valid && result.electronic) {
                setValue('attributes.iban', result.electronic);
                console.log('✓ IBAN valid');
            }
        }
    };

    const handleSWIFTBlur = (e: React.FocusEvent<HTMLInputElement>) => {
        const swift = e.target.value;
        if (swift && selectedCountry) {
            const result = validateSWIFT(swift);
            if (result.valid) {
                console.log('✓ SWIFT valid');
            }
        }
    };

    return (
        <div className="space-y-4">
            {/* CRITICAL INFO BANNER */}
            <div className="bg-blue-50 border border-blue-200 p-3 rounded-lg flex items-start gap-3">
                <Info className="h-4 w-4 text-blue-600 mt-0.5 flex-shrink-0" />
                <div className="text-xs text-blue-800">
                    <strong>UNESCO Requirement:</strong> Bank information fields are dynamically adjusted based on the bank's country. Please select the country first.
                </div>
            </div>

            {/* BANK GROUP FIELDS - Compact Grid */}
            <div className="grid grid-cols-12 gap-4">

                {/* Row 1: Country (4) + City/Postal (4) + Bank Agency (4) */}
                {/* Moved Country first as it drives logic */}
                <div className="col-span-12 md:col-span-4">
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                        Country *
                    </label>
                    <select
                        className="block w-full rounded-md border-gray-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm px-3 py-2"
                        onChange={handleCountryChange}
                        value={selectedCountry}
                    >
                        <option value="">-- Select Country --</option>
                        {countries.map(c => (
                            <option key={c.code} value={c.code}>{c.name}</option>
                        ))}
                    </select>
                </div>

                <div className="col-span-12 md:col-span-4">
                    <Input
                        label="City and Postal Code *"
                        {...register('attributes.cityPostalCode', {
                            required: selectedCountry ? 'City and postal code are required' : false
                        })}
                        error={errors.attributes?.cityPostalCode?.message as string}
                        readOnly={!selectedCountry}
                        style={!selectedCountry ? { backgroundColor: '#f3f4f6', cursor: 'not-allowed' } : {}}
                    />
                </div>

                <div className="col-span-12 md:col-span-4">
                    <Input
                        label="Bank agency *"
                        {...register('attributes.bankAgency', {
                            required: selectedCountry ? 'Bank agency is required' : false
                        })}
                        error={errors.attributes?.bankAgency?.message as string}
                        readOnly={!selectedCountry}
                        style={!selectedCountry ? { backgroundColor: '#f3f4f6', cursor: 'not-allowed' } : {}}
                    />
                </div>

                {/* Row 2: Name (8) + Abbreviation (4) */}
                <div className="col-span-12 md:col-span-8">
                    <Input
                        label="Name *"
                        {...register('attributes.bankName', {
                            required: selectedCountry ? 'Bank name is required' : false
                        })}
                        error={errors.attributes?.bankName?.message as string}
                        readOnly={!selectedCountry}
                        style={!selectedCountry ? { backgroundColor: '#f3f4f6', cursor: 'not-allowed' } : {}}
                    />
                </div>

                <div className="col-span-12 md:col-span-4">
                    <Input
                        label="Abbreviation"
                        {...register('attributes.abbreviation')}
                        readOnly={!selectedCountry}
                        style={!selectedCountry ? { backgroundColor: '#f3f4f6', cursor: 'not-allowed' } : {}}
                    />
                </div>

                {/* Row 3: Agency Address (Full) */}
                <div className="col-span-12">
                    <Input
                        label="Agency Address *"
                        {...register('attributes.agencyAddress', {
                            required: selectedCountry ? 'Agency address is required' : false
                        })}
                        error={errors.attributes?.agencyAddress?.message as string}
                        readOnly={!selectedCountry}
                        style={!selectedCountry ? { backgroundColor: '#f3f4f6', cursor: 'not-allowed' } : {}}
                    />
                </div>

                {/* Row 4: Account Holder (8) + Currency (4) */}
                <div className="col-span-12 md:col-span-8">
                    <Input
                        label="Account Holder Name *"
                        {...register('attributes.accountHolderName', {
                            required: selectedCountry ? 'Account holder is required' : false
                        })}
                        error={errors.attributes?.accountHolderName?.message as string}
                        readOnly={!selectedCountry}
                        style={!selectedCountry ? { backgroundColor: '#f3f4f6', cursor: 'not-allowed' } : {}}
                    />
                </div>

                <div className="col-span-12 md:col-span-4">
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                        Account Currency *
                    </label>
                    <select
                        className="block w-full rounded-md border-gray-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm px-3 py-2"
                        {...register('attributes.currency', {
                            required: selectedCountry ? 'Currency is required' : false
                        })}
                        disabled={!selectedCountry}
                        style={!selectedCountry ? { backgroundColor: '#f3f4f6', cursor: 'not-allowed' } : {}}
                    >
                        <option value="">-- Select --</option>
                        <option value="EUR">EUR - Euro</option>
                        <option value="USD">USD - US Dollar</option>
                        <option value="GBP">GBP - British Pound</option>
                        <option value="CHF">CHF - Swiss Franc</option>
                        <option value="ARS">ARS - Argentine Peso</option>
                        <option value="BRL">BRL - Brazilian Real</option>
                        <option value="MXN">MXN - Mexican Peso</option>
                        <option value="CAD">CAD - Canadian Dollar</option>
                    </select>
                    {errors.attributes?.currency && (
                        <p className="mt-1 text-sm text-red-600">{errors.attributes.currency.message as string}</p>
                    )}
                </div>

                {/* Row 5: Additional Info (Full) */}
                <div className="col-span-12">
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                        Additional bank information
                    </label>
                    <textarea
                        {...register('attributes.additionalInfo')}
                        className="block w-full rounded-md border-gray-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm px-3 py-2"
                        rows={2}
                        readOnly={!selectedCountry}
                        style={!selectedCountry ? { backgroundColor: '#f3f4f6', cursor: 'not-allowed' } : {}}
                    />
                </div>
            </div>

            {/* ACCOUNT GROUP - Compact Subsection */}
            <div className="border-t border-gray-300 pt-4 mt-4">
                <h3 className="text-sm font-bold text-gray-700 uppercase tracking-wide mb-3 flex items-center gap-2">
                    <Landmark className="h-4 w-4" />
                    Account Identification
                </h3>

                <div className="grid grid-cols-12 gap-4">

                    {/* Row 1: Bank/Branch (5) + Account Number (7) */}
                    <div className="col-span-12 md:col-span-5">
                        <Input
                            label={`${config?.bankNumberLabel || 'Bank number / Branch code'} ${config?.bankNumberMandatory ? '*' : ''}`}
                            {...register('attributes.bankNumber', {
                                required: (selectedCountry && config?.bankNumberMandatory) ? `${config?.bankNumberLabel || 'Bank number'} is required` : false
                            })}
                            error={errors.attributes?.bankNumber?.message as string}
                            readOnly={!selectedCountry || !config?.showBankNumber}
                            style={(!selectedCountry || !config?.showBankNumber) ? { backgroundColor: '#f3f4f6', cursor: 'not-allowed' } : {}}
                            placeholder={selectedCountry && config?.showBankNumber ? 'e.g., 021000021' : ''}
                        />
                    </div>

                    <div className="col-span-12 md:col-span-7">
                        <Input
                            label={`Account Number ${config?.accountNumberMandatory ? '*' : ''}`}
                            {...register('attributes.accountNumber', {
                                required: (selectedCountry && config?.accountNumberMandatory) ? 'Account number is required' : false
                            })}
                            error={errors.attributes?.accountNumber?.message as string}
                            readOnly={!selectedCountry || !config?.showAccountNumber}
                            style={(!selectedCountry || !config?.showAccountNumber) ? { backgroundColor: '#f3f4f6', cursor: 'not-allowed' } : {}}
                        />
                    </div>

                    {/* Row 2: IBAN (5) + SWIFT (4) + Control Key (3) */}
                    <div className="col-span-12 md:col-span-5">
                        <Input
                            label={`IBAN ${config?.ibanMandatory ? '*' : ''}`}
                            {...register('attributes.iban', {
                                required: (selectedCountry && config?.ibanMandatory) ? 'IBAN is required' : false,
                                validate: (value) => {
                                    if (value && selectedCountry && config?.showIBAN) {
                                        const result = validateIBAN(value);
                                        return result.valid || result.error || 'Invalid IBAN';
                                    }
                                    return true;
                                }
                            })}
                            error={errors.attributes?.iban?.message as string}
                            readOnly={!selectedCountry || !config?.showIBAN}
                            style={(!selectedCountry || !config?.showIBAN) ? { backgroundColor: '#f3f4f6', cursor: 'not-allowed' } : {}}
                            placeholder={selectedCountry && config?.showIBAN ? 'e.g., FR76...' : ''}
                        />
                    </div>

                    <div className="col-span-12 md:col-span-4">
                        <Input
                            label={`SWIFT/BIC Code ${config?.swiftMandatory ? '*' : ''}`}
                            {...register('attributes.swift', {
                                required: (selectedCountry && config?.swiftMandatory) ? 'SWIFT/BIC is required' : false,
                                validate: (value) => {
                                    if (value && selectedCountry) {
                                        const result = validateSWIFT(value);
                                        return result.valid || result.error || 'Invalid SWIFT/BIC';
                                    }
                                    return true;
                                }
                            })}
                            error={errors.attributes?.swift?.message as string}
                            readOnly={!selectedCountry || !config?.showSwiftBIC}
                            style={(!selectedCountry || !config?.showSwiftBIC) ? { backgroundColor: '#f3f4f6', cursor: 'not-allowed' } : {}}
                            placeholder={selectedCountry && config?.showSwiftBIC ? 'e.g., BNPA...' : ''}
                        />
                    </div>

                    <div className="col-span-12 md:col-span-3">
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                            Control Key {config?.controlKeyMandatory ? '*' : ''}
                        </label>
                        <select
                            className="block w-full rounded-md border-gray-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm px-3 py-2"
                            {...register('attributes.controlKey', {
                                required: (selectedCountry && config?.controlKeyMandatory) ? 'Required' : false
                            })}
                            disabled={!selectedCountry || !config?.showControlKey}
                            style={(!selectedCountry || !config?.showControlKey) ? { backgroundColor: '#f3f4f6', cursor: 'not-allowed' } : {}}
                        >
                            <option value="">--</option>
                            {config?.controlKeyValues?.map(key => (
                                <option key={key} value={key}>{key}</option>
                            ))}
                        </select>
                    </div>
                </div>
            </div>

            {/* SAP Configuration Display */}
            {config && (
                <div className="mt-2">
                    <div className="text-[10px] text-gray-500 p-2 bg-gray-50 border border-gray-200 rounded flex justify-between items-center">
                        <span><strong>SAP Config:</strong> {config.countryName} ({config.region})</span>
                        <span>Key: {config.primaryBankKey}</span>
                    </div>
                </div>
            )}
        </div>
    );
};
