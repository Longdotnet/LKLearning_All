using CommunityToolkit.Mvvm.ComponentModel;
using mauiluanvantotnghiep.Models;

namespace mauiluanvantotnghiep.ViewModels
{
    [QueryProperty("Transaction", "Transaction")]
    public partial class TransactionDetailsViewModel : ObservableObject
    {
        [ObservableProperty] Orders transaction;
    }

}
