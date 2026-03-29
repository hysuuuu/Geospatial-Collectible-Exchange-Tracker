using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRequestedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestedCollectibleId",
                table: "ExchangeRequests");

            migrationBuilder.DropColumn(
                name: "RequestedQuantity",
                table: "ExchangeRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequestedCollectibleId",
                table: "ExchangeRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequestedQuantity",
                table: "ExchangeRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
