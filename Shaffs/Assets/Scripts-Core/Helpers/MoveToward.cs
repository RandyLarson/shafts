using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Extensions;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
	class MoveToward
	{
		static public bool DebugVisualize = true;
		static RaycastHit2D[] SingletonHitArray = new RaycastHit2D[100];
		static int drawNum = 0;
		static DiagnosticVisualizer[] Visualizers;
		static public GameObject HitMarker = null;

		static public void Initialize()
		{
			DebugVisualize = GameController.TheController.DiagnosticPathFindingVisualize;
			if (DebugVisualize)
			{
				HitMarker = GameController.TheController.DiagnosticHitMarker;
				Visualizers = new DiagnosticVisualizer[30];
				for (int i = 0; i < 30; i++)
				{
					Visualizers[i] = GameObject.Instantiate(GameController.TheController.DiagnosticVisualizer, Vector3.zero, GameController.TheController.DiagnosticVisualizer.transform.rotation);
					Visualizers[i].AddOutput(i.ToString());
					Visualizers[i].Visual.text = i.ToString();
				}
			}
		}

		static public bool TestPath(GameObject sourceObject, Vector3 targetPosition, Vector3 start, Vector3 direction)
		{
			float dxToTarget = Vector3.Distance(start, targetPosition);
			float castRadius = 5;
			Vector3 probeStart = start + (direction * castRadius);

			float distanceToProbe = Mathf.Min(dxToTarget-castRadius, 25f);
			if (DebugVisualize)
			{
				if (Visualizers[drawNum] != null)
				{
					Visualizers[drawNum].transform.position = start + direction * (distanceToProbe + drawNum * 3);
					Visualizers[drawNum].gameObject.SetActive(true);

					var pathToward = (targetPosition - start);
					//var angle = Vector3.SignedAngle(direction, targetTranslated, Vector3.forward);

					var dot = Vector3.Dot(pathToward.normalized, direction.normalized);
					Visualizers[drawNum].AddOutput( drawNum.ToString() + ":" + dot.ToString());
				}
			}

			int numHits = Physics2D.CircleCastNonAlloc(probeStart, castRadius, direction, SingletonHitArray, distanceToProbe, GameConstants.LayerMaskDefaultAvoid);

			bool isPathClear = true;
			if (DebugVisualize)
			{
				Debug.DrawRay(probeStart, direction * distanceToProbe, Color.magenta, .1f);
			}

			for (int iHit = 0; iHit < numHits; iHit++)
			{
				var hit = SingletonHitArray[iHit];
				
				if (hit.transform.IsChildOf(sourceObject.transform))
					continue;

				isPathClear = false;

				if (DebugVisualize)
				{
					if (HitMarker != null)
					{
						GameObject marker = GameObject.Instantiate(HitMarker, hit.point, HitMarker.transform.rotation);
						GameObject.Destroy(marker, .1f);
					}
					Debug.DrawRay(probeStart, direction * hit.distance, isPathClear ? Color.green : Color.red, .1f);
				}
			}

			if (DebugVisualize)
			{
				if (isPathClear)
					Debug.DrawRay(probeStart, direction * distanceToProbe, isPathClear ? Color.green : Color.red, .1f);

				Visualizers[drawNum].Visual.color = isPathClear ? Color.green : Color.red;
			}

			drawNum++;
			return isPathClear;
		}



		static public Vector3? PathToward(GameObject sourceObject, Vector3 targetPosition, Vector3 origin, Vector3 dir, float totalAngle, float incAngle, Vector3? lastGood, Vector3? lastBad)
		{
			if (TestPath(sourceObject, targetPosition, origin, dir))
				lastGood = dir;
			else
				lastBad = dir;

			// No good path yet, continue to increase probe angle.
			// Need to know when to stop -- when we've come all the way around.
			if (Mathf.Abs(totalAngle) >= 360)
				return lastGood;

			if (lastGood.HasValue && lastBad.HasValue)
			{
				Vector3 directPath = (targetPosition - origin).normalized;

				float dotProduct = Vector3.Dot(directPath, lastGood.Value.normalized);
				if (dotProduct > .85)
					return lastGood;

				float remainingAngle = Vector3.Angle(lastBad.Value, lastGood.Value);
				if (remainingAngle <= 30)
					return lastGood;
			}

			Vector3 nextProbe;

			if (lastGood.HasValue)
			{
				// Have a bad and a good. Now bisect those and see if we can
				// cut it closer to the obstacle.
				if (lastBad.HasValue)
					nextProbe = (lastGood.Value.normalized + lastBad.Value.normalized) / 2;
				else
					return lastGood.Value;
			}
			else
			{
				// Continue to increment the search probe until we find and open path.
				nextProbe = Quaternion.AngleAxis(incAngle, Vector3.forward) * lastBad.Value;
			}

			return PathToward(sourceObject, targetPosition, origin, nextProbe, totalAngle + incAngle, incAngle, lastGood, lastBad);
		}



		static public Vector3 PlotAvoidanceToward(GameObject fromObject, Vector3 startPosition, Vector3 targetPosition)
		{
			drawNum = 0;
			Vector3 directPath = (targetPosition - startPosition).normalized;

			Vector3? pathLeft = PathToward(fromObject, targetPosition, startPosition, directPath, 0, -30, null, null);
			Vector3? pathRight = null;

			bool searchRight = true;

			if (pathLeft.HasValue && Vector3.Dot(directPath, pathLeft.Value.normalized) >= .85)
			{
				searchRight = false;
			}

			if (searchRight)
			{
				pathRight = PathToward(fromObject, targetPosition, startPosition, directPath, 0, 30, null, null);
			}

			Vector3 nextPath = Vector3.zero;

			if (pathLeft.HasValue && pathRight.HasValue)
			{
				// Tie break toward the most direct path
				var leftDot = Vector3.Dot(directPath, pathLeft.Value.normalized);
				var rightDot = Vector3.Dot(directPath, pathRight.Value.normalized);

				nextPath = leftDot > rightDot ? pathLeft.Value : pathRight.Value;
			}
			else
			{
				nextPath = pathLeft.HasValue ? pathLeft.Value : (pathRight.HasValue ? pathRight.Value : Vector3.zero);
			}

			if (DebugVisualize)
			{
				Debug.DrawRay(startPosition, nextPath * 20, Color.magenta, .1f);
				for (int i = drawNum; i < 30 && Visualizers[i] != null; i++)
					Visualizers[i].gameObject.SetActive(false);
			}

			return startPosition + nextPath;
		}


		static public void MoveToIntercept(
			ShipCharacteristics shipCharacteristics,
			Rigidbody2D srcBody,
			float minDistance,
			Vector3 target,
			Vector3 targetVelocity,
			bool slowAtTarget,
			float maxSpeedSquared,
			string[] additionalAvoidTags = null)
		{
			bool checkAvoidance = shipCharacteristics.ShouldCheckAvoidance;
			shipCharacteristics.LastCourseVector = MoveToIntercept(
				shipCharacteristics.transform,
				srcBody,
				checkAvoidance,
				shipCharacteristics.LastCourseVector,
				shipCharacteristics.PropulsiveForce,
				minDistance,
				target,
				targetVelocity,
				slowAtTarget,
				maxSpeedSquared,
				shipCharacteristics.DragMinimum,
				shipCharacteristics.DragAtMaxSpeed,
				shipCharacteristics.ProximitySlowingFactor,
				shipCharacteristics.ManeuveringAngle,
				shipCharacteristics.ManeuveringDrag,
				additionalAvoidTags);

			if (checkAvoidance)
				shipCharacteristics.UpdateAvoidanceCheck();
		}


		static public Vector3 MoveToIntercept(
			Transform srcTransform,
			Rigidbody2D srcBody,
			bool doTerrainAvoidance,
			Vector3 lastCourseDirection,
			float force,
			float minDistance,
			Vector3 target,
			Vector3 targetVelocity,
			bool slowAtTarget,
			float maxSpeedSquared,
			float minDrag,
			float maxDrag,
			float? slowingFactor = null,
			float? maneuveringAngle = null,
			float? maneuveringDrag = null,
			string[] additionalAvoidTags = null)
		{
			Vector3 courseDirection = lastCourseDirection;
			if (target != null)
			{
				float distanceToTarget = Vector3.Distance(target, srcTransform.position);

				// Initial interception calculation and terrain avoidance.
				if (doTerrainAvoidance)
				{
					Vector3 courseVector = AimingHelpers.FirstOrderIntercept(srcTransform.position, Vector3.zero, force / srcBody.mass, target, targetVelocity);
					if (DebugVisualize)
						Debug.DrawLine(srcTransform.position, courseVector, Color.cyan, .1f);

					// Terrain avoidance
					courseVector = PlotAvoidanceToward(srcTransform.gameObject, srcTransform.position, courseVector);
					if ( DebugVisualize)
						Debug.DrawRay(srcTransform.position, (courseVector - srcTransform.position).normalized * 20, Color.green, .1f);
					courseDirection = (courseVector - srcTransform.position).normalized;
				}

				if ( DebugVisualize)
					Debug.DrawRay(srcTransform.position, courseDirection * 15, doTerrainAvoidance ? Color.blue : Color.magenta, .1f);

				float forceFactor = 10f;
				Vector3 appliedForce = (courseDirection * forceFactor * force);


				// Drag increases in order to slow at a waypoint or better maneuver toward the target.
				bool dragSet = false;
				if (slowAtTarget)
				{
					var forceMag = srcBody.velocity.magnitude;
					Vector2 slowingForce = Vector2.zero;
					if (distanceToTarget < forceMag * (slowingFactor ?? 1.2))
					{
						slowingForce = Mathf.Pow(distanceToTarget - forceMag, 2) * -srcBody.velocity.normalized;
						appliedForce = slowingForce;
						dragSet = true;
						srcBody.drag = maneuveringDrag ?? .3f;
					}
				}

				if (!dragSet)
				{
					var toward = target - srcTransform.position;
					//Debug.DrawRay(srcTransform.position, toward, Color.cyan, .1f);
					float dotProduct = Vector3.Dot(srcBody.velocity.normalized, toward.normalized);

					//if (srcBody.gameObject.GetComponentInChildren<DiagnosticVisualizer>(out DiagnosticVisualizer diagnostics))
					//{
					//	diagnostics.AddOutput($"Dot: {dotProduct:F2}");
					//}

					if (dotProduct < (maneuveringAngle ?? .1f))
					{
						srcBody.drag = maneuveringDrag ?? .3f;
						dragSet = true;
					}
				}

				if (distanceToTarget < minDistance)
				{
					appliedForce *= -1;
				}

				// Drag based on velocity, when not set by some other action 
				// like maneuvering or slowing.
				if (maxDrag > 0 && !dragSet)
				{
					float percentMaxV = srcBody.velocity.sqrMagnitude / maxSpeedSquared;
					float drag = Math.Min(maxDrag, maxDrag * percentMaxV);
					srcBody.drag = drag;
				}

				if (minDrag > 0)
				{
					srcBody.drag = Math.Max(minDrag, srcBody.drag);
				}

				//else if ( maxSpeedSquared > 0 )
				//{
				//	float percentOfMax = srcBody.velocity.sqrMagnitude / maxSpeedSquared;
				//	appliedForce *= 1-percentOfMax;
				//}


				//Debug.DrawRay(srcTransform.position, forceVector, Color.green, .1f);
				//Debug.DrawRay(srcTransform.position, appliedForce, Color.magenta, .1f);
				//Debug.DrawLine(srcTransform.position, aimAt, Color.cyan, .1f);
				//Debug.DrawRay(srcTransform.position, srcBody.velocity, Color.blue, .1f);

				srcBody.AddForce(appliedForce);
			}

			return courseDirection;
		}

		static public void MoveToIntercept(Transform srcTransform, Rigidbody2D srcBody, float force, float minDistance, float maxSpeedSquared, float maxDrag, float minDrag, GameObject target)
		{
			if (target != null)
			{
				var TargetRB = target.GetComponent<Rigidbody2D>();
				Vector2 targetVelocity = Vector2.zero;
				if (TargetRB != null)
					targetVelocity = TargetRB.velocity;

				MoveToIntercept(
					srcTransform,
					srcBody,
					true,
					Vector3.zero,
					force,
					minDistance,
					target.transform.position,
					targetVelocity,
					false,
					maxSpeedSquared,
					maxDrag,
					minDrag,
					null,
					null);
			}
		}
	}
}
