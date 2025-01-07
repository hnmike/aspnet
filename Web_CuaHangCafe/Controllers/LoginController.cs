using Microsoft.AspNetCore.Mvc;

namespace Web_CuaHangCafe.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        //châu co gì ở đây  cả 

        public IActionResult Create(string user,string password)
        {
            return View();
        }
    }
}
