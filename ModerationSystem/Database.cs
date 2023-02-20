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
    public static LiteDatabase db = new(Path.Combine(Paths.Configs, "warns.db"));

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
                    break;
                }

                if (foundwarn)
                {
                    return "Verwarnung wurde gelöscht";
                }
                return "Verwarnung wurde nicht gefunden";
            }
            return "Spieler wurde nicht gefunden";


        }

        public static string AddWarn(string warned, string warner, float points, string reason)
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
                        Reason = reason, Points = points, WarnerUsername = warner, Date = DateTime.Now.Date, Id = max
                    };
                    var ply = new DatabasePlayer()
                    {
                        _id = playerID, warns = new List<Warn> { newwarn }
                    };
                    if (players.FindOne(x => x._id == playerID) != null) return "ERROR";
                    players.Insert(ply);
                    return "ea";
                }

                if (dbplayer.warns != null && dbplayer.warns.Count != 0)
                {
                    max = dbplayer.warns.Select(keyValue => keyValue.Id).Prepend(max).Max();
                    max += 1;
                }
                else
                    return "error";

                var warn = new Warn
                {
                    Reason = reason, Points = points, WarnerUsername = warner, Date = DateTime.Now.Date, Id = max
                };
                if (dbplayer.warns == null) return "Not found";

                Log.Info("test");
                dbplayer.warns.Add(warn);
                players.Update(dbplayer);
                return " hat keine Verwarnungen";
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        public static string GetWarns(string steamid, bool onlynew, bool showextra)
        {
            var playerID = steamid.Split('@')[0];
            var players = db.GetCollection<DatabasePlayer>("players");
            var dbplayer = players.FindOne(x => x._id == playerID);
            if (dbplayer == null)
                return "Dieser Spieler hat keine Verwarnungen";
            if (dbplayer.warns == null || dbplayer.warns.Count == 0)
                return "Dieser Spieler hat keine Verwarnungen";
            string builder = "\n------------------------------------------------\n";
            float total = 0;
            var newWarns = (from warn in dbplayer.warns
                let date = warn.Date.AddDays(30)
                let span = DateTime.Now - warn.Date
                where span.Days <= 30
                select warn).ToList();
            if (onlynew)
            {
                if (showextra)
                {
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

                    return builder;
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

                string Final = builder + "\n\n " + GetTotal(steamid);
                return Final;
            }

            return "ea";
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
        public static void RemovePlayer(Player? player)
        {
            if (player == null) return;
            var playerID = player.RawUserId.Split('@')[0];
            var players = db.GetCollection<DatabasePlayer>("players");
            var dbplayer = players.FindOne(x => x._id == playerID);

            if (dbplayer != null) players.Delete(dbplayer._id);
        }

        #region Transfer stuff

        public static Dictionary<string, List<Warn>> WarnsBySteam64ID = new();

        public static void Update()
        {
            string yaml = new Serializer().Serialize(WarnsBySteam64ID);
            File.WriteAllText(Path.Combine(Paths.Configs, "warns.yaml"), yaml);
            string yamldata = File.ReadAllText(Path.Combine(Paths.Configs, "warns.yaml"));
            var e = new Deserializer().Deserialize<Dictionary<string, List<Warn>>>(yamldata);
            WarnsBySteam64ID = e;
        }

        public static void Read()
        {
            if (!File.Exists(Path.Combine(Paths.Configs, "warns.yaml")))
            {
                Update();
            }

            string yamldata = File.ReadAllText(Path.Combine(Paths.Configs, "warns.yaml"));
            var e = new Deserializer().Deserialize<Dictionary<string, List<Warn>>>(yamldata);
            WarnsBySteam64ID = e;
            if (WarnsBySteam64ID.Count == 0)
            {
                WarnsBySteam64ID.Add("DEFAULT", new List<Warn>()
                {
                    new()
                    {
                        Reason = "DEFAULT", Points = 0, WarnerUsername = "DEFAULT",
                        Date = DateTime.Now.Date, Id = 0
                    },
                });
            }
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

                        AddWarn(keyValuePair.Key, variable.WarnerUsername, variable.Points, variable.Reason);
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