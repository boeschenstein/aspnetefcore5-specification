using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using FluentAssertions.Execution;

namespace MySpecificTest.Infrastructure.IntegrationTests.FluentAssertionsExtensibility
{
    internal static class HttpResponseMessageExtensions
    {
        public static HttpResponseMessageAssertions Should(this HttpResponseMessage response)
        {
            return new HttpResponseMessageAssertions(response);
        }
    }

    internal class HttpResponseMessageAssertions
    {
        private HttpResponseMessage response;

        public HttpResponseMessageAssertions(HttpResponseMessage response)
        {
            this.response = response;
        }

        [CustomAssertion]
        public AndConstraint<HttpResponseMessageAssertions> BeOk(string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(response.StatusCode == System.Net.HttpStatusCode.OK)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:response} to be OK{reason}, but found {0}", response.StatusCode);
            return new AndConstraint<HttpResponseMessageAssertions>(this);
        }

        [CustomAssertion]
        public AndConstraint<HttpResponseMessageAssertions> HaveReason(string expectedReason, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(response.ReasonPhrase.Equals(expectedReason, StringComparison.InvariantCultureIgnoreCase))
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:response} to have reason {0}{reason}, but found {1}", expectedReason, response.ReasonPhrase);
            return new AndConstraint<HttpResponseMessageAssertions>(this);
        }

        [CustomAssertion]
        public AndWhichConstraint<HttpResponseMessageAssertions, string> HaveHeader(string key, string because = "", params object[] becauseArgs)
        {
            string[] allHeadersKeys = response.Headers.Select(h => h.Key).ToArray();

            // todo: needs nspec: why?
            Execute.Assertion
                .ForCondition(response.Headers.TryGetValues(key, out IEnumerable<string> values))
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:response} to contain header with key {0}{reason}, but found {1}", key, allHeadersKeys.FirstOrDefault());
            return new AndWhichConstraint<HttpResponseMessageAssertions, string>(this, values!.FirstOrDefault());
        }
    }
}