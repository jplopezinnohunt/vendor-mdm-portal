import { VendorMasterData, ChangeRequest, ChangeRequestStatus, RequestType, VendorApplication, ApplicationStatus } from '../types';
import { api } from './api';

// Mock data to simulate Backend/SAP ECC response
const MOCK_VENDOR_DATA: VendorMasterData = {
  sapVendorId: '100450',
  name: 'Acme Corp Global',
  legalForm: 'Inc.',
  taxNumber1: 'US123456789',
  address: {
    street: '123 Innovation Drive',
    city: 'Tech Park',
    postalCode: '94000',
    country: 'US',
    region: 'CA'
  },
  email: 'finance@acme.com',
  phone: '+1 555 0123',
  companyCode: 'UNES', // Default UNESCO Company Code
  accountGroup: 'INDV', // Default Individual / Physical Vendor
  contactPerson: 'John Doe',
  contactEmail: 'john.doe@acme.com',
  contactPhone: '+1 555 9991',
  birthDate: '1980-01-01',
  gender: 'Mr',
  profession: 'Engineer',
  birthCountry: 'US',
  banks: [
    {
      id: '1',
      bankCountry: 'US',
      bankKey: '121000248',
      bankAccount: '*******8888',
      accountHolder: 'Acme Corp',
      iban: ''
    }
  ]
};

const MOCK_VENDOR_HQSU: VendorMasterData = {
  ...MOCK_VENDOR_DATA,
  sapVendorId: '100451',
  name: 'Acme Logistics',
  accountGroup: 'HQSU',
  legalForm: 'SA',
  birthDate: undefined,
  gender: undefined
};

const MOCK_VENDOR_EVNT: VendorMasterData = {
  ...MOCK_VENDOR_DATA,
  sapVendorId: '100452',
  name: 'ConfEx Center',
  accountGroup: 'EVNT',
  events: [{ name: 'Global Summit', date: '2024-12-01' }],
  address: { ...MOCK_VENDOR_DATA.address, street: 'Exhibition Rd 1' }
};

const MOCK_VENDOR_PART: VendorMasterData = {
  ...MOCK_VENDOR_DATA,
  sapVendorId: '100453',
  name: 'Jane Smith',
  accountGroup: 'PART',
  address: { ...MOCK_VENDOR_DATA.address, country: 'FR' }, // French participant
  banks: [{ ...MOCK_VENDOR_DATA.banks[0], bankCountry: 'FR', iban: 'FR76...' }]
};

const MOCK_VENDORS_MAP: Record<string, VendorMasterData> = {
  '100450': MOCK_VENDOR_DATA,
  '100451': MOCK_VENDOR_HQSU,
  '100452': MOCK_VENDOR_EVNT,
  '100453': MOCK_VENDOR_PART
};

// Mock Onboarding Applications (Prospects)
let MOCK_ONBOARDING_DB: VendorApplication[] = [
  {
    id: 'app-001',
    companyName: 'Stark Industries',
    taxId: 'US-9990001',
    contactName: 'Pepper Potts',
    email: 'ppotts@stark.com',
    status: ApplicationStatus.Submitted,
    submittedAt: '2023-11-01T10:00:00Z',
    sanctionCheckStatus: 'Passed'
  },
  {
    id: 'app-002',
    companyName: 'Wayne Enterprises',
    taxId: 'US-9990002',
    contactName: 'Lucius Fox',
    email: 'lfox@wayne.com',
    status: ApplicationStatus.Submitted,
    submittedAt: '2023-11-05T14:00:00Z',
    sanctionCheckStatus: 'Pending'
  }
];

