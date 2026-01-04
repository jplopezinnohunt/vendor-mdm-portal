import React, { useState, useMemo } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { Button, Card } from '../../components/ui/Elements';
import { useAuth } from '../../context/AuthContext';
import { api } from '../../services/api';
import { CheckCircle, ChevronRight, ChevronLeft, Layout, Settings, FileText, User, Globe, Landmark, AlertTriangle, ShieldAlert } from 'lucide-react';
import { DuplicateDetectionModal } from '../../components/DuplicateDetectionModal';
import { VendorHeaderPanel, WorkflowStatus } from '../../components/VendorHeaderPanel';
import { CollapsibleSection } from '../../components/ui/CollapsibleSection';
import { FileUpload } from '../../components/ui/FileUpload';
import { AttachmentMetadata } from '../../types/vendor';

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
    { id: 'definition', title: 'Definition', icon: Settings },
    { id: 'main-data', title: 'Main Data', icon: User },
    { id: 'profile', title: 'Profile', icon: Globe },
    { id: 'financial', title: 'Financial', icon: Landmark },
    { id: 'review', title: 'Review', icon: FileText },
];

export const CreateVendorForm: React.FC = () => {
    const navigate = useNavigate();
    const { user } = useAuth();
    const [submitting, setSubmitting] = useState(false);
    const [submitted, setSubmitted] = useState(false);

    // States for duplicate check flow
    const [isChecking, setIsChecking] = useState(false);
    const [checkResults, setCheckResults] = useState<any[] | null>(null);
    const [validationPassed, setValidationPassed] = useState(false); // This now means sanctions passed
    const [categoryCommitted, setCategoryCommitted] = useState(false);
    const [showDupModal, setShowDupModal] = useState(false);
    const [sanctionsResult, setSanctionsResult] = useState<any>(null);
    const [isScreening, setIsScreening] = useState(false);
    const [validationTimestamp, setValidationTimestamp] = useState<string | null>(null);

    // Header panel states
    const [requestId, setRequestId] = useState<string | null>(null);
    const [lastModification, setLastModification] = useState<string | null>(null);
    const [sapNumber, setSapNumber] = useState<string | null>(null);

    // Wizard state
    const [activeStep, setActiveStep] = useState(0);
    const [viewMode, setViewMode] = useState<'wizard' | 'full'>('wizard');

    const { register, handleSubmit, watch, setValue, formState: { errors } } = useForm({
        defaultValues: {
            name: '',
            acronym: '',
            legalStatus: '',
            givenName: '',
            familyName: '',
            dateOfBirth: '',
            country: '',
            taxNumber1: '',
            email: '',
            vendorType: 'Physical',
            accountGroup: 'INDV',
            companyCode: 'UNES',
            gender: '',
            countryOfBirth: '',
            profession: '',
            houseNo: '',
            street1: '',
            street2: '',
            postalCode: '',
            city: '',
            telephone: '',
            mobilePhone: '',
            fax: '',
            paymentEmail: '',
            sapLanguage: 'EN',
            taxCode1: '',
            taxCode2: '',
            nationality: '',
            currency: 'EUR',
            bankName: '',
            bankAbbr: '',
            bankAgency: '',
            bankAddress: '',
            bankCity: '',
            bankCountry: '',
            bankAccHolder: '',
            bankCurrency: '',
            bankInfo: '',
            bankAccNum: '',
            bankControlKey: '',
            bankIban: '',
            bankSwift: '',
            registrationDate: '',
            bankBranch: '',
            attributes: {}
        }
    });

    const selectedVendorType = watch('vendorType');
    const isIndividual = selectedVendorType === 'Physical' || selectedVendorType === 'Participant';

    // Computed values for header panel
    const vendorName = useMemo(() => {
        const data = watch();
        if (isIndividual) {
            const family = data.familyName || '';
            const given = data.givenName || '';
            if (family && given) {
                return `${family.toUpperCase()} ${given}`;
            }
            return family || given || '';
        }
        return data.name || '';
    }, [watch('familyName'), watch('givenName'), watch('name'), isIndividual]);

    const workflowStatus: WorkflowStatus = useMemo(() => {
        if (!categoryCommitted) return 'draft';
        if (!validationPassed) return 'in-progress';
        if (validationPassed && !submitted) return 'validation';
        return 'completed';
    }, [categoryCommitted, validationPassed, submitted]);

    const lastSavedByUser = user?.name || user?.email || 'Unknown User';


    const handleValidationSequence = async () => {
        const data = watch();
        const searchQuery = isIndividual ? `${data.familyName} ${data.givenName}` : data.name;

        if (!searchQuery) {
            alert('Please enter identification data to perform the check');
            return;
        }

        setIsChecking(true);
        try {
            // 1. SAP Duplicate Check
            const dupResponse = await api.post('sap/vendor/search', {
                vendorType: isIndividual ? 'INDV' : 'COMP',
                familyName: isIndividual ? data.familyName : undefined,
                givenName: isIndividual ? data.givenName : undefined,
                companyName: !isIndividual ? data.name : undefined,
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
        const searchQuery = isIndividual ? `${data.familyName} ${data.givenName}` : data.name;

        setIsChecking(true); // Reuse checking state for spinner
        try {
            const sanctionsResponse = await api.post('sanctions/screen', {
                entityName: searchQuery,
                entityType: isIndividual ? 'Individual' : 'Organization',
                country: data.country,
                vendorId: "NEW_VENDOR"
            });
            setSanctionsResult(sanctionsResponse.data);
            setValidationPassed(true);

            if (viewMode === 'wizard') {
                setActiveStep(2); // Move to Profile Step
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

    const nextStep = () => setActiveStep((prev) => Math.min(prev + 1, STEPS.length - 1));
    const prevStep = () => setActiveStep((prev) => Math.max(prev - 1, 0));

    const onSubmit = async (data: any) => {
        setSubmitting(true);
        try {
            const isIndividual = data.vendorType === 'Physical' || data.vendorType === 'Participant';
            const finalName = isIndividual
                ? `${data.familyName}, ${data.givenName}`
                : data.name;

            const payload = {
                LegalName: finalName,
                TaxId: data.taxCode1 || data.taxNumber1,
                PrimaryContactEmail: data.email,
                Status: 'Active',
                SourceSystem: 'MDM_INTERNAL',
                Data: JSON.stringify({
                    ...data,
                    fullName: finalName,
                    validationMetadata: {
                        timestamp: new Date().toISOString(),
                        validator: user?.email,
                        matchCount: checkResults?.length || 0
                    }
                })
            };
            await api.post('/vendor', payload);

            // Update header panel metadata after successful save
            const now = new Date();
            const year = now.getFullYear();
            const sequence = Math.floor(Math.random() * 99999).toString().padStart(5, '0'); // In production, this should come from backend
            setRequestId(`MV-${year}-${sequence}`);
            setLastModification(now.toLocaleString());

            setSubmitted(true);
        } catch (error: any) {
            console.error('Failed to create vendor:', error);
            alert(error.userMessage || 'Failed to create vendor');
        } finally {
            setSubmitting(false);
        }
    };

    if (submitted) {
        return (
            <div className="max-w-2xl mx-auto py-12">
                <Card>
                    <div className="text-center py-8">
                        <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-green-100 mb-6">
                            <CheckCircle className="h-10 w-10 text-green-600" />
                        </div>
                        <h2 className="text-3xl font-bold text-gray-900 mb-4">Vendor Created Successfully!</h2>
                        <p className="text-gray-600 mb-8 max-w-md mx-auto">
                            The vendor record has been created directly in the master data hub and is now active for transactions.
                        </p>
                        <div className="flex flex-col gap-3">
                            <Button onClick={() => setSubmitted(false)} variant="primary" className="w-full justify-center">
                                Create Another Vendor
                            </Button>
                            <Button onClick={() => navigate('/approver/worklist')} variant="secondary" className="w-full justify-center">
                                Back to Dashboard
                            </Button>
                        </div>
                    </div>
                </Card>
            </div>
        );
    }

    return (
        <div className="max-w-4xl mx-auto py-8">
            <div className="mb-8 flex flex-col md:flex-row md:items-end justify-between gap-4">
                <div>
                    <h1 className="text-3xl font-bold text-gray-900 border-b-2 border-brand-500 pb-2 inline-block">Direct Vendor Creation</h1>
                    <p className="mt-4 text-gray-600 text-sm">
                        Create a new vendor record directly in the system.
                    </p>
                </div>

                {/* View Mode Toggle */}
                <div className="flex bg-gray-100 p-1 rounded-lg self-start md:self-auto">
                    <button
                        type="button"
                        onClick={() => setViewMode('wizard')}
                        className={`flex items-center gap-2 px-3 py-1.5 rounded-md text-xs font-bold transition-all ${viewMode === 'wizard' ? 'bg-white text-brand-700 shadow-sm' : 'text-gray-500 hover:text-gray-700'}`}
                    >
                        <Layout className="w-3.5 h-3.5" />
                        Guided Wizard
                    </button>
                    <button
                        type="button"
                        onClick={() => setViewMode('full')}
                        className={`flex items-center gap-2 px-3 py-1.5 rounded-md text-xs font-bold transition-all ${viewMode === 'full' ? 'bg-white text-brand-700 shadow-sm' : 'text-gray-500 hover:text-gray-700'}`}
                    >
                        <FileText className="w-3.5 h-3.5" />
                        Full View
                    </button>
                </div>
            </div>

            {/* Header Information Panel - Only show when category is committed */}
            {categoryCommitted && (
                <VendorHeaderPanel
                    requestId={requestId}
                    vendorName={vendorName}
                    workflow={workflowStatus}
                    lastModification={lastModification}
                    sapNumber={sapNumber}
                    lastSavedBy={lastSavedByUser}
                    companyCode={watch('companyCode') || 'UNES'}
                    dutyStation={undefined}
                    isDraft={!submitted}
                />
            )}

            {viewMode === 'wizard' && (
                <div className="mb-8">
                    <div className="flex items-center justify-between relative px-2">
                        {/* Connecting Line */}
                        <div className="absolute top-1/2 left-0 w-full h-0.5 bg-gray-200 -translate-y-1/2 z-0" />
                        <div
                            className="absolute top-1/2 left-0 h-0.5 bg-brand-500 -translate-y-1/2 z-0 transition-all duration-500"
                            style={{ width: `${(activeStep / (STEPS.length - 1)) * 100}%` }}
                        />

                        {STEPS.map((step, index) => {
                            const Icon = step.icon;
                            const isCompleted = activeStep > index;
                            const isActive = activeStep === index;

                            return (
                                <div key={step.id} className="relative z-10 flex flex-col items-center group cursor-pointer" onClick={() => (index < activeStep || categoryCommitted) && setActiveStep(index)}>
                                    <div className={`w-10 h-10 rounded-full flex items-center justify-center transition-all duration-300 border-2 ${isCompleted ? 'bg-brand-600 border-brand-600 text-white shadow-lg' :
                                        isActive ? 'bg-white border-brand-500 text-brand-700 shadow-md ring-4 ring-brand-50' :
                                            'bg-gray-50 border-gray-200 text-gray-400'
                                        }`}>
                                        {isCompleted ? <CheckCircle className="w-6 h-6" /> : <Icon className="w-5 h-5" />}
                                    </div>
                                    <span className={`absolute -bottom-6 text-[10px] font-bold uppercase tracking-wider whitespace-nowrap transition-colors ${isActive ? 'text-brand-700' : isCompleted ? 'text-brand-600' : 'text-gray-400'
                                        }`}>
                                        {step.title}
                                    </span>
                                </div>
                            );
                        })}
                    </div>
                </div>
            )}

            <form onSubmit={handleSubmit(onSubmit)} className="space-y-8 mt-12">
                {/* Step 0: Definition */}
                {(viewMode === 'full' || activeStep === 0) && (
                    <div className="transition-all duration-500 ease-in-out">
                        {!categoryCommitted ? (
                            <Card>
                                <div className="flex flex-col md:flex-row gap-6 items-center justify-between">
                                    <div className="flex-1">
                                        <label className="block text-sm font-bold text-gray-700 uppercase tracking-wider mb-2">
                                            1. Select Vendor Category
                                        </label>
                                        <p className="text-xs text-gray-500 mb-4">
                                            This choice determines the required identification and legal fields for the new record.
                                        </p>
                                        <div className="flex gap-2">
                                            {['Physical', 'Company', 'Meeting', 'Participant'].map((type) => (
                                                <button
                                                    key={type}
                                                    type="button"
                                                    onClick={() => {
                                                        setValue('vendorType', type);
                                                        setValue('accountGroup', '');
                                                        setValue('companyCode', '');
                                                    }}
                                                    className={`px-4 py-2 text-sm font-bold border transition-all ${selectedVendorType === type
                                                        ? 'bg-brand-600 text-white border-brand-600 shadow-md transform scale-105'
                                                        : 'bg-gray-50 text-gray-600 border-gray-200 hover:bg-white hover:border-brand-300'
                                                        }`}
                                                >
                                                    {type === 'Physical' ? 'Individual' : type === 'Company' ? 'Company' : type}
                                                </button>
                                            ))}
                                        </div>

                                        {selectedVendorType && (
                                            <div className="mt-6 animate-in fade-in slide-in-from-left duration-300">
                                                <label className="block text-sm font-bold text-gray-700 uppercase tracking-wider mb-2">
                                                    2. Select Account Group
                                                </label>
                                                <div className="flex flex-wrap gap-2">
                                                    {(ACCOUNT_GROUP_OPTIONS[selectedVendorType] || []).map((opt) => (
                                                        <button
                                                            key={opt.value}
                                                            type="button"
                                                            onClick={() => {
                                                                setValue('accountGroup', opt.value);
                                                                setValue('companyCode', '');
                                                            }}
                                                            className={`px-3 py-1.5 text-xs font-medium border transition-all ${watch('accountGroup') === opt.value
                                                                ? 'bg-blue-600 text-white border-blue-600 shadow-sm'
                                                                : 'bg-white text-gray-600 border-gray-300 hover:border-blue-400'
                                                                }`}
                                                        >
                                                            {opt.label}
                                                        </button>
                                                    ))}
                                                </div>
                                            </div>
                                        )}

                                        {watch('accountGroup') && (
                                            <div className="mt-6 animate-in fade-in slide-in-from-left duration-300">
                                                <label className="block text-sm font-bold text-gray-700 uppercase tracking-wider mb-2">
                                                    3. Select UNESCO Entity (Company Code)
                                                </label>
                                                <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
                                                    {COMPANY_CODE_OPTIONS.map((cc) => (
                                                        <button
                                                            key={cc.value}
                                                            type="button"
                                                            onClick={() => setValue('companyCode', cc.value)}
                                                            className={`px-3 py-2 text-left text-xs font-medium border transition-all ${watch('companyCode') === cc.value
                                                                ? 'bg-brand-100 border-brand-500 text-brand-900 border-l-4'
                                                                : 'bg-white text-gray-600 border-gray-200 hover:border-brand-300'
                                                                }`}
                                                        >
                                                            {cc.label}
                                                        </button>
                                                    ))}
                                                </div>
                                            </div>
                                        )}
                                    </div>
                                    <div className="w-full md:w-64 border-l pl-6 hidden md:block">
                                        <label className="block text-[10px] font-bold text-gray-400 uppercase mb-1">Process Flow Guide</label>
                                        <div className="flex flex-col gap-3">
                                            <div className="flex items-center gap-2">
                                                <div className={`w-4 h-4 rounded-full flex items-center justify-center text-[8px] font-bold ${selectedVendorType ? 'bg-green-500 text-white' : 'bg-gray-200 text-gray-400'}`}>1</div>
                                                <span className={`text-[10px] font-bold ${selectedVendorType ? 'text-gray-900' : 'text-gray-400'}`}>Category selection</span>
                                            </div>
                                            <div className="flex items-center gap-2">
                                                <div className={`w-4 h-4 rounded-full flex items-center justify-center text-[8px] font-bold ${watch('accountGroup') ? 'bg-green-500 text-white' : 'bg-gray-200 text-gray-400'}`}>2</div>
                                                <span className={`text-[10px] font-bold ${watch('accountGroup') ? 'text-gray-900' : 'text-gray-400'}`}>Account Group selection</span>
                                            </div>
                                            <div className="flex items-center gap-2">
                                                <div className={`w-4 h-4 rounded-full flex items-center justify-center text-[8px] font-bold ${watch('companyCode') ? 'bg-green-500 text-white' : 'bg-gray-200 text-gray-400'}`}>3</div>
                                                <span className={`text-[10px] font-bold ${watch('companyCode') ? 'text-gray-900' : 'text-gray-400'}`}>Company Code selection</span>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <div className="mt-6 flex justify-end border-t pt-4">
                                    <Button
                                        type="button"
                                        disabled={!selectedVendorType || !watch('accountGroup') || !watch('companyCode')}
                                        onClick={() => {
                                            setCategoryCommitted(true);
                                            if (viewMode === 'wizard') nextStep();
                                        }}
                                        className={`px-8 font-bold rounded-none ${(!selectedVendorType || !watch('accountGroup') || !watch('companyCode')) ? 'bg-gray-200 text-gray-400 cursor-not-allowed' : 'bg-brand-700 text-white hover:bg-brand-800'}`}
                                    >
                                        Confirm & Start Record
                                    </Button>
                                </div>
                            </Card>
                        ) : (
                            <div className="bg-brand-700 text-white p-4 shadow-lg border-b-4 border-brand-800 flex items-center justify-between">
                                <div className="flex items-center gap-6">
                                    <div className="flex flex-col">
                                        <span className="text-[10px] font-bold uppercase text-brand-300 tracking-widest">Category</span>
                                        <span className="text-sm font-bold tracking-wider">{selectedVendorType}</span>
                                    </div>
                                    <div className="h-8 w-px bg-brand-700 hidden lg:block" />
                                    <div className="hidden lg:flex flex-col">
                                        <span className="text-[10px] font-bold uppercase text-brand-300 tracking-widest">Group</span>
                                        <span className="text-xs font-medium text-brand-100 max-w-[150px] truncate">
                                            {(ACCOUNT_GROUP_OPTIONS[selectedVendorType] || []).find(o => o.value === watch('accountGroup'))?.label || watch('accountGroup')}
                                        </span>
                                    </div>
                                    <div className="h-8 w-px bg-brand-700 hidden lg:block" />
                                    <div className="hidden lg:flex flex-col">
                                        <span className="text-[10px] font-bold uppercase text-brand-300 tracking-widest">UNESCO Entity</span>
                                        <span className="text-sm font-bold tracking-wider">{watch('companyCode') || 'UNES'}</span>
                                    </div>
                                </div>
                                <button
                                    type="button"
                                    onClick={() => {
                                        setCategoryCommitted(false);
                                        setActiveStep(0);
                                    }}
                                    className="bg-brand-600 hover:bg-brand-500 text-white px-3 py-1 text-[10px] font-bold uppercase tracking-tighter transition-colors"
                                >
                                    Change Base Selection
                                </button>
                            </div>
                        )}
                    </div>
                )}

                {/* Step 1: Main Data */}
                {(viewMode === 'full' || activeStep === 1) && categoryCommitted && (
                    <Card title={isIndividual ? "Main Data: Individual person" : "Main Data: Company or Organization"}>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-4">
                            {isIndividual ? (
                                <>
                                    <div>
                                        <label className="block text-sm font-medium text-blue-700 mb-1 flex items-center gap-1">Family name *</label>
                                        <input {...register('familyName', { required: isIndividual })} disabled={checkResults !== null} className="w-full px-3 py-1 border border-gray-300 rounded focus:ring-0 focus:border-blue-500 uppercase text-sm" placeholder="Family name" />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-blue-700 mb-1 flex items-center gap-1">Given name *</label>
                                        <input {...register('givenName', { required: isIndividual })} disabled={checkResults !== null} className="w-full px-3 py-1 border border-gray-300 rounded focus:ring-0 focus:border-blue-500 text-sm" placeholder="Given name" />
                                    </div>
                                    <div className="hidden md:block" />
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">Date of Birth</label>
                                        <input type="date" {...register('dateOfBirth')} disabled={checkResults !== null} className="w-full px-3 py-1 border border-gray-300 rounded focus:ring-0 focus:border-blue-500 text-sm" />
                                    </div>
                                </>
                            ) : (
                                <>
                                    <div className="md:col-span-2">
                                        <label className="block text-sm font-medium text-blue-700 mb-1 flex items-center gap-1">Legal name / Entity Name *</label>
                                        <input {...register('name', { required: !isIndividual })} disabled={checkResults !== null} className="w-full px-3 py-1 border border-gray-300 rounded focus:ring-0 focus:border-blue-500 text-sm" placeholder="Official company name" />
                                    </div>
                                    <div className="hidden md:block" />
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1 flex items-center gap-1">Country *</label>
                                        <select {...register('country', { required: !isIndividual })} disabled={checkResults !== null} className="w-full px-3 py-1 border border-gray-300 rounded focus:ring-0 focus:border-blue-500 text-sm bg-white">
                                            <option value="">Choose country</option>
                                            <option value="FR">France</option>
                                            <option value="CH">Switzerland</option>
                                            <option value="US">United States</option>
                                        </select>
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-blue-700 mb-1 flex items-center gap-1">Tax Identification Number</label>
                                        <input {...register('taxNumber1')} disabled={checkResults !== null} className="w-full px-3 py-1 border border-gray-300 rounded focus:ring-0 focus:border-blue-500 text-sm" placeholder="Tax ID / Registration No." />
                                    </div>
                                </>
                            )}
                        </div>

                        {!validationPassed && (
                            <div className="mt-6 flex justify-end items-center border-t pt-4">
                                <Button type="button" onClick={handleValidationSequence} isLoading={isChecking} className="bg-[#f2f7ff] text-blue-700 border border-blue-300 hover:bg-blue-100 h-8 px-8 font-bold rounded-none">
                                    Next Step
                                </Button>
                            </div>
                        )}
                    </Card>
                )}

                {/* Remove standalone SAP and Sanctions steps from wizard/full view */}

                {/* Step 2: Profile */}
                {(viewMode === 'full' || activeStep === 2) && categoryCommitted && validationPassed && (
                    <div className="animate-in fade-in slide-in-from-bottom duration-500 space-y-8">
                        {isIndividual ? (
                            <div className="border border-[#4a7ec5] rounded overflow-hidden">
                                <div className="bg-[#4a7ec5] text-white px-4 py-1 font-bold text-sm flex items-center gap-2">
                                    👤 Personal details ({selectedVendorType})
                                </div>
                                <div className="p-6 bg-white space-y-6">
                                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                                        <div>
                                            <label className="block text-xs font-medium text-blue-700 mb-1">Gender *</label>
                                            <select {...register('gender')} className="w-full px-2 py-1 border rounded text-sm"><option value="">Choose title</option><option value="Mr">Mr.</option><option value="Ms">Ms.</option></select>
                                        </div>
                                        <div>
                                            <label className="block text-xs font-medium text-blue-700 mb-1">Family name</label>
                                            <div className="flex gap-2">
                                                <input {...register('familyName')} disabled className="flex-1 px-2 py-1 border rounded bg-gray-100 text-sm uppercase" />
                                                <button type="button" onClick={() => setValidationPassed(false)} className="px-2 py-1 bg-[#f2f7ff] border border-blue-300 rounded text-blue-700 text-[10px] font-bold">Update</button>
                                            </div>
                                        </div>
                                        <div>
                                            <label className="block text-xs font-medium text-blue-700 mb-1">Given name</label>
                                            <input {...register('givenName')} disabled className="w-full px-2 py-1 border rounded bg-gray-100 text-sm" />
                                        </div>
                                    </div>
                                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                                        <div><label className="block text-xs font-medium text-blue-700 mb-1">Date of Birth *</label><input type="date" {...register('dateOfBirth')} className="w-full px-2 py-1 border rounded text-sm" /></div>
                                        <div><label className="block text-xs font-medium text-blue-700 mb-1">Country of Birth *</label><select {...register('countryOfBirth')} className="w-full px-2 py-1 border rounded text-sm bg-white"><option value="">Choose country</option><option value="FR">France</option></select></div>
                                        <div><label className="block text-xs font-medium text-blue-700 mb-1">Profession *</label><input {...register('profession')} className="w-full px-2 py-1 border rounded text-sm" /></div>
                                    </div>

                                    {/* Personal Identity Documents - Categorized */}
                                    <div className="mt-6 pt-6 border-t">
                                        <h4 className="text-sm font-bold text-gray-700 mb-4">📎 Personal Identity Documents</h4>

                                        {/* Passport */}
                                        <div className="mb-6">
                                            <h5 className="text-xs font-semibold text-blue-700 mb-2">🛂 Passport</h5>
                                            <p className="text-xs text-gray-500 mb-2">Upload passport copy (Max 1 file)</p>
                                            <FileUpload
                                                onFilesChange={(files) => {
                                                    console.log('Passport uploaded:', files);
                                                    // TODO: Store with category='passport'
                                                }}
                                                maxFiles={1}
                                                maxSizeBytes={10 * 1024 * 1024}
                                            />
                                        </div>

                                        {/* National ID / Residence Permit */}
                                        <div className="mb-6">
                                            <h5 className="text-xs font-semibold text-blue-700 mb-2">🪪 National ID / Residence Permit</h5>
                                            <p className="text-xs text-gray-500 mb-2">Upload national ID or residence permit (Max 2 files - front & back)</p>
                                            <FileUpload
                                                onFilesChange={(files) => {
                                                    console.log('National ID uploaded:', files);
                                                    // TODO: Store with category='national-id'
                                                }}
                                                maxFiles={2}
                                                maxSizeBytes={10 * 1024 * 1024}
                                            />
                                        </div>

                                        {/* Certificates & Other Documents */}
                                        <div className="mb-4">
                                            <h5 className="text-xs font-semibold text-blue-700 mb-2">📜 Certificates & Other Documents</h5>
                                            <p className="text-xs text-gray-500 mb-2">Professional certificates, diplomas, tax documents, etc. (Max 5 files)</p>
                                            <FileUpload
                                                onFilesChange={(files) => {
                                                    console.log('Certificates uploaded:', files);
                                                    // TODO: Store with category='certificates'
                                                }}
                                                maxFiles={5}
                                                maxSizeBytes={10 * 1024 * 1024}
                                            />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        ) : (
                            <div className="border border-[#4a7ec5] rounded overflow-hidden">
                                <div className="bg-[#4a7ec5] text-white px-4 py-1 font-bold text-sm flex items-center gap-2">
                                    🏢 Organization details ({selectedVendorType})
                                </div>
                                <div className="p-6 bg-white space-y-6">
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                        <div>
                                            <label className="block text-xs font-medium text-blue-700 mb-1">Company Legal Name</label>
                                            <div className="flex gap-2">
                                                <input {...register('name')} disabled className="flex-1 px-2 py-1 border rounded bg-gray-100 text-sm" />
                                                <button type="button" onClick={() => setValidationPassed(false)} className="px-2 py-1 bg-[#f2f7ff] border border-blue-300 rounded text-blue-700 text-[10px] font-bold">Update</button>
                                            </div>
                                        </div>
                                        <div><label className="block text-xs font-medium text-blue-700 mb-1">Acronym / Short name</label><input {...register('acronym')} className="w-full px-2 py-1 border rounded text-sm" /></div>
                                        <div><label className="block text-xs font-medium text-blue-700 mb-1">Legal Status *</label><select {...register('legalStatus')} className="w-full px-2 py-1 border rounded text-sm bg-white"><option value="">Choose status</option><option value="PRIVATE">Private Company</option><option value="PUBLIC">Public Entity</option></select></div>
                                        <div><label className="block text-xs font-medium text-blue-700 mb-1">Date of Registration</label><input type="date" {...register('registrationDate')} className="w-full px-2 py-1 border rounded text-sm" /></div>
                                    </div>
                                </div>
                            </div>
                        )}

                        {/* Address */}
                        <CollapsibleSection title="Address" defaultExpanded={true}>
                            <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
                                <div className="lg:col-span-1"><label className="block text-xs font-medium text-blue-700 mb-1">House No.</label><input {...register('houseNo')} className="w-full px-2 py-1 border rounded text-sm" /></div>
                                <div className="lg:col-span-3"><label className="block text-xs font-medium text-blue-700 mb-1">Street Name *</label><input {...register('street1')} className="w-full px-2 py-1 border rounded text-sm" /></div>
                                <div className="lg:col-span-4"><label className="block text-xs font-medium text-gray-500 mb-1">Street Name 2</label><input {...register('street2')} placeholder="Building name or additional address line" className="w-full px-2 py-1 border rounded text-sm" /></div>
                                <div className="lg:col-span-4"><label className="block text-xs font-medium text-gray-500 mb-1">Street Name 3</label><input {...register('street3')} placeholder="Additional address line" className="w-full px-2 py-1 border rounded text-sm" /></div>
                                <div className="lg:col-span-4"><label className="block text-xs font-medium text-gray-500 mb-1">Street Name 4</label><input {...register('street4')} placeholder="Additional address line" className="w-full px-2 py-1 border rounded text-sm" /></div>
                                <div className="lg:col-span-1"><label className="block text-xs font-medium text-gray-500 mb-1">Postal Code</label><input {...register('postalCode')} className="w-full px-2 py-1 border rounded text-sm" /></div>
                                <div className="lg:col-span-2"><label className="block text-xs font-medium text-blue-700 mb-1">City *</label><input {...register('city')} className="w-full px-2 py-1 border rounded text-sm" /></div>
                                <div className="lg:col-span-1"><label className="block text-xs font-medium text-blue-700 mb-1">Country *</label><select {...register('country')} className="w-full px-2 py-1 border rounded text-sm bg-white"><option value="">Choose country</option><option value="FR">France</option></select></div>
                            </div>
                        </CollapsibleSection>

                        {/* Contact */}
                        <CollapsibleSection title="Contact Information" defaultExpanded={true}>
                            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                                <div><label className="block text-xs font-medium text-gray-500 mb-1">Telephone</label><input {...register('telephone')} className="w-full px-2 py-1 border rounded text-sm" /></div>
                                <div><label className="block text-xs font-medium text-gray-500 mb-1">Mobile Phone</label><input {...register('mobilePhone')} className="w-full px-2 py-1 border rounded text-sm" /></div>
                                <div><label className="block text-xs font-medium text-gray-500 mb-1">Fax</label><input {...register('fax')} className="w-full px-2 py-1 border rounded text-sm" placeholder="+1 234 567 8900" /></div>
                                <div className="md:col-span-2"><label className="block text-xs font-medium text-blue-700 mb-1">Email *</label><input {...register('email')} className="w-full px-2 py-1 border rounded text-sm" /></div>
                                <div><label className="block text-xs font-medium text-gray-500 mb-1">Email for Payments</label><input {...register('paymentEmail')} placeholder="accounting@company.com" className="w-full px-2 py-1 border rounded text-sm" /></div>
                            </div>
                        </CollapsibleSection>

                        {/* Various / Other Details */}
                        <CollapsibleSection title="Other Details" defaultExpanded={false}>
                            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                                <div><label className="block text-xs font-medium text-blue-700 mb-1">SAP Language *</label><select {...register('sapLanguage')} className="w-full px-2 py-1 border rounded text-sm bg-white"><option value="EN">English</option><option value="FR">French</option></select></div>
                                <div><label className="block text-xs font-medium text-blue-700 mb-1">Currency *</label><select {...register('currency')} className="w-full px-2 py-1 border rounded text-sm bg-white"><option value="EUR">EUR</option><option value="USD">USD</option></select></div>
                            </div>
                        </CollapsibleSection>


                    </div>
                )}

                {/* Step 3: Financial */}
                {(viewMode === 'full' || activeStep === 3) && categoryCommitted && validationPassed && (
                    <div className="animate-in fade-in slide-in-from-bottom duration-500 space-y-8">
                        <div className="border border-[#4a7ec5] rounded overflow-hidden">
                            <div className="bg-[#4a7ec5] text-white px-4 py-1 font-bold text-sm flex items-center gap-2">🏦 Bank Information</div>
                            <div className="p-6 bg-white space-y-6">
                                <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
                                    <div><label className="block text-xs font-medium text-blue-700 mb-1">Bank Name *</label><input {...register('bankName')} className="w-full px-2 py-1 border rounded text-sm" /></div>
                                    <div><label className="block text-xs font-medium text-blue-700 mb-1">Account Holder *</label><input {...register('bankAccHolder')} className="w-full px-2 py-1 border rounded text-sm" /></div>
                                    <div><label className="block text-xs font-medium text-blue-700 mb-1">IBAN *</label><input {...register('bankIban')} className="w-full px-2 py-1 border rounded text-sm font-mono" /></div>
                                    <div><label className="block text-xs font-medium text-blue-700 mb-1">SWIFT *</label><input {...register('bankSwift')} className="w-full px-2 py-1 border rounded text-sm font-mono" /></div>
                                </div>
                            </div>
                        </div>
                    </div>
                )}

                {/* Step 4: Review */}
                {(viewMode === 'full' || activeStep === 4) && categoryCommitted && validationPassed && (
                    <Card title="Final Review">
                        <div className="space-y-4">
                            <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 font-medium text-yellow-700">Verification Required: Please review all information.</div>
                            <div className="grid grid-cols-2 gap-4 text-sm mt-4">
                                <div className="p-3 bg-gray-50 rounded"><span className="text-[10px] font-bold text-gray-400 uppercase block">Name</span><span className="font-bold">{isIndividual ? `${watch('familyName')}, ${watch('givenName')}` : watch('name')}</span></div>
                                <div className="p-3 bg-gray-50 rounded"><span className="text-[10px] font-bold text-gray-400 uppercase block">Email</span><span className="font-bold">{watch('email')}</span></div>
                            </div>

                            <div className="mt-6 border-t pt-4">
                                <h4 className="text-xs font-bold text-gray-400 uppercase tracking-widest mb-3">Validation Summary</h4>
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
                        </div>
                    </Card>
                )}

                {/* Navigation */}
                {categoryCommitted && viewMode === 'wizard' && (
                    <div className="mt-8 flex justify-between items-center bg-gray-50 p-4 border rounded">
                        <Button type="button" variant="secondary" onClick={prevStep} disabled={activeStep === 0} className="flex items-center gap-2"><ChevronLeft className="w-4 h-4" /> Previous</Button>
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
                                    className="flex items-center gap-2"
                                >
                                    Next Step <ChevronRight className="w-4 h-4" />
                                </Button>
                            ) : (
                                <Button type="submit" isLoading={submitting} variant="primary" className="bg-brand-700 text-white px-8 font-bold">Final Save and Submit</Button>
                            )}
                        </div>
                    </div>
                )}

                {/* Full View Actions */}
                {viewMode === 'full' && (
                    <div className="mt-8 pt-6 border-t flex justify-between items-center bg-gray-50 p-4 border rounded">
                        <Button type="button" variant="secondary" onClick={() => navigate('/approver/worklist')}>Discard</Button>
                        <div className="flex gap-4">
                            <Button type="button" variant="secondary">Save as draft</Button>
                            <Button type="submit" isLoading={submitting} variant="primary" className="bg-brand-700 text-white px-8 font-bold">Final Save and Submit</Button>
                        </div>
                    </div>
                )}
            </form>

            <DuplicateDetectionModal
                isOpen={showDupModal}
                onClose={() => setShowDupModal(false)}
                onProceed={handleConfirmDuplicateBypass}
                duplicates={checkResults || []}
                newVendorData={{
                    name: watch('name'),
                    familyName: watch('familyName'),
                    givenName: watch('givenName'),
                    dateOfBirth: watch('dateOfBirth'),
                    country: watch('country'),
                    accountGroup: watch('accountGroup'),
                    companyCode: watch('companyCode')
                }}
            />
        </div>
    );
};
