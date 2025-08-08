namespace WEBAPI.Services.ModelsVnPay
{
    public class PaymentInformationModel
    {
        public string OrderID { get; set; }
        public double Amount { get; set; }
        public string OrderDescription { get; set; }

        // Thêm property cho callback động
        public string? ReturnUrl { get; set; }
        public string? vnp_ReturnUrl { get; set; }
    }
}
