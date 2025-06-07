using System.Diagnostics;
using CryptoScreener.Models;
using CryptoScreener.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptoScreener.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BinanceService _binanceService;

        public HomeController(ILogger<HomeController> logger, BinanceService service)
        {
            _logger = logger;
            _binanceService = service;
        }

        public IActionResult Index()
        {
            var data = _binanceService.GetCachedData();
            return View(data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
