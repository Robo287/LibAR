using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Text;

namespace az_fn_ar_digital_twin_libcal_rooms
{
    public static class LibcalRooms
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

        public class RoomAvailability
        {
            public int id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string termsAndConditions { get; set; }
            public string image { get; set; }
            public int capacity { get; set; }
            public int formid { get; set; }
            public bool isBookableAsWhole { get; set; }
            public bool isAccessible { get; set; }
            public bool isPowered { get; set; }
            public bool isEventLocation { get; set; }
            public int zoneId { get; set; }
            public bool google { get; set; }
            public bool exchange { get; set; }
            public RoomDates[] availability { get; set; }

        }

      

        public class TokenResponse
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string token_type { get; set; }
            public string scope { get; set; }
        }

        public class ReservationResponse
        {
            public ReservationResponse()
            {

            }

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
        public class BlockOfTime
        {
            public BlockOfTime()
            {
            }


            public BlockOfTime(string _to, string _from) 
            {
                Dates = new RoomDates(_to, _from);
            }

            public RoomDates Dates;

        }

        #endregion

        private static string GetToken(ILogger log)
        {
            log.LogInformation("GetToken");

            var client = new RestClient("https://faulib.libcal.com/1.1/oauth/token");
            Uri reqUri = new Uri("https://faulib.libcal.com/1.1/oauth/token");
            var request = new RestRequest(reqUri, Method.Post);

            log.LogInformation("GetToken - Created Request");

            request.AddHeader("Content-Type", "application/json");
            var body = @"{
                " + "\n" +
                            @"    ""client_id"": 1061,
                " + "\n" +
                            @"    ""client_secret"": ""9ebe047b5aa84f6409248a43983fc6a1"",
                " + "\n" +
                            @"    ""grant_type"": ""client_credentials""
                " + "\n" +
                            @"}";

            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            log.LogInformation("GetToken - sent request to LibCal");

            TokenResponse jsonResponse = JsonConvert.DeserializeObject<TokenResponse>(response.Content);

            string strJsonResponse = jsonResponse?.ToString();

            log.LogInformation("GetToken - Parsing response " + response.Content);

            var token = jsonResponse.access_token;

            string sToken = string.IsNullOrEmpty(token) ? "91dc449f892ec7cb0dcfaa66aa96cc04e3e5eaa2" : token;

            log.LogInformation("GetToken - success");

            return sToken;
        }


        [FunctionName("GetAvailableRooms")]
        public static async Task<IActionResult> GetAvailableRooms(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Get Available room request");

            string room = req.Query["room"];
            string howlong = req.Query["howlong"];
            string when = req.Query["when"];


            log.LogInformation("Get Available room request - Retrieved params");

            int iHowLong = 10;
            int.TryParse(howlong, out iHowLong);

            
            var token = GetToken(log);

            log.LogInformation("Get Available room request - Received Token");

            var roomId = "";

            if (room == "1")
                roomId = "115430";
            else
                roomId = "115431";

            var surl = "";
            if (!string.IsNullOrEmpty(when))
            {
                surl = "https://faulib.libcal.com/1.1/space/item/" + roomId + "?availability=" + when;
            }
            else
                surl = "https://faulib.libcal.com/1.1/space/item/" + roomId + "?availability";

            var client = new RestClient(surl);
            var reqUri = new Uri(surl);
            var request = new RestRequest(reqUri, Method.Get);
        

            request.AddHeader("Authorization", "Bearer " + token);
            var response = client.Execute(request);

            log.LogInformation("Get Available room request - executed  request to get rooms from LibCal");

            log.LogInformation("Get Available room request - LibCal Response: " + response.Content);

            RoomAvailability[] jsonResponse = JsonConvert.DeserializeObject<RoomAvailability[]>(response.Content);


            // content is now room availability
            var availabilities = jsonResponse[0].availability;

            List<BlockOfTime> blocksOfTime = new List<BlockOfTime>();

            log.LogInformation("Get Available room request - received LibCal response, looping thru available times now");

            foreach (var p_availability in availabilities)
            {

                DateTime dateDesiredFrom = DateTime.Parse(p_availability.from);
                DateTime dateDesiredTo = dateDesiredFrom.AddMinutes(iHowLong);

                foreach (var availability in availabilities)
                {
                    DateTime dateTo = DateTime.Parse(availability.to);
                    DateTime dateFrom = DateTime.Parse(availability.from);

                    if (dateTo == dateDesiredTo)
                    {
                        // found a spot
                        blocksOfTime.Add(new BlockOfTime(dateTo.ToString(), dateDesiredFrom.ToString()));
                        break;
                    }

                }
            }

            string responseMessage = JsonConvert.SerializeObject(blocksOfTime);
            log.LogInformation("Get Available room request - Created Block of times as response");

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("ReserveRoom")]
        public static async Task<IActionResult> ReserveRoom(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("ReserveRoom");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ReserveRoomRequest data = JsonConvert.DeserializeObject<ReserveRoomRequest>(requestBody);

            log.LogInformation("ReserveRoom: Request - " + requestBody);

            string from = data.start;
            string to = data.bookings[0].to;
            string eventName = data.nickname;
            string roomName = data.q43;

            log.LogInformation("ReserveRoom: read from, to, and eventName");
            
            //name = name ?? data?.name;
            var token = GetToken(log);

            var url = "https://faulib.libcal.com/1.1/space/reserve";
            var client = new RestClient(url);
            var uri = new Uri(url);
            
            var request = new RestRequest(uri, Method.Post);
            log.LogInformation("ReserveRoom: Created the RestRequest");

            request.AddHeader("Authorization", "Bearer " + token);

            request.AddHeader("Content-Type", "text/plain");
            string body = requestBody;
            
            request.AddParameter("text/plain", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            LibCalReserveRoomResponse jsonResponse = JsonConvert.DeserializeObject<LibCalReserveRoomResponse>(response.Content);
            log.LogInformation("ReserveRoom: Response from LibCal: " + response.Content);
            
            var bookingId = jsonResponse.booking_id;
            bool hasErrors = false;
            List<string> errorMessages = new List<string>();

            if (string.IsNullOrEmpty(bookingId))
            {
                hasErrors = true;
                foreach(var errorMessage in jsonResponse.errors)
                {
                    errorMessages.Add(errorMessage);
                }
            }
            ReservationResponse reserved = new ReservationResponse(bookingId, from, to, roomName, eventName, hasErrors, errorMessages.ToArray());
          
            string responseMessage = JsonConvert.SerializeObject(reserved);
            log.LogInformation("ReserveRoom: Created Reservation response successfully: " + responseMessage);
            return new OkObjectResult(responseMessage);
        }
    }
}
