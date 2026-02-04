# Accessibility Standard (WCAG 2.1 AA)

**Category**: Operations & Quality
**Pattern #**: 26
**Status**: MANDATORY
**Priority**: 🟠 IMPORTANT

---

## Definition

All user interfaces MUST comply with WCAG 2.1 Level AA to ensure accessibility for users with disabilities.

---

## Rules

1. **ALWAYS** provide text alternatives for non-text content
2. **NEVER** rely solely on color to convey information
3. **ALWAYS** ensure keyboard navigability
4. **ALWAYS** maintain minimum contrast ratios
5. **NEVER** create content that flashes more than 3 times/second

---

## Core Principles (POUR)

| Principle | Requirement |
|-----------|-------------|
| **Perceivable** | Users can perceive all content |
| **Operable** | Users can navigate and interact |
| **Understandable** | Content and UI are understandable |
| **Robust** | Works with assistive technologies |

---

## Implementation Checklist

### 1. Images & Media

```tsx
// ✅ CORRECT: Alt text for images
<img src="/logo.png" alt="Vendor MDM Portal logo" />

// ✅ CORRECT: Decorative images
<img src="/divider.png" alt="" role="presentation" />

// ❌ FORBIDDEN: Missing alt
<img src="/chart.png" />

// ✅ CORRECT: Complex images
<figure>
  <img src="/workflow.png" alt="Vendor approval workflow diagram" />
  <figcaption>
    The workflow shows: Draft → Submitted → Under Review → Approved
  </figcaption>
</figure>
```

### 2. Color & Contrast

```css
/* ✅ CORRECT: Meets 4.5:1 contrast ratio */
.text-primary {
  color: #1a1a1a;  /* Dark gray on white = 12.6:1 */
  background: #ffffff;
}

/* ✅ CORRECT: Large text (3:1 minimum) */
.heading-large {
  font-size: 24px;
  color: #4a4a4a;  /* 7.4:1 on white */
}

/* ❌ FORBIDDEN: Low contrast */
.text-light {
  color: #999999;  /* 2.8:1 - fails AA */
  background: #ffffff;
}

/* ✅ CORRECT: Don't rely on color alone */
.error-field {
  border-color: #dc3545;
  border-width: 2px;  /* Visual indicator */
}
.error-field::after {
  content: "⚠";  /* Icon indicator */
}
```

### 3. Keyboard Navigation

```tsx
// ✅ CORRECT: Focusable interactive elements
<button onClick={handleClick}>Submit</button>

// ✅ CORRECT: Custom focusable element
<div
  role="button"
  tabIndex={0}
  onKeyDown={(e) => e.key === 'Enter' && handleClick()}
  onClick={handleClick}
>
  Custom Button
</div>

// ❌ FORBIDDEN: Non-focusable clickable
<div onClick={handleClick}>Click me</div>

// ✅ CORRECT: Skip link
<a href="#main-content" className="skip-link">
  Skip to main content
</a>

// ✅ CORRECT: Focus trap in modals
const Modal = ({ isOpen, onClose, children }) => {
  const modalRef = useRef();

  useEffect(() => {
    if (isOpen) {
      const focusableElements = modalRef.current.querySelectorAll(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
      );
      const firstElement = focusableElements[0];
      const lastElement = focusableElements[focusableElements.length - 1];

      firstElement?.focus();

      const handleTab = (e) => {
        if (e.key === 'Tab') {
          if (e.shiftKey && document.activeElement === firstElement) {
            e.preventDefault();
            lastElement.focus();
          } else if (!e.shiftKey && document.activeElement === lastElement) {
            e.preventDefault();
            firstElement.focus();
          }
        }
        if (e.key === 'Escape') onClose();
      };

      document.addEventListener('keydown', handleTab);
      return () => document.removeEventListener('keydown', handleTab);
    }
  }, [isOpen, onClose]);

  return isOpen ? <div ref={modalRef} role="dialog" aria-modal="true">{children}</div> : null;
};
```

### 4. Forms & Labels

