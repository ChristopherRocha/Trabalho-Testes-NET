using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GREENHERB.Migrations
{
    /// <inheritdoc />
    public partial class AddCultivationPlanColumnsClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "CultivationPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CultivationPlans",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "HumidityMax",
                table: "CultivationPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HumidityMin",
                table: "CultivationPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LuminosityMax",
                table: "CultivationPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LuminosityMin",
                table: "CultivationPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TemperatureMax",
                table: "CultivationPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TemperatureMin",
                table: "CultivationPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CultivationPlans",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "CultivationPlans");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CultivationPlans");

            migrationBuilder.DropColumn(
                name: "HumidityMax",
                table: "CultivationPlans");

            migrationBuilder.DropColumn(
                name: "HumidityMin",
                table: "CultivationPlans");

            migrationBuilder.DropColumn(
                name: "LuminosityMax",
                table: "CultivationPlans");

            migrationBuilder.DropColumn(
                name: "LuminosityMin",
                table: "CultivationPlans");

            migrationBuilder.DropColumn(
                name: "TemperatureMax",
                table: "CultivationPlans");

            migrationBuilder.DropColumn(
                name: "TemperatureMin",
                table: "CultivationPlans");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CultivationPlans");
        }
    }
}
