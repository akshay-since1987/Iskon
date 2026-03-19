# ISKCON Multi-Site Application (src/)

## Overview
A **multi-tenant web portal** where visitors start at a common landing page, select a temple from a dropdown, and are routed to that temple's dedicated portal (Dhule, Chalisgaon, Shirpur, Nashik Road). Each temple has its own complete site instance with isolated data, courses, programs, timings, and media.

## User Journey

### Visitor Flow (Anonymous User)
```
1. Visitor lands on common homepage
   ↓
2. Header shows: [Home] [Temples ▼]
   ↓
3. Visitor clicks "Temples" dropdown → sees: Dhule, Chalisgaon, Shirpur, Nashik Road
   ↓
4. Visitor selects a temple (e.g., Dhule)
   ↓
5. Redirected to Dhule's portal with:
   - Dhule Home page (hero, status, info)
   - Courses (Dhule-specific)
   - Programs (Dhule-specific)
   - Media Gallery (Dhule-specific)
   - Temple Timings (Dhule-specific)
   - Login button
```

### Registered User Flow
```
1. User visits any common page
   ↓
2. Clicks "Login"
   ↓
3. Authenticates with username/password
   ↓
4. System looks up user's associated temple
   ↓
5. Auto-redirects to user's temple portal
   ↓
6. User sees their temple's content + member-only features
```

### Registration Flow
```
1. New user clicks "Register" on login modal
   ↓
2. Fills: Name, DOB, Email, Phone, etc.
   ↓
3. **Selects their temple** from dropdown (Dhule, Chalisgaon, Shirpur, Nashik Road)
   ↓
4. Account created with temple affiliation
   ↓
5. Auto-redirected to their temple's portal
```

## Application Architecture

### Frontend Structure
- **Common Pages:**
  - `/index.html` - Landing page (Home + Temple selector)
  
- **Temple-Specific Pages** (4 instances):
  - `/pages/[temple]/index.html` (Home)
  - `/pages/[temple]/courses.html`
  - `/pages/[temple]/programs.html`
  - `/pages/[temple]/media-gallery.html`
  - `/pages/[temple]/temple-timings.html`

- **Assets per Temple:**
  - `/assets/[temple]/images/` (hero, courses, programs, media)
  - `/assets/[temple]/data/` (timings, programs, courses as JSON or API-driven)

### Backend Services
- **Authentication API** - User login/registration (cross-temple)
- **Temple Selection API** - Get list of all temples, redirect logic
- **temple Data API** - Fetch temple-specific: courses, programs, timings, media
- **User Dashboard API** - Authenticated user's temple-specific content

### Database Schema (Single Shared DB)
- `Temples` table (Dhule, Chalisgaon, Shirpur, Nashik Road)
- `Users` table (TempleId FK → user tied to temple)
- `Courses` table (TempleId → each course belongs to a temple)
- `Programs` table (TempleId → each program belongs to a temple)
- `TempleTimings` table (TempleId → ritual timings per temple)
- `MediaGallery` table (TempleId → media per temple)
- `Media Files` (stored in cloud: AWS S3 / Azure Blob / local storage with temple prefix)

## Tech Stack

### Backend
- **Framework:** ASP.NET Core 8 LTS (MVC pattern)
- **View Engine:** Razor (dynamic server-rendered HTML)
- **Databases:** MS SQL Server (dual-database strategy, multi-tenant)
  - **Iskcon-Master:** Authoring/drafts database (admin operations)
  - **Iskcon-Web:** Published content database (frontend reads only)
- **ORM:** Entity Framework Core (multi-tenant data isolation via TempleId)
- **Authentication:** ASP.NET Identity (MVC cookies) + JWT (future APIs)

### Frontend
- **Rendering:** Razor MVC (server-side rendered views)
- **Styling:** CSS (from Sample template)
- **Interactivity:** JavaScript (form validation, AJAX calls)
- **Live Streaming:** RTMP stream URL embedding (no server-side recording)

### File Storage
- **Images & Assets:** Server-side file system (organized by temple)
  - File paths stored in database
  - Direct file access via static files middleware
  - Example: `/uploads/temples/dhule/courses/{courseId}/{filename}`

### Live Streaming
- **Own Authentic Stream (self-hosted, not embed YT/Twitch)**
- **Broadcast Tool:** OBS (Open Broadcaster Software)
- **Infrastructure:** Nginx RTMP module or similar (RTMP input → HLS output)
  - OBS streams to RTMP endpoint: `rtmp://your-server/live/event-name`
  - Nginx converts stream to HLS format for web playback
  - Playback URL stored in database (Event/Course record)