// Global Mock DB for requests to allow interactions between Vendor and Approver
let MOCK_REQUESTS_DB: ChangeRequest[] = [
  {
    id: 'cr-001',
    vendorId: '100450',
    requestType: RequestType.Address,
    status: ChangeRequestStatus.Applied,
    createdAt: '2023-10-01T10:00:00Z',
    updatedAt: '2023-10-02T14:30:00Z',
    items: [],
    attachments: []
  },
  {
    id: 'cr-002',
    vendorId: '100450',
    requestType: RequestType.BankData,
    status: ChangeRequestStatus.UnderReview,
    createdAt: '2023-10-25T09:15:00Z',
    updatedAt: '2023-10-25T09:15:00Z',
    items: [
      {
        id: 'item-1',
        tableName: 'LFBK',
        fieldName: 'BANKN',
        oldValue: '*******8888',
        newValue: '123456789',
        isSensitive: true
      }
    ],
    attachments: [
      {
        id: 'att-1',
        fileName: 'bank_confirmation.pdf',
        mimeType: 'application/pdf',
        uploadedAt: '2023-10-25T09:15:00Z',
        category: 'BANK_LETTER'
      }
    ]
  },
  {
    id: 'cr-003',
    vendorId: '200999',
    requestType: RequestType.General,
    status: ChangeRequestStatus.Draft,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    items: [
      {
        id: 'item-2',
        tableName: 'LFA1',
        fieldName: 'NAME1',
        oldValue: 'Globex Corp',
        newValue: 'Globex Corporation Int.',
        isSensitive: false
      }
    ],
    attachments: []
  }
];

