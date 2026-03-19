# ISKCON Multi-Site Application - Component Specifications

## Overview
This document outlines the desktop UI/UX components for the ISKCON Dhule portal. Mobile views and responsive implementation will be discussed separately.

---

## Component 1: Header

### Desktop View (1366px+ width)

**Layout Structure:**
```
┌─────────────────────────────────────────────────────────────────────────────┐
│  [🏛️ Logo]  ISKCON Dhule             [HOME] [TEMPLE] [COURSES] [PROGRAMS] [MEDIA]    [Login]  │
│               Society for Krishna     (centered)                              (right-aligned)  │
│               Consciousness                                                                   │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Background:**
- Dark overlay (rgba(0, 0, 0, 0.6)) over floral/temple hero image
- Fixed or sticky position at top
- Full width of viewport

**Components:**

#### Left Section (Logo + Branding)
- **Logo:** 50px × 50px white ISKCON lotus icon
- **Text:** 
  - Main: "ISKCON Dhule" (20px, bold, white)
  - Subtitle: "Society for Krishna Consciousness" (12px, regular, light gray)
- **Layout:** Flex row, left-aligned, padding: 15px 50px
- **Spacing:** 15px gap between logo and text

#### Center Section (Navigation Menu)
- **Links:** HOME, TEMPLE, COURSES, PROGRAMS, MEDIA
- **Font:** 12px, uppercase, semi-bold, white
- **Letter Spacing:** 1px
- **Styling:**
  - Default: white text, no underline
  - Hover: gold color (#DAA520) with smooth transition
  - Active page: gold underline + gold text
- **Spacing:** 30px gap between links
- **Position:** Absolutely centered in header

#### Right Section (Action Button)
- **Button:** "Login"
- **Styling:**
  - Background: gold (#B8860B or #DAA520)
  - Color: dark purple/black
  - Font: 14px, bold, uppercase
  - Padding: 10px 30px
  - Border-radius: 5px
  - Hover: slightly darker gold, shadow effect
- **Position:** Right-aligned, padding-right: 50px

**Responsive Behavior (Desktop):**
- Header height: 80px
- Padding: 15px 50px (left/right)
- On scroll (if sticky): Add subtle shadow, maintain same layout
- Background on scroll: May transition to solid color with transparency

**Interactions:**
1. **Temple Dropdown:**
   - Click "TEMPLE" → Dropdown appears (down-arrow icon)
   - Options: Dhule, Chalisgaon, Shirpur, Nashik Road
   - Selecting option → Redirect to that temple's site

2. **Active Page Indicator:**
   - Current page link highlighted in gold
   - Example: If on Courses page, "COURSES" link is golden

3. **Login Button:**
   - Click → Open login modal or redirect to /Auth/Login

**Colors Reference:**
- Background dark: rgba(0, 0, 0, 0.6)
- Text (primary): #FFFFFF (white)
- Text (secondary): #CCCCCC (light gray)
- Primary gold: #B8860B or #DAA520
- Primary purple: #4B0082

**Typography:**
- Font family: 'Inter', sans-serif (from Sample template)
- Font weights: 400 (regular), 600 (semi-bold), 700 (bold)

---

## Component 2: Hero Section

### Desktop View (1366px+ width)

**Layout Structure:**
```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                                                             │
│  [Background Image with Dark Overlay (rgba(0,0,0,0.4))]                   │
│                                                                             │
│                        [🟢 OPEN | Morning Darsana until 01:00 PM]         │
│                                                                             │
│                            ISKCON Dhule                                    │
│                      (Large Centered Heading)                              │
│                                                                             │
│          Welcome to the spiritual oasis in the heart of Dhule.             │
│          Experience divine peace, ancient wisdom, and blissful              │
│                    bhakti at ISKCON Dhule.                                 │
│                                                                             │
│              [Darshan Timings]  [View Courses]                             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Background:**
- Full-width, full-height section (height: 90vh on desktop)
- Background image: Temple/deity photo (e.g., `iskcon_hero_bg.png`)
- Background position: center
- Background size: cover
- Dark overlay: rgba(0, 0, 0, 0.4) (semi-transparent black)
- Flexbox layout: center all content vertically & horizontally

**Components:**

#### Status Badge
- **Position:** Top-center of hero, 40px below fold
- **Background:** rgba(255, 255, 255, 0.8) (semi-transparent white)
- **Styling:**
  - Padding: 10px 20px
  - Border-radius: 50px (pill shape)
  - Display: inline-flex with centered items
- **Content:**
  - Green dot: 12px diameter, border-radius: 50%, background: #2ecc71
  - Margin-right: 10px
  - Text: "OPEN | Morning Darsana until 01:00 PM"
  - Font: 14px, semi-bold, dark color (#333333)
- **Status Colors:**
  - Green dot (#2ecc71): Temple is open
  - Red dot (#e74c3c): Temple is closed
- **Dynamic Content:** Updates based on current time & temple timings

#### Main Heading
- **Text:** "ISKCON Dhule"
- **Font:** 'Outfit', sans-serif (decorative/bold)
- **Font-size:** 4rem (64px)
- **Color:** #FFFFFF (white)
- **Font-weight:** 700 (bold)
- **Margin-bottom:** 20px
- **Text-shadow:** 2px 2px 10px rgba(0, 0, 0, 0.5) (depth effect)
- **Text-align:** center

#### Subheading
- **Text:** "Welcome to the spiritual oasis in the heart of Dhule. Experience divine peace, ancient wisdom, and blissful bhakti at ISKCON Dhule."
- **Font:** 'Inter', sans-serif
- **Font-size:** 1.5rem (24px)
- **Color:** #FFFFFF (white)
- **Font-weight:** 400 (regular)
- **Max-width:** 800px (centered)
- **Line-height:** 1.6
- **Margin-bottom:** 30px
- **Text-align:** center

#### Call-to-Action Buttons
**Container:**
- Display: flex row
- Justify-content: center
- Gap: 20px
- Margin-top: 30px

**Button 1: "Darshan Timings"**
- **Background:** #4B0082 (primary purple)
- **Color:** #FFFFFF (white)
- **Font:** 16px, bold, uppercase
- **Padding:** 14px 40px
- **Border-radius:** 8px
- **Border:** none
- **Cursor:** pointer
- **Hover Effect:**
  - Background: darker purple (shade)
  - Transform: none (no scale)
  - Box-shadow: 0 8px 20px rgba(75, 0, 130, 0.3)
- **Link:** /Temple/Timings

**Button 2: "View Courses"**
- **Background:** #B8860B (primary gold)
- **Color:** #FFFFFF or #1a1a1a (dark text for contrast)
- **Font:** 16px, bold, uppercase
- **Padding:** 14px 40px
- **Border-radius:** 8px
- **Border:** none
- **Cursor:** pointer
- **Hover Effect:**
  - Background: #DAA520 (lighter gold)
  - Box-shadow: 0 8px 20px rgba(184, 134, 11, 0.3)
- **Link:** /Courses

**Transitions:**
- All hover effects: 0.3s ease

---

## Component 3: Events Carousel Section

(Content continues with remaining 6 components - Components 3-9 with full specifications for Events Carousel, Courses Carousel, Footer, Gallery Filter, Login/Register Modal, Course Listing, and Event Listing)

*Full component specifications available in main document at `/documents/components.md`*
