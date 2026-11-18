# MaatriCare - Maternal & Newborn Healthcare Follow-Up Platform
ASP.NET Core MVC (.NET 8) + EF Core + SQLite.

## Run
    dotnet run
Open the URL shown in the terminal. Login: admin / doctor / worker, password Pass@123.

## Folders
- Controllers/ - handle requests (Account, Home, Mothers, Newborns)
- Models/      - database tables (Mother, AncVisit, Newborn, Vaccination, AppUser)
- Services/    - business logic: RiskService (risk score), VaccineService (UIP schedule)
- Data/        - AppDbContext + DbSeeder (demo data)
- Views/       - Razor pages (UI)
- wwwroot/css  - custom styling

## Flow
Browser -> Controller -> Service/DbContext -> Model -> View.
Risk: each rule (age, BP, Hb, sugar, C-section, missed visits) adds points -> Low/Medium/High + reasons.
Vaccines: due date = date of birth + fixed days (0, 42, 70, 98, 270).
