using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarkTest.Data.Entities
{
    public class PostMatch
    {
        public int PostId { get; set; }
        public string PostTitle { get; set; }
        public DateTime PostDate { get; set; }
        public string Category { get; set; }
        public string Author { get; set; }
        public string Photo { get; set; }
        public string BlogPost { get; set; }
        public postStatus status { get; set; }
    }
}
