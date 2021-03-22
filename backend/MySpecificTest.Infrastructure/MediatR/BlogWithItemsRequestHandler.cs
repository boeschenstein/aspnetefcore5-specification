using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MySpecificTest.Infrastructure.SpecificationPattern;

namespace MySpecificTest.Infrastructure.MediatR
{
    public class BlogWithItemsRequest : IRequest<IEnumerable<Blog>>
    {
        public string Url { get; }
        public BlogWithItemsSpecification Specification { get; }

        public BlogWithItemsRequest(string url)
        {
            Url = url;
            this.Specification = new BlogWithItemsSpecification(url);
        }
    }

    public class BlogWithItemsRequestHandler : IRequestHandler<BlogWithItemsRequest, IEnumerable<Blog>>
    {
        private readonly IGenericRepository<Blog> _repository;

        public BlogWithItemsRequestHandler(IGenericRepository<Blog> repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Blog>> Handle(BlogWithItemsRequest request, CancellationToken cancellationToken)
        {
            // instantiate BlogWithItemsSpecification in BlogWithItemsRequest -> now we can use it in Unittesting (Moq, AutoMoqer)
            // IEnumerable<Blog> blogs = _repository.List(new BlogWithItemsSpecification(request.Url)); // not moqable

            IEnumerable<Blog> blogs = _repository.List(request.Specification);

            return Task.FromResult(blogs);
        }
    }
}