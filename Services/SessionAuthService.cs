using Intro.DAL.Context;
using Intro.DAL.Entities;
using System;

namespace Intro.Services
{
    public class SessionAuthService : IAuthService
    {
        private readonly DAL.Context.IntroContext _context;
        public User User { get; set; }

        public SessionAuthService(IntroContext context)
        {
            _context = context;
        }

        
        public void Set(string id)
        {
            User = _context.Users.Find(Guid.Parse(id));
        }
    }
}
