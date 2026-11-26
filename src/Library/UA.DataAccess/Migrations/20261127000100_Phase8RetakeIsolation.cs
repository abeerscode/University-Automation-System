using Microsoft.EntityFrameworkCore.Migrations;
namespace UA.DataAccess.Migrations;
public partial class Phase8RetakeIsolation : Migration
{
 protected override void Up(MigrationBuilder m)
 {
  m.DropIndex("UQ_Offering_term_id_section_id_course_id","Offering");
  m.AddColumn<bool>("is_special_retake","Offering",type:"bit",nullable:false,defaultValue:false);
  m.CreateIndex("UQ_Offering_term_id_section_id_course_id","Offering",new[]{"term_id","section_id","course_id","is_special_retake"},unique:true);
  m.DropIndex("UQ_Semester_Drop_student_id","Semester_Drop");
  m.CreateIndex("UQ_Semester_Drop_student_id","Semester_Drop","student_id",unique:true,filter:"[status] <> N'Rejected'");
 }
 protected override void Down(MigrationBuilder m)
 {
  // Never silently discard later attempts or rejected requests to force a downgrade.
  m.DropIndex("UQ_Semester_Drop_student_id","Semester_Drop");
  m.CreateIndex("UQ_Semester_Drop_student_id","Semester_Drop","student_id",unique:true);
  m.DropIndex("UQ_Offering_term_id_section_id_course_id","Offering");
  m.CreateIndex("UQ_Offering_term_id_section_id_course_id","Offering",new[]{"term_id","section_id","course_id"},unique:true);
  m.DropColumn("is_special_retake","Offering");
 }
}
