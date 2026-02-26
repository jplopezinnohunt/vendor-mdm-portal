import React, { useState } from 'react';
import { Card } from '../../components/ui/Elements';
import {
    Share2,
    Server,
    Shield,
    Key,
    Mail,
    Database,
    Cloud,
    CheckCircle,
    AlertTriangle,
    Copy,
    ChevronDown,
    ChevronRight,
    ExternalLink,
    Code,
    Layers,
    Lock,
    Clock,
    Workflow,
    Globe,
    Cpu,
    HardDrive,
} from 'lucide-react';

// --- Types ---

type TabId = 'overview' | 'api' | 'azure' | 'code' | 'recommendations';

interface AzureResource {
    name: string;
    type: string;
    purpose: string;
    tier: string;
    icon: React.ElementType;
    color: string;
    required: boolean;
    configKeys: string[];
}

interface ApiEndpoint {
    method: 'GET' | 'POST';
    path: string;
    auth: 'Public' | 'ApproverOnly';
    description: string;
    requestBody?: string;
    responseBody: string;
    stage: string;
}

// --- Data ---

const AZURE_RESOURCES: AzureResource[] = [
    {
        name: 'Azure SQL Database',
        type: 'Microsoft.Sql/servers/databases',
        purpose: 'Stores VendorInvitation records, tokens, MFA codes, status, and VendorApplication data',
        tier: 'Basic / S0 Standard',
        icon: Database,
        color: 'blue',
        required: true,
        configKeys: [
            'ConnectionStrings:SqlConnection',
        ],
    },
    {
        name: 'Azure App Service',
        type: 'Microsoft.Web/sites',
        purpose: 'Hosts the .NET 8 API with invitation endpoints. The Drupal site calls these endpoints server-to-server',
        tier: 'B1 Basic+',
        icon: Server,
        color: 'green',
        required: true,
        configKeys: [
            'AZURE_WEBAPP_NAME',
            'ASPNETCORE_ENVIRONMENT',
        ],
    },
    {
        name: 'Azure Cosmos DB',
        type: 'Microsoft.DocumentDB/databaseAccounts',
        purpose: 'Stores invitation artifacts (full payload), domain events for audit trail',
        tier: 'Serverless',
        icon: Cloud,
        color: 'purple',
        required: false,
        configKeys: [
            'CosmosDb:ConnectionString',
            'CosmosDb:DatabaseName',
        ],
    },
    {
        name: 'Azure Communication Services / SMTP',
        type: 'Microsoft.Communication/communicationServices',
        purpose: 'Sends invitation emails and MFA verification codes. Primary channel for the 2FA flow',
        tier: 'Pay-as-you-go',
        icon: Mail,
        color: 'orange',
        required: true,
        configKeys: [
            'EmailService:Smtp:Host',
            'EmailService:Smtp:Port',
            'EmailService:Smtp:FromEmail',
            'EmailService:Smtp:FromName',
            'EmailService:AzureFunctionUrl',
        ],
    },
    {
        name: 'Azure App Service Plan',
        type: 'Microsoft.Web/serverfarms',
        purpose: 'Compute plan for the API. Needs AlwaysOn for production to prevent cold-start delays on MFA emails',
        tier: 'B1 Basic (min for AlwaysOn)',
        icon: Cpu,
        color: 'indigo',
        required: true,
        configKeys: [],
    },
    {
        name: 'Azure Key Vault',
        type: 'Microsoft.KeyVault/vaults',
        purpose: 'Stores connection strings, SMTP credentials, and API keys securely. Referenced via App Service configuration',
        tier: 'Standard',
        icon: Lock,
        color: 'red',
        required: true,
        configKeys: [
            'KeyVault:VaultUri',
        ],
    },
];

const API_ENDPOINTS: ApiEndpoint[] = [
    {
        method: 'GET',
        path: '/api/invitation/validate/{token}',
        auth: 'Public',
        description: 'Validate token existence, expiration, and status. Call this when the vendor clicks the invitation link.',
        responseBody: `{
  "isValid": true,
  "invitationId": "guid",
  "vendorLegalName": "Acme Corp",
  "primaryContactEmail": "vendor@acme.com",
  "status": "Pending",
  "currentStage": "InvitationSent",
  "expiresAt": "2026-03-12T00:00:00Z"
}`,
        stage: 'Step 1',
    },
    {
        method: 'POST',
        path: '/api/invitation/trigger-mfa/{token}',
        auth: 'Public',
        description: 'Sends a 6-digit MFA code to the vendor\'s email. Call this after token validation succeeds.',
        responseBody: `{ "message": "MFA code sent" }`,
        stage: 'Step 2',
    },
    {
        method: 'POST',
        path: '/api/invitation/verify-mfa/{token}',
        auth: 'Public',
        description: 'Verify the 6-digit code entered by the vendor. On success, stage advances to MfaVerified.',
        requestBody: `{ "code": "123456" }`,
        responseBody: `{
  "success": true,
  "currentStage": "MfaVerified",
  "attributes": { /* saved form data */ },
  "vendorLegalName": "Acme Corp",
  "primaryContactEmail": "vendor@acme.com",
  "expiresAt": "2026-03-12T00:00:00Z"
}`,
        stage: 'Step 3',
    },
    {
        method: 'POST',
        path: '/api/invitation/submit-initial/{token}',
        auth: 'Public',
        description: 'Submit initial vendor information (company name confirmation, basic contact info).',
        requestBody: `{
  "companyName": "Acme Corp",
  "contactName": "John Doe",
  "contactEmail": "vendor@acme.com"
}`,
        responseBody: `{ "message": "Initial information saved" }`,
        stage: 'Step 4',
    },
    {
        method: 'POST',
        path: '/api/invitation/submit-enrichment/{token}',
        auth: 'Public',
        description: 'Submit enrichment data (address, banking, tax info). Flexible key-value payload.',
        requestBody: `{
  "address": { "street": "...", "city": "...", "country": "..." },
  "bankDetails": { "iban": "...", "bic": "..." },
  "taxId": "DE123456789"
}`,
        responseBody: `{ "message": "Enrichment data saved" }`,
        stage: 'Step 5',
    },
    {
        method: 'POST',
        path: '/api/invitation/complete/{token}',
        auth: 'Public',
        description: 'Complete registration. Creates a VendorApplication, runs sanctions screening, and marks invitation as Completed.',
        requestBody: `{
  "companyName": "Acme Corp",
  "contactName": "John Doe",
  "email": "vendor@acme.com",
  "taxId": "DE123456789",
  "attributes": {
    "address": { ... },
    "bankDetails": { ... }
  }
}`,
        responseBody: `{
  "applicationId": "guid",
  "status": "PendingReview",
  "message": "Your application has been submitted and is under review."
}`,
        stage: 'Step 6',
    },
    {
        method: 'POST',
        path: '/api/invitation/save-draft/{token}',
        auth: 'Public',
        description: 'Save form progress as draft. Vendor can return later and resume via MFA re-verification.',
        requestBody: `{
  "companyName": "Acme Corp",
  "contactName": "John Doe",
  "email": "vendor@acme.com",
  "attributes": { /* partial form data */ }
}`,
        responseBody: `{
  "applicationId": "guid",
  "status": "Draft",
  "message": "Application saved as draft."
}`,
        stage: 'Optional',
    },
    {
        method: 'POST',
        path: '/api/invitation/create',
        auth: 'ApproverOnly',
        description: 'Create a new invitation. Generates token, stores record, sends email. Requires Bearer token with Approver role.',
        requestBody: `{
  "vendorLegalName": "Acme Corporation",
  "primaryContactEmail": "contact@acme.com",
  "vendorType": "Physical|Company|Meeting|Participant",
  "expirationDays": 14,
  "accountGroup": "Z001",
  "currency": "USD",
  "sapLanguage": "EN"
}`,
        responseBody: `{
  "invitationId": "guid",
  "invitationToken": "secure-token",
  "invitationLink": "/invitation/register/{token}",
  "expiresAt": "2026-03-12T00:00:00Z",
  "emailSent": true
}`,
        stage: 'Admin',
    },
];

