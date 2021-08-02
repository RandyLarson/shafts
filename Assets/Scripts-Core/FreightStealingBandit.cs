using UnityEngine;
using Assets.Scripts.Extensions;

public class FreightStealingBandit : MonoBehaviour
{
	void Start()
	{
		if ( gameObject.GetComponent(out ProjectileController controller))
		{
			controller.TargetQualifier = TargetQualifier;
		}

	}

	public bool TargetQualifier(GameObject potentialTarget)
	{
		if ( potentialTarget.GetInterface(out IFreightController freightController))
		{
			return freightController.IsCarryingFreight;
		}
		return false;
	}

}
