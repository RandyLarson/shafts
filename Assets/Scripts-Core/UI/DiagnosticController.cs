using System.Text;
using Assets.Scripts.Extensions;
using UnityEngine;

namespace Assets.Scripts.UI
{
	public class DiagnosticController : MonoBehaviour
	{

		StringBuilder DiagnosticStringBuilder = new StringBuilder();
		public void ClearDiagnostics() => DiagnosticStringBuilder?.Clear();
		public void AddDiagnostic(string toadd) => DiagnosticStringBuilder?.AppendLine(toadd);

		public static void Add(string toAdd) => GameController.DiagnosticsController?.AddDiagnostic(toAdd);

		private void LateUpdate()
		{
			GameUIController.Controller.Diagnostics.SafeSetText(DiagnosticStringBuilder.ToString());
			DiagnosticStringBuilder.Clear();
		}
	}
}
