using System;
using AutoBogus;
using FluentAssertions;
using Xunit;

namespace MySpecificTest.Infrastructure.IntegrationTests.AutoBogus
{
    public class SimpleAutoBogusTest
    {
        [Fact]
        public void AutoFaker_Generate_Tester()
        {
            Customer customer = AutoFaker.Generate<Customer>();

            customer.FirstName.Should().NotBe("Hello World");
            customer.LastName.Should().NotBe("Hello World");
            customer.FirstName.Should().NotBe(customer.LastName);
            customer.DateOfBirth.Should().NotBe(DateTime.Now);
        }

        [Fact]
        public void AutoFaker_RuleFor_Tester()
        {
            Bogus.Faker<Customer> customerFaker = new AutoFaker<Customer>()
              .RuleFor(fake => fake.Id, fake => fake.Random.Int(10, 20))
              .RuleSet("empty", rules =>
              {
                  rules.RuleFor(fake => fake.Id, () => 0);
              });

            // Use explicit conversion or call Generate()
            var customer1 = (Customer)customerFaker;
            var customer2 = customerFaker.Generate();

            customer1.FirstName.Should().NotBe("Hello World");
            customer1.LastName.Should().NotBe("Hello World");
            customer1.FirstName.Should().NotBe(customer1.LastName);
            customer1.FirstName.Should().NotBe(customer2.LastName);
            customer1.FirstName.Should().NotBe(customer2.FirstName);
            customer1.Id.Should<int>();
            customer1.Id.Should().BeInRange(10, 20, "random id's between 10 and 20 are generated");
            customer1.DateOfBirth.Should().NotBe(DateTime.Now);
        }

        public class Customer
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string EmailAddress { get; set; }
            public DateTime DateOfBirth { get; set; }
            public Address Address { get; set; }
        };

        public class Address
        {
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string ZipOrPostcode { get; set; }
            public string Country { get; set; }
        }
    }
}