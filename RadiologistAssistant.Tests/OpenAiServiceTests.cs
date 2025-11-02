using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using RadiologistAssistant.Models;
using RadiologistAssistant.Services;
using Xunit;

namespace RadiologistAssistant.Tests
{
    public class OpenAiServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;

        public OpenAiServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        }

        [Fact]
        public async Task GetDiagnosisFromImage_ReturnsAnalysis_WhenApiCallIsSuccessful()
        {
            // Arrange
            var expectedAnalysis = new Analysis
            {
                Summary = "Test Summary",
                Findings = new System.Collections.Generic.List<Finding>
                {
                    new Finding { Description = "Test Finding", ConfidenceLevel = 0.9 }
                }
            };
            var apiResponse = new
            {
                choices = new[]
                {
                    new { message = new { content = JsonConvert.SerializeObject(expectedAnalysis) } }
                }
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(apiResponse))
                });

            var openAiService = new OpenAiService(_httpClient);

            // Act
            var result = await openAiService.GetDiagnosisFromImage("test_base64_image");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedAnalysis.Summary, result.Summary);
        }

        [Fact]
        public async Task GetDiagnosisFromImage_ThrowsException_WhenApiCallFails()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var openAiService = new OpenAiService(_httpClient);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => openAiService.GetDiagnosisFromImage("test_base64_image"));
        }
    }
}
