using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMS.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationReportView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Flat, report-ready view of every reservation; the SSRS report reads from this.
            migrationBuilder.Sql(@"
CREATE VIEW dbo.vw_ReservationReport AS
SELECT
    r.Id        AS ReservationId,
    h.Name      AS HotelName,
    rm.Name     AS RoomName,
    r.StartTime AS StartTime,
    r.EndTime   AS EndTime,
    s.Name      AS SectorName,
    i.Name      AS InvestorName,
    i.Mobile    AS InvestorMobile,
    p.Name      AS PresenterName,
    p.Mobile    AS PresenterMobile
FROM dbo.Reservations r
JOIN dbo.RoomSlots  rs ON rs.Id = r.RoomSlotId
JOIN dbo.Rooms      rm ON rm.Id = rs.RoomId
JOIN dbo.Hotels     h  ON h.Id  = rm.HotelId
JOIN dbo.Sectors    s  ON s.Id  = r.SectorId
JOIN dbo.Investors  i  ON i.Id  = r.InvestorId
JOIN dbo.Presenters p  ON p.Id  = r.PresenterId;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW dbo.vw_ReservationReport;");
        }
    }
}
