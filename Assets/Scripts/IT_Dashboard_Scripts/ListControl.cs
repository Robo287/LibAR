using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;

public class ListControl : MonoBehaviour
{
    [SerializeField] private GameObject buttonTemplate;
    public GridObjectCollection scrollList;
    public ScrollingObjectCollection scrollProperties;
    public GameObject BackPlate;

    private readonly string libticketURL = GlobalVars.libTicketUrl;
    private readonly string reflibticketURL = GlobalVars.rtAPIUrl;

    void Start()
    {
        StartCoroutine(APIHandler());
    }

    IEnumerator APIHandler()
    {
        Debug.Log(libticketURL);
        WWWForm form = new WWWForm();
        form.AddField("user", "eng2022ed1");
        form.AddField("pass", "ARInLibrary");
        using (UnityWebRequest auth = UnityWebRequest.Post(reflibticketURL, form)) //open an authenticated session to make API calls in
        {
            yield return auth.SendWebRequest();
            if (auth.result != UnityWebRequest.Result.Success)
            {
                // TODO: Error in environment
                Debug.Log(auth.error);
            }
            else
            {
                Debug.Log("Success");
                using (UnityWebRequest getTicketList = UnityWebRequest.Get(reflibticketURL + "/REST/1.0/search/ticket?query=Queue='General'ANDStatus='open'&orderby=-Created"))
                {
                    getTicketList.SetRequestHeader("referer", libticketURL); //set referer header
                    yield return getTicketList.SendWebRequest(); //make API call, wait for response
                    var resTicketList = getTicketList.downloadHandler.text;
                    string[] textList = resTicketList.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries); //split ticket into lines, ignore empty lines
                    var ticketList = new List<KeyValuePair<string, string>>();
                    foreach (var ticketText in textList) //for each ticket, create a key, value pair
                    {
                        var tmp = ticketText.Split(new string[] { ": " }, StringSplitOptions.None);
                        string ticketID;
                        string ticketSum;
                        if (tmp.Length > 1)
                        {
                            ticketID = tmp[0]; //isolate ticket ID number
                            if (tmp.Length > 2) //isolate summary info
                            {
                                ticketSum = tmp[1] + ": " + tmp[2];
                            }
                            else
                            {
                                ticketSum = tmp[1];
                            }
                            if (ticketSum.Length > 40) //truncate summaries that are too long
                            {
                                ticketSum = ticketSum.Substring(0, 40) + "...";
                            }
                            ticketList.Add(new KeyValuePair<string, string>(ticketID, ticketSum)); //create key value pair for each ticket ID and summary
                        }
                    }
                    foreach (var ticket in ticketList) //step through key value list and instantiate buttons
                    {
                        GameObject button = Instantiate(buttonTemplate) as GameObject;
                        button.SetActive(true);
                        button.GetComponent<ButtonControl>().SetText("Ticket Number: " + ticket.Key + System.Environment.NewLine + ticket.Value);
                        button.transform.SetParent(buttonTemplate.transform.parent, false);
                    }
                    scrollList.UpdateCollection();
                    scrollProperties.UpdateContent();
                    BackPlate.SetActive(true);
                }
            }
        }
    }
}
