# ISKCON Multi-Site Portal - Security Standards

**Last Updated:** March 19, 2026  
**Version:** 1.0  
**Technology Stack:** ASP.NET Core 8 LTS, Entity Framework Core, SQL Server, Razor MVC

---

## Executive Summary

This document defines the comprehensive security standards for the ISKCON multi-site temple portal. All implementation must adhere to **OWASP Top 10**, **GDPR compliance**, and **banking-grade security practices** from Day 1. Security is not an afterthought but a core architectural principle.

---

## 1. Authentication & Authorization

### 1.1 Password Security
- **Algorithm:** PBKDF2 (via ASP.NET Identity default)
  - Minimum 10,000 iterations (default: 10,000+)
  - Salt: Automatically generated per user (128-bit minimum)
  - Hash output: Minimum 256 bits (SHA256)

### 1.2 Session Management
- **Cookies (Primary):**
  - HttpOnly: Required (prevents JavaScript access)
  - Secure: Required (HTTPS only)
  - SameSite: Strict (CSRF protection)
  - MaxAge: 30 minutes for general users, 60 minutes for admins

### 1.3 Role-Based Access Control (RBAC)
**Roles:**
- **SuperAdmin:** All temples, all operations
- **TempleAdmin:** Assigned to specific temple, full CRUD for own temple
- **Moderator:** Read/publish events and courses
- **RegisteredUser:** View content, register for events/courses
- **Guest:** View published content only

### 1.4 Multi-Factor Authentication (MFA)
- **Phase 1 (MVP):** Single-factor (email/password)
- **Phase 2 (Planned):** Add TOTP (Time-based One-Time Password)

---

## 2. Data Protection

### 2.1 Transport Security (HTTPS/TLS)
- **Minimum TLS Version:** 1.2 (enforced in production)
- **HSTS (HTTP Strict-Transport-Security):**
  - Header: `Strict-Transport-Security: max-age=63072000; includeSubDomains; preload`
  - Max-age: 2 years (63072000 seconds)

### 2.2 Data Encryption at Rest
- **Password hashes:** Built-in ASP.NET Identity (PBKDF2)
- **Sensitive fields (email/phone):** Encrypt at field-level using EF Core Data Protection API
- **Key Management:**
  - Development: DPAPI with local machine key
  - Production: Azure Key Vault or AWS Secrets Manager

### 2.3 Data Retention & Deletion
- **Event/Course registrations:** Keep 1 year after event/course ends, then anonymize
- **User accounts:** Keep indefinitely, allow user deletion
- **Audit logs:** Keep 90 days
- **Session logs:** Keep 30 days
- **Backup/Snapshots:** Keep 30 days (5 daily snapshots)

---

## 3. Input Validation & Output Encoding

### 3.1 Input Validation
- **Client-Side:** HTML5 validation (user convenience)
- **Server-Side (Critical):** Whitelist approach, reject invalid input immediately

### 3.2 File Upload Security
- **Whitelist:** Only `.jpg`, `.png`, `.gif`, `.webp` for images
- **Check:** MIME type and file signature
- **Size:** Maximum 5 MB per image
- **Virus Scan:** Optional, use ClamAV or cloud API

### 3.3 Output Encoding (XSS Prevention)
- **Razor Templating:** Auto-encodes HTML by default
- **@Html.Raw():** Only for trusted content
- **Never:** Concatenate user input into JavaScript

---

## 4. API Security

### 4.1 CORS (Cross-Origin Resource Sharing)
- **Whitelist:** Specific origin domains only (never use `*`)
- **Credentials:** Cookies only sent to matched origins

### 4.2 CSRF (Cross-Site Request Forgery)
- **Token:** Generated per-user, per-request
- **Storage:** Cookie (server-side validation)
- **Validation:** `@Html.AntiForgeryToken()` on all forms

### 4.3 Rate Limiting
- **General endpoints:** 100 req/min per IP
- **Login endpoint:** 5 req/min per IP (brute force protection)
- **Password reset:** 3 req/hour per email
- **File upload:** 10 req/hour per user

---

## 5. Database Security

### 5.1 SQL Injection Prevention
- **Parameterized Queries:** Always use EF Core LINQ (automatically parameterized)
- **Never:** Concatenate SQL strings

### 5.2 Connection Security
- **Development:** `Encrypt=false;TrustServerCertificate=true;`
- **Production:** `Encrypt=true;TrustServerCertificate=false;`
- **Connection Pooling:** Min=10, Max=100

