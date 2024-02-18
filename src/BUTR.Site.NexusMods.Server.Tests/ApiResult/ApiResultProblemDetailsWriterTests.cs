using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using Moq;

using System.Reflection;

namespace BUTR.Site.NexusMods.Server.Tests.ApiResult;

public class ApiResultProblemDetailsWriterTests
{
    private ApiResult<object> ControllerMethod() => default!;
    
    [Fact]
    public void CanWrite_ReturnsTrue_WhenEndpointHasControllerActionDescriptor()
    {
        // Arrange
        var actionResultExecutorMock = new Mock<IActionResultExecutor<ObjectResult>>();
        var writer = new ApiResultProblemDetailsWriter(actionResultExecutorMock.Object);
        var httpContext = new DefaultHttpContext();
        var endpoint = new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(new ControllerActionDescriptor()
        {
            MethodInfo = typeof(ApiResultProblemDetailsWriterTests).GetMethod(nameof(ControllerMethod), BindingFlags.NonPublic | BindingFlags.Instance)!
        }), "test");
        httpContext.SetEndpoint(endpoint);
        var problemDetailsContext = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails()
        };

        // Act
        var result = writer.CanWrite(problemDetailsContext);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanWrite_ReturnsFalse_WhenEndpointDoesNotHaveControllerActionDescriptor()
    {
        // Arrange
        var actionResultExecutorMock = new Mock<IActionResultExecutor<ObjectResult>>();
        var writer = new ApiResultProblemDetailsWriter(actionResultExecutorMock.Object);
        var httpContext = new DefaultHttpContext();
        var endpoint = new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(), "test");
        httpContext.SetEndpoint(endpoint);
        var problemDetailsContext = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails()
        };

        // Act
        var result = writer.CanWrite(problemDetailsContext);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task WriteAsync_ExecutesObjectResult_WhenCalled()
    {
        // Arrange
        var actionResultExecutorMock = new Mock<IActionResultExecutor<ObjectResult>>();
        var writer = new ApiResultProblemDetailsWriter(actionResultExecutorMock.Object);
        var httpContext = new DefaultHttpContext();
        var endpoint = new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(new ControllerActionDescriptor()
        {
            MethodInfo = typeof(ApiResultProblemDetailsWriterTests).GetMethod(nameof(ControllerMethod), BindingFlags.NonPublic | BindingFlags.Instance)!
        }), "test");
        httpContext.SetEndpoint(endpoint);
        var problemDetailsContext = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails()
        };

        // Act
        await writer.WriteAsync(problemDetailsContext);

        // Assert
        actionResultExecutorMock.Verify(m => m.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<ObjectResult>()), Times.Once);
    }
}