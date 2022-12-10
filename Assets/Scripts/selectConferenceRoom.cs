using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Paroxe.SuperCalendar;
using UnityEngine.UI;
using System.Globalization;
using TMPro;
using ns = Newtonsoft;

public class selectConferenceRoom : MonoBehaviour
{

    #region "REST Serialization Classes"

    public class Bookings
    {
        public int id { get; set; }
        public string to { get; set; }
    }

    public class LibCalReserveRoomResponse
    {
        public string booking_id { get; set; }
        public string cost { get; set; }
        public string[] errors { get; set; }
    }

    public class ReserveRoomRequest
    {
        public string start { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public string email { get; set; }
        public string nickname { get; set; }
        public string q43 { get; set; }
        public float cost { get; set; }
        public Bookings[] bookings { get; set; }
    }

    public class RoomDates
    {
        public RoomDates()
        {


        }
        public RoomDates(string _to, string _from)
        {
            to = _to;
            from = _from;
        }
        public string from { get; set; }
        public string to { get; set; }
    }


    public class ReservationResponse
    {
        public ReservationResponse()
        {        }

        public ReservationResponse(string bookingId, string from, string to, string roomName, string eventName, bool hasErrors, string[] errorMessages)
        {
            BookingId = bookingId;
            Dates = new RoomDates(to, from);
            RoomName = roomName;
            EventName = eventName;
            HasErrors = hasErrors;
            ErrorMessages = errorMessages;
        }

        public RoomDates Dates;
        public string BookingId { get; set; }
        public string RoomName { get; set; }
        public string EventName { get; set; }
        public bool HasErrors { get; set; }
        public string[] ErrorMessages { get; set; }

    }
    #endregion

    List<Tuple<string,string>> from_to_Times;
    private  string LIBCAL_URL_BASE = GlobalVars.LIBCAL_URL_BASE;
    private  string AZ_FN_GETAVAILABLE_ROOMS = GlobalVars.AZ_FN_GETAVAILABLE_ROOMS;
    private  string AZ_FN_RESERVE_ROOM = GlobalVars.AZ_FN_RESERVE_ROOM;

    public event EventHandler<ReservationResponse> OnReservationResponse;


    [Header("Organizer Name")]
    public TMP_InputField Organizer;

    [Header("Organizer Email")]
    public TMP_InputField OrganizerEmail;

    [Header("Event Name")]
    public TMP_InputField EventName;


    [Header("Available Times")]
    public Dropdown AvailableTimes;

    [Header("Desired Length (In Minutes)")]
    public Dropdown LengthInMinutes;

#pragma warning disable 649
    [SerializeField]
    private Paroxe.SuperCalendar.Calendar m_Calendar;
#pragma warning restore 649

    Dropdown m_Dropdown;

    int roomSelected = 1;
    int lengthInMinutes = 10;
    string startDateTime = "00:00";
    
    private void RaiseEventOnReservationRequestComplete(ReservationResponse response)
    {
        if (OnReservationResponse != null)
        {
            OnReservationResponse(this, response);
        }
    }

    public void OnCalendarDateTimeChanged()
    {
       
        var formattedDate = m_Calendar.DateTime.ToString("yyyy-MM-dd");
        Debug.Log("Selected Date is: " + formattedDate);
        onDateSelected(formattedDate);        
    }

    private string token;

    private enum WebRequestMethods
    {
        Get,
        Post
    }

    private string urlRoomSelect;
    private string selectedDate;

