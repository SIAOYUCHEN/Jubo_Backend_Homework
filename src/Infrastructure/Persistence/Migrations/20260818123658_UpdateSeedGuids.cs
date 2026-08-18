using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedGuids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"));

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("6891fed6-bfb8-478e-8993-abfd66ed859e"), "林大同" },
                    { new Guid("9a36db19-860d-40cf-8277-d4c53ca99bd0"), "陳美麗" },
                    { new Guid("aff37c0c-9146-4329-92c8-f63202ccc1de"), "張淑芬" },
                    { new Guid("b2cc2f38-22ec-4236-be7e-08445071a3d2"), "王小明" },
                    { new Guid("d2836813-d635-412e-ade8-ef3958e3cd39"), "李國強" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "PasswordHash", "Username" },
                values: new object[] { new Guid("1220680d-0d0b-423f-a4ff-4653a15b77f6"), "$2a$11$lKXu3UJut3gYYXQB3wuoWO4eZwxIHEfJahCOUXS2cIDd6eYk.xxVi", "demo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("6891fed6-bfb8-478e-8993-abfd66ed859e"));

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("9a36db19-860d-40cf-8277-d4c53ca99bd0"));

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("aff37c0c-9146-4329-92c8-f63202ccc1de"));

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("b2cc2f38-22ec-4236-be7e-08445071a3d2"));

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: new Guid("d2836813-d635-412e-ade8-ef3958e3cd39"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("1220680d-0d0b-423f-a4ff-4653a15b77f6"));

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "王小明" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "陳美麗" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "林大同" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "張淑芬" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "李國強" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "PasswordHash", "Username" },
                values: new object[] { new Guid("99999999-9999-9999-9999-999999999999"), "$2a$11$lKXu3UJut3gYYXQB3wuoWO4eZwxIHEfJahCOUXS2cIDd6eYk.xxVi", "demo" });
        }
    }
}
