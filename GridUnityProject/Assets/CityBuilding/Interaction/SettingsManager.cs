using UnityEngine;
using UnityEngine.UI;

namespace Interaction
{
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField]
        private Button SettingsButton;
        [SerializeField]
        private Button SettingsCloserButton;
        [SerializeField]
        private GameObject SettingsPanel;
        [SerializeField]
        private GameObject MainHud;

        private void Start()
        {
            SettingsButton.onClick.AddListener(ShowSettings);
            SettingsCloserButton.onClick.AddListener(HideSettings);
        }

        private void ShowSettings()
        {
            SettingsPanel.SetActive(true);
            MainHud.SetActive(false);
        }

        public void HideSettings()
        {
            SettingsPanel.SetActive(false);
            MainHud.SetActive(true);
        }
    }
}