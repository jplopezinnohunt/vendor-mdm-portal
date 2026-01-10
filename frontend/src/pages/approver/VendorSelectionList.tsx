import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, Edit, Building2 } from 'lucide-react';
import { Card, Button } from '../../components/ui/Elements';
import { api } from '../../services/api';

interface Vendor {
    id: string;
    legalName: string;
    taxId: string;
    sapId?: string;
    status: string;
}

export const VendorSelectionList: React.FC = () => {
    const [vendors, setVendors] = useState<Vendor[]>([]);
    const [filteredVendors, setFilteredVendors] = useState<Vendor[]>([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        const loadVendors = async () => {
            setLoading(true);
            try {
                // Try to load vendors from API
                const response = await api.get('/vendor/list');
                setVendors(response.data || []);
                setFilteredVendors(response.data || []);
            } catch (error) {
                console.error('Error loading vendors:', error);
                // Use mock data if API fails
                const mockVendors: Vendor[] = [
                    { id: 'V001', legalName: 'Acme Corporation', taxId: 'TAX-001', sapId: 'SAP-001', status: 'Active' },
                    { id: 'V002', legalName: 'Global Tech Solutions', taxId: 'TAX-002', sapId: 'SAP-002', status: 'Active' },
                    { id: 'V003', legalName: 'International Supplies Ltd', taxId: 'TAX-003', sapId: 'SAP-003', status: 'Active' },
                    { id: 'V004', legalName: 'Premium Services Inc', taxId: 'TAX-004', sapId: 'SAP-004', status: 'Active' },
                    { id: 'V005', legalName: 'Quality Products Co', taxId: 'TAX-005', sapId: 'SAP-005', status: 'Active' },
                ];
                setVendors(mockVendors);
                setFilteredVendors(mockVendors);
            } finally {
                setLoading(false);
            }
        };

        loadVendors();
    }, []);

    useEffect(() => {
        if (searchTerm === '') {
            setFilteredVendors(vendors);
        } else {
            const filtered = vendors.filter(v =>
                v.legalName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                v.taxId.toLowerCase().includes(searchTerm.toLowerCase()) ||
                v.sapId?.toLowerCase().includes(searchTerm.toLowerCase()) ||
                v.id.toLowerCase().includes(searchTerm.toLowerCase())
            );
            setFilteredVendors(filtered);
        }
    }, [searchTerm, vendors]);

    const handleSelectVendor = (vendorId: string) => {
        // Navigate to update vendor form with vendor ID
        navigate(`/approver/update-vendor/${vendorId}`);
    };

    return (
        <div className="mx-auto max-w-7xl px-4 sm:px-6 md:px-8 py-6">
            <div className="space-y-6">
                {/* Header */}
                <div className="sm:flex sm:items-center sm:justify-between">
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">Select Vendor to Update</h1>
                        <p className="mt-2 text-sm text-gray-700">
                            Choose a vendor from the list below to update their master data.
                        </p>
                    </div>
                    <div className="mt-4 sm:mt-0">
                        <Button
                            variant="outline"
                            onClick={() => navigate('/approver/worklist')}
                        >
                            Back to Worklist
                        </Button>
                    </div>
                </div>

                {/* Search Bar */}
                <Card>
                    <div className="relative">
                        <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                            <Search className="h-5 w-5 text-gray-400" />
                        </div>
                        <input
                            type="text"
                            className="block w-full rounded-md border-gray-300 pl-10 focus:border-brand-500 focus:ring-brand-500 sm:text-sm"
                            placeholder="Search by vendor name, tax ID, SAP ID, or vendor ID..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                    </div>
                </Card>

                {/* Vendor List */}
                <Card className="px-0 py-0">
                    <div className="overflow-x-auto">
                        <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-50">
                                <tr>
                                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                                        Vendor ID
                                    </th>
                                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                                        Legal Name
                                    </th>
                                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                                        Tax ID
                                    </th>
                                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                                        SAP ID
                                    </th>
                                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                                        Status
                                    </th>
                                    <th className="relative px-6 py-3">
                                        <span className="sr-only">Actions</span>
                                    </th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-200 bg-white">
                                {loading ? (
                                    <tr>
                                        <td colSpan={6} className="px-6 py-12 text-center">
                                            <div className="flex justify-center">
                                                <div className="h-8 w-8 animate-spin rounded-full border-4 border-brand-600 border-t-transparent"></div>
                                            </div>
                                            <p className="mt-2 text-sm text-gray-500">Loading vendors...</p>
                                        </td>
                                    </tr>
                                ) : filteredVendors.length === 0 ? (
                                    <tr>
                                        <td colSpan={6} className="px-6 py-12 text-center">
                                            <Building2 className="mx-auto h-12 w-12 text-gray-400" />
                                            <p className="mt-2 text-sm font-medium text-gray-900">No vendors found</p>
                                            <p className="mt-1 text-sm text-gray-500">
                                                {searchTerm ? 'Try adjusting your search criteria' : 'No vendors available in the system'}
                                            </p>
                                        </td>
                                    </tr>
                                ) : (
                                    filteredVendors.map((vendor) => (
                                        <tr key={vendor.id} className="hover:bg-gray-50">
                                            <td className="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">
                                                {vendor.id}
                                            </td>
                                            <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-900">
                                                {vendor.legalName}
                                            </td>
                                            <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                                                {vendor.taxId}
                                            </td>
                                            <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                                                {vendor.sapId || '-'}
                                            </td>
                                            <td className="whitespace-nowrap px-6 py-4 text-sm">
                                                <span className="inline-flex items-center rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-medium text-green-800">
                                                    {vendor.status}
                                                </span>
                                            </td>
                                            <td className="whitespace-nowrap px-6 py-4 text-right text-sm font-medium">
                                                <Button
                                                    size="sm"
                                                    onClick={() => handleSelectVendor(vendor.id)}
                                                    className="flex items-center gap-2"
                                                >
                                                    <Edit className="h-3 w-3" />
                                                    Update
                                                </Button>
                                            </td>
                                        </tr>
                                    ))
                                )}
                            </tbody>
                        </table>
                    </div>
                </Card>

                {/* Results Summary */}
                {!loading && filteredVendors.length > 0 && (
                    <div className="text-sm text-gray-500 text-center">
                        Showing {filteredVendors.length} of {vendors.length} vendor{vendors.length !== 1 ? 's' : ''}
                    </div>
                )}
            </div>
        </div>
    );
};
