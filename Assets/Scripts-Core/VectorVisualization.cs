using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorVisualization : MonoBehaviour
{
	public GameObject target;
	public GameObject hitMarker;
	public DiagnosticVisualizer visualizer;
	public bool DebugVisualize = false;
	DiagnosticVisualizer[] Visualizers;
	static RaycastHit2D[] SingletonHitArray = new RaycastHit2D[100];

	private void Start()
	{
		if (DebugVisualize)
		{
			Visualizers = new DiagnosticVisualizer[30];
			for (int i = 0; i < 30; i++)
			{
				Visualizers[i] = Instantiate(visualizer, Vector3.zero, visualizer.transform.rotation);
				Visualizers[i].AddOutput(i.ToString());
				Visualizers[i].Visual.text = i.ToString();
			}
		}
	}

	int drawNum = 0;


	bool TestPath(Vector3 start, Vector3 dst)
	{
		if (DebugVisualize)
		{
			Debug.DrawRay(transform.position, dst.normalized * 20, Color.blue, .1f);

			if (Visualizers[drawNum] != null)
			{
				Visualizers[drawNum].transform.position = start + dst.normalized * (30 + drawNum * 4);
				Visualizers[drawNum].Visual.color = Color.blue;
				Visualizers[drawNum].gameObject.SetActive(true);
			}
		}

		drawNum++;

		int numHits = Physics2D.CircleCastNonAlloc(start, 6, dst.normalized, SingletonHitArray, 20);//, GameConstants.LayerMaskDefaultAvoid);
		//int numHits = Physics2D.RaycastNonAlloc(start, dst.normalized, SingletonHitArray, 20);

		bool pathClear = true;

		for (int iHit = 0; iHit < numHits; iHit++)
		{
			var hit = SingletonHitArray[iHit];
			if (hit.collider.gameObject == gameObject)
				continue;

			pathClear = false;

			if (DebugVisualize)
			{
				GameObject marker = Instantiate(hitMarker, hit.point, hitMarker.transform.rotation);
				Destroy(marker, .1f);
			}
		}

		return pathClear;
	}



	Vector3? PathToward(Vector3 origin, Vector3 dst, float totalAngle, float incAngle, Vector3? lastGood, Vector3? lastBad)
	{
		if (TestPath(origin, dst))
			lastGood = dst;
		else
			lastBad = dst;

		// No good path yet, continue to increase probe angle.
		// Need to know when to stop -- when we've come all the way around.
		if (Mathf.Abs(totalAngle) >= 360)
			return lastGood;

		if (lastGood.HasValue && lastBad.HasValue)
		{
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

		return PathToward(origin, nextProbe, totalAngle + incAngle, incAngle, lastGood, lastBad);
	}

	float NextPlotTime = 0;
	Vector3? nextPath;

	void Update()
	{
		if (NextPlotTime < Time.time)
		{
			NextPlotTime = Time.time + .3f;
			drawNum = 0;

			var pathLeft = PathToward(transform.position, target.transform.position - transform.position, 0, -45, null, null);
			var pathRight = PathToward(transform.position, target.transform.position - transform.position, 0, 45, null, null);

			if (pathLeft.HasValue && pathRight.HasValue)
			{
				var leftTheta = Vector3.Angle(pathLeft.Value, target.transform.position);
				var rightTheta = Vector3.Angle(pathRight.Value, target.transform.position);

				nextPath = leftTheta < rightTheta ? pathLeft : pathRight;
			}
			else
			{
				nextPath = pathLeft.HasValue ? pathLeft : pathRight;
			}

			if (DebugVisualize)
			{
				for (int i = drawNum; i < 30 && Visualizers[i] != null; i++)
					Visualizers[i].gameObject.SetActive(false);

			}
		}

		if (nextPath.HasValue)
		{
			if ( DebugVisualize )
				Debug.DrawRay(transform.position, nextPath.Value.normalized * 10, Color.green, .1f);

			transform.position += nextPath.Value.normalized;
		}
	}
}
