using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class changeScene : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        GlobalVars.versionNumber = Application.version.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowKeyboard_for_input(string tag)
    {
        if (string.IsNullOrEmpty(tag)) { return; }

        string textboxText = "";

        var obj = GameObject.Find(tag);
        var txtMsg = obj.GetComponent<TMP_InputField>();
        if (txtMsg != null) {
            textboxText = txtMsg.text;    
        }
        
        GlobalVars.keyboard = TouchScreenKeyboard.Open(textboxText, TouchScreenKeyboardType.Default, false, false, false, false);
        GlobalVars.keyboardText = GlobalVars.keyboard.text;
    }

    public void ShowKeyboard_for_input(UnityEngine.UI.Text tag)
    {
        //if (string.IsNullOrEmpty(tag.text)) { return; }

        string textboxText = "";

        //var obj = GameObject.Find(tag);
        //var txtMsg = obj.GetComponent<TMP_InputField>();
        if (tag != null)
        {
            textboxText = tag.text;
        }

        GlobalVars.keyboard = TouchScreenKeyboard.Open(textboxText, TouchScreenKeyboardType.Default, false, false, false, false);
        GlobalVars.keyboardText = GlobalVars.keyboard.text;
    }

    public void GetKeyboardInput(UnityEngine.UI.Text textControl)
    {
        textControl.text = GlobalVars.keyboard.text;
    }

    public void ShowKeyboard_for_input(TMPro.TMP_InputField tag)
    {
        //if (string.IsNullOrEmpty(tag.text)) { return; }

        string textboxText = "";

        //var obj = GameObject.Find(tag);
        //var txtMsg = obj.GetComponent<TMP_InputField>();
        if (tag != null)
        {
            textboxText = tag.text;
        }

        bool bSecure = tag.name.ToLower().Contains("secret") || tag.name.ToLower().Contains("key");

        GlobalVars.keyboard = TouchScreenKeyboard.Open(textboxText, TouchScreenKeyboardType.Default, false, false, bSecure, false);
        GlobalVars.keyboardText = textboxText;
    }

    public void GetKeyboardInput(TMPro.TMP_InputField textControl)
    {
        if (GlobalVars.keyboardText == GlobalVars.keyboard.text) return;

        if (string.IsNullOrEmpty(GlobalVars.keyboard.text)) return;
        
        textControl.text = GlobalVars.keyboard.text;
        GlobalVars.keyboardText = GlobalVars.keyboard.text;        
    }

    public void ExitApplication()
    {
        Debug.Log("Application is exiting");
        Application.Quit();
    }

    public void changeButtons(string newScene)
    {        
        SceneManager.LoadScene(newScene);
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
    }

    public void launchBrowser(string Url)
    {
        Application.OpenURL(Url);
    }
}
