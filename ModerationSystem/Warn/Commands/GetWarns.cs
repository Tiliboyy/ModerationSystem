#region

using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using GameStore;

#endregion

namespace ModerationSystem
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Getwarns : ICommand
    {
        public string Command { get; } = "getwarns";
        public string[] Aliases { get; } = new string[] { "gwarns", "showwarns" };
        public string Description { get; } = "Usage: getwarns <ID/SteamID> <Optional: true/false>)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ws.getwarns"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            var onlynew = true;
            switch (arguments.Count)
            {
                case 0:
                    response = "Usage: getwarns <ID/SteamID>";
                    return true;
                case >= 2 when bool.TryParse(arguments.At(1), out var onlynewb):
                {
                    if (onlynewb)
                    {
                        onlynew = false;
                    }
                    else
                    {
                        onlynew = true;
                    }

                    break;
                }
                case >= 2:
                    onlynew = true;
                    break;
            }

            string e = "";
            if (arguments.At(0).Contains("@"))
            {
                e = WarnDatabase.Database.GetWarns(arguments.At(0), onlynew, true);
                e = e.Insert(0, "\nNutzer");
                response = e;
                return true;
            }

            if (int.TryParse(arguments.At(0), out var id))
            {
                var player = Player.Get(id);
                if (player == null)
                {
                    response = "Spieler wurde nicht gefunden";
                    return true;
                }

                e = WarnDatabase.Database.GetWarns(player.UserId, onlynew, true);
                e = e.Insert(0, "\nVerwarnungen von " + player.Nickname);
                response = e;

                return true;
            }


            response = "Spieler wurde nicht gefunden";

            return true;
        }
    }
}