export const VendorService = {
  // --- Vendor Methods ---

  getCurrentVendor: async (vendorId: string = '100450'): Promise<VendorMasterData> => {
    // Strategy: Always try API first if we are in MOCK or FINAL mode, or pointing to a real URL
    try {
      // Logic: If explicitly MOCK/FINAL, or if we just want to try the connection
      const response = await api.get(`/changerequest/vendor/${vendorId}`);
      const data = response.data;

      // Map Backend DTO to Frontend Type
      return {
        sapVendorId: data.vendorId || vendorId,
        name: data.name || 'Acme Corp',
        legalForm: 'Inc.',
        taxNumber1: 'US123456789',
        address: {
          street: data.address || '123 Innovation Drive',
          city: 'Tech Park',
          postalCode: '94000',
          country: 'US'
        },
        email: 'finance@acme.com',
        phone: '+1 555 0123',
        companyCode: 'UNES',
        accountGroup: 'INDV',
        contactPerson: 'John Doe',
        contactEmail: 'john.doe@acme.com',
        contactPhone: '+1 555 9991',
        birthDate: data.birthDate || '1980-01-01',
        gender: data.gender || 'Mr',
        profession: data.profession || 'Engineer',
        birthCountry: data.birthCountry || 'US',
        banks: MOCK_VENDOR_DATA.banks
      };
    } catch (error) {
      // Only fallback if we are NOT in strict FINAL mode (optional, but safer to always fallback in dev)
      console.warn('Backend Service unreachable. If you expected an Azure Mock response, check connection.', error);

      return new Promise((resolve) => {
        const mock = MOCK_VENDORS_MAP[vendorId] || { ...MOCK_VENDOR_DATA, sapVendorId: vendorId };
        setTimeout(() => resolve(mock), 800);
      });
    }
  },

  searchVendors: async (query: string): Promise<VendorMasterData[]> => {
    // 1. Try Real Backend Search
    try {
      const response = await api.get(`/vendor/search?query=${query}`);
      // Map backend vendor model to frontend model if necessary
      return response.data;
    } catch (error) {
      console.warn('Backend search unreachable, falling back to mock', error);

      // 2. Fallback to Mock Logic
      return new Promise((resolve) => {
        setTimeout(() => {
          const allMockVendors = Object.values(MOCK_VENDORS_MAP);
          const results = allMockVendors.filter(v =>
            v.name.toLowerCase().includes(query.toLowerCase()) ||
            v.sapVendorId.includes(query)
          );
          resolve(results);
        }, 600);
      });
    }
  },

  getChangeRequests: async (): Promise<ChangeRequest[]> => {
    try {
      const response = await api.get('/changerequest/vendor/100450'); // Uses new /changerequest/vendor/{id} route
      return response.data;
    } catch (error) {
      console.warn('Backend unreachable, using Mock Data for Change Requests', error);
      return new Promise((resolve) => {
        const myRequests = MOCK_REQUESTS_DB.filter(r => r.vendorId === '100450');
        setTimeout(() => resolve(myRequests), 600);
      });
    }
  },

  submitChangeRequest: async (
    deltaItems: any[],
    attachments: File[],
    vendorId: string = '100450'
  ): Promise<ChangeRequest> => {
    try {
      const payload = {
        requesterId: '00000000-0000-0000-0000-000000000001',
        sapVendorId: vendorId,
        payload: { items: deltaItems }
      };
      const response = await api.post('/changerequest', payload);

      const backendReq = response.data;
      return {
        id: backendReq.id,
        vendorId: backendReq.sapVendorId || '100450',
        requestType: RequestType.General,
        status: ChangeRequestStatus.Draft,
        createdAt: backendReq.createdAt,
        updatedAt: backendReq.updatedAt || backendReq.createdAt,
        items: deltaItems,
        attachments: []
      };
    } catch (error) {
      console.warn('Backend unreachable, simulating Submit', error);
      return new Promise((resolve) => {
        const newReq: ChangeRequest = {
          id: `cr-${Date.now()}`,
          vendorId: '100450',
          requestType: RequestType.General,
          status: ChangeRequestStatus.Draft,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
          items: deltaItems,
          attachments: []
        };
        MOCK_REQUESTS_DB.unshift(newReq);
        setTimeout(() => resolve(newReq), 1000);
      });
    }
  },

  // --- Approver / Admin Methods ---

  getOnboardingRequests: async (): Promise<VendorApplication[]> => {
    try {
      const response = await api.get('/review/pending');
      return response.data.map((item: any) => ({
        id: item.id,
        companyName: item.companyName,
        taxId: 'N/A',
        contactName: item.contactName || item.contactEmail,
        email: item.contactEmail,
        status: ApplicationStatus.Submitted,
        submittedAt: item.createdAt,
        sanctionCheckStatus: 'Pending'
      }));
    } catch (error) {
      console.warn('Backend unreachable, using Mock Data for Onboarding', error);
      return new Promise((resolve) => {
        setTimeout(() => resolve([...MOCK_ONBOARDING_DB]), 600);
      });
    }
  },

  getOnboardingRequestById: async (id: string): Promise<VendorApplication | undefined> => {
    return new Promise((resolve) => {
      const app = MOCK_ONBOARDING_DB.find(a => a.id === id);
      setTimeout(() => resolve(app), 400);
    });
  },

  processOnboardingRequest: async (id: string, status: ApplicationStatus.Approved | ApplicationStatus.Rejected): Promise<void> => {
    return new Promise((resolve) => {
      const idx = MOCK_ONBOARDING_DB.findIndex(a => a.id === id);
      if (idx >= 0) {
        MOCK_ONBOARDING_DB[idx] = {
          ...MOCK_ONBOARDING_DB[idx],
          status: status
        };
      }
      setTimeout(resolve, 800);
    });
  },

  getAllChangeRequests: async (): Promise<ChangeRequest[]> => {
    return new Promise((resolve) => {
      setTimeout(() => resolve([...MOCK_REQUESTS_DB]), 600);
    });
  },

  getChangeRequestById: async (id: string): Promise<ChangeRequest | undefined> => {
    try {
      const response = await api.get(`/changerequest/${id}`);
      return response.data;
    } catch (error) {
      console.warn('Backend unreachable, using Mock Data for Request Details', error);
      return new Promise((resolve) => {
        const req = MOCK_REQUESTS_DB.find(r => r.id === id);
        setTimeout(() => resolve(req), 400);
      });
    }
  },

  processChangeRequest: async (id: string, status: ChangeRequestStatus.Approved | ChangeRequestStatus.Rejected, comment?: string): Promise<void> => {
    try {
      if (status === ChangeRequestStatus.Approved) {
        await api.post(`/changerequest/${id}/approve`, {});
      } else {
        throw new Error("Reject not implemented");
      }
    } catch (error) {
      console.warn('Backend unreachable, simulating Process Request', error);
      return new Promise((resolve) => {
        const reqIndex = MOCK_REQUESTS_DB.findIndex(r => r.id === id);
        if (reqIndex >= 0) {
          MOCK_REQUESTS_DB[reqIndex] = {
            ...MOCK_REQUESTS_DB[reqIndex],
            status: status,
            updatedAt: new Date().toISOString()
          };
        }
        setTimeout(resolve, 800);
      });
    }
  },

  getWorkflowRules: async (): Promise<string> => {
    const mockRules = {
      rules: [
        { field: 'BANKN', risk: 'HIGH', approvers: 2 },
        { field: 'STRAS', risk: 'LOW', approvers: 1 }
      ]
    };
    return JSON.stringify(mockRules, null, 2);
  }
};