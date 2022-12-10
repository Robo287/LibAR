using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace LibAR
{

    public enum RecognitionMode { Speech_Recognizer, Intent_Recognizer, Tralation_Recognizer, Disabled, Offline };
    public enum SimuilateOfflineMode { Enabled, Disabled };
    public enum TranslateToLanguage { Russian, German, Chinese };

    public class larLunarcomButtonController : MonoBehaviour
    {
        [Header("Reference Objects")]
        public RecognitionMode speechRecognitionMode = RecognitionMode.Disabled;

        [Space(6)]
        [Header("Button States")]
        public Sprite Default;
        public Sprite Activated;

        private Button button;
        private bool isSelected = false;

        private larLunarcomController lunarcomController;

        private void Start()
        {
            lunarcomController = larLunarcomController.lunarcomController;
            button = GetComponent<Button>();
        }

        public bool GetIsSelected()
        {
            return isSelected;
        }

        public void ToggleSelected()
        {
            ToggleSelected(false, false);
        }

        public void ToggleSelected(bool overideSelected = false, bool selected = false )
        {
            if (overideSelected)
                isSelected = selected;

            if (isSelected)
            {
                DeselectButton();
            }
            else
            {
                if (button != null)
                    button.image.sprite = Activated;
                isSelected = true;
                lunarcomController.SetActiveButton(GetComponent<larLunarcomButtonController>());

                if (lunarcomController.IsOfflineMode())
                {
                    lunarcomController.SelectMode(RecognitionMode.Offline);
                }
                else
                {
                    lunarcomController.SelectMode(speechRecognitionMode);
                }
            }
        }

        public void ShowNotSelected()
        {
            if (button != null)
                button.image.sprite = Default;
            isSelected = false;
        }

        public void DeselectButton()
        {
            ShowNotSelected();
            lunarcomController.SelectMode(RecognitionMode.Disabled);
        }
    }
}