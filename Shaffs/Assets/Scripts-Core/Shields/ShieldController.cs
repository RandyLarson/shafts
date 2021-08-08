using System;
using Assets.Scripts.Extensions;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
	public GameObject explosion;
	public bool UnderConstruction = false;
	protected Animator ShieldAnimator = null;


	protected HealthPoints ShieldHP;
	public float EnableShieldsAt = 0f;
	protected EdgeCollider2D ShieldEdge;


	protected void Start()
	{
		ShieldAnimator = GetComponent<Animator>();
		ShieldHP = GetComponent<HealthPoints>();
		ShieldEdge = GetComponent<EdgeCollider2D>();

		var healthCallback = ShieldHP.gameObject.GetInterface<IHealthCallback>();
		if (null != healthCallback)
			healthCallback.OnHealthChanged += HealthCallback_OnHealthChanged;

		UpdateShieldStatus();

	}

	private void HealthCallback_OnHealthChanged(GameObject gameObject, float orgValue, float currentValue)
	{
		UpdateShieldStatus();
	}


	void Update()
	{
		UpdateShieldStatus();
	}

	internal HealthPoints ShieldHealth { get => ShieldHP; }

	public bool IsShieldActive
	{
		get => ShieldEdge.gameObject.SafeIsActive<GameObject>();
	}

	public void UpdateShieldStatus(bool isActivated)
	{
		ShieldAnimator.SetBool(GameConstants.ShieldDeactivatedHash, !isActivated);

		if (ShieldEdge != null)
		{
			ShieldEdge.enabled = isActivated;
		}
	}


	public void UpdateShieldStatus()
	{
		if (UnderConstruction || ShieldHP.HP <= 0)
		{
			UpdateShieldStatus(false);
			ShieldHP.HP = 0;
		}
		else if (EnableShieldsAt == 0f || Time.time >= EnableShieldsAt)
		{
			UpdateShieldStatus(true);
		}

		ShieldAnimator.SetFloat(GameConstants.ShieldHealthHash, ShieldHP.HP);
	}


	public float CurrentHP
	{
		get { return ShieldHP.HP; }
	}


	public bool IsRunning
	{
		get
		{
			return CurrentHP > 0 && ShieldAnimator.GetBool(GameConstants.ShieldDeactivatedHash);
		}
	}



	public void AddResource(Resource kind, float amount)
	{
		if (kind == Resource.Shield && ShieldHP != null)
			ShieldHP.AdjustHealthBy(amount);
	}

	internal void SetHealth(float hp)
	{
		ShieldHP.HP = hp;
	}
}
