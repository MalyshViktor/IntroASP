using System;

namespace Intro.Services
{
    public class ToLongTimeFormat : IDateTime
    {
        public string date(string date)
        {
            date = DateTime.Now.ToLongDateString();
            return date;
        }

        public string time(string time)
        {
            time = DateTime.Now.ToLongTimeString();
            return time;
        }
    }
}
