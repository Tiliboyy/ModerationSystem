#region

using System.Text;
using Exiled.API.Features;
using YamlDotNet.Serialization;

#endregion

namespace ModerationSystem;
/*
public class ReportDatabase
{
    public static List<Report> OpenReports = new();


    public static Report GetReport(int id)
    {
        Read();
        foreach (var report in OpenReports.Where(report => report.Id == id))
        {
            return report;
        }

        return new Report { Reporter = "DEFAULT", Reperted = "DEFAULT", Id = -1 };
    }

    public static int GetMewID()
    {
        Read();
        int highestid = 0;
        foreach (var report in OpenReports.Where(report => highestid <= report.Id))
        {
            highestid = report.Id + 1;
        }

        return highestid;
    }

    public static bool DeleteReport(int id)
    {
        Read();
        bool FoundReport = false;
        foreach (var report in OpenReports.Where(report => report.Id == id))
        {
            OpenReports.Remove(report);
            FoundReport = true;
        }

        if (!FoundReport)
            return false;
        Update();
        return true;
    }

    public static void Update()
    {
        string yaml = new Serializer().Serialize(OpenReports);
        File.WriteAllText(Path.Combine(Paths.Configs, "reports.yaml"), yaml);
        string yamldata = File.ReadAllText(Path.Combine(Paths.Configs, "reports.yaml"));
        var e = new Deserializer().Deserialize<List<Report>>(new StringReader(yamldata));
        OpenReports = e;
    }

    public static string GetReportList()
    {
        if (OpenReports.Count == 0)
            return "Es gibt momentan keine offenen Reports";
        var reports = new StringBuilder();
        reports.Append("\n");
        foreach (var report in OpenReports)
        {
            reports.Append(
                $"Report\n    Id: {report.Id}\n  Reporter: {report.Reporter}\n    Reported: {report.Reperted}\n  Grund: {report.Reason}\n");
        }

        return reports.ToString();
    }

    public static void Read()
    {
        if (!File.Exists(Path.Combine(Paths.Configs, "reports.yaml")))
        {
            Update();
        }

        string yamldata = File.ReadAllText(Path.Combine(Paths.Configs, "reports.yaml"));
        var data = new Deserializer().Deserialize<List<Report>>(new StringReader(yamldata));
        OpenReports = data;
        if (OpenReports.Count == 0)
        {
            OpenReports.Add(new Report { Reporter = "DEFAULT", Reperted = "DEFAULT", Id = 0, Reason = "DEFAULT" });
        }
    }

    public static int AddReport(Player reported, Player reporter, string reason)
    {
        Read();
        int highestid = GetMewID();
        OpenReports.Add(new Report
            { Reporter = reporter.UserId, Reperted = reported.UserId, Id = highestid, Reason = reason });
        Update();
        return highestid;
    }

    public struct Report
    {
        public int Id { get; set; }
        public string Reporter { get; set; }
        public string Reperted { get; set; }
        public string Reason { get; set; }
    }
}
*/