using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HandMenuController : MonoBehaviour
{
    public string option;
    public GameObject UI;
    public GameObject TicketPanel;
    public GameObject ComputerPanel;

    public void OnClicked()
    {
        switch(option)
        {
            case "Main Menu Staff":
                Debug.Log("Main Menu Staff pressed!");
                SceneManager.LoadScene("StaffUX");
                break;
            case "Main Menu Student":
                Debug.Log("Main Menu Student pressed!");
                SceneManager.LoadScene("StudentUX");
                break;
            case "UI":
                Debug.Log("UI was pressed!");
                if(UI.activeSelf) //check if UI GameObject is active
                {
                    UI.SetActive(false); //if UI is active, set it not active
                }
                else
                {
                    UI.SetActive(true); //if UI isn't active, set it active
                }
                break;
            case "Ticket":
                Debug.Log("Ticket was pressed!");
                if(TicketPanel.activeSelf) //check if TicketPanel GameObject is active
                {
                    TicketPanel.SetActive(false); //if TicketPanel is active, set it not active
                }
                else
                {
                    TicketPanel.SetActive(true); //if TicketPanel isn't active, set it active
                }
                break;
            case "Computer":
                Debug.Log("Computer was pressed!");
                if(ComputerPanel.activeSelf) //check if ComputerPanel GameObject is active
                {
                    ComputerPanel.SetActive(false); //if ComputerPanel is active, set it not active
                }
                else
                {
                    ComputerPanel.SetActive(true); //if ComputerPanel isn't active, set it active
                }
                break;
            case "Feedback":
                Debug.Log("Feeback was pressed!");
                Application.OpenURL("https://forms.microsoft.com/r/LFqdFdMpjL");
                break;
            case "Home":
                Debug.Log("Home was pressed!");
                SceneManager.LoadScene("LibAR");
                break;
        }
    }
}
