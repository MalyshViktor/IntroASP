using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intro.DAL.Context
{
    public class UsersConfiguration
        : IEntityTypeConfiguration<Entities.User>
    {
        public void Configure(
            EntityTypeBuilder<Entities.User> builder)
        {
            // Начальная конфигурация при построении модели
            // (делегируется из Контекста). Здесь
            // можно задать начальные значения полей,
            // поменять имя таблицы (по умолчанию - имя класса)
            // а также задать начальные данные для таблицы (seed)

            // Для пользователей традиционно задается корневой
            // администратор, с которого все начинается

            builder.HasData(new Entities.User
            {
                Id        = System.Guid.NewGuid(),
                RealName  = "Корневой администратор",
                Login     = "Admin",
                PassHash  = "",
                Email     = "",
                PassSalt  = "",
                RegMoment = System.DateTime.Now,
                Avatar    = ""
            });
        }
    }
}
