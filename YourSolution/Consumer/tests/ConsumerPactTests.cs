using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Consumer;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using Xunit;

namespace tests
{
    public class ConsumerPactTests : IClassFixture<ConsumerPactClassFixture>
    {
        private IMockProviderService _mockProviderService;
        private string _mockProviderServiceBaseUri;

        public ConsumerPactTests(ConsumerPactClassFixture fixture)
        {
            _mockProviderService = fixture.MockProviderService;
            _mockProviderService.ClearInteractions();
            _mockProviderServiceBaseUri = fixture.MockProviderServiceBaseUri;
        }

        [Fact]
        public async Task ItHandlesInvalidDateParam()
        {
            // Arange
            var invalidRequestMessage = "validDateTime is not a date or time";
            _mockProviderService.Given("There is data")
                                .UponReceiving("A invalid GET request for Date Validation with invalid date parameter")
                                .With(new ProviderServiceRequest
                                {
                                    Method = HttpVerb.Get,
                                    Path = "/api/provider",
                                    Query = "validDateTime=lolz"
                                })
                                .WillRespondWith(new ProviderServiceResponse
                                {
                                    Status = 400,
                                    Headers = new Dictionary<string, object>
                                    {
                                        { "Content-Type", "application/json; charset=utf-8" }
                                    },
                                    Body = new
                                    {
                                        message = invalidRequestMessage
                                    }
                                });

            // Act
            var result = await ConsumerApiClient.ValidateDateTimeUsingProviderApi("lolz", _mockProviderServiceBaseUri);
            var resultBodyText = await result.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains(invalidRequestMessage, resultBodyText);
        }


        [Fact]
        public async Task ItShouldReturnValidDateTime()
        {
            // Arange
            var expectedResult = "{\"test\":\"NO\",\"validDateTime\":\"08-09-2018 00:00:00\"}";
            var expectedDateTime = new DateTime(2018, 09, 08);
            _mockProviderService.Given("There is data")
                                .UponReceiving("A valid GET request for Date Validation with valid date parameter")
                                .With(new ProviderServiceRequest
                                {
                                    Method = HttpVerb.Get,
                                    Path = "/api/provider",
                                    Query = "validDateTime=2018-09-08"
                                })
                                .WillRespondWith(new ProviderServiceResponse
                                {
                                    Status = 200,
                                    Headers = new Dictionary<string, object>
                                    {
                                        { "Content-Type", "application/json; charset=utf-8" }
                                    },
                                    Body = new
                                    {
                                        test = "NO",
                                        validDateTime = expectedDateTime.ToString("dd-MM-yyyy HH:mm:ss")
                                    }
                                });

            // Act
            var result = await ConsumerApiClient.ValidateDateTimeUsingProviderApi("2018-09-08", _mockProviderServiceBaseUri);
            var resultBodyText = await result.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains(expectedResult, resultBodyText);
        }
    }
}
