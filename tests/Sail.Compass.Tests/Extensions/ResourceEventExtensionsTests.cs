using Sail.Compass.Extensions;
using Sail.Compass.Observers;

namespace Sail.Compass.Tests.Extensions;

public class ResourceEventExtensionsTests
{
    [Fact]
    public void ToResourceEvent_WithCreatedEvent_ReturnsResourceEvent()
    {
        // Arrange
        var obj = "test-resource";

        // Act
        var result = obj.ToResourceEvent(EventType.Created);

        // Assert
        Assert.Equal(EventType.Created, result.EventType);
        Assert.Equal("test-resource", result.Value);
        Assert.Null(result.OldValue);
    }

    [Fact]
    public void ToResourceEvent_WithUpdatedEvent_ReturnsResourceEventWithOldValue()
    {
        // Arrange
        var obj = "new-value";
        var oldValue = "old-value";

        // Act
        var result = obj.ToResourceEvent(EventType.Updated, oldValue);

        // Assert
        Assert.Equal(EventType.Updated, result.EventType);
        Assert.Equal("new-value", result.Value);
        Assert.Equal("old-value", result.OldValue);
    }

    [Fact]
    public void ToResourceEvent_WithDeletedEvent_UsesObjectAsOldValue()
    {
        // Arrange
        var obj = "deleted-resource";

        // Act
        var result = obj.ToResourceEvent(EventType.Deleted);

        // Assert
        Assert.Equal(EventType.Deleted, result.EventType);
        Assert.Equal("deleted-resource", result.Value);
        Assert.Equal("deleted-resource", result.OldValue);
    }

    [Fact]
    public void ToResourceEvent_WithDeletedEventAndOldValue_UsesProvidedOldValue()
    {
        // Arrange
        var obj = "deleted-resource";
        var oldValue = "previous-value";

        // Act
        var result = obj.ToResourceEvent(EventType.Deleted, oldValue);

        // Assert
        Assert.Equal(EventType.Deleted, result.EventType);
        Assert.Equal("deleted-resource", result.Value);
        Assert.Equal("previous-value", result.OldValue);
    }

    [Fact]
    public void ToResourceEvent_WithListEvent_ReturnsResourceEvent()
    {
        // Arrange
        var obj = "list-resource";

        // Act
        var result = obj.ToResourceEvent(EventType.List);

        // Assert
        Assert.Equal(EventType.List, result.EventType);
        Assert.Equal("list-resource", result.Value);
        Assert.Null(result.OldValue);
    }

    [Fact]
    public void ToResourceEvent_WithComplexObject_ReturnsResourceEvent()
    {
        // Arrange
        var obj = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = obj.ToResourceEvent(EventType.Created);

        // Assert
        Assert.Equal(EventType.Created, result.EventType);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("Test", result.Value.Name);
    }

    [Fact]
    public void ToResourceEvent_WithComplexObjectAndOldValue_ReturnsResourceEventWithOldValue()
    {
        // Arrange
        var obj = new TestData { Id = 1, Name = "New" };
        var oldValue = new TestData { Id = 1, Name = "Old" };

        // Act
        var result = obj.ToResourceEvent(EventType.Updated, oldValue);

        // Assert
        Assert.Equal(EventType.Updated, result.EventType);
        Assert.Equal("New", result.Value.Name);
        Assert.Equal("Old", result.OldValue!.Name);
    }

    [Fact]
    public void ToResourceEvent_WithNullableType_ReturnsResourceEvent()
    {
        // Arrange
        int? obj = 42;

        // Act
        var result = obj.ToResourceEvent(EventType.Created);

        // Assert
        Assert.Equal(EventType.Created, result.EventType);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ToResourceEvent_WithNullObject_ReturnsResourceEvent()
    {
        // Arrange
        string? obj = null;

        // Act
        var result = obj.ToResourceEvent(EventType.Deleted);

        // Assert
        Assert.Equal(EventType.Deleted, result.EventType);
        Assert.Null(result.Value);
        Assert.Null(result.OldValue);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

