using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarkTest.Data.Entities
{
    public class Like
    {
        public int LikeId { get; set; }
        public DateTime LikeDate { get; set; }

        public ApplicationUser User { get; set; }
        public Post Post { get; set; }
        public Comment Comment { get; set; }
    }
}
