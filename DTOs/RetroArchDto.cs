using System.Collections.Generic;

namespace BacklogBasement.DTOs
{
    public class RetroArchEntryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
    }

    public class RetroArchMatchRequestDto
    {
        public List<RetroArchEntryDto> Entries { get; set; } = new();
    }

    public class RetroArchMatchResultDto
    {
        public string InputName { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public GameSummaryDto? Game { get; set; }
    }
}
