using MySpecificTest.Infrastructure.SpecificationPattern;

namespace MySpecificTest.Infrastructure
{
    public class BlogWithItemsSpecification : BaseSpecification<Blog>
    {
        public BlogWithItemsSpecification(int blogId)
            : base(b => b.BlogId == blogId)
        {
            AddInclude(b => b.Posts);
        }

        public BlogWithItemsSpecification(string url)
            : base(b => b.Url == url)
        {
            AddInclude(b => b.Posts);
        }
    }
}