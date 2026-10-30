# EMS Technical Task — Handover

**Owner:** Hossam Eleissawy 

## 0. How to work with Hossam
- He must understand and explain every piece in the interview.
- Real technical decisions: ask him first, then recommend with the trade-off. Routine stuff: explain briefly.
- Small steps; stop after each piece so he can read it. Concise, chunked, analogies.
- Single project. No emails or irreversible changes without his explicit OK.

## 1. The task (from EFG Holding email, Oct 27, 2026)
Build an EMS (Event Management System) web site. Presenters meet investors to pitch their company.
A meeting happens only if presenter and investor are both free at that time slot, share the same
sector, and the room is free. Rooms are conference rooms across hotels. Every meeting is 1 hour.

Example: Presenter Oltob (Restaurants, free 8AM-4PM), Investor CIB (Finance, IT, Restaurants,
Real Estate, free 1PM-4PM) meet only if there is a free room from 1PM.

Deliverables:
1. Hotel page: hotel name, conference rooms, time slots per room
   (Hilton Room 1: 1-2, 2-3, 3-4 PM; Room 2: 1-2, 3-4 PM).
2. Investor page: name, mobile, several sectors each with its own time window.
3. Presenter page: same as investor.
4. Reservation page: pick investor -> sector -> time; match presenters; select ONLY available rooms
   for that time and hotel; after confirmation, room, investor and presenter are occupied (no double booking).
5. Report (SSRS): all reserved slots with investor, presenter, hotel, room.
6. SSIS package: load Hotel table from several CSV files (one line per hotel per file).

Deployment does not matter (confirmed with interviewer).

## 2. Decisions
| Decision | Choice |
|---|---|
| Stack | ASP.NET Core MVC (.NET 10), EF Core code-first, SQL Server LocalDB |
| Shape | One MVC project: Models/, Data/, Services/, Controllers/, Views/ |
| Times | `TimeOnly` / SQL `time`; display as "h:mm tt" |
| Meeting rule | **A+**: a meeting is one room slot fully inside both people's windows; slot generator accepts any start time (e.g. 1:30) and slots can be removed individually |
| Hotel columns | Name (unique) + optional Address -> CSV `Hilton,Nile Corniche Cairo` |
| Seed sectors | Finance, IT, Restaurants, Retail, Real Estate |
| Double booking | Service overlap check (real logic) + DB unique indexes (backstop) |
| Slots in a room | Never overlap each other (touching is fine: 1-2 and 2-3) |
| Time windows | Several windows per sector allowed (e.g. 2-3 and 4-6), but not overlapping within the same sector |
| Reservation | Stores SectorId too; FKs are Restrict, so anything with a booking can't be deleted |
| Edits/deletes | Blocked when an existing reservation depends on it (cancel the reservation first) |
| Reservation page | Plain GET steps (investor -> sector -> time) + POST to confirm; everything re-checked at confirm in a serializable transaction |
| SSIS duplicates | Skip by name: CSVs -> staging table -> one INSERT ... WHERE NOT EXISTS; re-running adds nothing |
| UI | Bootstrap 5 + EFG-style navy/gold theme (site.css); logo shows when `wwwroot/images/logo.svg` exists |

## 3. Data model
Sector, Hotel, Room, RoomSlot, Investor, Presenter, InvestorInterest, PresenterInterest, Reservation.
Reservation copies Start/EndTime from its slot so "is this person busy?" checks are simple.
Report view `vw_ReservationReport` (raw SQL migration) for SSRS.

## 4. Environment
- Windows work PC (not admin). .NET 10 SDK installed per-user in `%USERPROFILE%\dotnet`.
- VS Code for the web app; Visual Studio 2026 for SSIS + SSRS. LocalDB `(localdb)\MSSQLLocalDB`.

## 5. Build plan
1. ✅ Environment
2. ✅ Scaffold + entities + migration
3. ✅ Hotel page (rooms, slot generator, remove slots)
4. ✅ Investor + Presenter pages
5. ✅ MatchingService + tests (21 xUnit tests, `dotnet test EMS.Tests`)
6. ✅ Reservation page
7. ✅ SSIS package (`SSIS/LoadHotels.dtsx`, run with dtexec) + SSRS report (`SSRS/ReservationsReport.rdl`, schema-validated; preview in VS)
8. ✅ README; push to GitHub is Hossam's step
