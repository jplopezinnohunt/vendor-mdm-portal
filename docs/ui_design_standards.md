# UI Design Standards & Principles

## Core Philosophy
**"Consistency is Credibility"**
A scattered, ad-hoc UI erodes user trust. We build **Premium, Dense, and Consistent** interfaces.

## 1. The Four Pillars of Design

### A. Uniformity (The Law of Consistency)
*   **Theory:** Users should not have to wonder whether different words, situations, or actions mean the same thing.
*   **Rule:** NEVER hardcode CSS values (e.g., `margin-top: 13px`).
*   **Implementation:**
    *   **Spacing:** Use Tailwind utility classes (`p-4`, `gap-2`).
    *   **Components:** Always use shared components (`<PrimaryButton>`, `<VendorCard>`) instead of HTML primitives.
    *   **Typography:** Use the standard type scale (`text-xl font-bold`, `text-sm text-gray-500`).

### B. Proximity (Gestalt Psychology)
*   **Theory:** Objects that are near each other tend to be grouped together.
*   **Rule:** Use whitespace and boundaries to define relationships.
*   **Implementation:**
    *   Group related fields (e.g., "Address") in a Fieldset or Card.
    *   Use larger gaps between distinct sections (`gap-8`) and smaller gaps between related items (`gap-2`).

### C. Feedback (Doherty Threshold)
*   **Theory:** System must provide reaction within <400ms to keep user attention.
*   **Rule:** Every interaction must have a state.
*   **Implementation:**
    *   **Buttons:** Must show `Loading...` or spinner state on submit.
    *   **Validation:** Inline errors appear immediately on blur.
    *   **Empty States:** Never show a blank table. Show "No vendors found. [Create New]" placeholder.

### D. Aesthetics (Visceral Design)
*   **Theory:** Attractive things work better. Users perceive beautiful designs as more usable.
*   **Rule:** "If it looks basic, it is a failure."
*   **Implementation:**
    *   **Depth:** Use subtle shadows (`shadow-sm`) to lift active elements.
    *   **Scanning:** Use badges/colors to make status scannable (Green = Active, Gray = Draft).
    *   **Alignment:** Strict vertical alignment.

## 2. Technical Implementation

### The 12-Column Grid
All layouts must adhere to the 12-column system.
*   **Desktop:** 12 Columns.
*   **Tablet:** 8 Columns.
*   **Mobile:** 4 Columns.

### Dark Mode
*   **Mandate:** All components must support Dark Mode (`dark:bg-slate-900`).
*   **Colors:** Use semantic colors (`bg-surface`, `text-primary`) rather than hex codes.