const CODE_SNIPPETS = {
    tokenGeneration: {
        label: 'Token Generation (C# — InvitationService.cs:678)',
        language: 'csharp',
        code: `private string GenerateSecureToken()
{
    var bytes = new byte[32];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(bytes);
    return Convert.ToBase64String(bytes)
        .Replace("+", "-")
        .Replace("/", "_")
        .Replace("=", ""); // URL-safe Base64
}`,
        phpEquivalent: `// PHP equivalent
function generateSecureToken(): string {
    $bytes = random_bytes(32);
    return rtrim(strtr(base64_encode($bytes), '+/', '-_'), '=');
}`,
    },
    mfaGeneration: {
        label: 'MFA Code Generation (C# — InvitationService.cs:347)',
        language: 'csharp',
        code: `// Generate 6-digit code
var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

// Store in invitation attributes
attributes["mfaCode"] = code;
attributes["mfaExpires"] = DateTime.UtcNow.AddMinutes(15);

// Send via email
await _emailService.SendMfaCodeEmailAsync(
    invitation.PrimaryContactEmail,
    invitation.VendorLegalName,
    code
);`,
        phpEquivalent: `// PHP equivalent
$code = str_pad((string) random_int(100000, 999999), 6, '0', STR_PAD_LEFT);

// Store in DB
$invitation->mfa_code = $code;
$invitation->mfa_expires_at = now()->addMinutes(15);
$invitation->save();

// Send via Drupal Mail API
\\Drupal::service('plugin.manager.mail')
    ->mail('vendor_invitation', 'mfa_code', $email, 'en', [
        'code' => $code,
        'vendor_name' => $vendorName,
    ]);`,
    },
    mfaVerification: {
        label: 'MFA Verification (C# — InvitationService.cs:372)',
        language: 'csharp',
        code: `public async Task<VerifyMfaResponse> VerifyMfaCodeAsync(string token, string code)
{
    var invitation = await _context.VendorInvitations
        .FirstOrDefaultAsync(i => i.InvitationToken == token);
    if (invitation == null)
        return new VerifyMfaResponse { Success = false, Message = "Invalid token" };

    var attributes = JsonSerializer
        .Deserialize<Dictionary<string, object>>(invitation.Attributes);

    // Check code exists
    if (!attributes.TryGetValue("mfaCode", out var storedCodeObj) ||
        !attributes.TryGetValue("mfaExpires", out var expiresObj))
        return new VerifyMfaResponse { Success = false, Message = "No MFA code active" };

    // Check expiration
    var expires = DateTime.Parse(expiresObj.ToString()!);
    if (DateTime.UtcNow > expires)
        return new VerifyMfaResponse { Success = false, Message = "Code expired" };

    // Compare codes
    if (storedCodeObj.ToString() != code)
        return new VerifyMfaResponse { Success = false, Message = "Invalid code" };

    // Success — advance stage, clear code
    invitation.CurrentStage = InvitationStage.MfaVerified;
    attributes.Remove("mfaCode");
    attributes.Remove("mfaExpires");
    invitation.Attributes = JsonSerializer.Serialize(attributes);
    await _context.SaveChangesAsync();

    return new VerifyMfaResponse { Success = true };
}`,
        phpEquivalent: `// PHP equivalent (Drupal service)
public function verifyMfaCode(string $token, string $code): array {
    $invitation = \\Drupal::database()->select('vendor_invitations', 'vi')
        ->fields('vi')
        ->condition('invitation_token', $token)
        ->execute()
        ->fetchObject();

    if (!$invitation) {
        return ['success' => false, 'message' => 'Invalid token'];
    }

    $attrs = json_decode($invitation->attributes, true) ?? [];

    if (empty($attrs['mfaCode']) || empty($attrs['mfaExpires'])) {
        return ['success' => false, 'message' => 'No MFA code active'];
    }

    if (new \\DateTime('now', new \\DateTimeZone('UTC'))
        > new \\DateTime($attrs['mfaExpires'])) {
        return ['success' => false, 'message' => 'Code expired'];
    }

    if ($attrs['mfaCode'] !== $code) {
        return ['success' => false, 'message' => 'Invalid code'];
    }

    // Success
    unset($attrs['mfaCode'], $attrs['mfaExpires']);
    \\Drupal::database()->update('vendor_invitations')
        ->fields([
            'current_stage' => 'MfaVerified',
            'attributes' => json_encode($attrs),
        ])
        ->condition('invitation_token', $token)
        ->execute();

    return ['success' => true];
}`,
    },
    stateMachine: {
        label: 'Invitation State Machine (C# — InvitationStatus.cs)',
        language: 'csharp',
        code: `// Status values
Pending → Accepted → Completed    (happy path)
Pending → Expired                  (timeout)
Pending → Cancelled                (admin cancel)
Expired → Resent → Accepted        (resend flow)

// Valid transitions
Pending   → { Accepted, Expired, Cancelled, Resent }
Accepted  → { Completed, Cancelled }
Expired   → { Pending, Resent }
Completed → { }  // Terminal
Cancelled → { }  // Terminal
Resent    → { Accepted, Expired, Cancelled }

// Stages (within Accepted status)
InvitationSent → MfaVerified → InitialInfoCompleted → Enriched`,
        phpEquivalent: `// PHP equivalent
const VALID_TRANSITIONS = [
    'Pending'   => ['Accepted', 'Expired', 'Cancelled', 'Resent'],
    'Accepted'  => ['Completed', 'Cancelled'],
    'Expired'   => ['Pending', 'Resent'],
    'Completed' => [],
    'Cancelled' => [],
    'Resent'    => ['Accepted', 'Expired', 'Cancelled'],
];

function isValidTransition(string $from, string $to): bool {
    return in_array($to, self::VALID_TRANSITIONS[$from] ?? [], true);
}`,
    },
    dbSchema: {
        label: 'Database Schema (SQL — VendorInvitation table)',
        language: 'sql',
        code: `CREATE TABLE VendorInvitations (
    Id                   UNIQUEIDENTIFIER  PRIMARY KEY DEFAULT NEWID(),
    InvitationToken      NVARCHAR(100)     NOT NULL UNIQUE,
    VendorLegalName      NVARCHAR(200)     NOT NULL,
    PrimaryContactEmail  NVARCHAR(255)     NOT NULL,
    InvitedBy            UNIQUEIDENTIFIER  NOT NULL,
    InvitedByName        NVARCHAR(200)     NOT NULL,
    CreatedAt            DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt            DATETIME2         NOT NULL,
    Status               NVARCHAR(20)      NOT NULL DEFAULT 'Pending',
    CompletedAt          DATETIME2         NULL,
    VendorApplicationId  UNIQUEIDENTIFIER  NULL,
    EventId              UNIQUEIDENTIFIER  NULL,
    VendorType           NVARCHAR(50)      NOT NULL,
    AccountGroup         NVARCHAR(10)      NOT NULL,
    SanctionsStatus      NVARCHAR(20)      NOT NULL DEFAULT 'NotScreened',
    SanctionsScore       DECIMAL(5,2)      NULL,
    ReviewStatus         NVARCHAR(20)      NOT NULL DEFAULT 'NotRequired',
    CurrentStage         NVARCHAR(50)      NOT NULL DEFAULT 'InvitationSent',
    Attributes           NVARCHAR(MAX)     NOT NULL DEFAULT '{}',
    -- Attributes JSON stores: mfaCode, mfaExpires, notes, customFields,
    -- currency, sapLanguage, taxCode1, taxCode2, permittedPayee
);

CREATE INDEX IX_VendorInvitations_Token ON VendorInvitations (InvitationToken);
CREATE INDEX IX_VendorInvitations_Email ON VendorInvitations (PrimaryContactEmail);
CREATE INDEX IX_VendorInvitations_Status ON VendorInvitations (Status);`,
        phpEquivalent: `-- Drupal hook_schema() equivalent
function vendor_invitation_schema() {
  $schema['vendor_invitations'] = [
    'fields' => [
      'id' => ['type' => 'varchar', 'length' => 36, 'not null' => TRUE],
      'invitation_token' => ['type' => 'varchar', 'length' => 100, 'not null' => TRUE],
      'vendor_legal_name' => ['type' => 'varchar', 'length' => 200, 'not null' => TRUE],
      'primary_contact_email' => ['type' => 'varchar', 'length' => 255, 'not null' => TRUE],
      'status' => ['type' => 'varchar', 'length' => 20, 'default' => 'Pending'],
      'current_stage' => ['type' => 'varchar', 'length' => 50, 'default' => 'InvitationSent'],
      'expires_at' => ['type' => 'int', 'size' => 'big', 'not null' => TRUE],
      'attributes' => ['type' => 'text', 'size' => 'big', 'default' => '{}'],
    ],
    'primary key' => ['id'],
    'unique keys' => ['token' => ['invitation_token']],
    'indexes' => ['status' => ['status'], 'email' => ['primary_contact_email']],
  ];
  return $schema;
}`,
    },
    emailTemplate: {
        label: 'MFA Email Template (HTML — EmailService.cs:382)',
        language: 'html',
        code: `<!-- MFA Verification Code Email -->
<div style="max-width:600px;margin:0 auto;font-family:Arial,sans-serif">
  <div style="background:linear-gradient(135deg,#1e40af,#3b82f6);
              padding:32px;text-align:center;border-radius:8px 8px 0 0">
    <h1 style="color:#fff;margin:0;font-size:24px">
      Identity Verification
    </h1>
  </div>
  <div style="padding:32px;background:#fff;border:1px solid #e5e7eb">
    <p>Your verification code is:</p>
    <div style="text-align:center;margin:24px 0">
      <div style="display:inline-block;padding:16px 32px;
                  background:#f0f9ff;border:2px dashed #3b82f6;
                  border-radius:8px;font-size:42px;
                  font-family:monospace;letter-spacing:8px;
                  color:#1e40af;font-weight:bold">
        {{CODE}}
      </div>
    </div>
    <div style="background:#fef3c7;border-left:4px solid #f59e0b;
                padding:12px;border-radius:4px;margin-top:16px">
      <strong>&#9200; This code will expire in 15 minutes</strong>
    </div>
  </div>
</div>`,
        phpEquivalent: `// Drupal: Use the same HTML template in a Twig file
// templates/vendor-invitation-mfa.html.twig
//
// Then in your mail plugin:
// $message['body'][] = [
//   '#theme' => 'vendor_invitation_mfa',
//   '#code' => $params['code'],
// ];`,
    },
};

