using mauiluanvantotnghiep.Views.CoursesPage;
using mauiluanvantotnghiep.Views.ExamPage;
using mauiluanvantotnghiep.Views.ForgotPasswordPage;
using mauiluanvantotnghiep.Views.LessonPage;
using mauiluanvantotnghiep.Views.PaymentPage;
using mauiluanvantotnghiep.Views.QuestionPage;
using mauiluanvantotnghiep.Views.ReportPage;
using mauiluanvantotnghiep.Views.ReviewPage;
using mauiluanvantotnghiep.Views.TransactionPage;
using mauiluanvantotnghiep.Views.VocabularyPage;
using mauiluanvantotnghiep.Views.ChatDashboardPage;
using mauiluanvantotnghiep.Views.StoryPage;           // ✅ THÊM USING CHO STORYPAGE
using mauiluanvantotnghiep.Views.UserProfilePage;     // ✅ THÊM USING CHO USERPROFILEPAGE
using mauiluanvantotnghiep.Views.CertificatesPage;    // ✅ THÊM USING CHO CERTIFICATESPAGE

namespace mauiluanvantotnghiep
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Đăng ký route cho CoursesDetailPage
            Routing.RegisterRoute("coursesdetailpage", typeof(CoursesDetailPage));

            // Đăng ký route cho AllCoursesPage
            Routing.RegisterRoute("allcoursespage", typeof(AllCoursesPage));

            Routing.RegisterRoute("courselessonpage", typeof(LessonPage));

            Routing.RegisterRoute("questionpage", typeof(QuestionPage));

            Routing.RegisterRoute(nameof(TransactionDetailsPage), typeof(TransactionDetailsPage));

            Routing.RegisterRoute(nameof(PaymentgatewayPage), typeof(PaymentgatewayPage));

            Routing.RegisterRoute("reviewdetailpage", typeof(ReviewDetailPage));

            Routing.RegisterRoute("addreviewpage", typeof(AddReviewPage));

            Routing.RegisterRoute("reportpage", typeof(ReportPage));

            Routing.RegisterRoute("vocabularydetailpage", typeof(VocabularyDetailPage));

            Routing.RegisterRoute("categoryvocabularypage", typeof(CategoryVocabularyPage));

            Routing.RegisterRoute("flashcardpage", typeof(FlashcardPage));

            Routing.RegisterRoute("lessondetailpage", typeof(LessonDetailPage));
           
            Routing.RegisterRoute("exampage", typeof(ExamPage));

            Routing.RegisterRoute("examquestionpage", typeof(ExamQuestionPage));

            // Đăng ký routes cho relative navigation
            Routing.RegisterRoute("categoryvocabularylistpage", typeof(CategoryVocabularyListPage));

            // Đăng ký route cho ChatDashboardPage
            Routing.RegisterRoute("chatdashboardpage", typeof(ChatDashboardPage));

            // ✅ ĐĂNG KÝ ROUTES CHO UTILITY PAGES
            Routing.RegisterRoute("storypage", typeof(StoryPage));
            Routing.RegisterRoute("userprofilepage", typeof(UserProfilePage));
            Routing.RegisterRoute("certificatespage", typeof(CertificatesPage));
            Routing.RegisterRoute("certificatedetailpage", typeof(CertificateDetailPage));
            
            // ✅ ĐĂNG KÝ ROUTE CHO FORGOT PASSWORD PAGE
            Routing.RegisterRoute("forgotpasswordpage", typeof(ForgotPasswordPage));

            Routing.RegisterRoute("allnewwordspage", typeof(AllNewWordsPage));
        }
    }
}
