namespace Intro.Services
{
    //создаем интерфейс для реализации разных вариантов отображений даты и времени в классах,
    //реализующих данный интерфейс
    public interface IDateTime
    {
        string date(string date);

        string time(string time);
    }
}
