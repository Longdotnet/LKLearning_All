using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WEBAPI.Models;

public partial class LuanvantienganhContext : DbContext
{
    public LuanvantienganhContext()
    {
    }

    public LuanvantienganhContext(DbContextOptions<LuanvantienganhContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcademicResult> AcademicResults { get; set; }

    public virtual DbSet<Avatar> Avatars { get; set; }

    public virtual DbSet<Banner> Banners { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<CertificateType> CertificateTypes { get; set; }

    public virtual DbSet<ContentType> ContentTypes { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseLesson> CourseLessons { get; set; }

    public virtual DbSet<ExamSet> ExamSets { get; set; }

    public virtual DbSet<ExamSetQuestion> ExamSetQuestions { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonQuestion> LessonQuestions { get; set; }

    public virtual DbSet<Level> Levels { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Package> Packages { get; set; }

    public virtual DbSet<PackageInclusion> PackageInclusions { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionLevel> QuestionLevels { get; set; }

    public virtual DbSet<QuestionType> QuestionTypes { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserExamHistory> UserExamHistories { get; set; }

    public virtual DbSet<UserPackageRegistration> UserPackageRegistrations { get; set; }

    public virtual DbSet<Vocabulary> Vocabularies { get; set; }

    public virtual DbSet<VocabularyCategory> VocabularyCategories { get; set; }

    public virtual DbSet<VocabularyCategoryMapping> VocabularyCategoryMappings { get; set; }

    public virtual DbSet<VocabularyMeaning> VocabularyMeanings { get; set; }

   

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcademicResult>(entity =>
        {
            entity.HasKey(e => e.AcademicResultId).HasName("PK__Academic__D5B70E7EDCA5BC99");

            entity.ToTable("Academic_Result");

            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LessonId).HasColumnName("LessonID");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TimeSpent).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Course).WithMany(p => p.AcademicResults)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AcademicResult_Course");

            entity.HasOne(d => d.Lesson).WithMany(p => p.AcademicResults)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AcademicResult_Lesson");

            entity.HasOne(d => d.User).WithMany(p => p.AcademicResults)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AcademicResult_User");
        });

        modelBuilder.Entity<Avatar>(entity =>
        {
            entity.HasKey(e => e.AvatarId).HasName("PK__Avatars__4811D66A54EA044F");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UrlPath).HasMaxLength(500);
        });

        modelBuilder.Entity<Banner>(entity =>
        {
            entity.HasKey(e => e.BannerId).HasName("PK__Banner__32E86AD1856F050A");

            entity.ToTable("Banner");

            entity.Property(e => e.BannerImageUrl).HasMaxLength(500);
            entity.Property(e => e.BannerSubtitle).HasMaxLength(255);
            entity.Property(e => e.BannerTitle).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LinkUrl).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A2B83CE4368");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasIndex(e => e.VerificationCode, "UQ_Certificates_VerificationCode").IsUnique();

            entity.Property(e => e.CertificateId).HasColumnName("CertificateID");
            entity.Property(e => e.CertificateTypeId).HasColumnName("CertificateTypeID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ExpirationDate).HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.VerificationCode).HasMaxLength(50);

            entity.HasOne(d => d.CertificateType).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.CertificateTypeId)
                .HasConstraintName("FK_Certificate_CertificateTypes");

            entity.HasOne(d => d.Course).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Certificates_Course");

            entity.HasOne(d => d.User).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Certificates_User");
        });

        modelBuilder.Entity<CertificateType>(entity =>
        {
            entity.HasKey(e => e.CertificateTypeId).HasName("PK__Certific__78F0E8D923A04FC7");

            entity.Property(e => e.CertificateTypeId).HasColumnName("CertificateTypeID");
            entity.Property(e => e.TypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<ContentType>(entity =>
        {
            entity.ToTable("ContentType");

            entity.Property(e => e.ContentTypeId).HasColumnName("ContentTypeID");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("Conversation");

            entity.Property(e => e.ConversationID).HasColumnName("ConversationID");
            entity.Property(e => e.AdminID).HasColumnName("AdminID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UserID).HasColumnName("UserID");

            entity.HasOne(d => d.Admin).WithMany(p => p.ConversationAdmins)
                .HasForeignKey(d => d.AdminID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversation_Admin");

            entity.HasOne(d => d.User).WithMany(p => p.ConversationUsers)
                .HasForeignKey(d => d.UserID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversation_User");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Course__C92D71872CDF6125");

            entity.ToTable("Course");

            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CertificateDurationDays).HasColumnName("certificate_duration_days");
            entity.Property(e => e.CourseName).HasMaxLength(100);
            entity.Property(e => e.Img)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("img");
            entity.Property(e => e.LevelId).HasColumnName("LevelID");
            entity.Property(e => e.PackageId).HasColumnName("PackageID");

            entity.HasOne(d => d.Category).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Course_Category");

            entity.HasOne(d => d.Level).WithMany(p => p.Courses)
                .HasForeignKey(d => d.LevelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Course_Level");

            entity.HasOne(d => d.Package).WithMany(p => p.Courses)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("FK_Course_Package");
        });

        modelBuilder.Entity<CourseLesson>(entity =>
        {
            entity.HasKey(e => new { e.CourseId, e.LessonId });

            entity.ToTable("CourseLesson");

            entity.HasIndex(e => new { e.CourseId, e.OrderNo }, "UQ_CourseLesson_Course_OrderNo").IsUnique();

            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.LessonId).HasColumnName("LessonID");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseLessons)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CourseLesson_Course");

            entity.HasOne(d => d.Lesson).WithMany(p => p.CourseLessons)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CourseLesson_Lesson");
        });

        modelBuilder.Entity<ExamSet>(entity =>
        {
            entity.HasKey(e => e.ExamSetId).HasName("PK__ExamSet__03D28B85A4BABC64");

            entity.ToTable("ExamSet");

            entity.Property(e => e.ExamSetId).HasColumnName("ExamSetID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.PassingScore).HasColumnType("decimal(4, 1)");

            entity.HasOne(d => d.Course).WithMany(p => p.ExamSets)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExamSet_Course");
        });

        modelBuilder.Entity<ExamSetQuestion>(entity =>
        {
            entity.HasKey(e => new { e.ExamSetId, e.QuestionId }).HasName("PK__ExamSetQ__D30E8D7D25F1AB52");

            entity.ToTable("ExamSetQuestion");

            entity.Property(e => e.ExamSetId).HasColumnName("ExamSetID");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.QuestionScore).HasColumnType("decimal(4, 2)");

            entity.HasOne(d => d.ExamSet).WithMany(p => p.ExamSetQuestions)
                .HasForeignKey(d => d.ExamSetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ESQ_ExamSet");

            entity.HasOne(d => d.Question).WithMany(p => p.ExamSetQuestions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ESQ_Question");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PK__Lesson__B084ACB04C392F71");

            entity.ToTable("Lesson");

            entity.Property(e => e.LessonId).HasColumnName("LessonID");
            entity.Property(e => e.IsActivate).HasDefaultValue(true);
            entity.Property(e => e.LessonTitle).HasMaxLength(255);
        });

        modelBuilder.Entity<LessonQuestion>(entity =>
        {
            entity.HasKey(e => new { e.LessonId, e.QuestionId });

            entity.ToTable("LessonQuestion");

            entity.HasIndex(e => new { e.LessonId, e.OrderNo }, "UQ_Lesson_Order").IsUnique();

            entity.Property(e => e.LessonId).HasColumnName("LessonID");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonQuestions)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LessonQuestion_Lesson");

            entity.HasOne(d => d.Question).WithMany(p => p.LessonQuestions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LessonQuestion_Question");
        });

        modelBuilder.Entity<Level>(entity =>
        {
            entity.HasKey(e => e.LevelId).HasName("PK__Level__09F03C06643F612B");

            entity.ToTable("Level");

            entity.Property(e => e.LevelId).HasColumnName("LevelID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.LevelName).HasMaxLength(50);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Message");

            entity.Property(e => e.MessageID).HasColumnName("MessageID");
            entity.Property(e => e.ConversationID).HasColumnName("ConversationID");
            entity.Property(e => e.SenderID).HasColumnName("SenderID");
            entity.Property(e => e.SentAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Message_Conversation");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Message_Sender");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCFF16FFEB6");

            entity.Property(e => e.OrderId).HasMaxLength(50);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TransactionId).HasMaxLength(100);

            entity.HasOne(d => d.Package).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Packages");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_PaymentMethods");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Users");
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.PackageId).HasName("PK__Package__322035EC4E16E07A");

            entity.ToTable("Package");

            entity.Property(e => e.PackageId).HasColumnName("PackageID");
            entity.Property(e => e.PackageName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UrlImage).HasColumnName("urlImage");
        });

        modelBuilder.Entity<PackageInclusion>(entity =>
        {
            entity.HasKey(e => new { e.ParentPackageId, e.IncludedPackageId });

            entity.ToTable("PackageInclusion");

            entity.Property(e => e.ParentPackageId).HasColumnName("ParentPackageID");
            entity.Property(e => e.IncludedPackageId).HasColumnName("IncludedPackageID");
            entity.Property(e => e.DateAdd).HasColumnType("datetime");

            entity.HasOne(d => d.IncludedPackage).WithMany(p => p.PackageInclusionIncludedPackages)
                .HasForeignKey(d => d.IncludedPackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PackageInclusion_IncludedPackage");

            entity.HasOne(d => d.ParentPackage).WithMany(p => p.PackageInclusionParentPackages)
                .HasForeignKey(d => d.ParentPackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PackageInclusion_ParentPackage");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PK__PaymentM__DC31C1D32CC7DAB2");

            entity.Property(e => e.Logo).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06F8CC0A35A91");

            entity.ToTable("Question");

            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.AudioUrl).HasMaxLength(255);
            entity.Property(e => e.ContentTypeId).HasColumnName("ContentTypeID");
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.QuestionLevelId).HasColumnName("QuestionLevelID");

            entity.HasOne(d => d.ContentType).WithMany(p => p.Questions)
                .HasForeignKey(d => d.ContentTypeId)
                .HasConstraintName("FK_Question_ContentType");

            entity.HasOne(d => d.QuestionLevel).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuestionLevelId)
                .HasConstraintName("FK_Question_QuestionLevel");

            entity.HasOne(d => d.QuestionType).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuestionTypeId)
                .HasConstraintName("FK_Question_QuestionType");
        });

