using UnityEngine;
using ProjectViola.Unity.TwitchAPI.EventSub;
using Event = ProjectViola.Unity.TwitchAPI.EventSub.Event;

public class EventSubHandler : MonoBehaviour, IEventHandler
{
    // Please use this class to handle events
    // They are pretty straightforward with what kind of data they have
    // Debug Logging examples should be enough to get you started
    // If you need more information, you can find all these event subs on the twitch api doc

    public void OnSubscriptionGift(Event giftSubscriptionEvent)
    {
        string gifterName = giftSubscriptionEvent.is_anonymous ? "An anonymous gifter" : giftSubscriptionEvent.gifter_user_name;
        Debug.Log($"{gifterName} gifted {giftSubscriptionEvent.total} subscription(s) to the channel! " +
                  $"They have gifted a total of {giftSubscriptionEvent.cumulative_total} subscriptions.");
    }
    public void OnChatMessage(Event chatEvent)
    {
        Debug.Log($"Chat message from {chatEvent.chatter_user_name}: {chatEvent.message.text}");
    }

    public void OnFollow(Event followEvent)
    {
        Debug.Log($"{followEvent.user_name} followed the channel!");
    }

    public void OnSubscription(Event subscriptionEvent)
    {
        if (!subscriptionEvent.is_gift) // Don't want gifts to show up
        {
            Debug.Log($"{subscriptionEvent.user_name} subscribed to the channel!");
        }
    }

    public void OnCheer(Event cheerEvent)
    {
        Debug.Log($"{cheerEvent.user_name} cheered {cheerEvent.bits} bits!");
    }

    public void OnRaid(Event raidEvent)
    {
        Debug.Log($"{raidEvent.from_broadcaster_user_name} raided with {raidEvent.viewers} viewers!");
    }
}