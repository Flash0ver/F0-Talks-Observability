namespace F0.Talks.Observability.Benchmarks.Assertions;

internal static class CollectionsExtensions
{
	extension<T>(ICollection<T> collection)
	{
		internal void AssertCount(int count)
		{
			if (collection.Count != count)
			{
				AssertionException.Throw(nameof(collection.Count), count, collection.Count);
			}
		}
	}
}
