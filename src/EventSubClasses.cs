using System.Collections.Generic;

namespace ProjectViola.Unity.TwitchAPI.EventSub
{
    [System.Serializable]
    public class EventSubMessage
    {
        public Metadata metadata { get; set; }
        public PayloadWrapper payload { get; set; }
    }

    [System.Serializable]
    public class Metadata
    {
        public string message_id { get; set; }
        public string message_type { get; set; }
        public string message_timestamp { get; set; }
        public string subscription_type { get; set; }
        public string subscription_version { get; set; }
    }

    [System.Serializable]
    public class PayloadWrapper
    {
        public Subscription subscription { get; set; }
        public Session session { get; set; }
        public Event @event { get; set; }
    }

    [System.Serializable]
    public class Payload
    {
        public Subscription subscription { get; set; }
        public Session session { get; set; }
        public Event @event { get; set; }
    }

    [System.Serializable]
    public class Subscription
    {
        public string id { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public string version { get; set; }
        public int cost { get; set; }
        public Dictionary<string, string> condition { get; set; }
        public Transport transport { get; set; }
        public string created_at { get; set; }
    }

    [System.Serializable]
    public class Session
    {
        public string id { get; set; }
        public string status { get; set; }
        public string connected_at { get; set; }
        public int keepalive_timeout_seconds { get; set; }
        public string reconnect_url { get; set; }
        public string recovery_url { get; set; }
    }

    [System.Serializable]
    public class Transport
    {
        public string method { get; set; }
        public string session_id { get; set; }
    }

    [System.Serializable]
    public class Event
    {
        // Common fields for all events
        public string broadcaster_user_id { get; set; }
        public string broadcaster_user_login { get; set; }
        public string broadcaster_user_name { get; set; }
        public string user_id { get; set; }
        public string user_login { get; set; }
        public string user_name { get; set; }

        // Chat message specific fields
        public string chatter_user_id { get; set; }
        public string chatter_user_login { get; set; }
        public string chatter_user_name { get; set; }
        public string message_id { get; set; }
        public ChatMessage message { get; set; }
        public string color { get; set; }
        public List<Badge> badges { get; set; }
        public string message_type { get; set; }

        // Bits event specific fields
        public int bits { get; set; }

        // Subscription event specific fields
        public string tier { get; set; }
        public bool is_gift { get; set; }

        // Raid event specific fields
        public string from_broadcaster_user_id { get; set; }
        public string from_broadcaster_user_login { get; set; }
        public string from_broadcaster_user_name { get; set; }
        public string to_broadcaster_user_id { get; set; }
        public string to_broadcaster_user_login { get; set; }
        public string to_broadcaster_user_name { get; set; }
        public int viewers { get; set; }

        // Gift subscription specific fields
        public string gifter_user_id { get; set; }
        public string gifter_user_login { get; set; }
        public string gifter_user_name { get; set; }
        public int total { get; set; }
        public int cumulative_total { get; set; }
        public bool is_anonymous { get; set; }
    }

    [System.Serializable]
    public class ChatMessage
    {
        public string text { get; set; }
        public List<Fragment> fragments { get; set; }
    }

    [System.Serializable]
    public class Fragment
    {
        public string type { get; set; }
        public string text { get; set; }
        public Cheermote cheermote { get; set; }
        public Emote emote { get; set; }
        public object mention { get; set; }
    }

    [System.Serializable]
    public class Emote
    {
        public string id { get; set; }
        public string emote_set_id { get; set; }
        public string owner_id { get; set; }
    }

    [System.Serializable]
    public class Cheermote
    {
        public string prefix { get; set; }
        public int bits { get; set; }
        public int tier { get; set; }
    }

    [System.Serializable]
    public class Badge
    {
        public string BadgeSetId { get; set; }

        public string BadgeId { get; set; }

        public string BadgeInfo { get; set; }
    }

    [System.Serializable]
    public class SubscriptionRequest
    {
        public string type { get; set; }
        public string version { get; set; }
        public Dictionary<string, string> condition { get; set; }
        public Transport transport { get; set; }
    }
}

    