### 5.3 Least Privilege Principle
| User | Role | Purpose |
|------|------|---------|
| `iskcon_app` | db_datareader, db_datawriter | Application runtime |
| `iskcon_sync` | Read Master, Write Web | Content sync service |
| `iskcon_backup` | db_backupoperator | Automated backups |
| `sa` | sysadmin | Migration/schema changes only |

### 5.4 Row-Level Security (Multi-Tenancy)
- **Strategy:** Filter all queries by TempleId at application layer
- **Enforcement:** Application-level checks + database constraints

---

## 6. Infrastructure & Network Security

### 6.1 HTTPS Enforcement
- **Middleware:** `app.UseHttpsRedirection();`
- **HSTS:** `app.UseHsts();`

### 6.2 Security Headers
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});
```

---

## 7. Logging, Monitoring & Incident Response

### 7.1 Audit Logging
**Log all security-relevant events:**
- Login success/failure
- Brute force attempts (5+ failures)
- Role changes
- Content publication
- User deletion
- Unauthorized access attempts

### 7.2 Sensitive Data Masking
**Never log:**
- Passwords, password hashes
- Credit card numbers
- API keys, secrets
- Full email addresses in detailed logs

### 7.3 Monitoring & Alerting
- **5+ failed logins in 5 minutes:** Lock account, alert admin
- **100+ HTTP 4xx errors in 5 minutes:** Check for DDoS/scan activity
- **Database connection failures:** Page on-call DBA
- **Structured logging:** Serilog with seq server
- **Metrics:** Prometheus/Grafana
- **Tracing:** Application Insights or Jaeger

---

## 8. Secure Development Practices

### 8.1 Dependency Management
- **Source:** Officially published NuGet packages only
- **Versioning:** Pin to stable versions (avoid pre-release)
- **Scanning:** `dotnet list package --vulnerable`
- **Update Cadence:** Quarterly minimum

### 8.2 Code Review & Testing
- **Reviews:** Peer review for all changes (2+ approvals for security-critical)
- **Checklist:** SQL injection, XSS, auth/authz, secrets in code
- **Testing:** Unit tests (>80% coverage), integration tests, penetration testing

### 8.3 Secure Configuration Management
**Secrets (Never in code):**
- Database connection strings
- API keys
- JWT secrets
- Third-party credentials

**Storage:**
- Development: `appsettings.Development.json` (local, not committed) or user secrets
- Production: Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault

---

## 9. Compliance & Legal

### 9.1 GDPR Compliance
- **Right to access:** Users can request and download their data
- **Right to deletion:** Users can request account deletion (soft-delete with 30-day grace)
- **Right to portability:** Export data in machine-readable format
- **Consent management:** Users explicitly opt-in to cookie/tracking
- **Data breach notification:** Notify within 72 hours

### 9.2 Data Breach Response Plan
1. **Detect:** Monitoring alerts, customer reports, security audits
2. **Contain:** Isolate affected systems, revoke compromised credentials
3. **Assess:** Determine scope (who, what data, when, how)
4. **Notify:** GDPR requires notification within 72 hours
5. **Remediate:** Fix vulnerability, patch, update WAF rules
6. **Post-Mortem:** Root cause analysis, process improvements

---

## 10. Security Implementation Checklist

**Phase 1 - Database & Authentication:**
- [ ] Implement ASP.NET Identity with PBKDF2 hashing
- [ ] Configure HTTPS/TLS 1.2+ in development
- [ ] Setup database connection with encryption flag
- [ ] Implement CSRF token validation on forms
- [ ] Add security headers (HSTS, X-Frame-Options, CSP)
- [ ] Setup audit logging for auth events
- [ ] Configure role-based access control (RBAC)
- [ ] Implement temple-level row filtering

**Phase 2 - Input Validation & API Security:**
- [ ] Implement server-side input validation (whitelist)
- [ ] Add file upload validation (MIME type, size, extension)
- [ ] Enable CORS with specific origins
- [ ] Implement rate limiting on login (5 req/min)
- [ ] Setup API versioning
- [ ] Generate Swagger documentation (no secrets exposed)
- [ ] Test SQL injection and XSS vulnerabilities

**Phase 3 - Infrastructure & Monitoring:**
- [ ] Deploy to HTTPS-only environment
- [ ] Setup structured logging (Serilog)
- [ ] Configure monitoring/alerting for security events
- [ ] Implement WAF rules (DDoS, injection attacks)
- [ ] Setup automated backups with encryption
- [ ] Conduct penetration testing
- [ ] Document incident response plan
- [ ] Compliance audit (GDPR, OWASP)

---

**Document Status:** APPROVED FOR IMPLEMENTATION
**Next Review Date:** March 19, 2027