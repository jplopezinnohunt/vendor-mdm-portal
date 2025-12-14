-- Migration: AddAttributesJsonColumns
-- Date: 2025-12-13
-- Description: Add JSON Attributes columns to all SQL entities per Hybrid Relational-Document Model

-- Add Attributes column to ChangeRequests
ALTER TABLE [ChangeRequests] 
ADD [Attributes] nvarchar(max) NOT NULL DEFAULT '{}';

-- Add Attributes column to VendorApplications
ALTER TABLE [VendorApplications] 
ADD [Attributes] nvarchar(max) NOT NULL DEFAULT '{}';

-- Add Attributes column to VendorInvitations
ALTER TABLE [VendorInvitations] 
ADD [Attributes] nvarchar(max) NOT NULL DEFAULT '{}';

-- Add Attributes column to Attachments
ALTER TABLE [Attachments] 
ADD [Attributes] nvarchar(max) NOT NULL DEFAULT '{}';

-- Add Attributes column to UsersAndRoles
ALTER TABLE [UsersAndRoles] 
ADD [Attributes] nvarchar(max) NOT NULL DEFAULT '{}';

-- Add Attributes column to WorkflowStates
ALTER TABLE [WorkflowStates] 
ADD [Attributes] nvarchar(max) NOT NULL DEFAULT '{}';

-- Migrate existing Notes from VendorInvitations to Attributes
UPDATE [VendorInvitations]
SET [Attributes] = JSON_MODIFY('{}', '$.notes', [Notes])
WHERE [Notes] IS NOT NULL AND [Notes] != '';

-- Note: The Notes column is marked as [Obsolete] in code but not dropped for backward compatibility
-- It will be removed in a future major version
