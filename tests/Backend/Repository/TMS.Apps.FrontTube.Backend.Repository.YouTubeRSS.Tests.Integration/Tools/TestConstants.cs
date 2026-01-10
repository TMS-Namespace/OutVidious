namespace TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS.Tests.Integration.Tools;

/// <summary>
/// Shared test data constants for integration tests.
/// Uses the same channel as the Invidious integration tests for consistency.
/// </summary>
internal static class TestConstants
{
    /// <summary>
    /// Test channel URL (same as Invidious tests for consistency).
    /// </summary>
    internal const string TestChannelUrl = "https://www.youtube.com/channel/UCN2cteWR8MCliIYAkxWSlkg";

    /// <summary>
    /// Test video URL (same as Invidious tests for consistency).
    /// </summary>
    internal const string TestVideoUrl = "https://www.youtube.com/watch?v=zH4E32vyJ5U";

    /// <summary>
    /// A well-known channel with guaranteed videos for testing (Google Developers).
    /// </summary>
    internal const string GoogleDevelopersChannelId = "UC_x5XG1OV2P6uZZ5FSM9Ttw";

    /// <summary>
    /// A channel ID that does not exist.
    /// </summary>
    internal const string NonExistentChannelId = "UCXXXXXXXXXXXXXXXXXXXXXXXXX";
}
