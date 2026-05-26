using System.Diagnostics;

namespace F0.Talks.Observability.Tests.Diagnostics;

[TestClass]
public class TagListTests
{
	private readonly TestContext _context;

	public TagListTests(TestContext context)
	{
		_context = context;
	}

	[TestMethod]
	public void Access_Internal_Tags_Property()
	{
		TagList tagList = new()
		{
			{ "one", 1 },
			{ "two", 2 },
			{ "three", 3 },
			{ "four", 4 },
			{ "five", 5 },
			{ "six", 6 },
			{ "seven", 7 },
			{ "eight", 8 },
		};

		ReadOnlySpan<KeyValuePair<string, object?>> tags = Accessors.GetTags(tagList);

		Assert.AreEqual(8, tags.Length);
	}
}

file static class Accessors
{
	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Tags")]
	internal static extern ReadOnlySpan<KeyValuePair<string, object?>> GetTags([UnscopedRef] in TagList tagList);
}
