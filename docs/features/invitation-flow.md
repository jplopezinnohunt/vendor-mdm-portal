# Vendor Invitation & Onboarding Flow

This document outlines the end-to-end flow for inviting and onboarding new vendors, including email dispatch, multi-factor authentication (MFA), data submission, and internal approval.

## Mermaid Diagram

To view this diagram in a Mermaid-compatible viewer (e.g., GitHub, VS Code), or to edit it in **Draw.io**:

**How to open in Draw.io:**
1. Open [draw.io](https://app.diagrams.net/).
2. Go to `Arrange` > `Insert` > `Advanced` > `Mermaid`.
3. Paste the code block below.

![Sequence Diagram](../images/invitation-sequence.png)

### Source Code (Sequence)
```mermaid
sequenceDiagram
    autonumber
    actor Approver as Internal Approver
    participant Backend as VendorMDM API
    participant Sanctions as Sanctions Service
    participant Email as Email Service
    actor Vendor as Vendor (User)
    participant Frontend as Vendor Portal
    participant DB as SQL & Cosmos DB

    Note over Approver, DB: 1. Invitation Phase
    Approver->>Backend: POST /invitation/create (Email, Name, Type)
    
    rect rgb(240, 240, 240)
        Note right of Backend: Pre-Invitation Checks
        Backend->>DB: Check Duplicate (Email/Tax ID)
        Backend->>Sanctions: Run Screening (Name)
        alt High Risk Found
            Backend--xApprover: Block Invitation (Default)
            opt Overrule
                Approver->>Backend: Retry with ForceCreation=True
            end
        end
    end
    
    Backend->>DB: Create Invitation (Token, Status: Pending)
    Backend->>Email: Send Invitation Email (w/ Token)
    Email-->>Vendor: Email with Link /invitation/register/{token}

    Note over Vendor, DB: 2. Authentication Phase
    Vendor->>Frontend: Click Link
    Frontend->>Backend: GET /invitation/validate/{token}
    Backend-->>Frontend: Valid (Stage: InvitationSent)
    
    Frontend->>Backend: POST /invitation/trigger-mfa
    Backend->>DB: Generate 6-digit Code (Expires 15m)
    Backend->>Email: Send Verification Code
    Email-->>Vendor: "Your Code: 123456"
    
    Vendor->>Frontend: Enter Code "123456"
    Frontend->>Backend: POST /invitation/verify-mfa
    Backend->>DB: Verify & Update Stage -> MfaVerified
    Backend-->>Frontend: Success

    Note over Vendor, DB: 3. Data Submission Phase
    Vendor->>Frontend: Review Initial Info (Name, Contact)
    Frontend->>Backend: POST /invitation/submit-initial
    Backend->>DB: Update Stage -> InitialInfoCompleted
    
    Vendor->>Frontend: Fill Enrichment Form (Address, Bank, Tax)
    Frontend->>Backend: POST /invitation/submit-enrichment
    Backend->>DB: Save Attributes (JSON), Update Stage -> Enriched
    
    Frontend->>Backend: POST /invitation/complete
    Backend->>DB: Create VendorApplication (Status: PendingReview)
    Backend->>DB: Update Invitation (Status: Completed)
    Backend-->>Frontend: Success (Application ID)

    Note over Approver, DB: 4. Approval Phase
    Approver->>Backend: GET /review/pending
    Backend-->>Approver: List of Pending Applications
    Approver->>Backend: POST /review/{id}/approve (Optional Enrichment)
    Backend->>DB: Update Application (Status: Approved)
    Backend-->>Approver: Success
```

## State Transition Diagram

![State Diagram](../images/invitation-state.png)

### Source Code (State)
```mermaid
stateDiagram-v2
    [*] --> InvitationCreated: Approver Invites
    InvitationCreated --> InvitationSent: Email Dispatched
    InvitationSent --> MfaVerified: Vendor Verifies Identity
    MfaVerified --> InitialInfoCompleted: Vendor Confirms Basics
    InitialInfoCompleted --> Enriched: Vendor Submits Details
    Enriched --> ApplicationCreated: System Finalizes Submission
    
    state ApplicationCreated {
        [*] --> PendingReview
        PendingReview --> Approved: Approver Accepts
        PendingReview --> Rejected: Approver Rejects
    }
    
    Approved --> [*]
    Rejected --> [*]
```

## Activity Diagram

This diagram visualizes the **system logic and control flow**, highlighting decisions and parallel processes (e.g., Validation, MFA loop).

![Activity Diagram](../images/invitation-activity.png)

### Source Code (Activity)
```mermaid
flowchart TD
    Start([Start]) --> Invite[Approver Initiates Invitation]
    
    Invite --> Checks{Pre-Checks}
    Checks -- Pass --> Create[Create Invitation Record]
    Checks -- Risk Found --> RiskDecision{Overrule?}
    
    RiskDecision -- Yes (Force) --> Create
    RiskDecision -- No --> Stop([Block Invitation])
    
    Create --> Email[Send Email]
    Email --> UserClick[Vendor Clicks Link]
    
    UserClick --> Validate{Token Valid?}
    Validate -- No --> Error([Show Error])
    Validate -- Yes --> MFA[Trigger MFA]
    
    MFA --> SentCode[Email 6-Digit Code]
    SentCode --> InputCode[/Vendor Enters Code/]
    
    InputCode --> CheckCode{Code Valid?}
    CheckCode -- No --> InputCode
    CheckCode -- Yes --> Verified[Mark MFA Verified]
    
    Verified --> InitInfo[Vendor Confirms Contact Info]
    InitInfo --> Enrich[/Vendor Fills Enrichment Form/]
    
    Enrich --> Submit[Submit to Backend]
    Submit --> CreateApp[System Creates VendorApplication]
    CreateApp --> Screening[Run Final Screening]
    
    Screening --> End([End: Pending Review])
```

## Diagram Explanations & Reference

| Feature | User Flow / Sequence Diagram | Activity Diagram | State Diagram |
| :--- | :--- | :--- | :--- |
| **Primary Goal** | **User Experience (UX)** <br> Focuses on the path the user takes through screens and interactions. | **System Logic & Algorithms** <br> Focuses on the flow of control, decisions, loops, and data processing. | **Object Lifecycle** <br> Focuses on the condition (state) of a specific object at any given time. |
| **Key Question** | *"What does the user see next?"* | *"What does the system do next?"* | *"What is the status of this object now?"* |
| **Best For** | Visualizing navigation and API interactions between User, Frontend, and Backend. | Complex business logic, validation loops (like MFA), and parallel processing steps. | Objects with complex statuses (e.g., Invitation: `Pending` → `Sent` → `MfaVerified` → `Completed`). |
