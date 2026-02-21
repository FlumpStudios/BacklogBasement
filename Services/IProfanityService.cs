namespace BacklogBasement.Services
{
    public interface IProfanityService
    {
        void AssertClean(string? text, string fieldName);
    }
}