        modelBuilder.Entity<QuestionLevel>(entity =>
        {
            entity.HasKey(e => e.QuestionLevelId).HasName("PK__Question__4DCE689F3DD12B96");

            entity.ToTable("QuestionLevel");

            entity.Property(e => e.QuestionLevelId)
                .ValueGeneratedNever()
                .HasColumnName("QuestionLevelID");
            entity.Property(e => e.QuestionName).HasMaxLength(50);
        });

        modelBuilder.Entity<QuestionType>(entity =>
        {
            entity.HasKey(e => e.QuestionTypeId).HasName("PK__Question__7EDFF9313CB97AE0");

            entity.ToTable("QuestionType");

            entity.Property(e => e.TypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Review__74BC79AE7E4C6D85");

            entity.ToTable("Review");

            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LessonId).HasColumnName("LessonID");
            entity.Property(e => e.ReviewType).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Course).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Review_Course");

            entity.HasOne(d => d.Lesson).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.LessonId)
                .HasConstraintName("FK_Review_Lesson");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Review_User");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__ROLE__8AFACE3A263754CA");

            entity.ToTable("ROLE");

            entity.Property(e => e.RoleId)
                .ValueGeneratedNever()
                .HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK___USER__1788CCAC82B0D5D2");

            entity.ToTable("_USER");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Fullname).HasMaxLength(255);
            entity.Property(e => e.LastLoginDate)
                .HasColumnType("datetime")
                .HasColumnName("lastLoginDate");
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.HasOne(d => d.Avatar).WithMany(p => p.Users)
                .HasForeignKey(d => d.AvatarId)
                .HasConstraintName("FK_Users_Avatars");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK___USER__RoleID__398D8EEE");
        });

        modelBuilder.Entity<UserExamHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__UserExam__4D7B4ADDC68AF2DC");

            entity.ToTable("UserExamHistory");

            entity.Property(e => e.HistoryId).HasColumnName("HistoryID");
            entity.Property(e => e.ExamSetId).HasColumnName("ExamSetID");
            entity.Property(e => e.TakenAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalScore).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.ExamSet).WithMany(p => p.UserExamHistories)
                .HasForeignKey(d => d.ExamSetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExamHistory_ExamSet");

            entity.HasOne(d => d.User).WithMany(p => p.UserExamHistories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExamHistory_User");
        });

        modelBuilder.Entity<UserPackageRegistration>(entity =>
        {
            entity.HasKey(e => new { e.PackageId, e.UserId });

            entity.ToTable("UserPackageRegistration");

            entity.Property(e => e.PackageId).HasColumnName("PackageID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.ExpirationDate).HasColumnType("datetime");
            entity.Property(e => e.RegistrationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Package).WithMany(p => p.UserPackageRegistrations)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UPR_Package");

            entity.HasOne(d => d.User).WithMany(p => p.UserPackageRegistrations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UPR_User");
        });

        modelBuilder.Entity<Vocabulary>(entity =>
        {
            entity.HasKey(e => e.VocabularyId).HasName("PK__Vocabula__927406BFDB159057");

            entity.Property(e => e.AudioUrlUk)
                .HasMaxLength(200)
                .HasColumnName("AudioUrlUK");
            entity.Property(e => e.AudioUrlUs)
                .HasMaxLength(200)
                .HasColumnName("AudioUrlUS");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Pronunciation).HasMaxLength(50);
            entity.Property(e => e.Word).HasMaxLength(50);
        });

        modelBuilder.Entity<VocabularyCategory>(entity =>
        {
            entity.HasKey(e => e.VocabularyCategoryId).HasName("PK__Vocabula__22F98D85DE0C00E7");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UrlImage)
                .HasMaxLength(500)
                .HasColumnName("urlImage");
            entity.Property(e => e.VocabularyCategoryDescription).HasMaxLength(200);
            entity.Property(e => e.VocabularyCategoryName).HasMaxLength(50);
        });

        modelBuilder.Entity<VocabularyCategoryMapping>(entity =>
        {
            entity.HasKey(e => new { e.VocabularyId, e.VocabularyCategoryId });

            entity.ToTable("VocabularyCategoryMapping");

            entity.Property(e => e.VocabularyId).HasColumnName("VocabularyID");
            entity.Property(e => e.VocabularyCategoryId).HasColumnName("VocabularyCategoryID");
            entity.Property(e => e.DateAdded).HasColumnType("datetime");

            entity.HasOne(d => d.VocabularyCategory).WithMany(p => p.VocabularyCategoryMappings)
                .HasForeignKey(d => d.VocabularyCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VocabularyCategoryMapping_VocabularyCategories");

            entity.HasOne(d => d.Vocabulary).WithMany(p => p.VocabularyCategoryMappings)
                .HasForeignKey(d => d.VocabularyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VocabularyCategoryMapping_Vocabularies");
        });

        modelBuilder.Entity<VocabularyMeaning>(entity =>
        {
            entity.HasKey(e => e.VocabularyMeaningId).HasName("PK__Vocabula__01FBC49325399152");

            entity.Property(e => e.PartOfSpeech).HasMaxLength(50);
            entity.Property(e => e.TranslatedExampleSentence).HasMaxLength(200);

            entity.HasOne(d => d.Vocabulary).WithMany(p => p.VocabularyMeanings)
                .HasForeignKey(d => d.VocabularyId)
                .HasConstraintName("FK__VocabMeaning__Vocab__11216507");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
