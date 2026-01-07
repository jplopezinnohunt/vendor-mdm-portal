import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { Button, Input, Card } from '../components/ui/Elements';
import { CheckCircle, AlertCircle, Loader } from 'lucide-react';
import { api } from '../services/api';
import { DynamicRegistrationForm } from './DynamicRegistrationForm';

interface InvitationValidation {
    isValid: boolean;
    errorMessage?: string;
    vendorLegalName?: string;
    primaryContactEmail?: string;
    expiresAt?: string;
    vendorType?: string;
    currentStage?: string;
}

interface RegistrationFormData {
    companyName: string;
    taxId: string;
    contactName: string;
    email: string;
    attributes: any;
}

export const InvitationRegistration: React.FC = () => {
    const { token } = useParams<{ token: string }>();
    const navigate = useNavigate();
    const { register, handleSubmit, setValue, getValues, watch, formState: { errors } } = useForm<RegistrationFormData>({
        shouldUnregister: false
    });

    const [validating, setValidating] = useState(true);
    const [validation, setValidation] = useState<InvitationValidation | null>(null);
    const [submitting, setSubmitting] = useState(false);
    const [draftSaving, setDraftSaving] = useState(false);
    const [submitted, setSubmitted] = useState(false);

    // MFA and Multi-Stage State
    const [mfaTriggered, setMfaTriggered] = useState(false);
    const [mfaVerified, setMfaVerified] = useState(false);
    const [mfaCode, setMfaCode] = useState('');
    const [verifyingMfa, setVerifyingMfa] = useState(false);
    const [wizardStep, setWizardStep] = useState(1); // 1: MFA, 2: Initial Info, 3: Enrichment

    const mfaAutoSent = React.useRef(false);

    useEffect(() => {
        const validateToken = async () => {
            if (!token) return;
            try {
                setValidating(true);
                const response = await api.get<InvitationValidation>(`/invitation/validate/${token}`);
                setValidation(response.data);

                if (response.data.isValid) {
                    // Sync wizard step with backend stage
                    if (response.data.currentStage === 'InvitationSent') {
                        setWizardStep(1); // Need MFA
                        if (!mfaAutoSent.current) {
                            mfaAutoSent.current = true;
                            triggerMfa();
                        }
                    } else if (response.data.currentStage === 'MfaVerified') {
                        setWizardStep(2); // Initial Info
                        setMfaVerified(true);
                    } else if (response.data.currentStage === 'InitialInfoCompleted') {
                        setWizardStep(3); // Enrichment
                        setMfaVerified(true);
                    }

                    // Pre-fill form if data is returned
                    if (response.data.vendorLegalName) {
                        setValue('companyName', response.data.vendorLegalName);
                        // ... name splitting logic ...
                        if (response.data.vendorType === 'Physical') {
                            const nameParts = response.data.vendorLegalName.split(' ');
                            if (nameParts.length >= 2) {
                                setValue('attributes.givenName', nameParts[0]);
                                setValue('attributes.familyName', nameParts.slice(1).join(' '));
                            } else {
                                setValue('attributes.givenName', response.data.vendorLegalName);
                            }
                        }
                    }
                    if (response.data.primaryContactEmail) setValue('email', response.data.primaryContactEmail);
                }
            } catch (error: any) {
                console.error('Validation failed:', error);
                setValidation({
                    isValid: false,
                    errorMessage: error.response?.data?.error || 'Failed to validate link. It may have expired or is incorrect.'
                });
            } finally {
                setValidating(false);
            }
        };

        validateToken();
    }, [token, setValue]);

    const triggerMfa = async () => {
        try {
            await api.post(`/invitation/trigger-mfa/${token}`);
            setMfaTriggered(true);
        } catch (error) {
            console.error('MFA trigger failed', error);
        }
    };

    const handleVerifyMfa = async () => {
        try {
            setVerifyingMfa(true);
            await api.post(`/invitation/verify-mfa/${token}`, { code: mfaCode });
            setMfaVerified(true);
            setWizardStep(2);
        } catch (error: any) {
            alert(error.response?.data?.error || 'Invalid MFA code');
        } finally {
            setVerifyingMfa(false);
        }
    };

    const handleSaveDraft = async () => {
        try {
            setDraftSaving(true);
            const data = getValues(); // Get current form data without validation
            await api.post(`/invitation/save-draft/${token}`, data);
            alert("Draft saved successfully! You can return to this link anytime to complete your registration.");
        } catch (error: any) {
            console.error('Draft save failed:', error);
            const errorMessage = error.response?.data?.error || 'Failed to save draft.';
            alert(errorMessage);
        } finally {
            setDraftSaving(false);
        }
    };

    const onSubmit = async (data: RegistrationFormData) => {
        try {
            setSubmitting(true);

            if (wizardStep === 2) {
                // Submit Initial Info
                await api.post(`/invitation/submit-initial/${token}`, data.attributes || {});
                setWizardStep(3);
            } else if (wizardStep === 3) {
                // Submit Enrichment
                await api.post(`/invitation/submit-enrichment/${token}`, data.attributes || {});

                // Final Completion - Triggers Application Creation and Screening
                await api.post(`/invitation/complete/${token}`, {
                    companyName: data.companyName,
                    taxId: data.taxId,
                    contactName: data.contactName,
                    email: data.email,
                    attributes: data.attributes
                });

                setSubmitted(true);
            }
        } catch (error: any) {
            console.error('Submission failed:', error);
            const errorMessage = error.response?.data?.error || 'Failed to submit data. Please try again.';
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

    // Registration form wizard logic
    const expiresAt = validation.expiresAt ? new Date(validation.expiresAt).toLocaleString() : '';

    return (
        <div className="min-h-screen bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
            <div className="max-w-3xl mx-auto">
                <div className="mb-8">
                    <h1 className="text-3xl font-bold text-gray-900">UNESCO Vendor Registration</h1>
                    <div className="mt-4 flex items-center gap-2">
                        {[1, 2, 3].map((s) => (
                            <div key={s} className="flex items-center">
                                <div className={`h-8 w-8 rounded-full flex items-center justify-center text-sm font-bold ${wizardStep >= s ? 'bg-brand-600 text-white' : 'bg-gray-200 text-gray-400'}`}>
                                    {s}
                                </div>
                                {s < 3 && <div className={`h-1 w-12 ${wizardStep > s ? 'bg-brand-600' : 'bg-gray-200'}`} />}
                            </div>
                        ))}
                    </div>
                </div>

                {!mfaVerified ? (
                    <Card>
                        <div className="py-6">
                            <h2 className="text-xl font-bold mb-4">Identity Verification</h2>
                            <p className="text-gray-600 mb-6">
                                We've sent a 6-digit verification code to <strong>{validation.primaryContactEmail}</strong>.
                                Please enter it below to access your invitation.
                            </p>
                            <div className="max-w-xs">
                                <Input
                                    label="Verification Code"
                                    placeholder="000000"
                                    value={mfaCode}
                                    onChange={(e) => setMfaCode(e.target.value)}
                                    maxLength={6}
                                />
                                <Button
                                    className="mt-4 w-full justify-center"
                                    onClick={handleVerifyMfa}
                                    disabled={verifyingMfa || mfaCode.length !== 6}
                                >
                                    {verifyingMfa ? 'Verifying...' : 'Verify & Continue'}
                                </Button>
                                <button
                                    className="mt-4 text-sm text-brand-600 hover:underline"
                                    onClick={triggerMfa}
                                >
                                    Resend code
                                </button>
                            </div>
                        </div>
                    </Card>
                ) : (
                    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
                        <Card>
                            <div className="mb-6">
                                <h2 className="text-xl font-bold">
                                    {wizardStep === 2 ? 'Initial Information' : 'Detailed Enrichment'}
                                </h2>
                                <p className="text-sm text-gray-500">
                                    {wizardStep === 2
                                        ? 'Please confirm your basic identity details.'
                                        : 'Please provide full address, contact, and bank information.'}
                                </p>
                            </div>

                            <DynamicRegistrationForm
                                vendorType={validation.vendorType || 'Company'}
                                wizardStep={wizardStep}
                                register={register}
                                errors={errors}
                                readOnlyData={{
                                    vendorLegalName: validation.vendorLegalName,
                                    primaryContactEmail: validation.primaryContactEmail
                                }}
                                setValue={setValue}
                                watch={watch}
                            />

                            <div className="mt-8 pt-6 border-t border-gray-200 flex gap-4">
                                <Button
                                    type="button"
                                    variant="secondary"
                                    className="w-full justify-center"
                                    onClick={handleSaveDraft}
                                    disabled={submitting || draftSaving}
                                >
                                    {draftSaving ? 'Saving...' : 'Save for Later'}
                                </Button>
                                <Button
                                    type="submit"
                                    className="w-full justify-center"
                                    size="lg"
                                    disabled={submitting || draftSaving}
                                >
                                    {submitting ? 'Submitting...' : wizardStep === 2 ? 'Next: Enrichment' : 'Submit Final Application'}
                                </Button>
                            </div>
                        </Card>
                    </form>
                )}
            </div>
        </div>
    );
};
