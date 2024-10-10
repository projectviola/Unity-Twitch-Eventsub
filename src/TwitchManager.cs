using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using ProjectViola.Unity.TwitchAPI.EventSub;

    public class TwitchManager : MonoBehaviour
{
    public string clientId;
    public bool useMockServer = false;
    public bool forceReauthentication = false;
    public bool dontRunInEditor = false;

    private TwitchOAuth twitchOAuth;
    private EventSubClient eventSubClient;
    private EventSubHandler eventHandler;
    private LocalCallbackServer callbackServer;
    private TwitchChatSender chatSender;

    private void Start()
    {
#if UNITY_EDITOR
        if (!dontRunInEditor) 
        {
            StartCoroutine(InitializeTwitch());
        }
#else
        StartCoroutine(InitializeTwitch());
#endif
    }

    private IEnumerator InitializeTwitch()
    {
        if (forceReauthentication)
        {
            TwitchCredentials.ClearCredentials();
            StartAuthorizationFlow();
            forceReauthentication = false;  // Reset the flag
            yield break;
        }

        if (TwitchCredentials.AreCredentialsStored() && !useMockServer)
        {
            if (TwitchCredentials.IsTokenExpired())
            {
                Debug.Log("Stored token is expired. Starting new authorization flow.");
                StartAuthorizationFlow();
            }
            else
            {
                yield return StartCoroutine(TwitchCredentials.VerifyToken((isValid) =>
                {
                    if (isValid)
                    {
                        ConnectToEventSub();
                        InitializeChatSender();
                    }
                    else
                    {
                        Debug.Log("Stored token is invalid. Starting new authorization flow.");
                        StartAuthorizationFlow();
                    }
                }));
            }
        }
        else
        {
            StartAuthorizationFlow();
        }
    }

    private void StartAuthorizationFlow()
    {
        twitchOAuth = gameObject.AddComponent<TwitchOAuth>();
        twitchOAuth.Initialize(clientId);

        callbackServer = gameObject.AddComponent<LocalCallbackServer>();
        callbackServer.StartServer();

        twitchOAuth.StartAuthorizationFlow();
    }

    public void HandleAuthorizationCallback(string accessToken)
    {
        twitchOAuth.SetAccessToken(accessToken);
        StartCoroutine(ValidateAndConnect());
    }

    public void ForceReauthentication()
    {
        forceReauthentication = true;
        StartCoroutine(InitializeTwitch());
    }

    private IEnumerator ValidateAndConnect()
    {
        yield return StartCoroutine(twitchOAuth.ValidateToken());

        if (!string.IsNullOrEmpty(twitchOAuth.AccessToken))
        {
            yield return StartCoroutine(twitchOAuth.GetUserInfo());

            TwitchCredentials.SaveCredentials(
                twitchOAuth.AccessToken,
                twitchOAuth.UserId,
                twitchOAuth.UserName,
                clientId,
                DateTime.UtcNow.AddDays(60) // Assuming token expires in 60 days, adjust as needed
            );

            ConnectToEventSub();
            InitializeChatSender();
        }
        else
        {
            Debug.LogError("Failed to validate access token");
            StartAuthorizationFlow();
        }
    }

    private void ConnectToEventSub()
    {
        eventSubClient = gameObject.AddComponent<EventSubClient>();
        eventHandler = gameObject.AddComponent<EventSubHandler>();

        eventSubClient.Initialize(
            TwitchCredentials.GetClientId(),
            TwitchCredentials.GetAccessToken(),
            TwitchCredentials.GetUserId(),
            eventHandler,
            useMockServer
        );
        eventSubClient.Connect();
    }

    private void InitializeChatSender()
    {
        chatSender = gameObject.AddComponent<TwitchChatSender>();
        chatSender.Initialize(
            TwitchCredentials.GetClientId(),
            TwitchCredentials.GetAccessToken(),
            TwitchCredentials.GetUserId(),
            TwitchCredentials.GetUserId()  // Using the same ID for broadcaster and sender
        );
    }

    // Public methods to send chat messages and announcements
    public void SendChatMessage(string message, string replyParentMessageId = null)
    {
        if (chatSender != null)
        {
            chatSender.SendChatMessage(message, replyParentMessageId);
        }
        else
        {
            Debug.LogError("TwitchChatSender is not initialized");
        }
    }

    public void SendAnnouncement(string message, string color = "primary")
    {
        if (chatSender != null)
        {
            chatSender.SendAnnouncement(message, color);
        }
        else
        {
            Debug.LogError("TwitchChatSender is not initialized");
        }
    }
}