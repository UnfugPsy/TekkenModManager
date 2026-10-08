namespace ModManager.Models
{
    internal sealed record ModMetadataDto(
        string Version,
        string Category,
        string Description,
        string Author);
}
