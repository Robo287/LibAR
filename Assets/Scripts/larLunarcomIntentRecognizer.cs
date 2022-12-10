using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Windows.Speech;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace LibAR
{

    /*
     * 
     * Sample code taken from MRTK Tutorials
     * https://docs.microsoft.com/en-us/learn/modules/azure-speech-services-tutorials-mrtk/3-exercise-integrate-speech-recognition-transcription
     * 
     * 
     */

    public class larLunarcomIntentRecognizer : MonoBehaviour
    {
        [Header("LUIS Credentials")]
        public string luisEndpoint = "";
        public larChatBotController chatBotController;
        public GameObject micButton;

        [Header("Cube to show")]
        public GameObject foundSearch;

        DictationRecognizer dictationRecognizer;
        larLunarcomController lunarcomController;
        public GetBooks bookProxy;

        string recognizedString;
        bool capturingAudio = false;
        bool commandCaptured = false;
        bool firstRun = false;
        bool attemptedDictation = false;

        Assets.Scripts.IT_Dashboard_Scripts.ticketInfoProxy ticketProxy;

        void Start()
        {
            lunarcomController = larLunarcomController.lunarcomController;

            if (lunarcomController.outputText == null)
            {
                Debug.LogError("outputText property is null! Assign a UI Text element to it.");
            }

            if (chatBotController == null)
            {
                Debug.LogError("need the chat bot controller to launch the dialog bot");
            }

            chatBotController.onStartListeningBecauseSpeakingStopped += ChatBotController_onStartListeningBecauseSpeakingStopped;
            chatBotController.onStopListeningBecauseSpeaking += ChatBotController_onStopListeningBecauseSpeaking;
            lunarcomController.onSelectRecognitionMode += HandleOnSelectRecognitionMode;
            ticketProxy = new Assets.Scripts.IT_Dashboard_Scripts.ticketInfoProxy();
            ticketProxy.onTicketInfoComplete += Proxy_onTicketInfoComplete;

        }

        private void ChatBotController_onStopListeningBecauseSpeaking(ListeningBecauseSpeakingEventArgs args)
        {
            if (dictationRecognizer != null)
            {
                if (dictationRecognizer.Status == SpeechSystemStatus.Stopped) return;

                dictationRecognizer.Stop();
                // CompleteButtonPress("toggle microphone", "microphone", micButton, true, false);
                var responseMsg = args.Message;
                //StartCoroutine(processResponseMessage(responseMsg));
                processResponseMessage(responseMsg);
            }
            
        }

        private void processResponseMessage(string responseMsg)
        {
            //Need to strip
            // 
            // string format:
            // Author: XXX\r\n
            // Subject: XXX\r\n
            // Title: xxx\r\n
            //
            //Find author
            var author = "";
            bool isFindBook = false;
            bool isGetTicketInfo = false;

            if (responseMsg.IndexOf("Author: ") != -1)
            {
                isFindBook = true;
                int authorPos = responseMsg.IndexOf("Author: ") + 8;
                int authorPosEnd = responseMsg.IndexOf("\r\n", authorPos);
                author = responseMsg.Substring(authorPos, authorPosEnd - authorPos);
            }


            //find subject
            var subject = "";
            if (responseMsg.IndexOf("Subject: ") != -1)
            {
                isFindBook = true;
                int subjectPos = responseMsg.IndexOf("Subject: ") + 9;
                int subjectPosEnd = responseMsg.IndexOf("\r\n", subjectPos);
                subject = responseMsg.Substring(subjectPos, subjectPosEnd - subjectPos);
            }

            var title = "";
            if (responseMsg.IndexOf("Title: ") != -1)
            {
                isFindBook = true;
                int titlePos = responseMsg.IndexOf("Title: ") + 7;
                int titlePosEnd = responseMsg.IndexOf("\r\n", titlePos);
                title = responseMsg.Substring(titlePos, titlePosEnd - titlePos);
            }

            var ticketNumber = "";
            if (responseMsg.IndexOf("TicketNumber: ") != -1)
            {
                isGetTicketInfo = true;
                int ticketPos = responseMsg.IndexOf("TicketNumber: ") + 14;
                int ticketPosEnd = responseMsg.IndexOf("\r\n", ticketPos);
                ticketNumber = responseMsg.Substring(ticketPos, ticketPosEnd - ticketPos).Replace("ticket", "").Replace("number", "").Trim();
            
            }


            if (isFindBook)
            {
                if (author.ToLower().Contains("anthony") || author.ToLower().Contains("enzo") || author.ToLower().Contains("shivani") || author.ToLower().Contains("rawan") || author.ToLower().Contains("dwight") )
                {
                    chatBotController.SpeakText($@"I tell you what if we don't have a book by {author} you might want to try Amazon, because {author} is the best in the whole world... But I'm still searching. ");
                }

                if (bookProxy != null)
                {
                    //foundSearch = bookProxy.callGetBooks(subject, title, author);
                    //foundSearch.SetActive(true);
                    GlobalVars.getDirectionBookTitle = title;
                    SceneManager.LoadScene("Directions");
                }
            }
            else if (isGetTicketInfo)
            {
                StartCoroutine(ticketProxy.APIHandler(ticketNumber));

            }
        }

        private void Proxy_onTicketInfoComplete(object sender, string e)
        {  
            recognizedString = e;
            lunarcomController.UpdateLunarcomText(recognizedString);

        }

        private void ChatBotController_onStartListeningBecauseSpeakingStopped(ListeningBecauseSpeakingEventArgs args)
        {
            if (dictationRecognizer != null && !chatBotController.IsSpeaking && chatBotController.IsListening)
            {
                if (dictationRecognizer.Status == SpeechSystemStatus.Stopped)
                {
                    dictationRecognizer.Stop();
                    dictationRecognizer.Start();
                    capturingAudio = true;
            //        CompleteButtonPress("toggle microphone", "microphone", micButton, true, true);
                    return;
                }

                if (dictationRecognizer.Status == SpeechSystemStatus.Failed)
                {
                    dictationRecognizer.Stop();
                    dictationRecognizer.Start();
                    capturingAudio = true;
            //        CompleteButtonPress("toggle microphone", "microphone", micButton, true, true);
                }
                
               
            }
            //CompleteButtonPress("toggle microphone", "microphone", micButton, true, false);
        }

        public void HandleOnSelectRecognitionMode(RecognitionMode recognitionMode)
        {
            if (recognitionMode == RecognitionMode.Intent_Recognizer)
            {
                recognizedString = "Listening, please say something...";
                BeginRecognizing();
            }
            else
            {
                if (capturingAudio)
                {
                    StopCapturingAudio();
                }
                recognizedString = "";
                commandCaptured = false;
            }
        }

        private void BeginRecognizing()
        {
            chatBotController.StartConversation();
            chatBotController.StartListening();
            LaunchOwlseyAssistant();
            firstRun = true;
            if (Microphone.devices.Length > 0)
            {
                if (dictationRecognizer == null)
                {
                    dictationRecognizer = new DictationRecognizer
                    {
                        InitialSilenceTimeoutSeconds = 45,
                        AutoSilenceTimeoutSeconds = 10
                    };

                    dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
                    dictationRecognizer.DictationError += DictationRecognizer_DictationError;
                    dictationRecognizer.DictationComplete += DictationRecognizer_DictationComplete;
                    dictationRecognizer.DictationHypothesis += DictationRecognizer_DictationHypothesis;
                }


                if (dictationRecognizer.Status == SpeechSystemStatus.Stopped)
                {
                    dictationRecognizer.Stop();
                    dictationRecognizer.Start();
                    capturingAudio = true;
                }

            }
        }

        private void DictationRecognizer_DictationHypothesis(string text)
        {
            // Debug.Log($@"Hypothesis: {text}");
            capturingAudio = true;
            attemptedDictation = true;
        }

        private void DictationRecognizer_DictationComplete(DictationCompletionCause cause)
        {
            //Debug.Log($@"Status {dictationRecognizer.Status} - Cause: {cause}");
            switch (cause)
            {
                case DictationCompletionCause.Canceled:
                case DictationCompletionCause.TimeoutExceeded:
                case DictationCompletionCause.PauseLimitExceeded:
                case DictationCompletionCause.UnknownError:
                case DictationCompletionCause.NetworkFailure:
                case DictationCompletionCause.MicrophoneUnavailable:
                    capturingAudio = false;
                    break;
            }

            if (!capturingAudio)
            {
                //we need to reset
                CompleteButtonPress("toggle microphone", "microphone", micButton, true, false);

            }
            else
            {

                if (attemptedDictation)
                {
                    attemptedDictation = false;
                }
                else
                {
                    CompleteButtonPress("toggle microphone", "microphone", micButton, true, false);
                }
                // this is probably right after ditaction ends
                capturingAudio = false;
            }
        }

        public void StopCapturingAudio()
        {
            chatBotController.StopConversation();

            if (dictationRecognizer != null && dictationRecognizer.Status != SpeechSystemStatus.Stopped)
            {
                dictationRecognizer.DictationResult -= DictationRecognizer_DictationResult;
                dictationRecognizer.DictationError -= DictationRecognizer_DictationError;
                dictationRecognizer.Stop();
                dictationRecognizer = null;
                capturingAudio = false;
            }

        }

        private void DictationRecognizer_DictationResult(string dictationCaptured, ConfidenceLevel confidence)
        {
            recognizedString = dictationCaptured;
            chatBotController.OnDictationComplete(recognizedString);
            capturingAudio = true;
        }

        private void DictationRecognizer_DictationError(string error, int hresult)
        {
            Debug.Log("Dictation exception: " + error);
            lunarcomController.outputText.text = "Dictation error: " + error;
        }

        [Serializable]
        class AnalysedQuery
        {
            public string query = default;
            public TopScoringIntentData prediction = default;
            // public EntityData[] entities = default;
            ///public string query;
        }

        [Serializable]
        class TopScoringIntentData
        {
            public string topIntent = default;
            public LuisIntents intents = default;
            public EntityData entities = default;
            public SentimentData sentiment = default;
            ///public float score;
        }


        [Serializable]
        class LuisIntents
        {
            public Intent FindBook;
            public Intent None;
        }


        [Serializable]
        class Intent
        {
            public float score = default;
        }

        [Serializable]
        class SentimentData
        {
            public string label = default;
            public float score = default;
        }

        [Serializable]
        class EntityData
        {
            public string[] FindBookTitle = default;
            public string[] FindBookAuthor = default;
            public string[] FindBookSubject = default;
            public FindBookInstance instance;
            ///public int startIndex;
            ///public int endIndex;
            ///public float score;
        }

        [Serializable]
        class FindBookInstance
        {
            public string[] FindBookSubject = default;
        }

        public IEnumerator SubmitRequestToLuis(string dictationResult, Action done)
        {
            string queryString = string.Concat(Uri.EscapeDataString(dictationResult));

            using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(luisEndpoint + queryString))
            {
                yield return unityWebRequest.SendWebRequest();

                if (unityWebRequest.isNetworkError || unityWebRequest.isHttpError)
                {
                    Debug.Log(unityWebRequest.error);
                }
                else
                {
                    try
                    {
                        AnalysedQuery analysedQuery = JsonUtility.FromJson<AnalysedQuery>(unityWebRequest.downloadHandler.text);
                        UnpackResults(analysedQuery);
                    }
                    catch (Exception exception)
                    {
                        Debug.Log("Luis Request Exception Message: " + exception.Message);
                    }
                }

                done();
                yield return null;
            }
        }

        private void UnpackResults(AnalysedQuery aQuery)
        {
            string topIntent = aQuery.prediction.topIntent;

            Dictionary<string, string> entityDic = new Dictionary<string, string>();

            //foreach (EntityData ed in aQuery.prediction.entities)
            //{
            //    entityDic.Add(ed.type, ed.entity);
            //}

            switch (topIntent)
            {
                case "AskForHelp":

                    LaunchOwlseyAssistant();

                    break;
                case "FindBook":
                    //string actionToTake = null;
                    //string targetButton = null;
                    string title = "";
                    string author = "";
                    string subject = "";

                    if (aQuery.prediction.entities.FindBookTitle != null)
                        title = aQuery.prediction.entities.FindBookTitle[0];//Save action intent

                    if (aQuery.prediction.entities.FindBookAuthor != null)
                        author = aQuery.prediction.entities.FindBookAuthor[0];//Save target intent

                    if (aQuery.prediction.entities.FindBookSubject != null)
                        subject = aQuery.prediction.entities.FindBookSubject[0];//Save target intent

                    Debug.Log(string.Format("Looking for a book with\nTitle: {0} - Author: {1} - Subject: {2}", title, author, subject));

                    //foreach (var pair in entityDic)
                    //{
                    //    if (pair.Key == "Target")
                    //    {
                    //        targetButton = pair.Value;
                    //    }
                    //    else if (pair.Key == "Action")
                    //    {
                    //        actionToTake = pair.Value;
                    //    }
                    //}
                    var bookProxy = new GetBooks();
                    bookProxy.callGetBooks(subject, title, author);

                    ProcessResults("mictoggle", title, author, subject);
                    break;
                default:
                    ProcessResults("mictoggle");
                    break;
            }
        }

        public void ProcessResults(string targetButton = null, string title = null, string author = null, string subject = null)
        {
            switch (targetButton)
            {
                //    case "launch":
                //        CompleteButtonPress(actionToTake, targetButton, LaunchButton);
                //        break;
                //    case "reset":             
                //        CompleteButtonPress(actionToTake, targetButton, ResetButton);
                //        break;
                //    case "hint":
                //        CompleteButtonPress(actionToTake, targetButton, HintsButton);
                //        break;
                case "mictoggle":
                    CompleteButtonPress("toggle microphone", "microphone", micButton);
                    break;
                default:
                    CompleteButtonPress();
                    break;
            }
        }

        private void CompleteButtonPress(string actionToTake = null, string buttonName = null, GameObject buttonToPush = null, bool overideSelected = false, bool isSelected = false)
        {
            // recognizedString += (actionToTake != null) ? "\n\nAction: " + actionToTake : "\n\nAction: -";
            // recognizedString += (buttonName != null) ? "\nTarget: " + buttonName : "\nTarget: -";

            if (actionToTake != null && buttonName != null && buttonToPush != null)
            {
                //recognizedString += "\n\nCommand recognized, pushing the '" + buttonName + "' button because I was told to '" + actionToTake + "'";
                buttonToPush.GetComponent<Button>().onClick.Invoke();
                //lunarcomController.outputText.text = "Click the microphone to speak to the assistant.";
                foreach (larLunarcomButtonController button in lunarcomController.buttons)
                {
                    if (button.name == buttonName)
                    {                        
                        button.ToggleSelected(overideSelected, isSelected);
                    }
                }
            }
            else
            {
                recognizedString += "\n\nCommand not recognized";
            }
            commandCaptured = true;
        }

        private void Update()
        {
            if (lunarcomController.CurrentRecognitionMode() == RecognitionMode.Intent_Recognizer)
            {
                lunarcomController.UpdateLunarcomText(recognizedString);

                if (commandCaptured)
                {
                    //foreach (larLunarcomButtonController button in lunarcomController.buttons)
                    //{
                    //    if (button.GetIsSelected())
                    //    {
                    //        button.DeselectButton();
                    //    }
                    //}
                    commandCaptured = false;
                }
            }
        }

        void OnDestroy()
        {
            if (dictationRecognizer != null)
            {
                dictationRecognizer.Dispose();
            }
        }

        void LaunchOwlseyAssistant()
        {
            lunarcomController.LaunchOwlseyAssistant();
        }
    }
}