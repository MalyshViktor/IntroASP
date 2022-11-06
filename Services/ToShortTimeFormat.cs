using System;

namespace Intro.Services
{
    public class ToShortTimeFormat : IDateTime
    {
        public string date(string date)
        {
            date = DateTime.Now.ToShortDateString();
            return date;
        }

        public string time(string time)
        {
            time = DateTime.Now.ToShortTimeString();
            return time;
        }
    }
}