// --- Helpers ---

const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
};

const CodeBlock: React.FC<{ code: string; label?: string; language?: string }> = ({ code, label, language }) => {
    const [copied, setCopied] = useState(false);

    const handleCopy = () => {
        copyToClipboard(code);
        setCopied(true);
        setTimeout(() => setCopied(false), 2000);
    };

    return (
        <div className="relative group">
            {label && (
                <div className="flex items-center justify-between bg-gray-800 px-4 py-2 rounded-t-lg">
                    <span className="text-xs font-medium text-gray-400">{label}</span>
                    {language && <span className="text-xs text-gray-500">{language}</span>}
                </div>
            )}
            <pre className={`bg-gray-900 text-gray-100 text-sm p-4 overflow-x-auto ${label ? 'rounded-b-lg' : 'rounded-lg'}`}>
                <code>{code}</code>
            </pre>
            <button
                onClick={handleCopy}
                className="absolute top-2 right-2 p-1.5 rounded bg-gray-700 text-gray-300 opacity-0 group-hover:opacity-100 transition-opacity hover:bg-gray-600"
                title="Copy to clipboard"
            >
                {copied ? <CheckCircle className="h-4 w-4 text-green-400" /> : <Copy className="h-4 w-4" />}
            </button>
        </div>
    );
};

const CollapsibleSection: React.FC<{
    title: string;
    children: React.ReactNode;
    defaultOpen?: boolean;
    badge?: string;
}> = ({ title, children, defaultOpen = false, badge }) => {
    const [isOpen, setIsOpen] = useState(defaultOpen);

    return (
        <div className="border border-gray-200 rounded-lg overflow-hidden">
            <button
                onClick={() => setIsOpen(!isOpen)}
                className="w-full flex items-center justify-between px-4 py-3 bg-gray-50 hover:bg-gray-100 transition-colors text-left"
            >
                <div className="flex items-center gap-2">
                    {isOpen ? <ChevronDown className="h-4 w-4 text-gray-500" /> : <ChevronRight className="h-4 w-4 text-gray-500" />}
                    <span className="font-medium text-gray-900 text-sm">{title}</span>
                </div>
                {badge && (
                    <span className="text-xs px-2 py-0.5 rounded-full bg-brand-100 text-brand-700 font-medium">{badge}</span>
                )}
            </button>
            {isOpen && <div className="p-4 bg-white">{children}</div>}
        </div>
    );
};

// --- Tabs ---

