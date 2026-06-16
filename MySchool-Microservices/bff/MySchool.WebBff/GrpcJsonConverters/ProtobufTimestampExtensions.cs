using Google.Protobuf.WellKnownTypes;

namespace MySchool.WebBff.GrpcJsonConverters;

public static class ProtobufTimestampExtensions
{
    public static DateTime ToUtcDateTime(this Timestamp timestamp) =>
        DateTime.SpecifyKind(timestamp.ToDateTime(), DateTimeKind.Utc);
}
