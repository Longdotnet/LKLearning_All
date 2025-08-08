using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
   
    // Mỗi nhóm: header Title (vd "Tháng 06/2025") + list transactions
    public class TransactionGroup : ObservableCollection<Orders>
    {
        public string Title { get; }
        public TransactionGroup(string title) => Title = title;
    }
}
