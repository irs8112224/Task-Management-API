using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using Xunit;

namespace TaskManagement.Application.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repositoryMock;
    private readonly Mock<IValidator<CreateTaskRequest>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateTaskRequest>> _updateValidatorMock;

    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _repositoryMock = new Mock<ITaskRepository>();
        _createValidatorMock =
            new Mock<IValidator<CreateTaskRequest>>();

        _updateValidatorMock =
            new Mock<IValidator<UpdateTaskRequest>>();

        _service = new TaskService(
            _repositoryMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTask()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Description = "Description",
            Status = TaskItemStatus.Pending,
            Priority = TaskPriority.Low
        };

        _createValidatorMock
            .Setup(x => x.ValidateAsync(
                request,
                default))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _service.CreateAsync(
            request,
            "tenant-1");

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        result.TenantId.Should().Be("tenant-1");

        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<TaskItem>()),
            Times.Once);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateTaskRequest();

        var failures = new List<ValidationFailure>
        {
            new("Title", "Title is required")
        };

        _createValidatorMock
            .Setup(x => x.ValidateAsync(
                request,
                default))
            .ReturnsAsync(new ValidationResult(failures));

        // Act
        Func<Task> action = async () =>
            await _service.CreateAsync(request);

        // Assert
        await action.Should()
            .ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        var existingTask = new TaskItem
        {
            Id = taskId,
            Title = "Old Title",
            Status = TaskItemStatus.Pending,
            Priority = TaskPriority.Low,
            TenantId = "tenant-1"
        };

        var request = new UpdateTaskRequest
        {
            Title = "Updated Title"
        };

        _updateValidatorMock
            .Setup(x => x.ValidateAsync(
                request,
                default))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                taskId,
                "tenant-1"))
            .ReturnsAsync(existingTask);

        // Act
        await _service.UpdateAsync(
            taskId,
            request,
            "tenant-1");

        // Assert
        existingTask.Title.Should().Be("Updated Title");

        _repositoryMock.Verify(
            x => x.UpdateAsync(existingTask),
            Times.Once);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenTaskNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        var request = new UpdateTaskRequest
        {
            Title = "Updated"
        };

        _updateValidatorMock
            .Setup(x => x.ValidateAsync(
                request,
                default))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                taskId,
                "tenant-1"))
            .ReturnsAsync((TaskItem?)null);

        // Act
        Func<Task> action = async () =>
            await _service.UpdateAsync(
                taskId,
                request,
                "tenant-1");

        // Assert
        await action.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Task not found.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenTaskCancelled()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        var task = new TaskItem
        {
            Id = taskId,
            Status = TaskItemStatus.Cancelled,
            TenantId = "tenant-1"
        };

        var request = new UpdateTaskRequest();

        _updateValidatorMock
            .Setup(x => x.ValidateAsync(
                request,
                default))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                taskId,
                "tenant-1"))
            .ReturnsAsync(task);

        // Act
        Func<Task> action = async () =>
            await _service.UpdateAsync(
                taskId,
                request,
                "tenant-1");

        // Assert
        await action.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Cancelled tasks are read-only.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCompletedToInProgress()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        var task = new TaskItem
        {
            Id = taskId,
            Status = TaskItemStatus.Completed,
            TenantId = "tenant-1"
        };

        var request = new UpdateTaskRequest
        {
            Status = TaskItemStatus.InProgress
        };

        _updateValidatorMock
            .Setup(x => x.ValidateAsync(
                request,
                default))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                taskId,
                "tenant-1"))
            .ReturnsAsync(task);

        // Act
        Func<Task> action = async () =>
            await _service.UpdateAsync(
                taskId,
                request,
                "tenant-1");

        // Assert
        await action.Should()
            .ThrowAsync<Exception>()
            .WithMessage(
                "Status cannot move from Completed to InProgress.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        var task = new TaskItem
        {
            Id = taskId,
            Status = TaskItemStatus.Pending,
            TenantId = "tenant-1",
            IsDeleted = false
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                taskId,
                "tenant-1"))
            .ReturnsAsync(task);

        // Act
        await _service.DeleteAsync(
            taskId,
            "tenant-1");

        // Assert
        task.IsDeleted.Should().BeTrue();

        _repositoryMock.Verify(
            x => x.UpdateAsync(task),
            Times.Once);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenCompleted()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        var task = new TaskItem
        {
            Id = taskId,
            Status = TaskItemStatus.Completed,
            TenantId = "tenant-1"
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                taskId,
                "tenant-1"))
            .ReturnsAsync(task);

        // Act
        Func<Task> action = async () =>
            await _service.DeleteAsync(
                taskId,
                "tenant-1");

        // Assert
        await action.Should()
            .ThrowAsync<Exception>()
            .WithMessage(
                "Completed tasks cannot be deleted.");
    }
}