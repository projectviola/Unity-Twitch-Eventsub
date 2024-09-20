using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using Newtonsoft.Json;


namespace ProjectViola.Unity.TwitchAPI.EventSub
{

    public class TwitchOAuth : MonoBehaviour
    {
        private const string AuthorizationEndpoint = "https://id.twitch.tv/oauth2/authorize";
        private const string RedirectUri = "http://localhost:8080/oauth/callback"; // Make sure this matches your Twitch app settings

        public string ClientId { get; private set; }
        public string AccessToken { get; private set; }

        public string UserId { get; private set; }
        public string UserLogin { get; private set; }
        public string UserName { get; private set; }  

        public void Initialize(string clientId)
        {
            this.ClientId = clientId;
        }

        public void StartAuthorizationFlow()
        {
            string scopes = "chat:read channel:read:subscriptions bits:read channel:read:redemptions moderator:read:followers user:read:email user:read:chat";
            string authorizationUrl = $"{AuthorizationEndpoint}?client_id={ClientId}&redirect_uri={RedirectUri}&response_type=token&scope={Uri.EscapeDataString(scopes)}";
            Application.OpenURL(authorizationUrl);
        }

        public void SetAccessToken(string accessToken)
        {
            AccessToken = accessToken;
        }

        public IEnumerator ValidateToken()
        {
            string validationUrl = "https://id.twitch.tv/oauth2/validate";
            using (UnityWebRequest www = UnityWebRequest.Get(validationUrl))
            {
                www.SetRequestHeader("Authorization", $"OAuth {AccessToken}");
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to validate token: {www.error}");
                    AccessToken = null;
                }
                else
                {
                    Debug.Log("Token validated successfully");
                    ValidateResponse response = JsonConvert.DeserializeObject<ValidateResponse>(www.downloadHandler.text);
                    UserId = response.user_id;
                    UserLogin = response.login;
                }
            }
        }

        public IEnumerator GetUserInfo()
        {
            string userInfoUrl = "https://api.twitch.tv/helix/users";
            using (UnityWebRequest www = UnityWebRequest.Get(userInfoUrl))
            {
                www.SetRequestHeader("Authorization", $"Bearer {AccessToken}");
                www.SetRequestHeader("Client-Id", ClientId);

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to get user info: {www.error}");
                }
                else
                {
                    string responseText = www.downloadHandler.text;
                    UserInfoResponse response = JsonConvert.DeserializeObject<UserInfoResponse>(responseText);
                    if (response.data != null && response.data.Length > 0)
                    {
                        UserData userData = response.data[0];
                        UserId = userData.id;
                        UserLogin = userData.login;
                        UserName = userData.display_name;  // Corrected this line
                        Debug.Log($"Got user info: ID={UserId}, Login={UserLogin}, Name={UserName}");
                    }
                    else
                    {
                        Debug.LogError("User info response contained no data");
                    }
                }
            }
        }

        [System.Serializable]
        private class UserInfoResponse
        {
            public UserData[] data;
        }

        [System.Serializable]
        private class UserData
        {
            public string id;
            public string login;
            public string display_name;
        }

        [System.Serializable]
        private class ValidateResponse
        {
            public string client_id;
            public string login;
            public string[] scopes;
            public string user_id;
            public string display_name;
            public int expires_in;
        }
    }
}