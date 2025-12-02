
import { VendorMasterData, ChangeRequest, ChangeRequestStatus, RequestType, VendorApplication, ApplicationStatus } from '../types';

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
    status: ChangeRequestStatus.InReview,
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
    status: ChangeRequestStatus.New,
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

  getCurrentVendor: async (): Promise<VendorMasterData> => {
    return new Promise((resolve) => {
      setTimeout(() => resolve(MOCK_VENDOR_DATA), 800);
    });
  },

  getChangeRequests: async (): Promise<ChangeRequest[]> => {
    return new Promise((resolve) => {
      // Filter for current mock user (100450)
      const myRequests = MOCK_REQUESTS_DB.filter(r => r.vendorId === '100450');
      setTimeout(() => resolve(myRequests), 600);
    });
  },

  submitChangeRequest: async (
    deltaItems: any[], 
    attachments: File[]
  ): Promise<ChangeRequest> => {
    return new Promise((resolve) => {
      const newReq: ChangeRequest = {
        id: `cr-${Date.now()}`,
        vendorId: '100450',
        requestType: RequestType.General, // Simplified logic
        status: ChangeRequestStatus.New, // Starts as NEW/Draft or IN_REVIEW based on auto-rules
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        items: deltaItems,
        attachments: [] // Mock: attachments not actually stored in simulated DB array
      };
      
      MOCK_REQUESTS_DB.unshift(newReq);
      
      setTimeout(() => resolve(newReq), 1000);
    });
  },

  // --- Approver / Admin Methods ---

  // Get Onboarding Requests
  getOnboardingRequests: async (): Promise<VendorApplication[]> => {
    return new Promise((resolve) => {
      setTimeout(() => resolve([...MOCK_ONBOARDING_DB]), 600);
    });
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

  // Get ALL requests (Worklist)
  getAllChangeRequests: async (): Promise<ChangeRequest[]> => {
    return new Promise((resolve) => {
      setTimeout(() => resolve([...MOCK_REQUESTS_DB]), 600);
    });
  },

  // Get Single Request by ID
  getChangeRequestById: async (id: string): Promise<ChangeRequest | undefined> => {
    return new Promise((resolve) => {
      const req = MOCK_REQUESTS_DB.find(r => r.id === id);
      setTimeout(() => resolve(req), 400);
    });
  },

  // Approve or Reject
  processChangeRequest: async (id: string, status: ChangeRequestStatus.Approved | ChangeRequestStatus.Rejected, comment?: string): Promise<void> => {
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
  },

  // --- Admin Config Methods ---
  
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