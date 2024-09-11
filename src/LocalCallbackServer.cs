using System;
using System.Net;
using System.Threading;
using UnityEngine;

namespace ProjectViola.Unity.TwitchAPI.EventSub
{
    public class LocalCallbackServer : MonoBehaviour
    {
        private HttpListener listener;
        private Thread listenerThread;
        private string receivedAccessToken;
        private bool tokenReceived = false;

        public void StartServer()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/oauth/");
            listener.Start();

            listenerThread = new Thread(ListenForCallback);
            listenerThread.Start();
        }

        private void ListenForCallback()
        {
            while (listener.IsListening)
            {
                try
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;

                    if (request.Url.AbsolutePath == "/oauth/callback")
                    {
                        string responseHtml = @"
                        <html>
                        <body>
                            <h1>Authorization Successful</h1>
                            <p>You can close this window now.</p>
                            <script>
                                if (window.location.hash) {
                                    var accessToken = new URLSearchParams(window.location.hash.substr(1)).get('access_token');
                                    if (accessToken) {
                                        window.location.href = '/oauth/token?access_token=' + accessToken;
                                    }
                                }
                            </script>
                        </body>
                        </html>";

                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseHtml);
                        HttpListenerResponse response = context.Response;
                        response.ContentLength64 = buffer.Length;
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                        response.OutputStream.Close();
                    }
                    else if (request.Url.AbsolutePath == "/oauth/token")
                    {
                        string accessToken = request.QueryString["access_token"];
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            receivedAccessToken = accessToken;
                            tokenReceived = true;
                        }

                        string responseHtml = "<html><body><h1>Token Received</h1><p>You can close this window now.</p></body></html>";
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseHtml);
                        HttpListenerResponse response = context.Response;
                        response.ContentLength64 = buffer.Length;
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                        response.OutputStream.Close();

                        // Stop listening after receiving the token
                        listener.Stop();
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error in callback server: {e.Message}");
                }
            }
        }

        private void Update()
        {
            if (tokenReceived)
            {
                tokenReceived = false;
                FindObjectOfType<TwitchManager>().HandleAuthorizationCallback(receivedAccessToken);
            }
        }

        private void OnDestroy()
        {
            if (listener != null && listener.IsListening)
            {
                listener.Stop();
            }
            if (listenerThread != null && listenerThread.IsAlive)
            {
                listenerThread.Abort();
            }
        }
    }
}
