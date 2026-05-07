using Microsoft.AspNetCore.Mvc;

namespace Prueba3._0.Controllers
{
    public class DocumentosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            return View();
        }

        public IActionResult Approve(int id)
        {
            return View();
        }
    }
}
