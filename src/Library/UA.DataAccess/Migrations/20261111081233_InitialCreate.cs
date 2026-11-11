using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UA.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Academic_Term",
                columns: table => new
                {
                    term_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    year = table.Column<short>(type: "smallint", nullable: false),
                    term_no = table.Column<byte>(type: "tinyint", nullable: false),
                    start_date = table.Column<DateTime>(type: "date", nullable: false),
                    end_date = table.Column<DateTime>(type: "date", nullable: false),
                    registration_deadline = table.Column<DateTime>(type: "date", nullable: false),
                    drop_deadline = table.Column<DateTime>(type: "date", nullable: false),
                    retake_deadline = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Academic_Term", x => x.term_id);
                });

            migrationBuilder.CreateTable(
                name: "Admission_Circular",
                columns: table => new
                {
                    circular_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    intake_year = table.Column<short>(type: "smallint", nullable: false),
                    application_open = table.Column<DateTime>(type: "date", nullable: false),
                    application_close = table.Column<DateTime>(type: "date", nullable: false),
                    test_date = table.Column<DateTime>(type: "date", nullable: false),
                    test_venue = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    result_date = table.Column<DateTime>(type: "date", nullable: false),
                    enrollment_deadline = table.Column<DateTime>(type: "date", nullable: false),
                    status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admission_Circular", x => x.circular_id);
                });

            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    department_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    id_code = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.department_id);
                });

            migrationBuilder.CreateTable(
                name: "Notification_Template",
                columns: table => new
                {
                    template_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    channel = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    subject = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification_Template", x => x.template_id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    department_scoped = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    room_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    capacity = table.Column<short>(type: "smallint", nullable: false),
                    room_type = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Room", x => x.room_id);
                });

            migrationBuilder.CreateTable(
                name: "User_Account",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    is_verified = table.Column<bool>(type: "bit", nullable: false),
                    status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    mfa_enabled = table.Column<bool>(type: "bit", nullable: false),
                    failed_login_count = table.Column<short>(type: "smallint", nullable: false),
                    locked_until = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Account", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "Academic_Calendar_Event",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    term_id = table.Column<int>(type: "int", nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    start_date = table.Column<DateTime>(type: "date", nullable: false),
                    end_date = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Academic_Calendar_Event", x => x.event_id);
                    table.ForeignKey(
                        name: "FK_Academic_Calendar_Event_Academic_Term_term_id",
                        column: x => x.term_id,
                        principalTable: "Academic_Term",
                        principalColumn: "term_id");
                });

            migrationBuilder.CreateTable(
                name: "Exam_Event",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    term_id = table.Column<int>(type: "int", nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    start_date = table.Column<DateTime>(type: "date", nullable: false),
                    end_date = table.Column<DateTime>(type: "date", nullable: false),
                    status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exam_Event", x => x.event_id);
                    table.ForeignKey(
                        name: "FK_Exam_Event_Academic_Term_term_id",
                        column: x => x.term_id,
                        principalTable: "Academic_Term",
                        principalColumn: "term_id");
                });

            migrationBuilder.CreateTable(
                name: "Attendance_Scale",
                columns: table => new
                {
                    scale_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    version_no = table.Column<int>(type: "int", nullable: false),
                    min_percent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    mark_percent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    effective_from = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendance_Scale", x => x.scale_id);
                    table.ForeignKey(
                        name: "FK_Attendance_Scale_Department_department_id",
                        column: x => x.department_id,
                        principalTable: "Department",
                        principalColumn: "department_id");
                });

            migrationBuilder.CreateTable(
                name: "Circular_Seat",
                columns: table => new
                {
                    circular_id = table.Column<int>(type: "int", nullable: false),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    seats = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Circular_Seat", x => new { x.circular_id, x.department_id });
                    table.ForeignKey(
                        name: "FK_Circular_Seat_Admission_Circular_circular_id",
                        column: x => x.circular_id,
                        principalTable: "Admission_Circular",
                        principalColumn: "circular_id");
                    table.ForeignKey(
                        name: "FK_Circular_Seat_Department_department_id",
                        column: x => x.department_id,
                        principalTable: "Department",
                        principalColumn: "department_id");
                });

            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    course_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    code = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    credits = table.Column<decimal>(type: "decimal(3,1)", nullable: false),
                    course_type = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.course_id);
                    table.ForeignKey(
                        name: "FK_Course_Department_department_id",
                        column: x => x.department_id,
                        principalTable: "Department",
                        principalColumn: "department_id");
                });

            migrationBuilder.CreateTable(
                name: "Fee_Type",
                columns: table => new
                {
                    fee_type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    unit = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    effective_from = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fee_Type", x => x.fee_type_id);
                    table.ForeignKey(
                        name: "FK_Fee_Type_Department_department_id",
                        column: x => x.department_id,
                        principalTable: "Department",
                        principalColumn: "department_id");
                });

            migrationBuilder.CreateTable(
                name: "Grade_Scale",
                columns: table => new
                {
                    scale_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    version_no = table.Column<int>(type: "int", nullable: false),
                    letter_grade = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    min_mark = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    max_mark = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    grade_point = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    effective_from = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grade_Scale", x => x.scale_id);
                    table.ForeignKey(
                        name: "FK_Grade_Scale_Department_department_id",
                        column: x => x.department_id,
                        principalTable: "Department",
                        principalColumn: "department_id");
                });

            migrationBuilder.CreateTable(
                name: "Mark_Component",
                columns: table => new
                {
                    component_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    course_type = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    max_marks = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    is_attendance_based = table.Column<bool>(type: "bit", nullable: false),
                    version_no = table.Column<int>(type: "int", nullable: false),
                    effective_from = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mark_Component", x => x.component_id);
                    table.ForeignKey(
                        name: "FK_Mark_Component_Department_department_id",
                        column: x => x.department_id,
                        principalTable: "Department",
                        principalColumn: "department_id");
                });

            migrationBuilder.CreateTable(
                name: "Program",
                columns: table => new
                {
                    program_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    total_credits = table.Column<short>(type: "smallint", nullable: false),
                    semester_count = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Program", x => x.program_id);
                    table.ForeignKey(
                        name: "FK_Program_Department_department_id",
                        column: x => x.department_id,
                        principalTable: "Department",
                        principalColumn: "department_id");
                });

            migrationBuilder.CreateTable(
                name: "Web_Page",
                columns: table => new
                {
                    page_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    department_id = table.Column<int>(type: "int", nullable: true),
                    slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Web_Page", x => x.page_id);
                    table.ForeignKey(
                        name: "FK_Web_Page_Department_department_id",
                        column: x => x.department_id,
                        principalTable: "Department",
                        principalColumn: "department_id");
                });

            migrationBuilder.CreateTable(
                name: "Role_Permission",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false),
                    permission_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role_Permission", x => new { x.role_id, x.permission_code });
                    table.ForeignKey(
                        name: "FK_Role_Permission_Role_role_id",
                        column: x => x.role_id,
                        principalTable: "Role",
                        principalColumn: "role_id");
                });

            migrationBuilder.CreateTable(
                name: "Applicant",
                columns: table => new
                {
                    applicant_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "date", nullable: false),
                    gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    guardian_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    guardian_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ssc_board = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ssc_roll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ssc_result = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    hsc_board = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    hsc_roll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    hsc_result = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    photo_path = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applicant", x => x.applicant_id);
                    table.ForeignKey(
                        name: "FK_Applicant_User_Account_user_id",
                        column: x => x.user_id,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Approval_Record",
                columns: table => new
                {
                    approval_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    reference_no = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    approval_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    approver_id = table.Column<int>(type: "int", nullable: false),
                    approval_date = table.Column<DateTime>(type: "date", nullable: false),
                    supersedes_id = table.Column<int>(type: "int", nullable: true),
                    scan_path = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Approval_Record", x => x.approval_id);
                    table.ForeignKey(
                        name: "FK_Approval_Record_Approval_Record_supersedes_id",
                        column: x => x.supersedes_id,
                        principalTable: "Approval_Record",
                        principalColumn: "approval_id");
                    table.ForeignKey(
                        name: "FK_Approval_Record_User_Account_approver_id",
                        column: x => x.approver_id,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Audit_Entry",
                columns: table => new
                {
                    audit_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    entity_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    old_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    logged_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_Entry", x => x.audit_id);
                    table.ForeignKey(
                        name: "FK_Audit_Entry_User_Account_user_id",
                        column: x => x.user_id,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Background_Job",
                columns: table => new
                {
                    job_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    job_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    requested_by = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    progress_percent = table.Column<byte>(type: "tinyint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    finished_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    error_report_path = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Background_Job", x => x.job_id);
                    table.ForeignKey(
                        name: "FK_Background_Job_User_Account_requested_by",
                        column: x => x.requested_by,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Config_Parameter",
                columns: table => new
                {
                    parameter_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    department_id = table.Column<int>(type: "int", nullable: true),
                    param_key = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    param_value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    version_no = table.Column<int>(type: "int", nullable: false),
                    effective_term_id = table.Column<int>(type: "int", nullable: false),
                    changed_by = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_Parameter", x => x.parameter_id);
                    table.ForeignKey(
                        name: "FK_Config_Parameter_Academic_Term_effective_term_id",
                        column: x => x.effective_term_id,
                        principalTable: "Academic_Term",
                        principalColumn: "term_id");
                    table.ForeignKey(
                        name: "FK_Config_Parameter_Department_department_id",
                        column: x => x.department_id,
                        principalTable: "Department",
                        principalColumn: "department_id");
                    table.ForeignKey(
                        name: "FK_Config_Parameter_User_Account_changed_by",
                        column: x => x.changed_by,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Faculty",
                columns: table => new
                {
                    faculty_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    employee_ref = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    designation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    faculty_type = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    qualifications = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    photo_path = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faculty", x => x.faculty_id);
                    table.ForeignKey(
                        name: "FK_Faculty_Department_department_id",
                        column: x => x.department_id,
                        principalTable: "Department",
                        principalColumn: "department_id");
                    table.ForeignKey(
                        name: "FK_Faculty_User_Account_user_id",
                        column: x => x.user_id,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Notice",
                columns: table => new
                {
                    notice_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    department_id = table.Column<int>(type: "int", nullable: true),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_public = table.Column<bool>(type: "bit", nullable: false),
                    published_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    author_id = table.Column<int>(type: "int", nullable: false),
                    approved_by = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notice", x => x.notice_id);
                    table.ForeignKey(
                        name: "FK_Notice_Department_department_id",
                        column: x => x.department_id,
                        principalTable: "Department",
                        principalColumn: "department_id");
                    table.ForeignKey(
                        name: "FK_Notice_User_Account_approved_by",
                        column: x => x.approved_by,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_Notice_User_Account_author_id",
                        column: x => x.author_id,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    notification_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    template_id = table.Column<int>(type: "int", nullable: false),
                    recipient_user_id = table.Column<int>(type: "int", nullable: false),
                    event_key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    attempts = table.Column<byte>(type: "tinyint", nullable: false),
                    queued_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    sent_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_Notification_Notification_Template_template_id",
                        column: x => x.template_id,
                        principalTable: "Notification_Template",
                        principalColumn: "template_id");
                    table.ForeignKey(
                        name: "FK_Notification_User_Account_recipient_user_id",
                        column: x => x.recipient_user_id,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "User_Role",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    department_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Role", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_User_Role_Department_department_id",
                        column: x => x.department_id,
                        principalTable: "Department",
                        principalColumn: "department_id");
                    table.ForeignKey(
                        name: "FK_User_Role_Role_role_id",
                        column: x => x.role_id,
                        principalTable: "Role",
                        principalColumn: "role_id");
                    table.ForeignKey(
                        name: "FK_User_Role_User_Account_user_id",
                        column: x => x.user_id,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Verification_Token",
                columns: table => new
                {
                    token_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    purpose = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    code_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    attempts = table.Column<byte>(type: "tinyint", nullable: false),
                    used_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Verification_Token", x => x.token_id);
                    table.ForeignKey(
                        name: "FK_Verification_Token_User_Account_user_id",
                        column: x => x.user_id,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Course_Prerequisite",
                columns: table => new
                {
                    course_id = table.Column<int>(type: "int", nullable: false),
                    prerequisite_course_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course_Prerequisite", x => new { x.course_id, x.prerequisite_course_id });
                    table.ForeignKey(
                        name: "FK_Course_Prerequisite_Course_course_id",
                        column: x => x.course_id,
                        principalTable: "Course",
                        principalColumn: "course_id");
                    table.ForeignKey(
                        name: "FK_Course_Prerequisite_Course_prerequisite_course_id",
                        column: x => x.prerequisite_course_id,
                        principalTable: "Course",
                        principalColumn: "course_id");
                });

            migrationBuilder.CreateTable(
                name: "Exam_Sitting",
                columns: table => new
                {
                    sitting_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    event_id = table.Column<int>(type: "int", nullable: false),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    sitting_type = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    exam_date = table.Column<DateTime>(type: "date", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exam_Sitting", x => x.sitting_id);
                    table.ForeignKey(
                        name: "FK_Exam_Sitting_Course_course_id",
                        column: x => x.course_id,
                        principalTable: "Course",
                        principalColumn: "course_id");
                    table.ForeignKey(
                        name: "FK_Exam_Sitting_Exam_Event_event_id",
                        column: x => x.event_id,
                        principalTable: "Exam_Event",
                        principalColumn: "event_id");
                });

            migrationBuilder.CreateTable(
                name: "Web_Page_Version",
                columns: table => new
                {
                    version_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    page_id = table.Column<int>(type: "int", nullable: false),
                    version_no = table.Column<int>(type: "int", nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    author_id = table.Column<int>(type: "int", nullable: false),
                    approved_by = table.Column<int>(type: "int", nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Web_Page_Version", x => x.version_id);
                    table.ForeignKey(
                        name: "FK_Web_Page_Version_User_Account_approved_by",
                        column: x => x.approved_by,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_Web_Page_Version_User_Account_author_id",
                        column: x => x.author_id,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_Web_Page_Version_Web_Page_page_id",
                        column: x => x.page_id,
                        principalTable: "Web_Page",
                        principalColumn: "page_id");
                });

            migrationBuilder.CreateTable(
                name: "Application",
                columns: table => new
                {
                    application_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    applicant_id = table.Column<int>(type: "int", nullable: false),
                    circular_id = table.Column<int>(type: "int", nullable: false),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    application_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    test_marks = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    merit_rank = table.Column<int>(type: "int", nullable: true),
                    selection_status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    documents_verified = table.Column<bool>(type: "bit", nullable: false),
                    approval_id = table.Column<int>(type: "int", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Application", x => x.application_id);
                    table.ForeignKey(
                        name: "FK_Application_Admission_Circular_circular_id",
                        column: x => x.circular_id,
                        principalTable: "Admission_Circular",
                        principalColumn: "circular_id");
                    table.ForeignKey(
                        name: "FK_Application_Applicant_applicant_id",
                        column: x => x.applicant_id,
                        principalTable: "Applicant",
                        principalColumn: "applicant_id");
                    table.ForeignKey(
                        name: "FK_Application_Approval_Record_approval_id",
                        column: x => x.approval_id,
                        principalTable: "Approval_Record",
                        principalColumn: "approval_id");
                    table.ForeignKey(
                        name: "FK_Application_Department_department_id",
                        column: x => x.department_id,
                        principalTable: "Department",
                        principalColumn: "department_id");
                });

            migrationBuilder.CreateTable(
                name: "Curriculum_Version",
                columns: table => new
                {
                    curriculum_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    program_id = table.Column<int>(type: "int", nullable: false),
                    version_no = table.Column<int>(type: "int", nullable: false),
                    effective_from_year = table.Column<short>(type: "smallint", nullable: false),
                    status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    approval_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Curriculum_Version", x => x.curriculum_id);
                    table.ForeignKey(
                        name: "FK_Curriculum_Version_Approval_Record_approval_id",
                        column: x => x.approval_id,
                        principalTable: "Approval_Record",
                        principalColumn: "approval_id");
                    table.ForeignKey(
                        name: "FK_Curriculum_Version_Program_program_id",
                        column: x => x.program_id,
                        principalTable: "Program",
                        principalColumn: "program_id");
                });

            migrationBuilder.CreateTable(
                name: "Batch",
                columns: table => new
                {
                    batch_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    curriculum_id = table.Column<int>(type: "int", nullable: false),
                    admission_year = table.Column<short>(type: "smallint", nullable: false),
                    seat_limit = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batch", x => x.batch_id);
                    table.ForeignKey(
                        name: "FK_Batch_Curriculum_Version_curriculum_id",
                        column: x => x.curriculum_id,
                        principalTable: "Curriculum_Version",
                        principalColumn: "curriculum_id");
                    table.ForeignKey(
                        name: "FK_Batch_Department_department_id",
                        column: x => x.department_id,
                        principalTable: "Department",
                        principalColumn: "department_id");
                });

            migrationBuilder.CreateTable(
                name: "Curriculum_Course",
                columns: table => new
                {
                    curriculum_id = table.Column<int>(type: "int", nullable: false),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    semester_no = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Curriculum_Course", x => new { x.curriculum_id, x.course_id });
                    table.ForeignKey(
                        name: "FK_Curriculum_Course_Course_course_id",
                        column: x => x.course_id,
                        principalTable: "Course",
                        principalColumn: "course_id");
                    table.ForeignKey(
                        name: "FK_Curriculum_Course_Curriculum_Version_curriculum_id",
                        column: x => x.curriculum_id,
                        principalTable: "Curriculum_Version",
                        principalColumn: "curriculum_id");
                });

            migrationBuilder.CreateTable(
                name: "Section",
                columns: table => new
                {
                    section_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    batch_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    capacity = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Section", x => x.section_id);
                    table.ForeignKey(
                        name: "FK_Section_Batch_batch_id",
                        column: x => x.batch_id,
                        principalTable: "Batch",
                        principalColumn: "batch_id");
                });

            migrationBuilder.CreateTable(
                name: "Mentor_Assignment",
                columns: table => new
                {
                    assignment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    faculty_id = table.Column<int>(type: "int", nullable: false),
                    batch_id = table.Column<int>(type: "int", nullable: false),
                    section_id = table.Column<int>(type: "int", nullable: true),
                    assigned_on = table.Column<DateTime>(type: "date", nullable: false),
                    ended_on = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mentor_Assignment", x => x.assignment_id);
                    table.ForeignKey(
                        name: "FK_Mentor_Assignment_Batch_batch_id",
                        column: x => x.batch_id,
                        principalTable: "Batch",
                        principalColumn: "batch_id");
                    table.ForeignKey(
                        name: "FK_Mentor_Assignment_Faculty_faculty_id",
                        column: x => x.faculty_id,
                        principalTable: "Faculty",
                        principalColumn: "faculty_id");
                    table.ForeignKey(
                        name: "FK_Mentor_Assignment_Section_section_id",
                        column: x => x.section_id,
                        principalTable: "Section",
                        principalColumn: "section_id");
                });

            migrationBuilder.CreateTable(
                name: "Offering",
                columns: table => new
                {
                    offering_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    term_id = table.Column<int>(type: "int", nullable: false),
                    batch_id = table.Column<int>(type: "int", nullable: false),
                    section_id = table.Column<int>(type: "int", nullable: false),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    semester_no = table.Column<byte>(type: "tinyint", nullable: false),
                    status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offering", x => x.offering_id);
                    table.ForeignKey(
                        name: "FK_Offering_Academic_Term_term_id",
                        column: x => x.term_id,
                        principalTable: "Academic_Term",
                        principalColumn: "term_id");
                    table.ForeignKey(
                        name: "FK_Offering_Batch_batch_id",
                        column: x => x.batch_id,
                        principalTable: "Batch",
                        principalColumn: "batch_id");
                    table.ForeignKey(
                        name: "FK_Offering_Course_course_id",
                        column: x => x.course_id,
                        principalTable: "Course",
                        principalColumn: "course_id");
                    table.ForeignKey(
                        name: "FK_Offering_Section_section_id",
                        column: x => x.section_id,
                        principalTable: "Section",
                        principalColumn: "section_id");
                });

            migrationBuilder.CreateTable(
                name: "Student",
                columns: table => new
                {
                    student_id = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    application_id = table.Column<int>(type: "int", nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    admission_batch_id = table.Column<int>(type: "int", nullable: false),
                    current_batch_id = table.Column<int>(type: "int", nullable: false),
                    section_id = table.Column<int>(type: "int", nullable: false),
                    roll_no = table.Column<short>(type: "smallint", nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "date", nullable: false),
                    gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    guardian_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    guardian_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    photo_path = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student", x => x.student_id);
                    table.ForeignKey(
                        name: "FK_Student_Application_application_id",
                        column: x => x.application_id,
                        principalTable: "Application",
                        principalColumn: "application_id");
                    table.ForeignKey(
                        name: "FK_Student_Batch_admission_batch_id",
                        column: x => x.admission_batch_id,
                        principalTable: "Batch",
                        principalColumn: "batch_id");
                    table.ForeignKey(
                        name: "FK_Student_Batch_current_batch_id",
                        column: x => x.current_batch_id,
                        principalTable: "Batch",
                        principalColumn: "batch_id");
                    table.ForeignKey(
                        name: "FK_Student_Department_department_id",
                        column: x => x.department_id,
                        principalTable: "Department",
                        principalColumn: "department_id");
                    table.ForeignKey(
                        name: "FK_Student_Section_section_id",
                        column: x => x.section_id,
                        principalTable: "Section",
                        principalColumn: "section_id");
                    table.ForeignKey(
                        name: "FK_Student_User_Account_user_id",
                        column: x => x.user_id,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Class_Routine",
                columns: table => new
                {
                    routine_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    offering_id = table.Column<int>(type: "int", nullable: false),
                    room_id = table.Column<int>(type: "int", nullable: false),
                    day_of_week = table.Column<byte>(type: "tinyint", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Class_Routine", x => x.routine_id);
                    table.ForeignKey(
                        name: "FK_Class_Routine_Offering_offering_id",
                        column: x => x.offering_id,
                        principalTable: "Offering",
                        principalColumn: "offering_id");
                    table.ForeignKey(
                        name: "FK_Class_Routine_Room_room_id",
                        column: x => x.room_id,
                        principalTable: "Room",
                        principalColumn: "room_id");
                });

            migrationBuilder.CreateTable(
                name: "Mark_Submission",
                columns: table => new
                {
                    submission_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    offering_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    submitted_by = table.Column<int>(type: "int", nullable: false),
                    verified_by = table.Column<int>(type: "int", nullable: true),
                    approved_by = table.Column<int>(type: "int", nullable: true),
                    return_comment = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    verified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approved_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mark_Submission", x => x.submission_id);
                    table.ForeignKey(
                        name: "FK_Mark_Submission_Offering_offering_id",
                        column: x => x.offering_id,
                        principalTable: "Offering",
                        principalColumn: "offering_id");
                    table.ForeignKey(
                        name: "FK_Mark_Submission_User_Account_approved_by",
                        column: x => x.approved_by,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_Mark_Submission_User_Account_submitted_by",
                        column: x => x.submitted_by,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_Mark_Submission_User_Account_verified_by",
                        column: x => x.verified_by,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Offering_Teacher",
                columns: table => new
                {
                    offering_id = table.Column<int>(type: "int", nullable: false),
                    faculty_id = table.Column<int>(type: "int", nullable: false),
                    assigned_on = table.Column<DateTime>(type: "date", nullable: false),
                    ended_on = table.Column<DateTime>(type: "date", nullable: true),
                    is_retake_marker = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offering_Teacher", x => new { x.offering_id, x.faculty_id });
                    table.ForeignKey(
                        name: "FK_Offering_Teacher_Faculty_faculty_id",
                        column: x => x.faculty_id,
                        principalTable: "Faculty",
                        principalColumn: "faculty_id");
                    table.ForeignKey(
                        name: "FK_Offering_Teacher_Offering_offering_id",
                        column: x => x.offering_id,
                        principalTable: "Offering",
                        principalColumn: "offering_id");
                });

            migrationBuilder.CreateTable(
                name: "Course_Equivalence",
                columns: table => new
                {
                    equivalence_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    old_course_id = table.Column<int>(type: "int", nullable: false),
                    new_course_id = table.Column<int>(type: "int", nullable: false),
                    approval_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course_Equivalence", x => x.equivalence_id);
                    table.ForeignKey(
                        name: "FK_Course_Equivalence_Approval_Record_approval_id",
                        column: x => x.approval_id,
                        principalTable: "Approval_Record",
                        principalColumn: "approval_id");
                    table.ForeignKey(
                        name: "FK_Course_Equivalence_Course_new_course_id",
                        column: x => x.new_course_id,
                        principalTable: "Course",
                        principalColumn: "course_id");
                    table.ForeignKey(
                        name: "FK_Course_Equivalence_Course_old_course_id",
                        column: x => x.old_course_id,
                        principalTable: "Course",
                        principalColumn: "course_id");
                    table.ForeignKey(
                        name: "FK_Course_Equivalence_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "Eligibility_Hold",
                columns: table => new
                {
                    hold_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    event_id = table.Column<int>(type: "int", nullable: false),
                    course_id = table.Column<int>(type: "int", nullable: true),
                    hold_type = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    released_by = table.Column<int>(type: "int", nullable: true),
                    release_reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    released_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approval_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eligibility_Hold", x => x.hold_id);
                    table.ForeignKey(
                        name: "FK_Eligibility_Hold_Approval_Record_approval_id",
                        column: x => x.approval_id,
                        principalTable: "Approval_Record",
                        principalColumn: "approval_id");
                    table.ForeignKey(
                        name: "FK_Eligibility_Hold_Course_course_id",
                        column: x => x.course_id,
                        principalTable: "Course",
                        principalColumn: "course_id");
                    table.ForeignKey(
                        name: "FK_Eligibility_Hold_Exam_Event_event_id",
                        column: x => x.event_id,
                        principalTable: "Exam_Event",
                        principalColumn: "event_id");
                    table.ForeignKey(
                        name: "FK_Eligibility_Hold_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id");
                    table.ForeignKey(
                        name: "FK_Eligibility_Hold_User_Account_released_by",
                        column: x => x.released_by,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Enrollment",
                columns: table => new
                {
                    enrollment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    offering_id = table.Column<int>(type: "int", nullable: false),
                    enrollment_type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    attempt_no = table.Column<byte>(type: "tinyint", nullable: false),
                    status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    approval_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollment", x => x.enrollment_id);
                    table.ForeignKey(
                        name: "FK_Enrollment_Approval_Record_approval_id",
                        column: x => x.approval_id,
                        principalTable: "Approval_Record",
                        principalColumn: "approval_id");
                    table.ForeignKey(
                        name: "FK_Enrollment_Offering_offering_id",
                        column: x => x.offering_id,
                        principalTable: "Offering",
                        principalColumn: "offering_id");
                    table.ForeignKey(
                        name: "FK_Enrollment_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "Fee_Waiver",
                columns: table => new
                {
                    waiver_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    term_id = table.Column<int>(type: "int", nullable: false),
                    discount_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    approval_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fee_Waiver", x => x.waiver_id);
                    table.ForeignKey(
                        name: "FK_Fee_Waiver_Academic_Term_term_id",
                        column: x => x.term_id,
                        principalTable: "Academic_Term",
                        principalColumn: "term_id");
                    table.ForeignKey(
                        name: "FK_Fee_Waiver_Approval_Record_approval_id",
                        column: x => x.approval_id,
                        principalTable: "Approval_Record",
                        principalColumn: "approval_id");
                    table.ForeignKey(
                        name: "FK_Fee_Waiver_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<string>(type: "char(8)", maxLength: 8, nullable: true),
                    applicant_id = table.Column<int>(type: "int", nullable: true),
                    method = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    gateway_txn_id = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    recorded_by = table.Column<int>(type: "int", nullable: true),
                    reverses_payment_id = table.Column<int>(type: "int", nullable: true),
                    reversal_reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.payment_id);
                    table.CheckConstraint("CK_Payment_Owner", "[student_id] IS NOT NULL OR [applicant_id] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Payment_Applicant_applicant_id",
                        column: x => x.applicant_id,
                        principalTable: "Applicant",
                        principalColumn: "applicant_id");
                    table.ForeignKey(
                        name: "FK_Payment_Payment_reverses_payment_id",
                        column: x => x.reverses_payment_id,
                        principalTable: "Payment",
                        principalColumn: "payment_id");
                    table.ForeignKey(
                        name: "FK_Payment_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id");
                    table.ForeignKey(
                        name: "FK_Payment_User_Account_recorded_by",
                        column: x => x.recorded_by,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Seat_Allocation",
                columns: table => new
                {
                    seat_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sitting_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    room_id = table.Column<int>(type: "int", nullable: false),
                    seat_no = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    makeup_day = table.Column<DateTime>(type: "date", nullable: true),
                    clash_confirmed_by = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seat_Allocation", x => x.seat_id);
                    table.ForeignKey(
                        name: "FK_Seat_Allocation_Exam_Sitting_sitting_id",
                        column: x => x.sitting_id,
                        principalTable: "Exam_Sitting",
                        principalColumn: "sitting_id");
                    table.ForeignKey(
                        name: "FK_Seat_Allocation_Room_room_id",
                        column: x => x.room_id,
                        principalTable: "Room",
                        principalColumn: "room_id");
                    table.ForeignKey(
                        name: "FK_Seat_Allocation_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id");
                    table.ForeignKey(
                        name: "FK_Seat_Allocation_User_Account_clash_confirmed_by",
                        column: x => x.clash_confirmed_by,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Semester_Drop",
                columns: table => new
                {
                    drop_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    dropped_semester_no = table.Column<byte>(type: "tinyint", nullable: false),
                    target_batch_id = table.Column<int>(type: "int", nullable: true),
                    rejoin_semester_no = table.Column<byte>(type: "tinyint", nullable: false),
                    status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    recommended_by = table.Column<int>(type: "int", nullable: false),
                    approval_id = table.Column<int>(type: "int", nullable: true),
                    requested_on = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semester_Drop", x => x.drop_id);
                    table.ForeignKey(
                        name: "FK_Semester_Drop_Approval_Record_approval_id",
                        column: x => x.approval_id,
                        principalTable: "Approval_Record",
                        principalColumn: "approval_id");
                    table.ForeignKey(
                        name: "FK_Semester_Drop_Batch_target_batch_id",
                        column: x => x.target_batch_id,
                        principalTable: "Batch",
                        principalColumn: "batch_id");
                    table.ForeignKey(
                        name: "FK_Semester_Drop_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id");
                    table.ForeignKey(
                        name: "FK_Semester_Drop_User_Account_recommended_by",
                        column: x => x.recommended_by,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Semester_Result",
                columns: table => new
                {
                    semester_result_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    term_id = table.Column<int>(type: "int", nullable: false),
                    semester_no = table.Column<byte>(type: "tinyint", nullable: false),
                    credits_earned = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    semester_gpa = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    cgpa = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    version_no = table.Column<int>(type: "int", nullable: false),
                    published_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    supersedes_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semester_Result", x => x.semester_result_id);
                    table.ForeignKey(
                        name: "FK_Semester_Result_Academic_Term_term_id",
                        column: x => x.term_id,
                        principalTable: "Academic_Term",
                        principalColumn: "term_id");
                    table.ForeignKey(
                        name: "FK_Semester_Result_Semester_Result_supersedes_id",
                        column: x => x.supersedes_id,
                        principalTable: "Semester_Result",
                        principalColumn: "semester_result_id");
                    table.ForeignKey(
                        name: "FK_Semester_Result_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "Student_Batch_History",
                columns: table => new
                {
                    move_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    batch_id = table.Column<int>(type: "int", nullable: false),
                    section_id = table.Column<int>(type: "int", nullable: false),
                    from_date = table.Column<DateTime>(type: "date", nullable: false),
                    to_date = table.Column<DateTime>(type: "date", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    approval_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student_Batch_History", x => x.move_id);
                    table.ForeignKey(
                        name: "FK_Student_Batch_History_Approval_Record_approval_id",
                        column: x => x.approval_id,
                        principalTable: "Approval_Record",
                        principalColumn: "approval_id");
                    table.ForeignKey(
                        name: "FK_Student_Batch_History_Batch_batch_id",
                        column: x => x.batch_id,
                        principalTable: "Batch",
                        principalColumn: "batch_id");
                    table.ForeignKey(
                        name: "FK_Student_Batch_History_Section_section_id",
                        column: x => x.section_id,
                        principalTable: "Section",
                        principalColumn: "section_id");
                    table.ForeignKey(
                        name: "FK_Student_Batch_History_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "Student_Status_History",
                columns: table => new
                {
                    history_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    effective_date = table.Column<DateTime>(type: "date", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    approval_id = table.Column<int>(type: "int", nullable: true),
                    changed_by = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student_Status_History", x => x.history_id);
                    table.ForeignKey(
                        name: "FK_Student_Status_History_Approval_Record_approval_id",
                        column: x => x.approval_id,
                        principalTable: "Approval_Record",
                        principalColumn: "approval_id");
                    table.ForeignKey(
                        name: "FK_Student_Status_History_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id");
                    table.ForeignKey(
                        name: "FK_Student_Status_History_User_Account_changed_by",
                        column: x => x.changed_by,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Uploaded_Document",
                columns: table => new
                {
                    document_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    applicant_id = table.Column<int>(type: "int", nullable: true),
                    student_id = table.Column<string>(type: "char(8)", maxLength: 8, nullable: true),
                    document_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    file_path = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_size_kb = table.Column<int>(type: "int", nullable: false),
                    mime_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uploaded_Document", x => x.document_id);
                    table.CheckConstraint("CK_Uploaded_Document_Owner", "[applicant_id] IS NOT NULL OR [student_id] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Uploaded_Document_Applicant_applicant_id",
                        column: x => x.applicant_id,
                        principalTable: "Applicant",
                        principalColumn: "applicant_id");
                    table.ForeignKey(
                        name: "FK_Uploaded_Document_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "Attendance_Record",
                columns: table => new
                {
                    attendance_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enrollment_id = table.Column<int>(type: "int", nullable: false),
                    routine_id = table.Column<int>(type: "int", nullable: false),
                    class_date = table.Column<DateTime>(type: "date", nullable: false),
                    status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    recorded_by = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendance_Record", x => x.attendance_id);
                    table.ForeignKey(
                        name: "FK_Attendance_Record_Class_Routine_routine_id",
                        column: x => x.routine_id,
                        principalTable: "Class_Routine",
                        principalColumn: "routine_id");
                    table.ForeignKey(
                        name: "FK_Attendance_Record_Enrollment_enrollment_id",
                        column: x => x.enrollment_id,
                        principalTable: "Enrollment",
                        principalColumn: "enrollment_id");
                    table.ForeignKey(
                        name: "FK_Attendance_Record_User_Account_recorded_by",
                        column: x => x.recorded_by,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Due",
                columns: table => new
                {
                    due_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<string>(type: "char(8)", maxLength: 8, nullable: true),
                    applicant_id = table.Column<int>(type: "int", nullable: true),
                    fee_type_id = table.Column<int>(type: "int", nullable: false),
                    term_id = table.Column<int>(type: "int", nullable: true),
                    enrollment_id = table.Column<int>(type: "int", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    due_date = table.Column<DateTime>(type: "date", nullable: false),
                    status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Due", x => x.due_id);
                    table.CheckConstraint("CK_Due_Owner", "[student_id] IS NOT NULL OR [applicant_id] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Due_Academic_Term_term_id",
                        column: x => x.term_id,
                        principalTable: "Academic_Term",
                        principalColumn: "term_id");
                    table.ForeignKey(
                        name: "FK_Due_Applicant_applicant_id",
                        column: x => x.applicant_id,
                        principalTable: "Applicant",
                        principalColumn: "applicant_id");
                    table.ForeignKey(
                        name: "FK_Due_Enrollment_enrollment_id",
                        column: x => x.enrollment_id,
                        principalTable: "Enrollment",
                        principalColumn: "enrollment_id");
                    table.ForeignKey(
                        name: "FK_Due_Fee_Type_fee_type_id",
                        column: x => x.fee_type_id,
                        principalTable: "Fee_Type",
                        principalColumn: "fee_type_id");
                    table.ForeignKey(
                        name: "FK_Due_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "Mark_Entry",
                columns: table => new
                {
                    entry_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enrollment_id = table.Column<int>(type: "int", nullable: false),
                    component_id = table.Column<int>(type: "int", nullable: false),
                    submission_id = table.Column<int>(type: "int", nullable: false),
                    marks = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    is_absent = table.Column<bool>(type: "bit", nullable: false),
                    entered_by = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mark_Entry", x => x.entry_id);
                    table.ForeignKey(
                        name: "FK_Mark_Entry_Enrollment_enrollment_id",
                        column: x => x.enrollment_id,
                        principalTable: "Enrollment",
                        principalColumn: "enrollment_id");
                    table.ForeignKey(
                        name: "FK_Mark_Entry_Mark_Component_component_id",
                        column: x => x.component_id,
                        principalTable: "Mark_Component",
                        principalColumn: "component_id");
                    table.ForeignKey(
                        name: "FK_Mark_Entry_Mark_Submission_submission_id",
                        column: x => x.submission_id,
                        principalTable: "Mark_Submission",
                        principalColumn: "submission_id");
                    table.ForeignKey(
                        name: "FK_Mark_Entry_User_Account_entered_by",
                        column: x => x.entered_by,
                        principalTable: "User_Account",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Result",
                columns: table => new
                {
                    result_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enrollment_id = table.Column<int>(type: "int", nullable: false),
                    version_no = table.Column<int>(type: "int", nullable: false),
                    total_marks = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    letter_grade = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    grade_point = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    result_status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    is_replaced = table.Column<bool>(type: "bit", nullable: false),
                    grade_scale_version = table.Column<int>(type: "int", nullable: false),
                    attendance_scale_version = table.Column<int>(type: "int", nullable: false),
                    published_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    supersedes_result_id = table.Column<int>(type: "int", nullable: true),
                    correction_reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    approval_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Result", x => x.result_id);
                    table.ForeignKey(
                        name: "FK_Result_Approval_Record_approval_id",
                        column: x => x.approval_id,
                        principalTable: "Approval_Record",
                        principalColumn: "approval_id");
                    table.ForeignKey(
                        name: "FK_Result_Enrollment_enrollment_id",
                        column: x => x.enrollment_id,
                        principalTable: "Enrollment",
                        principalColumn: "enrollment_id");
                    table.ForeignKey(
                        name: "FK_Result_Result_supersedes_result_id",
                        column: x => x.supersedes_result_id,
                        principalTable: "Result",
                        principalColumn: "result_id");
                });

            migrationBuilder.CreateTable(
                name: "Gateway_Callback",
                columns: table => new
                {
                    callback_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    gateway_txn_id = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    payment_id = table.Column<int>(type: "int", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    signature_valid = table.Column<bool>(type: "bit", nullable: false),
                    match_status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    received_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gateway_Callback", x => x.callback_id);
                    table.ForeignKey(
                        name: "FK_Gateway_Callback_Payment_payment_id",
                        column: x => x.payment_id,
                        principalTable: "Payment",
                        principalColumn: "payment_id");
                });

            migrationBuilder.CreateTable(
                name: "Receipt",
                columns: table => new
                {
                    receipt_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    payment_id = table.Column<int>(type: "int", nullable: false),
                    receipt_no = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    issued_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipt", x => x.receipt_id);
                    table.ForeignKey(
                        name: "FK_Receipt_Payment_payment_id",
                        column: x => x.payment_id,
                        principalTable: "Payment",
                        principalColumn: "payment_id");
                });

            migrationBuilder.CreateTable(
                name: "Document_Request",
                columns: table => new
                {
                    request_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<string>(type: "char(8)", maxLength: 8, nullable: false),
                    request_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    due_id = table.Column<int>(type: "int", nullable: true),
                    requested_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    issued_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document_Request", x => x.request_id);
                    table.ForeignKey(
                        name: "FK_Document_Request_Due_due_id",
                        column: x => x.due_id,
                        principalTable: "Due",
                        principalColumn: "due_id");
                    table.ForeignKey(
                        name: "FK_Document_Request_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "Payment_Allocation",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "int", nullable: false),
                    due_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment_Allocation", x => new { x.payment_id, x.due_id });
                    table.ForeignKey(
                        name: "FK_Payment_Allocation_Due_due_id",
                        column: x => x.due_id,
                        principalTable: "Due",
                        principalColumn: "due_id");
                    table.ForeignKey(
                        name: "FK_Payment_Allocation_Payment_payment_id",
                        column: x => x.payment_id,
                        principalTable: "Payment",
                        principalColumn: "payment_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Academic_Calendar_Event_term_id",
                table: "Academic_Calendar_Event",
                column: "term_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Academic_Term_year_term_no",
                table: "Academic_Term",
                columns: new[] { "year", "term_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Admission_Circular_intake_year",
                table: "Admission_Circular",
                column: "intake_year",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Applicant_ssc_board_ssc_roll_hsc_board_hsc_roll",
                table: "Applicant",
                columns: new[] { "ssc_board", "ssc_roll", "hsc_board", "hsc_roll" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Applicant_user_id",
                table: "Applicant",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Application_approval_id",
                table: "Application",
                column: "approval_id");

            migrationBuilder.CreateIndex(
                name: "IX_Application_circular_id",
                table: "Application",
                column: "circular_id");

            migrationBuilder.CreateIndex(
                name: "IX_Application_department_id",
                table: "Application",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Application_applicant_id_circular_id",
                table: "Application",
                columns: new[] { "applicant_id", "circular_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Approval_Record_approver_id",
                table: "Approval_Record",
                column: "approver_id");

            migrationBuilder.CreateIndex(
                name: "IX_Approval_Record_supersedes_id",
                table: "Approval_Record",
                column: "supersedes_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Approval_Record_reference_no",
                table: "Approval_Record",
                column: "reference_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Record_recorded_by",
                table: "Attendance_Record",
                column: "recorded_by");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Record_routine_id",
                table: "Attendance_Record",
                column: "routine_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Attendance_Record_enrollment_id_routine_id_class_date",
                table: "Attendance_Record",
                columns: new[] { "enrollment_id", "routine_id", "class_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Attendance_Scale_department_id_version_no_min_percent",
                table: "Attendance_Scale",
                columns: new[] { "department_id", "version_no", "min_percent" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Audit_Entry_user_id",
                table: "Audit_Entry",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Background_Job_requested_by",
                table: "Background_Job",
                column: "requested_by");

            migrationBuilder.CreateIndex(
                name: "IX_Batch_curriculum_id",
                table: "Batch",
                column: "curriculum_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Batch_department_id_admission_year",
                table: "Batch",
                columns: new[] { "department_id", "admission_year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Circular_Seat_department_id",
                table: "Circular_Seat",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_Class_Routine_offering_id",
                table: "Class_Routine",
                column: "offering_id");

            migrationBuilder.CreateIndex(
                name: "IX_Class_Routine_room_id",
                table: "Class_Routine",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_Config_Parameter_changed_by",
                table: "Config_Parameter",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "IX_Config_Parameter_effective_term_id",
                table: "Config_Parameter",
                column: "effective_term_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Config_Parameter_department_id_param_key_version_no",
                table: "Config_Parameter",
                columns: new[] { "department_id", "param_key", "version_no" },
                unique: true,
                filter: "[department_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Course_department_id",
                table: "Course",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Course_code",
                table: "Course",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Course_Equivalence_approval_id",
                table: "Course_Equivalence",
                column: "approval_id");

            migrationBuilder.CreateIndex(
                name: "IX_Course_Equivalence_new_course_id",
                table: "Course_Equivalence",
                column: "new_course_id");

            migrationBuilder.CreateIndex(
                name: "IX_Course_Equivalence_old_course_id",
                table: "Course_Equivalence",
                column: "old_course_id");

            migrationBuilder.CreateIndex(
                name: "IX_Course_Equivalence_student_id",
                table: "Course_Equivalence",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_Course_Prerequisite_prerequisite_course_id",
                table: "Course_Prerequisite",
                column: "prerequisite_course_id");

            migrationBuilder.CreateIndex(
                name: "IX_Curriculum_Course_course_id",
                table: "Curriculum_Course",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_Curriculum_Version_approval_id",
                table: "Curriculum_Version",
                column: "approval_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Curriculum_Version_program_id_version_no",
                table: "Curriculum_Version",
                columns: new[] { "program_id", "version_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Department_code",
                table: "Department",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Department_id_code",
                table: "Department",
                column: "id_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Department_name",
                table: "Department",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Document_Request_due_id",
                table: "Document_Request",
                column: "due_id");

            migrationBuilder.CreateIndex(
                name: "IX_Document_Request_student_id",
                table: "Document_Request",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_Due_applicant_id",
                table: "Due",
                column: "applicant_id");

            migrationBuilder.CreateIndex(
                name: "IX_Due_enrollment_id",
                table: "Due",
                column: "enrollment_id");

            migrationBuilder.CreateIndex(
                name: "IX_Due_fee_type_id",
                table: "Due",
                column: "fee_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_Due_student_id",
                table: "Due",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_Due_term_id",
                table: "Due",
                column: "term_id");

            migrationBuilder.CreateIndex(
                name: "IX_Eligibility_Hold_approval_id",
                table: "Eligibility_Hold",
                column: "approval_id");

            migrationBuilder.CreateIndex(
                name: "IX_Eligibility_Hold_course_id",
                table: "Eligibility_Hold",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_Eligibility_Hold_event_id",
                table: "Eligibility_Hold",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_Eligibility_Hold_released_by",
                table: "Eligibility_Hold",
                column: "released_by");

            migrationBuilder.CreateIndex(
                name: "IX_Eligibility_Hold_student_id",
                table: "Eligibility_Hold",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_approval_id",
                table: "Enrollment",
                column: "approval_id");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_offering_id",
                table: "Enrollment",
                column: "offering_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Enrollment_student_id_offering_id",
                table: "Enrollment",
                columns: new[] { "student_id", "offering_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Event_term_id",
                table: "Exam_Event",
                column: "term_id");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Sitting_course_id",
                table: "Exam_Sitting",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Sitting_event_id",
                table: "Exam_Sitting",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_Faculty_department_id",
                table: "Faculty",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Faculty_employee_ref",
                table: "Faculty",
                column: "employee_ref",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Faculty_user_id",
                table: "Faculty",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fee_Type_department_id",
                table: "Fee_Type",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_Fee_Waiver_approval_id",
                table: "Fee_Waiver",
                column: "approval_id");

            migrationBuilder.CreateIndex(
                name: "IX_Fee_Waiver_term_id",
                table: "Fee_Waiver",
                column: "term_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Fee_Waiver_student_id_term_id",
                table: "Fee_Waiver",
                columns: new[] { "student_id", "term_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gateway_Callback_payment_id",
                table: "Gateway_Callback",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Grade_Scale_department_id_version_no_letter_grade",
                table: "Grade_Scale",
                columns: new[] { "department_id", "version_no", "letter_grade" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Mark_Component_department_id_course_type_name_version_no",
                table: "Mark_Component",
                columns: new[] { "department_id", "course_type", "name", "version_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mark_Entry_component_id",
                table: "Mark_Entry",
                column: "component_id");

            migrationBuilder.CreateIndex(
                name: "IX_Mark_Entry_entered_by",
                table: "Mark_Entry",
                column: "entered_by");

            migrationBuilder.CreateIndex(
                name: "IX_Mark_Entry_submission_id",
                table: "Mark_Entry",
                column: "submission_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Mark_Entry_enrollment_id_component_id",
                table: "Mark_Entry",
                columns: new[] { "enrollment_id", "component_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mark_Submission_approved_by",
                table: "Mark_Submission",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_Mark_Submission_offering_id",
                table: "Mark_Submission",
                column: "offering_id");

            migrationBuilder.CreateIndex(
                name: "IX_Mark_Submission_submitted_by",
                table: "Mark_Submission",
                column: "submitted_by");

            migrationBuilder.CreateIndex(
                name: "IX_Mark_Submission_verified_by",
                table: "Mark_Submission",
                column: "verified_by");

            migrationBuilder.CreateIndex(
                name: "IX_Mentor_Assignment_batch_id",
                table: "Mentor_Assignment",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_Mentor_Assignment_faculty_id",
                table: "Mentor_Assignment",
                column: "faculty_id");

            migrationBuilder.CreateIndex(
                name: "IX_Mentor_Assignment_section_id",
                table: "Mentor_Assignment",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notice_approved_by",
                table: "Notice",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_Notice_author_id",
                table: "Notice",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notice_department_id",
                table: "Notice",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_recipient_user_id",
                table: "Notification",
                column: "recipient_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_template_id",
                table: "Notification",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Notification_event_key_recipient_user_id",
                table: "Notification",
                columns: new[] { "event_key", "recipient_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Notification_Template_code",
                table: "Notification_Template",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Offering_batch_id",
                table: "Offering",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_Offering_course_id",
                table: "Offering",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_Offering_section_id",
                table: "Offering",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Offering_term_id_section_id_course_id",
                table: "Offering",
                columns: new[] { "term_id", "section_id", "course_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Offering_Teacher_faculty_id",
                table: "Offering_Teacher",
                column: "faculty_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_applicant_id",
                table: "Payment",
                column: "applicant_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_recorded_by",
                table: "Payment",
                column: "recorded_by");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_reverses_payment_id",
                table: "Payment",
                column: "reverses_payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_student_id",
                table: "Payment",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Payment_gateway_txn_id",
                table: "Payment",
                column: "gateway_txn_id",
                unique: true,
                filter: "[gateway_txn_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_Allocation_due_id",
                table: "Payment_Allocation",
                column: "due_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Program_department_id",
                table: "Program",
                column: "department_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Receipt_payment_id",
                table: "Receipt",
                column: "payment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Receipt_receipt_no",
                table: "Receipt",
                column: "receipt_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Result_approval_id",
                table: "Result",
                column: "approval_id");

            migrationBuilder.CreateIndex(
                name: "IX_Result_supersedes_result_id",
                table: "Result",
                column: "supersedes_result_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Result_enrollment_id_version_no",
                table: "Result",
                columns: new[] { "enrollment_id", "version_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Role_name",
                table: "Role",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Room_name",
                table: "Room",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seat_Allocation_clash_confirmed_by",
                table: "Seat_Allocation",
                column: "clash_confirmed_by");

            migrationBuilder.CreateIndex(
                name: "IX_Seat_Allocation_room_id",
                table: "Seat_Allocation",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_Seat_Allocation_student_id",
                table: "Seat_Allocation",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Seat_Allocation_sitting_id_student_id",
                table: "Seat_Allocation",
                columns: new[] { "sitting_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Section_batch_id_name",
                table: "Section",
                columns: new[] { "batch_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Semester_Drop_approval_id",
                table: "Semester_Drop",
                column: "approval_id");

            migrationBuilder.CreateIndex(
                name: "IX_Semester_Drop_recommended_by",
                table: "Semester_Drop",
                column: "recommended_by");

            migrationBuilder.CreateIndex(
                name: "IX_Semester_Drop_target_batch_id",
                table: "Semester_Drop",
                column: "target_batch_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Semester_Drop_student_id",
                table: "Semester_Drop",
                column: "student_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Semester_Result_student_id",
                table: "Semester_Result",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_Semester_Result_supersedes_id",
                table: "Semester_Result",
                column: "supersedes_id");

            migrationBuilder.CreateIndex(
                name: "IX_Semester_Result_term_id",
                table: "Semester_Result",
                column: "term_id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_current_batch_id",
                table: "Student",
                column: "current_batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_department_id",
                table: "Student",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_section_id",
                table: "Student",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Student_admission_batch_id_roll_no",
                table: "Student",
                columns: new[] { "admission_batch_id", "roll_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Student_application_id",
                table: "Student",
                column: "application_id",
                unique: true,
                filter: "[application_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_Student_user_id",
                table: "Student",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Student_Batch_History_approval_id",
                table: "Student_Batch_History",
                column: "approval_id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Batch_History_batch_id",
                table: "Student_Batch_History",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Batch_History_section_id",
                table: "Student_Batch_History",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Batch_History_student_id",
                table: "Student_Batch_History",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Status_History_approval_id",
                table: "Student_Status_History",
                column: "approval_id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Status_History_changed_by",
                table: "Student_Status_History",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Status_History_student_id",
                table: "Student_Status_History",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_Uploaded_Document_applicant_id",
                table: "Uploaded_Document",
                column: "applicant_id");

            migrationBuilder.CreateIndex(
                name: "IX_Uploaded_Document_student_id",
                table: "Uploaded_Document",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "UQ_User_Account_email",
                table: "User_Account",
                column: "email",
                unique: true,
                filter: "[email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_User_Account_username",
                table: "User_Account",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Role_department_id",
                table: "User_Role",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Role_role_id",
                table: "User_Role",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_Verification_Token_user_id",
                table: "Verification_Token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Web_Page_department_id",
                table: "Web_Page",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Web_Page_slug",
                table: "Web_Page",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Web_Page_Version_approved_by",
                table: "Web_Page_Version",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_Web_Page_Version_author_id",
                table: "Web_Page_Version",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Web_Page_Version_page_id_version_no",
                table: "Web_Page_Version",
                columns: new[] { "page_id", "version_no" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Academic_Calendar_Event");

            migrationBuilder.DropTable(
                name: "Attendance_Record");

            migrationBuilder.DropTable(
                name: "Attendance_Scale");

            migrationBuilder.DropTable(
                name: "Audit_Entry");

            migrationBuilder.DropTable(
                name: "Background_Job");

            migrationBuilder.DropTable(
                name: "Circular_Seat");

            migrationBuilder.DropTable(
                name: "Config_Parameter");

            migrationBuilder.DropTable(
                name: "Course_Equivalence");

            migrationBuilder.DropTable(
                name: "Course_Prerequisite");

            migrationBuilder.DropTable(
                name: "Curriculum_Course");

            migrationBuilder.DropTable(
                name: "Document_Request");

            migrationBuilder.DropTable(
                name: "Eligibility_Hold");

            migrationBuilder.DropTable(
                name: "Fee_Waiver");

            migrationBuilder.DropTable(
                name: "Gateway_Callback");

            migrationBuilder.DropTable(
                name: "Grade_Scale");

            migrationBuilder.DropTable(
                name: "Mark_Entry");

            migrationBuilder.DropTable(
                name: "Mentor_Assignment");

            migrationBuilder.DropTable(
                name: "Notice");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "Offering_Teacher");

            migrationBuilder.DropTable(
                name: "Payment_Allocation");

            migrationBuilder.DropTable(
                name: "Receipt");

            migrationBuilder.DropTable(
                name: "Result");

            migrationBuilder.DropTable(
                name: "Role_Permission");

            migrationBuilder.DropTable(
                name: "Seat_Allocation");

            migrationBuilder.DropTable(
                name: "Semester_Drop");

            migrationBuilder.DropTable(
                name: "Semester_Result");

            migrationBuilder.DropTable(
                name: "Student_Batch_History");

            migrationBuilder.DropTable(
                name: "Student_Status_History");

            migrationBuilder.DropTable(
                name: "Uploaded_Document");

            migrationBuilder.DropTable(
                name: "User_Role");

            migrationBuilder.DropTable(
                name: "Verification_Token");

            migrationBuilder.DropTable(
                name: "Web_Page_Version");

            migrationBuilder.DropTable(
                name: "Class_Routine");

            migrationBuilder.DropTable(
                name: "Mark_Component");

            migrationBuilder.DropTable(
                name: "Mark_Submission");

            migrationBuilder.DropTable(
                name: "Notification_Template");

            migrationBuilder.DropTable(
                name: "Faculty");

            migrationBuilder.DropTable(
                name: "Due");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "Exam_Sitting");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Web_Page");

            migrationBuilder.DropTable(
                name: "Room");

            migrationBuilder.DropTable(
                name: "Enrollment");

            migrationBuilder.DropTable(
                name: "Fee_Type");

            migrationBuilder.DropTable(
                name: "Exam_Event");

            migrationBuilder.DropTable(
                name: "Offering");

            migrationBuilder.DropTable(
                name: "Student");

            migrationBuilder.DropTable(
                name: "Academic_Term");

            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.DropTable(
                name: "Application");

            migrationBuilder.DropTable(
                name: "Section");

            migrationBuilder.DropTable(
                name: "Admission_Circular");

            migrationBuilder.DropTable(
                name: "Applicant");

            migrationBuilder.DropTable(
                name: "Batch");

            migrationBuilder.DropTable(
                name: "Curriculum_Version");

            migrationBuilder.DropTable(
                name: "Approval_Record");

            migrationBuilder.DropTable(
                name: "Program");

            migrationBuilder.DropTable(
                name: "User_Account");

            migrationBuilder.DropTable(
                name: "Department");
        }
    }
}