- **Playback:** HLS.js or native HTML5 video player embedded on event detail page
- **No Server Recording:** Stream is live-only (no persistent video files captured)
- **Interaction:** Deferred to Phase 2 (chat, participant count, etc.)
- **Access Control:** Optional metadata for registered vs. public broadcasts (Phase 2)

---

## Dual-Database Strategy Documentation

### **Database Configuration**

**Instance:** `.\SQLEXPRESS`
**Authentication:** Windows Authentication (`Trusted_Connection=true`)

#### **Iskcon-Master Database**
**Purpose:** Content authoring, drafting, and administration
- **Database Name:** `Iskon_Master`
- **Connection String:**
  ```
   Server=.\SQLEXPRESS;Database=Iskon_Master;Trusted_Connection=true;Encrypt=false;TrustServerCertificate=True;
  ```
- **Users:** Admin staff, content editors
- **Access Level:** Full CRUD operations
- **Content State Tracking:**
  - `IsDraft` (boolean) - Content not ready for publication
  - `IsPublished` (boolean) - Content approved and live
  - `PublishedDate` (DateTime) - When content was published
  - `LastModifiedDate` (DateTime) - Last edit timestamp
- **Tables with State Flags:**
  - Events (drafts + published)
  - Courses (drafts + published)
  - MediaGallery (unpublished media)
  - TempleTimings (version control)
- **Data Retention:** All versions, complete audit trail, soft deletes
- **Multi-Tenant:** All tables include `TempleId` (GUID FK for Dhule, Chalisgaon, Shirpur, Nashik Road)

#### **Iskcon-Web Database**
**Purpose:** Published content delivery to frontend (read-only)
- **Database Name:** `Iskon_Web`
- **Connection String:**
  ```
   Server=.\SQLEXPRESS;Database=Iskon_Web;Trusted_Connection=true;Encrypt=false;TrustServerCertificate=True;
  ```
- **Users:** Website visitors, mobile app clients
- **Access Level:** Read-only (SELECT queries only)
- **Content Visibility:** Only `IsPublished = true` records
- **Data Sync:** Synchronized from Iskcon-Master via:
  - Application-level sync service (after publish action)
  - SQL trigger (optional, for real-time sync)
  - Scheduled batch sync (nightly, fallback)
- **Data Cleanliness:** Only current published versions (lean, optimized database)
- **Multi-Tenant:** All tables include `TempleId` for temple-specific filtering
- **Schema:** Identical to Iskcon-Master (published tables only)

### **Data Flow & Content Lifecycle**
```
1. Admin creates Event/Course in Iskcon-Master (IsPublished = false)
   ↓
2. Admin reviews and marks IsPublished = true
   ↓
3. Sync Service detects publish action
   ↓
4. Inserts/Updates record in Iskcon-Web (exact copy)
   ↓
5. Frontend queries Iskcon-Web (renders published content)
   ↓
6. Visitor sees the published Event/Course on website
```

### **Benefits of Dual-Database Approach**
1. **Content Safety:** Drafts never exposed to public
2. **Approval Workflow:** Admin review before publishing
3. **Rollback Capability:** Can revert Iskcon-Web without losing draft history
4. **Performance Optimization:** Iskcon-Web tuned for read-heavy frontend queries
5. **Scalability:** Iskcon-Web can be replicated, cached, or distributed
6. **Audit & Compliance:** Iskcon-Master maintains complete history
7. **Version Control:** Easy to publish/unpublish content versions

### **Sync Strategy**
- **Event:** Admin clicks "Publish" button in admin panel
- **Trigger:** Application service executes sync method
- **Operation:** 
  ```sql
  -- In Iskcon-Web DB:
  INSERT INTO Events (Id, TempleId, Title, Description, ..., PublishedDate)
  VALUES (...) WHERE IsPublished = true
  
  -- OR UPDATE if already exists
  UPDATE Events SET ... WHERE Id = @Id AND TempleId = @TempleId
  ```
- **Fallback:** Nightly batch job syncs all published changes (recovery mechanism)

### **Multi-Tenant Filtering**
Both databases filter by `TempleId` at query time:
```csharp
// In service/API:
var templeId = GetCurrentTempleContext();
var publishedEvents = dbContext.Events
  .Where(e => e.TempleId == templeId && e.IsPublished)
  .ToList();
```

---

## Project Scope & Decisions ✅

