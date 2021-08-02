using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerTarget : Target
{
	public Vector2 Location;

	public PlayerTarget()
	{
		Kind = TargetKind.Player;
	}
}

public enum TargetKind
{
	None = 0,
	Player = 1,
	CivilianGround = 2,
	MilitaryGround = 4,
	GroundAny = (CivilianGround | MilitaryGround),
	CivilianAir = 8,
	MilitaryAir = 16,
	CivilianOther = 32   // Freight
}

public class Target
{
	public TargetKind Kind;
	public GameObject gameObject;
	public float WhenFound;

	public bool IsValidGameObject { get => gameObject != null; }

	public float TimeSinceFound { get => Time.time - WhenFound; }


	/// <summary>
	/// If memory is really an issue -- or cycling memory, then this could go to some sort
	/// of fixed allocation, where null is the placeholder for empty.
	/// There would need to be a max-attacker constant someplace probably.
	/// </summary>
	public List<GameObject> AssignedAttackers { get; private set; } = new List<GameObject>();

	public void AddAttacker(GameObject toAdd)
	{
		AssignedAttackers.Add(toAdd);
	}

	public void RemoveAttacker(GameObject toAdd)
	{
		AssignedAttackers.Remove(toAdd);
	}

	public void PruneAttackers()
	{
		AssignedAttackers.Where(t => t.gameObject == null)
			.ToList()
			.ForEach(t =>
			{
				AssignedAttackers.Remove(t);
			});
	}
}

