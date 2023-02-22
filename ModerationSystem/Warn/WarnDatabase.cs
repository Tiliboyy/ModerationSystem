#region

using System.Drawing;
using Exiled.API.Features;
using LiteDB;
using ModerationSystem;
using YamlDotNet.Serialization;

#endregion

namespace GameStore;

public static class WarnDatabase
{
    public static LiteDatabase db = new(Path.Combine(Paths.Configs, "ModerationSystem/Warns.db"));

    public class DatabasePlayer
    {
        public string? _id { get; set; }


        public List<Warn>? warns { get; set; }
    }

    public static class Database
    {
        public static void CreatePlayers()
        {
            var players = db.GetCollection<DatabasePlayer>("players");

            if (!players.Exists(x => true)) players.EnsureIndex(x => x._id);
        }

        public static string RemoveWarn(string steam64id, int id)
        {
            var playerID = steam64id.Split('@')[0];
            var players = db.GetCollection<DatabasePlayer>("players");

            var dbplayer = players.FindOne(x => x._id == playerID);
            if (dbplayer.warns != null)
            {
                bool foundwarn = false;
                foreach (var warn in dbplayer.warns.Where(warn => warn.Id == id))
                {
                    dbplayer.warns.Remove(warn);
                    foundwarn = true;
                    players.Update(dbplayer);
                    break;
                }

                return foundwarn ? "Verwarnung wurde gelöscht" : "Verwarnung wurde nicht gefunden";
            }
            return "Spieler wurde nicht gefunden";


        }