```tsx
// ✅ CORRECT: Associated label
<label htmlFor="vendor-name">Vendor Name</label>
<input id="vendor-name" type="text" />

// ✅ CORRECT: Required field indication
<label htmlFor="email">
  Email <span aria-hidden="true">*</span>
  <span className="sr-only">(required)</span>
</label>
<input id="email" type="email" required aria-required="true" />

// ✅ CORRECT: Error messages
<input
  id="tax-id"
  aria-invalid={hasError}
  aria-describedby={hasError ? "tax-id-error" : undefined}
/>
{hasError && (
  <span id="tax-id-error" role="alert">
    Tax ID must be 9 digits
  </span>
)}

// ✅ CORRECT: Fieldset for groups
<fieldset>
  <legend>Vendor Type</legend>
  <input type="radio" id="supplier" name="type" />
  <label htmlFor="supplier">Supplier</label>
  <input type="radio" id="contractor" name="type" />
  <label htmlFor="contractor">Contractor</label>
</fieldset>
```

### 5. ARIA Landmarks & Roles

```tsx
// ✅ CORRECT: Page structure
<header role="banner">
  <nav role="navigation" aria-label="Main">...</nav>
</header>

<main role="main" id="main-content">
  <h1>Vendor List</h1>
  <section aria-labelledby="active-vendors">
    <h2 id="active-vendors">Active Vendors</h2>
    ...
  </section>
</main>

<aside role="complementary" aria-label="Filters">
  ...
</aside>

<footer role="contentinfo">...</footer>

// ✅ CORRECT: Dynamic content
<div aria-live="polite" aria-atomic="true">
  {statusMessage}
</div>

// ✅ CORRECT: Loading state
<div aria-busy={isLoading} aria-live="polite">
  {isLoading ? <Spinner /> : <Content />}
</div>
```

### 6. Tables

```tsx
// ✅ CORRECT: Data table
<table>
  <caption>Vendor List - 25 vendors total</caption>
  <thead>
    <tr>
      <th scope="col">Name</th>
      <th scope="col">Status</th>
      <th scope="col">Actions</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <th scope="row">Acme Corp</th>
      <td>Active</td>
      <td>
        <button aria-label="Edit Acme Corp">Edit</button>
      </td>
    </tr>
  </tbody>
</table>

// ✅ CORRECT: Sortable columns
<th scope="col">
  <button
    aria-sort={sortDirection}
    onClick={handleSort}
  >
    Name {sortDirection === 'ascending' ? '↑' : '↓'}
  </button>
</th>
```

---

## Contrast Ratios

| Element | Minimum Ratio | Example |
|---------|---------------|---------|
| Normal text | 4.5:1 | #595959 on white |
| Large text (18px+) | 3:1 | #767676 on white |
| UI components | 3:1 | Borders, icons |
| Focus indicators | 3:1 | Visible focus ring |

**Tool**: Use [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)

---

## Screen Reader Classes

```css
/* Visually hidden but accessible to screen readers */
.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

/* Skip link */
.skip-link {
  position: absolute;
  top: -40px;
  left: 0;
  padding: 8px;
  z-index: 100;
}
.skip-link:focus {
  top: 0;
}
```

---

## Testing Checklist

### Automated Testing

```bash
# Lighthouse accessibility audit
npx lighthouse http://localhost:3000 --only-categories=accessibility

# axe-core in tests
npm install --save-dev @axe-core/react
```

```tsx
// Jest + axe test
import { axe, toHaveNoViolations } from 'jest-axe';

expect.extend(toHaveNoViolations);

test('VendorList is accessible', async () => {
  const { container } = render(<VendorList />);
  const results = await axe(container);
  expect(results).toHaveNoViolations();
});
```

### Manual Testing

- [ ] Tab through entire page (logical order)
- [ ] Use screen reader (VoiceOver/NVDA)
- [ ] Test at 200% zoom
- [ ] Test with high contrast mode
- [ ] Test with reduced motion
- [ ] Test with keyboard only (no mouse)

---

## Anti-Patterns

❌ Using `outline: none` without alternative focus style
❌ Placeholder text as only label
❌ Auto-playing media without controls
❌ Time limits without extension option
❌ Content that requires specific orientation
❌ Links that say "click here" or "read more"
❌ Opening new windows without warning

---

## Agent Behavior

**Before PR**:
1. ✅ Run Lighthouse accessibility audit (score > 90)
2. ✅ Test keyboard navigation
3. ✅ Verify all images have alt text
4. ✅ Check contrast ratios
5. ✅ Verify form labels are associated

**During Code Review**:
1. ✅ Check for missing ARIA attributes
2. ✅ Verify semantic HTML usage
3. ✅ Check for color-only indicators

---

## Reference

- **WCAG 2.1**: https://www.w3.org/WAI/WCAG21/quickref/
- **axe-core**: https://github.com/dequelabs/axe-core
- **Golden Rules**: Section 4, Category 5 (Operations & Quality)
