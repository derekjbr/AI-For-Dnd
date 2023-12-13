public class AlertFriend : Goal
{
    GoalOrientatedController Friend;
    public AlertFriend(GoalOrientatedController friend) : base()
    {
        Name = "AlertedFriend" + friend.GetInstanceID();
        goals.Add("friendAlerted" + friend.GetInstanceID(), 0);
        Friend = friend;
    }
    public override float Desireability()
    {
        if (Friend != null && Friend.HasCurrnetCondition("knowsAboutBattle"))
            return 0f;

        return 1f;
    }
}
