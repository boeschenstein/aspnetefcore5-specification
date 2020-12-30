using System.Collections.Generic;

namespace MySpecificTest.Infrastructure
{
    public class Blog
    {
        public int BlogId { get; set; }
        public string Url { get; set; }

        // Referential Integrity: with this line, EF will generate a foreign key
        public List<Post> Posts { get; } = new List<Post>();
    }
}