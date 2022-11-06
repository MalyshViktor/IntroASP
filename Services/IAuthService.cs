namespace Intro.Services
{
    public interface IAuthService
    {
        public DAL.Entities.User User { get; set; }

        public void Set(string id);
    }
}
