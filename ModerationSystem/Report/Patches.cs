#region

using Exiled.API.Features;
using HarmonyLib;
using Utf8Json;

#endregion

namespace ModerationSystem;

public class Patches
{
    [HarmonyPatch(typeof(CheaterReport))]
    [HarmonyPatch(nameof(CheaterReport.SubmitReport))]
    internal static class LocalReportPatch
    {
        public static bool Prefix(
            ref bool __state,
            string reporterUserId,
            string reportedUserId,
            string reason,
            ref int reportedId,
            string reporterNickname,
            string reportedNickname,
            bool friendlyFire
        )
        {
            try
            {
                int id = 0;
                string pings = Plugin.Singleton.Config.RoleId.Aggregate("", (current, s) => current + $"<@&{s}> ");

                var reported = Player.Get(reportedUserId);
                var reporter = Player.Get(reporterUserId);
                if (reported != null && reporter != null)
                {
                    id = ReportDatabase.AddReport(reported, reporter, reason);
                    pings = Uri.EscapeDataString(pings);
                }

                string payload = JsonSerializer.ToJsonString(new DiscordWebhook(
                    pings, CheaterReport.WebhookUsername,
                    CheaterReport.WebhookAvatar, false, new DiscordEmbed[1]
                    {
                        new(CheaterReport.ReportHeader, "rich", CheaterReport.ReportContent,
                            CheaterReport.WebhookColor, new DiscordEmbedField[]
                            {
                                new("Server Name", CheaterReport.ServerName, false),
                                new("Server Endpoint",
                                    $"{(object)ServerConsole.Ip}:{(object)ServerConsole.PortToReport}", false),
                                new("ReportID", CheaterReport.AsDiscordCode(id.ToString()), false),
                                new("Reporter UserID", CheaterReport.AsDiscordCode(reporterUserId), false),
                                new("Reporter Nickname", CheaterReport.DiscordSanitize(reporterNickname), false),
                                new("Reported UserID", CheaterReport.AsDiscordCode(reportedUserId), false),
                                new("Reported Nickname", CheaterReport.DiscordSanitize(reportedNickname), false),
                                new("Reason", CheaterReport.DiscordSanitize(reason), false),
                                new("Timestamp", TimeBehaviour.Rfc3339Time(), false),
                                new("UTC Timestamp", TimeBehaviour.Rfc3339Time(DateTimeOffset.UtcNow), false)
                            })
                    }));
                HttpQuery.Post(friendlyFire ? FriendlyFireConfig.WebhookUrl : CheaterReport.WebhookUrl,
                    "payload_json=" + payload);
                __state = true;
            }
            catch (Exception ex)
            {
                ServerConsole.AddLog("Failed to send report by webhook: " + ex.Message);
                __state = false;
            }

            return false;
        }

        public static void Postfix(ref bool __result, bool __state)
        {
            __result = __state;
        }
    }
}