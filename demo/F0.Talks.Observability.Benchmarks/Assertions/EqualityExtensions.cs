namespace F0.Talks.Observability.Benchmarks.Assertions;

internal static class EqualityExtensions
{
	extension<T>(T expected)
	{
		internal void AssertEquals(T actual)
		{
			if (!EqualityComparer<T>.Default.Equals(expected, actual))
			{
				AssertionException.Throw(nameof(IEqualityComparer<>.Equals), expected, actual);
			}
		}
	}
}
