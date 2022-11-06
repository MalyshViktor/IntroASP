using Intro.Models;
using Intro.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Intro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RandomService _randomService;
        private readonly IHasher _hasher;
        private readonly IDateTime _dateTime;
        private readonly DAL.Context.IntroContext _introContext;
        private readonly IAuthService _authService;

        public HomeController(              // Внедрение зависимостей
            ILogger<HomeController> logger, // через конструктор
            RandomService randomService,
            IHasher hasher,
            IDateTime dateTime,
            DAL.Context.IntroContext introContext,
            IAuthService authService)  
        {
            _logger = logger;
            _randomService = randomService;
            _hasher = hasher;
            _dateTime = dateTime;
            _introContext = introContext;
            _authService = authService;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewData["AuthUser"] = _authService.User;
            // base.OnActionExecuting(context);
            /* var list = _introContext
                .Articles
                .Select( a => new { 
                    Article = a, 
                    Replies = _introContext.Articles.Where( r => a.Id == r.ReplyId).ToList()

                }).ToList(); */

        }

        public IActionResult Index()
        {
            ViewData["rnd"] = "<b>" + _randomService.Integer + "</b>";
            ViewBag.hash = _hasher.Hash("123");
            ViewData["UsersCount"] = _introContext.Users.Count();
            ViewBag.RealName = _introContext.Users.Select(x => x.RealName).ToList();
            //добавляем данные о времени для вывода в двух разных форматах
            ViewBag.dateLong = _dateTime.date("dd:mm:yyyy");
            ViewBag.timeLong = _dateTime.time("HH:MM:SS");
            ViewBag.date = _dateTime.date("M:d:yyyy");
            ViewBag.time = _dateTime.time("hh:mm");
            // Значение в HttpContext.Items добавлено в классе SessionAuthMiddleware
            ViewData["fromAuthMiddleware"] = HttpContext.Items["fromAuthMiddleware"];
            // проверяем службу авторизации
            ViewData["authUserName"] = _authService.User?.RealName;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            var model = new AboutModel
            {
                Data = "The Model Data"
            };
            return View(model);
        }

       
        public async Task<IActionResult> Random()
        {
            return Content(
                _randomService.Integer.ToString());
        }

        public IActionResult Data()
        {
            return Json(new { field = "value" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
                });
        }
    }
}
/*
 * Д.З. Вывести на стартовой странице всех
 * зарегистрированных в БД пользователей (только RealName)
 */