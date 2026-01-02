import React from 'react';
import { UseFormRegister, FieldErrors, UseFormSetValue } from 'react-hook-form';
import { Input } from '../components/ui/Elements';
import { BankInformationForm } from '../components/BankInformationForm';

interface DynamicRegistrationFormProps {
    vendorType: string;
    wizardStep: number;
    register: UseFormRegister<any>;
    errors: FieldErrors<any>;
    readOnlyData: {
        vendorLegalName?: string;
        primaryContactEmail?: string;
    };
    setValue: UseFormSetValue<any>;
}

export const DynamicRegistrationForm: React.FC<DynamicRegistrationFormProps> = ({
    vendorType,
    wizardStep,
    register,
    errors,
    readOnlyData,
    setValue
}) => {
    // Shared common section titles
    const ADDRESS_TITLE = vendorType === 'Meeting' ? 'Event Provider Address' :
        vendorType === 'Physical' ? 'Contact & Address' :
            vendorType === 'Participant' ? 'Contact Information (Simplified)' :
                'Corporate Contact Information';

    // Physical Person (INDV)
    if (vendorType === 'Physical') {
        if (wizardStep === 2) {
            return (
                <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                    <div>
                        <Input
                            label="Given Name (First Name) *"
                            {...register('attributes.givenName', { required: 'Given name is required' })}
                            error={errors.attributes?.givenName?.message as string}
                            placeholder="John"
                        />
                    </div>
                    <div>
                        <Input
                            label="Family Name (Last Name) *"
                            {...register('attributes.familyName', { required: 'Family name is required' })}
                            error={errors.attributes?.familyName?.message as string}
                            placeholder="Doe"
                        />
                    </div>
                    <div>
                        <Input
                            label="Date of Birth *"
                            type="date"
                            {...register('attributes.dateOfBirth', { required: 'Date of Birth is required' })}
                            error={errors.attributes?.dateOfBirth?.message as string}
                        />
                    </div>
                    <div>
                        <Input
                            label="Profession / Title"
                            {...register('attributes.profession')}
                            placeholder="Consultant, Speaker, etc."
                        />
                    </div>
                </div>
            );
        }

        if (wizardStep === 3) {
            return (
                <div className="space-y-8">
                    <section>
                        <h3 className="text-lg font-medium border-b pb-2 mb-4">Identity & Nationality (MoUV)</h3>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Gender *</label>
                                <select {...register('attributes.gender')} className="w-full border rounded-md px-4 py-2 border-gray-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm">
                                    <option value="">Select...</option>
                                    <option value="Male">Male</option>
                                    <option value="Female">Female</option>
                                    <option value="Non-binary">Non-binary</option>
                                </select>
                            </div>
                            <div>
                                <Input label="Nationality *" {...register('attributes.nationality', { required: 'Nationality is required' })} />
                            </div>
                        </div>
                    </section>

                    <section>
                        <h3 className="text-lg font-medium border-b pb-2 mb-4">{ADDRESS_TITLE}</h3>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div className="md:col-span-2">
                                <Input label="Street Address *" {...register('attributes.street', { required: 'Street is required' })} />
                            </div>
                            <div>
                                <Input label="City *" {...register('attributes.city', { required: 'City is required' })} />
                            </div>
                            <div>
                                <Input label="Country *" {...register('attributes.country', { required: 'Country is required' })} />
                            </div>
                        </div>
                    </section>

                    <section>
                        <h3 className="text-lg font-medium border-b pb-2 mb-4">Bank Information (Mandatory)</h3>
                        <BankInformationForm register={register} errors={errors} setValue={setValue} />
                    </section>
                </div>
            );
        }
    }

    // Company or Organization (HQSU)
    if (vendorType === 'Company') {
        if (wizardStep === 2) {
            return (
                <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                    <div className="md:col-span-2">
                        <Input
                            label="Company Legal Name"
                            value={readOnlyData.vendorLegalName}
                            readOnly
                            className="bg-gray-50"
                            {...register('companyName')}
                        />
                    </div>
                    <div>
                        <Input
                            label="Tax ID / Registration Number *"
                            {...register('taxId', { required: 'Tax ID is required' })}
                            error={errors.taxId?.message as string}
                            placeholder="Tax ID, VAT, or Registration #"
                        />
                    </div>
                    <div>
                        <Input
                            label="Contact Person Name *"
                            {...register('contactName', { required: 'Contact name is required' })}
                            error={errors.contactName?.message as string}
                            placeholder="Name of person completing form"
                        />
                    </div>
                </div>
            );
        }

        if (wizardStep === 3) {
            return (
                <div className="space-y-8">
                    <section>
                        <h3 className="text-lg font-medium border-b pb-2 mb-4">Corporate Contact Information</h3>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div className="md:col-span-2">
                                <Input label="Street Address *" {...register('attributes.street', { required: 'Street is required' })} />
                            </div>
                            <div>
                                <Input label="City *" {...register('attributes.city', { required: 'City is required' })} />
                            </div>
                            <div>
                                <Input label="Country *" {...register('attributes.country', { required: 'Country is required' })} />
                            </div>
                            <div>
                                <Input label="Phone Number *" {...register('attributes.phone', { required: 'Phone is required' })} />
                            </div>
                        </div>
                    </section>

                    <section>
                        <h3 className="text-lg font-medium border-b pb-2 mb-4">Banking Details (MoUV Compliance)</h3>
                        <BankInformationForm register={register} errors={errors} setValue={setValue} />
                    </section>
                </div>
            );
        }
    }

    // Meeting or Conference (EVNT)
    if (vendorType === 'Meeting') {
        if (wizardStep === 2) {
            return (
                <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                    <div className="md:col-span-2">
                        <Input
                            label="Event / Meeting Name *"
                            {...register('attributes.eventName', { required: 'Event name is required' })}
                            error={errors.attributes?.eventName?.message as string}
                            placeholder="UNESCO Global Summit 2025"
                        />
                    </div>
                    <div>
                        <Input
                            label="Event Date *"
                            type="date"
                            {...register('attributes.eventDate', { required: 'Event date is required' })}
                            error={errors.attributes?.eventDate?.message as string}
                        />
                    </div>
                    <div>
                        <Input
                            label="Service Category *"
                            {...register('attributes.serviceCategory', { required: 'Service category is required' })}
                            placeholder="Catering, Venue, Audio/Visual, etc."
                        />
                    </div>
                </div>
            );
        }

        if (wizardStep === 3) {
            return (
                <div className="space-y-8">
                    <section>
                        <h3 className="text-lg font-medium border-b pb-2 mb-4">Event Provider Details</h3>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div className="md:col-span-2">
                                <Input label="Vendor Legal Name" value={readOnlyData.vendorLegalName} readOnly className="bg-gray-50" />
                            </div>
                            <div className="md:col-span-2">
                                <Input label="Street Address *" {...register('attributes.street', { required: 'Street is required' })} />
                            </div>
                            <div>
                                <Input label="City *" {...register('attributes.city', { required: 'City is required' })} />
                            </div>
                            <div>
                                <Input label="Country *" {...register('attributes.country', { required: 'Country is required' })} />
                            </div>
                        </div>
                    </section>

                    <section>
                        <h3 className="text-lg font-medium border-b pb-2 mb-4">Bank Information</h3>
                        <BankInformationForm register={register} errors={errors} setValue={setValue} />
                    </section>
                </div>
            );
        }
    }

    // Participant (PART)
    if (vendorType === 'Participant') {
        if (wizardStep === 2) {
            return (
                <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                    <div>
                        <Input
                            label="Given Name *"
                            {...register('attributes.givenName', { required: 'Given name is required' })}
                            error={errors.attributes?.givenName?.message as string}
                        />
                    </div>
                    <div>
                        <Input
                            label="Family Name *"
                            {...register('attributes.familyName', { required: 'Family name is required' })}
                            error={errors.attributes?.familyName?.message as string}
                        />
                    </div>
                    <div className="md:col-span-2">
                        <Input
                            label="Reason for Payment *"
                            {...register('attributes.paymentReason', { required: 'Reason for payment is required' })}
                            placeholder="Travel reimbursement, stipend, etc."
                        />
                    </div>
                </div>
            );
        }

        if (wizardStep === 3) {
            return (
                <div className="space-y-8">
                    <section>
                        <h3 className="text-lg font-medium border-b pb-2 mb-4">{ADDRESS_TITLE}</h3>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                                <Input label="Country *" {...register('attributes.country', { required: 'Country is required' })} />
                            </div>
                            <div className="md:col-span-2">
                                <p className="text-xs text-gray-400 italic">Only country of residence is required for Participant flows.</p>
                            </div>
                        </div>
                    </section>

                    <section>
                        <h3 className="text-lg font-medium border-b pb-2 mb-4">Bank Details (Bank Transfer Only)</h3>
                        <p className="text-sm text-gray-500 mb-4">Participant payments are processed exclusively via bank transfer. Please ensure accuracy.</p>
                        <BankInformationForm register={register} errors={errors} setValue={setValue} />
                    </section>
                </div>
            );
        }
    }

    // Default Fallback (Generic)
    return (
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
            <div className="md:col-span-2">
                <Input
                    label="Vendor Name"
                    value={readOnlyData.vendorLegalName}
                    readOnly
                    className="bg-gray-50"
                    {...register('companyName')} // Map to companyName as fallback
                />
            </div>

            <div>
                <Input
                    label="Tax ID / Reference"
                    {...register('taxId')}
                    placeholder="Reference Number"
                />
            </div>

            <div>
                <Input
                    label="Contact Person"
                    {...register('contactName', { required: 'Contact name is required' })}
                    error={errors.contactName?.message as string}
                />
            </div>

            <div className="md:col-span-2">
                <Input
                    label="Email Address"
                    value={readOnlyData.primaryContactEmail}
                    readOnly
                    className="bg-gray-50"
                    {...register('email')}
                />
            </div>
        </div>
    );
};
