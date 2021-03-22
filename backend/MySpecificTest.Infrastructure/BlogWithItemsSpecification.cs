using MySpecificTest.Infrastructure.SpecificationPattern;

namespace MySpecificTest.Infrastructure
{
    public class BlogWithItemsSpecification : BaseSpecification<Blog>
    {
        //public string BlogId { get; } // for unittesting
        public string Url { get; } // for unittesting

        public BlogWithItemsSpecification(int blogId)
            : base(b => b.BlogId == blogId)
        {
            //this.BlogId = BlogId;
            AddInclude(b => b.Posts);
        }

        public BlogWithItemsSpecification(string url)
            : base(b => b.Url == url)
        {
            this.Url = url;
            AddInclude(b => b.Posts);
        }
    }
}