---
trigger: always_on
---

Architectural Standard: Hybrid Relational-Document Model
1. The Core Rule
"Structured Identity, Semi-Structured Attributes."

The database schema adheres to a Hybrid Architecture utilizing PostgreSQL's relational engine for data integrity and JSONB for schema evolution. We reject the EAV (Entity-Attribute-Value) pattern in favor of document storage within relational rows.

Rule A (Structured Data): Any data element required for relational integrity, indexing, aggregation, or strict business logic validation must be modeled as a standard SQL Column.

Rule B (Semi-Structured Data): Any data element that is polymorphic, sparse, presentation-layer specific, or subject to frequent schema changes must be stored within the JSONB attributes column.

2. The Decision Matrix (Where does data go?)
Developers must apply the following criteria to determine if a field belongs in the Root Schema (Column) or the Document Store (JSONB).

Use a SQL Column (Structured) if ANY of these apply:
Foreign Key Constraint: The field links to another table (e.g., vendor_id, parent_category_id).

Indexing Requirement: The field is a primary search key or used in ORDER BY / GROUP BY clauses regularly (e.g., created_at, email, status).

Atomic Consistency: The field represents a financial value or critical state where ACID compliance is non-negotiable (e.g., payment_amount, workflow_state).

Universal Presence: The field exists for 100% of records (e.g., legal_name).

Use JSONB (Semi-Structured) if ANY of these apply:
High Volatility: The business requirements for this data change faster than our deployment cycle (e.g., ui_preferences, campaign_metadata).

Context-Specific: The data only applies to a subset of records (e.g., specific specific integration settings for one vendor type).

Read-Only Payload: The data is primarily read by the frontend and rarely queried directly by the backend (e.g., css_styles, logo_url).

Dynamic Hierarchy: Nested data structures that do not warrant their own normalized tables (e.g., audit_log_snapshot).

3. Entity Implementation Specifications
Based on the standard above, here is the approved schema strategy for core entities:

A. Vendors
Structured (Columns): vendor_id (PK), legal_name, tax_id, verification_status.

Reason: These define the legal entity and are used for joins across the system.

Semi-Structured (JSONB): social_links, branding_assets, communication_preferences.

Reason: Presentation data that varies by vendor.

B. Master Data
Structured (Columns): lookup_code (PK), standard_label, category_group.

Reason: Used strictly for referential integrity in other tables.

Semi-Structured (JSONB): translations (i18n), localized_formats, display_order.

Reason: Variable content based on user locale.

C. Workflows
Structured (Columns): workflow_id, current_stage, assigned_user_id, sla_due_date.

Reason: Critical for query performance and process enforcement.

Semi-Structured (JSONB): form_submission_data, context_variables, step_metadata.

Reason: The payload differs completely depending on the workflow type.

D. Payments (Strict Compliance)
Structured (Columns): transaction_id, amount_gross, currency_iso, payment_status, payer_id.

Reason: Financial data requires strict typing (Decimal/Numeric) to avoid floating-point errors.

Semi-Structured (JSONB): gateway_response_log, risk_analysis_score, card_fingerprint.

Reason: informational logs provided by third parties (Stripe/PayPal) that change format often.

4. Performance Optimization Clause
If a field within the JSONB attributes becomes a frequent target for search or filtering:

Do not refactor the table immediately.

Do implement a PostgreSQL Generated Column to materialize the specific JSON key into an indexed virtual column.

Example:

SQL

ALTER TABLE vendors 
ADD COLUMN region_code TEXT 
GENERATED ALWAYS AS (attributes ->> 'region') STORED;
CREATE INDEX idx_vendors_region ON vendors(region_code);