const TABS: { id: TabId; label: string; icon: React.ElementType }[] = [
    { id: 'overview', label: 'Flow Overview', icon: Workflow },
    { id: 'api', label: 'API Endpoints', icon: Globe },
    { id: 'azure', label: 'Azure Resources', icon: Cloud },
    { id: 'code', label: 'Code Handoff', icon: Code },
    { id: 'recommendations', label: 'Recommendations', icon: Shield },
];

// --- Main Component ---

export const IntegrationHandoff: React.FC = () => {
    const [activeTab, setActiveTab] = useState<TabId>('overview');

    return (
        <div className="space-y-6">
            {/* Header */}
            <div>
                <div className="flex items-center gap-3">
                    <div className="rounded-lg bg-brand-100 p-2">
                        <Share2 className="h-6 w-6 text-brand-600" />
                    </div>
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">Drupal Integration Handoff</h1>
                        <p className="mt-0.5 text-sm text-gray-500">
                            Invitation + Email MFA authentication flow — API spec, code, and Azure resources for external developer integration
                        </p>
                    </div>
                </div>
            </div>

            {/* Summary Cards */}
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5">
                <div className="rounded-lg border border-blue-200 bg-blue-50 p-4">
                    <div className="flex items-center gap-2">
                        <Globe className="h-5 w-5 text-blue-600" />
                        <span className="text-xs font-medium text-blue-700">API Endpoints</span>
                    </div>
                    <p className="mt-2 text-2xl font-bold text-blue-900">8</p>
                    <p className="text-xs text-blue-600">6 public, 2 admin</p>
                </div>
                <div className="rounded-lg border border-green-200 bg-green-50 p-4">
                    <div className="flex items-center gap-2">
                        <Shield className="h-5 w-5 text-green-600" />
                        <span className="text-xs font-medium text-green-700">MFA Type</span>
                    </div>
                    <p className="mt-2 text-2xl font-bold text-green-900">Email</p>
                    <p className="text-xs text-green-600">6-digit code, 15 min TTL</p>
                </div>
                <div className="rounded-lg border border-purple-200 bg-purple-50 p-4">
                    <div className="flex items-center gap-2">
                        <Key className="h-5 w-5 text-purple-600" />
                        <span className="text-xs font-medium text-purple-700">Token</span>
                    </div>
                    <p className="mt-2 text-2xl font-bold text-purple-900">32-byte</p>
                    <p className="text-xs text-purple-600">Base64URL, 14-day expiry</p>
                </div>
                <div className="rounded-lg border border-orange-200 bg-orange-50 p-4">
                    <div className="flex items-center gap-2">
                        <Cloud className="h-5 w-5 text-orange-600" />
                        <span className="text-xs font-medium text-orange-700">Azure Resources</span>
                    </div>
                    <p className="mt-2 text-2xl font-bold text-orange-900">6</p>
                    <p className="text-xs text-orange-600">5 required, 1 optional</p>
                </div>
                <div className="rounded-lg border border-indigo-200 bg-indigo-50 p-4">
                    <div className="flex items-center gap-2">
                        <Layers className="h-5 w-5 text-indigo-600" />
                        <span className="text-xs font-medium text-indigo-700">Stages</span>
                    </div>
                    <p className="mt-2 text-2xl font-bold text-indigo-900">4</p>
                    <p className="text-xs text-indigo-600">Invite → MFA → Info → Done</p>
                </div>
            </div>

            {/* Tabs */}
            <div className="border-b border-gray-200">
                <nav className="-mb-px flex space-x-8 overflow-x-auto">
                    {TABS.map((tab) => (
                        <button
                            key={tab.id}
                            onClick={() => setActiveTab(tab.id)}
                            className={`flex items-center gap-2 border-b-2 py-4 px-1 text-sm font-medium whitespace-nowrap ${
                                activeTab === tab.id
                                    ? 'border-brand-500 text-brand-600'
                                    : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700'
                            }`}
                        >
                            <tab.icon className="h-4 w-4" />
                            {tab.label}
                        </button>
                    ))}
                </nav>
            </div>

            {/* Tab Content */}
            {activeTab === 'overview' && <OverviewTab />}
            {activeTab === 'api' && <ApiTab />}
            {activeTab === 'azure' && <AzureTab />}
            {activeTab === 'code' && <CodeTab />}
            {activeTab === 'recommendations' && <RecommendationsTab />}
        </div>
    );
};

// --- Tab: Overview ---

