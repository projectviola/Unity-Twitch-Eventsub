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

    private TwitchOAuth twitchOAuth;
    private EventSubClient eventSubClient;
    private EventSubHandler eventHandler;
    private LocalCallbackServer callbackServer;

    private void Start()
    {
        StartCoroutine(InitializeTwitch());
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
}


