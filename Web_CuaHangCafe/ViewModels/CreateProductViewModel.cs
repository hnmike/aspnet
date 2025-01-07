namespace Web_CuaHangCafe.ViewModels
{
    public class CreateProductViewModel
    {
        public int MaSanPham { get; set; }

        public string TenSanPham { get; set; } = null!;

        public decimal? GiaBan { get; set; }

        public string? MoTa { get; set; }

        public IFormFile? HinhAnh { get; set; }
        public string? HinhAnhLink { get; set; }

        public string? GhiChu { get; set; }

        public int MaLoaiSanPham { get; set; }
    }
}
