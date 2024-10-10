using AutoMapper;
using Clutch.API.Extensions;
using Clutch.API.Models.Image;
using Clutch.API.Models.Registry;
using Clutch.API.Services.Interfaces;
using Clutch.API.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Moq;
using Clutch.API.Filters;

namespace Clutch.API.Tests
{
    /*
    [TestClass]
    public class ContainerImageControllerTests
    {
        private Mock<IContainerImageService> _mockService;
        private Mock<IMapper> _mockMapper;
        private object _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockService = new Mock<IContainerImageService>();
            _mockMapper = new Mock<IMapper>();
            _controller = new ContainerImageController(_mockService.Object, _mockMapper.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _mockService.VerifyAll();
            _mockMapper.VerifyAll();
            _mockService.Reset();
            _mockMapper.Reset();
        }

        #region GetContainerImage

        [TestMethod]
        public async Task GetContainerImage_Success_ReturnsOkWithContainerImageResponse1()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("test/image", "latest", RegistryType.Docker);
            var responseData = TestUtils.GetContainerImageResponseData("test/image", "latest", RegistryType.Docker);
            ContainerImageResponse expectedResponse = TestUtils.GetContainerImageResponse("test/image", "latest", RegistryType.Docker);
            _mockService.Setup(s => s.GetImageAsync(request, It.IsAny<string>()))
                .ReturnsAsync(responseData);
            _mockMapper.Setup(m => m.Map<ContainerImage>(responseData.ContainerImageModel))
                .Returns(expectedResponse.ContainerImage);
            _mockMapper.Setup(m => m.Map<RegistryManifest>(responseData.RegistryManifestModel))
                .Returns(expectedResponse.RegistryManifest);

            // Act
            var result = await _controller.GetContainerImage(request);

            // Assert
            var typeResult = result.Result as OkObjectResult;
            Assert.IsNotNull(typeResult);
            Assert.IsTrue(typeResult.StatusCode == 200);
            var response = typeResult.Value as ContainerImageResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.IsTrue(response.ContainerImage.Equals(expectedResponse.ContainerImage));
            Assert.IsTrue(response.RegistryManifest.Equals(expectedResponse.RegistryManifest));
            _mockService.Verify(s => s.GetImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        [TestMethod]
        public async Task GetContainerImage_Success_ReturnsOkWithContainerImageResponse2()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("joedward32/cs2", "latest", RegistryType.Docker);
            var responseData = TestUtils.GetContainerImageResponseData("joedward32/cs2", "latest", RegistryType.Docker);
            ContainerImageResponse expectedResponse = TestUtils.GetContainerImageResponse("joedward32/cs2", "latest", RegistryType.Docker);
            _mockService.Setup(s => s.GetImageAsync(request, It.IsAny<string>()))
                .ReturnsAsync(responseData);
            _mockMapper.Setup(m => m.Map<ContainerImage>(responseData.ContainerImageModel))
                .Returns(expectedResponse.ContainerImage);
            _mockMapper.Setup(m => m.Map<RegistryManifest>(responseData.RegistryManifestModel))
                .Returns(expectedResponse.RegistryManifest);

            // Act
            var result = await _controller.GetContainerImage(request);

            // Assert
            var typeResult = result.Result as OkObjectResult;
            Assert.IsNotNull(typeResult);
            Assert.IsTrue(typeResult.StatusCode == 200);
            var response = typeResult.Value as ContainerImageResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.IsTrue(response.ContainerImage.Equals(expectedResponse.ContainerImage));
            Assert.IsTrue(response.RegistryManifest.Equals(expectedResponse.RegistryManifest));
            _mockService.Verify(s => s.GetImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        [TestMethod]
        public async Task GetContainerImage_Success_ReturnsOkWithContainerImageResponse3()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            var responseData = TestUtils.GetContainerImageResponseData("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            ContainerImageResponse expectedResponse = TestUtils.GetContainerImageResponse("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            _mockService.Setup(s => s.GetImageAsync(request, It.IsAny<string>()))
                .ReturnsAsync(responseData);
            _mockMapper.Setup(m => m.Map<ContainerImage>(responseData.ContainerImageModel))
                .Returns(expectedResponse.ContainerImage);
            _mockMapper.Setup(m => m.Map<RegistryManifest>(responseData.RegistryManifestModel))
                .Returns(expectedResponse.RegistryManifest);

            // Act
            var result = await _controller.GetContainerImage(request);

            // Assert
            var typeResult = result.Result as OkObjectResult;
            Assert.IsNotNull(typeResult);
            Assert.IsTrue(typeResult.StatusCode == 200);
            var response = typeResult.Value as ContainerImageResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.IsTrue(response.ContainerImage.Equals(expectedResponse.ContainerImage));
            Assert.IsTrue(response.RegistryManifest.Equals(expectedResponse.RegistryManifest));
            _mockService.Verify(s => s.GetImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        [TestMethod]
        public async Task GetContainerImage_Success_ReturnsOkWithContainerImageResponse()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("test/image", "latest", RegistryType.Docker);
            var responseData = TestUtils.GetContainerImageResponseData("test/image", "latest", RegistryType.Docker);
            ContainerImageResponse expectedResponse = TestUtils.GetContainerImageResponse("test/image", "latest", RegistryType.Docker);
            _mockService.Setup(s => s.GetImageAsync(request, It.IsAny<string>()))
                .ReturnsAsync(responseData);
            _mockMapper.Setup(m => m.Map<ContainerImage>(responseData.ContainerImageModel))
                .Returns(expectedResponse.ContainerImage);
            _mockMapper.Setup(m => m.Map<RegistryManifest>(responseData.RegistryManifestModel))
                .Returns(expectedResponse.RegistryManifest);

            // Act
            var result = await _controller.GetContainerImage(request);

            // Assert
            var typeResult = result.Result as OkObjectResult;
            Assert.IsNotNull(typeResult);
            Assert.IsTrue(typeResult.StatusCode == 200);
            var response = typeResult.Value as ContainerImageResponse;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.IsTrue(response.ContainerImage.Equals(expectedResponse.ContainerImage));
            Assert.IsTrue(response.RegistryManifest.Equals(expectedResponse.RegistryManifest));
            _mockService.Verify(s => s.GetImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        [TestMethod]
        public async Task GetContainerImage_NotFound_ReturnsNotFound1()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("test/image", "latest", RegistryType.Docker);
            var responseData = new ContainerImageResponseData(false, ContainerImageModel.Null, RegistryManifestModel.Null);
            _mockService.Setup(s => s.GetImageAsync(request, It.IsAny<string>()))
                        .ReturnsAsync(responseData);

            // Act
            var result = await _controller.GetContainerImage(request);

            // Assert
            var notFoundResult = result.Result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.IsNotNull(notFoundResult.StatusCode == 404);
            _mockService.Verify(s => s.GetImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        [TestMethod]
        public async Task GetContainerImage_NotFound_ReturnsNotFound2()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("joedward32/cs2", "latest", RegistryType.Docker);
            var responseData = new ContainerImageResponseData(false, ContainerImageModel.Null, RegistryManifestModel.Null);
            _mockService.Setup(s => s.GetImageAsync(request, It.IsAny<string>()))
                        .ReturnsAsync(responseData);

            // Act
            var result = await _controller.GetContainerImage(request);

            // Assert
            var notFoundResult = result.Result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.IsNotNull(notFoundResult.StatusCode == 404);
            _mockService.Verify(s => s.GetImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        [TestMethod]
        public async Task GetContainerImage_NotFound_ReturnsNotFound3()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            var responseData = new ContainerImageResponseData(false, ContainerImageModel.Null, RegistryManifestModel.Null);
            _mockService.Setup(s => s.GetImageAsync(request, It.IsAny<string>()))
                        .ReturnsAsync(responseData);

            // Act
            var result = await _controller.GetContainerImage(request);

            // Assert
            var notFoundResult = result.Result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.IsNotNull(notFoundResult.StatusCode == 404);
            _mockService.Verify(s => s.GetImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        #endregion
        #region SetContainerImage

        [TestMethod]
        public async Task SetContainerImage_Success_ReturnsOk1()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("test/image", "latest", RegistryType.Docker);
            _mockService.Setup(s => s.SetImageAsync(request, "1.0.0.0")).Returns(Task.FromResult(true));

            // Act
            var result = await _controller.SetContainerImage(request);

            // Assert
            var typeResult = result as OkResult;
            Assert.IsNotNull(typeResult);
            Assert.IsTrue(typeResult.StatusCode == 200);
            _mockService.Verify(s => s.SetImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        [TestMethod]
        public async Task SetContainerImage_Success_ReturnsOk2()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("joedwards32/cs2", "latest", RegistryType.Docker);
            _mockService.Setup(s => s.SetImageAsync(request, "1.0.0.0")).Returns(Task.FromResult(true));

            // Act
            var result = await _controller.SetContainerImage(request);

            // Assert
            var typeResult = result as OkResult;
            Assert.IsNotNull(typeResult);
            Assert.IsTrue(typeResult.StatusCode == 200);
            _mockService.Verify(s => s.SetImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        [TestMethod]
        public async Task SetContainerImage_Success_ReturnsOk3()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            _mockService.Setup(s => s.SetImageAsync(request, "1.0.0.0")).Returns(Task.FromResult(true));

            // Act
            var result = await _controller.SetContainerImage(request);

            // Assert
            var typeResult = result as OkResult;
            Assert.IsNotNull(typeResult);
            Assert.IsTrue(typeResult.StatusCode == 200);
            _mockService.Verify(s => s.SetImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        [TestMethod]
        public async Task SetContainerImage_Failure_ReturnsNoContent1()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("test/image", "latest", RegistryType.Docker);
            _mockService.Setup(s => s.SetImageAsync(request, "1.0.0.0")).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.SetContainerImage(request);

            // Assert
            var typeResult = result as NoContentResult;
            Assert.IsNotNull(typeResult);
            Assert.IsTrue(typeResult.StatusCode == 204);
            _mockService.Verify(s => s.SetImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        [TestMethod]
        public async Task SetContainerImage_Failure_ReturnsNoContent2()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("joedwards32/cs2", "latest", RegistryType.Docker);
            _mockService.Setup(s => s.SetImageAsync(request, "1.0.0.0")).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.SetContainerImage(request);

            // Assert
            var typeResult = result as NoContentResult;
            Assert.IsNotNull(typeResult);
            Assert.IsTrue(typeResult.StatusCode == 204);
            _mockService.Verify(s => s.SetImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        [TestMethod]
        public async Task SetContainerImage_Failure_ReturnsNoContent3()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            _mockService.Setup(s => s.SetImageAsync(request, "1.0.0.0")).Returns(Task.FromResult(true));

            // Act
            var result = await _controller.SetContainerImage(request);

            // Assert
            var typeResult = result as OkResult;
            Assert.IsNotNull(typeResult);
            Assert.IsTrue(typeResult.StatusCode == 200);
            _mockService.Verify(s => s.SetImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        #endregion
        #region DeleteContainerImage

        [TestMethod]
        public async Task DeleteContainerImage_Success_ReturnsOk1()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("test/image", "latest", RegistryType.Docker);
            _mockService.Setup(s => s.DeleteImageAsync(request, "1.0.0.0")).Returns(Task.FromResult(true));

            // Act
            var result = await _controller.DeleteContainerImage(request);

            // Assert
            var typeResult = result as OkResult;
            Assert.IsNotNull(typeResult);
            Assert.IsTrue(typeResult.StatusCode == 200);
            _mockService.Verify(s => s.DeleteImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        [TestMethod]
        public async Task DeleteContainerImage_Success_ReturnsOk2()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("joedwards32/cs2", "latest", RegistryType.Docker);
            _mockService.Setup(s => s.DeleteImageAsync(request, "1.0.0.0")).Returns(Task.FromResult(true));

            // Act
            var result = await _controller.DeleteContainerImage(request);

            // Assert
            var typeResult = result as OkResult;
            Assert.IsNotNull(typeResult);
            Assert.IsTrue(typeResult.StatusCode == 200);
            _mockService.Verify(s => s.DeleteImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        [TestMethod]
        public async Task DeleteContainerImage_Success_ReturnsOk3()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            _mockService.Setup(s => s.DeleteImageAsync(request, "1.0.0.0")).Returns(Task.FromResult(true));

            // Act
            var result = await _controller.DeleteContainerImage(request);

            // Assert
            var typeResult = result as OkResult;
            Assert.IsNotNull(typeResult);
            Assert.IsTrue(typeResult.StatusCode == 200);
            _mockService.Verify(s => s.DeleteImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        [TestMethod]
        public async Task DeleteContainerImage_Failure_ReturnsNoContent1()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("test/image", "latest", RegistryType.Docker);
            _mockService.Setup(s => s.DeleteImageAsync(request, "1.0.0.0")).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.DeleteContainerImage(request);

            // Assert
            var typeResult = result as NoContentResult;
            Assert.IsNotNull(typeResult);
            Assert.IsTrue(typeResult.StatusCode == 204);
            _mockService.Verify(s => s.DeleteImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        [TestMethod]
        public async Task DeleteContainerImage_Failure_ReturnsNoContent2()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("joedwards32/cs2", "latest", RegistryType.Docker);
            _mockService.Setup(s => s.DeleteImageAsync(request, "1.0.0.0")).Returns(Task.FromResult(false));

            // Act
            var result = await _controller.DeleteContainerImage(request);

            // Assert
            var typeResult = result as NoContentResult;
            Assert.IsNotNull(typeResult);
            Assert.IsTrue(typeResult.StatusCode == 204);
            _mockService.Verify(s => s.DeleteImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        [TestMethod]
        public async Task DeleteContainerImage_Failure_ReturnsNoContent3()
        {
            // Arrange
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            _mockService.Setup(s => s.DeleteImageAsync(request, "1.0.0.0")).Returns(Task.FromResult(true));

            // Act
            var result = await _controller.DeleteContainerImage(request);

            // Assert
            var typeResult = result as OkResult;
            Assert.IsNotNull(typeResult);
            Assert.IsTrue(typeResult.StatusCode == 200);
            _mockService.Verify(s => s.DeleteImageAsync(request, "1.0.0.0"), Times.Exactly(1));
        }

        #endregion
        #region Special Cases

        [TestMethod]
        public void ContainerImage_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = TestUtils.GetInvalidContainerImageRequest();

            // Create ActionExecutingContext to simulate filter execution
            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            var actionExecutingContext = new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), _controller);
            actionExecutingContext.ActionArguments["request"] = request;

            // Act
            var validateRequestFilter = new ValidateRequest();
            validateRequestFilter.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.IsInstanceOfType(actionExecutingContext.Result, typeof(BadRequestObjectResult));
            var badRequestResult = actionExecutingContext.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.IsTrue(badRequestResult.StatusCode == 400);
        }

        [TestMethod]
        public async Task ContainerImage_ServiceThrowsTimeoutException_ReturnsGatewayTimeout()
        {
            // Arrange
            // Create ActionExecutingContext to simulate filter execution
            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            var exceptionContext = new ExceptionContext(actionContext, [])
            {
                Exception = new TimeoutException()
            };

            // Act
            var handleExceptionsFilter = new HandleExceptions();
            await handleExceptionsFilter.OnExceptionAsync(exceptionContext);

            // Assert
            Assert.IsInstanceOfType(exceptionContext.Result, typeof(ObjectResult));
            var timeoutRequestResult = exceptionContext.Result as ObjectResult;
            Assert.IsNotNull(timeoutRequestResult);
            Assert.IsTrue(timeoutRequestResult.StatusCode == 504);
        }

        [TestMethod]
        public async Task ContainerImage_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            // Create ActionExecutingContext to simulate filter execution
            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            var exceptionContext = new ExceptionContext(actionContext, [])
            {
                Exception = new Exception()
            };

            // Act
            var handleExceptionsFilter = new HandleExceptions();
            await handleExceptionsFilter.OnExceptionAsync(exceptionContext);

            // Assert
            Assert.IsInstanceOfType(exceptionContext.Result, typeof(ObjectResult));
            var internalErrorRequestResult = exceptionContext.Result as ObjectResult;
            Assert.IsNotNull(internalErrorRequestResult);
            Assert.IsTrue(internalErrorRequestResult.StatusCode == 500);
        }

        #endregion
    }
    */
}
