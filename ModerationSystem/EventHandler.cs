#region

using Exiled.API.Features;
using GameStore;

#endregion

namespace ModerationSystem;

public class EventHandler : Plugin<Config>
{
    public static void OnWaitingForPlayers()
    {
        WarnDatabase.Database.CreatePlayers();
    }
}