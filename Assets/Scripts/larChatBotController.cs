using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;



namespace LibAR
{

    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;

    public class ListeningBecauseSpeakingEventArgs : EventArgs
    {
        public string Message;
        public ListeningBecauseSpeakingEventArgs(string message)
        {
            Message = message;
        }
    }


    public class larChatBotController : MonoBehaviour
    {
        public bool IsListening { get; private set; }
        public bool IsSpeaking { get; private set; }

        [SerializeField]
        private larChatBotManager chatBotManager = default;

        [Header("References")]
        [SerializeField]
        private Txt2SpeechManager textToSpeechManager = default;

        [Header("UI Elements")]
        [SerializeField]
        public UnityEngine.UI.Text messageLabel = default;

        [Header("Events")]
        [SerializeField]
        private UnityEvent onConversationStarted = default;
        [SerializeField]
        private UnityEvent onConversationFinished = default;


        public delegate void OnStopListeningBecauseSpeaking(ListeningBecauseSpeakingEventArgs args);
        public event OnStopListeningBecauseSpeaking onStopListeningBecauseSpeaking;

        public delegate void OnStartListeningBecauseSpeakingStopped(ListeningBecauseSpeakingEventArgs args);
        public event OnStartListeningBecauseSpeakingStopped onStartListeningBecauseSpeakingStopped;


        private bool isPerformingInit;
        private string userId = Guid.NewGuid().ToString().Replace("-", "");
        private string conversationId;
        private List<string> processedMessages = new List<string>();

        private void Awake()
        {
            chatBotManager.OnConversationStarted += HandleOnConversationStarted;
            chatBotManager.OnMessageSent += HandleOnMessageSent;
            chatBotManager.OnMessagesReceived += HandleOnMessagesReceived;
        }

        private void OnEnable()
        {
           // StartConversation();
        }

        private void OnDisable()
        {
            StopConversation();
        }
        
        public void SpeakText(string message)
        {

            IsSpeaking = textToSpeechManager.AudioSource.isPlaying;
            IsListening = !textToSpeechManager.AudioSource.isPlaying;

            if (IsSpeaking) return;

            if (IsListening)
            {
                if (textToSpeechManager != null)
                    textToSpeechManager.SpeakText(message);
            }

            do
            {
                onStopListeningBecauseSpeaking(new ListeningBecauseSpeakingEventArgs(message));
                

            } while (textToSpeechManager.AudioSource.isPlaying);

            IsSpeaking = textToSpeechManager.AudioSource.isPlaying;
            IsListening = !textToSpeechManager.AudioSource.isPlaying;

            if (IsListening)
            {
                onStartListeningBecauseSpeakingStopped(new ListeningBecauseSpeakingEventArgs(message));
            }
        }

        public void StartConversation()
        {
            if (isPerformingInit)
            {
                return;
            }

            Debug.Log("Starting conversation with Bot.");
            isPerformingInit = true;

            messageLabel.text = "Starting conversation, please wait.";
            chatBotManager.StartConversation();
            onConversationStarted?.Invoke();
        }

        public void StopConversation()
        {
            IsListening = false;
            conversationId = null;
            processedMessages = new List<string>();
            onConversationFinished?.Invoke();
        }

        public void StartListening()
        {
            if (isPerformingInit || IsListening || IsSpeaking)
            {
                return;
            }

            IsListening = true;
            messageLabel.text = "Listening...";

        }

        private void HandleOnConversationStarted(object sender, string id)
        {
            conversationId = id;
            isPerformingInit = false;
            // var greetingMessage = "Greetings, I'm Owlsey. I can help you find books, get help or book a room. Just ask.";
            // textToSpeechManager.SpeakText(greetingMessage);
            // messageLabel.text = greetingMessage;

        }

        private void HandleOnMessagesReceived(object sender, IList<MessageActivity> messages)
        {
            StartCoroutine(HandleReceivedMessagesCoroutine(messages));
        }

        private void HandleOnMessageSent(object sender, string messageId)
        {
            StartCoroutine(chatBotManager.ReceiveMessages(conversationId));
        }

        IEnumerator HandleReceivedMessagesCoroutine(IList<MessageActivity> messages)
        {
            // Debug.Log("HandleReceivedMessages");

            // Wait in case previous message are still being processed.
            while (IsSpeaking)
            {
                yield return new WaitForSeconds(0.5f);
            }
            IsSpeaking = true;
            IsListening = false;
            messageLabel.text = String.Empty;

            var textToSpeech = new StringBuilder();

            foreach (var messageActivity in messages.Where(m => m.FromId != userId))
            {
                if (processedMessages.Contains(messageActivity.Id))
                {
                    continue;
                }
                if (messageActivity.Text.Contains("Greetings"))
                {
                    continue;
                }

                processedMessages.Add(messageActivity.Id);

                //Debug.Log($"Appending message: {messageActivity.Text}");
                textToSpeech.AppendLine(messageActivity.Text);

            }

            var txtMessage = textToSpeech.ToString();
            textToSpeech.Clear();
            Debug.Log($"Bot will say: {txtMessage}");
            messageLabel.text = txtMessage;

            textToSpeechManager.SpeakText(txtMessage);
            
            do
            {
                onStopListeningBecauseSpeaking(new ListeningBecauseSpeakingEventArgs(txtMessage));
                yield return new WaitForSeconds(1.0f);

            } while (textToSpeechManager.AudioSource.isPlaying);
            //yield return new WaitWhile(() => textToSpeechManager.AudioSource.isPlaying);

            //System.IO.File.Delete("tmp-owlsey.mp3");
            IsSpeaking = textToSpeechManager.AudioSource.isPlaying;
            IsListening = !textToSpeechManager.AudioSource.isPlaying;
            do
            {
                onStopListeningBecauseSpeaking(new ListeningBecauseSpeakingEventArgs(txtMessage));
                yield return new WaitForSeconds(1.0f);

            } while (textToSpeechManager.AudioSource.isPlaying);

            IsSpeaking = textToSpeechManager.AudioSource.isPlaying;
            IsListening = !textToSpeechManager.AudioSource.isPlaying;
            onStartListeningBecauseSpeakingStopped(new ListeningBecauseSpeakingEventArgs(txtMessage));
            //chatBotManager.SentMessage(conversationId, userId, "");

        }

        public void OnDictationComplete(string detectedDictation)
        {
            detectedDictation = SanitizeDictation(detectedDictation);

            // Debug.Log($"Dictation received: {detectedDictation}");
            // messageLabel.text = "Ok, let me process that quickly.";
            Debug.Log($"Sending message to bot: {detectedDictation}");
            //IsListening = false;
            chatBotManager.SentMessage(conversationId, userId, detectedDictation);
        }

        private string SanitizeDictation(string dictation)
        {
            dictation = dictation.Replace(".", "");
            dictation = dictation.Replace("\n", "");
            dictation = dictation.Replace("\r", "");
            dictation = dictation.Replace("\\", "");

            if (dictation.EndsWith(" ") && dictation.Length > 2)
            {
                dictation = dictation.Remove(dictation.Length - 1, 1);
            }

            return dictation;
        }
    }
}