using System;
using System.Collections.Generic;
using MRTK.Tutorials.AzureCloudServices.Scripts.BotDirectLine;
using UnityEngine;


namespace LibAR
{

    public class larChatBotManager : MonoBehaviour
    {
        /// <summary>
        /// Conversation started event with conversation id.
        /// </summary>
        public event EventHandler<string> OnConversationStarted;

        /// <summary>
        /// Message sent event with message id.
        /// </summary>
        public event EventHandler<string> OnMessageSent;

        /// <summary>
        /// Messages received event with MessageActivity objects.
        /// </summary>
        public event EventHandler<IList<MessageActivity>> OnMessagesReceived;

       
        [SerializeField]
        private string botId = default;

        public bool isActive = false;

        private void Awake()
        {
            larBotDirectLineManager.Initialize(botId);
            larBotDirectLineManager.Instance.BotResponse += HandleBotResponse;
        }

        
        public void StartConversation()
        {
            if (isActive)
            {
                StartCoroutine(larBotDirectLineManager.Instance.StartConversationCoroutine());
            }

        }

        public System.Collections.IEnumerator ReceiveMessages(string conversationId)
        {
            yield return StartCoroutine(larBotDirectLineManager.Instance.GetMessagesCoroutine(conversationId));
        }

        public void SentMessage(string conversationId, string userId, string message)
        {
            StartCoroutine(larBotDirectLineManager.Instance.SendMessageCoroutine(conversationId, userId, message));
        }

        private void HandleBotResponse(object sender, BotResponseEventArgs e)
        {
            //Debug.Log($"Response from Bot of type: {e.EventType}");

            switch (e.EventType)
            {
                case EventTypes.None:

                    // Debug.Log(e.Message);
                    break;
                case EventTypes.ConversationStarted:
                    OnConversationStarted?.Invoke(this, e.ConversationId);
                    break;
                case EventTypes.MessageSent:
                    OnMessageSent?.Invoke(this, e.SentMessageId);
                    break;
                case EventTypes.MessageReceived:
                    OnMessagesReceived?.Invoke(this, e.Messages);
                    break;
                case EventTypes.Error:
                    Debug.Log(e.Message);
                    break;
            }
        }
    }
}