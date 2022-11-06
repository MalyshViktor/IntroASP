using Intro.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Intro.Middleware
{
    public class SessionAuthMiddleware
    {
        // обязательное поле (для Middleware)
        private readonly RequestDelegate next;  // ссылка на следующий слой 

        // обязательная форма конструктора
        public SessionAuthMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        // обязательный метод класса
        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            String userId = context.Session.GetString("userId");
            if (userId != null)
            {   // Была авторизация и в сессии хранится id пользователя
                authService.Set(userId);
                // Извлекаем метку времени начала авторизации и вычисляем длительность
                long authMoment = Convert.ToInt64(
                    context.Session.GetString("AuthMoment"));

                // 10 000 000 тиков в секунде
                long authInterval = (DateTime.Now.Ticks - authMoment) / (long)1e7;

                if (authInterval > 100)  // Предельная длительность сеанса авторизации
                {
                    // Стираем из сессии признак авторизации
                    context.Session.Remove("userId");
                    context.Session.Remove("AuthMoment");
                    // По правилам безопасности: если меняется состояние авторизации
                    //  то необходимо перезагрузить систему (страницу)
                    context.Response.Redirect("/");
                    return;
                }
            }
            context.Items.Add("fromAuthMiddleware", "Hello !!");
            await next(context);
        }
    }
}
