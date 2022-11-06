using Intro.DAL.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intro.API
{
    [Route("api/topic")]
    [ApiController]
    public class TopicController : ControllerBase
    {
        private readonly IntroContext _context;

        public TopicController(IntroContext context)
        {
            _context = context;
        }


        // CREATE - POST
        [HttpPost]
        public object Post(Models.TopicModel topic)
        {
            // HttpContext.Request.Headers["User-Id"].ToString();
            // проверено - при отсутствие заголовка возвращает "" (не Exception)
            string UserIdHeader = HttpContext.Request.Headers["User-Id"].ToString();
            Guid UserId;
            // Validation
            try { UserId = Guid.Parse(UserIdHeader); }
            catch
            {
                return new
                {
                    status = "Error",
                    message = "User-Id Header empty or invalid (GUID expected)"
                };
            }
            // Задание: Добавить анализ заголовка Culture (xx-xx) uk-ua
            var SupportedCultures = new String[] { "uk-ua", "en-gb" };
            String CultureHeader = HttpContext.Request.Headers["Culture"].ToString().ToLower();
            if(Array.IndexOf(SupportedCultures, CultureHeader) == -1)
            {
                return new
                {
                    status = "Error",
                    message = "Culture Header empty or invalid or not supported"
                };
            }
            // Переносим информацию о локализации (культуре) в заголовки ответа
            HttpContext.Response.Headers.Add("Culture", CultureHeader);

            // проверяем данные
            if(topic == null)
            {
                return new { status = "Error", message = "No data" };
            }
            if (String.IsNullOrEmpty(topic.Title) 
             || String.IsNullOrEmpty(topic.Description))
            {
                return new { status = "Error", message = "Empty Title or Description" };
            }

            // поиск пользователя в БД
            var user = _context.Users.Find(UserId);
            if (user == null)
            {
                return new { status = "Error", message = "Forbidden" };
            }

            // Есть ли уже топик с таким названием?
            if(_context.Topics.Where(t => t.Title == topic.Title).Any())
            {
                return new { status = "Error", message = $"Topic '{topic.Title}' does exist" };
            }

            // Создаем топик и сохраняем в БД
            _context.Topics.Add(new()
            {
                Title = topic.Title,
                Description = topic.Description,
                AuthorId = user.Id,
                CreatedMoment = DateTime.Now
            });
            _context.SaveChangesAsync();

            return new { status = "Ok", message = $"Topic '{topic.Title}' created" };
        }

        [HttpGet]
        public IEnumerable<DAL.Entities.Topic> Get()
        {
            return _context.Topics.OrderByDescending(t => t.LastArticleMoment);
        }
    }
}
