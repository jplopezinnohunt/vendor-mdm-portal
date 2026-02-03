# UI Design Standards & Principles

## Core Philosophy
**"Consistency is Credibility"**
A scattered, ad-hoc UI erodes user trust. We build **Premium, Dense, and Consistent** interfaces following **Microsoft Fluent UI and Microsoft 365 design patterns**.

---

## 1. Microsoft Fluent UI v9 Standards (Official)

### Layout Architecture
**Source**: [Microsoft Fluent UI v9 Documentation](https://react.fluentui.dev/)

> **"Fluent UI React v9 does not include `Stack` or `StackItem` components. Use native CSS Flexbox and Grid instead."**

**Core Principles**:
- ✅ **Native CSS First**: Use CSS Flexbox for component layout, CSS Grid for page structure
- ✅ **No Layout Wrapper Components**: Fluent v9 intentionally removed layout containers
- ✅ **Responsive by Design**: Layouts must adapt across devices (desktop, tablet, mobile)
- ✅ **Design Tokens Over Hardcoded Values**: Use spacing tokens instead of arbitrary pixel values

### Spacing System
**Source**: [Fluent 2 Design System - Layout](https://fluent2.microsoft.design/layout)

**Global Spacing Ramp** (base unit: 4px):
```
0, 2, 4, 6, 8, 10, 12, 16, 20, 24, 28, 32, 36, 40, 48, 52, 56
```

**Spacing Principles**:
- White space creates visual hierarchy
- Consistent spacing creates rhythm and cohesion
- Responsive spacing adapts to device scale
- Use spacing to direct focus to important areas

**Tailwind → Fluent Mapping**:
| Tailwind | Pixels | Fluent Token | Usage |
|----------|--------|--------------|-------|
| `p-0` | 0px | `sizeNone` | No padding |
| `p-1` | 4px | `size40` | Tight spacing |
| `p-2` | 8px | `size80` | Component padding |
| `p-3` | 12px | `size120` | Default spacing |
| `p-4` | 16px | `size160` | Section padding |
| `p-6` | 24px | `size240` | Large spacing |
| `p-8` | 32px | `size320` | Section gaps |
| `p-12` | 48px | `size480` | Page margins |

**Rule**: NEVER use arbitrary values (e.g., `p-[13px]`). Always use the 4px spacing ramp.

### Grid System
**Source**: [Fluent 2 Design System - Grid](https://fluent2.microsoft.design/layout)

**12-Column Framework** (Microsoft Standard):
- **Columns**: Building blocks for element placement
- **Gutters**: Negative space between columns (multiple of 4px)
- **Margins**: Space outside grid (fixed or percentage-based)
- **Responsive**: Gutter/margin widths change at breakpoints

**Grid Types**:
1. **Column Grid**: Most common for web apps (12 columns)
2. **Manuscript Grid**: Single column for readability
3. **Modular Grid**: Both vertical columns and horizontal rows
4. **Baseline Grid**: Dense horizontal rows for text alignment

**Implementation**:
```tsx
// Desktop: 12 columns
<div className="grid grid-cols-12 gap-4">
  <div className="col-span-3">Sidebar</div>
  <div className="col-span-9">Content</div>
</div>

// Tablet: 8 columns
<div className="grid grid-cols-8 md:grid-cols-12 gap-4">
  ...
</div>

// Mobile: 4 columns
<div className="grid grid-cols-4 md:grid-cols-8 lg:grid-cols-12 gap-4">
  ...
</div>
```

### Responsive Breakpoints
**Source**: [Fluent 2 Design System - Breakpoints](https://fluent2.microsoft.design/layout)

| Breakpoint | Range | Tailwind | Usage |
|------------|-------|----------|-------|
| Small | 0-639px | `default` | Mobile portrait |
| Medium | 640-1023px | `sm:` | Mobile landscape / small tablet |
| Large | 1024-1365px | `md:` | Tablet / small desktop |
| XLarge | 1366-1919px | `lg:` | Desktop |
| XXLarge | 1920px+ | `xl:` | Large desktop |

**Rule**: Tailwind breakpoints align well with Fluent standards. Use them consistently.

### Accessibility Standards
**Source**: [Fluent UI Accessibility](https://react.fluentui.dev/)

**Touch Targets** (Minimum Sizes):
- iOS & Web: **44 x 44 pixels**
- Android: **48 x 48 pixels**

**Implementation**:
```tsx
// ❌ BAD: Too small for mobile touch
<button className="h-6 w-6">Icon</button>

// ✅ GOOD: Responsive touch targets
<button className="h-11 w-11 md:h-7 md:w-7">Icon</button>
```

**ARIA Attributes** (Required):
- `aria-label`: For icon-only buttons
- `aria-expanded`: For collapsible/expandable elements
- `aria-live`: For dynamic content updates
- `role`: For semantic meaning when HTML5 elements aren't sufficient

```tsx
// ✅ Proper ARIA usage
<button 
  aria-label="Toggle navigation" 
  aria-expanded={isOpen}
  onClick={toggle}
>
  <MenuIcon />
</button>
```

---

## 2. SharePoint Modern Page Pattern (Official)

**Source**: Microsoft 365 / SharePoint Design Guidelines

**Application Structure**:
```
┌─────────────────────────────────────────┐
│ ┌──────────┐ ┌──────────────────────┐  │
│ │          │ │ [Header with Toggle] │  │
│ │ Sidebar  │ ├──────────────────────┤  │
│ │ (Fixed)  │ │                      │  │
│ │  Nav     │ │  Main Content        │  │
│ │          │ │  (Flex, Responsive)  │  │
│ └──────────┘ └──────────────────────┘  │
│  48-260px      Fills remaining space   │
└─────────────────────────────────────────┘
```

**Layout Rules**:
1. **Left Navigation**: Fixed/collapsible sidebar
   - Collapsed: 48-60px
   - Expanded: ~260px
2. **Content Area**: Flows ADJACENT to navigation (NOT overlapping)
3. **Header**: INSIDE content area (not separate layer)
4. **Responsive**: Mobile uses drawer/overlay pattern

**Implementation Pattern**:
```tsx
// ✅ Correct Microsoft 365 Pattern
<SidebarProvider>
  <AppSidebar />
  <main className="relative flex min-h-screen w-full flex-1 flex-col">
    <header>...</header>
    <div className="flex-1">
      <Outlet /> {/* Content */}
    </div>
    <footer>...</footer>
  </main>
</SidebarProvider>
```

**Rule**: Header must be inside `<main>`, not a sibling. Content flows naturally using Flexbox.

---

## 3. The Four Pillars of Design (Enhanced)

### A. Uniformity (The Law of Consistency)
*   **Theory:** Users should not have to wonder whether different words, situations, or actions mean the same thing.
*   **Rule:** NEVER hardcode CSS values. Use design tokens from the Fluent spacing ramp.
*   **Implementation:**
    *   **Spacing:** Use Tailwind classes that map to Fluent tokens (`p-4` = `size160`)
    *   **Components:** Always use shared components (`<PrimaryButton>`, `<VendorCard>`)
    *   **Typography:** Use the standard type scale (`text-xl font-bold`, `text-sm text-gray-500`)

### B. Proximity (Gestalt Psychology)
*   **Theory:** Objects that are near each other tend to be grouped together.
*   **Rule:** Use whitespace and boundaries to define relationships.
*   **Implementation:**
    *   Group related fields in a Card or Fieldset
    *   Use larger gaps between sections (`gap-8`) vs related items (`gap-2`)
    *   Follow Fluent's spacing hierarchy

### C. Feedback (Doherty Threshold)
*   **Theory:** System must provide reaction within <400ms to keep user attention.
*   **Rule:** Every interaction must have a visible state.
*   **Implementation:**
    *   **Buttons:** Show `Loading...` or spinner state on submit
    *   **Validation:** Inline errors appear immediately on blur
    *   **Empty States:** Never show blank spaces. Show meaningful placeholders
    *   **ARIA Live Regions:** Announce dynamic changes to screen readers

### D. Aesthetics (Visceral Design)
*   **Theory:** Attractive things work better. Users perceive beautiful designs as more usable.
*   **Rule:** "If it looks basic, it is a failure."
*   **Implementation:**
    *   **Depth:** Use subtle shadows (`shadow-sm`) to lift active elements
    *   **Scanning:** Use badges/colors for status (Green = Active, Gray = Draft)
    *   **Alignment:** Strict vertical alignment following the grid
    *   **Premium Feel:** High-quality icons, smooth transitions, polished interactions

---

## 4. Component Architecture

### Microsoft Pattern: Modular Components
**Source**: Fluent UI React v9 Component Model

```
Layout.tsx           → Page structure (CSS Grid)
├── AppSidebar.tsx   → Navigation component
├── Header           → Top bar with branding/actions
├── MainContent      → Flexible content area (Flexbox)
└── Footer           → Status/actions
```

**Rules**:
1. **Single Responsibility**: Each component does ONE thing well
2. **Composition Over Configuration**: Build complex UIs by composing simple components
3. **Semantic HTML**: Use `<header>`, `<main>`, `<nav>`, `<section>`, `<article>`
4. **No Layout Wrappers**: Don't create components that only wrap children in flex/grid

---

## 5. Dark Mode (Microsoft Standard)

**Mandate**: All components must support Dark Mode.

**Implementation**:
```tsx
// ✅ Use semantic Tailwind classes
<div className="bg-white dark:bg-slate-900 text-gray-900 dark:text-gray-100">
  Content
</div>

// ❌ Don't hardcode colors
<div style={{ backgroundColor: '#ffffff', color: '#000000' }}>
  Content
</div>
```

**Colors**: Use semantic tokens:
- `bg-background` (adapts to theme)
- `text-foreground` (adapts to theme)
- `border-border` (adapts to theme)

---

## 6. Error Handling & User Feedback (MANDATORY)

**Philosophy**: "Silent failures destroy user trust. Every action must have visible feedback."

### The Mandatory Pattern

ALL async operations MUST implement these 4 components:

#### 1. Try/Catch Block (Required)
```typescript
// ✅ CORRECT: Comprehensive error handling
const handleSubmit = async () => {
  setError(null);
  setLoading(true);
  
  try {
    await apiCall();
    setSuccess('Operation successful!');
  } catch (err: any) {
    console.error('Operation failed:', err);
    setError(err.message || 'Operation failed. Please try again.');
  } finally {
    setLoading(false);
  }
};

// ❌ WRONG: No error handling
const handleSubmit = async () => {
  await apiCall();
  setSuccess('Done!');
};
```

#### 2. Timeout Protection (Required - 10 seconds default)
```typescript
// ✅ CORRECT: Timeout protection
const loadData = async () => {
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), 10000);
  
  try {
    const data = await apiCall({ signal: controller.signal });
    clearTimeout(timeoutId);
    setData(data);
  } catch (err: any) {
    if (err.name === 'AbortError') {
      setError('Request timed out. Please check your connection.');
    } else {
      setError(err.message);
    }
  }
};

// ❌ WRONG: No timeout - can hang forever
const loadData = async () => {
  const data = await apiCall();
  setData(data);
};
```

#### 3. Loading State (Required)
```typescript
// ✅ CORRECT: Visible loading state
const [loading, setLoading] = useState(false);

return (
  <Button onClick={handleSubmit} disabled={loading}>
    {loading ? (
      <>
        <Spinner className="mr-2" />
        Saving...
      </>
    ) : 'Save'}
  </Button>
);

// ❌ WRONG: No loading indicator
return <Button onClick={handleSubmit}>Save</Button>;
```

#### 4. User Feedback (Required)
```typescript
// ✅ CORRECT: Success and error messages
const [success, setSuccess] = useState<string | null>(null);
const [error, setError] = useState<string | null>(null);

return (
  <>
    {success && (
      <div className="bg-green-50 p-4 rounded-md">
        <p className="text-green-800">{success}</p>
      </div>
    )}
    
    {error && (
      <div className="bg-red-50 p-4 rounded-md">
        <p className="text-red-800">{error}</p>
        <button onClick={() => setError(null)}>Dismiss</button>
      </div>
    )}
  </>
);

// ❌ WRONG: Silent success/failure
// User has no idea if operation succeeded
```

### Visual Feedback Standards

#### Success Messages
- **Color**: Green (`bg-green-50`, `text-green-800`)
- **Icon**: Checkmark (`✓`)
- **Duration**: Auto-dismiss after 5 seconds
- **Dismissible**: Yes (X button)

```tsx
<div className="rounded-md bg-green-50 p-4">
  <div className="flex">
    <CheckCircleIcon className="h-5 w-5 text-green-400" />
    <div className="ml-3">
      <p className="text-sm font-medium text-green-800">
        {successMessage}
      </p>
    </div>
    <button onClick={dismiss} className="ml-auto">
      <XMarkIcon className="h-5 w-5 text-green-500" />
    </button>
  </div>
</div>
```

#### Error Messages
- **Color**: Red (`bg-red-50`, `text-red-800`)
- **Icon**: X or Alert (`⚠`)
- **Duration**: Manual dismiss only
- **Dismissible**: Yes (required)
- **Retry**: Include retry button when applicable

```tsx
<div className="rounded-md bg-red-50 p-4">
  <div className="flex">
    <XCircleIcon className="h-5 w-5 text-red-400" />
    <div className="ml-3 flex-1">
      <p className="text-sm font-medium text-red-800">{error}</p>
      <button onClick={retry} className="mt-2 text-sm text-red-600">
        Retry
      </button>
    </div>
  </div>
</div>
```

#### Loading States
- **Spinner**: Animated spinner icon
- **Text**: Descriptive ("Loading...", "Saving...", "Creating...")
- **Disable**: Disable buttons during loading
- **Skeleton**: Use skeleton screens for initial page load

```tsx
{loading ? (
  <div className="flex items-center justify-center py-12">
    <Spinner className="h-8 w-8 text-brand-600" />
    <p className="ml-3 text-sm text-gray-500">Loading data...</p>
  </div>
) : (
  <DataTable data={data} />
)}
```

#### Empty States
- **Icon**: Relevant icon (no data, no results)
- **Message**: Clear explanation ("No events yet")
- **Action**: CTA button when applicable ("Create Event")

```tsx
{data.length === 0 && (
  <div className="text-center py-12">
    <CalendarIcon className="mx-auto h-12 w-12 text-gray-400" />
    <h3 className="mt-2 text-sm font-medium text-gray-900">No events</h3>
    <p className="mt-1 text-sm text-gray-500">
      Get started by creating a new event.
    </p>
    <Button onClick={onCreate} className="mt-6">
      Create Event
    </Button>
  </div>
)}
```

### FORBIDDEN Patterns

❌ **Silent Failures**:
```typescript
// NEVER DO THIS
const handleCreate = async () => {
  await createEvent(data);
  closeModal();
};
// User has no idea if it succeeded or failed!
```

❌ **Infinite Loading**:
```typescript
// NEVER DO THIS
const loadData = async () => {
  setLoading(true);
  await fetchData(); // No timeout - can hang forever
  setLoading(false);
};
```

❌ **Missing Error Handling**:
```typescript
// NEVER DO THIS
const handleSubmit = async () => {
  const result = await apiCall();
  setData(result);
};
// If apiCall fails, app crashes or shows blank screen
```

### Agent Behavior

When implementing UI components, the agent MUST:
1. ✅ Add try/catch to ALL async operations
2. ✅ Implement 10-second timeout for ALL API calls
3. ✅ Show loading state during operations
4. ✅ Display success message on completion
5. ✅ Display error message on failure
6. ✅ Implement empty state when no data
7. ✅ Make error messages dismissible
8. ✅ Include retry button when applicable

**Verification**: Before committing UI code, agent must verify:
- [ ] All async calls have try/catch
- [ ] All API calls have timeout
- [ ] Loading states are visible
- [ ] Success/error messages are implemented
- [ ] Empty states are implemented

---

## 7. Quality Checklist

Before shipping any UI component, verify:

### Layout
- [ ] Uses native CSS Flexbox/Grid (no layout wrapper components)
- [ ] Follows 12-column grid system
- [ ] Spacing uses 4px ramp (no arbitrary values)
- [ ] Responsive across all breakpoints

### Accessibility
- [ ] Touch targets are 44x44px minimum on mobile
- [ ] All interactive elements have ARIA labels
- [ ] Keyboard navigation works (Tab, Enter, Escape)
- [ ] Color contrast meets WCAG 2.1 AA standards (4.5:1)
- [ ] Screen reader announces state changes

### Components
- [ ] Uses shared components (not HTML primitives)
- [ ] Semantic HTML elements used correctly
- [ ] Dark mode fully supported
- [ ] Loading/error/empty states implemented

### Visual Polish
- [ ] Consistent spacing throughout
- [ ] Proper visual hierarchy with whitespace
- [ ] Smooth transitions and animations
- [ ] Icons from consistent library (Lucide React)

---

## 7. References

Official Microsoft Documentation:
- [Fluent UI React v9](https://react.fluentui.dev/)
- [Fluent 2 Design System](https://fluent2.microsoft.design/)
- [Microsoft 365 Design Patterns](https://learn.microsoft.com/en-us/sharepoint/dev/design/design-guidance-overview)
- [Accessibility Guidelines](https://www.microsoft.com/design/inclusive/)

Internal Standards:
- [Hexagonal Architecture Standards](./hexagonal-architecture-standards.md)
- [Data Model Standards](./data-model-standards.md)
