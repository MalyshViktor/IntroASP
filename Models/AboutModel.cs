namespace Intro.Models
{
    public class AboutModel
    {
        public string Data { get; set; }
        public string Date =>
            System.DateTime.Now.ToString();
    }
}
