using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixFeedbackRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_AspNetUsers_InstructorId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Classrooms_ClassroomId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_InstructorId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "InstructorId",
                table: "Feedbacks");

            migrationBuilder.RenameColumn(
                name: "ClassroomId",
                table: "Feedbacks",
                newName: "ReservationId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_ClassroomId",
                table: "Feedbacks",
                newName: "IX_Feedbacks_ReservationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Reservations_ReservationId",
                table: "Feedbacks",
                column: "ReservationId",
                principalTable: "Reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Reservations_ReservationId",
                table: "Feedbacks");

            migrationBuilder.RenameColumn(
                name: "ReservationId",
                table: "Feedbacks",
                newName: "ClassroomId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_ReservationId",
                table: "Feedbacks",
                newName: "IX_Feedbacks_ClassroomId");

            migrationBuilder.AddColumn<string>(
                name: "InstructorId",
                table: "Feedbacks",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_InstructorId",
                table: "Feedbacks",
                column: "InstructorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_AspNetUsers_InstructorId",
                table: "Feedbacks",
                column: "InstructorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Classrooms_ClassroomId",
                table: "Feedbacks",
                column: "ClassroomId",
                principalTable: "Classrooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