const OverviewTab: React.FC = () => (
    <div className="space-y-6">
        {/* Integration Architecture */}
        <Card title="Integration Architecture">
            <div className="p-4">
                <div className="rounded-lg bg-gray-50 border border-gray-200 p-6">
                    <h3 className="text-sm font-semibold text-gray-700 mb-4">Recommended: Microservice Architecture</h3>
                    <div className="flex flex-col lg:flex-row items-center gap-6">
                        {/* Drupal Side */}
                        <div className="flex-1 rounded-lg border-2 border-blue-300 bg-blue-50 p-4 w-full">
                            <h4 className="text-sm font-bold text-blue-900 mb-3 flex items-center gap-2">
                                <Globe className="h-4 w-4" /> Drupal Application
                            </h4>
                            <ul className="space-y-2 text-xs text-blue-800">
                                <li className="flex items-start gap-2">
                                    <CheckCircle className="h-3.5 w-3.5 text-blue-500 mt-0.5 shrink-0" />
                                    <span>Renders registration form (Form API)</span>
                                </li>
                                <li className="flex items-start gap-2">
                                    <CheckCircle className="h-3.5 w-3.5 text-blue-500 mt-0.5 shrink-0" />
                                    <span>Routes <code className="bg-blue-100 px-1 rounded">/invitation/register/{'{'}{'{'}token{'}'}{'}'}</code></span>
                                </li>
                                <li className="flex items-start gap-2">
                                    <CheckCircle className="h-3.5 w-3.5 text-blue-500 mt-0.5 shrink-0" />
                                    <span>Calls .NET API via <code className="bg-blue-100 px-1 rounded">\Drupal::httpClient()</code></span>
                                </li>
                                <li className="flex items-start gap-2">
                                    <CheckCircle className="h-3.5 w-3.5 text-blue-500 mt-0.5 shrink-0" />
                                    <span>Displays MFA input and form wizard</span>
                                </li>
                            </ul>
                        </div>

                        {/* Arrow */}
                        <div className="flex flex-col items-center gap-1 text-gray-400">
                            <span className="text-xs font-medium">HTTPS</span>
                            <div className="hidden lg:block text-2xl">&harr;</div>
                            <div className="lg:hidden text-2xl">&varr;</div>
                            <span className="text-xs font-medium">REST API</span>
                        </div>

                        {/* .NET API Side */}
                        <div className="flex-1 rounded-lg border-2 border-green-300 bg-green-50 p-4 w-full">
                            <h4 className="text-sm font-bold text-green-900 mb-3 flex items-center gap-2">
                                <Server className="h-4 w-4" /> .NET 8 API (Azure)
                            </h4>
                            <ul className="space-y-2 text-xs text-green-800">
                                <li className="flex items-start gap-2">
                                    <Shield className="h-3.5 w-3.5 text-green-500 mt-0.5 shrink-0" />
                                    <span>Token generation + validation</span>
                                </li>
                                <li className="flex items-start gap-2">
                                    <Shield className="h-3.5 w-3.5 text-green-500 mt-0.5 shrink-0" />
                                    <span>MFA code generation + verification</span>
                                </li>
                                <li className="flex items-start gap-2">
                                    <Shield className="h-3.5 w-3.5 text-green-500 mt-0.5 shrink-0" />
                                    <span>Email dispatch (invitation + MFA)</span>
                                </li>
                                <li className="flex items-start gap-2">
                                    <Shield className="h-3.5 w-3.5 text-green-500 mt-0.5 shrink-0" />
                                    <span>State machine + audit logging</span>
                                </li>
                                <li className="flex items-start gap-2">
                                    <Shield className="h-3.5 w-3.5 text-green-500 mt-0.5 shrink-0" />
                                    <span>Sanctions screening on completion</span>
                                </li>
                            </ul>
                        </div>
                    </div>
                </div>
            </div>
        </Card>

        {/* Sequence Flow */}
        <Card title="Invitation + MFA Sequence">
            <div className="p-4">
                <div className="space-y-0">
                    {[
                        { step: '1', label: 'Admin creates invitation', detail: 'POST /api/invitation/create → generates 32-byte token, stores in SQL, sends email', actor: 'Admin', color: 'purple' },
                        { step: '2', label: 'Vendor clicks invitation link', detail: 'GET /invitation/register/{token} → Drupal routes to registration page', actor: 'Vendor', color: 'blue' },
                        { step: '3', label: 'Validate token', detail: 'GET /api/invitation/validate/{token} → checks existence, expiration, status', actor: 'System', color: 'gray' },
                        { step: '4', label: 'Trigger MFA', detail: 'POST /api/invitation/trigger-mfa/{token} → generates 6-digit code, emails to vendor', actor: 'System', color: 'gray' },
                        { step: '5', label: 'Vendor enters MFA code', detail: 'POST /api/invitation/verify-mfa/{token} → validates code, 15-min expiry, advances to MfaVerified', actor: 'Vendor', color: 'blue' },
                        { step: '6', label: 'Submit initial info', detail: 'POST /api/invitation/submit-initial/{token} → company name, contact info', actor: 'Vendor', color: 'blue' },
                        { step: '7', label: 'Submit enrichment', detail: 'POST /api/invitation/submit-enrichment/{token} → address, banking, tax data', actor: 'Vendor', color: 'blue' },
                        { step: '8', label: 'Complete registration', detail: 'POST /api/invitation/complete/{token} → creates VendorApplication, runs sanctions, marks Completed', actor: 'System', color: 'green' },
                    ].map((item) => (
                        <div key={item.step} className="flex gap-4">
                            <div className="flex flex-col items-center">
                                <div className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-white text-sm font-bold ${
                                    item.color === 'purple' ? 'bg-purple-500' :
                                    item.color === 'blue' ? 'bg-blue-500' :
                                    item.color === 'green' ? 'bg-green-500' :
                                    'bg-gray-500'
                                }`}>
                                    {item.step}
                                </div>
                                {item.step !== '8' && <div className="w-px flex-1 bg-gray-200 my-1" />}
                            </div>
                            <div className="pb-6">
                                <div className="flex items-center gap-2">
                                    <p className="text-sm font-semibold text-gray-900">{item.label}</p>
                                    <span className={`text-xs px-1.5 py-0.5 rounded-full ${
                                        item.actor === 'Admin' ? 'bg-purple-100 text-purple-700' :
                                        item.actor === 'Vendor' ? 'bg-blue-100 text-blue-700' :
                                        'bg-gray-100 text-gray-700'
                                    }`}>{item.actor}</span>
                                </div>
                                <p className="text-xs text-gray-500 mt-1 font-mono">{item.detail}</p>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </Card>

        {/* State Machine */}
        <Card title="Invitation State Machine">
            <div className="p-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                        <h4 className="text-sm font-semibold text-gray-700 mb-3">Status Transitions</h4>
                        <div className="space-y-2">
                            {[
                                { from: 'Pending', to: ['Accepted', 'Expired', 'Cancelled', 'Resent'], color: 'yellow' },
                                { from: 'Accepted', to: ['Completed', 'Cancelled'], color: 'blue' },
                                { from: 'Expired', to: ['Pending', 'Resent'], color: 'red' },
                                { from: 'Resent', to: ['Accepted', 'Expired', 'Cancelled'], color: 'orange' },
                                { from: 'Completed', to: [], color: 'green' },
                                { from: 'Cancelled', to: [], color: 'gray' },
                            ].map((row) => (
                                <div key={row.from} className="flex items-center gap-2 text-sm">
                                    <span className={`inline-block w-24 text-center px-2 py-1 rounded font-medium text-xs ${
                                        row.color === 'yellow' ? 'bg-yellow-100 text-yellow-800' :
                                        row.color === 'blue' ? 'bg-blue-100 text-blue-800' :
                                        row.color === 'red' ? 'bg-red-100 text-red-800' :
                                        row.color === 'orange' ? 'bg-orange-100 text-orange-800' :
                                        row.color === 'green' ? 'bg-green-100 text-green-800' :
                                        'bg-gray-100 text-gray-800'
                                    }`}>{row.from}</span>
                                    <span className="text-gray-400">&rarr;</span>
                                    <span className="text-xs text-gray-600">
                                        {row.to.length > 0 ? row.to.join(' | ') : <em className="text-gray-400">Terminal</em>}
                                    </span>
                                </div>
                            ))}
                        </div>
                    </div>
                    <div>
                        <h4 className="text-sm font-semibold text-gray-700 mb-3">Workflow Stages (within Accepted)</h4>
                        <div className="space-y-3">
                            {[
                                { stage: 'InvitationSent', desc: 'Email sent, awaiting vendor click', icon: Mail },
                                { stage: 'MfaVerified', desc: '6-digit code verified successfully', icon: Shield },
                                { stage: 'InitialInfoCompleted', desc: 'Basic company info submitted', icon: Server },
                                { stage: 'Enriched', desc: 'Full data submitted, ready for review', icon: CheckCircle },
                            ].map((s, i) => (
                                <div key={s.stage} className="flex items-center gap-3">
                                    <div className="flex items-center gap-2">
                                        <div className="flex h-6 w-6 items-center justify-center rounded-full bg-brand-100 text-brand-600 text-xs font-bold">{i + 1}</div>
                                        <s.icon className="h-4 w-4 text-gray-400" />
                                    </div>
                                    <div>
                                        <p className="text-sm font-medium text-gray-900">{s.stage}</p>
                                        <p className="text-xs text-gray-500">{s.desc}</p>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
            </div>
        </Card>
    </div>
);

// --- Tab: API ---

const ApiTab: React.FC = () => (
    <div className="space-y-6">
        <Card title="API Base URL Configuration">
            <div className="p-4">
                <div className="rounded-lg bg-amber-50 border border-amber-200 p-4 mb-4">
                    <div className="flex items-start gap-2">
                        <AlertTriangle className="h-5 w-5 text-amber-600 mt-0.5 shrink-0" />
                        <div>
                            <p className="text-sm font-medium text-amber-900">CORS or Server-Side Calls</p>
                            <p className="text-xs text-amber-700 mt-1">
                                The Drupal site should call these endpoints <strong>server-to-server</strong> (via <code className="bg-amber-100 px-1 rounded">Guzzle / \Drupal::httpClient()</code>)
                                to avoid CORS configuration. If calling from browser JavaScript, the .NET API needs a CORS policy allowing the Drupal domain.
                            </p>
                        </div>
                    </div>
                </div>
                <CodeBlock
                    label="Base URL"
                    code={`# Production
https://your-app-service.azurewebsites.net/api/invitation/

# Local Development
https://localhost:5001/api/invitation/

# Drupal settings.php
$settings['vendor_mdm_api_base'] = 'https://your-app-service.azurewebsites.net';`}
                />
            </div>
        </Card>

        <Card title="Public Endpoints (Vendor-Facing)">
            <div className="p-4 space-y-4">
                {API_ENDPOINTS.filter(e => e.auth === 'Public').map((endpoint) => (
                    <CollapsibleSection
                        key={endpoint.path}
                        title={`${endpoint.method} ${endpoint.path}`}
                        badge={endpoint.stage}
                    >
                        <div className="space-y-3">
                            <p className="text-sm text-gray-600">{endpoint.description}</p>
                            {endpoint.requestBody && (
                                <div>
                                    <p className="text-xs font-medium text-gray-500 mb-1">Request Body</p>
                                    <CodeBlock code={endpoint.requestBody} language="json" />
                                </div>
                            )}
                            <div>
                                <p className="text-xs font-medium text-gray-500 mb-1">Response</p>
                                <CodeBlock code={endpoint.responseBody} language="json" />
                            </div>
                        </div>
                    </CollapsibleSection>
                ))}
            </div>
        </Card>

        <Card title="Admin Endpoints (Authenticated)">
            <div className="p-4 space-y-4">
                <div className="rounded-lg bg-red-50 border border-red-200 p-3 mb-2">
                    <p className="text-xs text-red-700 flex items-center gap-2">
                        <Lock className="h-3.5 w-3.5" />
                        These endpoints require a Bearer token with <strong>Approver</strong> role. The Drupal admin module needs its own authentication to the .NET API.
                    </p>
                </div>
                {API_ENDPOINTS.filter(e => e.auth === 'ApproverOnly').map((endpoint) => (
                    <CollapsibleSection
                        key={endpoint.path}
                        title={`${endpoint.method} ${endpoint.path}`}
                        badge={endpoint.stage}
                    >
                        <div className="space-y-3">
                            <p className="text-sm text-gray-600">{endpoint.description}</p>
                            {endpoint.requestBody && (
                                <div>
                                    <p className="text-xs font-medium text-gray-500 mb-1">Request Body</p>
                                    <CodeBlock code={endpoint.requestBody} language="json" />
                                </div>
                            )}
                            <div>
                                <p className="text-xs font-medium text-gray-500 mb-1">Response</p>
                                <CodeBlock code={endpoint.responseBody} language="json" />
                            </div>
                        </div>
                    </CollapsibleSection>
                ))}
            </div>
        </Card>
    </div>
);

// --- Tab: Azure ---

const AzureTab: React.FC = () => (
    <div className="space-y-6">
        <Card title="Required Azure Resources">
            <div className="p-4">
                <p className="text-sm text-gray-600 mb-4">
                    These are the Azure resources the external developer needs provisioned in their Azure subscription to run the invitation + MFA flow.
                </p>
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
                    {AZURE_RESOURCES.map((resource) => (
                        <div key={resource.name} className={`rounded-lg border p-4 ${
                            resource.required ? 'border-gray-200' : 'border-dashed border-gray-300'
                        }`}>
                            <div className="flex items-start gap-3">
                                <div className={`rounded-lg p-2 ${
                                    resource.color === 'blue' ? 'bg-blue-100' :
                                    resource.color === 'green' ? 'bg-green-100' :
                                    resource.color === 'purple' ? 'bg-purple-100' :
                                    resource.color === 'orange' ? 'bg-orange-100' :
                                    resource.color === 'indigo' ? 'bg-indigo-100' :
                                    'bg-red-100'
                                }`}>
                                    <resource.icon className={`h-5 w-5 ${
                                        resource.color === 'blue' ? 'text-blue-600' :
                                        resource.color === 'green' ? 'text-green-600' :
                                        resource.color === 'purple' ? 'text-purple-600' :
                                        resource.color === 'orange' ? 'text-orange-600' :
                                        resource.color === 'indigo' ? 'text-indigo-600' :
                                        'text-red-600'
                                    }`} />
                                </div>
                                <div className="flex-1">
                                    <div className="flex items-center justify-between">
                                        <h4 className="text-sm font-semibold text-gray-900">{resource.name}</h4>
                                        {resource.required ? (
                                            <span className="text-xs px-2 py-0.5 rounded-full bg-red-100 text-red-700 font-medium">Required</span>
                                        ) : (
                                            <span className="text-xs px-2 py-0.5 rounded-full bg-gray-100 text-gray-600 font-medium">Optional</span>
                                        )}
                                    </div>
                                    <p className="text-xs text-gray-500 mt-0.5 font-mono">{resource.type}</p>
                                    <p className="text-xs text-gray-600 mt-2">{resource.purpose}</p>
                                    <div className="flex items-center gap-2 mt-2">
                                        <span className="text-xs text-gray-500">Tier:</span>
                                        <span className="text-xs font-medium text-gray-700">{resource.tier}</span>
                                    </div>
                                    {resource.configKeys.length > 0 && (
                                        <div className="mt-2">
                                            <p className="text-xs text-gray-500 mb-1">Configuration Keys:</p>
                                            <div className="flex flex-wrap gap-1">
                                                {resource.configKeys.map((key) => (
                                                    <code key={key} className="text-xs bg-gray-100 text-gray-700 px-1.5 py-0.5 rounded">{key}</code>
                                                ))}
                                            </div>
                                        </div>
                                    )}
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </Card>

        <Card title="appsettings.json — Required Configuration">
            <div className="p-4">
                <CodeBlock
                    label="appsettings.json (template for external developer)"
                    language="json"
                    code={`{
  "ConnectionStrings": {
    "SqlConnection": "Server=tcp:<your-server>.database.windows.net,1433;Database=VendorMdm;..."
  },
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=https://<your-cosmos>.documents.azure.com:443/;...",
    "DatabaseName": "VendorMdm"
  },
  "EmailService": {
    "Smtp": {
      "Host": "smtp.office365.com",
      "Port": 587,
      "FromEmail": "no-reply@yourcompany.com",
      "FromName": "Vendor Portal",
      "Username": "<from-keyvault>",
      "Password": "<from-keyvault>"
    },
    "AzureFunctionUrl": "https://<your-function>.azurewebsites.net/api/SendEmail"
  },
  "KeyVault": {
    "VaultUri": "https://<your-keyvault>.vault.azure.net/"
  },
  "Invitation": {
    "DefaultExpirationDays": 14,
    "MfaCodeExpirationMinutes": 15,
    "BaseUrl": "https://your-drupal-site.com"
  }
}`}
                />
            </div>
        </Card>

        <Card title="Bicep / ARM Deployment">
            <div className="p-4">
                <p className="text-sm text-gray-600 mb-3">
                    Minimal Bicep template to provision the required resources in the developer's Azure subscription.
                </p>
                <CodeBlock
                    label="main.bicep (minimal deployment)"
                    language="bicep"
                    code={`param location string = resourceGroup().location
param appName string = 'vendor-invitation-api'

// App Service Plan
resource plan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: '\${appName}-plan'
  location: location
  sku: { name: 'B1', tier: 'Basic' }
  properties: { reserved: false }
}

// App Service (.NET 8)
resource app 'Microsoft.Web/sites@2023-01-01' = {
  name: appName
  location: location
  properties: {
    serverFarmId: plan.id
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      alwaysOn: true
      httpsOnly: true
    }
  }
}

// SQL Server + Database
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: '\${appName}-sql'
  location: location
  properties: {
    administratorLogin: 'sqladmin'
    administratorLoginPassword: '<from-keyvault>'
  }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: 'VendorMdm'
  location: location
  sku: { name: 'Basic', tier: 'Basic' }
}

