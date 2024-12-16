using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Web_CuaHangCafe.Data;
using Web_CuaHangCafe.Models;
using Web_CuaHangCafe.Models.Authentication;
using Web_CuaHangCafe.ViewModels;
using X.PagedList;

namespace Web_CuaHangCafe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("HomeAdmin")]
    public class HomeAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        IWebHostEnvironment hostEnvironment;

        public HomeAdminController(ApplicationDbContext context, IWebHostEnvironment hc)
        {
            _context = context;
            hostEnvironment = hc;
        }

        [Route("")]
        [Authentication]
        public IActionResult Index(int? page)
        {
            int pageSize = 30;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            var listItem = (from product in _context.TbSanPhams
                            join type in _context.TbNhomSanPhams on product.MaNhomSp equals type.MaNhomSp
                            orderby product.MaSanPham
                            select new ProductViewModel
                            {
                                MaSanPham = product.MaSanPham,
                                TenSanPham = product.TenSanPham,
                                GiaBan = product.GiaBan,
                                MoTa = product.MoTa,
                                HinhAnh = product.HinhAnh,
                                GhiChu = product.GhiChu,
                                LoaiSanPham = type.TenNhomSp
                            }).ToList();

            PagedList<ProductViewModel> pagedListItem = new PagedList<ProductViewModel>(listItem, pageNumber, pageSize);

            return View(pagedListItem);
        }

        [Route("Search")]
        [Authentication]
        [HttpGet]
        public IActionResult Search(int? page, string search)
        {
            int pageSize = 30;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            search = search.ToLower();
            ViewBag.search = search;

            var listItem = _context.TbSanPhams.AsNoTracking().Where(x => x.TenSanPham.ToLower().Contains(search)).OrderBy(x => x.MaSanPham).ToList();
            PagedList<TbSanPham> pagedListItem = new PagedList<TbSanPham>(listItem, pageNumber, pageSize);

            return View(pagedListItem);
        }

        [Route("Create")]
        [Authentication]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.MaNhomSp = new SelectList(_context.TbNhomSanPhams.ToList(), "MaNhomSp", "TenNhomSp");

            return View();
        }

        [Route("Create")]
        [Authentication]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateSanPhamDto sanPham, IFormFile imageFile)
        {
            string filePath = "";
            string uniqueFileName = "";
            if (sanPham.HinhAnh != null && sanPham.HinhAnh.Length > 0)
            {
                string uploadFolder = Path.Combine(Path.Combine(hostEnvironment.WebRootPath, "img"), "products");
                string cleanFileName = Path.GetFileNameWithoutExtension(sanPham.HinhAnh.FileName).Replace(" ", "-");
                string fileExtension = Path.GetExtension(sanPham.HinhAnh.FileName);
                uniqueFileName = Guid.NewGuid().ToString() + "_" + cleanFileName + fileExtension;
                filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    sanPham.HinhAnh.CopyTo(stream);
                }
            }
            TbSanPham product = new TbSanPham()
            {
                TenSanPham = sanPham.TenSanPham,
                MaNhomSp = sanPham.MaNhomSp,
                MaSanPham = sanPham.MaSanPham,
                GiaBan = sanPham.GiaBan,
                MoTa = sanPham.MoTa,
                GhiChu = sanPham.GhiChu,
                HinhAnh = uniqueFileName,

            };
            _context.TbSanPhams.Add(product);
            _context.SaveChanges();
            TempData["Message"] = "Thêm sản phẩm thành công";

            return RedirectToAction("Index", "HomeAdmin");
        }

        [Route("Details")]
        [Authentication]
        [HttpGet]
        public IActionResult Details(int id, string name)
        {
            var productItem = (from product in _context.TbSanPhams
                               join type in _context.TbNhomSanPhams on product.MaNhomSp equals type.MaNhomSp
                               where product.MaSanPham == id
                               select new ProductViewModel
                               {
                                   MaSanPham = product.MaSanPham,
                                   TenSanPham = product.TenSanPham,
                                   GiaBan = product.GiaBan,
                                   MoTa = product.MoTa,
                                   HinhAnh = product.HinhAnh,
                                   GhiChu = product.GhiChu,
                                   LoaiSanPham = type.TenNhomSp
                               }).SingleOrDefault();

            ViewBag.name = name;

            return View(productItem);
        }

        [Route("Edit")]
        [Authentication]
        [HttpGet]
        public IActionResult Edit(int id, string name)
        {
            var sanPham = _context.TbSanPhams.Select(x => new CreateProductViewModel()
            {
                MaSanPham = x.MaSanPham,
                TenSanPham = x.TenSanPham,
                GiaBan = x.GiaBan,
                MoTa = x.MoTa,
                HinhAnhLink = x.HinhAnh,
                GhiChu = x.GhiChu,
                MaLoaiSanPham = x.MaNhomSp
            });

            ViewBag.MaNhomSp = new SelectList(_context.TbNhomSanPhams.ToList(), "MaNhomSp", "TenNhomSp");
            ViewBag.name = name;

            return View(sanPham);
        }

        [Route("Edit")]
        [Authentication]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CreateProductViewModel createProduct)
        {
            string fileName = "";

            if (createProduct.HinhAnh != null)
            {
                string uploadFolder = Path.Combine(Path.Combine(hostEnvironment.WebRootPath, "img"), "products");
                string cleanFileName = Path.GetFileNameWithoutExtension(createProduct.HinhAnh.FileName).Replace(" ", "-");
                string fileExtension = Path.GetExtension(createProduct.HinhAnh.FileName);
                fileName = Guid.NewGuid().ToString() + "_" + cleanFileName + fileExtension;
                string filePath = Path.Combine(uploadFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    createProduct.HinhAnh.CopyTo(stream);
                }
            }

            var product = new TbSanPham
            {
                MaSanPham = createProduct.MaSanPham,
                TenSanPham = createProduct.TenSanPham,
                GiaBan = createProduct.GiaBan,
                MoTa = createProduct.MoTa,
                HinhAnh = fileName,
                GhiChu = createProduct.GhiChu,
                MaNhomSp = createProduct.MaLoaiSanPham
            };

            _context.Entry(product).State = EntityState.Modified;
            _context.SaveChanges();
            TempData["Message"] = "Sửa sản phẩm thành công";
            return RedirectToAction("Index", "HomeAdmin");
        }

        [Route("Delete")]
        [Authentication]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            TempData["Message"] = "";
            var chiTietHoaDon = _context.TbChiTietHoaDonBans.Where(x => x.MaSanPham == id).ToList();

            if (chiTietHoaDon.Count() > 0)
            {
                TempData["Message"] = "Không xoá được sản phẩm";

                return RedirectToAction("Index", "HomeAdmin");
            }

            _context.Remove(_context.TbSanPhams.Find(id));
            _context.SaveChanges();

            TempData["Message"] = "Sản phẩm đã được xoá";

            return RedirectToAction("Index", "HomeAdmin");
        }
    }
}
