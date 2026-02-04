-- =====================================================
-- Migration: AddOutboxEvents (Safe - Additive Only)
-- Purpose: Add OutboxEvents table for EDA Outbox Pattern
-- Date: 2026-02-04
-- ZERO DATA LOSS: This script only CREATES new table
-- =====================================================

-- Check if table already exists before creating
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OutboxEvents')
BEGIN
    CREATE TABLE [OutboxEvents] (
        [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
        [EventType] nvarchar(200) NOT NULL,
        [Payload] nvarchar(max) NOT NULL DEFAULT '{}',
        [Status] nvarchar(50) NOT NULL DEFAULT 'Pending',
        [CorrelationId] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [ProcessedAt] datetime2 NULL,
        [RetryCount] int NOT NULL DEFAULT 0,
        [LastError] nvarchar(2000) NULL,
        [NextRetryAt] datetime2 NULL,
        [Destination] nvarchar(200) NULL,
        CONSTRAINT [PK_OutboxEvents] PRIMARY KEY ([Id])
    );

    -- Index for pending events processing
    CREATE INDEX [IX_OutboxEvent_Processing] ON [OutboxEvents] ([Status], [CreatedAt]);

    -- Index for retry processing
    CREATE INDEX [IX_OutboxEvent_Retry] ON [OutboxEvents] ([Status], [NextRetryAt]);

    -- Index for event type queries
    CREATE INDEX [IX_OutboxEvent_EventType] ON [OutboxEvents] ([EventType]);

    -- Index for correlation tracking
    CREATE INDEX [IX_OutboxEvent_Correlation] ON [OutboxEvents] ([CorrelationId]);

    PRINT 'OutboxEvents table created successfully';
END
ELSE
BEGIN
    PRINT 'OutboxEvents table already exists - no changes made';
END
GO

-- =====================================================
-- SQLite Version (for local development)
-- Run this in SQLite if using local development
-- =====================================================
-- CREATE TABLE IF NOT EXISTS OutboxEvents (
--     Id TEXT PRIMARY KEY,
--     EventType TEXT NOT NULL,
--     Payload TEXT NOT NULL DEFAULT '{}',
--     Status TEXT NOT NULL DEFAULT 'Pending',
--     CorrelationId TEXT,
--     CreatedAt TEXT NOT NULL,
--     ProcessedAt TEXT,
--     RetryCount INTEGER NOT NULL DEFAULT 0,
--     LastError TEXT,
--     NextRetryAt TEXT,
--     Destination TEXT
-- );
-- CREATE INDEX IF NOT EXISTS IX_OutboxEvent_Processing ON OutboxEvents (Status, CreatedAt);
-- CREATE INDEX IF NOT EXISTS IX_OutboxEvent_Retry ON OutboxEvents (Status, NextRetryAt);
-- CREATE INDEX IF NOT EXISTS IX_OutboxEvent_EventType ON OutboxEvents (EventType);
-- CREATE INDEX IF NOT EXISTS IX_OutboxEvent_Correlation ON OutboxEvents (CorrelationId);
