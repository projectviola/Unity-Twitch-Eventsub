# Twitch EventSub Unity Integration

This project provides a simple integration of Twitch's EventSub API for Unity projects. It allows developers to easily incorporate Twitch events into their Unity games or applications.

## Dependencies

- System.Net is required. (No WebGL support)
- NativeWebSocket is required. (There is no way around this; You'll have to get it using Unity Package Manager)
	- https://github.com/endel/NativeWebSocket.git

## Features

- Easy setup with minimal configuration
- Supports Twitch events: Raids, Chat Messages, Subscriptions, Gifted Subs, and Follows
- Handles OAuth authentication flow
- Persistent credential storage
- Support for connecting to Twitch CLI mock server for testing

## Setup

1. Add the `TwitchManager` script to a GameObject in your Unity scene.
2. Set your Twitch application's Client ID in the `TwitchManager` component in the Inspector.

## Usage

The `EventSubHandler` class handles incoming Twitch events. Modify this class to implement your desired functionality for each event type.

Example:

```csharp
public class EventSubHandler : MonoBehaviour, IEventHandler
{
    public void OnChatMessage(Event chatEvent)
    {
	// Implement other functionality here
        Debug.Log($"Chat message from {chatEvent.chatter_user_name}: {chatEvent.message.text}");
    }

}
```

## Mock Server Testing

To test your integration without connecting to the live Twitch API, you can use the Twitch CLI mock server:

1. Set up and run the Twitch CLI mock server.
2. In the Unity Inspector, find the `TwitchManager` component and enable the "Use Mock Server" toggle.

This will direct all API calls to the local mock server instead of the live Twitch API, allowing for easier testing and development.

## Customization

Feel free to modify and extend the functionality to suit your project's needs. You can add support for additional Twitch events by updating the `EventSubClient` and `EventSubHandler` classes.

## Limitations

- Currently supports a limited set of Twitch events (Raids, Chat Messages, Subscriptions, Gifted Subs, and Follows)
- Requires a Twitch Developer application for authentication when not using the mock server
- System.net is required. There is no way to implement this package into a WebGL project.

## License

This project is open-source and free to use in your own projects without attribution.

## Disclaimer

This is not an official Twitch product. Use at your own risk and ensure compliance with Twitch's Developer Agreement and Terms of Service.
