import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { Button, Input, Card } from '../components/ui/Elements';
import { CheckCircle, AlertCircle, Loader } from 'lucide-react';
import { api } from '../services/api';

interface InvitationValidation {
    isValid: boolean;
    errorMessage?: string;
    vendorLegalName?: string;
    primaryContactEmail?: string;
    expiresAt?: string;
}

interface RegistrationFormData {
    companyName: string;
    taxId: string;
    contactName: string;
    email: string;
}

export const InvitationRegistration: React.FC = () => {
    const { token } = useParams<{ token: string }>();
    const navigate = useNavigate();
    const { register, handleSubmit, setValue, formState: { errors } } = useForm<RegistrationFormData>();

    const [validating, setValidating] = useState(true);
    const [validation, setValidation] = useState<InvitationValidation | null>(null);
    const [submitting, setSubmitting] = useState(false);
    const [submitted, setSubmitted] = useState(false);

    useEffect(() => {
        const validateInvitation = async () => {
            if (!token) {
                setValidating(false);
                return;
            }

            try {
                const response = await api.get(`/invitation/validate/${token}`);
                setValidation(response.data);

                // Pre-fill form fields
                if (response.data.isValid) {
                    setValue('companyName', response.data.vendorLegalName);
                    setValue('email', response.data.primaryContactEmail);
                }
            } catch (error) {
                console.error('Failed to validate invitation:', error);
                setValidation({
                    isValid: false,
                    errorMessage: 'Failed to validate invitation link'
                });
            } finally {
                setValidating(false);
            }
        };

        validateInvitation();
    }, [token, setValue]);

    const onSubmit = async (data: RegistrationFormData) => {
        try {
            setSubmitting(true);
            await api.post(`/invitation/complete/${token}`, data);
            setSubmitted(true);
        } catch (error: any) {
            console.error('Submission failed:', error);
            const errorMessage = error.response?.data?.error || 'Failed to submit application. Please try again.';
            alert(errorMessage);
        } finally {
            setSubmitting(false);
        }
    };

    // Loading state
    if (validating) {
        return (
            <div className="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
                <div className="sm:mx-auto sm:w-full sm:max-w-md">
                    <Card>
                        <div className="text-center py-12">
                            <Loader className="mx-auto h-12 w-12 text-brand-600 animate-spin" />
                            <p className="mt-4 text-gray-600">Validating invitation...</p>
                        </div>
                    </Card>
                </div>
            </div>
        );
    }

    // Invalid invitation
    if (!validation?.isValid) {
        return (
            <div className="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
                <div className="sm:mx-auto sm:w-full sm:max-w-md">
                    <Card>
                        <div className="text-center py-8">
                            <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-red-100 mb-4">
                                <AlertCircle className="h-6 w-6 text-red-600" />
                            </div>
                            <h2 className="text-2xl font-bold text-gray-900 mb-2">Invalid Invitation</h2>
                            <p className="text-gray-600 mb-6 px-4">
                                {validation?.errorMessage || 'This invitation link is not valid.'}
                            </p>
                            <div className="space-y-3">
                                <p className="text-sm text-gray-600">
                                    If you believe this is an error, please contact our vendor support team:
                                </p>
                                <p className="text-sm font-medium text-gray-900">
                                    vendorsupport@company.com
                                </p>
                            </div>
                            <div className="mt-6">
                                <Link to="/login">
                                    <Button variant="secondary" className="w-full justify-center">
                                        Return to Login
                                    </Button>
                                </Link>
                            </div>
                        </div>
                    </Card>
                </div>
            </div>
        );
    }

    // Success state
    if (submitted) {
        return (
            <div className="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
                <div className="sm:mx-auto sm:w-full sm:max-w-md">
                    <Card>
                        <div className="text-center py-8">
                            <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-green-100 mb-4">
                                <CheckCircle className="h-6 w-6 text-green-600" />
                            </div>
                            <h2 className="text-2xl font-bold text-gray-900 mb-2">Application Submitted!</h2>
                            <p className="text-gray-600 mb-6 px-4">
                                Thank you for completing your vendor registration. We have received your application and sent a confirmation email to <strong>{validation.primaryContactEmail}</strong>.
                            </p>
                            <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
                                <p className="text-sm text-blue-800">
                                    <strong>What's Next?</strong>
                                    <br />
                                    Our team will review your application and contact you within 3-5 business days. You'll receive login credentials once your application is approved.
                                </p>
                            </div>
                            <Link to="/login">
                                <Button className="w-full justify-center">Return to Home</Button>
                            </Link>
                        </div>
                    </Card>
                </div>
            </div>
        );
    }

    // Registration form
    const expiresAt = validation.expiresAt ? new Date(validation.expiresAt).toLocaleString() : '';

    return (
        <div className="min-h-screen bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
            <div className="max-w-3xl mx-auto">
                <div className="mb-8">
                    <h1 className="text-3xl font-bold text-gray-900">Complete Vendor Registration</h1>
                    <p className="mt-2 text-gray-600">
                        You've been invited to register as a vendor. Please complete the form below to submit your application.
                    </p>
                    {expiresAt && (
                        <p className="mt-2 text-sm text-orange-600">
                            This invitation expires on {expiresAt}
                        </p>
                    )}
                </div>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
                    <Card>
                        <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                            {/* Company Name - Pre-filled and read-only */}
                            <div className="md:col-span-2">
                                <Input
                                    label="Company Name"
                                    {...register('companyName', { required: 'Company name is required' })}
                                    error={errors.companyName?.message}
                                    readOnly
                                    className="bg-gray-50"
                                />
                                <p className="mt-1 text-xs text-gray-500">This field is pre-filled from your invitation</p>
                            </div>

                            {/* Tax ID */}
                            <div>
                                <Input
                                    label="Tax ID / VAT Number"
                                    {...register('taxId', { required: 'Tax ID is required' })}
                                    error={errors.taxId?.message}
                                    placeholder="e.g. US-123456789"
                                />
                            </div>

                            {/* Contact Person */}
                            <div>
                                <Input
                                    label="Contact Person"
                                    {...register('contactName', { required: 'Contact name is required' })}
                                    error={errors.contactName?.message}
                                    placeholder="Full name"
                                />
                            </div>

                            {/* Email - Pre-filled and read-only */}
                            <div className="md:col-span-2">
                                <Input
                                    label="Email Address"
                                    type="email"
                                    {...register('email', {
                                        required: 'Email is required',
                                        pattern: {
                                            value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                                            message: "Invalid email address"
                                        }
                                    })}
                                    error={errors.email?.message}
                                    readOnly
                                    className="bg-gray-50"
                                />
                                <p className="mt-1 text-xs text-gray-500">This field is pre-filled from your invitation</p>
                            </div>
                        </div>

                        {/* Information Box */}
                        <div className="mt-6 p-4 bg-blue-50 border border-blue-200 rounded-lg">
                            <p className="text-sm text-blue-800">
                                <strong>Required Documents (Next Step):</strong>
                                <br />
                                After submitting this form, you'll need to provide:
                            </p>
                            <ul className="mt-2 space-y-1 text-sm text-blue-700 list-disc list-inside">
                                <li>Tax Certificate (W-9/W-8 or equivalent)</li>
                                <li>Banking Details (IBAN, Account Number)</li>
                                <li>Certificate of Insurance</li>
                                <li>Legal Entity Documentation</li>
                            </ul>
                        </div>

                        <div className="mt-8 pt-6 border-t border-gray-200">
                            <Button
                                type="submit"
                                className="w-full justify-center"
                                size="lg"
                                disabled={submitting}
                            >
                                {submitting ? 'Submitting...' : 'Submit Application'}
                            </Button>
                        </div>

                        {/* Service Indicators */}
                        <div className="mt-6 pt-6 border-t border-gray-100 grid grid-cols-2 gap-4">
                            <div className="flex items-center gap-2 text-xs text-gray-500">
                                <div className="h-1.5 w-1.5 bg-blue-500 rounded-full"></div>
                                <span>Form Validation: Client-side</span>
                            </div>
                            <div className="flex items-center gap-2 text-xs text-green-600 font-medium">
                                <div className="h-1.5 w-1.5 bg-green-500 rounded-full animate-pulse"></div>
                                <span>Backend: Connected (Invitation API)</span>
                            </div>
                        </div>
                    </Card>
                </form>
            </div>
        </div>
    );
};
