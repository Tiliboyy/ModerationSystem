#region

using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using GameStore;
using NorthwoodLib.Pools;

#endregion

namespace ModerationSystem
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class WarnCommand : ICommand
    {
        public string Command { get; } = "warn";
        public string[] Aliases { get; } = new string[] { "WarnPlayer" };
        public string Description { get; } = "Usage: warn <steam64ID@steam> <Punkte> <Grund>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ws.addwarn"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count < 3)
            {
                response = "Usage: warn <steam64ID@steam> <Punkte> <Grund>";
                return true;
            }

            var player = Player.Get(sender);
            if (player == null)
            {
                response = "Something went wrong try again";
                return true;
            }

            var e = float.TryParse(arguments.At(1), out float number);
            var oplayer = Player.Get(arguments.At(0));
            oplayer?.Broadcast(Plugin.Singleton!.Config.Broadcasttexttime, Plugin.Singleton.Config.Broadcasttext);


            if (arguments.At(0).Contains("@"))
            {
                response = WarnDatabase.Database.AddWarn(arguments.At(0), player.Nickname, number,
                    FormatArguments(arguments, 2), null);
                return true;
            }

            if (int.TryParse(arguments.At(0), out var id))
            {
                var playera = Player.Get(id);
                if (playera == null)
                {
                    response = "Spieler wurde nicht gefunden";
                    return true;
                }

                response = WarnDatabase.Database.AddWarn(playera.UserId, player.Nickname, number,
                    FormatArguments(arguments, 2), null);
                return true;
            }


            response = "Spieler wurde nicht gefunden";

            return true;
        }


        public static string FormatArguments(ArraySegment<string> sentence, int index)
        {
            var sb = StringBuilderPool.Shared.Rent();
            foreach (string word in sentence.Segment(index))
            {
                sb.Append(word);
                sb.Append(" ");
            }

            string msg = sb.ToString();
            StringBuilderPool.Shared.Return(sb);
            return msg;
        }
    }
}