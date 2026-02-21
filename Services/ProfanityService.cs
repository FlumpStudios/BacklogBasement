using BacklogBasement.Exceptions;
using ProfanityFilter;

namespace BacklogBasement.Services
{
    public class ProfanityService : IProfanityService
    {
        private readonly ProfanityFilter.ProfanityFilter _filter = new();

        public void AssertClean(string? text, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            var found = _filter.DetectAllProfanities(text);
            if (found.Count > 0)
                throw new BadRequestException($"{fieldName} contains inappropriate language.");
        }
    }
}
