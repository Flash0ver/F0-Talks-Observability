namespace F0.Talks.Observability.Benchmarks.Assertions;

public sealed class AssertionException : Exception
{
	public AssertionException(string? message)
		: base(message)
	{
	}
}

internal static class AssertionExceptionExtensions
{
	extension(AssertionException)
	{
		[DoesNotReturn]
		internal static void Throw<T>(string assertion, T expected, T actual)
		{
			throw new AssertionException($"Expected {assertion}: {expected}; but actually: {actual}");
		}
	}
}
