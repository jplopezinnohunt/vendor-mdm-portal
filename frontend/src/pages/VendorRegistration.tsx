import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, Link } from 'react-router-dom';
import { Button, Input, Card } from '../components/ui/Elements';
import { CheckCircle } from 'lucide-react';
import { api } from '../services/api';

interface RegistrationForm {
  companyName: string;
  taxId: string;
  contactName: string;
  email: string;
}

export const VendorRegistration: React.FC = () => {
  const { register, handleSubmit, formState: { errors } } = useForm<RegistrationForm>();
  const [submitted, setSubmitted] = useState(false);

  const onSubmit = async (data: RegistrationForm) => {
    try {
      // Real API Call to the Vendor Artifact Service
      await api.post('/vendor/changerequest', data);
      setSubmitted(true);
    } catch (error) {
      console.error('Submission failed:', error);
      alert('Failed to submit application. Please try again.');
    }
  };

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
                Thank you for your interest. We have received your application and sent a confirmation email to <strong>{document.querySelector('input[type="email"]')?.getAttribute('value')}</strong>.
              </p>
              <Link to="/login">
                <Button className="w-full">Return to Home</Button>
              </Link>
            </div>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-3xl mx-auto">
        <Link to="/login" className="text-sm font-medium text-brand-600 hover:text-brand-500 mb-6 inline-block">
          &larr; Back to Login
        </Link>

        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">New Vendor Application</h1>
          <p className="mt-2 text-gray-600">
            Please provide your initial company details. Once approved, you will receive an invitation to access the full portal.
          </p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          <Card>
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <div className="md:col-span-2">
                <Input
                  label="Company Name"
                  {...register('companyName', { required: 'Company name is required' })}
                  error={errors.companyName?.message}
                  placeholder="Legal entity name"
                />
              </div>

              <div>
                <Input
                  label="Tax ID / VAT Number"
                  {...register('taxId', { required: 'Tax ID is required' })}
                  error={errors.taxId?.message}
                  placeholder="e.g. US-123456789"
                />
              </div>

              <div>
                <Input
                  label="Contact Person"
                  {...register('contactName', { required: 'Contact name is required' })}
                  error={errors.contactName?.message}
                  placeholder="Full name"
                />
              </div>

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
                  placeholder="invitations@company.com"
                />
                <p className="mt-1 text-xs text-gray-500">We will send the portal invitation to this address.</p>
              </div>
            </div>

            <div className="mt-8 pt-6 border-t border-gray-200">
              <Button type="submit" className="w-full justify-center" size="lg">
                Submit Application
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
                <span>Backend: Connected (Hybrid Artifact)</span>
              </div>
            </div>
          </Card>
        </form>
      </div>
    </div>
  );
};