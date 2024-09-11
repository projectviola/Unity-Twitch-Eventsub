using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;


namespace ProjectViola.Unity.TwitchAPI.EventSub
{
    public static class TwitchCredentials
    {
        private const string ACCESS_TOKEN_KEY = "TwitchAccessToken";
        private const string USER_ID_KEY = "TwitchUserId";
        private const string USER_NAME_KEY = "TwitchUserName";
        private const string CLIENT_ID_KEY = "TwitchClientId";
        private const string TOKEN_EXPIRY_KEY = "TwitchTokenExpiry";


        public static void SaveCredentials(string accessToken, string userId, string userName, string clientId, DateTime expiryDate)
        {
            PlayerPrefs.SetString(ACCESS_TOKEN_KEY, accessToken);
            PlayerPrefs.SetString(USER_ID_KEY, userId);
            PlayerPrefs.SetString(USER_NAME_KEY, userName);
            PlayerPrefs.SetString(CLIENT_ID_KEY, clientId);
            PlayerPrefs.SetString(TOKEN_EXPIRY_KEY, expiryDate.ToString("o")); // ISO 8601 format
            PlayerPrefs.Save();
        }

        public static string GetAccessToken()
        {
            return PlayerPrefs.GetString(ACCESS_TOKEN_KEY, string.Empty);
        }

        public static string GetUserId()
        {
            return PlayerPrefs.GetString(USER_ID_KEY, string.Empty);
        }

        public static string GetUserName()
        {
            return PlayerPrefs.GetString(USER_NAME_KEY, string.Empty);
        }

        public static string GetClientId()
        {
            return PlayerPrefs.GetString(CLIENT_ID_KEY, string.Empty);
        }

        public static DateTime GetTokenExpiry()
        {
            string expiryString = PlayerPrefs.GetString(TOKEN_EXPIRY_KEY, string.Empty);
            return string.IsNullOrEmpty(expiryString) ? DateTime.MinValue : DateTime.Parse(expiryString);
        }

        public static bool AreCredentialsStored()
        {
            return !string.IsNullOrEmpty(GetAccessToken()) &&
                   !string.IsNullOrEmpty(GetUserId()) &&
                   !string.IsNullOrEmpty(GetClientId());
        }

        public static bool IsTokenExpired()
        {
            return GetTokenExpiry() <= DateTime.UtcNow;
        }

        public static void ClearCredentials()
        {
            PlayerPrefs.DeleteKey(ACCESS_TOKEN_KEY);
            PlayerPrefs.DeleteKey(USER_ID_KEY);
            PlayerPrefs.DeleteKey(USER_NAME_KEY);
            PlayerPrefs.DeleteKey(CLIENT_ID_KEY);
            PlayerPrefs.DeleteKey(TOKEN_EXPIRY_KEY);
            PlayerPrefs.Save();
        }

        public static IEnumerator VerifyToken(Action<bool> callback)
        {
            string accessToken = GetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                callback(false);
                yield break;
            }

            string url = "https://id.twitch.tv/oauth2/validate";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", $"OAuth {accessToken}");
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    // Token is valid
                    callback(true);
                }
                else
                {
                    // Token is invalid
                    Debug.LogError($"Token validation failed: {request.error}");
                    ClearCredentials();
                    callback(false);
                }
            }
        }
    }
}