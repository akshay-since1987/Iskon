# ISKCON Multi-Site Portal - Solution Architecture

## Project Structure Overview

This solution implements a professional multi-project architecture for the ISKCON portal:

```
IskconWeb.sln
├── IskconWeb.Core (Shared Layer)
│   ├── Models/Entities.cs (8 domain entities)
│   ├── Data/DbContexts.cs (Master + Web contexts)
│   ├── Services/DbInitializationService.cs
│   └── Services/ContentSyncService.cs
│
├── IskconWeb.API (RESTful Backend, Port 7001)
│   ├── Controllers/ (Events, Courses, Temples CRUD)
│   ├── DTOs/ (Data Transfer Objects)
│   ├── Middleware/
│   ├── Program.cs (DI, migrations, seeding)
│   └── appsettings.json
│
├── IskconWeb.Web (Public Website, Port 7002)
│   ├── Controllers/ (Home, Courses, Events, Gallery, Timings)
│   ├── Views/ (Razor templates)
│   ├── wwwroot/ (CSS, JS, images, cache)
│   ├── Program.cs (API consumer, caching)
│   └── appsettings.json
│
└── IskconWeb.Admin (CMS Panel, Port 7003)
    ├── Controllers/ (Dashboard, Events, Courses admin)
    ├── Views/ (CRUD forms, admin dashboard)
    ├── wwwroot/ (Admin assets)
    ├── Program.cs (API consumer)
    └── appsettings.json
```

## Key Features

### Architecture Principles
- **Layered Design:** Clear separation between data, business logic, and presentation
- **API-First:** All business logic centralized in API layer
- **Multi-Tenancy:** TempleId (GUID) for row-level filtering supporting 4 temples
- **Content Publishing Workflow:** Draft → Published with synchronized read-only Web DB
- **Caching Strategy:** Public pages cached, admin operations bypass cache

### Database Strategy
- **Master DB (Iskcon_Master):** Complete authoring database (drafts + published)
- **Web DB (Iskcon_Web):** Published content only, optimized for read-heavy public site
- **Automatic Sync:** ContentSyncService publishes Master → Web on publish action
- **Credentials:** parth\SQLEXPRESS, sa/server@123

### Security
- ASP.NET Identity with PBKDF2 password hashing
- Role-based access control (Admin, Manager, User)
- CORS configured for inter-project communication
- Multi-tenancy enforcement at application layer
- Admin user: admin@iskcon.com / Admin@123456

### Caching
- MemoryCache with TTL (Events: 15 min, Courses: 30 min, Timings: 60 min)
- Admin operations bypass cache for fresh data
- Event-based cache invalidation on content changes

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (parth\SQLEXPRESS)
- Visual Studio 2022 or VS Code

### Build & Run

```powershell
# Restore NuGet packages
dotnet restore

# Build solution
dotnet build

# Run API (from IskconWeb.API directory)
dotnet run --project IskconWeb.API

# Run Web (from IskconWeb.Web directory, in separate terminal)
dotnet run --project IskconWeb.Web

# Run Admin (from IskconWeb.Admin directory, in separate terminal)
dotnet run --project IskconWeb.Admin
```

### First Launch
- Databases auto-created: Iskcon_Master, Iskcon_Web
- Tables auto-migrated with EF Core
- 4 temples seeded with default data
- Admin user created: admin@iskcon.com / Admin@123456

## API Endpoints

### Temples
- `GET /api/temples` - List all temples
- `GET /api/temples/{id}` - Get temple details
- `GET /api/temples/{id}/details` - Get temple with events, courses, timings

### Events
- `GET /api/events/temple/{templeId}` - List published events
- `GET /api/events/{id}` - Get event details
- `POST /api/events` - Create event (Draft)
- `PUT /api/events/{id}` - Update event
- `POST /api/events/{id}/publish` - Publish & sync to Web DB
- `DELETE /api/events/{id}` - Delete event

### Courses
- `GET /api/courses/temple/{templeId}` - List published courses
- `GET /api/courses/{id}` - Get course details
- `POST /api/courses` - Create course (Draft)
- `PUT /api/courses/{id}` - Update course
- `POST /api/courses/{id}/publish` - Publish & sync to Web DB
- `DELETE /api/courses/{id}` - Delete course

## Web Application Routes

### Public Site (IskconWeb.Web)
- `/` - Home page with temple selector
- `/Courses` - List courses by temple
- `/Courses/Details/{id}` - Course details
- `/Events` - List events by temple
- `/Events/Details/{id}` - Event details
- `/Gallery` - Media gallery by temple
- `/TempleTimings` - Arati timings by temple

### Admin Portal (IskconWeb.Admin)
- `/Dashboard` - Admin dashboard
- `/EventsAdmin` - Event management (CRUD + Publish)
- `/CoursesAdmin` - Course management (CRUD + Publish)

## Domain Models (8 Entities)

1. **Temple** - 4 ISKCON temple locations
2. **User** - Admin/staff accounts with roles
3. **Event** - Temple events with registration support
4. **Course** - Spiritual courses with enrollment
5. **TempleTimings** - Arati/prayer schedule
6. **MediaGallery** - Images/videos by temple
7. **EventRegistration** - User registrations for events
8. **CourseEnrollment** - User enrollments for courses

## Configuration Files

### IskconWeb.API/appsettings.json
```json
{
  "ConnectionStrings": {
    "MasterDb": "Server=parth\\SQLEXPRESS;Database=Iskcon_Master;...",
    "WebDb": "Server=parth\\SQLEXPRESS;Database=Iskcon_Web;..."
  },
  "ApiConfig": {
    "CorsAllowedOrigins": ["https://localhost:7002", "https://localhost:7003"],
    "JwtExpirationMinutes": 60
  }
}
```

### IskconWeb.Web/appsettings.json
```json
{
  "ApiConfig": { "BaseUrl": "https://localhost:7001" },
  "CacheConfig": {
    "EventCacheDurationMinutes": 15,
    "CourseCacheDurationMinutes": 30,
    "TempleC ageDurationMinutes": 60
  }
}
```

### IskconWeb.Admin/appsettings.json
```json
{
  "ApiConfig": { "BaseUrl": "https://localhost:7001" },
  "AdminConfig": {
    "DefaultTempleId": "11111111-1111-1111-1111-111111111111",
    "MaxUploadSizeMB": 5
  }
}
```

## Development Workflow

### Creating a New Event (Admin Perspective)
1. Admin logs in to CMS (localhost:7003)
2. Navigate to EventsAdmin → Create
3. Fill form (title, description, date, location, etc.)
4. Event saved as **Draft** in Master DB
5. Click "Publish" to move to Published status
6. ContentSyncService copies event to Web DB
7. Event appears on public website immediately (with cache TTL)

### Publishing Content
All publish operations:
1. Update PublishStatus to "Published" in Master DB
2. Set PublishedAt timestamp
3. ContentSyncService syncs to Web DB
4. Web site cache invalidated (event-based)
5. Changes visible on public site after cache TTL

## Future Enhancements

- [ ] Authentication middleware for API
- [ ] File upload service for media
- [ ] Full testing infrastructure (unit, integration, E2E)
- [ ] Docker containerization
- [ ] CI/CD pipeline
- [ ] Audit logging service
- [ ] Email notifications on publish
- [ ] Content versioning/rollback
- [ ] Admin activity logging

## Notes

- Logging configured via Serilog (console + file)
- Logs stored in `logs/` directory (not in git)
- All timestamps UTC for consistency
- GUIDs used for all primary keys
- Soft delete pattern optional (consider adding IsDeleted field to content)
