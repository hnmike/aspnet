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
            //string fileName = "";

            //if (createProduct.HinhAnh != null)
            //{
            //    string uploadFolder = Path.Combine(Path.Combine(hostEnvironment.WebRootPath, "img"), "products");
            //    fileName = createProduct.HinhAnh.FileName;
            //    string filePath = Path.Combine(uploadFolder, fileName);
            //    createProduct.HinhAnh.CopyTo(new FileStream(filePath, FileMode.Create));
            //}

            //var product = new TbSanPham
            //{
            //    MaSanPham = createProduct.MaSanPham,
            //    TenSanPham = createProduct.TenSanPham,
            //    GiaBan = createProduct.GiaBan,
            //    MoTa = createProduct.MoTa,
            //    HinhAnh = fileName,
            //    GhiChu = createProduct.GhiChu,
            //    MaNhomSp = createProduct.MaLoaiSanPham
            //};
            string filePath = "";
            string uniqueFileName = "";
            if (sanPham.HinhAnh != null && sanPham.HinhAnh.Length > 0)
            {
                // Đối với mục đích minh họa, chúng ta sẽ lưu ảnh vào thư mục Images trong wwwroot
                string uploadFolder = Path.Combine(Path.Combine(hostEnvironment.WebRootPath, "img"), "products");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + sanPham.HinhAnh.FileName;
                filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    sanPham.HinhAnh.CopyTo(stream);
                }

                // Lưu đường dẫn hoặc thông tin về ảnh vào cơ sở dữ liệu nếu cần
                // Ví dụ: lưu đường dẫn filePath vào cơ sở dữ liệu
                // ...
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
            try
            {
                // Lấy sản phẩm từ database
                var product = _context.TbSanPhams.FirstOrDefault(x => x.MaSanPham == id);

                if (product == null)
                {
                    return NotFound();
                }

                // Chuyển đổi sang CreateProductViewModel
                var productViewModel = new CreateProductViewModel
                {
                    MaSanPham = product.MaSanPham,
                    TenSanPham = product.TenSanPham,
                    GiaBan = product.GiaBan,
                    MoTa = product.MoTa,
                    HinhAnhLink = product.HinhAnh,
                    GhiChu = product.GhiChu,
                    MaLoaiSanPham = product.MaNhomSp
                };

                // Chuẩn bị dữ liệu cho dropdown list nhóm sản phẩm
                ViewBag.MaNhomSp = new SelectList(_context.TbNhomSanPhams.ToList(), "MaNhomSp", "TenNhomSp", product.MaNhomSp);
                ViewBag.name = name;

                return View(productViewModel);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                TempData["Message"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [Route("Edit")]
        [Authentication]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CreateProductViewModel createProduct)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.MaNhomSp = new SelectList(_context.TbNhomSanPhams.ToList(), "MaNhomSp", "TenNhomSp");
                    return View(createProduct);
                }

                var existingProduct = _context.TbSanPhams.Find(createProduct.MaSanPham);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                string fileName = existingProduct.HinhAnh;

                if (createProduct.HinhAnh != null)
                {
                    string uploadFolder = Path.Combine(hostEnvironment.WebRootPath, "img", "products");
                    fileName = Guid.NewGuid().ToString() + "_" + createProduct.HinhAnh.FileName;
                    string filePath = Path.Combine(uploadFolder, fileName);

                    // Xóa file cũ nếu tồn tại
                    if (!string.IsNullOrEmpty(existingProduct.HinhAnh))
                    {
                        string oldFilePath = Path.Combine(uploadFolder, existingProduct.HinhAnh);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        createProduct.HinhAnh.CopyTo(fileStream);
                    }
                }

                // Cập nhật thông tin sản phẩm
                existingProduct.TenSanPham = createProduct.TenSanPham;
                existingProduct.GiaBan = createProduct.GiaBan;
                existingProduct.MoTa = createProduct.MoTa;
                if (createProduct.HinhAnh != null)
                {
                    existingProduct.HinhAnh = fileName;
                }
                existingProduct.GhiChu = createProduct.GhiChu;
                existingProduct.MaNhomSp = createProduct.MaLoaiSanPham;

                _context.SaveChanges();
                TempData["Message"] = "Sửa sản phẩm thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi sửa sản phẩm: " + ex.Message;
                ViewBag.MaNhomSp = new SelectList(_context.TbNhomSanPhams.ToList(), "MaNhomSp", "TenNhomSp");
                return View(createProduct);
            }
        }

        [Route("Delete")]
        [Authentication]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var product = _context.TbSanPhams.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            // Xóa file ảnh
            if (!string.IsNullOrEmpty(product.HinhAnh))
            {
                string uploadFolder = Path.Combine(hostEnvironment.WebRootPath, "img", "products");
                string filePath = Path.Combine(uploadFolder, product.HinhAnh);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.TbSanPhams.Remove(product);
            _context.SaveChanges();

            TempData["Message"] = "Xóa sản phẩm thành công";
            return RedirectToAction("Index");
        }
    }
}
