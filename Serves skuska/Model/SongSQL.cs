using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Tabber.Model
{
    class SongSQL
    {


        //public static Song getByFilename(String fileName)
        //{
        //    var db = new BakalarkaEntities();
        //    var songs = db.Songs.Where(p => p.fileName == fileName);
        //    Debug.WriteLine(songs.Count() + " > " + fileName);

        //    Song s;
        //    if (songs.Count() > 0)
        //    {
        //        s = songs.First();
        //        return s;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public static Song getLastLoggedSong(int user_id)
        //{
        //    var db = new BakalarkaEntities();
        //    var logs = db.Log_Song.Where(ls => ls.user_id == user_id);

        //    Song s;
        //    if (logs.Count() > 0)
        //    {
        //        s = db.Songs.Find(logs.OrderByDescending(ls => ls.started).First().song_id);
        //        return s;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}


        //public static void endLastLoggedSong(int user_id)
        //{

        //    var db = new BakalarkaEntities();
        //    var logs = db.Log_Song.Where(ls => ls.user_id == user_id);

        //    Log_Song lastls;
        //    if (logs.Count() > 0)
        //    {
        //        lastls = logs.OrderByDescending(ls => ls.started).First();
        //        if (lastls.ended == null)
        //        {
        //            lastls.ended = DateTime.Now;
        //        }
        //        db.SaveChanges();

        //    }
        //}

        //public static Song addSong(Song song)
        //{
        //    try
        //    {
        //        var db = new BakalarkaEntities();
        //        db.Songs.Add(song);
        //        db.SaveChanges();
        //        return song;
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Debug.WriteLine("CHYBA>" + ex.ToString());
        //        throw ex;
        //    }

        //}

        //public static void logSong(ProcessData songPlayer, Song song, int user_id)
        //{
        //    Song s = getByFilename(song.fileName);
        //    if (s == null)
        //    {
        //        Debug.WriteLine("Pridavam>" + song.length + " > " + song.fileName + " > " + song.fileName.Length);
        //        s = addSong(song);
        //    }

        //    Song lastSong = getLastLoggedSong(user_id);


        //    if (lastSong == null || lastSong != null && lastSong.id != s.id)
        //    {
        //        endLastLoggedSong(user_id);
        //        Log_Song ls = new Log_Song
        //        {
        //            software_id = SoftwareSQL.getByProcessName(songPlayer.processName).id,
        //            song_id = s.id,
        //            user_id = user_id,
        //            started = DateTime.Now
        //        };
        //        var db = new BakalarkaEntities();
        //        db.Log_Song.Add(ls);
        //        db.SaveChanges();
        //    }

        //}
    }
}
