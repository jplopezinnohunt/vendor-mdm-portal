import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { Button, Card } from '../../components/ui/Elements';
import { DuplicateDetectionModal } from '../../components/DuplicateDetectionModal';
import { useAuth } from '../../context/AuthContext';
import { api } from '../../services/api';
import { DuplicateVendor, InvitationResponse, SanctionsResult } from '../../types/vendor';
import { CheckCircle, ChevronRight, ChevronLeft, Layout, Settings, FileText, Mail, ShieldCheck, Copy, Search, AlertTriangle, ShieldAlert, User, Globe } from 'lucide-react';

interface InviteVendorFormData {
    vendorLegalName: string;
    givenName?: string;
    familyName?: string;
    primaryContactEmail: string;
    vendorType: string;
    accountGroup: string;
    expirationDays: number;
    companyCode: string;
    country: string;
    currency: string;
    sapLanguage: string;
    taxCode1?: string;
    taxCode2?: string;
    permittedPayee?: string;
    dateOfBirth?: string;
    notes?: string;
}

const ACCOUNT_GROUP_OPTIONS: Record<string, { value: string, label: string }[]> = {
    'Physical': [
        { value: 'INDV', label: 'Individual - Physical Person (INDV)' },
        { value: 'SCSA', label: 'SC - Staff Contract Holder (SCSA)' },
        { value: 'FELL', label: 'Fellow / Grant Recipient (FELL)' },
    ],
    'Company': [
        { value: 'HQSU', label: 'Company / Organization (HQSU)' },
        { value: 'INSO', label: 'Insurance Provider (INSO)' },
        { value: 'NGOS', label: 'NGO Supplier (NGOS)' },
    ],
    'Meeting': [
        { value: 'EVNT', label: 'Event Venue/Service Provider (EVNT)' },
        { value: 'CONF', label: 'Conference Organizer (CONF)' },
    ],
    'Participant': [
        { value: 'PART', label: 'Participant - One-time Payment (PART)' },
    ]
};

const COMPANY_CODE_OPTIONS = [
    { value: 'UNES', label: 'UNESCO Headquarters (UNES)' },
    { value: 'IIEP', label: 'IIEP - International Institute for Educational Planning' },
    { value: 'UIS', label: 'UIS - UNESCO Institute for Statistics' },
    { value: 'UBO', label: 'UBO - UNESCO Brasilia Office' },
];

const STEPS = [
    { id: 'selection', title: 'Selection', icon: Settings },
    { id: 'main-data', title: 'Main Data', icon: Mail },
    { id: 'review', title: 'Review', icon: FileText },
];

