#region

using CommandSystem;
using Exiled.Permissions;
using Exiled.Permissions.Extensions;
using GameStore;

#endregion

[CommandHandler(typeof(RemoteAdminCommandHandler))]
internal class Transfer : ICommand
{
    public string Command { get; } = "Transfer";

    public string[] Aliases { get; } = Array.Empty<string>();

    public string Description { get; } = "Transfers old Warns";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission("ws.transfer"))
        {
            response = "You do not have permission to use this command";
            return false;
        }
        var i = WarnDatabase.Database.Transfer();
        response = $"Transfered " + i + " warns!";
        return true;
    }
}