    // Start is called before the first frame update
    void Start()
    {
        from_to_Times = new List<Tuple<string, string>>();
        urlRoomSelect = LIBCAL_URL_BASE + "115430?availability";
        m_Calendar.DateTime = DateTime.Now;
        m_Calendar.KeepSelected = true;
        m_Calendar.SelectionMode = Paroxe.SuperCalendar.Calendar.SelectionModeType.DateOnly;
        m_Calendar.SelectionState = Paroxe.SuperCalendar.Calendar.SelectionStateType.DaySelected;

        AvailableTimes.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // For testing purposes only
    public void DummyRun()
    {
        onRoomSelect(0);
        onDateSelected("2022-10-05");

    }

    private int transformMinuteSelectionToMinutes(int selection)
    {
        int minutes = 10;
        switch (selection)
        {
            case 1:
                minutes = 20;
                break;
            case 2:
                minutes = 30;
                break;
            case 3:
                minutes = 40;
                break;
            case 4:
                minutes = 50;
                break;
            case 5:
                minutes = 60;
                break;
            case 6:
                minutes = 90;
                break;
            case 7:
                minutes = 120;
                break;
            case 8:
                minutes = 180;
                break;
            case 9:
                minutes = 240;
                break;
            case 10:
                minutes = 300;
                break;
            case 11:
                minutes = 360;
                break;
            case 12:
                minutes = 420;
                break;
            case 13:
                minutes = 480;
                break;

            default:
                break;
        }
        return minutes;
    }

    private void CreateReservation(string _name, string _email, string eventName, int _roomSelected, string startDate="", string endDate = "")
    {
        var roomName = "Video Room 1";
        int roomId = 115430;
        if (_roomSelected != 1)
        {
            roomName = "Video Room 2";
            roomId = 115431;
        }

        var name = "";
        if (!_name.Contains(" ") )
        {
            name = _name + " ";
        }
        else
        {
            name = _name;
        }

        ReserveRoomRequest request = new ReserveRoomRequest()
        {
            fname = name.Split(' ')[0].Trim(),
            lname = name.Split(' ')[1].Trim(),
            cost = 0,
            email = _email,
            nickname = eventName,
            start = startDate,
            q43 = roomName,
            bookings = new Bookings[] {
            new Bookings() {
                id = roomId,
                to = endDate
                }
            }
        };

        StartCoroutine(postReservation(request));

    }

    public void onAvailableTimesSelected(int timeSelected)
    {
        var options = AvailableTimes.options.ToArray();
        var selection = AvailableTimes.value;
        var selectedOption = options[selection];

        if (selectedOption != null)
        {
            if (string.IsNullOrEmpty(selectedOption.text)) return;

            var data = selectedOption.text.Replace(" ~To~ ", "~");
            if (string.IsNullOrEmpty(data)) return;

            startDateTime = data.Split('~')[0];
          
        }
        
    }

    public void onSubmitClick()
    {
        var organizerName = Organizer.text;
        var email = OrganizerEmail.text;
        var eventName = EventName.text;

        //var date_ = new DateTime(m_Calendar.DateTime.Ticks, DateTimeKind.Local);

        var strStart = string.Format("{0}T{1}-0400", m_Calendar.DateTime.ToString("yyyy-MM-dd"), startDateTime);
        var startDate = new DateTime(DateTime.Parse(strStart).Ticks, DateTimeKind.Local);
        var endDate = startDate.AddMinutes(lengthInMinutes);
               
        CreateReservation(organizerName, email, eventName, roomSelected, string.Format("{0:O}", startDate), string.Format("{0:O}", endDate));
    }

    public void onMinuteLengthSelect(int choice)
    {
        lengthInMinutes = transformMinuteSelectionToMinutes(choice);
        if (AvailableTimes != null)
        {
            AvailableTimes.options.Clear();
        }
        StartCoroutine(getAvaiabilityTimes(roomSelected, m_Calendar.DateTime.ToString("yyyy-MM-dd")));

    }

    public void onRoomSelect(int _roomSelected)
    {
       // urlRoomSelect = string.Empty;
       roomSelected = _roomSelected + 1;    
       if(AvailableTimes != null)
        {
            AvailableTimes.options.Clear();
        }
      
        StartCoroutine(getAvaiabilityTimes(roomSelected, m_Calendar.DateTime.ToString("yyyy-MM-dd")));

    }

    public void onDateSelected(string date)
    {
        if (AvailableTimes != null)
        {
            AvailableTimes.options.Clear();
        }

     //   var urlRoomSelectDate = urlRoomSelect + "=" + date;
        // now we need to get the list of times
        //first get token
       // StartCoroutine(getToken());
        StartCoroutine(getAvaiabilityTimes(roomSelected, date));
    }


    private IEnumerator postReservation(ReserveRoomRequest request)
    {

        //?room=1&when=2022-10-05&howlong=30
        var url = AZ_FN_RESERVE_ROOM;

        var jsonReservation = ns.Json.JsonConvert.SerializeObject(request);

        var client = CreateWebRequest(WebRequestMethods.Post, url, jsonReservation );
        yield return client.SendWebRequest();

        var result = client.result;
        bool bsuccess = false;

        switch (result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.ProtocolError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.Log("Web request failed: " + client.error);
                break;
            default:
                bsuccess = true;
                break;
        }

        if (bsuccess)
        {
            ReservationResponse response = ns.Json.JsonConvert.DeserializeObject<ReservationResponse>(client.downloadHandler.text);
            RaiseEventOnReservationRequestComplete(response);

            Debug.Log("Response from Azure Fn Reserve Room - WasSuccessful: " + (!response.HasErrors).ToString());
            foreach (var errorMsg in response.ErrorMessages)
            {
                Debug.Log("Errors: " + errorMsg);
            }

        }

    }


    private IEnumerator getAvaiabilityTimes(int _roomSelected, string dateSelected ="")
    {

        //?room=1&when=2022-10-05&howlong=30
        var url = AZ_FN_GETAVAILABLE_ROOMS + "?room=" + _roomSelected.ToString() + "&when=" + dateSelected + "&howlong=" + lengthInMinutes.ToString();

        var client = CreateWebRequest(WebRequestMethods.Get, url);
        yield return client.SendWebRequest();
        
        var result = client.result;
        bool bsuccess = false;

        switch (result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.ProtocolError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.Log("Web request failed: " + client.error);
                break;
            default:
                bsuccess = true;
                break;
        }

        if (bsuccess)
        {
            var node = JSONNode.Parse(client.downloadHandler.text);
            var nodeArray = node.AsArray;

            foreach (JSONNode activityNode in nodeArray)
            {
                var Dates = activityNode["Dates"];
                //var timesAsArray = timesAsNode.AsArray;
                //foreach(JSONNode time in timesAsArray)
                {
                    var strFrom = Dates["from"].ToString().Trim().Replace("\"","");
                   
                    var strTo = Dates["to"].ToString().Trim().Replace("\"", ""); ;
                   
                    var from = DateTime.Parse(strFrom, null, DateTimeStyles.RoundtripKind).ToString("HH:mm");
                    var to = DateTime.Parse(strTo, null, DateTimeStyles.RoundtripKind).ToString("HH:mm");
                    var availTimes = $@"{from} ~To~ {to}";
                    Debug.Log(availTimes);
                    if(AvailableTimes != null )
                    {
                        List<Dropdown.OptionData> times = new List<Dropdown.OptionData>();
                        var data = new Dropdown.OptionData(availTimes);
                        times.Add(data);
                        AvailableTimes.AddOptions(times);
                    }
                    from_to_Times.Add(new Tuple<string, string>(from, to));

                }
                Debug.Log(from_to_Times);
            }
            AvailableTimes.value = 0;
        }
            
    }

    /// <summary>
    /// Creates a new UnityWebRequest instance initialized with bot authentication and JSON content type.
    /// </summary>
    /// <param name="webRequestMethod">Defines whether to use GET or POST method.</param>
    /// <param name="uri">The request URI.</param>
    /// <param name="content">The content to post (expecting JSON as UTF-8 encoded string or null).</param>
    /// <returns>A newly created UnityWebRequest instance.</returns>
    private UnityWebRequest CreateWebRequest(WebRequestMethods webRequestMethod, string uri, string content = null, string SecretKey = "")
    {
        Debug.Log("CreateWebRequest: " + webRequestMethod + "; " + uri + (string.IsNullOrEmpty(content) ? "" : ("; " + content)));

        UnityWebRequest webRequest = new UnityWebRequest();

        webRequest.url = uri;
        webRequest.SetRequestHeader("Authorization", "Bearer " + SecretKey);

        if (webRequestMethod == WebRequestMethods.Get)
        {
            webRequest.method = "GET";
        }
        else
        {
            webRequest.method = "POST";
        }

        if (!string.IsNullOrEmpty(content))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(Utf8StringToByteArray(content));
        }

        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        return webRequest;
    }

    private UnityWebRequest CreateWebRequestForToken(WebRequestMethods webRequestMethod, string uri, string content = null)
    {
        Debug.Log("CreateWebRequestForToken: " + webRequestMethod + "; " + uri + (string.IsNullOrEmpty(content) ? "" : ("; " + content)));

        UnityWebRequest webRequest = new UnityWebRequest();

        webRequest.url = uri;
        
        if (webRequestMethod == WebRequestMethods.Get)
        {
            webRequest.method = "GET";
        }
        else
        {
            webRequest.method = "POST";
        }

        if (!string.IsNullOrEmpty(content))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(Utf8StringToByteArray(content));
        }

        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        return webRequest;
    }


    private byte[] Utf8StringToByteArray(string stringToBeConverted)
    {
        return Encoding.UTF8.GetBytes(stringToBeConverted);
    }

}
