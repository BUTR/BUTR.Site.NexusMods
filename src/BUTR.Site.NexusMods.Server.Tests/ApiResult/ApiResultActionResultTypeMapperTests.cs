using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using Moq;

namespace BUTR.Site.NexusMods.Server.Tests.ApiResult;

public class ApiResultActionResultTypeMapperTests
{
    [Fact]
    public void GetResultDataType_ReturnsApiResultModelType_WhenReturnTypeIsApiResult()
    {
        // Arrange
        var actionResultTypeMapperMock = new Mock<IActionResultTypeMapper>();
        var apiResultActionResultTypeMapper = new ApiResultActionResultTypeMapper(actionResultTypeMapperMock.Object);

        // Act
        var result = apiResultActionResultTypeMapper.GetResultDataType(typeof(Utils.Http.ApiResults.ApiResult));

        // Assert
        Assert.Equal(typeof(ApiResultModel), result);
    }

    [Fact]
    public void GetResultDataType_ReturnsApiResultModelGenericType_WhenReturnTypeIsApiResultGeneric()
    {
        // Arrange
        var actionResultTypeMapperMock = new Mock<IActionResultTypeMapper>();
        var apiResultActionResultTypeMapper = new ApiResultActionResultTypeMapper(actionResultTypeMapperMock.Object);

        // Act
        var result = apiResultActionResultTypeMapper.GetResultDataType(typeof(ApiResult<int>));

        // Assert
        Assert.Equal(typeof(ApiResultModel<int>), result);
    }

    [Fact]
    public void GetResultDataType_ReturnsImplementationResult_WhenReturnTypeIsNotApiResult()
    {
        // Arrange
        var actionResultTypeMapperMock = new Mock<IActionResultTypeMapper>();
        actionResultTypeMapperMock.Setup(m => m.GetResultDataType(It.IsAny<Type>())).Returns(typeof(string));
        var apiResultActionResultTypeMapper = new ApiResultActionResultTypeMapper(actionResultTypeMapperMock.Object);

        // Act
        var result = apiResultActionResultTypeMapper.GetResultDataType(typeof(ObjectResult));

        // Assert
        Assert.Equal(typeof(string), result);
    }
}