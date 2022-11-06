using Intro.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Intro.Controllers
{
    public class AuthController : Controller
    {
        private readonly Services.IHasher _hasher;
        private readonly DAL.Context.IntroContext _introContext;
        private readonly IAuthService _authService;

        // private DAL.Entities.User authUser;  // ссылка на авторизованного пользователя

        public AuthController(
            Services.IHasher hasher,
            DAL.Context.IntroContext introContext, 
            IAuthService authService)
        {
            _hasher = hasher;
            _introContext = introContext;
            _authService = authService;
        }


        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewData["AuthUser"] = _authService.User;
        }

        public IActionResult Index()
        {
            // Сюда мы попадаем как при начале авторизации, так и в результате
            // перенаправлений из других методов. Каждый из них устанавливает
            // свои сессионные атрибуты
            String LoginError = HttpContext.Session.GetString("LoginError");
            if (LoginError != null)
            {   // Был запрос на авторизацию (логин) и он завершился ошибкой
                ViewData["LoginError"] = LoginError;
                HttpContext.Session.Remove("LoginError");
            }
            String userId = HttpContext.Session.GetString("userId");
            if (userId != null)
            {   // Был запрос на авторизацию (логин) и он завершился успехом
                // Находим пользователя по id и передаем найденный объект
                ViewData["AuthUser"] = _introContext.Users.Find(Guid.Parse(userId));
            }
            // Значение в HttpContext.Items добавлено в классе SessionAuthMiddleware
            ViewData["fromAuthMiddleware"] = HttpContext.Items["fromAuthMiddleware"];

            return View();
        }

        public IActionResult Register()
        {
            // извлекаем из сессии значение по ключу "RegError"
            String err = HttpContext.Session.GetString("RegError");
            if (err != null)  // Есть сообщение
            {
                ViewData["Error"] = err;  // передаем на View
                ViewData["err"] = err.Split(";");  // разделение строки на массив
                // и удаляем из сессии - однократный вывод
                HttpContext.Session.Remove("RegError");

                Models.RegUserModel UserData =  // десериализация объекта
                    JsonConvert.DeserializeObject<Models.RegUserModel>(
                        HttpContext.Session.GetString("UserData"));

                ViewData["UserData"] = HttpContext.Session.GetString("UserData");
            }

            return View();
        }


        [HttpPost]                 // Срабатывает только на POST запрос
        public RedirectResult      // Возвращает перенаправление (302)
            Login(                 // Название метода
            String UserLogin,      //  параметры связываются по именам
            String UserPassword)   //  в форме должны быть такие же имена
        {
            // Валидация данных - проверка на пустоту и шаблоны
            // Для многократных проверок часто пользуются try-catch
            DAL.Entities.User user;  // ссылка на авторизованного пользователя
            try
            {
                if (String.IsNullOrEmpty(UserLogin))
                    throw new Exception("Login Empty");

                if(String.IsNullOrEmpty(UserPassword))
                    throw new Exception("Password Empty");

                // проверяем (авторизируемся)
                // 1. По логину ищем пользователя и извлекаем соль, хеш пароля
                user = 
                    _introContext
                    .Users
                    .Where(u => u.Login == UserLogin)
                    .FirstOrDefault();

                if (user == null)
                {   // нет пользователя с таким логином
                    throw new Exception("Login invalid");
                }
                // 2. Хешируем соль + введенный пароль
                String PassHash = _hasher.Hash(UserPassword + user.PassSalt);

                // 3. Проверяем равенство полученного и хранимого хешей
                if(PassHash != user.PassHash)
                    throw new Exception("Password invalid");
            }
            catch (Exception ex)
            {   // сюда мы попадаем если была ошибка
                HttpContext.Session.SetString("LoginError", ex.Message);
                // завершаем обработку
                return Redirect("/Auth/Index");
            }
            // Если не было catch, то и не было ошибок
            // тогда user - авторизованный пользователь, сохраняем 
            // его данные в сессии (обычно ограничиваются id)

            HttpContext.Session.SetString("userId", user.Id.ToString());
            // создаем метку времени начала авторизации для контроля ее длительности
            HttpContext.Session.SetString("AuthMoment", DateTime.Now.Ticks.ToString());

            /// Метод закончится установкой сессии LoginError либо userId
            /// и перенаправлением
            return Redirect("/");  //   /Home/Index  - главная страница
        }

        [HttpPost]  // Следующий метод срабатывает только на POST-запрос
        // Метод может автоматически собрать все переданные данные в модель
        //  по совпадению имен
        public IActionResult RegUser(Models.RegUserModel UserData)
        {
            // return Json(UserData);  // способ проверить передачу данных
            String[] err = new String[6];

            if (UserData == null)
            {
                err[0] = "Некорректный вызов (нет данных)";
            }
            else
            {
                if (String.IsNullOrEmpty(UserData.Login))
                {
                    err[2] = "Логин не может быть пустым";
                }
                if (String.IsNullOrEmpty(UserData.Email))
                {
                    err[5] = "Email не может быть пустым";
                }
                if (UserData.Avatar != null)  // есть переданный файл
                {
                    // файл нужно сохранить в нужном месте.
                    // для простоты доступа - в папку wwwroot/img
                    // !! крайне НЕ рекомендуется сохранять имя переданного файла
                    //  - возможен конфликт, если разные пользователи -- одно имя
                    //  - уязвимость - если в имени файла есть ../

                    // Д.З. Сформировать новое имя файла, сохранить расширение,
                    //   убедиться, что файл не стирает к-то другой файл

                    UserData.Avatar.CopyToAsync(
                        new FileStream(
                            "./wwwroot/img/" + UserData.Avatar.FileName,
                            FileMode.Create));
                }
                // если валидация пройдена, то добавляем пользователя в БД
                // валидация успешна если нет сообщений об ошибках
                bool isValid = true;
                foreach (string error in err)
                {
                    if (!String.IsNullOrEmpty(error)) isValid = false;
                }
                if (isValid)   // валидация успешна
                {
                    var user = new DAL.Entities.User();
                    // крипто-соль - это случайное число (в сроковом виде)
                    user.PassSalt = _hasher.Hash(DateTime.Now.ToString());
                    user.PassHash = _hasher.Hash(
                        UserData.Password1 + user.PassSalt);  // соль "смешивается" с паролем
                    user.Avatar = UserData.Avatar?.FileName;   // заменить по результатам ДЗ
                    user.Email = UserData.Email;
                    user.RealName = UserData.RealName;
                    user.Login = UserData.Login;
                    user.RegMoment = DateTime.Now;

                    // добавляем в БД (контекст)
                    _introContext.Users.Add(user);

                    // сохраняем изменения
                    _introContext.SaveChanges();
                }
            }

            // Для "сохранения" на форме ранее введенных данных,
            //  помещаем в сессию данные формы, кроме пароля и файла
            UserData.Password1 = String.Empty;
            UserData.Password2 = String.Empty;
            UserData.Avatar = null;
            HttpContext.Session.SetString(
                "UserData",
                JsonConvert.SerializeObject(UserData));

            // ViewData["err"] = err;  -- не переживает redirect
            HttpContext.Session  // Сессия - "межзапросное хранилище"
                .SetString(      // обычно сохраняют значимые типы
                    "RegError",  //  ключ (индекс)
                    String.Join(";", err));    //  значение

            // return View("Register");  -- POST запрос не должен завершаться View
            return RedirectToAction("Register");
        }


        public IActionResult Profile()
        {
            // Задание: если пользователь не авторизован, то перенаправить на страницу логина
            if(_authService.User == null)
            {
                // Внутренний (внутрисервервный) редирект. В браузере остается адрес 
                //  /Auth/Profile, а реально отображается /Auth/Index
                // return View("Index");

                // Внешний редирект - браузер повторяет запрос на новый адрес
                //  запрос /Auth/Profile -- редирект на /Auth/Index и он отображается
                return Redirect("/Auth/Index");
            }

            return View();
        }

        public String ChangeRealName(String NewName)
        {
            if(_authService.User == null)
            {
                return "Forbidden";
            }
            return NewName + " changed";
        }

        [HttpPost]
        public JsonResult ChangeLogin([FromBody] String NewLogin)
        {
            String message = "OK";
            if(_authService.User == null)
            {
                message = "Unathorized";
            }
            else if (String.IsNullOrEmpty(NewLogin))
            {
                message = "Login could not be empty";
            }
            else if (Regex.IsMatch(NewLogin, @"\s"))
            {
                message = "Login could not contain space(s)";
            }
            else if(_introContext.Users.Where(u => u.Login == NewLogin).Count() > 0)
            {
                message = "Login in use";
            }

            if(message == "OK")  // не было ошибок
            {
                // обновляем данные в БД
                _authService.User.Login = NewLogin;  
                // authService.User - ссылка на пользователя в БД,
                // поэтому изменения через authService.User сразу отражаются на БД
                _introContext.SaveChanges();  // остается только сохранить изменения
            }
            return Json(message);
        }

        [HttpPut]
        public JsonResult ChangeEmail([FromForm] String NewEmail)
        {
            String message = "OK " + NewEmail;

            return Json(message);
        }

        [HttpPost]
        public JsonResult ChangeAvatar(IFormFile userAvatar)
        {
            if (_authService.User == null)
            {
                return Json(new { Status = "Error", Message = "Forbidden" });
            }
            if (userAvatar == null)
            {
                return Json(new { Status = "Error", Message = "No file" });
            }
            // формируем имя для файла и сохраняем
            int pos = userAvatar.FileName.LastIndexOf('.');
            string IMG_Format = userAvatar.FileName.Substring(pos);
            string ImageName = 
                _hasher.Hash( Guid.NewGuid().ToString() )
                 + IMG_Format;

            var file = new FileStream("./wwwroot/img/" + ImageName, FileMode.Create);
            userAvatar.CopyToAsync(file).ContinueWith(t => file.Dispose());

            // удаляем старый файл и заменяем у пользователя ссылку на новый файл
            System.IO.File.Delete("./wwwroot/img/" + _authService.User.Avatar);
            _authService.User.Avatar = ImageName;
            _introContext.SaveChanges();  // сохраняем изменения в БД

            return Json(new { Status = "Ok", Message = ImageName });
        }
    }
}
/* В методе ChangeRealName реализовать случайным образом ответы
 * об успехе или ошибке.
 * Со стороны JS если приходит ошибка, то возвращать старое имя
 * если успех - оставлять новое.
 * ** Обеспечить серверную валидацию по типу "Имя Фамилия" (без спецзнаков,
 *    начинаются с больших букв и т.п.)
 */