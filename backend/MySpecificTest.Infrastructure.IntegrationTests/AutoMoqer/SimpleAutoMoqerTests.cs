using Xunit;

namespace MySpecificTest.Infrastructure.IntegrationTests.AutoMoqer
{
    public class SimpleAutoMoqerTests
    {
        [Fact]
        public void SimpeTest()
        {
            var mocker = new AutoMoqCore.AutoMoqer();

            mocker.GetMock<IDataDependency>()
               .Setup(x => x.GetData())
               .Returns("TEST DATA");

            // not to use new() is an advantage:
            // If constructor of ClassToTest gets more arguments, the following line does not need a change:
            var classToTest = mocker.Create<ClassToTest>(); // create or resolve: either way works
            //var classToTest = mocker.Resolve<ClassToTest>(); // create or resolve: either way works

            classToTest.DoSomething();

            mocker.GetMock<IDependencyToCheck>()
               .Verify(x => x.CallMe("TEST DATA"), Moq.Times.Once);
        }

        public interface IDataDependency
        {
            string GetData();
        }

        public interface IDependencyToCheck
        {
            void CallMe(string info);
        }

        public class ClassToTest
        {
            private readonly IDataDependency dataDependency;
            private readonly IDependencyToCheck dependencyToCheck;

            public ClassToTest(IDataDependency dataDependency, IDependencyToCheck dependencyToCheck)
            {
                this.dataDependency = dataDependency;
                this.dependencyToCheck = dependencyToCheck;
            }

            public void DoSomething()
            {
                var ret = this.dataDependency.GetData();
                this.dependencyToCheck.CallMe(ret);
            }
        }
    }
}