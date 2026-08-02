namespace Compass.Tests.Shared;

public static class TestConstants
{
    // O mesmo ID que você usa de fallback no header "X-User-Id" no Controller
    public static readonly Guid DefaultUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DefaultProjectId = Guid.Parse("22222222-2222-2222-2222-222222222222");
}