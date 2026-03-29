using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GeoTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddExchangeRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExchangeRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InitiatorUserId = table.Column<int>(type: "integer", nullable: false),
                    ReceiverUserId = table.Column<int>(type: "integer", nullable: false),
                    OfferedCollectibleId = table.Column<int>(type: "integer", nullable: false),
                    OfferedQuantity = table.Column<int>(type: "integer", nullable: false),
                    RequestedCollectibleId = table.Column<int>(type: "integer", nullable: false),
                    RequestedQuantity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRequests", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeRequests");
        }
    }
}
