namespace BacklogBasement.DTOs
{
    public class XpInfoDto
    {
        public int Level { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string NextLevelName { get; set; } = string.Empty;
        public int XpTotal { get; set; }
        public int XpForCurrentLevel { get; set; }
        public int XpForNextLevel { get; set; }
        public int XpIntoCurrentLevel { get; set; }
        public int XpNeededForNextLevel { get; set; }
        public double ProgressPercent { get; set; }
        public bool IsMaxLevel { get; set; }
    }
}
