using Microsoft.AspNetCore.Http;
using System;

namespace Intro.Models
{
    public class ArticleModel
    {
        public Guid TopicId { get; set; }
        public string Text { get; set; }
        public Guid AuthorId { get; set; }
        public Guid? ReplyId { get; set; }
        public IFormFile Picture { get; set; }
    }
}
