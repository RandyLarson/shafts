namespace Assets.Scripts.Player
{
	public class PlayerHealth : HealthPoints
	{
		protected override bool UpdateHeathTo(float newValue)
		{
			base.UpdateHeathTo(newValue);

			// Destruction of player
			if ( newValue <= 0 )
			{
				return true;
				//GameController.TheController.PlayerDestruction();
			}
			return false;
		}

		public override bool AdjustHealthBy(float amount)
		{
			return base.AdjustHealthBy(amount);
		}

	}
}
