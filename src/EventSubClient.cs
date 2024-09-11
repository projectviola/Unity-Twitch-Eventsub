using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using NativeWebSocket;
using Newtonsoft.Json;

namespace ProjectViola.Unity.TwitchAPI.EventSub
{

    public class EventSubClient : MonoBehaviour
    {
        private const string MockWebSocketUrl = "ws://127.0.0.1:8080/ws";
        private const string MockApiUrl = "http://127.0.0.1:8080/eventsub/subscriptions";
        private const string DefaultWebSocketUrl = "wss://eventsub.wss.twitch.tv/ws";
        private const string TwitchApiUrl = "https://api.twitch.tv/helix/eventsub/subscriptions";

        private string clientId;
        private string accessToken;
        private string userId;
        private WebSocket webSocket;
        private IEventHandler eventHandler;
        private string sessionId;
        private bool isConnected = false;
        private string currentWebSocketUrl;
        private string currentApiUrl;
        private bool useMockServer;

        public bool ConnectionEstablished 
        {
            get 
            {
                return isConnected;
            }
        }

        public void Initialize(string clientId, string accessToken, string userId, IEventHandler handler, bool useMockServer)
        {
            this.clientId = clientId;
            this.accessToken = accessToken;
            this.userId = userId;
            this.eventHandler = handler;
            this.useMockServer = useMockServer;
            this.currentWebSocketUrl = useMockServer ? MockWebSocketUrl : DefaultWebSocketUrl;
            this.currentApiUrl = useMockServer ? MockApiUrl : TwitchApiUrl;
        }

        public void Connect()
        {
            StartCoroutine(ConnectAndReconnect());
        }

        private IEnumerator ConnectAndReconnect()
        {
            while (true)
            {
                if (webSocket == null || webSocket.State == WebSocketState.Closed)
                {
                    Debug.Log($"Attempting to connect to WebSocket at URL: {currentWebSocketUrl}");
                    webSocket = new WebSocket(currentWebSocketUrl);

                    webSocket.OnOpen += OnWebSocketOpen;
                    webSocket.OnMessage += OnWebSocketMessage;
                    webSocket.OnError += OnWebSocketError;
                    webSocket.OnClose += OnWebSocketClose;

                    yield return webSocket.Connect();

                    if (webSocket.State != WebSocketState.Open)
                    {
                        Debug.LogError($"Failed to connect to WebSocket. Current state: {webSocket.State}");
                        yield return HandleReconnect();
                    }
                    else
                    {
                        isConnected = true;
                    }
                }

                while (webSocket.State == WebSocketState.Open)
                {
                    yield return new WaitForSeconds(1f);
                }

                Debug.Log("WebSocket is no longer open. Attempting to reconnect...");
                yield return HandleReconnect();
            }
        }

        private IEnumerator HandleReconnect()
        {
            int delay = 15;
            Debug.Log($"Waiting {delay} seconds before reconnecting...");
            yield return new WaitForSeconds(delay);
        }

        private IEnumerator ConnectWebSocket()
        {
            webSocket = new WebSocket(currentWebSocketUrl);

            webSocket.OnOpen += OnWebSocketOpen;
            webSocket.OnMessage += OnWebSocketMessage;
            webSocket.OnError += OnWebSocketError;
            webSocket.OnClose += OnWebSocketClose;

            yield return webSocket.Connect();

            if (webSocket.State != WebSocketState.Open)
            {
                Debug.LogError($"Failed to connect to WebSocket. Current state: {webSocket.State}");
            }
            else
            {
                isConnected = true;
                Debug.Log("WebSocket connected successfully");
            }
        }

        private void OnWebSocketOpen()
        {
            Debug.Log("WebSocket connection opened");
        }

        private void OnWebSocketMessage(byte[] data)
        {
            string jsonMessage = Encoding.UTF8.GetString(data);
            ParseAndHandleMessage(jsonMessage);
        }

        private void OnWebSocketError(string errorMsg)
        {
            Debug.LogError($"WebSocket error: {errorMsg}");
        }

        private void OnWebSocketClose(WebSocketCloseCode closeCode)
        {
            Debug.Log($"WebSocket connection closed with code: {closeCode}");
            isConnected = false;
        }

        private void ParseAndHandleMessage(string jsonMessage)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<EventSubMessage>(jsonMessage);

                switch (message.metadata.message_type)
                {
                    case "session_welcome":
                        HandleWelcomeMessage(message);
                        break;
                    case "notification":
                        HandleNotification(message);
                        break;
                    case "session_keepalive":
                        Debug.Log("Received keepalive message");
                        break;
                    case "revocation":
                        HandleRevocation(message);
                        break;
                    default:
                        Debug.LogWarning($"Unknown message type: {message.metadata.message_type}");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing message: {e.Message}");
                Debug.LogError($"Stack Trace: {e.StackTrace}");
                Debug.LogError($"Message: {jsonMessage}");
            }
        }

