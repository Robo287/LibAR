using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class showConfirmationScreen : MonoBehaviour
{
    [Header("Select Conference Room Script")]
    public selectConferenceRoom SelectConferenceRoom;

    [Header("Confirmation Panel")]
    public GameObject ConfirmationPanel;


    private System.Timers.Timer timer;

    private void SetTimer()
    {
        // Create a timer with a 5-second interval.
        timer = new System.Timers.Timer(5000);
        // Hook up the Elapsed event for the timer. 
        timer.Elapsed += Timer_Elapsed;
        timer.Enabled = true;
        timer.AutoReset = false;
    }

   
    private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {

        try
        {
            var components = ConfirmationPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            var messagePanel = components[0];
            messagePanel.text = "";
        }
        catch
        { }

        ConfirmationPanel.SetActive(false);        
        timer.Stop();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (SelectConferenceRoom != null)
        {
            SelectConferenceRoom.OnReservationResponse += SelectConferenceRoom_OnReservationResponse;
           
        }
    }

    private void SelectConferenceRoom_OnReservationResponse(object sender, selectConferenceRoom.ReservationResponse e)
    {
        
        try
        {
            var components = ConfirmationPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            var messagePanel = components[0];

            //var messagePanel = ConfirmationPanel.GetComponent("MessageText").GetComponent<TMPro.TextMeshPro>();
            System.Text.StringBuilder strMessage = new System.Text.StringBuilder("Reservation Was " + (e.HasErrors ? "Not Successful Errors occured: \n" : "Successful!"));

            foreach (var errorMsg in e.ErrorMessages)
            {
                strMessage.AppendLine(errorMsg);
            }
            messagePanel.text = strMessage.ToString();

            ConfirmationPanel.SetActive(true);
            SetTimer();
        }
        catch
        { }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