        public static string AddWarn(string warned, string warner, float points, string reason, DateTime? time)
        {
            try
            {
                var playerID = warned.Split('@')[0];
                var players = db.GetCollection<DatabasePlayer>("players");

                var dbplayer = players.FindOne(x => x._id == playerID);

                int max = 0;
                if (dbplayer == null)
                {
                    var newwarn = new Warn
                    {
                        Reason = reason, Points = points, WarnerUsername = warner, Date = time ?? DateTime.Now.Date, Id = max
                    };
                    var ply = new DatabasePlayer()
                    {
                        _id = playerID, warns = new List<Warn> { newwarn }
                    };
                    if (players.FindOne(x => x._id == playerID) != null) return "Error";
                    players.Insert(ply);
                    return "Spieler wurde verwarnt";
                }

                if (dbplayer.warns != null && dbplayer.warns.Count != 0)
                {
                    max = dbplayer.warns.Select(keyValue => keyValue.Id).Prepend(max).Max();
                    max += 1;
                }

                var warn = new Warn
                {
                    Reason = reason, Points = points, WarnerUsername = warner, Date = time ?? DateTime.Now.Date, Id = max
                };
                dbplayer.warns ??= new List<Warn>();
                dbplayer.warns.Add(warn);
                players.Update(dbplayer);
                return "Spieler wurde verwarnt";
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        public static string GetWarns(string steamid, bool onlynew, out bool haswarns)
        {
            var playerID = steamid.Split('@')[0];
            var players = db.GetCollection<DatabasePlayer>("players");
            var dbplayer = players.FindOne(x => x._id == playerID);
            if (dbplayer == null)
            {
                haswarns = false;
                return "Dieser Spieler hat keine Verwarnungen";
            }

            if (dbplayer.warns == null || dbplayer.warns.Count == 0)
            {
                haswarns = false;
                return "Dieser Spieler hat keine Verwarnungen";

            } 
            string builder = "\n------------------------------------------------\n";
            float total = 0;

            
            
            if (onlynew)
            {
                    var newWarns = (from warn in dbplayer.warns
                        let date = warn.Date.AddDays(30)
                        let span = DateTime.Now - warn.Date
                        where span.Days <= 30
                        select warn).ToList();
                    if (newWarns.Count == 0)
                    {
                        haswarns = false;
                        return "Dieser Spieler hat keine Verwarnungen";
                    }
                    foreach (var warn in newWarns)
                    {
                        var span = DateTime.Now - warn.Date;
                        total += warn.Points;

                        builder +=
                            $"Verwarnung({warn.Id})" +
                            $"\nGrund: {warn.Reason}" +
                            $"\nPunkte: {warn.Points}" +
                            $"\nModerator: {warn.WarnerUsername}" +
                            $"\nVor {span.Days} Tagen" +
                            $"\n------------------------------------------------\n";
                    }

                    haswarns = true;
                    return builder + "\n\nTotal: " + GetTotal(steamid) + " Punkte";
            }
            
            var oldwarns = dbplayer.warns;
            foreach (var warn in oldwarns)
            {
                var span = DateTime.Now - warn.Date;
                total += warn.Points;

                builder +=
                    $"Verwarnung({warn.Id})" +
                    $"\nGrund: {warn.Reason}" +
                    $"\nPunkte: {warn.Points}" +
                    $"\nModerator: {warn.WarnerUsername}" +
                    $"\nVor {span.Days} Tagen" +
                    $"\n------------------------------------------------\n";
            }
            haswarns = true;
            return builder + "\n\nTotal: " + GetTotal(steamid) + " Punkte";
        }

        public static float GetTotal(string steamid)
        {
            var playerID = steamid.Split('@')[0];
            var players = db.GetCollection<DatabasePlayer>("players");

            var dbplayer = players.FindOne(x => x._id == playerID);

            if (dbplayer?.warns == null)
            {
                return 0;
            }

            if (dbplayer.warns.Count == 0)
                return 0;
            var newWarns = (from warn in dbplayer.warns
                let date = warn.Date.AddDays(30)
                let span = DateTime.Now - warn.Date
                where span.Days <= 30
                select warn).ToList();
            return (from warn in newWarns let span = DateTime.Now - warn.Date select warn.Points).Sum();
        }

        #region Transfer stuff

        public static Dictionary<string, List<OldWarn>> WarnsBySteam64ID = new();

        public static void Update()
        {
            string yaml = new Serializer().Serialize(WarnsBySteam64ID);
            File.WriteAllText(Path.Combine(Paths.Configs, "warns.yaml"), yaml);
            string yamldata = File.ReadAllText(Path.Combine(Paths.Configs, "warns.yaml"));
            var e = new Deserializer().Deserialize<Dictionary<string, List<OldWarn>>>(yamldata);
            WarnsBySteam64ID = e;
        }

        public static void Read()
        {
            if (!File.Exists(Path.Combine(Paths.Configs, "warns.yaml")))
            {
                Update();
            }

            string yamldata = File.ReadAllText(Path.Combine(Paths.Configs, "warns.yaml"));
            var e = new Deserializer().Deserialize<Dictionary<string, List<OldWarn>>>(yamldata);
            WarnsBySteam64ID = e;
            if (WarnsBySteam64ID.Count == 0)
            {
                WarnsBySteam64ID.Add("DEFAULT", new List<OldWarn>()
                {
                    new()
                    {
                        Reason = "DEFAULT", Points = 0, WarnerUsername = "DEFAULT",
                        Date = DateTime.Now.Date, Id = 0
                    },
                });
            }
        }
        public struct OldWarn
        {
            public int Id { get; set; }
            public string Reason { get; set; }
            public float Points { get; set; }
            public string Extra { get; set; }
            public string WarnerUsername { get; set; }
            public DateTime Date { get; set; }
        }
        public static int Transfer()
        {
            try
            {
                Read();
                var i = 0;
                foreach (var keyValuePair in WarnsBySteam64ID)
                {
                    foreach (var variable in keyValuePair.Value)
                    {
                        i++;
                        if (keyValuePair.Value == null)
                        {
                            continue;
                        }

                        AddWarn(keyValuePair.Key, variable.WarnerUsername, variable.Points, variable.Reason, variable.Date);
                    }
                }

                return i;
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }



        #endregion
    }

    public struct Warn
    {
        public int Id { get; set; }
        public string Reason { get; set; }
        public float Points { get; set; }
        public string WarnerUsername { get; set; }
        public DateTime Date { get; set; }
    }
}