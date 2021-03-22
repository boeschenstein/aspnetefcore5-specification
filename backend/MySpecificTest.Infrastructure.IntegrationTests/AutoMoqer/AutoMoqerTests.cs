using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using MySpecificTest.Infrastructure.MediatR;
using MySpecificTest.Infrastructure.SpecificationPattern;
using Xunit;

namespace MySpecificTest.Infrastructure.IntegrationTests.AutoMoqer
{
    public class AutoMoqerTests
    {
        [Fact]
        public async Task Test_AutoMoqer_new()
        {
            var mocker = new AutoMoqCore.AutoMoqer();

            // not to use new() is an advantage:
            // If contructor of BlogWithItemsRequestHandler gets more arguments, the following line does not need a change:
            var neatoRepository = mocker.Create<BlogWithItemsRequestHandler>();

            // but what about IGenericRepository<Blog>?
            var blogs = new List<Blog>();
            var mock = mocker.GetMock<IGenericRepository<Blog>>(); // I was injected as IGenericRepository<Blog>

            var c = new System.Threading.CancellationToken();
            var req = new BlogWithItemsRequest("bla");

            // Act
            var ret = await neatoRepository.Handle(req, c);

            // Assert
            ret.Should().BeEquivalentTo(blogs);
        }

        [Fact]
        public async Task Test_AutoMoqer_setup()
        {
            var mocker = new AutoMoqCore.AutoMoqer();

            var blogWithItemsRequestHandler = mocker.Create<BlogWithItemsRequestHandler>();

            var blogs = new List<Blog> { new Blog { BlogId = 123, Url = "bla" } };

            // but what about IGenericRepository<Blog>?
            var req = new BlogWithItemsRequest("bla");
            mocker.GetMock<IGenericRepository<Blog>>() // I was injected as IGenericRepository<Blog>
                .Setup(x => x.List(req.Specification))
                .Returns(blogs);

            var c = new System.Threading.CancellationToken();

            blogWithItemsRequestHandler = mocker.Resolve<BlogWithItemsRequestHandler>();  // use Create or Resolve (either way works)

            // Act
            var ret = await blogWithItemsRequestHandler.Handle(req, c);

            // Assert
            ret.Should().BeEquivalentTo(blogs);
        }

    }
}