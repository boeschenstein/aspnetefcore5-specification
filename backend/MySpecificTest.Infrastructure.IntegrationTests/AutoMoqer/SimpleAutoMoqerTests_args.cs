using Xunit;

namespace MySpecificTest.Infrastructure.IntegrationTests.AutoMoqer
{
    public class SimpleAutoMoqerTests_args
    {
        [Fact]
        public void SimpeTest()
        {
            var mocker = new AutoMoqCore.AutoMoqer();

            mocker.GetMock<IDataDependency>()
               .Setup(x => x.GetDataArgs("testArgs - setup and real call must match"))
               .Returns("TEST DATA");

            var classToTest = mocker.Resolve<ClassToTest>();

            classToTest.DoSomething();

            mocker.GetMock<IDependencyToCheck>()
               .Verify(x => x.CallMe("TEST DATA"), Moq.Times.Once);
        }

        public interface IDataDependency
        {
            string GetDataArgs(string args);
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
                var ret = this.dataDependency.GetDataArgs("testArgs - setup and real call must match");
                this.dependencyToCheck.CallMe(ret);
            }
        }
    }
}