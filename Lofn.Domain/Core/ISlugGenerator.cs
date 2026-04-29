namespace Lofn.Domain.Core
{
    /// <summary>
    /// Generates URL-safe slugs from arbitrary text. Local replacement for the
    /// remote zTools StringClient — runs in-process so no characters can break
    /// the upstream HTTP routing (e.g. "/" in product names).
    /// </summary>
    public interface ISlugGenerator
    {
        string Generate(string text);
    }
}
