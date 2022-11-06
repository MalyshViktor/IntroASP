using Microsoft.AspNetCore.Http;

namespace Intro.Models
{
    // Модель для сбора данных из формы регистрации пользователя
    // Имена свойств должны совпадать (до регистра) с именами
    //  полей формы
    public class RegUserModel
    {
        public string    RealName  { get; set; }
        public string    Login     { get; set; }
        public string    Password1 { get; set; }
        public string    Password2 { get; set; }
        public string    Email     { get; set; }
        public IFormFile Avatar    { get; set; }
    }
}
