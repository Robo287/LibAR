using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FindBookSubmit : MonoBehaviour
{
    public InputField Title;
    public void OnButtonClick()
    {
        GlobalVars.getDirectionBookTitle = Title.text;
        SceneManager.LoadScene("Directions");
    }
}
