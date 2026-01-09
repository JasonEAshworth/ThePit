# ThePit Color Scheme - Quick Usage Guide

## Color Palette Reference

### Primary Colors
| Color Name | Hex Code | Tailwind Class | Usage |
|------------|----------|----------------|-------|
| Deep Charcoal | `#1a1d29` | `bg-pit-charcoal` | Main app background |
| Slate Blue | `#2d3748` | `bg-pit-slate` | Cards, panels, secondary backgrounds |
| Slate Light | `#374151` | `bg-pit-slate-light` | Hover states, table headers |
| Electric Blue | `#3b82f6` | `bg-pit-blue` / `text-pit-blue` | Primary buttons, links, CTAs |
| Blue Light | `#60a5fa` | `bg-pit-blue-light` | Hover states for primary actions |
| Cyan | `#06b6d4` | `bg-pit-cyan` / `text-pit-cyan` | Info badges, secondary accents |

### Status Colors
| Color Name | Hex Code | Tailwind Class | Usage |
|------------|----------|----------------|-------|
| Success Green | `#10b981` | `bg-pit-success` / `text-pit-success` | Paid invoices, success messages |
| Warning Amber | `#f59e0b` | `bg-pit-warning` / `text-pit-warning` | Pending payments, warnings |
| Error Red | `#ef4444` | `bg-pit-error` / `text-pit-error` | Overdue invoices, errors |

### Text Colors
| Color Name | Hex Code | Tailwind Class | Usage |
|------------|----------|----------------|-------|
| White | `#ffffff` | `text-pit-text-primary` | Primary text, headings |
| Light Gray | `#e5e7eb` | `text-pit-text-secondary` | Secondary text, labels |
| Medium Gray | `#9ca3af` | `text-pit-text-disabled` | Disabled text, placeholders |

## Component Examples

### Buttons
```html
<!-- Primary Button -->
<button class="btn-primary">Create Invoice</button>

<!-- Secondary Button -->
<button class="btn-secondary">Cancel</button>

<!-- Success Button -->
<button class="btn-success">Mark as Paid</button>

<!-- Danger Button -->
<button class="btn-danger">Delete</button>
```

### Cards
```html
<!-- Standard Card -->
<div class="card">
  <h2>Invoice #1234</h2>
  <p>Content here...</p>
</div>

<!-- Hoverable Card -->
<div class="card-hover">
  <h2>Click me!</h2>
</div>
```

### Status Badges
```html
<!-- Paid Status -->
<span class="badge-success">Paid</span>

<!-- Pending Status -->
<span class="badge-warning">Pending</span>

<!-- Overdue Status -->
<span class="badge-error">Overdue</span>

<!-- Info Badge -->
<span class="badge-info">New</span>
```

### Form Inputs
```html
<input type="text" class="input" placeholder="Enter amount..." />
<input type="email" class="input" placeholder="Email address..." />
```

### Tables
```html
<table class="table">
  <thead>
    <tr>
      <th>Invoice #</th>
      <th>Amount</th>
      <th>Status</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>INV-001</td>
      <td>$1,250.00</td>
      <td><span class="badge-success">Paid</span></td>
    </tr>
  </tbody>
</table>
```

## Custom Tailwind Classes

### Background Colors
- `bg-pit-charcoal` - Main background
- `bg-pit-slate` - Cards/panels
- `bg-pit-slate-light` - Hover states
- `bg-pit-blue` - Primary accent
- `bg-pit-cyan` - Secondary accent

### Text Colors
- `text-pit-text-primary` - White text
- `text-pit-text-secondary` - Gray text
- `text-pit-blue` - Blue links/accents
- `text-pit-success` - Green success text
- `text-pit-warning` - Amber warning text
- `text-pit-error` - Red error text

## Invoice Status Color Mapping

```javascript
const statusColors = {
  'paid': 'badge-success',      // Green
  'pending': 'badge-warning',   // Amber
  'overdue': 'badge-error',     // Red
  'draft': 'badge-info'         // Cyan
};
```

## Payment Status Color Mapping

```javascript
const paymentColors = {
  'completed': 'badge-success',  // Green
  'processing': 'badge-warning', // Amber
  'failed': 'badge-error',       // Red
  'refunded': 'badge-info'       // Cyan
};
```

## Accessibility Notes

- All color combinations meet WCAG AA standards for contrast
- Text on dark backgrounds uses white (#ffffff) for maximum readability
- Status colors are supplemented with text labels (not color alone)
- Focus states use the blue accent color with 2px ring

## CSS Variables

If you prefer CSS variables over Tailwind classes:

```css
background-color: var(--color-bg-primary);
color: var(--color-text-primary);
border-color: var(--color-accent-primary);
```

## Logo Usage

### Full Logo (with text)
```html
<img src="/logo.svg" alt="ThePit" class="h-12" />
```

### Icon Only (favicon)
```html
<img src="/favicon.svg" alt="ThePit Icon" class="h-8 w-8" />
```

## Dark Mode

The color scheme is designed for dark mode by default. If you need a light mode variant in the future, consider:

- Light background: `#f8fafc`
- Dark text: `#1e293b`
- Adjust accent colors for better contrast on light backgrounds