        private void HandleWelcomeMessage(EventSubMessage message)
        {
            if (message.payload?.session != null)
            {
                sessionId = message.payload.session.id;
                Debug.Log($"Received session ID: {sessionId}");
                StartCoroutine(SubscribeToAllEvents());
            }
            else
            {
                Debug.LogError("Failed to get session ID from welcome message.");
                Debug.LogError($"Welcome message content: {JsonConvert.SerializeObject(message)}");
            }
        }

        private void HandleNotification(EventSubMessage message)
        {
            Debug.Log($"Received notification of type: {message.metadata.subscription_type}");

            switch (message.metadata.subscription_type)
            {
                case "channel.chat.message":
                    eventHandler.OnChatMessage(message.payload.@event);
                    break;
                case "channel.follow":
                    eventHandler.OnFollow(message.payload.@event);
                    break;
                case "channel.subscribe":
                    eventHandler.OnSubscription(message.payload.@event);
                    break;
                case "channel.subscription.gift":
                    eventHandler.OnSubscriptionGift(message.payload.@event);
                    break;
                case "channel.cheer":
                    eventHandler.OnCheer(message.payload.@event);
                    break;
                case "channel.raid":
                    eventHandler.OnRaid(message.payload.@event);
                    break;
                default:
                    Debug.LogWarning($"Unhandled notification type: {message.metadata.subscription_type}");
                    break;
            }
        }

        private void HandleChatMessage(Event chatEvent)
        {
            Debug.Log($"Chat message from {chatEvent.chatter_user_name}: {chatEvent.message.text}");

            // Process badges
            if (chatEvent.badges != null && chatEvent.badges.Count > 0)
            {
                foreach (var badge in chatEvent.badges)
                {
                    Debug.Log($"Badge: {badge.BadgeSetId} - {badge.BadgeId} - Info: {badge.BadgeInfo}");
                }
            }

            // Rest of your chat message handling code...
        }

        private void HandleRevocation(EventSubMessage message)
        {
            Debug.LogWarning($"Subscription revoked: {message.payload.subscription.type}");
            // Resubscribe if needed
            //StartCoroutine(SubscribeToEvent(message.payload.subscription.type, message.payload.subscription.version));
        }

        private IEnumerator SubscribeToAllEvents()
        {
            yield return SubscribeToEvent("channel.follow", "2", new Dictionary<string, string> {
            {"broadcaster_user_id", userId},
            {"moderator_user_id", userId}
            });
            yield return SubscribeToEvent("channel.subscribe", "1", new Dictionary<string, string> {
            {"broadcaster_user_id", userId}
            });
            yield return SubscribeToEvent("channel.subscription.gift", "1", new Dictionary<string, string> {
            {"broadcaster_user_id", userId}
            });
            yield return SubscribeToEvent("channel.cheer", "1", new Dictionary<string, string> {
            {"broadcaster_user_id", userId}
            });
            yield return SubscribeToEvent("channel.raid", "1", new Dictionary<string, string> {
            {"to_broadcaster_user_id", userId}
            });

            // Only subscribe to chat messages if not using the mock server
            if (!useMockServer)
            {
                yield return SubscribeToEvent("channel.chat.message", "1", new Dictionary<string, string> {
                {"broadcaster_user_id", userId},
                {"user_id", userId}
            });
            }
        }

        private IEnumerator SubscribeToEvent(string eventType, string version, Dictionary<string, string> conditions)
        {
            var subscriptionData = new Dictionary<string, object>
            {
                ["type"] = eventType,
                ["version"] = version,
                ["condition"] = conditions,
                ["transport"] = new Dictionary<string, string>
                {
                    ["method"] = "websocket",
                    ["session_id"] = sessionId
                }
            };

            string jsonBody = JsonConvert.SerializeObject(subscriptionData);

            using (UnityWebRequest request = new UnityWebRequest(currentApiUrl, "POST"))
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
                    Debug.LogError($"Error subscribing to event {eventType}: {request.error}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                }
                else
                {
                    Debug.Log($"Successfully subscribed to event {eventType}");
                    Debug.Log($"Response: {request.downloadHandler.text}");
                }
            }
        }

        private void Update()
        {
            if (webSocket != null)
            {
#if !UNITY_WEBGL || UNITY_EDITOR
                webSocket.DispatchMessageQueue();
#endif
            }
        }

        private void OnDestroy()
        {
            if (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                webSocket.Close();
            }
        }
    }

    public interface IEventHandler
    {
        void OnChatMessage(Event chatEvent);
        void OnFollow(Event followEvent);
        void OnSubscription(Event subscriptionEvent);
        void OnSubscriptionGift(Event giftSubscriptionEvent);
        void OnCheer(Event cheerEvent);
        void OnRaid(Event raidEvent);
    }
}
