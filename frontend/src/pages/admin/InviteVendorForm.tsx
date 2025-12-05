import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { Button, Input, Card } from '../../components/ui/Elements';
import { CheckCircle, Copy, Mail } from 'lucide-react';
import { api } from '../../services/api';

interface InviteVendorFormData {
    vendorLegalName: string;
    primaryContactEmail: string;
    expirationDays: number;
    notes?: string;
}

export const InviteVendorForm: React.FC = () => {
    const { register, handleSubmit, reset, formState: { errors } } = useForm<InviteVendorFormData>();
    const [submitted, setSubmitted] = useState(false);
    const [invitationData, setInvitationData] = useState<any>(null);
    const [copied, setCopied] = useState(false);
    const navigate = useNavigate();

    const onSubmit = async (data: InviteVendorFormData) => {
        try {
            const response = await api.post('/invitation/create', data);
            setInvitationData(response.data);
            setSubmitted(true);
        } catch (error: any) {
            const errorMessage = error.response?.data?.error || 'Failed to create invitation';
            alert(errorMessage);
            console.error('Failed to create invitation:', error);
        }
    };

    const copyInvitationLink = () => {
        const fullLink = `${window.location.origin}${invitationData.invitationLink}`;
        navigator.clipboard.writeText(fullLink);
        setCopied(true);
        setTimeout(() => setCopied(false), 2000);
    };

    const sendAnother = () => {
        setSubmitted(false);
        setInvitationData(null);
        reset();
    };

    if (submitted && invitationData) {
        const fullLink = `${window.location.origin}${invitationData.invitationLink}`;
        const expiresAt = new Date(invitationData.expiresAt).toLocaleString();

        return (
            <div className="min-h-screen bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
                <div className="max-w-3xl mx-auto">
                    <Card>
                        <div className="text-center py-8">
                            <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-green-100 mb-4">
                                <CheckCircle className="h-6 w-6 text-green-600" />
                            </div>
                            <h2 className="text-2xl font-bold text-gray-900 mb-2">Invitation Created!</h2>
                            <p className="text-gray-600 mb-6">
                                The invitation has been created successfully and is ready to be sent.
                            </p>

                            <div className="mb-6 p-4 bg-gray-50 rounded-lg">
                                <label className="block text-sm font-medium text-gray-700 mb-2">
                                    Invitation Link
                                </label>
                                <div className="flex gap-2">
                                    <input
                                        type="text"
                                        value={fullLink}
                                        readOnly
                                        className="flex-1 px-3 py-2 border border-gray-300 rounded-md bg-white text-sm"
                                    />
                                    <Button
                                        onClick={copyInvitationLink}
                                        className="flex items-center gap-2"
                                        variant={copied ? 'secondary' : 'primary'}
                                    >
                                        <Copy className="h-4 w-4" />
                                        {copied ? 'Copied!' : 'Copy'}
                                    </Button>
                                </div>
                                <p className="mt-2 text-xs text-gray-500">
                                    Expires: {expiresAt}
                                </p>
                            </div>

                            <div className="space-y-3">
                                <Button
                                    onClick={sendAnother}
                                    className="w-full justify-center"
                                    size="lg"
                                >
                                    Send Another Invitation
                                </Button>
                                <Button
                                    onClick={() => navigate('/admin/invitations')}
                                    variant="secondary"
                                    className="w-full justify-center"
                                >
                                    View All Invitations
                                </Button>
                            </div>

                            <div className="mt-6 p-4 bg-blue-50 border border-blue-200 rounded-lg text-left">
                                <div className="flex items-start gap-2">
                                    <Mail className="h-5 w-5 text-blue-600 mt-0.5" />
                                    <div className="text-sm text-blue-800">
                                        <strong>Next Steps:</strong>
                                        <ul className="mt-2 space-y-1 list-disc list-inside">
                                            <li>Copy the invitation link above</li>
                                            <li>Send it to the vendor contact via email</li>
                                            <li>Vendor will use this link to complete their registration</li>
                                            <li>You'll be notified once they submit their application</li>
                                        </ul>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </Card>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
            <div className="max-w-3xl mx-auto">
                <div className="mb-8">
                    <h1 className="text-3xl font-bold text-gray-900">Invite New Vendor</h1>
                    <p className="mt-2 text-gray-600">
                        Create a secure invitation link for a new vendor to register with your organization.
                    </p>
                </div>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
                    <Card>
                        <div className="space-y-6">
                            <div>
                                <Input
                                    label="Vendor Legal Name"
                                    {...register('vendorLegalName', {
                                        required: 'Vendor legal name is required',
                                        minLength: { value: 2, message: 'Name must be at least 2 characters' }
                                    })}
                                    error={errors.vendorLegalName?.message}
                                    placeholder="Acme Corporation Ltd."
                                />
                                <p className="mt-1 text-xs text-gray-500">
                                    The official legal name of the vendor company
                                </p>
                            </div>

                            <div>
                                <Input
                                    label="Primary Contact Email"
                                    type="email"
                                    {...register('primaryContactEmail', {
                                        required: 'Email is required',
                                        pattern: {
                                            value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                                            message: "Invalid email address"
                                        }
                                    })}
                                    error={errors.primaryContactEmail?.message}
                                    placeholder="contact@vendor.com"
                                />
                                <p className="mt-1 text-xs text-gray-500">
                                    This person will receive the invitation link and be responsible for completing the registration
                                </p>
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                    Invitation Expiration
                                </label>
                                <select
                                    {...register('expirationDays', { required: true })}
                                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent"
                                    defaultValue="14"
                                >
                                    <option value="7">7 days</option>
                                    <option value="14">14 days (Recommended)</option>
                                    <option value="30">30 days</option>
                                </select>
                                <p className="mt-1 text-xs text-gray-500">
                                    The invitation link will expire after this period
                                </p>
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                    Internal Notes (Optional)
                                </label>
                                <textarea
                                    {...register('notes')}
                                    rows={3}
                                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent"
                                    placeholder="Why is this vendor being onboarded? Project name, service type, etc."
                                />
                                <p className="mt-1 text-xs text-gray-500">
                                    These notes are for internal use only and won't be shared with the vendor
                                </p>
                            </div>
                        </div>

                        <div className="mt-8 pt-6 border-t border-gray-200">
                            <div className="flex gap-3">
                                <Button
                                    type="button"
                                    variant="secondary"
                                    onClick={() => navigate('/admin/invitations')}
                                    className="flex-1 justify-center"
                                >
                                    Cancel
                                </Button>
                                <Button
                                    type="submit"
                                    className="flex-1 justify-center"
                                    size="lg"
                                >
                                    Create Invitation
                                </Button>
                            </div>
                        </div>
                    </Card>
                </form>
            </div>
        </div>
    );
};
