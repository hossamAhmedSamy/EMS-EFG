using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMS.Migrations
{
    /// <inheritdoc />
    public partial class MultipleWindowsAndReservationSector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Investors_InvestorId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Presenters_PresenterId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_RoomSlots_RoomSlotId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_PresenterInterests_PresenterId_SectorId",
                table: "PresenterInterests");

            migrationBuilder.DropIndex(
                name: "IX_InvestorInterests_InvestorId_SectorId",
                table: "InvestorInterests");

            migrationBuilder.AddColumn<int>(
                name: "SectorId",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_SectorId",
                table: "Reservations",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_PresenterInterests_PresenterId",
                table: "PresenterInterests",
                column: "PresenterId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestorInterests_InvestorId",
                table: "InvestorInterests",
                column: "InvestorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Investors_InvestorId",
                table: "Reservations",
                column: "InvestorId",
                principalTable: "Investors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Presenters_PresenterId",
                table: "Reservations",
                column: "PresenterId",
                principalTable: "Presenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_RoomSlots_RoomSlotId",
                table: "Reservations",
                column: "RoomSlotId",
                principalTable: "RoomSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Sectors_SectorId",
                table: "Reservations",
                column: "SectorId",
                principalTable: "Sectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Investors_InvestorId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Presenters_PresenterId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_RoomSlots_RoomSlotId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Sectors_SectorId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_SectorId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_PresenterInterests_PresenterId",
                table: "PresenterInterests");

            migrationBuilder.DropIndex(
                name: "IX_InvestorInterests_InvestorId",
                table: "InvestorInterests");

            migrationBuilder.DropColumn(
                name: "SectorId",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_PresenterInterests_PresenterId_SectorId",
                table: "PresenterInterests",
                columns: new[] { "PresenterId", "SectorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvestorInterests_InvestorId_SectorId",
                table: "InvestorInterests",
                columns: new[] { "InvestorId", "SectorId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Investors_InvestorId",
                table: "Reservations",
                column: "InvestorId",
                principalTable: "Investors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Presenters_PresenterId",
                table: "Reservations",
                column: "PresenterId",
                principalTable: "Presenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_RoomSlots_RoomSlotId",
                table: "Reservations",
                column: "RoomSlotId",
                principalTable: "RoomSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
