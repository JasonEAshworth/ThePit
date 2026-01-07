using Microsoft.AspNetCore.Mvc;
using ThePitApi.Controllers;
using ThePitApi.Models;
using Xunit;

namespace ThePitApi.Tests;

public class DefaultControllerTests
{
    private readonly DefaultController _controller;

    public DefaultControllerTests()
    {
        _controller = new DefaultController();
    }

    [Fact]
    public void Get_ReturnsOkResult_WithExpectedMessage()
    {
        // Act
        var result = _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("You got it!", okResult.Value);
    }

    [Fact]
    public void Put_ReturnsOkResult_WithInputAppended()
    {
        // Arrange
        var input = " test input";

        // Act
        var result = _controller.Put(input);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("You put it right test input", okResult.Value);
    }

    [Fact]
    public void Put_WithEmptyString_ReturnsOkResult()
    {
        // Arrange
        var input = "";

        // Act
        var result = _controller.Put(input);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("You put it right", okResult.Value);
    }

    [Fact]
    public void Post_ReturnsOkResult_WithContactDetails()
    {
        // Arrange
        var contact = new ContactModel
        {
            Name = "John",
            Email = "john@example.com"
        };

        // Act
        var result = _controller.Post(contact);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Thank youJohnjohn@example.com", okResult.Value);
    }

    [Fact]
    public void Post_WithEmptyContact_ReturnsOkResult()
    {
        // Arrange
        var contact = new ContactModel();

        // Act
        var result = _controller.Post(contact);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Thank you", okResult.Value);
    }
}
