using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace LibAR
{

    public class larLunarcomController : MonoBehaviour
    {
        public static larLunarcomController lunarcomController = null;

        public OwlController owlController = null;

        [Header("Object References")]
        public GameObject terminal;
        public ConnectionLightController connectionLight;
        public Text outputText;
        public List<larLunarcomButtonController> buttons;

        public delegate void OnSelectRecognitionMode(RecognitionMode selectedMode);
        public event OnSelectRecognitionMode onSelectRecognitionMode;

        RecognitionMode speechRecognitionMode = RecognitionMode.Disabled;
        larLunarcomButtonController activeButton = null;
        larLunarcomOfflineRecognizer lunarcomOfflineRecognizer = null;
        public GameObject OwlseyPrefab;
        public GameObject TreeStump;


        private void Awake()
        {
            if (lunarcomController == null)
                lunarcomController = this;
            else if (lunarcomController != this)
                Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {

            if (GetComponent<larLunarcomOfflineRecognizer>())
            {
                lunarcomOfflineRecognizer = GetComponent<larLunarcomOfflineRecognizer>();
                if (lunarcomOfflineRecognizer.simulateOfflineMode == SimuilateOfflineMode.Disabled)
                {
                    SetupOnlineMode();
                }
                else
                {
                    SetupOfflineMode();
                }
            }
            else
            {
                SetupOnlineMode();
            }
        }

        public bool IsOfflineMode()
        {
            if (lunarcomOfflineRecognizer != null)
            {
                return lunarcomOfflineRecognizer.simulateOfflineMode == SimuilateOfflineMode.Enabled;
            }
            else
            {
                return false;
            }
        }

        private void SetupOnlineMode()
        {

            ShowConnected(true);
        }

        private void SetupOfflineMode()
        {

            if (GetComponent<larLunarcomIntentRecognizer>())
            {
                GetComponent<larLunarcomIntentRecognizer>().enabled = false;
                ActivateButtonNamed("MicButton", false);
            }

            ShowConnected(false);
        }

        private void ActivateButtonNamed(string name, bool makeActive = true)
        {
            foreach (larLunarcomButtonController button in buttons)
            {
                if (button.gameObject.name == name)
                {
                    button.gameObject.SetActive(makeActive);
                }
            }
        }

        public RecognitionMode CurrentRecognitionMode()
        {
            return speechRecognitionMode;
        }

        public void SetActiveButton(larLunarcomButtonController buttonToSetActive)
        {
            activeButton = buttonToSetActive;
            foreach (larLunarcomButtonController button in buttons)
            {
                if (button != activeButton && button.GetIsSelected())
                {
                    button.ShowNotSelected();
                }
            }
        }

        public void SelectMode(RecognitionMode speechRecognitionModeToSet)
        {
            speechRecognitionMode = speechRecognitionModeToSet;
            onSelectRecognitionMode(speechRecognitionMode);
            //if (speechRecognitionMode == RecognitionMode.Disabled)
            //{

            //}
        }

        public void ShowConnected(bool showConnected)
        {
            if(connectionLight != null)
                connectionLight.ShowConnected(showConnected);
        }

        public void ShowTerminal()
        {
            terminal.SetActive(true);
        }

        public void HideTerminal()
        {
            if (terminal.activeSelf)
            {
                foreach (larLunarcomButtonController button in buttons)
                {
                    if (button.GetIsSelected())
                    {
                        button.ShowNotSelected();
                    }
                }

                // outputText.text = "Select a mode to begin.";
                terminal.SetActive(false);
                SelectMode(RecognitionMode.Disabled);
            }
        }

        public void UpdateLunarcomText(string textToUpdate)
        {
            outputText.text = textToUpdate;
        }

        public void LaunchOwlseyAssistant()
        {
            TreeStump.SetActive(true);
            OwlseyPrefab.SetActive(true);
            //OwlseyPrefab.transform.position = new Vector3(-8.53f, -3.05f, 11.01f);        

        }
    }
}