### Phase 1: Complete Website for 1 Location (Dhule)
- Build full stack with scope to add other temples later (Chalisgaon, Shirpur, Nashik Road)
- Single temple deployment, but DB schema supports multi-tenant from day 1

### Admin Panel Features
**Events Management:**
- Upload event image
- Event title, description, detailed info, location
- Live event link (YouTube/Twitch embed, or custom streaming)
- Event registration link
- Capture event registrations (user name, email, phone, temple affiliation)

**Courses Management:**
- Upload course image
- Course title, description, detailed info
- Course registration link
- Capture course enrollments (user name, email, phone, temple affiliation)

**Data Display on Website:**
- Events show up on Programs page with image, title, description, registration button
- Courses show up on Courses page with image, title, description, registration button
- Event/Course registration links redirect to external form or in-app enrollment

### Future Feature: Live Streaming
- **Architecture ready for:**
  - Embed live RTMP stream player (YouTube/Twitch/OBS output)
  - Stream URL stored in Event/Course database record
  - Access control metadata (registered vs. public broadcasts)
  - **NO server-side video capture/recording** (external services only if needed later)
  - Stream playback via iframe or HLS.js client-side player
  - Note: Recording capability deferred to third-party service (if required)

## APIs (RESTful)

### Temple APIs
```
GET /api/temples                        → List all temples
GET /api/temples/{templeId}             → Get temple details
```

### Authentication APIs
```
POST /api/auth/register                 → Register new user
POST /api/auth/login                    → Authenticate user
POST /api/auth/logout                   → Logout
GET /api/auth/refresh                   → Refresh JWT token
GET /api/user/me                        → Get logged user profile
```

### Events APIs
```
GET /api/temples/{templeId}/events      → Get all events for temple
GET /api/events/{eventId}               → Get event details
POST /api/events/{eventId}/register     → Register user for event
GET /api/events/{eventId}/registrations → Get event registrations (admin only)
```

### Courses APIs
```
GET /api/temples/{templeId}/courses     → Get all courses for temple
GET /api/courses/{courseId}             → Get course details
POST /api/courses/{courseId}/enroll     → Enroll user in course
GET /api/courses/{courseId}/enrollments → Get course enrollments (admin only)
```

### Admin APIs
```
POST /api/admin/courses                 → Create course
PUT /api/admin/courses/{courseId}       → Update course
DELETE /api/admin/courses/{courseId}    → Delete course
POST /api/admin/courses/{courseId}/image → Upload course image
POST /api/admin/events                  → Create event
PUT /api/admin/events/{eventId}         → Update event
DELETE /api/admin/events/{eventId}      → Delete event
POST /api/admin/events/{eventId}/image  → Upload event image
```

### Timings & Media APIs
```
GET /api/temples/{templeId}/timings     → Get temple timings
GET /api/temples/{templeId}/media       → Get media gallery
```

## Phase 1 Implementation Plan

### 1.1 Foundation (Database + Core Structure)
- [ ] Set up ASP.NET Core 8 MVC project with Razor views
- [ ] Configure static files middleware (/uploads, /assets)
- [ ] Set up Nginx RTMP module (on server or local dev)
  - RTMP input from OBS: `rtmp://server/live/`
  - HLS output: `/hls/stream.m3u8` (for web playback)
  - Configuration: Nginx conf with RTMP + HLS directives
- [ ] Design & migrate database schema:
  - `Temples` (Dhule, future: Chalisgaon, Shirpur, Nashik Road)
  - `Users` (with TempleId, email, phone)
  - `Courses` (ImagePath, title, description, details, registration link, **LiveStreamUrl**)
  - `CourseEnrollments` (user + course + registration date)
  - `Events` (ImagePath, title, description, details, location, registration link, **LiveStreamUrl**, **IsLive** bool)
  - `EventRegistrations` (user + event + registration date)
  - `TempleTimings` (arati schedule)
  - `MediaGallery` (ImagePath, caption)
- [ ] Set up ASP.NET Identity for user authentication
- [ ] Configure file upload handler (save images to `/uploads/temples/{templeId}/`)

### 1.2 Admin Panel (Razor MVC Views + Controllers)
- [ ] Admin authentication/authorization
- [ ] Admin dashboard (layout & navigation)
- [ ] Events CRUD (Razor forms):
  - POST form: Image file upload, title, description, details, location, registration link
  - **Live streaming toggle:** Enable/disable; input field for HLS stream URL (e.g., `http://server:8080/hls/event1/stream.m3u8`)
  - File handler: Save image → return file path → store in database
  - List view: All events with edit/delete buttons, **live status indicator**
  - Edit form: Pre-populate & update event
  - View registrations (table with user details)
