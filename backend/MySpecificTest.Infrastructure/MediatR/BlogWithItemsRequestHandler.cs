using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MySpecificTest.Infrastructure.SpecificationPattern;

namespace MySpecificTest.Infrastructure.MediatR
{
    public class BlogWithItemsRequest : IRequest<IEnumerable<Blog>>
    {
        public string Url { get; private set; }

        public BlogWithItemsRequest(string url)
        {
            Url = url;
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
            IEnumerable<Blog> blogs = _repository.List(new BlogWithItemsSpecification(request.Url));
            return Task.FromResult(blogs);
        }
    }
}