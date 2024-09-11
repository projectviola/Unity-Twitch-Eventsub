using UnityEngine;
using ProjectViola.Unity.TwitchAPI.EventSub;

public class EventSubHandler : MonoBehaviour, IEventHandler
{
    public void OnSubscriptionGift(ProjectViola.Unity.TwitchAPI.EventSub.Event giftSubscriptionEvent)
    {
        string gifterName = giftSubscriptionEvent.is_anonymous ? "An anonymous gifter" : giftSubscriptionEvent.gifter_user_name;
        Debug.Log($"{gifterName} gifted {giftSubscriptionEvent.total} subscription(s) to the channel! " +
                  $"They have gifted a total of {giftSubscriptionEvent.cumulative_total} subscriptions.");
    }
    public void OnChatMessage(ProjectViola.Unity.TwitchAPI.EventSub.Event chatEvent)
    {
        Debug.Log($"Chat message from {chatEvent.chatter_user_name}: {chatEvent.message.text}");
    }

    public void OnFollow(ProjectViola.Unity.TwitchAPI.EventSub.Event followEvent)
    {
        Debug.Log($"{followEvent.user_name} followed the channel!");
    }

    public void OnSubscription(ProjectViola.Unity.TwitchAPI.EventSub.Event subscriptionEvent)
    {
        if (!subscriptionEvent.is_gift) // Don't want gifts to show up
        {
            Debug.Log($"{subscriptionEvent.user_name} subscribed to the channel!");
        }
    }

    public void OnCheer(ProjectViola.Unity.TwitchAPI.EventSub.Event cheerEvent)
    {
        Debug.Log($"{cheerEvent.user_name} cheered {cheerEvent.bits} bits!");
    }

    public void OnRaid(ProjectViola.Unity.TwitchAPI.EventSub.Event raidEvent)
    {
        Debug.Log($"{raidEvent.from_broadcaster_user_name} raided with {raidEvent.viewers} viewers!");
    }
}