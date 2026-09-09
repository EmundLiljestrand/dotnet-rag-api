namespace RagApi.Services;

public interface IAnswerGenerator
{
    Task<string> GenerateAsync(
        string question,
        IReadOnlyList<string> context,
        CancellationToken cancellationToken = default);
}
