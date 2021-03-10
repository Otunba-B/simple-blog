using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarkTest.Data.Entities
{
    public class LikeCommentMatch
    {
        public int LikeId { get; set; }
        public DateTime LikeDate { get; set; }
        //public int PostId { get; set; }
        public int CommentId { get; set; }
    }
}
