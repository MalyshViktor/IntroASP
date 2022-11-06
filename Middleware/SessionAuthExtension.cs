using Microsoft.AspNetCore.Builder;

namespace Intro.Middleware
{
    // Расширение - добавление метода в другой класс (интерфейс)
    public static class SessionAuthExtension      // класс должен быть static
    {                                             // метод-расширение - именно он
        public static IApplicationBuilder         // будет доступен в другом классе
            UseSessionAuth(                       // название метода-расширения
                this IApplicationBuilder builder) // какой класс расширяется (IApplicationBuilder)
        {                                         // когда будет инструкция 
            return builder.                       // builder.UseSessionAuth(), выполнится следующий код:
                UseMiddleware<SessionAuthMiddleware>();
        }
    }
}
