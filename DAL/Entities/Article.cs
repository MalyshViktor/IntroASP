using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intro.DAL.Entities
{
    public class Article
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid TopicId { get; set; }
        public Entities.Topic Topic { get; set; }

        public string Text { get; set; }

        public Guid AuthorId { get; set; }
        public Entities.User Author { get; set; }

        public Guid? ReplyId { get; set; }
        public Entities.Article Reply { get; set; }
        // public List<Article> Replies { get; set; }

        public DateTime CreatedDate { get; set; }
        public string PictureFile { get; set; }

        public DateTime? DeleteMoment { get; set; }
    }
}
