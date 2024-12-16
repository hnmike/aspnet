namespace Web_CuaHangCafe.Models
{
    public class CartItem
    {
        public int MaSp { get; set; }
        public string TenSp { get; set; }
        public string AnhSp { get; set; } = null!;
        public decimal? DonGia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien => DonGia.HasValue ? SoLuong * DonGia.Value : 0;
    }
}
