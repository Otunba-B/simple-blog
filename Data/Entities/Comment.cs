using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarkTest.Data.Entities
{
    public class Comment
    {
        public int CommentId { get; set; }
        public DateTime CommentDate { get; set; }

        public ApplicationUser User { get; set; }
        public Post Post { get; set; }
    }
}
