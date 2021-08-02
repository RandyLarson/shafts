using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct TimedMessageEvent
{
	[Multiline]
	public string MessageContent;

	public float TimeOfEvent;
	public bool IsRelativeToLast;

	public UnityEvent TriggeredEvent;

}

public class TimedEventCollection : MonoBehaviour
{
	public TimedMessageEvent[] TimeSeries;

	private int EventIndex { get; set; } = 0;
	private float NextEventTime = 0;

	void Start()
	{
		if (TimeSeries?.Length > 0)
		{
			EventIndex = 0;
			DetermineNextEventTime();
		}
	}

	void Update()
	{
		if (TimeSeries == null || TimeSeries.Length == 0 || EventIndex >= TimeSeries.Length)
		{
			GameObject.Destroy(gameObject);
			return;
		}

		if ( Time.time >= NextEventTime)
		{
			if ( TimeSeries[EventIndex].MessageContent?.Length > 0 )
			{
				MessageContoller.AddMessage(TimeSeries[EventIndex].MessageContent);
			}

			TimeSeries[EventIndex].TriggeredEvent?.Invoke();

			EventIndex++;
			DetermineNextEventTime();
		}
	}

	void DetermineNextEventTime()
	{
		if (EventIndex < TimeSeries.Length)
		{
			NextEventTime = TimeSeries[EventIndex].TimeOfEvent;
			if (TimeSeries[EventIndex].IsRelativeToLast)
				NextEventTime += Time.time;
		}
	}

}
