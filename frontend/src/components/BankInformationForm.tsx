import React, { useState, useEffect } from 'react';
import { UseFormRegister, FieldErrors, UseFormSetValue } from 'react-hook-form';
import { Input } from '../components/ui/Elements';
import { Info, Lock } from 'lucide-react';

interface BankInformationFormProps {
    register: UseFormRegister<any>;
    errors: FieldErrors<any>;
    setValue: UseFormSetValue<any>;
}

export const BankInformationForm: React.FC<BankInformationFormProps> = ({
    register,
    errors,
    setValue
}) => {
    const [selectedCountry, setSelectedCountry] = useState('');
    const [isBankLocked, setIsBankLocked] = useState(true);

    const countries = [
        { code: 'US', name: 'United States' },
        { code: 'FR', name: 'France' },
        { code: 'AR', name: 'Argentina' },
        { code: 'CH', name: 'Switzerland' },
        { code: 'GB', name: 'United Kingdom' },
        // ... more countries would be loaded from a master data service
    ];

    useEffect(() => {
        if (selectedCountry) {
            setIsBankLocked(false);
        } else {
            setIsBankLocked(true);
        }
    }, [selectedCountry]);

    const handleCountryChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const country = e.target.value;
        setSelectedCountry(country);
        setValue('attributes.bankCountry', country);
    };

    return (
        <div className="space-y-6">
            <div className="bg-brand-50 p-4 rounded-lg flex items-start gap-3">
                <Info className="h-5 w-5 text-brand-600 mt-0.5" />
                <div className="text-sm text-brand-800">
                    <strong>UNESCO Requirement:</strong> Bank information fields are dynamically adjusted based on the bank's country. Please select the country first.
                </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="md:col-span-2">
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                        Bank Country *
                    </label>
                    <select
                        className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-brand-500 shadow-sm"
                        onChange={handleCountryChange}
                        value={selectedCountry}
                    >
                        <option value="">-- Select Country --</option>
                        {countries.map(c => (
                            <option key={c.code} value={c.code}>{c.name}</option>
                        ))}
                    </select>
                </div>

                {isBankLocked ? (
                    <div className="md:col-span-2 py-12 flex flex-col items-center justify-center border-2 border-dashed border-gray-200 rounded-xl bg-gray-50 text-gray-400">
                        <Lock className="h-8 w-8 mb-2" />
                        <p>Select a country to unlock bank fields</p>
                    </div>
                ) : (
                    <>
                        <div className="md:col-span-2">
                            <Input
                                label="Bank Name *"
                                {...register('attributes.bankName', { required: 'Bank name is required' })}
                                error={errors.attributes?.bankName?.message as string}
                            />
                        </div>

                        <div>
                            <Input
                                label="Account Holder Name *"
                                {...register('attributes.accountHolderName', { required: 'Account holder is required' })}
                                error={errors.attributes?.accountHolderName?.message as string}
                            />
                        </div>

                        <div>
                            <Input
                                label="Currency *"
                                {...register('attributes.bankCurrency', { required: 'Currency is required' })}
                                error={errors.attributes?.bankCurrency?.message as string}
                                placeholder="EUR, USD, etc."
                            />
                        </div>

                        {/* SEPA countries */}
                        {['FR', 'CH'].includes(selectedCountry) && (
                            <div className="md:col-span-2">
                                <Input
                                    label="IBAN *"
                                    {...register('attributes.iban', {
                                        required: 'IBAN is required for SEPA countries',
                                        pattern: { value: /^[A-Z0-9]{15,34}$/, message: 'Invalid IBAN format' }
                                    })}
                                    error={errors.attributes?.iban?.message as string}
                                />
                            </div>
                        )}

                        {/* US (Domestic) */}
                        {selectedCountry === 'US' && (
                            <>
                                <div>
                                    <Input
                                        label="Account Number *"
                                        {...register('attributes.accountNumber', { required: 'Account number is required' })}
                                        error={errors.attributes?.accountNumber?.message as string}
                                    />
                                </div>
                                <div>
                                    <Input
                                        label="ABA Routing Number *"
                                        {...register('attributes.abaNumber', { required: 'ABA number is required' })}
                                        error={errors.attributes?.abaNumber?.message as string}
                                    />
                                </div>
                            </>
                        )}

                        {/* Argentina (Domestic) */}
                        {selectedCountry === 'AR' && (
                            <div className="md:col-span-2">
                                <Input
                                    label="CBU *"
                                    {...register('attributes.cbu', {
                                        required: 'CBU is required',
                                        pattern: { value: /^\d{22}$/, message: 'CBU must be 22 digits' }
                                    })}
                                    error={errors.attributes?.cbu?.message as string}
                                />
                            </div>
                        )}

                        <div className="md:col-span-2">
                            <Input
                                label="BIC / SWIFT Code (If applicable)"
                                {...register('attributes.swiftCode')}
                                error={errors.attributes?.swiftCode?.message as string}
                            />
                        </div>
                    </>
                )}
            </div>
        </div>
    );
};
