using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DiagnosticVisualizer : MonoBehaviour
{
	public TextMeshProUGUI Visual;
	public bool Velocity = true;
	public GameObject Target;
	public bool Camera = false;

	Rigidbody2D TargetRB;
	public string AdditionalText;

	void Start()
	{
		if (Target != null)
		{
			TargetRB = Target.GetComponent<Rigidbody2D>();
		}
	}

	string AppendContent(string target, string toApppend)
	{
		if (toApppend != null)
		{
			if (target.Length > 0)
				target += " // ";
			target += toApppend;
		}
		return target;
	}

	void Update()
	{
		string content = string.Empty;

		if (Velocity && TargetRB != null && Visual != null)
		{
			content = AppendContent(content, $"{(int)TargetRB.velocity.magnitude}");
		}

		if ( Camera )
		{
			string aux = $"{GameUIController.Controller.MainCamera.transform.position.ToString()}, {GameUIController.Controller.MainCamera.nearClipPlane}, {GameUIController.Controller.MainCamera.farClipPlane} ";
			content = AppendContent(content, aux);
		}

		content = AppendContent(content, AdditionalText);

		Visual.text = content;
	}

	public void AddOutput(string toAdd)
	{
		AdditionalText = toAdd;
	}
}
