using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarkTest.Data.Entities
{
    public class CommentMatch 
    {
        public string UserComment { get; set; }
        public int CommentId { get; set; }
        public DateTime CommentDate { get; set; }
        public int PostId { get; set; }
     //   public PostMatch commentPostMatch { get; set; }
    }
}
