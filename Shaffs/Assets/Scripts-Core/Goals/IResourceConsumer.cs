public interface IResourceConsumer
{
	void AdjustResource(Resource kind, float amount);
	void SetResource(Resource kind, float amount);
}