// Key Vault
resource vault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: '\${appName}-kv'
  location: location
  properties: {
    sku: { family: 'A', name: 'standard' }
    tenantId: subscription().tenantId
    accessPolicies: []
  }
}`}
                />
            </div>
        </Card>
    </div>
);

// --- Tab: Code ---

const CodeTab: React.FC = () => (
    <div className="space-y-6">
        <div className="rounded-lg bg-brand-50 border border-brand-200 p-4">
            <p className="text-sm text-brand-800">
                Each section below shows the <strong>C# source code</strong> from this portal alongside a <strong>PHP/Drupal equivalent</strong>.
                The developer can either call the .NET API directly (recommended) or port these snippets into their Drupal module.
            </p>
        </div>

        {Object.entries(CODE_SNIPPETS).map(([key, snippet]) => (
            <Card key={key} title={snippet.label}>
                <div className="p-4 space-y-4">
                    <CollapsibleSection title="C# Source (from this portal)" defaultOpen={true}>
                        <CodeBlock code={snippet.code} language={snippet.language} />
                    </CollapsibleSection>
                    <CollapsibleSection title="PHP / Drupal Equivalent" badge="Drupal">
                        <CodeBlock code={snippet.phpEquivalent} language="php" />
                    </CollapsibleSection>
                </div>
            </Card>
        ))}
    </div>
);

// --- Tab: Recommendations ---

const RecommendationsTab: React.FC = () => (
    <div className="space-y-6">
        {/* Option A vs B */}
        <Card title="Integration Strategy: Option A vs Option B">
            <div className="p-4">
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
                    {/* Option A */}
                    <div className="rounded-lg border-2 border-green-300 bg-green-50 p-5">
                        <div className="flex items-center gap-2 mb-3">
                            <CheckCircle className="h-5 w-5 text-green-600" />
                            <h4 className="text-sm font-bold text-green-900">Option A: Keep .NET API as Microservice</h4>
                            <span className="text-xs px-2 py-0.5 rounded-full bg-green-200 text-green-800 font-bold">RECOMMENDED</span>
                        </div>
                        <ul className="space-y-2 text-xs text-green-800">
                            <li className="flex items-start gap-2">
                                <CheckCircle className="h-3.5 w-3.5 text-green-500 mt-0.5 shrink-0" />
                                <span>Security logic stays centralized — no risk of weakened token/MFA in PHP</span>
                            </li>
                            <li className="flex items-start gap-2">
                                <CheckCircle className="h-3.5 w-3.5 text-green-500 mt-0.5 shrink-0" />
                                <span>Already built and tested — production-ready endpoints</span>
                            </li>
                            <li className="flex items-start gap-2">
                                <CheckCircle className="h-3.5 w-3.5 text-green-500 mt-0.5 shrink-0" />
                                <span>Drupal side is simple — just HTTP calls + form rendering</span>
                            </li>
                            <li className="flex items-start gap-2">
                                <CheckCircle className="h-3.5 w-3.5 text-green-500 mt-0.5 shrink-0" />
                                <span>Shared state — both apps see the same invitations in the same DB</span>
                            </li>
                            <li className="flex items-start gap-2">
                                <CheckCircle className="h-3.5 w-3.5 text-green-500 mt-0.5 shrink-0" />
                                <span>Future updates to auth flow only need one deployment</span>
                            </li>
                        </ul>
                        <div className="mt-3 pt-3 border-t border-green-200">
                            <p className="text-xs text-green-700"><strong>Drupal dev effort:</strong> ~2-3 days (form + HTTP client + routing)</p>
                        </div>
                    </div>

                    {/* Option B */}
                    <div className="rounded-lg border-2 border-orange-300 bg-orange-50 p-5">
                        <div className="flex items-center gap-2 mb-3">
                            <AlertTriangle className="h-5 w-5 text-orange-600" />
                            <h4 className="text-sm font-bold text-orange-900">Option B: Port Auth Logic to Drupal/PHP</h4>
                        </div>
                        <ul className="space-y-2 text-xs text-orange-800">
                            <li className="flex items-start gap-2">
                                <AlertTriangle className="h-3.5 w-3.5 text-orange-500 mt-0.5 shrink-0" />
                                <span>Must re-implement all security logic correctly in PHP</span>
                            </li>
                            <li className="flex items-start gap-2">
                                <AlertTriangle className="h-3.5 w-3.5 text-orange-500 mt-0.5 shrink-0" />
                                <span>Two separate codebases to maintain for the same flow</span>
                            </li>
                            <li className="flex items-start gap-2">
                                <AlertTriangle className="h-3.5 w-3.5 text-orange-500 mt-0.5 shrink-0" />
                                <span>No shared database — data sync complexity</span>
                            </li>
                            <li className="flex items-start gap-2">
                                <CheckCircle className="h-3.5 w-3.5 text-orange-500 mt-0.5 shrink-0" />
                                <span>Full ownership — no external API dependency</span>
                            </li>
                            <li className="flex items-start gap-2">
                                <CheckCircle className="h-3.5 w-3.5 text-orange-500 mt-0.5 shrink-0" />
                                <span>No CORS or network config needed</span>
                            </li>
                        </ul>
                        <div className="mt-3 pt-3 border-t border-orange-200">
                            <p className="text-xs text-orange-700"><strong>Drupal dev effort:</strong> ~1-2 weeks (full module + email + DB + testing)</p>
                        </div>
                    </div>
                </div>
            </div>
        </Card>

        {/* Security Checklist */}
        <Card title="Security Checklist for External Developer">
            <div className="p-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {[
                        { category: 'Token Security', items: [
                            'Use cryptographically secure random bytes (32 bytes minimum)',
                            'Base64URL encode tokens (no +, /, or = characters)',
                            'Never log or expose tokens in error responses',
                            'Unique index on InvitationToken column',
                            'Rate-limit token validation endpoint',
                        ]},
                        { category: 'MFA Security', items: [
                            'Generate 6-digit codes with secure RNG (not Math.random)',
                            'Enforce 15-minute expiration on codes',
                            'Delete code from DB after successful verification',
                            'Rate-limit verification attempts (max 5 per token)',
                            'Send code only to the pre-registered email',
                        ]},
                        { category: 'API Security', items: [
                            'HTTPS only — never expose over HTTP',
                            'Rate limiting on all public endpoints',
                            'Input validation on all request bodies',
                            'No sensitive data in API responses',
                            'CORS locked to specific Drupal domain',
                        ]},
                        { category: 'Data Security', items: [
                            'Store connection strings in Key Vault',
                            'Encrypt data at rest (Azure SQL TDE)',
                            'Audit log all invitation state changes',
                            'PII handling per GDPR/compliance requirements',
                            'Automatic expiration of stale invitations',
                        ]},
                    ].map((section) => (
                        <div key={section.category} className="rounded-lg border border-gray-200 p-4">
                            <h4 className="text-sm font-semibold text-gray-900 mb-3 flex items-center gap-2">
                                <Shield className="h-4 w-4 text-brand-500" />
                                {section.category}
                            </h4>
                            <ul className="space-y-2">
                                {section.items.map((item, i) => (
                                    <li key={i} className="flex items-start gap-2 text-xs text-gray-600">
                                        <CheckCircle className="h-3.5 w-3.5 text-gray-300 mt-0.5 shrink-0" />
                                        {item}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    ))}
                </div>
            </div>
        </Card>

        {/* What to hand off */}
        <Card title="Handoff Package for Drupal Developer">
            <div className="p-4">
                <div className="rounded-lg bg-gray-50 border border-gray-200 p-5">
                    <h4 className="text-sm font-semibold text-gray-900 mb-4">Deliverables Checklist</h4>
                    <div className="space-y-3">
                        {[
                            { item: 'API Specification', desc: 'The 8 REST endpoints documented in the API tab with request/response schemas', done: true },
                            { item: 'Azure Resource List', desc: '6 Azure resources with Bicep template for automated provisioning', done: true },
                            { item: 'appsettings.json Template', desc: 'Configuration keys for SQL, Cosmos, Email, and Key Vault', done: true },
                            { item: 'Database Schema (SQL)', desc: 'VendorInvitations table DDL with indexes', done: true },
                            { item: 'State Machine Definition', desc: 'Status transitions + stage progression rules', done: true },
                            { item: 'Email Templates (HTML)', desc: 'Invitation email + MFA code email templates', done: true },
                            { item: 'PHP/Drupal Code Equivalents', desc: 'Token generation, MFA, verification logic in PHP', done: true },
                            { item: 'Security Checklist', desc: 'Token, MFA, API, and data security requirements', done: true },
                            { item: 'CORS Configuration', desc: 'If using browser-side calls: allowed origins for .NET API', done: false },
                            { item: 'Service-to-Service Auth', desc: 'If Option A: API key or client credentials for Drupal → .NET calls', done: false },
                        ].map((d, i) => (
                            <div key={i} className="flex items-start gap-3">
                                <div className={`h-5 w-5 rounded-full flex items-center justify-center shrink-0 mt-0.5 ${
                                    d.done ? 'bg-green-100' : 'bg-yellow-100'
                                }`}>
                                    {d.done
                                        ? <CheckCircle className="h-3.5 w-3.5 text-green-600" />
                                        : <Clock className="h-3.5 w-3.5 text-yellow-600" />
                                    }
                                </div>
                                <div>
                                    <p className="text-sm font-medium text-gray-900">{d.item}</p>
                                    <p className="text-xs text-gray-500">{d.desc}</p>
                                </div>
                                <span className={`ml-auto text-xs px-2 py-0.5 rounded-full font-medium shrink-0 ${
                                    d.done ? 'bg-green-100 text-green-700' : 'bg-yellow-100 text-yellow-700'
                                }`}>
                                    {d.done ? 'Included' : 'Pending'}
                                </span>
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </Card>

        {/* Drupal Module Skeleton */}
        <Card title="Drupal Module Skeleton (Option A — API Consumer)">
            <div className="p-4">
                <p className="text-sm text-gray-600 mb-3">
                    If going with Option A, the Drupal developer creates a simple custom module:
                </p>
                <CodeBlock
                    label="vendor_invitation.routing.yml"
                    language="yaml"
                    code={`vendor_invitation.register:
  path: '/invitation/register/{token}'
  defaults:
    _controller: '\\Drupal\\vendor_invitation\\Controller\\RegistrationController::register'
    _title: 'Vendor Registration'
  requirements:
    _access: 'TRUE'  # Public route

