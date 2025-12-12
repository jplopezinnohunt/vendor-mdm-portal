---
description: Generate professional Azure Architecture Diagrams using strict design principles (Whitespace, Orthogonal Lines, Official Icons).
---

# Azure Architecture Diagram Generation

This workflow is used to generate high-quality, professional Azure architecture diagrams that adhere to a specific "Clean & Minimal" design language.

## Design Style Guide

### 1. Core Philosophy: Clarity Over Clutter
*   **Objective**: Reduce cognitive load.
*   **Golden Rule**: If an element does not add structural information, remove it.

### 2. Spacing & Layout (The Grid)
*   **Whitespace**: Start with a wide canvas. Maintain a "buffer zone" around every component.
*   **Ratio**: The space between two icons should be roughly **1.5x** the width of the icon itself.
*   **Alignment**: Align elements to a strict central axis (vertical or horizontal). Do not "stair-step" unless representing hierarchy.
*   **Grouping**: Use **whitespace/proximity** to imply groups. **DO NOT** use heavy, colored background boxes. If a container is necessary, use a dotted, light-grey stroke with no fill.

### 3. Iconography
*   **Source**: Use valid Azure Service Icons style (flat/semi-flat, blue palette).
*   **Scale**: Usage small, uniform icons (~48px-64px equivalent). **DO NOT** scale icons up to fill space.
*   **Consistency**: All icons must match the same style/library.

### 4. Connectors (The Wiring)
*   **Style**: **Orthogonal Only** (90-degree elbow turns). No diagonals or curves.
*   **Weight**: **Thin** (1pt or 1.5pt).
*   **Color**: **Neutral Grey** or Muted Blue (e.g., #597089 or #7F8C8D). **NEVER** use saturated "Hyperlink Blue".
*   **Types**: 
    *   *Solid*: Direct dependency.
    *   *Dashed*: Transient connection (e.g., Invitation Link).
*   **Routing**: Lines must not cross through text or icons.

### 5. Typography
*   **Font**: Segoe UI / Roboto / Open Sans (Clean Sans-Serif).
*   **Size**: Small. Text is secondary to the icon.
*   **Color**: Dark Grey / Charcoal (Not Black, Not Blue).
*   **Alignment**: Centered below or Left-aligned to the right of the icon.

### 6. Special Rules for "Status" Diagrams
*   Only when explicitly requested (e.g., "V3 Status Map"):
*   Add **SMALL, SUBTLE** indicators (Green Check / Red X) overlaying the icon corner.
*   **DO NOT** use large badges that obscure the icon or clutter the layout.

---

## Prompt Template

Copy and paste this prompt structure for DALL-E / Image Generation:

> **Role**: Expert Technical Illustrator.
> **Task**: Create a system architecture diagram for [System Name].
> **Style**: Official Microsoft Azure Documentation (Clean, White Background, Blue Icons).
> 
> **CRITICAL VISUAL RULES (DO NOT BREAK)**:
> 1.  **Layout**: [Horizontal/Left-to-Right].
> 2.  **Connectors**: THIN (1pt), GREY (#7F8C8D), ORTHOGONAL (90-degree angles ONLY). No curves.
> 3.  **Whitespace**: Massive spacing between elements (1.5x icon width). No overlapping.
> 4.  **Grouping**: Use WHITESPACE to group. **NO COLORED BACKGROUND BOXES**.
> 5.  **Scaling**: Keep icons SMALL. Text SMALL and GREY.
> 
> **Components**:
> [List of Components and their connections]
> 
> **Status Overlays** (Only if requested):
> [Component Name]: [Status Icon - e.g., Small Green Check]

## Quick Checklist for Approval
Before generating or finalizing a design, verify:
- [ ] Are all icons from the same official library?
- [ ] Is there equal spacing between all major components?
- [ ] Are all connector lines straight (90-degree angles)?
- [ ] Have all "status" badges been minimized or removed (unless requested)?
- [ ] Is the text hierarchy (Bold vs Regular) applied consistently?
