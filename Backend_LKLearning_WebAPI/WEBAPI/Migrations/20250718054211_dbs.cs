using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEBAPI.Migrations
{
    /// <inheritdoc />
    public partial class dbs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banner",
                columns: table => new
                {
                    BannerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BannerTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BannerSubtitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BannerDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BannerImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LinkUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Banner__32E86AD1856F050A", x => x.BannerId);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Category__19093A2B83CE4368", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "ContentType",
                columns: table => new
                {
                    ContentTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeDescription = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentType", x => x.ContentTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Lesson",
                columns: table => new
                {
                    LessonID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LessonContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LessonDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Duration = table.Column<DateOnly>(type: "date", nullable: false),
                    DurationMinute = table.Column<int>(type: "int", nullable: false),
                    IsActivate = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Lesson__B084ACB04C392F71", x => x.LessonID);
                });

            migrationBuilder.CreateTable(
                name: "Level",
                columns: table => new
                {
                    LevelID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LevelName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Level__09F03C06643F612B", x => x.LevelID);
                });

            migrationBuilder.CreateTable(
                name: "Package",
                columns: table => new
                {
                    PackageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DurationDay = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    urlImage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Package__322035EC4E16E07A", x => x.PackageID);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaymentM__DC31C1D32CC7DAB2", x => x.PaymentMethodId);
                });

            migrationBuilder.CreateTable(
                name: "QuestionLevel",
                columns: table => new
                {
                    QuestionLevelID = table.Column<int>(type: "int", nullable: false),
                    QuestionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Question__4DCE689F3DD12B96", x => x.QuestionLevelID);
                });

            migrationBuilder.CreateTable(
                name: "QuestionType",
                columns: table => new
                {
                    QuestionTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TypeDescription = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Question__7EDFF9313CB97AE0", x => x.QuestionTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ROLE",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ROLE__8AFACE3A263754CA", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "Vocabularies",
                columns: table => new
                {
                    VocabularyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Word = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Pronunciation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AudioUrlUK = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    AudioUrlUS = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Vocabula__927406BFDB159057", x => x.VocabularyId);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyCategories",
                columns: table => new
                {
                    VocabularyCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VocabularyCategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VocabularyCategoryDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    urlImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Vocabula__22F98D85DE0C00E7", x => x.VocabularyCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    CourseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurationInMonths = table.Column<int>(type: "int", nullable: false),
                    LevelID = table.Column<int>(type: "int", nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    PackageID = table.Column<int>(type: "int", nullable: true),
                    img = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Course__C92D71872CDF6125", x => x.CourseID);
                    table.ForeignKey(
                        name: "FK_Course_Category",
                        column: x => x.CategoryID,
                        principalTable: "Category",
                        principalColumn: "CategoryID");
                    table.ForeignKey(
                        name: "FK_Course_Level",
                        column: x => x.LevelID,
                        principalTable: "Level",
                        principalColumn: "LevelID");
                    table.ForeignKey(
                        name: "FK_Course_Package",
                        column: x => x.PackageID,
                        principalTable: "Package",
                        principalColumn: "PackageID");
                });

            migrationBuilder.CreateTable(
                name: "PackageInclusion",
                columns: table => new
                {
                    ParentPackageID = table.Column<int>(type: "int", nullable: false),
                    IncludedPackageID = table.Column<int>(type: "int", nullable: false),
                    DateAdd = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageInclusion", x => new { x.ParentPackageID, x.IncludedPackageID });
                    table.ForeignKey(
                        name: "FK_PackageInclusion_IncludedPackage",
                        column: x => x.IncludedPackageID,
                        principalTable: "Package",
                        principalColumn: "PackageID");
                    table.ForeignKey(
                        name: "FK_PackageInclusion_ParentPackage",
                        column: x => x.ParentPackageID,
                        principalTable: "Package",
                        principalColumn: "PackageID");
                });

            migrationBuilder.CreateTable(
                name: "Question",
                columns: table => new
                {
                    QuestionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContentTypeID = table.Column<int>(type: "int", nullable: true),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionTypeId = table.Column<int>(type: "int", nullable: true),
                    AnswerOptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AudioUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuestionLevelID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Question__0DC06F8CC0A35A91", x => x.QuestionID);
                    table.ForeignKey(
                        name: "FK_Question_ContentType",
                        column: x => x.ContentTypeID,
                        principalTable: "ContentType",
                        principalColumn: "ContentTypeID");
                    table.ForeignKey(
                        name: "FK_Question_QuestionLevel",
                        column: x => x.QuestionLevelID,
                        principalTable: "QuestionLevel",
                        principalColumn: "QuestionLevelID");
                    table.ForeignKey(
                        name: "FK_Question_QuestionType",
                        column: x => x.QuestionTypeId,
                        principalTable: "QuestionType",
                        principalColumn: "QuestionTypeId");
                });

            migrationBuilder.CreateTable(
                name: "_USER",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fullname = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Age = table.Column<int>(type: "int", nullable: true),
                    Phone = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    Gender = table.Column<bool>(type: "bit", nullable: true),
                    DateofBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    RoleID = table.Column<int>(type: "int", nullable: true),
                    lastLoginDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK___USER__1788CCAC82B0D5D2", x => x.UserID);
                    table.ForeignKey(
                        name: "FK___USER__RoleID__398D8EEE",
                        column: x => x.RoleID,
                        principalTable: "ROLE",
                        principalColumn: "RoleID");
                });

            migrationBuilder.CreateTable(
                name: "VocabularyMeanings",
                columns: table => new
                {
                    VocabularyMeaningId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VocabularyId = table.Column<int>(type: "int", nullable: false),
                    Meaning = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExampleSentence = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TranslatedMeaning = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TranslatedExampleSentence = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Synonyms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Antonyms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartOfSpeech = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Vocabula__01FBC49325399152", x => x.VocabularyMeaningId);
                    table.ForeignKey(
                        name: "FK__VocabMeaning__Vocab__11216507",
                        column: x => x.VocabularyId,
                        principalTable: "Vocabularies",
                        principalColumn: "VocabularyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyCategoryMapping",
                columns: table => new
                {
                    VocabularyID = table.Column<int>(type: "int", nullable: false),
                    VocabularyCategoryID = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyCategoryMapping", x => new { x.VocabularyID, x.VocabularyCategoryID });
                    table.ForeignKey(
                        name: "FK_VocabularyCategoryMapping_Vocabularies",
                        column: x => x.VocabularyID,
                        principalTable: "Vocabularies",
                        principalColumn: "VocabularyId");
                    table.ForeignKey(
                        name: "FK_VocabularyCategoryMapping_VocabularyCategories",
                        column: x => x.VocabularyCategoryID,
                        principalTable: "VocabularyCategories",
                        principalColumn: "VocabularyCategoryId");
                });

            migrationBuilder.CreateTable(
                name: "CourseLesson",
                columns: table => new
                {
                    CourseID = table.Column<int>(type: "int", nullable: false),
                    LessonID = table.Column<int>(type: "int", nullable: false),
                    OrderNo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseLesson", x => new { x.CourseID, x.LessonID });
                    table.ForeignKey(
                        name: "FK_CourseLesson_Course",
                        column: x => x.CourseID,
                        principalTable: "Course",
                        principalColumn: "CourseID");
                    table.ForeignKey(
                        name: "FK_CourseLesson_Lesson",
                        column: x => x.LessonID,
                        principalTable: "Lesson",
                        principalColumn: "LessonID");
                });

            migrationBuilder.CreateTable(
                name: "LessonQuestion",
                columns: table => new
                {
                    LessonID = table.Column<int>(type: "int", nullable: false),
                    QuestionID = table.Column<int>(type: "int", nullable: false),
                    OrderNo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonQuestion", x => new { x.LessonID, x.QuestionID });
                    table.ForeignKey(
                        name: "FK_LessonQuestion_Lesson",
                        column: x => x.LessonID,
                        principalTable: "Lesson",
                        principalColumn: "LessonID");
                    table.ForeignKey(
                        name: "FK_LessonQuestion_Question",
                        column: x => x.QuestionID,
                        principalTable: "Question",
                        principalColumn: "QuestionID");
                });

            migrationBuilder.CreateTable(
                name: "Academic_Result",
                columns: table => new
                {
                    AcademicResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CourseID = table.Column<int>(type: "int", nullable: false),
                    LessonID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TimeSpent = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Academic__D5B70E7EDCA5BC99", x => x.AcademicResultId);
                    table.ForeignKey(
                        name: "FK_AcademicResult_Course",
                        column: x => x.CourseID,
                        principalTable: "Course",
                        principalColumn: "CourseID");
                    table.ForeignKey(
                        name: "FK_AcademicResult_Lesson",
                        column: x => x.LessonID,
                        principalTable: "Lesson",
                        principalColumn: "LessonID");
                    table.ForeignKey(
                        name: "FK_AcademicResult_User",
                        column: x => x.UserID,
                        principalTable: "_USER",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    CertificateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CourseID = table.Column<int>(type: "int", nullable: false),
                    VerificationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Hash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    TransactionId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.CertificateID);
                    table.ForeignKey(
                        name: "FK_Certificates_Course",
                        column: x => x.CourseID,
                        principalTable: "Course",
                        principalColumn: "CourseID");
                    table.ForeignKey(
                        name: "FK_Certificates_User",
                        column: x => x.UserID,
                        principalTable: "_USER",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Conversation",
                columns: table => new
                {
                    ConversationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversation", x => x.ConversationID);
                    table.ForeignKey(
                        name: "FK_Conversation_Admin",
                        column: x => x.AdminID,
                        principalTable: "_USER",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Conversation_User",
                        column: x => x.UserID,
                        principalTable: "_USER",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Orders__C3905BCFF16FFEB6", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_Packages",
                        column: x => x.PackageId,
                        principalTable: "Package",
                        principalColumn: "PackageID");
                    table.ForeignKey(
                        name: "FK_Orders_PaymentMethods",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "PaymentMethodId");
                    table.ForeignKey(
                        name: "FK_Orders_Users",
                        column: x => x.UserId,
                        principalTable: "_USER",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    ReviewID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CourseID = table.Column<int>(type: "int", nullable: false),
                    LessonID = table.Column<int>(type: "int", nullable: true),
                    ReviewType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Review__74BC79AE7E4C6D85", x => x.ReviewID);
                    table.ForeignKey(
                        name: "FK_Review_Course",
                        column: x => x.CourseID,
                        principalTable: "Course",
                        principalColumn: "CourseID");
                    table.ForeignKey(
                        name: "FK_Review_Lesson",
                        column: x => x.LessonID,
                        principalTable: "Lesson",
                        principalColumn: "LessonID");
                    table.ForeignKey(
                        name: "FK_Review_User",
                        column: x => x.UserID,
                        principalTable: "_USER",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    TestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    Score = table.Column<double>(type: "float", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Tests__8CC331601C0B3DA4", x => x.TestId);
                    table.ForeignKey(
                        name: "FK__Tests__UserId__3CBF0154",
                        column: x => x.UserId,
                        principalTable: "_USER",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "UserPackageRegistration",
                columns: table => new
                {
                    PackageID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    ExpirationDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPackageRegistration", x => new { x.PackageID, x.UserID });
                    table.ForeignKey(
                        name: "FK_UPR_Package",
                        column: x => x.PackageID,
                        principalTable: "Package",
                        principalColumn: "PackageID");
                    table.ForeignKey(
                        name: "FK_UPR_User",
                        column: x => x.UserID,
                        principalTable: "_USER",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    MessageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationID = table.Column<int>(type: "int", nullable: false),
                    SenderID = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.MessageID);
                    table.ForeignKey(
                        name: "FK_Message_Conversation",
                        column: x => x.ConversationID,
                        principalTable: "Conversation",
                        principalColumn: "ConversationID");
                    table.ForeignKey(
                        name: "FK_Message_Sender",
                        column: x => x.SenderID,
                        principalTable: "_USER",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "TestQuestions",
                columns: table => new
                {
                    TestQuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TestQues__4C589E09C41704DF", x => x.TestQuestionId);
                    table.ForeignKey(
                        name: "FK__TestQuest__Quest__408F9238",
                        column: x => x.QuestionId,
                        principalTable: "Question",
                        principalColumn: "QuestionID");
                    table.ForeignKey(
                        name: "FK__TestQuest__TestI__3F9B6DFF",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "TestId");
                });

            migrationBuilder.CreateTable(
                name: "UserAnswers",
                columns: table => new
                {
                    UserAnswerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SelectedAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserAnsw__47CE237F19CFB3D2", x => x.UserAnswerId);
                    table.ForeignKey(
                        name: "FK__UserAnswe__Quest__4460231C",
                        column: x => x.QuestionId,
                        principalTable: "Question",
                        principalColumn: "QuestionID");
                    table.ForeignKey(
                        name: "FK__UserAnswe__TestI__436BFEE3",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "TestId");
                    table.ForeignKey(
                        name: "FK__UserAnswe__UserI__45544755",
                        column: x => x.UserId,
                        principalTable: "_USER",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateIndex(
                name: "IX__USER_RoleID",
                table: "_USER",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_Academic_Result_CourseID",
                table: "Academic_Result",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Academic_Result_LessonID",
                table: "Academic_Result",
                column: "LessonID");

            migrationBuilder.CreateIndex(
                name: "IX_Academic_Result_UserID",
                table: "Academic_Result",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CourseID",
                table: "Certificates",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_UserID",
                table: "Certificates",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "UQ_Certificates_VerificationCode",
                table: "Certificates",
                column: "VerificationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversation_AdminID_UserID",
                table: "Conversation",
                columns: new[] { "AdminID", "UserID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversation_UserID",
                table: "Conversation",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Course_CategoryID",
                table: "Course",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Course_LevelID",
                table: "Course",
                column: "LevelID");

            migrationBuilder.CreateIndex(
                name: "IX_Course_PackageID",
                table: "Course",
                column: "PackageID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseLesson_LessonID",
                table: "CourseLesson",
                column: "LessonID");

            migrationBuilder.CreateIndex(
                name: "UQ_CourseLesson_Course_OrderNo",
                table: "CourseLesson",
                columns: new[] { "CourseID", "OrderNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonQuestion_QuestionID",
                table: "LessonQuestion",
                column: "QuestionID");

            migrationBuilder.CreateIndex(
                name: "UQ_Lesson_Order",
                table: "LessonQuestion",
                columns: new[] { "LessonID", "OrderNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Message_ConversationID",
                table: "Message",
                column: "ConversationID");

            migrationBuilder.CreateIndex(
                name: "IX_Message_SenderID",
                table: "Message",
                column: "SenderID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PackageId",
                table: "Orders",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentMethodId",
                table: "Orders",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageInclusion_IncludedPackageID",
                table: "PackageInclusion",
                column: "IncludedPackageID");

            migrationBuilder.CreateIndex(
                name: "IX_Question_ContentTypeID",
                table: "Question",
                column: "ContentTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Question_QuestionLevelID",
                table: "Question",
                column: "QuestionLevelID");

            migrationBuilder.CreateIndex(
                name: "IX_Question_QuestionTypeId",
                table: "Question",
                column: "QuestionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Review_CourseID",
                table: "Review",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Review_LessonID",
                table: "Review",
                column: "LessonID");

            migrationBuilder.CreateIndex(
                name: "IX_Review_UserID",
                table: "Review",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_TestQuestions_QuestionId",
                table: "TestQuestions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestQuestions_TestId",
                table: "TestQuestions",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_UserId",
                table: "Tests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_QuestionId",
                table: "UserAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_TestId",
                table: "UserAnswers",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_UserId",
                table: "UserAnswers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPackageRegistration_UserID",
                table: "UserPackageRegistration",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyCategoryMapping_VocabularyCategoryID",
                table: "VocabularyCategoryMapping",
                column: "VocabularyCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyMeanings_VocabularyId",
                table: "VocabularyMeanings",
                column: "VocabularyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Academic_Result");

            migrationBuilder.DropTable(
                name: "Banner");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "CourseLesson");

            migrationBuilder.DropTable(
                name: "LessonQuestion");

            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "PackageInclusion");

            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropTable(
                name: "TestQuestions");

            migrationBuilder.DropTable(
                name: "UserAnswers");

            migrationBuilder.DropTable(
                name: "UserPackageRegistration");

            migrationBuilder.DropTable(
                name: "VocabularyCategoryMapping");

            migrationBuilder.DropTable(
                name: "VocabularyMeanings");

            migrationBuilder.DropTable(
                name: "Conversation");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.DropTable(
                name: "Lesson");

            migrationBuilder.DropTable(
                name: "Question");

            migrationBuilder.DropTable(
                name: "Tests");

            migrationBuilder.DropTable(
                name: "VocabularyCategories");

            migrationBuilder.DropTable(
                name: "Vocabularies");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "Level");

            migrationBuilder.DropTable(
                name: "Package");

            migrationBuilder.DropTable(
                name: "ContentType");

            migrationBuilder.DropTable(
                name: "QuestionLevel");

            migrationBuilder.DropTable(
                name: "QuestionType");

            migrationBuilder.DropTable(
                name: "_USER");

            migrationBuilder.DropTable(
                name: "ROLE");
        }
    }
}
