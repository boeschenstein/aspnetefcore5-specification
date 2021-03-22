﻿using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using MySpecificTest.Infrastructure.MediatR;
using Xunit;

namespace MySpecificTest.Infrastructure.IntegrationTests.AutoFixture
{
    public class SimpleAutoFixtureTest
    {
        [Theory, AutoData]
        public void IntroductoryTest_all_args_autogenerated(int expectedNumber, MyClass sut)
        {
            // Act
            int result = sut.Echo(expectedNumber);
            // Assert
            Assert.Equal(expectedNumber, result);
        }

        [Theory, AutoData]
        public void IntroductoryTest_autogenerate_class(MyClass sut)
        {
            // Arrange
            Fixture fixture = new Fixture();
            int expectedNumber = fixture.Create<int>();
            // Act
            int result = sut.Echo(expectedNumber);
            // Assert
            Assert.Equal(expectedNumber, result);
        }

        [Theory, AutoData]
        public void IntroductoryTest_autogenerate_int(int expectedNumber)
        {
            // Arrange
            Fixture fixture = new Fixture();
            MyClass sut = fixture.Create<MyClass>();
            // Act
            int result = sut.Echo(expectedNumber);
            // Assert
            Assert.Equal(expectedNumber, result);
        }

        [Theory, AutoData]
        public void AutoFixture_BlogWithItemsRequest(BlogWithItemsRequest sut)
        {
            sut.Url.Should().Be(sut.Specification.Url);
        }

        public class MyClass
        {
            public T Echo<T>(T value)
            {
                return value;
            }
        }
    }
}