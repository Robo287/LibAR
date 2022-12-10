using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.IT_Dashboard_Scripts;


public class ButtonControl : MonoBehaviour
{
    [SerializeField] public TextMeshPro btnText;

    [Header("Ticket Details")]
    public Text infoText;

    public GameObject InfoPanelUI;
    //public string TextString;
    private readonly string libticketURL = "https://api.lib.fau.edu/systems";
    private readonly string reflibticketURL = "https://libticket.fau.edu/systems";

    
    private ticketInfoProxy proxy;

    public void Start()
    {
        proxy = new ticketInfoProxy();
        proxy.onTicketInfoComplete += Proxy_onTicketInfoComplete;
    }

    private void Proxy_onTicketInfoComplete(object sender, string e)
    {
        infoText.text = e;
        InfoPanelUI.SetActive(true);
    }

    public void SetText(string textString)
    {
        btnText.SetText(textString); //take button text and set it to btnText
    }

    public void OnClick()
    {
        var tmp = btnText.text.Split(new string[] { ": " }, StringSplitOptions.None); //get text from the button
        var tmp2 = tmp[1].Split(new string[] { "\n" }, StringSplitOptions.None); //isolate the ticket number
        Debug.Log(tmp2[0]);
        showTicketInfo(tmp2[0]);
    }

    public void showTicketInfo(string ticketNumber)
    {
        Debug.Log("showTicketInfo called, parsed: " + ticketNumber);
        StartCoroutine(proxy.APIHandler(ticketNumber));

    }

}