export const InviteVendorForm: React.FC = () => {
    const navigate = useNavigate();
    const { user } = useAuth();
    const [submitting, setSubmitting] = useState(false);
    const [submitted, setSubmitted] = useState(false);
    const [invitationData, setInvitationData] = useState<InvitationResponse | null>(null);
    const [copied, setCopied] = useState(false);

    // Flow states
    const [categoryCommitted, setCategoryCommitted] = useState(false);
    const [activeStep, setActiveStep] = useState(0);
    const [viewMode, setViewMode] = useState<'wizard' | 'full'>('wizard');
    const [isChecking, setIsChecking] = useState(false);
    const [checkResults, setCheckResults] = useState<DuplicateVendor[] | null>(null);
    const [sanctionsResult, setSanctionsResult] = useState<SanctionsResult | null>(null);
    const [validationPassed, setValidationPassed] = useState(false);
    const [showDupModal, setShowDupModal] = useState(false);
    const [validationTimestamp, setValidationTimestamp] = useState<string | null>(null);
    const [emailStatus, setEmailStatus] = useState<{ isConnected: boolean; mode: string } | null>(null);

    useEffect(() => {
        const checkEmailStatus = async () => {
            try {
                const response = await api.get('/system/data-sources');
                setEmailStatus(response.data.email);
            } catch (error) {
                console.error('Failed to check email status:', error);
            }
        };
        checkEmailStatus();
    }, []);

    const { register, handleSubmit, watch, setValue, reset, formState: { errors } } = useForm<InviteVendorFormData>({
        defaultValues: {
            vendorType: 'Physical',
            accountGroup: 'INDV',
            companyCode: 'UNES',
            country: '',
            currency: 'EUR',
            sapLanguage: 'EN',
            taxCode1: '',
            taxCode2: '',
            permittedPayee: '',
            expirationDays: 14,
            givenName: '',
            familyName: ''
        }
    });

    const selectedVendorType = watch('vendorType');
    const selectedAccountGroup = watch('accountGroup');
    const selectedCompanyCode = watch('companyCode');
    const isIndividual = selectedVendorType === 'Physical' || selectedVendorType === 'Participant';

    const nextStep = () => setActiveStep((prev) => Math.min(prev + 1, STEPS.length - 1));
    const prevStep = () => setActiveStep((prev) => Math.max(prev - 1, 0));

    const handleValidationSequence = async () => {
        const data = watch();
        // Construct search query based on type
        const searchQuery = isIndividual
            ? (data.familyName && data.givenName ? `${data.familyName} ${data.givenName}` : '')
            : data.vendorLegalName;

        if (!searchQuery) {
            alert('Please enter vendor name to perform the check');
            return;
        }

        setIsChecking(true);
        try {
            // 1. SAP Duplicate Check
            const dupResponse = await api.post('sap/vendor/search', {
                vendorType: isIndividual ? 'INDV' : 'COMP',
                familyName: isIndividual ? data.familyName : undefined,
                givenName: isIndividual ? data.givenName : undefined,
                companyName: !isIndividual ? data.vendorLegalName : undefined,
                companyCode: data.companyCode || 'UNES',
                searchThreshold: 0.75
            });

            const duplicates = dupResponse.data.vendors;
            setCheckResults(duplicates);
            setValidationTimestamp(new Date().toLocaleString());

            // ALWAYS show modal, even if no duplicates found
            setShowDupModal(true);
            setIsChecking(false);

        } catch (error: any) {
            console.error('Validation failed:', error);
            const msg = error.userMessage || error.response?.data?.error || error.message || 'Validation failed';
            alert(`Check failed: ${msg}`);
            setIsChecking(false);
        }
    };

    const performSanctionsScreening = async () => {
        const data = watch();
        // Construct name correctly for individuals vs companies
        const searchQuery = isIndividual
            ? (data.familyName && data.givenName ? `${data.familyName} ${data.givenName}` : '')
            : data.vendorLegalName;

        if (!searchQuery) {
            alert('Cannot perform sanctions check: Vendor name is missing');
            return;
        }

        setIsChecking(true);
        try {
            const sanctionsResponse = await api.post('sanctions/screen', {
                entityName: searchQuery,
                entityType: isIndividual ? 'Individual' : 'Organization',
                country: data.country,
                vendorId: "NEW_INVITATION"
            });
            setSanctionsResult(sanctionsResponse.data);
            setValidationPassed(true);

            if (viewMode === 'wizard') {
                setActiveStep(2); // Move to Review Step
            }
        } catch (error: any) {
            console.error('Sanctions screening failed:', error);
            const msg = error.userMessage || error.response?.data?.error || error.message || 'Sanctions screening failed';
            alert(`Sanctions check failed: ${msg}`);
        } finally {
            setIsChecking(false);
        }
    };

    const handleConfirmDuplicateBypass = () => {
        setShowDupModal(false);
        performSanctionsScreening();
    };

    const onSubmit = async (data: InviteVendorFormData) => {
        setSubmitting(true);
        try {
            // Concatenate name if individual
            if (isIndividual && data.familyName && data.givenName) {
                data.vendorLegalName = `${data.familyName.toUpperCase()} ${data.givenName}`;
            }

            // Sanitize payload: ASP.NET Core dislikes empty strings for nullable DateTime/numbers
            const payload = {
                ...data,
                expirationDays: Number(data.expirationDays),
                dateOfBirth: data.dateOfBirth || null,
                taxCode1: data.taxCode1 || null,
                taxCode2: data.taxCode2 || null,
                permittedPayee: data.permittedPayee || null,
                notes: data.notes || null,
                accountGroup: data.accountGroup || null,
                companyCode: data.companyCode || null
            };



            const response = await api.post('/invitation/create', payload);
            setInvitationData(response.data);
            setSubmitted(true);
        } catch (error: any) {
            console.error('Failed to create invitation:', error);

            // Handle server error redirect explicitly
            if (error.isServerError && error.serverDetails) {
                navigate('/500', { state: error.serverDetails });
                return;
            }

            // Safe error handling for Alert
            try {
                const errData = error.response?.data || {};
                // Prioritize problem details properties
                const msg = errData.detail || errData.message || errData.error || errData.title || error.message || 'Failed to create invitation';

                let finalMsg = msg;

                // If 400 Bad Request, dump the validation errors if available
                if (error.response?.status === 400 && errData.errors) {
                    finalMsg += `\n\nValidation Errors:\n${JSON.stringify(errData.errors, null, 2)}`;
                } else if (!msg && Object.keys(errData).length > 0) {
                    finalMsg += `\n\nPayload: ${JSON.stringify(errData, null, 2)}`;
                }

                const stack = errData.stack ? `\n\nDebug Info:\n${errData.stack.substring(0, 200)}...` : '';
                alert(`Error: ${finalMsg}${stack}`);
            } catch (alertError) {
                alert('An error occurred, and we failed to process the error message. check console.');
                console.error('Error processing alert:', alertError);
            }
        } finally {
            setSubmitting(false);
        }
    };

    const copyInvitationLink = () => {
        if (!invitationData) return;
        const fullLink = `${window.location.origin}${invitationData.invitationLink}`;
        navigator.clipboard.writeText(fullLink);
        setCopied(true);
        setTimeout(() => setCopied(false), 2000);
    };

    const sendAnother = () => {
        setSubmitted(false);
        setInvitationData(null);
        setCategoryCommitted(false);
        setActiveStep(0);
        reset();
    };

    if (submitted && invitationData) {
        const fullLink = `${window.location.origin}${invitationData.invitationLink}`;
        const expiresAt = new Date(invitationData.expiresAt).toLocaleString();

        return (
            <div className="max-w-3xl mx-auto py-12">
                <Card>
                    <div className="text-center py-8">
                        <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-blue-100 mb-6">
                            <Mail className="h-10 w-10 text-blue-600" />
                        </div>
                        <h2 className="text-3xl font-bold text-gray-900 mb-4">Invitation Ready!</h2>

                        {invitationData.emailSent === false && (
                            <div className="mb-6 mx-auto max-w-lg bg-amber-50 border border-amber-200 rounded-md p-4 flex items-start text-left">
                                <AlertTriangle className="h-5 w-5 text-amber-500 mt-0.5 flex-shrink-0" />
                                <div className="ml-3">
                                    <h3 className="text-sm font-medium text-amber-800">Email Warning</h3>
                                    <div className="mt-1 text-sm text-amber-700">
                                        <p>{invitationData.emailError || "The invitation was created, but the email could not be sent. Please copy the link below and send it manually."}</p>
                                    </div>
                                </div>
                            </div>
                        )}

                        <div className="mb-8 p-6 bg-gray-50 rounded-xl border border-gray-200">
                            <label className="block text-xs font-bold text-gray-400 uppercase tracking-widest mb-2 text-left">Secure Link</label>
                            <div className="flex gap-2">
                                <input readOnly value={fullLink} className="flex-1 px-3 py-2 border rounded bg-white text-sm font-mono truncate" />
                                <Button onClick={copyInvitationLink} variant={copied ? 'secondary' : 'primary'} className="flex items-center gap-2">
                                    <Copy className="h-4 w-4" />
                                    {copied ? 'Copied!' : 'Copy'}
                                </Button>
                            </div>
                            <p className="mt-3 text-xs text-gray-500 text-left">Expires: {expiresAt}</p>
                        </div>

                        <div className="flex flex-col gap-3">
                            <Button onClick={sendAnother} variant="primary" className="w-full justify-center py-3 font-bold">Send Another Invitation</Button>
                            <Button onClick={() => user?.role === 'Admin' ? navigate('/admin/invitations') : navigate('/approver/worklist')} variant="secondary" className="w-full justify-center">Back to Dashboard</Button>
                        </div>
                    </div>
                </Card>
            </div>
        );
    }

    return (
        <div className="mx-auto max-w-7xl py-8 px-4 sm:px-6 md:px-8">
            <div className="mb-8 flex flex-col md:flex-row md:items-end justify-between gap-4">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900 border-b-2 border-brand-500 pb-2 inline-block">Invite New Vendor</h1>
                    <p className="mt-4 text-gray-600 text-sm">Initiate the secure onboarding flow for a new vendor partner.</p>

                </div>

                {/* View Toggle */}
                <div className="flex bg-gray-100 p-1 rounded-lg self-start">
                    <button type="button" onClick={() => setViewMode('wizard')} className={`flex items-center gap-2 px-3 py-1.5 rounded-md text-xs font-bold transition-all ${viewMode === 'wizard' ? 'bg-white text-brand-700 shadow-sm' : 'text-gray-500 hover:text-gray-700'}`}>
                        <Layout className="w-3.5 h-3.5" /> Wizard
                    </button>
                    <button type="button" onClick={() => setViewMode('full')} className={`flex items-center gap-2 px-3 py-1.5 rounded-md text-xs font-bold transition-all ${viewMode === 'full' ? 'bg-white text-brand-700 shadow-sm' : 'text-gray-500 hover:text-gray-700'}`}>
                        <FileText className="w-3.5 h-3.5" /> Full
                    </button>
                </div>
            </div>

            {emailStatus && !emailStatus.isConnected && emailStatus.mode !== 'Console Logging' && (
                <div className="mb-8 mx-auto max-w-7xl bg-red-50 border-l-4 border-red-400 p-4 flex items-start shadow-sm rounded-r-md animate-in fade-in slide-in-from-top duration-500">
                    <ShieldAlert className="h-6 w-6 text-red-500 mt-0.5 flex-shrink-0" />
                    <div className="ml-4">
                        <h3 className="text-base font-black text-red-900 uppercase tracking-tight">System Alert: Email Service Offline</h3>
                        <p className="mt-1 text-sm text-red-700 font-medium">
                            The configured email provider ({emailStatus.mode}) is currently unreachable.
                            Invitations will be created, but you must <span className="underline font-bold">manually copy and send</span> the link to the vendor.
                        </p>
                    </div>
                </div>
            )}

            {/* Debug removed */}

            {viewMode === 'wizard' && (
                <div className="mb-12">
                    <div className="flex items-center justify-between relative px-2">
                        <div className="absolute top-1/2 left-0 w-full h-0.5 bg-gray-200 -translate-y-1/2 z-0" />
                        <div className="absolute top-1/2 left-0 h-0.5 bg-brand-500 -translate-y-1/2 z-0 transition-all duration-500" style={{ width: `${(activeStep / (STEPS.length - 1)) * 100}%` }} />
                        {STEPS.map((step, idx) => {
                            const Icon = step.icon;
                            const isCompleted = activeStep > idx;
                            const isActive = activeStep === idx;
                            return (
                                <div key={step.id} className="relative z-10 flex flex-col items-center cursor-pointer" onClick={() => (idx < activeStep || categoryCommitted) && setActiveStep(idx)}>
                                    <div className={`w-10 h-10 rounded-full flex items-center justify-center transition-all duration-300 border-2 ${isCompleted ? 'bg-brand-600 border-brand-600 text-white shadow-lg' : isActive ? 'bg-white border-brand-500 text-brand-700 shadow-md ring-4 ring-brand-50' : 'bg-gray-50 border-gray-200 text-gray-400'}`}>
                                        {isCompleted ? <CheckCircle className="w-6 h-6" /> : <Icon className="w-5 h-5" />}
                                    </div>
                                    <span className={`absolute -bottom-6 text-[10px] font-bold uppercase tracking-wider whitespace-nowrap ${isActive ? 'text-brand-700' : isCompleted ? 'text-brand-600' : 'text-gray-400'}`}>{step.title}</span>
                                </div>
                            );
                        })}
                    </div>
                </div>
            )}

            <form onSubmit={handleSubmit(onSubmit)} className="space-y-8 mt-12">
                {/* Step 0: Selection */}
                {(viewMode === 'full' || activeStep === 0) && (
                    <div className="transition-all">
                        {!categoryCommitted ? (
                            <Card>
                                <div className="flex flex-col md:flex-row gap-8 items-center justify-between">
                                    <div className="flex-1 space-y-6">
                                        <div>
                                            <label className="block text-sm font-bold text-gray-700 uppercase mb-3">1. Select Vendor Category</label>
                                            <div className="flex flex-wrap gap-2">
                                                {['Physical', 'Company', 'Meeting', 'Participant'].map((type) => (
                                                    <button key={type} type="button" onClick={() => { setValue('vendorType', type); setValue('accountGroup', ''); setValue('companyCode', ''); }} className={`px-4 py-2 text-sm font-bold border transition-all ${selectedVendorType === type ? 'bg-brand-600 text-white border-brand-600 shadow-md' : 'bg-gray-50 text-gray-600 border-gray-200 hover:border-brand-300'}`}>
                                                        {type === 'Physical' ? 'Individual' : type}
                                                    </button>
                                                ))}
                                            </div>
                                        </div>

                                        {selectedVendorType && (
                                            <div className="animate-in fade-in slide-in-from-left">
                                                <label className="block text-sm font-bold text-gray-700 uppercase mb-3">2. Select Account Group</label>
                                                <div className="flex flex-wrap gap-2">
                                                    {(ACCOUNT_GROUP_OPTIONS[selectedVendorType] || []).map((opt) => (
                                                        <button key={opt.value} type="button" onClick={() => { setValue('accountGroup', opt.value); setValue('companyCode', ''); }} className={`px-3 py-1.5 text-xs font-medium border transition-all ${watch('accountGroup') === opt.value ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-600 border-gray-300 hover:border-blue-400'}`}>
                                                            {opt.label}
                                                        </button>
                                                    ))}
                                                </div>
                                            </div>
                                        )}

                                        {watch('accountGroup') && (
                                            <div className="animate-in fade-in slide-in-from-left">
                                                <label className="block text-sm font-bold text-gray-700 uppercase mb-3">3. UNESCO Entity (Company Code)</label>
                                                <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
                                                    {COMPANY_CODE_OPTIONS.map((cc) => (
                                                        <button key={cc.value} type="button" onClick={() => setValue('companyCode', cc.value)} className={`px-3 py-2 text-left text-xs font-medium border transition-all ${watch('companyCode') === cc.value ? 'bg-brand-100 border-brand-500 text-brand-900 border-l-4' : 'bg-white text-gray-600 border-gray-200 hover:border-brand-300'}`}>
                                                            {cc.label}
                                                        </button>
                                                    ))}
                                                </div>
                                            </div>
                                        )}
                                    </div>
                                </div>
                                <div className="mt-8 flex justify-end border-t pt-4">
                                    <Button type="button" disabled={!selectedVendorType || !selectedAccountGroup || !selectedCompanyCode} onClick={() => { setCategoryCommitted(true); if (viewMode === 'wizard') nextStep(); }} className="px-8 font-bold bg-brand-700 text-white">Confirm & Continue</Button>
                                </div>
                            </Card>
                        ) : (
                            <div className="bg-brand-900 text-white px-6 py-4 shadow-lg flex items-center justify-between rounded-sm">
                                <div className="flex items-center gap-8">
                                    <div className="flex flex-col">
                                        <span className="text-[10px] font-bold uppercase text-brand-300 tracking-widest leading-none">Category</span>
                                        <span className="text-lg font-black tracking-tight">{selectedVendorType === 'Physical' ? 'Individual' : selectedVendorType}</span>
                                    </div>
                                    <div className="hidden md:flex flex-col">
                                        <span className="text-[10px] font-bold uppercase text-brand-300 tracking-widest leading-none">Operational Group</span>
                                        <span className="text-sm font-bold text-brand-100">{(ACCOUNT_GROUP_OPTIONS[selectedVendorType] || []).find(o => o.value === watch('accountGroup'))?.label || watch('accountGroup')}</span>
                                    </div>
                                </div>
                                <button type="button" onClick={() => { setCategoryCommitted(false); setActiveStep(0); }} className="text-[10px] bg-brand-800 hover:bg-white hover:text-brand-900 border border-brand-700 px-3 py-1 font-bold uppercase tracking-tighter cursor-pointer">Change</button>
                            </div>
                        )}
                    </div>
                )}

                {/* Step 1: Main Data */}
                {(viewMode === 'full' || activeStep === 1) && categoryCommitted && (
                    <Card title="Main Data: Vendor Contact Information">
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            {isIndividual ? (
                                <>
                                    <div>
                                        <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Family Name *</label>
                                        <input {...register('familyName', { required: true })} disabled={validationPassed} className="w-full px-3 py-2 border rounded focus:ring-brand-500 focus:border-brand-500" placeholder="Family Name" />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Given Name *</label>
                                        <input {...register('givenName', { required: true })} disabled={validationPassed} className="w-full px-3 py-2 border rounded focus:ring-brand-500 focus:border-brand-500" placeholder="Given Name" />
                                    </div>
                                </>
                            ) : (
                                <div className="md:col-span-2">
                                    <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Vendor Legal Name *</label>
                                    <input {...register('vendorLegalName', { required: true })} disabled={validationPassed} className="w-full px-3 py-2 border rounded focus:ring-brand-500 focus:border-brand-500" placeholder="Acme International Ltd." />
                                </div>
                            )}
                            <div className="md:col-span-2">
                                <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Primary Contact Email *</label>
                                <input type="email" {...register('primaryContactEmail', { required: true })} disabled={validationPassed} className="w-full px-3 py-2 border rounded focus:ring-brand-500 focus:border-brand-500" placeholder="contact@vendor.com" />
                            </div>
                            <div>
                                <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Country *</label>
                                <input {...register('country', { required: true })} disabled={validationPassed} className="w-full px-3 py-2 border rounded focus:ring-brand-500 focus:border-brand-500" placeholder="Country" />
                            </div>
                            {isIndividual && (
                                <div>
                                    <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Date of Birth (Optional)</label>
                                    <input type="date" {...register('dateOfBirth')} disabled={validationPassed} className="w-full px-3 py-2 border rounded focus:ring-brand-500 focus:border-brand-500" />
                                </div>
                            )}
                        </div>
                        {!validationPassed && (
                            <div className="mt-8 flex justify-end border-t pt-4">
                                <Button type="button" onClick={handleValidationSequence} isLoading={isChecking} className="bg-[#f2f7ff] text-blue-700 border border-blue-300 hover:bg-blue-100 h-8 px-8 font-bold rounded-none">
                                    Next Step
                                </Button>
                            </div>
                        )}
                    </Card>
                )}

                {/* Step 2: Review (Formerly Step 3) */}
                {(viewMode === 'full' || activeStep === 2) && categoryCommitted && validationPassed && (
                    <Card title="Review & Finalize">
                        <div className="bg-blue-50 border-l-4 border-blue-400 p-4 mb-6">
                            <p className="text-sm text-blue-700">Summary of the invitation profile. Please review before issuing the link.</p>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                            {/* Vendor Information */}
                            <div>
                                <h4 className="text-xs font-bold text-gray-400 uppercase tracking-widest mb-3">Vendor Information</h4>
                                <div className="space-y-2 text-sm">
                                    <div><span className="text-gray-500 block text-xs">Name</span><span className="font-bold">{isIndividual ? `${watch('familyName')} ${watch('givenName')}` : watch('vendorLegalName')}</span></div>
                                    <div><span className="text-gray-500 block text-xs">Email</span><span className="font-bold">{watch('primaryContactEmail')}</span></div>
                                    <div><span className="text-gray-500 block text-xs">Country</span><span className="font-bold">{watch('country')}</span></div>
                                </div>
                            </div>

                            {/* Classification */}
                            <div>
                                <h4 className="text-xs font-bold text-gray-400 uppercase tracking-widest mb-3">Classification</h4>
                                <div className="space-y-2 text-sm">
                                    <div><span className="text-gray-500 block text-xs">Type</span><span className="font-bold">{watch('vendorType')}</span></div>
                                    <div><span className="text-gray-500 block text-xs">Account Group</span><span className="font-bold">{watch('accountGroup')}</span></div>
                                    <div><span className="text-gray-500 block text-xs">Entity</span><span className="font-bold">{watch('companyCode')}</span></div>
                                </div>
                            </div>
                        </div>

                        <div className="border-t pt-6 space-y-6">
                            <h4 className="text-xs font-bold text-gray-400 uppercase tracking-widest">Additional Configuration</h4>

                            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                <div>
                                    <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Expiration *</label>
                                    <select {...register('expirationDays', { required: true })} className="w-full px-3 py-2 border rounded bg-white focus:ring-brand-500 focus:border-brand-500">
                                        <option value="7">7 days</option>
                                        <option value="14">14 days (Recommended)</option>
                                        <option value="30">30 days</option>
                                    </select>
                                </div>
                                <div className="md:col-span-2">
                                    <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Internal Notes (Optional)</label>
                                    <textarea {...register('notes')} rows={3} className="w-full px-3 py-2 border rounded focus:ring-brand-500 focus:border-brand-500" placeholder="Add internal context for this invitation..." />
                                </div>
                            </div>
                        </div>

                        <div className="mt-8 border-t pt-4">
                            <h4 className="text-xs font-bold text-gray-400 uppercase tracking-widest mb-3">Checks Summary</h4>
                            <div className="grid grid-cols-2 gap-4">
                                <div className={`p-3 rounded border ${checkResults && checkResults.length > 0 ? 'bg-orange-50 border-orange-100' : 'bg-green-50 border-green-100'}`}>
                                    <div className="flex items-center gap-2 mb-1">
                                        <AlertTriangle className={`w-3 h-3 ${checkResults && checkResults.length > 0 ? 'text-orange-600' : 'text-green-600'}`} />
                                        <span className="text-[10px] font-bold uppercase">SAP Duplicate Check</span>
                                    </div>
                                    <p className="text-xs font-bold">{checkResults && checkResults.length > 0 ? `${checkResults.length} Matches Found (Overridden)` : 'No matches found'}</p>
                                </div>
                                <div className={`p-3 rounded border ${sanctionsResult && (sanctionsResult.status === 'MatchFound' || sanctionsResult.overallRisk === 'Critical') ? 'bg-red-50 border-red-100' : 'bg-green-50 border-green-100'}`}>
                                    <div className="flex items-center gap-2 mb-1">
                                        <ShieldAlert className={`w-3 h-3 ${sanctionsResult && (sanctionsResult.status === 'MatchFound' || sanctionsResult.overallRisk === 'Critical') ? 'text-red-600' : 'text-green-600'}`} />
                                        <span className="text-[10px] font-bold uppercase">Sanctions Screening</span>
                                    </div>
                                    <p className="text-xs font-bold">{sanctionsResult && (sanctionsResult.status === 'MatchFound' || sanctionsResult.overallRisk === 'Critical') ? 'Potential Match (Overridden)' : 'Clear'}</p>
                                </div>
                            </div>
                        </div>
                    </Card>
                )}

                {/* Navigation */}
                {categoryCommitted && viewMode === 'wizard' && (
                    <div className="mt-8 flex justify-between items-center bg-gray-50 p-4 border rounded">
                        <Button type="button" variant="secondary" onClick={prevStep} disabled={activeStep === 0} className="flex items-center gap-2 px-6"><ChevronLeft className="w-4 h-4" /> Previous</Button>
                        <div className="flex gap-4">
                            {activeStep < STEPS.length - 1 ? (
                                <Button
                                    type="button"
                                    variant="primary"
                                    onClick={() => {
                                        if (activeStep === 1 && !validationPassed) {
                                            handleValidationSequence();
                                        } else {
                                            nextStep();
                                        }
                                    }}
                                    isLoading={activeStep === 1 && isChecking}
                                    className="flex items-center gap-2 px-8 font-bold"
                                >
                                    Next Step <ChevronRight className="w-4 h-4" />
                                </Button>
                            ) : (
                                <Button type="submit" isLoading={submitting} variant="primary" className="bg-brand-700 text-white px-10 font-bold">Issue Invitation Link</Button>
                            )}
                        </div>
                    </div>
                )}

                {/* Full View Actions */}
                {viewMode === 'full' && (
                    <div className="mt-8 pt-6 border-t flex justify-between items-center bg-gray-50 p-4 border rounded">
                        <Button type="button" variant="secondary" onClick={() => navigate('/admin/invitations')}>Discard</Button>
                        <Button type="submit" isLoading={submitting} variant="primary" className="bg-brand-700 text-white px-10 font-bold">Issue Invitation Link</Button>
                    </div>
                )}
                <DuplicateDetectionModal
                    isOpen={showDupModal}
                    onClose={() => setShowDupModal(false)}
                    onProceed={handleConfirmDuplicateBypass}
                    duplicates={checkResults || []}
                />
            </form>
        </div>
    );
};
