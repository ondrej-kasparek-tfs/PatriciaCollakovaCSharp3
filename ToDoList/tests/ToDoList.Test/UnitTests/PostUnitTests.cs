namespace ToDoList.Test;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ToDoList.Domain.DTOs;
using ToDoList.Domain.Models;
using ToDoList.Persistence.Repositories;
using ToDoList.WebApi.Controllers;

public class PostUnitTests
{

    [Fact]
    public async Task Post_CreateValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var repositoryMock = Substitute.For<IRepository<ToDoItem>>();
        var controller = new ToDoItemsController(repositoryMock);

        var request = new ToDoItemsCreateRequestDto(
            Name: "Name",
            Description: "Description",
            IsCompleted: false,
            Category: "Work"
        );

        // Act
        var result = await controller.CreateAsync(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var value = Assert.IsType<ToDoItemGetResponseDto>(createdResult.Value);

        Assert.Equal(request.Description, value.Description);
        Assert.Equal(request.IsCompleted, value.IsCompleted);
        Assert.Equal(request.Name, value.Name);
        Assert.Equal(request.Category, value.Category);

        await repositoryMock.Received(1).CreateAsync(Arg.Any<ToDoItem>());
    }



    [Fact]
    public async Task Post_CreateUnhandledException_ReturnsInternalServerError()
    {
        // Arrange
        var repositoryMock = Substitute.For<IRepository<ToDoItem>>();
        repositoryMock.When(repo => repo.CreateAsync(Arg.Any<ToDoItem>())).Throw(new Exception("Unhandled exception"));

        var controller = new ToDoItemsController(repositoryMock);

        var request = new ToDoItemsCreateRequestDto(
            Name: "Name",
            Description: "Description",
            IsCompleted: false,
            Category: "Work"
        );

        // Act
        var result = await controller.CreateAsync(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Unhandled exception", problemDetails.Detail);

        await repositoryMock.Received(1).CreateAsync(Arg.Any<ToDoItem>());
    }

}

