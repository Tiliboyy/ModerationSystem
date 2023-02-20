#region

using GameStore;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using PluginAPI.Core;
using RemoteAdmin;
using RemoteAdmin.Communication;
using Utils;
using VoiceChat;

#endregion

namespace ModerationSystem;

public static class RaPlayerPatch
{
    [HarmonyPatch(typeof(RaPlayer), nameof(RaPlayer.ReceiveData), typeof(CommandSender), typeof(string))]
    [HarmonyPriority(Priority.HigherThanNormal)]
    public static bool Prefix(RaPlayer __instance, CommandSender sender, string data)
    {
        string[] source = data.Split(' ');
        if (source.Length != 2 || !int.TryParse(source[0], out var result))
            return false;
        bool flag1 = result == 1;
        var playerCommandSender1 = sender as PlayerCommandSender;
        if (!flag1 && playerCommandSender1 != null && !playerCommandSender1.ServerRoles.Staff &&
            !CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayerSensitiveDataAccess))
            return false;
        List<ReferenceHub> referenceHubList = RAUtils.ProcessPlayerIdOrNamesList(
            new ArraySegment<string>(((IEnumerable<string>)source).Skip<string>(1).ToArray<string>()), 0,
            out string[] _);
        if (referenceHubList.Count == 0)
            return false;
        bool flag2 = PermissionsHandler.IsPermitted(sender.Permissions, 18007046UL) || playerCommandSender1 != null &&
            (playerCommandSender1.ServerRoles.Staff || playerCommandSender1.ServerRoles.RaEverywhere);
        if (referenceHubList.Count > 1)
        {
            var stringBuilder = StringBuilderPool.Shared.Rent("<color=white>");
            stringBuilder.Append("Selecting multiple players:");
            stringBuilder.Append("\nPlayer ID: <color=green><link=CP_ID>\uF0C5</link></color>");
            stringBuilder.Append("\nIP Address: " +
                                 (!flag1 ? "<color=green><link=CP_IP>\uF0C5</link></color>" : "[REDACTED]"));
            stringBuilder.Append("\nUser ID: " +
                                 (flag2 ? "<color=green><link=CP_USERID>\uF0C5</link></color>" : "[REDACTED]"));
            stringBuilder.Append("</color>");
            string data1 = string.Empty;
            string data2 = string.Empty;
            string data3 = string.Empty;
            foreach (ReferenceHub referenceHub in referenceHubList)
            {
                data1 = data1 + (object)referenceHub.PlayerId + ".";
                if (!flag1)
                    data2 = data2 + (referenceHub.networkIdentity.connectionToClient.IpOverride != null
                        ? referenceHub.networkIdentity.connectionToClient.OriginalIpAddress
                        : referenceHub.networkIdentity.connectionToClient.address) + ",";
                if (flag2)
                    data3 = data3 + referenceHub.characterClassManager.UserId + ".";
            }

            if (data1.Length > 0)
                RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId, data1);
            if (data2.Length > 0)
                RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, data2);
            if (data3.Length > 0)
                RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, data3);
            sender.RaReply($"${(object)__instance.DataId} {(object)stringBuilder}", true, true, string.Empty);
            StringBuilderPool.Shared.Return(stringBuilder);
        }
        else
        {
            var referenceHub = referenceHubList[0];
            ServerLogs.AddLog(ServerLogs.Modules.DataAccess,
                string.Format("{0} accessed IP address of player {1} ({2}).", (object)sender.LogName,
                    (object)referenceHub.PlayerId, (object)referenceHub.nicknameSync.MyNick),
                ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
            bool flag3 = PermissionsHandler.IsPermitted(sender.Permissions, PlayerPermissions.GameplayData);
            var characterClassManager = referenceHub.characterClassManager;
            var nicknameSync = referenceHub.nicknameSync;
            var connectionToClient = referenceHub.networkIdentity.connectionToClient;
            var serverRoles = referenceHub.serverRoles;
            if (sender is PlayerCommandSender playerCommandSender2)
                playerCommandSender2.ReferenceHub.queryProcessor.GameplayData = flag3;
            var stringBuilder = StringBuilderPool.Shared.Rent("<color=white>");
            stringBuilder.Append("Nickname: " + nicknameSync.CombinedName);
            stringBuilder.Append(string.Format("\nPlayer ID: {0} <color=green><link=CP_ID>\uF0C5</link></color>",
                (object)referenceHub.PlayerId));
            RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId,
                string.Format("{0}", (object)referenceHub.PlayerId));
            if (connectionToClient == null)
                stringBuilder.Append("\nIP Address: null");
            else if (!flag1)
            {
                stringBuilder.Append("\nIP Address: " + connectionToClient.address + " ");
                if (connectionToClient.IpOverride != null)
                {
                    RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip,
                        connectionToClient.OriginalIpAddress ?? "");
                    stringBuilder.Append(" [routed via " + connectionToClient.OriginalIpAddress + "]");
                }
                else
                    RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, connectionToClient.address ?? "");

                stringBuilder.Append(" <color=green><link=CP_IP>\uF0C5</link></color>");
            }
            else
                stringBuilder.Append("\nIP Address: [REDACTED]");

            stringBuilder.Append("\nUser ID: " +
                                 (flag2
                                     ? (string.IsNullOrEmpty(characterClassManager.UserId)
                                         ? "(none)"
                                         : characterClassManager.UserId +
                                           " <color=green><link=CP_USERID>\uF0C5</link></color>")
                                     : "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>"));
            if (flag2)
            {
                RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, characterClassManager.UserId ?? "");
                if (characterClassManager.SaltedUserId != null && characterClassManager.SaltedUserId.Contains("$"))
                    stringBuilder.Append("\nSalted User ID: " + characterClassManager.SaltedUserId);
                if (!string.IsNullOrEmpty(characterClassManager.UserId2))
                    stringBuilder.Append("\nUser ID 2: " + characterClassManager.UserId2);
            }

            stringBuilder.Append("\nServer role: " + serverRoles.GetColoredRoleString());
            bool flag4 = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenBadges);
            bool flag5 = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenGlobalBadges);
            if (playerCommandSender1 != null && playerCommandSender1.ServerRoles.Staff)
            {
                flag4 = true;
                flag5 = true;
            }

            bool flag6 = !string.IsNullOrEmpty(serverRoles.HiddenBadge);
            bool flag7 = !flag6 || serverRoles.GlobalHidden & flag5 || !serverRoles.GlobalHidden & flag4;
            if (flag7)
            {
                if (flag6)
                {
                    stringBuilder.Append("\n<color=#DC143C>Hidden role: </color>" + serverRoles.HiddenBadge);
                    stringBuilder.Append("\n<color=#DC143C>Hidden role type: </color>" +
                                         (serverRoles.GlobalHidden ? "GLOBAL" : "LOCAL"));
                }

                if (serverRoles.RaEverywhere)
                    stringBuilder.Append(
                        "\nStudio Status: <color=#BCC6CC>Studio GLOBAL Staff (management or global moderation)</color>");
                else if (serverRoles.Staff)
                    stringBuilder.Append("\nStudio Status: <color=#94B9CF>Studio Staff</color>");
            }

            int flags = (int)VoiceChatMutes.GetFlags(referenceHubList[0]);
            if (flags != 0)
            {
                stringBuilder.Append("\nMUTE STATUS:");
                foreach (int num in Enum.GetValues(typeof(VcMuteFlags)))
                {
                    if (num == 0 || (flags & num) != num) continue;
                    stringBuilder.Append(" <color=#F70D1A>");
                    stringBuilder.Append((object)(VcMuteFlags)num);
                    stringBuilder.Append("</color>");
                }
            }

            stringBuilder.Append("\nActive flag(s):");
            if (characterClassManager.GodMode)
                stringBuilder.Append(" <color=#659EC7>[GOD MODE]</color>");
            if (referenceHub.playerStats.GetModule<AdminFlagsStat>().HasFlag(AdminFlags.Noclip))
                stringBuilder.Append(" <color=#DC143C>[NOCLIP ENABLED]</color>");
            else if (FpcNoclip.IsPermitted(referenceHub))
                stringBuilder.Append(" <color=#E52B50>[NOCLIP UNLOCKED]</color>");
            if (serverRoles.DoNotTrack)
                stringBuilder.Append(" <color=#BFFF00>[DO NOT TRACK]</color>");
            if (serverRoles.BypassMode)
                stringBuilder.Append(" <color=#BFFF00>[BYPASS MODE]</color>");
            if (flag7 && serverRoles.RemoteAdmin)
                stringBuilder.Append(" <color=#43C6DB>[RA AUTHENTICATED]</color>");
            if (serverRoles.IsInOverwatch)
                stringBuilder.Append(" <color=#008080>[OVERWATCH MODE]</color>");
            else if (flag3)
            {
                stringBuilder.Append("\nClass: ")
                    .Append(PlayerRoleLoader.AllRoles.TryGetValue(referenceHub.GetRoleId(), out var playerRoleBase)
                        ? playerRoleBase.RoleName
                        : "None");
                stringBuilder.Append(" <color=#fcff99>[HP: ")
                    .Append(CommandProcessor.GetRoundedStat<HealthStat>(referenceHub)).Append("]</color>");
                stringBuilder.Append(" <color=green>[AHP: ")
                    .Append(CommandProcessor.GetRoundedStat<AhpStat>(referenceHub)).Append("]</color>");
                stringBuilder.Append(" <color=#977dff>[HS: ")
                    .Append(CommandProcessor.GetRoundedStat<HumeShieldStat>(referenceHub)).Append("]</color>");
                stringBuilder.Append("\nPosition: ").Append(referenceHub.transform.position.ToPreciseString());
            }
            else
                stringBuilder.Append(
                    "\n<color=#D4AF37>Some fields were hidden. GameplayData permission required.</color>");

            stringBuilder.Append("</color>");
            var player = Player.Get(referenceHub);
            if (player != null)
            {
                //Funny Zone
                switch (player.Nickname.ToLower())
                {
                    case "tiliboyy":
                        stringBuilder.Append($"\nSex Update Progress: <b><color=#f102f9>71%</color></b>");
                        break;
                    case "trix":
                        stringBuilder.Append($"\nBitches: <color=#fca505>0</color>");
                        break;
                    case "indie van gaming":
                        stringBuilder.Append($"\nFreunde:<color=#fca505> -5</color>");
                        break;
                    case "eisnfaust328":
                        stringBuilder.Append($"\nStatus: <color=#fc05e3>Furry</color>");
                        break;
                    case "peter":
                        stringBuilder.Append($"\nStatus: <color=#fc05e3>Tinybrain</color>");
                        break;
                    case "schwert300":
                        stringBuilder.Append($"<color=#ed1515>RDMer</color>");
                        break;
                    case "speedy0607":
                        stringBuilder.Append($"<b><color=#ed1515>Retard</color></b>");
                        break;
                }
                //End of Funny zone  

                var total = WarnDatabase.Database.GetTotal(player.UserId);
                stringBuilder.Append($"\nPunkte: <color={GetColor(total)}>{total}</color>");
            }

            sender.RaReply(
                $"${(object)__instance.DataId} {(object)StringBuilderPool.Shared.ToStringReturn(stringBuilder)}", true,
                true, string.Empty);
            RaPlayerQR.Send(sender, false,
                string.IsNullOrEmpty(characterClassManager.UserId) ? "(no User ID)" : characterClassManager.UserId);
        }

        return false;
    }

    private static string GetColor(float points)
    {
        return points switch
        {
            >= 4 => "red",
            0 => "green",
            > 0 and < 4 => "yellow",
            _ => "green"
        };
    }
}