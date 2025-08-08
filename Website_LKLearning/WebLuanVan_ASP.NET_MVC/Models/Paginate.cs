namespace WebLuanVan_ASP.NET_MVC.Models
{
    public class Paginate
    {
        public int TotalItems { get;  private set; } // tổng số items
        public int PageSize { get; private set; } // số lượng items trên mỗi trang
        public int CurrentPage { get; private set; } // trang hiện tại
        public int TotalPages { get; private set; } // tổng số trang
        public int StartPage { get; private set; } // trang bắt đầu
        public int EndPage { get; private set; } // trang kết thúc
        public Paginate()
        {

        }
        public Paginate(int totalItems,int page, int pageSize = 10)
        {
            // làm tròn  lên số trang  ví dụ totalitems = 25, pageSize = 10 thì totalPages = 3

            int totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);
            int currentpage = page;
            int startPage = currentpage - 5;
            int endPage = currentpage + 4;
            if (startPage < 1)
            {
                endPage = endPage-(startPage-1);
                startPage = 1;
            }
            if (endPage>totalPages) // nếu số trang cuối lớn hơn tổng số trang
            {
                endPage = totalPages; // số trang cuối = số tổng trang
                if (endPage >10) // số trang cưới lớn hơn 10
                {
                    startPage = endPage - 9; // trang bắt đầu = trang cuối - 9
                }
               
            }
            TotalItems = totalItems;
            CurrentPage = currentpage;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;

        }

    }
}
