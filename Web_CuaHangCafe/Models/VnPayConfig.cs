public class VnPayConfig
{
    public string Version { get; set; } = "2.1.0";
    public string TmnCode { get; set; }
    public string HashSecret { get; set; }
    public string PaymentUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    public string ReturnUrl { get; set; }
}