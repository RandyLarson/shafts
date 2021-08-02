using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;



public class LevelIntroductionController : MonoBehaviour
{
	private LevelAnnotation InnerDetails;
	public float FadeOutDuration = 7f;
	public float DefaultDuration = 5f;
	public TMP_Text ChapterTitle;
	public TMP_Text ChapterDetails;
	public TMP_Text StartButton;
	public string DefaultButtonText = "I'm on it!";

	private (TMP_Text item, Color org, Color dst)[] ItemsToFade;
	private float FadeOutStart;
	private float FadeOutEnd;
	protected virtual GameState ControllerKind { get; set; } = GameState.LevelIntro;

	public bool HasContent { get => ChapterTitle.text.Length > 0 || ChapterDetails.text.Length > 0; }

	public LevelAnnotation Details
	{
		get
		{
			return InnerDetails;
		}
		set
		{
			InnerDetails = value;
			if (InnerDetails != null)
			{
				if (ChapterTitle != null)
					ChapterTitle.text = InnerDetails.LevelTitle;
				if (ChapterDetails != null)
					ChapterDetails.text = InnerDetails.OverallGoal;
				if (InnerDetails.StartPromptOverride?.Length > 0 && StartButton != null)
					StartButton.text = InnerDetails.StartPromptOverride;
				else
					StartButton.text = DefaultButtonText;
			}
		}
	}

	public virtual void Clear()
	{
		InnerDetails?.Clear();
	}

	protected virtual void Awake()
	{
		ItemsToFade = GetComponentsInChildren<TMP_Text>()
			.Select(uiObj => (uiObj, uiObj.color, new Color(uiObj.color.r, uiObj.color.g, uiObj.color.b, 0)))
			.ToArray();

		if (null == ChapterTitle || null == ChapterDetails)
			Debug.LogWarning("Level controller is missing a title or detail reference");
	}

	void ShowElements()
	{
		if (ItemsToFade?.Any() == true)
		{
			gameObject.SetActive(true);
			foreach (var toFade in ItemsToFade)
			{
				toFade.item.color = toFade.org;
				toFade.item.color = toFade.org;
			}
			//StartCoroutine(ShowLevelDetails());
		}
	}

	private void OnEnable()
	{
	
	}

	private void OnDisable()
	{
		
	}

	internal void Activate(LevelAnnotation details, GameState gameStateKind)
	{
		Details = details;
		Activate(gameStateKind);
	}

	internal virtual void Activate(GameState? forState =null)
	{
		// These won't be wired if we are pre-start of the game, but there
		// are some UI elements enabled in the editor.
		if (GameController.TheController != null)
		{
			FadeOutStart = Time.time + ((InnerDetails != null && InnerDetails.Lifetime != 0) ? InnerDetails.Lifetime : DefaultDuration);
			FadeOutEnd = FadeOutStart + FadeOutDuration;
			ShowElements();
			GameController.TheController.LevelDetailsActive(forState ?? ControllerKind);
		}
	}

	public virtual void Dismiss()
	{ 
		GameController.TheController.LevelDetailsDismissed(ControllerKind);
		gameObject.SetActive(false);
	}

	private IEnumerator ShowLevelDetails()
	{
		yield return new WaitForFixedUpdate();

		yield break;
	}


	void Update()
	{
		if (ItemsToFade?.Any() == false)
			return;

		if (Time.time >= FadeOutStart)
		{
			float a = Mathf.Lerp(1, 0, (Time.time - FadeOutStart) / FadeOutDuration);

			foreach (var toFade in ItemsToFade)
			{
				toFade.item.color = new Color(toFade.org.r, toFade.org.g, toFade.org.b, a);

				if (a <= .05)
				{
					Dismiss();
				}
			}
		}
	}
}
