#region

using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;

#endregion

namespace ModerationSystem;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
internal class JailCommand : ICommand
{
    public string Command { get; } = "jail";

    public string[] Aliases { get; } = Array.Empty<string>();

    public string Description { get; } = "Usage: Jail <player>";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission("ws.jail"))
        {
            response = "You do not have permission to use this command";
            return false;
        }
        Player? player;
        switch (arguments.Count)
        {
            case 0:
                player = Player.Get(sender);
                break;
            case 1:
                player = Player.Get(arguments.At(0));
                break;
            default:
                response = "Usage: Jail <player>";
                return true;
        }

        if (player == null)
        {
            response = "Player not found";
            return true;
        }

        Timing.RunCoroutine(Jail.JailPlayer(player));
        response = $"{player.Nickname} was Jailed";
        return true;
    }
}