using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public interface IResourceVisualizer
{
	void InventoryChanged();
	ResourceAmount[] Requirements { get; set; }
	IInventory Inventory { get; set; }
}

public enum VisualizationStyle
{
	numeric,
	progress
}

public class ResourceVisualizer : MonoBehaviour, IResourceVisualizer
{
	public TextMeshProUGUI Visualizer;
	public VisualizationStyle DisplayStyle = VisualizationStyle.numeric;

	public bool DisplayFood = false;
	public bool DisplayMaterials = false;
	public bool DisplayTime = false;
	public bool DisplayShields = false;
	public bool DisplayPeople = false;
	public bool MultiLine = false;
	public bool DisplayRequirements = true;

	public InventoryHolder SourceInventory;

	private IInventory InventoryToDisplay { get; set; }
	private ResourceAmount[] ResourceRequirements { get; set; }


	private bool IsDisplayOutOfDate = true;


	private void Start()
	{
		if (SourceInventory != null)
		{
			// Wires up the inventory source (and change notifications)
			Inventory = SourceInventory;
		}
	}

	private void Update()
	{
		if (!IsDisplayOutOfDate || Visualizer == null)
			return;

		IsDisplayOutOfDate = false;
		UpdateVisualization();
	}


	private void AppendLabel(bool appendIt, StringBuilder sb, string label, Resource kind, float? req)
	{
		if (!appendIt || null == sb)
			return;

		if (sb.Length > 0)
		{
			if (MultiLine)
				sb.AppendLine();
			else
				sb.Append(" ");
		}

		if (!string.IsNullOrWhiteSpace(label))
		{
			sb.Append(label);
			sb.Append(" ");
		}

		var amt = InventoryToDisplay.GetResource(kind);
		sb.AppendFormat("{0:N0}", amt);
		if (DisplayRequirements && req.HasValue)
			sb.AppendFormat("/{0:N0}", req.Value);
	}

	float? GetRequirement(Resource kind)
	{
		if (Requirements == null || DisplayRequirements == false)
			return null;

		var matching = Requirements
			.Where(req => req.Kind == kind)
			.Select(req => req.Amount);

		return matching.Any() ? matching.First() : (float?)null;
	}

	private string BuildDisplay()
	{
		StringBuilder sb = new StringBuilder();

		if (InventoryToDisplay == null)
		{
			sb.Append("**");
		}
		else
		{
			AppendLabel(DisplayFood, sb, string.Empty, Resource.Food, GetRequirement(Resource.Food));
			AppendLabel(DisplayTime, sb, string.Empty, Resource.Time, GetRequirement(Resource.Time));
			AppendLabel(DisplayShields, sb, string.Empty, Resource.Shield, GetRequirement(Resource.Shield));
			AppendLabel(DisplayMaterials, sb, string.Empty, Resource.Material, GetRequirement(Resource.Material));
			AppendLabel(DisplayPeople, sb, string.Empty, Resource.People, GetRequirement(Resource.People));
		}

		return sb.ToString();
	}

	public void UpdateVisualization()
	{
		if (Visualizer != null)
			Visualizer.text = BuildDisplay();
	}


	public void InventoryChanged()
	{
		IsDisplayOutOfDate = true;
	}

	public ResourceAmount[] Requirements
	{
		set
		{
			ResourceRequirements = value;
			InventoryChanged();
		}
		get { return ResourceRequirements; }
	}

	public IInventory Inventory
	{
		get
		{
			return InventoryToDisplay;
		}

		set
		{
			InventoryToDisplay = value;
			if (InventoryToDisplay != null)
			{
				InventoryToDisplay.OnResourceChanged += InventoryToDisplay_OnResourceChanged;
				InventoryChanged();
			}
		}
	}

	private void InventoryToDisplay_OnResourceChanged(Resource kind, float newAmt)
	{
		InventoryChanged();
	}
}

