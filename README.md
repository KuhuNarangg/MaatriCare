# MaatriCare — Maternal & Newborn Healthcare Follow-Up Platform

ASP.NET Core MVC (.NET 8) + EF Core + SQLite + Razor Views + Custom CSS.

## Quick Start
```bash
dotnet restore
dotnet build
dotnet run
```
Open `http://localhost:5000` (public website) and click **Staff Login** (`/Account/Login`).

## Demo Accounts (Password for all: `Pass@123`)

| Role | Username | Landing Page After Login |
| :--- | :--- | :--- |
| **Admin** | `admin` | `/Home` (Clinic Analytics Dashboard) |
| **Doctor** | `doctor` | `/Doctor` (Doctor Portal — Live Queue) |
| **Nurse** | `nurse` | `/Nurse` (Nurse Portal — Live Queue) |
| **Receptionist** | `receptionist` | `/Reception` (Reception Desk & Day Calendar) |
| **Mother** | `mother` | `/Home` (Personal Pregnancy & Baby Dashboard) |

## Roles & Permissions Matrix (Centralised in `Services/PermissionService.cs`)

| Capability | Admin | Doctor | Nurse | Receptionist | Mother |
| :--- | :---: | :---: | :---: | :---: | :---: |
| View all patient records & Visit Records | Yes | Yes | Yes | Read-Only | Own Only |
| Register new patient (Mother) | Yes | Yes | Yes | Yes | No |
| Edit patient profile & clinical data (vitals, ANC, delivery, growth) | Yes | Yes | Yes | No | No |
| Record / mark vaccines given & medications administered | Yes | Yes | Yes | No | No |
| Write Nurse notes (edit own only) | Yes | No | Yes (own) | No | No |
| Write Doctor notes, diagnosis & full Prescription | Yes | Yes | No | No | No |
| Create Hospital Referral from Consultation page | Yes | Yes | No | No | No |
| Book / Reschedule / Cancel / Check-In Appointments (Reception Desk) | Yes | No | No | Yes | No |
| Follow-Ups & Referrals pages | Yes | No | No | Yes | No |
| Admin Live Appointment Portal | Full | No | No | Read-Only | No |
| Delete records, Users & Roles, Health Tips CRUD, Audit Log | Yes | No | No | No | No |

## Folder Structure
- `Controllers/` — MVC controllers (`Public`, `Account`, `Home`, `Reception`, `Doctor`, `Nurse`, `Consultation`, `Records`, `LivePortal`, `Mothers`, `Newborns`, `FollowUps`, `Referrals`, `Notifications`, `Users`, `Tips`, `Audit`, `Profile`, `Error`)
- `Models/` — EF Core entities (`AppUser`, `Mother`, `AncVisit`, `Appointment`, `Delivery`, `Newborn`, `Vaccination`, `GrowthRecord`, `VisitRecord`, `PrescriptionItem`, `FollowUpContact`, `Notification`, `Referral`, `HealthTip`, `ContactMessage`, `AuditLog`)
- `ViewModels/` — Form and page view models with validation
- `Services/` — `PermissionService`, `AuthService`, `MotherService`, `AppointmentService`, `ConsultationService`, `NewbornService`, `RiskService`, `VaccineService`, `ReminderService`, `ReferralService`, `HealthTipService`, `DashboardService`, `UserService`, `AuditService`, `ContactService`
- `Data/` — `AppDbContext` + `DbSeeder` (15 mothers across 4 Punjab villages, 5 demo roles, appointments, visit records, vaccines)
- `Views/` — Public & Portal Razor views
- `wwwroot/` — Custom CSS (`public.css`, `site.css`), vanilla JS (`public.js`, `app.js`), images
