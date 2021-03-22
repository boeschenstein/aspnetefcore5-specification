using Xunit;

namespace MySpecificTest.Infrastructure.IntegrationTests.AutoMoqer
{
    public class SimpleAutoMoqerTests_args_object
    {
        [Fact]
        public void SimpeTest()
        {
            var mocker = new AutoMoqCore.AutoMoqer();

            var myArgs = new MyArgs("myArg1");

            mocker.GetMock<IDataDependency>()
               .Setup(x => x.GetDataArgs(myArgs))
               .Returns("TEST DATA");

            var classToTest = mocker.Resolve<ClassToTest>();

            classToTest.DoSomething(myArgs);
            // classToTest.DoSomething(new MyArgs("myArg1")); DOES NOT WORK - NEEDS TO BE THE SAME INSTANCE

            mocker.GetMock<IDependencyToCheck>()
               .Verify(x => x.CallMe("TEST DATA"), Moq.Times.Once);
        }

        public interface IDataDependency
        {
            string GetDataArgs(MyArgs args);
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

            public void DoSomething(MyArgs myArgs)
            {
                var ret = this.dataDependency.GetDataArgs(myArgs);
                this.dependencyToCheck.CallMe(ret);
            }
        }

        public class MyArgs
        {
            public MyArgs(string arg1)
            {
                Arg1 = arg1;
            }

            public string Arg1 { get; }
        }
    }
}