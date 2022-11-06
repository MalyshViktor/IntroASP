using Intro.DAL.Context;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Intro.API
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IntroContext _context;
        private readonly Services.IHasher _hasher;

        public UserController(IntroContext context, Services.IHasher hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        [HttpGet]
        public string Get(string login, string password)
        {
            if (string.IsNullOrEmpty(login))
            {
                HttpContext.Response.StatusCode = 409;
                return "Conflict: login required";
            }
            if (string.IsNullOrEmpty(password))
            {
                HttpContext.Response.StatusCode = 409;
                return "Conflict: password required";
            }
            DAL.Entities.User user = 
                _context
                .Users
                .Where(u => u.Login == login)
                .FirstOrDefault();

            if (user == null)
            {
                HttpContext.Response.StatusCode = 401;
                return "Unauthorized: credentials rejected";
            }
            // Скопировано из AuthController - Login
            // 2. Хешируем соль + введенный пароль
            String PassHash = _hasher.Hash(password + user.PassSalt);

            // 3. Проверяем равенство полученного и хранимого хешей
            if (PassHash != user.PassHash)
            {
                HttpContext.Response.StatusCode = 401;
                return "Unauthorized: credentials invalid";
            }

            return user.Id.ToString();
        }

        // GET /api/user/0ab58465-6253-47a9-d48f-08da5b8a155b
        [HttpGet("{id}")]
        public object Get(String id)
        {
            Guid guid;
            // Validation
            try
            {
                guid = Guid.Parse(id);
            }
            catch
            {
                HttpContext.Response.StatusCode = 409;
                return "Conflict: invalid id format (GUID required)";
            }
            // find user
            // return ( _context.Users.Find(guid) ?? new DAL.Entities.User() ) with { PassHash = "*", PassSalt = "*"};
            var user = _context.Users.Find(guid);
            if (user != null) return user with { PassHash = "*", PassSalt = "*" };
            return "null";
        }

        // POST api/<UserController>
        [HttpPost]
        public string Post([FromBody] string value)
        {
            return $"POST {value}";
        }

        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public object Put(String id, [FromForm] Models.RegUserModel userData)
        {
            DAL.Entities.User user;
            Guid userid;
            String avatarFilename = null;

            #region Validation            
            try  // is id valid?
            {
                userid = Guid.Parse(id);
            }
            catch
            {
                HttpContext.Response.StatusCode = 409;
                return "Conflict: invalid id format (GUID required)";
            }
            // does user exist (with given id) ?
            user = _context.Users.Find(userid);
            if (user == null)
            {
                HttpContext.Response.StatusCode = 404;
                return "User with given id not found";
            }
            if(userData.Login != null)
            {
                if (Regex.IsMatch(userData.Login, @"\s"))
                {
                    HttpContext.Response.StatusCode = 409;
                    return "Conflict: Login could not contain space(s)";
                }
                else if (_context.Users.Where(u => u.Login == userData.Login).Count() > 0)
                {
                    HttpContext.Response.StatusCode = 409;
                    return "Conflict: Login in use";
                }
            }
            if(userData.RealName != null)
            {
                // Имя: нет цифр, не пустое
                if(userData.RealName == String.Empty)
                {
                    HttpContext.Response.StatusCode = 409;
                    return "Conflict: RealName could not be empty";
                }
                if (Regex.IsMatch(userData.Login, @"\d"))
                {
                    HttpContext.Response.StatusCode = 409;
                    return "Conflict: RealName could not contain digit(s)";
                }
            }

            // Avatar
            if (userData.Avatar != null)
            {
                // формируем имя для файла и сохраняем
                int pos = userData.Avatar.FileName.LastIndexOf('.');
                string IMG_Format = userData.Avatar.FileName.Substring(pos);
                avatarFilename =
                    _hasher.Hash(Guid.NewGuid().ToString())
                     + IMG_Format;
                var file = new FileStream("./wwwroot/img/" + avatarFilename, FileMode.Create);
                userData.Avatar.CopyToAsync(file).ContinueWith(t => file.Dispose());
                // удаляем старый файл
                System.IO.File.Delete("./wwwroot/img/" + user.Avatar);
            }

            #endregion

            // updates
            if (userData.Login != null)  user.Login = userData.Login;
            if (userData.RealName != null) user.RealName = userData.RealName;
            if(avatarFilename != null) user.Avatar = avatarFilename;
            _context.SaveChanges();

            return user with { PassHash = "*", PassSalt = "*" };
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
/* Д.З. Реализовать в методе PUT :
    возможность изменения пароля
    возможность изменения E-mail
 */