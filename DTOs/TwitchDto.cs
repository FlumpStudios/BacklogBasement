namespace BacklogBasement.DTOs
{
    public class TwitchStreamDto
    {
        public string Login { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int ViewerCount { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
    }

    public class TwitchLiveDto
    {
        public bool IsLive { get; set; }
        public string? StreamTitle { get; set; }
        public string? GameName { get; set; }
        public long? IgdbGameId { get; set; }
        public int ViewerCount { get; set; }
        public string? TwitchLogin { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool UpdatedPlayingStatus { get; set; }
    }

    public class TwitchImportResultDto
    {
        public int Total { get; set; }
        public int Imported { get; set; }
        public int Skipped { get; set; }
        public List<TwitchImportedGameDto> ImportedGames { get; set; } = new();
    }

    public class TwitchImportedGameDto
    {
        public string Name { get; set; } = string.Empty;
        public long IgdbId { get; set; }
        public int StreamedMinutes { get; set; }
    }
}
