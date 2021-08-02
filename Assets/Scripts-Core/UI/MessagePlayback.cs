using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public delegate void NewPlayerMessageDelegate(string messageContent);

public static class MessageContoller
{
	public static uint MessagesToKeep { get; set; } = 10;

	private static List<(float time, string msg)> Messages = new List<(float, string)>();
	public static event NewPlayerMessageDelegate OnNewPlayerMessage;
	public static event Action OnClearMessages;

	public static void AddMessage(string content)
	{
		Messages.Add((Time.time, content));
		while (Messages.Count > MessagesToKeep)
			Messages.RemoveAt(0);

		OnNewPlayerMessage?.Invoke(content);
	}

	public static void Clear()
	{
		Messages.Clear();
		if ( null != OnClearMessages )
			OnClearMessages.Invoke();
	}
}


public class MessagePlayback : MonoBehaviour
{
	private TextMeshProUGUI MessageDisplay;
	private Queue<string> MessageBacklog { get; set; } = new Queue<string>();
	private StringBuilder CurrentMessage { get; set; } = new StringBuilder();
	private StringBuilder UIContent { get; set; } = new StringBuilder();
	private Image BackgroundImage {get; set;}

	public GameObject BackgroundPane;
	public float NextTextAt = 0f;
	public int InnerMessageIndex = 0;
	public float MessageSpeed = 30; // Characters per second
	public string AudioNameMessageReceived; // Per message
	public string AudioNameMessageTyping; // Per character

	void Awake()
	{
		MessageDisplay = gameObject.GetComponent<TextMeshProUGUI>();
		MessageDisplay.SetText(string.Empty);
		MessageContoller.OnNewPlayerMessage += MessageContoller_OnNewPlayerMessage;
		MessageContoller.OnClearMessages += MessageContoller_OnClearMessages;
	}

	private void MessageContoller_OnClearMessages()
	{
		MessageBacklog.Clear();
		CurrentMessage.Length = 0;
		UIContent.Length = 0;
		MessageDisplay.text = string.Empty;
	}

	private void Start()
	{
		if ( BackgroundPane != null )
		{
			BackgroundImage = BackgroundPane.GetComponent<Image>();
		}
	}

	private void MessageContoller_OnNewPlayerMessage(string messageContent)
	{
		MessageBacklog.Enqueue(messageContent);
	}

	private float FadeDuration = 5f;
	private float FadeOutStart = 0f;


	void Update()
	{
		// Current Message drain
		if (CurrentMessage.Length > 0)
		{
			if (NextTextAt < Time.time)
			{
				// Ch per second:
				NextTextAt += 1f/MessageSpeed;
				UIContent.Insert(InnerMessageIndex, CurrentMessage[0]);
				CurrentMessage.Remove(0, 1);
				InnerMessageIndex++;

				if (UIContent.Length > 400)
					UIContent.Length = 400;

				if (AudioNameMessageTyping != null)
					AudioManager.TheAudioManager.Play(AudioNameMessageTyping);

				MessageDisplay.text = UIContent.ToString();

				if (CurrentMessage.Length == 0)
				{
					FadeOutStart = Time.time;
				}
			}
		}
		else if (MessageBacklog.Count > 0)
		{
			string toPlayack = MessageBacklog.Dequeue();
			CurrentMessage.Length = 0;
			CurrentMessage.Append(toPlayack);
			InnerMessageIndex = 0;

			UIContent.Insert(0, System.Environment.NewLine);
			UIContent.Insert(0, System.Environment.NewLine);
			MessageDisplay.SetText(UIContent);

			if (AudioNameMessageReceived != null)
				AudioManager.TheAudioManager.Play(AudioNameMessageReceived);

			if ( BackgroundImage != null)
			{
				BackgroundImage.color = new Color(BackgroundImage.color.r, BackgroundImage.color.g, BackgroundImage.color.b, .7f);
			}
		}
		else
		{
			if (BackgroundImage != null && BackgroundImage.color.a > .22f && (FadeOutStart + FadeDuration) > Time.time)
			{
				float a = Mathf.Lerp(.7f, .22f, (Time.time - FadeOutStart) / FadeDuration);

				BackgroundImage.color = new Color(BackgroundImage.color.r, BackgroundImage.color.g, BackgroundImage.color.b, a);
			}
		}

	}
}
