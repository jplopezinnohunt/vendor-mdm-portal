import React, { useState } from 'react';
import { UseFormRegister, FieldErrors, UseFormSetValue, UseFormWatch } from 'react-hook-form';
import { Input } from '../components/ui/Elements';
import { BankInformationForm } from '../components/BankInformationForm';
import { CollapsibleSection } from '../components/ui/CollapsibleSection';
import { FileUpload } from '../components/ui/FileUpload';
import { AttachmentMetadata } from '../types/vendor';

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
    watch: UseFormWatch<any>;
}

export const DynamicRegistrationForm: React.FC<DynamicRegistrationFormProps> = ({
    vendorType,
    wizardStep,
    register,
    errors,
    readOnlyData,
    setValue,
    watch
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

                    <CollapsibleSection title={ADDRESS_TITLE} defaultExpanded={true}>
                        <div className="grid grid-cols-4 gap-4">
                            <div className="col-span-1">
                                <Input
                                    label="House No."
                                    {...register('attributes.address.houseNo')}
                                    placeholder="123"
                                />
                            </div>
                            <div className="col-span-3">
                                <Input
                                    label="Street Name *"
                                    {...register('attributes.address.streetName', { required: 'Street name is required' })}
                                    error={errors.attributes?.address?.streetName?.message as string}
                                    placeholder="Main Street"
                                />
                            </div>
                            <div className="col-span-4">
                                <Input
                                    label="Street Name 2"
                                    {...register('attributes.address.streetName2')}
                                    placeholder="Building name or additional address line"
                                />
                            </div>
                            <div className="col-span-4">
                                <Input
                                    label="Street Name 3"
                                    {...register('attributes.address.streetName3')}
                                    placeholder="Additional address line"
                                />
                            </div>
                            <div className="col-span-4">
                                <Input
                                    label="Street Name 4"
                                    {...register('attributes.address.streetName4')}
                                    placeholder="Additional address line"
                                />
                            </div>
                            <div className="col-span-2">
                                <Input
                                    label="Postal Code *"
                                    {...register('attributes.address.postalCode', { required: 'Postal code is required' })}
                                    error={errors.attributes?.address?.postalCode?.message as string}
                                    placeholder="12345"
                                />
                            </div>
                            <div className="col-span-2">
                                <Input
                                    label="City *"
                                    {...register('attributes.address.city', { required: 'City is required' })}
                                    error={errors.attributes?.address?.city?.message as string}
                                    placeholder="Dubai"
                                />
                            </div>
                            <div className="col-span-4">
                                <Input
                                    label="Country *"
                                    {...register('attributes.address.country', { required: 'Country is required' })}
                                    error={errors.attributes?.address?.country?.message as string}
                                    placeholder="United Arab Emirates"
                                />
                            </div>
                        </div>
                    </CollapsibleSection>

                    <CollapsibleSection title="Contact Information" defaultExpanded={true}>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                                <Input
                                    label="Phone"
                                    {...register('attributes.contactInfo.phone')}
                                    placeholder="+971 50 123 4567"
                                />
                            </div>
                            <div>
                                <Input
                                    label="Mobile Phone"
                                    {...register('attributes.contactInfo.mobilePhone')}
                                    placeholder="+971 55 123 4567"
                                />
                            </div>
                            <div>
                                <Input
                                    label="Email"
                                    type="email"
                                    {...register('attributes.contactInfo.email')}
                                    placeholder="contact@example.com"
                                />
                            </div>
                            <div>
                                <Input
                                    label="Email for Payment"
                                    type="email"
                                    {...register('attributes.contactInfo.paymentEmail')}
                                    placeholder="accounting@example.com"
                                />
                            </div>
                            <div>
                                <Input
                                    label="Fax"
                                    {...register('attributes.contactInfo.fax')}
                                    placeholder="+971 4 123 4567"
                                />
                            </div>
                        </div>
                    </CollapsibleSection>

                    <CollapsibleSection title="Identification Documents" defaultExpanded={true}>
                        <FileUpload
                            label="Identification (Max 2 files)"
                            category="DOC_ID_VERIFY"
                            docType="DOCTYPE_PASSPORT"
                            maxFiles={2}
                            vendorId={readOnlyData.vendorLegalName || 'new-vendor'}
                            onUploadComplete={(metadata) => {
                                const current = watch('attributes.attachments') || [];
                                setValue('attributes.attachments', [...current, metadata]);
                            }}
                            onDelete={(blobName) => {
                                const filtered = (watch('attributes.attachments') || [])
                                    .filter((a: AttachmentMetadata) => a.blobName !== blobName);
                                setValue('attributes.attachments', filtered);
                            }}
                            existingFiles={watch('attributes.attachments')}
                        />
                    </CollapsibleSection>

                    <CollapsibleSection title="Other Details" defaultExpanded={false}>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                                <Input
                                    label="Emergency Contact Name"
                                    {...register('attributes.otherDetails.emergencyContactName')}
                                    placeholder="Jane Doe"
                                />
                            </div>
                            <div>
                                <Input
                                    label="Emergency Contact Phone"
                                    {...register('attributes.otherDetails.emergencyContactPhone')}
                                    placeholder="+971 50 999 8888"
                                />
                            </div>
                        </div>
                    </CollapsibleSection>

                    <section>
                        <h3 className="text-lg font-medium border-b pb-2 mb-4">Bank Information (Mandatory)</h3>
                        <BankInformationForm key="bank-form-v2" register={register} errors={errors} setValue={setValue} />
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
                            label="Company Legal Name (Name 1)"
                            value={readOnlyData.vendorLegalName}
                            readOnly
                            className="bg-gray-50"
                            {...register('companyName')}
                        />
                    </div>
                    {/* Extended Names */}
                    <div>
                        <Input
                            label="Name 2 (Continuation)"
                            {...register('attributes.name2')}
                            placeholder="e.g. Division or Trade Name"
                        />
                    </div>
                    <div>
                        <Input
                            label="Name 3"
                            {...register('attributes.name3')}
                            placeholder="Additional name line"
                        />
                    </div>
                    <div>
                        <Input
                            label="Name 4"
                            {...register('attributes.name4')}
                            placeholder="Additional name line"
                        />
                    </div>
                    <div>
                        <Input
                            label="Search Term 1 *"
                            {...register('attributes.searchTerm1', { required: 'Search term is required' })}
                            error={errors.attributes?.searchTerm1?.message as string}
                            placeholder="Short alias for searching"
                        />
                    </div>

                    <div className="md:col-span-2 border-t pt-4 mt-2">
                        <h4 className="text-sm font-medium text-gray-900 mb-4">Tax Identification</h4>
                    </div>
                    <div>
                        <Input
                            label="Tax Number 1 (Main Tax ID) *"
                            {...register('taxId', { required: 'Tax ID is required' })}
                            error={errors.taxId?.message as string}
                            placeholder="Main Tax ID / VAT"
                        />
                    </div>
                    <div>
                        <Input
                            label="Tax Number 2 (Optional)"
                            {...register('attributes.taxNumber2')}
                            placeholder="Secondary Registration No."
                        />
                    </div>

                    <div className="md:col-span-2 border-t pt-4 mt-2">
                        <h4 className="text-sm font-medium text-gray-900 mb-4">Primary Contact</h4>
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
                        <div className="grid grid-cols-4 gap-4">
                            <div className="col-span-1">
                                <Input label="House No." {...register('attributes.address.houseNo')} placeholder="123" />
                            </div>
                            <div className="col-span-3">
                                <Input label="Street Name *" {...register('attributes.address.streetName', { required: 'Street is required' })} error={errors.attributes?.address?.streetName?.message as string} placeholder="Main Street" />
                            </div>
                            <div className="col-span-4">
                                <Input label="Street Name 2" {...register('attributes.address.streetName2')} placeholder="Building, Floor, Suite" />
                            </div>
                            <div className="col-span-4">
                                <Input label="Street Name 3" {...register('attributes.address.streetName3')} />
                            </div>
                            <div className="col-span-4">
                                <Input label="Street Name 4" {...register('attributes.address.streetName4')} />
                            </div>
                            <div className="col-span-2">
                                <Input label="Postal Code *" {...register('attributes.address.postalCode', { required: 'Postal Code is required' })} error={errors.attributes?.address?.postalCode?.message as string} />
                            </div>
                            <div className="col-span-2">
                                <Input label="City *" {...register('attributes.city', { required: 'City is required' })} error={errors.attributes?.city?.message as string} />
                            </div>
                            <div className="col-span-4">
                                <Input label="Country *" {...register('attributes.country', { required: 'Country is required' })} error={errors.attributes?.country?.message as string} />
                            </div>

                            <div className="col-span-4 border-t pt-4 mt-2">
                                <h4 className="text-sm font-medium text-gray-900 mb-2">Communication</h4>
                            </div>
                            <div className="col-span-2">
                                <Input label="Phone Number *" {...register('attributes.phone', { required: 'Phone is required' })} error={errors.attributes?.phone?.message as string} />
                            </div>
                            <div className="col-span-2">
                                <Input label="Fax" {...register('attributes.fax')} />
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
