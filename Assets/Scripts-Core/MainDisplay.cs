using UnityEngine;
using TMPro;

namespace Milkman
{

	public class MainDisplay : MonoBehaviour
    {

        private TextMeshProUGUI m_textMeshPro;
        //private TMP_FontAsset m_FontAsset;

        private const string label = "The <#0050FF>count is: </color>{0:2}";
        private float m_frame;

		private HealthPoints PlayerHealth;


        void Start()
        {
			PlayerHealth = GameController.ThePlayer?.GetComponent<HealthPoints>();

			// Add new TextMesh Pro Component
			m_textMeshPro = gameObject.GetComponent<TextMeshProUGUI>();
			if (m_textMeshPro != null)
			{
				m_textMeshPro.autoSizeTextContainer = true;

				// Load the Font Asset to be used.
				//m_FontAsset = Resources.Load("Fonts & Materials/LiberationSans SDF", typeof(TMP_FontAsset)) as TMP_FontAsset;
				//m_textMeshPro.font = m_FontAsset;

				// Assign Material to TextMesh Pro Component
				//m_textMeshPro.fontSharedMaterial = Resources.Load("Fonts & Materials/LiberationSans SDF - Bevel", typeof(Material)) as Material;
				//m_textMeshPro.fontSharedMaterial.EnableKeyword("BEVEL_ON");

				// Set various font settings.
				//m_textMeshPro.fontSize = 48;

				m_textMeshPro.alignment = TextAlignmentOptions.Left;

				//m_textMeshPro.anchorDampening = true; // Has been deprecated but under consideration for re-implementation.
				//m_textMeshPro.enableAutoSizing = true;

				//m_textMeshPro.characterSpacing = 0.2f;
				//m_textMeshPro.wordSpacing = 0.1f;

				//m_textMeshPro.enableCulling = true;
				m_textMeshPro.enableWordWrapping = false;

				//textMeshPro.fontColor = new Color32(255, 255, 255, 255);
			}
        }


        void Update()
        {
			if (m_textMeshPro != null)
			{
				m_textMeshPro.SetText(label, m_frame % 1000);
				m_frame += 1 * Time.deltaTime;

				m_textMeshPro.SetText("Health: " + PlayerHealth.HP);
			}
        }

    }
}
