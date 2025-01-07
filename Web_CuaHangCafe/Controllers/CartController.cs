using Microsoft.AspNetCore.Mvc;
using Web_CuaHangCafe.Data;
using Web_CuaHangCafe.Models;
using Web_CuaHangCafe.Helpers;
using Microsoft.Extensions.Configuration;

namespace Web_CuaHangCafe.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public CartController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public List<CartItem> Carts
        {
            get
            {
                var data = HttpContext.Session.Get<List<CartItem>>("Cart");

                if (data == null)
                {
                    data = new List<CartItem>();
                }

                return data;
            }
        }

        public IActionResult Index()
        {
            var cartItems = HttpContext.Session.Get<List<CartItem>>("Cart");
            return View(cartItems ?? new List<CartItem>());
        }

        [HttpGet]
        public IActionResult Add(int id, int quantity = 1)
        {
            try
            {
                var myCart = Carts;
                var item = myCart.SingleOrDefault(p => p.MaSp == id);

                if (item == null)
                {
                    var hangHoa = _context.TbSanPhams.SingleOrDefault(p => p.MaSanPham == id);
                    if (hangHoa == null)
                    {
                        return NotFound();
                    }

                    item = new CartItem
                    {
                        MaSp = id,
                        TenSp = hangHoa.TenSanPham,
                        DonGia = hangHoa.GiaBan.Value,
                        SoLuong = quantity,
                        AnhSp = hangHoa.HinhAnh
                    };

                    myCart.Add(item);
                }
                else
                {
                    item.SoLuong += quantity;
                }

                HttpContext.Session.Set("Cart", myCart);

                // Chuyển hướng về trang giỏ hàng
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public IActionResult Update([FromBody] List<UpdateQuantityRequest> updates)
        {
            if (updates == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid request.");
            }

            var cartItems = HttpContext.Session.Get<List<CartItem>>("Cart");

            if (cartItems != null)
            {
                foreach (var update in updates)
                {
                    var cartItemToUpdate = cartItems.Find(item => item.MaSp == update.ProductId);

                    if (cartItemToUpdate != null)
                    {
                        cartItemToUpdate.SoLuong = update.Quantity;
                    }
                }

                HttpContext.Session.Set("Cart", cartItems);

                decimal? totalAmount = 0;
                foreach (var item in cartItems)
                {
                    totalAmount += item.SoLuong * item.DonGia;
                }

                return Json(new { success = true, message = "Số lượng đã được cập nhật.", totalAmount = totalAmount, cartItems = cartItems });
            }

            return BadRequest("Invalid cart.");
        }

        public class UpdateQuantityRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        [HttpPost]
        public IActionResult Remove(int maSp)
        {
            try
            {
                var cartItems = HttpContext.Session.Get<List<CartItem>>("Cart");

                if (cartItems != null)
                {
                    var productToRemove = cartItems.SingleOrDefault(item => item.MaSp == maSp);

                    if (productToRemove != null)
                    {
                        cartItems.Remove(productToRemove);

                        HttpContext.Session.Set("Cart", cartItems);

                        decimal? totalAmount = 0;

                        foreach (var item in cartItems)
                        {
                            totalAmount += item.SoLuong * item.DonGia;
                        }

                        return Json(new { success = true, message = "Đã xoá sản phẩm.", totalAmount = totalAmount, cartItems = cartItems });
                    }
                    else
                    {
                        Console.WriteLine(maSp);
                        return Json(new { success = false, message = "Không có sản phẩm." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Giỏ hàng rỗng." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public IActionResult Checkout()
        {
            var cartItems = HttpContext.Session.Get<List<CartItem>>("Cart");


            if (cartItems != null && cartItems.Any())
            {
                decimal? tongTien = cartItems.Sum(p => p.DonGia * p.SoLuong);
                string totalCart = tongTien.Value.ToString("n0");
                ViewData["total"] = totalCart;
                return View(Carts);
            }
            else
            {
                return RedirectToAction("index", "home");
            }
        }

        public IActionResult Confirmation(string customerName, string phoneNumber, string address)
        {
            var cartItems = HttpContext.Session.Get<List<CartItem>>("Cart");

            Random random = new Random();
            Guid orderId = Guid.NewGuid();
            Guid customerId = Guid.NewGuid();

            if (cartItems != null && cartItems.Any())
            {
                var custormer = _context.TbKhachHangs.FirstOrDefault(x => x.SdtkhachHang.Equals(phoneNumber));

                if (custormer == null)
                {
                    var _customer = new TbKhachHang
                    {
                        Id = customerId,
                        TenKhachHang = customerName,
                        SdtkhachHang = phoneNumber,
                        DiaChi = address
                    };

                    _context.TbKhachHangs.Add(_customer);
                    _context.SaveChanges();

                    var order = new TbHoaDonBan
                    {
                        MaHoaDon = orderId,
                        SoHoaDon = random.Next(1, 100).ToString(),
                        NgayBan = DateTime.Now,
                        TongTien = cartItems.Sum(p => p.DonGia * p.SoLuong),
                        CustomerId = customerId
                    };

                    _context.TbHoaDonBans.Add(order);
                    _context.SaveChanges();
                }
                else
                {
                    custormer.TenKhachHang = customerName;
                    custormer.DiaChi = address;
                    _context.SaveChanges();

                    var order = new TbHoaDonBan
                    {
                        MaHoaDon = orderId,
                        SoHoaDon = random.Next(1, 100).ToString(),
                        NgayBan = DateTime.Now,
                        TongTien = cartItems.Sum(p => p.DonGia * p.SoLuong),
                        CustomerId = custormer.Id
                    };

                    _context.TbHoaDonBans.Add(order);
                    _context.SaveChanges();
                }

                foreach (var cartItem in cartItems)
                {
                    var orderDetails = new TbChiTietHoaDonBan
                    {
                        MaHoaDon = orderId,
                        MaSanPham = cartItem.MaSp,
                        GiaBan = cartItem.DonGia,
                        GiamGia = 0,
                        SoLuong = cartItem.SoLuong,
                        ThanhTien = cartItem.ThanhTien
                    };

                    _context.TbChiTietHoaDonBans.Add(orderDetails);
                    _context.SaveChanges();
                }

                HttpContext.Session.Remove("Cart");

                return RedirectToAction("success");
            }
            else
            {
                return RedirectToAction("index", "home");
            }
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult GetCartCount()
        {
            var cartItems = HttpContext.Session.Get<List<CartItem>>("Cart");
            return Json(cartItems?.Count ?? 0);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int productId, int quantity)
        {
            try
            {
                var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();

                if (cart.Any(x => x.MaSp == productId))
                {
                    // Nếu sản phẩm đã có trong giỏ hàng, tăng số lượng
                    var cartItem = cart.First(x => x.MaSp == productId);
                    cartItem.SoLuong += quantity;
                }
                else
                {
                    // Nếu sản phẩm chưa có trong giỏ hàng, thêm mới
                    var product = _context.TbSanPhams.Find(productId);
                    if (product != null)
                    {
                        cart.Add(new CartItem
                        {
                            MaSp = product.MaSanPham,
                            TenSp = product.TenSanPham,
                            AnhSp = product.HinhAnh,
                            DonGia = product.GiaBan,
                            SoLuong = quantity
                        });
                    }
                }

                HttpContext.Session.Set("Cart", cart);

                return Json(new { success = true, message = "Đã thêm vào giỏ hàng", cartCount = cart.Count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CreatePayment(string paymentMethod, string customerName, string phoneNumber, string address)
        {
            try
            {
                var cartItems = HttpContext.Session.Get<List<CartItem>>("Cart");
                if (cartItems == null || !cartItems.Any())
                    return RedirectToAction("index", "home");

                if (paymentMethod == "cod")
                {
                    return RedirectToAction("Confirmation", new { customerName, phoneNumber, address });
                }
                else if (paymentMethod == "vnpay")
                {
                    var vnpayConfig = _configuration.GetSection("Vnpay").Get<VnPayConfig>();
                    var vnpay = new VnPayLibrary();

                    var amount = cartItems.Sum(p => p.DonGia * p.SoLuong).Value;
                    var tick = DateTime.Now.Ticks.ToString();

                    // Cấu hình thông tin gửi đến VNPAY
                    vnpay.AddRequestData("vnp_Version", "2.1.0");
                    vnpay.AddRequestData("vnp_Command", "pay");
                    vnpay.AddRequestData("vnp_TmnCode", vnpayConfig.TmnCode);
                    vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString()); // Nhân 100 và chuyển về long
                    vnpay.AddRequestData("vnp_BankCode", ""); // Để trống để hiện tất cả ngân hàng
                    vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    vnpay.AddRequestData("vnp_CurrCode", "VND");
                    vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
                    vnpay.AddRequestData("vnp_Locale", "vn");
                    vnpay.AddRequestData("vnp_OrderInfo", $"{customerName} thanh toan don hang {tick}");
                    vnpay.AddRequestData("vnp_OrderType", "other");
                    vnpay.AddRequestData("vnp_ReturnUrl", vnpayConfig.ReturnUrl);
                    vnpay.AddRequestData("vnp_TxnRef", DateTime.Now.ToString("yyyyMMddHHmmss")); // Mã tham chiếu

                    var paymentUrl = vnpay.CreateRequestUrl(vnpayConfig.PaymentUrl, vnpayConfig.HashSecret);
                    return Redirect(paymentUrl);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult PaymentCallback()
        {
            var vnpayData = HttpContext.Request.Query;
            var vnpay = new VnPayLibrary();
            var vnpayConfig = _configuration.GetSection("Vnpay").Get<VnPayConfig>();

            foreach (var (key, value) in vnpayData)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            var orderId = vnpay.GetResponseData("vnp_TxnRef");
            var vnpayTranId = vnpay.GetResponseData("vnp_TransactionNo");
            var vnpResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnpSecureHash = vnpayData.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;
            var checkSignature = vnpay.ValidateSignature(vnpSecureHash, vnpayConfig.HashSecret);

            if (checkSignature)
            {
                if (vnpResponseCode == "00")
                {
                    // Thanh toán thành công
                    return RedirectToAction("Success");
                }
            }

            // Thanh toán thất bại
            TempData["ErrorMessage"] = "Thanh toán không thành công";
            return RedirectToAction("Index");
        }
    }
}
