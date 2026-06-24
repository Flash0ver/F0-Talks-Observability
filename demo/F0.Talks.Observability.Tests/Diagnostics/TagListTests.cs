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
	public void SizeOf_Type()
	{
		int expected = Environment.Is64BitProcess ? 144 : 72;

		int size = Unsafe.SizeOf<TagList>();

		Assert.AreEqual(expected, size);
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

	[TestMethod]
	public void UpToEightTagsStoredInline_MoreThanEightTagsAllocatedAsArray()
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
		};

		ReadOnlySpan<KeyValuePair<string, object?>> inlineArray = Accessors.GetTags(tagList);

		Assert.AreEqual(1, inlineArray[0].Value);

		tagList.Add("eight", 8);
		tagList[0] = new KeyValuePair<string, object?>(tagList[0].Key, 2);
		Assert.AreEqual(2, inlineArray[0].Value);

		tagList.Add("nine", 9);
		tagList[0] = new KeyValuePair<string, object?>(tagList[0].Key, 3);
		Assert.AreEqual(2, inlineArray[0].Value);

		Assert.AreEqual(7, inlineArray.Length);
		Assert.AreEqual(9, tagList.Count);
	}
}

file static class Accessors
{
	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Tags")]
	internal static extern ReadOnlySpan<KeyValuePair<string, object?>> GetTags([UnscopedRef] in TagList tagList);
}
