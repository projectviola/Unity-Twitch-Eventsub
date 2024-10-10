using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ProjectViola.Unity.TwitchAPI.EventSub
{
    public class TwitchChatSender : MonoBehaviour
    {
        private const string SEND_MESSAGE_URL = "https://api.twitch.tv/helix/chat/messages";
        private const string SEND_ANNOUNCEMENT_URL = "https://api.twitch.tv/helix/chat/announcements";

        private string clientId;
        private string accessToken;
        private string broadcasterId;
        private string senderId;

        public void Initialize(string clientId, string accessToken, string broadcasterId, string senderId)
        {
            this.clientId = clientId;
            this.accessToken = accessToken;
            this.broadcasterId = broadcasterId;
            this.senderId = senderId;
        }

        public void SendChatMessage(string message, string replyParentMessageId = null)
        {
            StartCoroutine(SendChatMessageCoroutine(message, replyParentMessageId));
        }

        public void SendAnnouncement(string message, string color = "primary")
        {
            StartCoroutine(SendAnnouncementCoroutine(message, color));
        }

        private IEnumerator SendChatMessageCoroutine(string message, string replyParentMessageId)
        {
            var requestData = new Dictionary<string, object>
            {
                ["broadcaster_id"] = broadcasterId,
                ["sender_id"] = senderId,
                ["message"] = message
            };

            if (!string.IsNullOrEmpty(replyParentMessageId))
            {
                requestData["reply_parent_message_id"] = replyParentMessageId;
            }

            string jsonBody = JsonConvert.SerializeObject(requestData);

            using (UnityWebRequest request = new UnityWebRequest(SEND_MESSAGE_URL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Client-ID", clientId);
                request.SetRequestHeader("Authorization", $"Bearer {accessToken}");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error sending chat message: {request.error}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                }
                else
                {
                    Debug.Log($"Chat message sent successfully");
                    Debug.Log($"Response: {request.downloadHandler.text}");
                }
            }
        }

        private IEnumerator SendAnnouncementCoroutine(string message, string color)
        {
            string url = $"{SEND_ANNOUNCEMENT_URL}?broadcaster_id={broadcasterId}&moderator_id={senderId}";

            var requestData = new Dictionary<string, string>
            {
                ["message"] = message,
                ["color"] = color
            };

            string jsonBody = JsonConvert.SerializeObject(requestData);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Client-ID", clientId);
                request.SetRequestHeader("Authorization", $"Bearer {accessToken}");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error sending announcement: {request.error}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                }
                else
                {
                    Debug.Log($"Announcement sent successfully");
                    Debug.Log($"Response: {request.downloadHandler.text}");
                }
            }
        }
    }
}