vendor_invitation.verify_mfa:
  path: '/invitation/verify-mfa/{token}'
  defaults:
    _controller: '\\Drupal\\vendor_invitation\\Controller\\RegistrationController::verifyMfa'
  requirements:
    _access: 'TRUE'
  methods: [POST]`}
                />
                <div className="mt-4">
                    <CodeBlock
                        label="RegistrationController.php (simplified)"
                        language="php"
                        code={`<?php

namespace Drupal\\vendor_invitation\\Controller;

use Drupal\\Core\\Controller\\ControllerBase;
use Symfony\\Component\\HttpFoundation\\Request;
use Symfony\\Component\\HttpFoundation\\JsonResponse;

class RegistrationController extends ControllerBase {

  protected string $apiBase;

  public function __construct() {
    $this->apiBase = \\Drupal::config('vendor_invitation.settings')
      ->get('api_base_url');
  }

  public function register(string $token) {
    // Step 1: Validate token with .NET API
    $client = \\Drupal::httpClient();
    $response = $client->get("{$this->apiBase}/api/invitation/validate/{$token}");
    $data = json_decode($response->getBody(), true);

    if (!$data['isValid']) {
      return ['#markup' => '<p>Invalid or expired invitation.</p>'];
    }

    // Step 2: Trigger MFA
    $client->post("{$this->apiBase}/api/invitation/trigger-mfa/{$token}");

    // Step 3: Render form with MFA input
    return \\Drupal::formBuilder()
      ->getForm('Drupal\\vendor_invitation\\Form\\RegistrationForm', $token, $data);
  }

  public function verifyMfa(string $token, Request $request) {
    $code = $request->get('code');
    $client = \\Drupal::httpClient();

    $response = $client->post(
      "{$this->apiBase}/api/invitation/verify-mfa/{$token}",
      ['json' => ['code' => $code]]
    );

    return new JsonResponse(json_decode($response->getBody(), true));
  }
}`}
                    />
                </div>
            </div>
        </Card>
    </div>
);
