using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Http;
using UnityEngine.Networking;

namespace Assets.Scripts.IT_Dashboard_Scripts
{
    public class ticketInfoProxy : MonoBehaviour
    {
        public string InfoText;
        private readonly string libticketURL = GlobalVars.libTicketUrl;
        private readonly string reflibticketURL = GlobalVars.rtAPIUrl;

        public event EventHandler<string> onTicketInfoComplete;

        public void showTicketInfo(string ticketNumber)
        {
            Debug.Log("showTicketInfo called, parsed: " + ticketNumber);
            StartCoroutine(APIHandler(ticketNumber));
        }

        public IEnumerator APIHandler(string ticketNumber)
        {
            Debug.Log("APIHandler Coroutine started, parsed: " + ticketNumber);
            // var uriString = "https://api.lib.fau.edu/systems/REST/1.0/ticket/" + ticketNumber + "/show";
            Uri getURI = new Uri(reflibticketURL + "/REST/1.0/ticket/" + ticketNumber);
            // Uri getURI = new Uri("https://api.lib.fau.edu/systems/REST/1.0/ticket/17599");

            WWWForm form = new WWWForm();
            form.AddField("user", "eng2022ed1");
            form.AddField("pass", "ARInLibrary");
            using (UnityWebRequest auth = UnityWebRequest.Post(reflibticketURL, form))
            {
                yield return auth.SendWebRequest();
                if (auth.result != UnityWebRequest.Result.Success)
                {
                    // TODO: Error message
                    Debug.Log(auth.error);
                }
                else
                {
                    Debug.Log("Success");
                    Debug.Log(getURI);
                    using (UnityWebRequest getTicketInfo = UnityWebRequest.Get(getURI))
                    {
                        getTicketInfo.SetRequestHeader("referer", reflibticketURL); //set referer header
                        yield return getTicketInfo.SendWebRequest();
                        Debug.Log(getTicketInfo.downloadHandler.text);
                        var infoText = getTicketInfo.downloadHandler.text; //display result
                        var tmp = infoText.Split(new string[] { "\n" }, StringSplitOptions.None); //create array of API response seperated by newline
                        var outputText = tmp[2] + "\n" + tmp[7] + "\n" + tmp[6] + "\n" + tmp[11] + "\n" + tmp[4] + "\n" + tmp[14] + "\n" + tmp[20]; //edit output text to make it more readable
                        InfoText = outputText;
                        if(onTicketInfoComplete != null)
                        {
                            Debug.Log(tmp[5]);
                            onTicketInfoComplete(this, outputText);
                        }
                    }
                }
            }
        }
    }
}