- [ ] Courses CRUD (Razor forms):
  - POST form: Image file upload, title, description, details, registration link
  - **Live streaming toggle:** Enable/disable; input field for HLS stream URL
  - File handler: Save image → return file path → store in database
  - List view: All courses with edit/delete buttons, **live status indicator**
  - Edit form: Pre-populate & update course
  - View enrollments (table with user details)

### 1.3 Public Website Views (Razor MVC)
- [ ] Create Razor layouts (shared header, footer, navigation)
- [ ] Home page (/Home/Index)
- [ ] Temple dropdown selector → redirects to temple context (cookie/session)
- [ ] Courses page (/Courses/Index) → Fetch courses from DB → Display cards
- [ ] Programs/Events page (/Programs/Index) → Fetch events from DB → Display cards
- [ ] Course detail page (/Courses/Details/{id}) → Show image from path, registration link, **live player if IsLive=true**
- [ ] Event detail page (/Events/Details/{id}) → Show image from path, registration link, **live player if IsLive=true**
  - **If **LiveStreamUrl** exists:** Embed HLS.js player with stream URL
  - Display: "**LIVE NOW**" badge when stream is active
- [ ] Temple timings page (/Temple/Timings)
- [ ] Media gallery page (/Gallery/Index)

### 1.4 User Registration & Login
- [ ] Register view (Razor form): Email, password, name, phone, temple selection
- [ ] Login view (Razor form): Email, password
- [ ] Create account → stored in database with TempleId
- [ ] Login → redirect user to their temple's home page
- [ ] Dashboard (logged-in user): Show my enrollments/registrations

### 1.5 API Endpoints (Optional for future mobile/SPA)
- [ ] REST APIs for mobile/external consumption (scaffold endpoints for Events, Courses, Auth)
- [ ] Controllers: EventsApiController, CoursesApiController, AuthApiController

### 1.6 Testing & Deployment
- [ ] Unit tests (service layer)
- [ ] Integration tests (database operations)
- [ ] Manual testing (admin flow, public views)
- [ ] File upload validation (size, type)
- [ ] Live stream testing (OBS → Nginx → HLS playback)
- [ ] Error handling & logging

---

## Live Streaming Setup (OBS + Nginx RTMP)

### OBS Configuration
1. Open OBS Studio
2. **Settings → Stream:**
   - Service: Custom RTMP Server
   - Server: `rtmp://your-server-ip:1935/live`
   - Stream Key: `dhule-event-1` (or unique identifier)
   - Example full URL: `rtmp://your-server-ip:1935/live/dhule-event-1`

3. **Start Stream** → OBS broadcasts to Nginx server

### Nginx Configuration (on server)
```nginx
rtmp {
    server {
        listen 1935;
        chunk_size 4096;

        application live {
            live on;
            record off;  # No recording
            hls on;
            hls_path /mnt/hls/;
            hls_fragment 3;
            hls_playlist_length 20;
        }
    }
}

http {
    server {
        listen 8080;

        location /hls {
            types {
                application/vnd.apple.mpegurl m3u8;
                video/mp2t ts;
            }
            alias /mnt/hls/;
            expires -1;
        }
    }
}
```

### How It Works
1. OBS streams to `rtmp://server/live/dhule-event-1`
2. Nginx receives RTMP stream and converts to HLS
3. HLS stream available at: `http://server:8080/hls/dhule-event-1/stream.m3u8`
4. Admin stores this URL in database Event/Course record
5. Website embeds HLS.js player → plays stream on event/course detail page

### HLS.js Web Player (Razor View)
```html
@if (Model.IsLive && !String.IsNullOrEmpty(Model.LiveStreamUrl))
{
    <div class="live-container">
        <div class="live-badge">🔴 LIVE NOW</div>
        <video id="livePlayer" class="video-js" controls style="width: 100%; height: auto;"></video>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/hls.js@latest"></script>
    <script>
        var video = document.getElementById('livePlayer');
        var hls = new Hls();
        hls.loadSource('@Model.LiveStreamUrl');
        hls.attachMedia(video);
    </script>
}
```

---

## Future Enhancements (Phase 2+)
- Real-time chat during live streams (SignalR)
- Viewer count & analytics
- Save stream recordings (third-party service)
- Access control (members-only streams)
- Stream scheduling & countdowns
- Multi-camera support (OBS scenes)