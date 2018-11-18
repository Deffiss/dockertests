using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DockerTest.UI.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Age = table.Column<int>(nullable: false),
                    Address = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Persons",
                columns: new[] { "Id", "Address", "Age", "Name" },
                values: new object[,]
                {
                    { 1, "Belarus, Gomel", 21, "Aliaksei Harshkalep1" },
                    { 2, "Belarus, Gomel", 22, "Aliaksei Harshkalep2" },
                    { 3, "Belarus, Gomel", 23, "Aliaksei Harshkalep3" },
                    { 4, "Belarus, Gomel", 24, "Aliaksei Harshkalep4" },
                    { 5, "Belarus, Gomel", 25, "Aliaksei Harshkalep5" },
                    { 6, "Belarus, Gomel", 26, "Aliaksei Harshkalep6" },
                    { 7, "Belarus, Gomel", 27, "Aliaksei Harshkalep7" },
                    { 8, "Belarus, Gomel", 28, "Aliaksei Harshkalep8" },
                    { 9, "Belarus, Gomel", 29, "Aliaksei Harshkalep9" },
                    { 10, "Belarus, Gomel", 30, "Aliaksei Harshkalep10" },
                    { 11, "Belarus, Gomel", 31, "Aliaksei Harshkalep11" },
                    { 12, "Belarus, Gomel", 32, "Aliaksei Harshkalep12" },
                    { 13, "Belarus, Gomel", 33, "Aliaksei Harshkalep13" },
                    { 14, "Belarus, Gomel", 34, "Aliaksei Harshkalep14" },
                    { 15, "Belarus, Gomel", 35, "Aliaksei Harshkalep15" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Persons");
        }
    }
}
