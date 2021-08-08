using System;
using System.Collections.Generic;
using Assets.Scripts.Extensions;
using Milkman;
using UnityEngine;

namespace Assets.Scripts.MissionPlanning
{
    public interface IConfigurator
	{
		Type TargetType { get; }
		void ConfigureTarget(Component component);

	}


	[RequireComponent(typeof(PickUp))]
	internal class RaiderMissionController : Interceptor
	{
		public float FreightStealingRadius = 100f;
		public GameObject CurrentFreightTarget = null;
		public GameObject ExitDestination = null;
		public GameObject MusterLocation = null;

		protected Vector2 OriginalApproachOffset = Vector2.zero;

		private PickUp CarryingScript { get; set; }

		protected override void Start()
		{
			OriginalApproachOffset = ApproachOffset;
			CarryingScript = GetComponent<PickUp>();
			base.Start();
		}

		public static Collider2D[] SharedHitPool = new Collider2D[50];


		public override bool MissionUpdate(MissionPlanner missionPlanner, PathFollower waypointFollower)
		{
			// If we are carrying Freight, then depart the area
			if (CarryingScript != null && CarryingScript.IsCarryingFreight)
			{
				// Ensure we are leaving and have plotted a course.
				if (missionPlanner.CurrentShipMode == ShipMode.Leaving)
					return true;

				missionPlanner.ChangeShipMode(ShipMode.Leaving);
				PlotExitCourse(missionPlanner);
			}
			else if (missionPlanner.CurrentShipMode == ShipMode.Leaving)
			{
				// We lost the freight. Go back to normal procedures 
				//	- locate freight to take
				//	- or fall back to mission planning for interception 
				ApproachOffset = OriginalApproachOffset;
				missionPlanner.ChangeShipMode(ShipMode.Patrolling);
			}

			// Scan for available Freight if we are not currently targeting an available freight payload.
			if (CurrentFreightTarget != null && CurrentFreightTarget.transform.parent != null)
			{
				return true;
			}

			// Scan for freight - 
			// Hopefully this will be Freight just released from the Player.
			int numHits = Physics2D.OverlapCircleNonAlloc(gameObject.transform.position, FreightStealingRadius, SharedHitPool);

			(GameObject freight, float distance) closestFreight = (null, 0);

			for (int i = 0; i < numHits; i++)
			{
				// Found something.. first encountered in list, perhaps not the closest to us.
				if (SharedHitPool[i].gameObject.transform.parent == null &&
					SharedHitPool[i].gameObject.CompareTag(GameConstants.Freight))
				{
					bool useAsTarget = false;
					Vector2 closestPoint = SharedHitPool[i].ClosestPoint(gameObject.transform.position);
					float distanceTo = Vector2.Distance(closestPoint, gameObject.transform.position);

					if (closestFreight.freight == null || distanceTo < closestFreight.distance)
					{
						useAsTarget = true;
					}

					if (useAsTarget)
					{
						closestFreight.freight = SharedHitPool[i].gameObject;
						closestFreight.distance = distanceTo;
					}
				}
			}

			if (closestFreight.freight != null)
			{
				CurrentFreightTarget = closestFreight.freight;
				Target asTarget = new Target()
				{
					gameObject = closestFreight.freight,
					Kind = TargetKind.CivilianOther
				};

				// Change our course to the freight, adjust our interception offset to be nothing 
				// so we'll pick it up.
				ApproachOffset = Vector2.zero;
				missionPlanner.ChangeTargetTo(asTarget);
				return true;
			}

			// Scan for the player/target - intercept only if they are carrying freight to steal.
			ScanForTargetWithFreight(missionPlanner);
			return true;
		}


		private void ScanForTargetWithFreight(MissionPlanner missionPlanner)
		{
			Target toIntercept = MissionTracker.GetClosestTarget(missionPlanner.CurrentTarget, missionPlanner.MissionTargetKind, transform.position);

			if (toIntercept != null && toIntercept.IsValidGameObject)
			{
				// Does the target have freight.
				if (toIntercept.gameObject.GetInterface(out IFreightController freightController))
				{
					if (!freightController.IsCarryingFreight)
					{
						toIntercept = null;
					}
				}
			}

			// Notify tracking to new target (possibly null target)
			missionPlanner.ChangeTargetTo(toIntercept);

			if (toIntercept != null)
			{
				missionPlanner.PlotCourse(PlanAttackPath(missionPlanner, transform.position, toIntercept.gameObject.transform.position));
			}
			else if (MusterLocation != null)
			{
				missionPlanner.PlotCourse(PlanAttackPath(missionPlanner, transform.position, MusterLocation.transform.position));
			}
		}

		private void PlotExitCourse(MissionPlanner missionPlanner)
		{
			if (ExitDestination != null)
				missionPlanner.PlotCourseTo(ExitDestination);
			else
				missionPlanner.PlotCourseTo(
					new Vector2(
						GameController.TheController.LevelBoundary.xMin,
						GameController.TheController.LevelBoundary.yMax));
		}

		public override IEnumerable<Vector3> PlanAttackPath(MissionPlanner missionPlanner, Vector3 startingPosition, Vector3 givenTarget)
		{
			// Working region -- constrain to this area if designated to do so.
			// Either head back to a designated starting/muster position, or go idle
			// if the target is outside our region.
			if (missionPlanner != null && !missionPlanner.WorkingRegion.Contains(givenTarget))
			{
				if (MusterLocation != null)
					givenTarget = MusterLocation.transform.position;
				else
					givenTarget = startingPosition;
			}

			return base.PlanAttackPath(missionPlanner, startingPosition, givenTarget);
		}
	}
}
