using MyTypes;

namespace SourceGenerationMain.EventSubscribing;

public class SubscriberClass
{
	public SubscriberClass()
	{
		this.SubscribeToEvents();
	}

	[EventReceiver]
	internal void Subscribe(EventTwo @event)
	{
		Console.WriteLine($"Event received: {@event}");
	}

	[EventReceiver]
	internal void SubscribeToOther(EventOne @event)
	{
		Console.WriteLine($"Event received: {@event}");
	}
}

