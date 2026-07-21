using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MapIdeaHub.BirSign.SharedKernel.Dtos
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "eventType")]
    [JsonDerivedType(typeof(DepartmentCreatedEvent), typeDiscriminator: "DepartmentCreated")]
    [JsonDerivedType(typeof(DepartmentUpdatedEvent), typeDiscriminator: "DepartmentUpdated")]
    [JsonDerivedType(typeof(DepartmentDeletedEvent), typeDiscriminator: "DepartmentDeleted")]
    [JsonDerivedType(typeof(PositionCreatedEvent), typeDiscriminator: "PositionCreated")]
    [JsonDerivedType(typeof(PositionUpdatedEvent), typeDiscriminator: "PositionUpdated")]
    [JsonDerivedType(typeof(PositionDeletedEvent), typeDiscriminator: "PositionDeleted")]
    [JsonDerivedType(typeof(UserPositionCreatedEvent), typeDiscriminator: "UserPositionCreated")]
    [JsonDerivedType(typeof(UserPositionUpdatedEvent), typeDiscriminator: "UserPositionUpdated")]
    [JsonDerivedType(typeof(UserPositionDeletedEvent), typeDiscriminator: "UserPositionDeleted")]
    [JsonDerivedType(typeof(UserProfileUpdatedEvent), typeDiscriminator: "UserProfileUpdated")]
    public abstract class WebhookEvent
    {
        public string EventType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class DepartmentCreatedEvent : WebhookEvent
    {
        public DepartmentCreatedEvent() => EventType = "DepartmentCreated";
        public DepartmentApiDto Department { get; set; } = new DepartmentApiDto();
    }

    public class DepartmentUpdatedEvent : WebhookEvent
    {
        public DepartmentUpdatedEvent() => EventType = "DepartmentUpdated";
        public DepartmentApiDto Department { get; set; } = new DepartmentApiDto();
    }

    public class DepartmentDeletedEvent : WebhookEvent
    {
        public DepartmentDeletedEvent() => EventType = "DepartmentDeleted";
        public Guid DepartmentId { get; set; }
    }

    public class PositionCreatedEvent : WebhookEvent
    {
        public PositionCreatedEvent() => EventType = "PositionCreated";
        public PositionApiDto Position { get; set; } = new PositionApiDto();
    }

    public class PositionUpdatedEvent : WebhookEvent
    {
        public PositionUpdatedEvent() => EventType = "PositionUpdated";
        public PositionApiDto Position { get; set; } = new PositionApiDto();
    }

    public class PositionDeletedEvent : WebhookEvent
    {
        public PositionDeletedEvent() => EventType = "PositionDeleted";
        public Guid PositionId { get; set; }
    }

    public class UserPositionCreatedEvent : WebhookEvent
    {
        public UserPositionCreatedEvent() => EventType = "UserPositionCreated";
        public UserPositionApiDto UserPosition { get; set; } = new UserPositionApiDto();
    }

    public class UserPositionUpdatedEvent : WebhookEvent
    {
        public UserPositionUpdatedEvent() => EventType = "UserPositionUpdated";
        public UserPositionApiDto UserPosition { get; set; } = new UserPositionApiDto();
    }

    public class UserPositionDeletedEvent : WebhookEvent
    {
        public UserPositionDeletedEvent() => EventType = "UserPositionDeleted";
        public Guid UserPositionId { get; set; }
        public Guid PositionId { get; set; }
    }

    public class UserProfileUpdatedEvent : WebhookEvent
    {
        public UserProfileUpdatedEvent() => EventType = "UserProfileUpdated";
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
