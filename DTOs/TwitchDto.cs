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
}
