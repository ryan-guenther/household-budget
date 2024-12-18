using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdBudget.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAdminDefaultUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "1" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "9E695B49-3EEA-4BAC-9A0F-73E21977E65F", 0, "b1fd63fe-c93a-41b8-b96e-b6cf5d4fcc46", "admin@householdbudget.com", true, false, null, "ADMIN@HOUSEHOLDBUDGET.COM", "ADMIN@HOUSEHOLDBUDGET.COM", "AQAAAAIAAYagAAAAEH0MHgXb85VobOE8N045dkFU/SISHU9gRMbIoqXMXHQpcH/JNYmnrfl3BFSo16xxew==", null, false, "1f1f3b55-9e3a-4f51-8f42-d0bb0a622562", false, "admin@householdbudget.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "9E695B49-3EEA-4BAC-9A0F-73E21977E65F" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "9E695B49-3EEA-4BAC-9A0F-73E21977E65F" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "9E695B49-3EEA-4BAC-9A0F-73E21977E65F");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "1", 0, "b1fd63fe-c93a-41b8-b96e-b6cf5d4fcc46", "admin@example.com", true, false, null, "ADMIN@EXAMPLE.COM", "ADMIN@EXAMPLE.COM", "AQAAAAIAAYagAAAAEH0MHgXb85VobOE8N045dkFU/SISHU9gRMbIoqXMXHQpcH/JNYmnrfl3BFSo16xxew==", null, false, "1f1f3b55-9e3a-4f51-8f42-d0bb0a622562", false, "admin@example.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "1" });
        }
    }
}
