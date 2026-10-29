using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hotels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hotels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Investors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Investors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Presenters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presenters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sectors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sectors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HotelId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestorInterests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvestorId = table.Column<int>(type: "int", nullable: false),
                    SectorId = table.Column<int>(type: "int", nullable: false),
                    FromTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    ToTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestorInterests", x => x.Id);
                    table.CheckConstraint("CK_InvestorInterest_Time", "[ToTime] > [FromTime]");
                    table.ForeignKey(
                        name: "FK_InvestorInterests_Investors_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "Investors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvestorInterests_Sectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PresenterInterests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PresenterId = table.Column<int>(type: "int", nullable: false),
                    SectorId = table.Column<int>(type: "int", nullable: false),
                    FromTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    ToTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresenterInterests", x => x.Id);
                    table.CheckConstraint("CK_PresenterInterest_Time", "[ToTime] > [FromTime]");
                    table.ForeignKey(
                        name: "FK_PresenterInterests_Presenters_PresenterId",
                        column: x => x.PresenterId,
                        principalTable: "Presenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PresenterInterests_Sectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoomSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomSlots", x => x.Id);
                    table.CheckConstraint("CK_RoomSlot_Time", "[EndTime] > [StartTime]");
                    table.ForeignKey(
                        name: "FK_RoomSlots_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvestorId = table.Column<int>(type: "int", nullable: false),
                    PresenterId = table.Column<int>(type: "int", nullable: false),
                    SectorId = table.Column<int>(type: "int", nullable: false),
                    RoomSlotId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Investors_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "Investors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Presenters_PresenterId",
                        column: x => x.PresenterId,
                        principalTable: "Presenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_RoomSlots_RoomSlotId",
                        column: x => x.RoomSlotId,
                        principalTable: "RoomSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Sectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Sectors",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Finance" },
                    { 2, "IT" },
                    { 3, "Restaurants" },
                    { 4, "Retail" },
                    { 5, "Real Estate" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_Name",
                table: "Hotels",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvestorInterests_InvestorId",
                table: "InvestorInterests",
                column: "InvestorId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestorInterests_SectorId",
                table: "InvestorInterests",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_PresenterInterests_PresenterId",
                table: "PresenterInterests",
                column: "PresenterId");

            migrationBuilder.CreateIndex(
                name: "IX_PresenterInterests_SectorId",
                table: "PresenterInterests",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_InvestorId_StartTime",
                table: "Reservations",
                columns: new[] { "InvestorId", "StartTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PresenterId_StartTime",
                table: "Reservations",
                columns: new[] { "PresenterId", "StartTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_RoomSlotId",
                table: "Reservations",
                column: "RoomSlotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_SectorId",
                table: "Reservations",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_HotelId_Name",
                table: "Rooms",
                columns: new[] { "HotelId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomSlots_RoomId_StartTime",
                table: "RoomSlots",
                columns: new[] { "RoomId", "StartTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sectors_Name",
                table: "Sectors",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestorInterests");

            migrationBuilder.DropTable(
                name: "PresenterInterests");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Investors");

            migrationBuilder.DropTable(
                name: "Presenters");

            migrationBuilder.DropTable(
                name: "RoomSlots");

            migrationBuilder.DropTable(
                name: "Sectors");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Hotels");
        }
    }
}
