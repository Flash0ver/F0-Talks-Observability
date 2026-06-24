using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace F0.Talks.Observability.Benchmarks.Diagnostics;

public class TagListBenchmarks
{
	private static readonly string s_namespace = typeof(TagListBenchmarks).Namespace ?? "Benchmark";
	private static readonly AssemblyName s_assemblyName = typeof(TagListBenchmarks).Assembly.GetName();

	private Meter _meter = null!;
	private Counter<int> _instrument = null!;
	private KeyValuePair<string, object?>[] _measurement = null!;

	private TagList _tagList;

	[GlobalSetup]
	public void Setup()
	{
		_meter = new Meter(s_namespace, s_assemblyName.Version?.ToString(),
			[KeyValuePair.Create<string, object?>("meter.key", "meter-value")]);
		_instrument = _meter.CreateCounter<int>("benchmark.instrument", "unit", "Description.",
			[KeyValuePair.Create<string, object?>("instrument.key", "instrument-value")]);
		_measurement = [KeyValuePair.Create<string, object?>("measurement.key", "measurement-value")];

		_tagList = new TagList
		{
			{ "key.one", "value-1" },
			{ "key.two", "value-2" },
		};
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		_meter.Dispose();

		CreateTagList_ForEach_Add().AssertCount(3);
		CreateTagList_CollectionExpression_SpreadElement().AssertCount(3);

		IterateTagList_Enumerator().AssertEquals(new KeyValuePair<string, object?>("key.two", "value-2"));
		IterateTagList_Indexer().AssertEquals(new KeyValuePair<string, object?>("key.two", "value-2"));
	}

	[Benchmark]
	public TagList CreateTagList_ForEach_Add()
	{
		ReadOnlySpan<KeyValuePair<string, object?>> tags = _measurement.AsSpan();

		TagList tagList = [];

		if (_instrument.Meter.Tags is not null)
		{
			foreach (KeyValuePair<string, object?> tag in _instrument.Meter.Tags)
			{
				tagList.Add(tag);
			}
		}

		if (_instrument.Tags is not null)
		{
			foreach (KeyValuePair<string, object?> tag in _instrument.Tags)
			{
				tagList.Add(tag);
			}
		}

		if (!tags.IsEmpty)
		{
			foreach (KeyValuePair<string, object?> tag in tags)
			{
				tagList.Add(tag);
			}
		}

		return tagList;
	}

	[Benchmark]
	public TagList CreateTagList_CollectionExpression_SpreadElement()
	{
		ReadOnlySpan<KeyValuePair<string, object?>> tags = _measurement.AsSpan();

		TagList tagList = [
			.._instrument.Meter.Tags ?? [],
			.._instrument.Tags ?? [],
			..tags,
		];

		return tagList;
	}

	[Benchmark]
	public KeyValuePair<string, object?> IterateTagList_Enumerator()
	{
		KeyValuePair<string, object?> last = default;

		foreach (KeyValuePair<string, object?> tag in _tagList)
		{
			last = tag;
		}

		return last;
	}

	[Benchmark]
	public KeyValuePair<string, object?> IterateTagList_Indexer()
	{
		KeyValuePair<string, object?> last = default;

		for (int index = 0; index < _tagList.Count; index++)
		{
			KeyValuePair<string, object?> tag = _tagList[index];
			last = tag;
		}

		return last;
	}
}
