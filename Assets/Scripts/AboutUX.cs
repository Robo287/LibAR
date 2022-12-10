using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AboutUX : MonoBehaviour
{
    [SerializeField]
    TextMesh aboutBox;

    [SerializeField]
    TMPro.TMP_InputField directLine;

    [SerializeField]
    TMPro.TMP_InputField storageKey;

    [SerializeField]
    TMPro.TMP_InputField libcalUrl;

    [SerializeField]
    TMPro.TMP_InputField azfnAvailableRoomUrl;

    [SerializeField]
    TMPro.TMP_InputField azfnReserveRoomUrl;

    [SerializeField]
    TMPro.TMP_InputField cogServiceKey;

    [SerializeField]
    TMPro.TMP_InputField cogServiceRegion;

    [SerializeField]
    TMPro.TMP_InputField rtApiUrl;

    [SerializeField]
    TMPro.TMP_InputField libTicketUrl;

    // Start is called before the first frame update
    void Start()
    {
        if ((aboutBox == null) || (directLine == null) || (storageKey == null) || (libcalUrl == null) || (azfnAvailableRoomUrl == null) || (azfnReserveRoomUrl == null)||(cogServiceKey == null) || (cogServiceRegion == null) || (rtApiUrl == null) || (libTicketUrl == null))
        {
            Debug.Log("All of the TMP.TextMeshProUGUI properties need to be populated");
            return;
        }

        aboutBox.text = string.Format("LibAR Version: {0}\n", GlobalVars.versionNumber);
        directLine.text = GlobalVars.chatBotDirectLineSecret;
        storageKey.text = GlobalVars.azureStorageKey;
        libcalUrl.text = GlobalVars.LIBCAL_URL_BASE;
        azfnAvailableRoomUrl.text = GlobalVars.AZ_FN_GETAVAILABLE_ROOMS;
        azfnReserveRoomUrl.text = GlobalVars.AZ_FN_RESERVE_ROOM;
        cogServiceKey.text = GlobalVars.cogServicesKey;
        cogServiceRegion.text = GlobalVars.cogServiceRegion;
        rtApiUrl.text = GlobalVars.rtAPIUrl;
        libTicketUrl.text = GlobalVars.libTicketUrl;
    }

    // Update is called once per frame
    void Update()
    {
   
    }

    public void UpdateConfiguration()
    {
       GlobalVars.chatBotDirectLineSecret = directLine.text;
       GlobalVars.azureStorageKey = storageKey.text;
        GlobalVars.LIBCAL_URL_BASE = libcalUrl.text;
        GlobalVars.AZ_FN_GETAVAILABLE_ROOMS = azfnAvailableRoomUrl.text;
        GlobalVars.AZ_FN_RESERVE_ROOM = azfnReserveRoomUrl.text;
        GlobalVars.cogServicesKey = cogServiceKey.text;
        GlobalVars.cogServiceRegion = cogServiceRegion.text;
        GlobalVars.rtAPIUrl = rtApiUrl.text;
        GlobalVars.libTicketUrl = libTicketUrl.text;
        Debug.Log("Configuration Updated");
        
    }
}
