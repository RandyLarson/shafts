using System;

public class ResourceStat
{
	public Resource Kind;

	/// <summary>
	/// The total available for production (if applicable).
	/// For example, people - the total available to be saved on a level, though they
	/// are produced incrementally throughout game play.
	/// Will be 0 if not applicable (most food/resources/shield type things will be like this
	/// unless there is a level where a finite amount of the stuff will be produced).
	/// </summary>
	public float Potential;
	public float Produced;
	public float Lost;
	public float Delivered;

	internal ResourceStat Clone()
	{
		return new ResourceStat()
		{
			Delivered = this.Delivered,
			Kind = this.Kind,
			Lost = this.Lost,
			Potential = this.Potential,
			Produced = this.Produced
		};
	}
}
