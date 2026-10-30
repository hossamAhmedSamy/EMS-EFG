# EMS – Event Management System

An internal web app for scheduling 1-hour meetings in which company **presenters** pitch to **investors** in hotel conference rooms.

A meeting can only be booked when:
- the presenter and the investor share the same **sector**,
- both are free for the whole hour, and
- a **room slot** at that time is free.

Built with ASP.NET Core MVC (.NET 10), EF Core (code-first) and SQL Server. An **SSIS** package imports hotels from CSV files, and an **SSRS** report lists every reservation.

## What's included

| Task deliverable | Where |
|---|---|
| Hotel page: hotels, conference rooms, time slots per room | **Hotels** → Manage |
| Investor page: name, mobile, several sectors each with its own time window | **Investors** → Manage |
| Presenter page: same as investor | **Presenters** → Manage |
| Reservation page: investor → sector → time → matching presenter + available room; no double booking | **Reservations** → New reservation |
| SSRS report: all reserved slots with investor, presenter, hotel, room | [`SSRS/ReservationsReport.rdl`](SSRS/ReservationsReport.rdl) |
| SSIS package: load the Hotel table from several CSV files | [`SSIS/LoadHotels.dtsx`](SSIS/LoadHotels.dtsx) |

## Run it

Prerequisites:
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (installed with Visual Studio). Any SQL Server instance works if you change `ConnectionStrings:DefaultConnection` in `appsettings.json`.
- EF Core CLI: `dotnet tool install --global dotnet-ef`

```bash
dotnet ef database update   # creates the EMS database, seeds the 5 sectors, creates the report view
dotnet run                  # http://localhost:5024
```

A typical walkthrough:
1. Add a hotel, its rooms and their slots.
2. Add an investor and a presenter, each with a time window in the same sector.
3. Book a meeting under **Reservations → New reservation**.

### Tests

```bash
dotnet test EMS.Tests
```

21 xUnit tests cover the time rules, slot generation, matching and booking. Each test runs on a fresh in-memory SQLite database, so the database's unique indexes and foreign keys are enforced as on SQL Server.

## How matching works

The rule lives in [`Services/MatchingService.cs`](Services/MatchingService.cs):

> A meeting is **one free room slot** that fits **fully inside** one of the investor's windows **and** one of the presenter's windows, in the same sector, while **neither person already has a meeting overlapping it**.

Example from the task: presenter Oltob (Restaurants, free 8 AM–4 PM) and investor CIB (Finance, IT, Restaurants, Real Estate, free 1–4 PM) can only meet in a room slot between 1 PM and 4 PM.

How double booking is prevented:
- **Service check.** At confirmation, `BookAsync` re-checks every rule inside a serializable transaction. It refuses if the room slot is taken, or if the investor or presenter has a meeting overlapping that time (even in another hotel).
- **Database backstop.** A unique index on `Reservation.RoomSlotId` means a slot can never hold two reservations. Reservation foreign keys are `RESTRICT`, so a slot, person or sector with bookings can't be deleted.

## Data model

| Table | Purpose |
|---|---|
| `Sectors` | Finance, IT, Restaurants, Retail, Real Estate (seeded) |
| `Hotels` → `Rooms` → `RoomSlots` | A hotel has rooms; each room offers 1-hour slots (any start time, no overlaps within a room) |
| `Investors` / `Presenters` | Name + mobile |
| `InvestorInterests` / `PresenterInterests` | Sector + time window. A sector can have several windows, as long as they don't overlap |
| `Reservations` | Room slot + investor + presenter + sector. Start/end times are copied from the slot, which keeps "is this person busy?" checks simple |
| `vw_ReservationReport` | Flat view (hotel, room, time, sector, investor, presenter) used by the SSRS report |

## Design decisions

- **One MVC project.** Business rules live in `Services/` (`MatchingService`, `SlotService`, `TimeRange`); controllers stay thin.
- **Times** use `TimeOnly` (SQL `time`) and are shown as `h:mm tt`. Every meeting is exactly 1 hour.
- **Slots** are generated from a From/To range, cut into back-to-back hours. Any start time works (e.g. 1:30), slots can be removed one by one, and a reserved slot can't be removed.
- **Edits are protected.** A time window can't be edited or removed if an existing reservation would fall outside it; cancel the reservation first.
- **The reservation page** is a series of plain page loads (investor → sector → time), so it needs no front-end framework. Every rule is checked again when the user confirms, because the options they saw may be out of date by then.

## SSIS – load hotels from CSV files

Input: every `*.csv` in a folder, one line per hotel, in the form `Name,"Address"` (address optional, no header row, CRLF line endings). Samples are in [`SSIS/HotelCsv/`](SSIS/HotelCsv).

The package has three steps:
1. **Prepare staging table** (Execute SQL): creates `dbo.HotelImportStaging` if needed and empties it.
2. **Foreach CSV file** (Foreach Loop, file enumerator): for each file, the data flow **Load CSV into staging** runs Flat File Source → OLE DB Destination.
3. **Insert new hotels** (Execute SQL): trims names, removes duplicates (also across files), **skips names that already exist** in `Hotels`, and inserts the rest.

Running the package again changes nothing. With the samples it inserts 7 hotels: Kempinski appears in two files but is inserted once.

Run it from the command line, pointing it at your CSV folder (the default is this repo's `SSIS\HotelCsv` on the author's machine):

```cmd
dtexec /F SSIS\LoadHotels.dtsx /SET "\Package.Variables[User::SourceFolder].Properties[Value]";"C:\path\to\HotelCsv"
```

To run it in Visual Studio (with the SQL Server Integration Services Projects extension):
1. Create a new **Integration Services Project**.
2. Right-click **SSIS Packages → Add Existing Package** and choose `SSIS/LoadHotels.dtsx`.
3. Set the `SourceFolder` variable, then press **Start**.

The package connects through the **EMS** connection manager (`(localdb)\MSSQLLocalDB`, database `EMS`, Microsoft OLE DB Driver for SQL Server).

## SSRS – reservations report

[`SSRS/ReservationsReport.rdl`](SSRS/ReservationsReport.rdl) is a landscape table of all reservations: time, hotel, room, sector, investor and presenter, with their mobiles. Its single dataset reads from `dbo.vw_ReservationReport`.

To preview it in Visual Studio (with the Microsoft Reporting Services Projects extension):
1. Create a **Report Server Project**.
2. Right-click **Reports → Add → Existing Item** and choose the `.rdl` file.
3. Open the **Preview** tab.

The embedded data source points to `(localdb)\MSSQLLocalDB`, database `EMS`. The task confirmed that deployment to a report server is not required.

## Project layout

```
Controllers/    Hotel, Investor, Presenter, Reservation (+ Home)
Data/           AppDbContext: indexes, delete rules, sector seed data
Migrations/     EF Core migrations (incl. the report view)
Models/         Entities
Services/       MatchingService, SlotService, TimeRange
ViewModels/     Page-specific view models
Views/          Razor views (Bootstrap 5 + wwwroot/css/site.css theme)
EMS.Tests/      xUnit tests
SSIS/           LoadHotels.dtsx + sample CSV files
SSRS/           ReservationsReport.rdl
```
