using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectShowLib;
using DirectShowLib.DES;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using Updates_namespace;


namespace Tabber.Model
{
    class VideoSQL
    {
        //String file_name, name;
        //int length;
        //Software video_player;

        //public VideoSQL(String filename, String name, ProcessData video_player)
        //{
        //    this.file_name = filename;
        //    this.name = name;
        //    this.video_player = SoftwareSQL.getByProcessName(video_player.processName);
        //    this.length = getMovieLength(this.file_name);

        //}

        //public void save(int user_id)
        //{
        //    try
        //    {
        //        Movie m = getByFilename(this.file_name);
        //        if (m == null)
        //        {
        //            m = new Movie
        //            {
        //                filename = this.file_name,
        //                name = this.name,
        //                length = this.length,
        //                times = 1,
        //                software_id = this.video_player.id

        //            };
        //            Debug.WriteLine("Pridavam>" + this.length + " > " + this.file_name);
        //            m = VideoSQL.addMovie(m);
        //        }


        //        Movie lastMovie = getLastLoggedMovie(user_id);


        //        if (lastMovie == null || lastMovie != null && lastMovie.id != m.id)
        //        {
        //            endLastLoggedMovie(user_id);
        //            Log_Movie lm = new Log_Movie
        //            {
        //                software_id = this.video_player.id,
        //                movie_id = m.id,
        //                user_id = user_id,
        //                started = DateTime.Now
        //            };
        //            var db = new BakalarkaEntities();
        //            db.Log_Movie.Add(lm);
        //            db.SaveChanges();
        //        }

        //    }
        //    catch (System.Exception ex)
        //    {
        //        Error.Show(ex.ToString(), Form1.get_user_id().ToString());
        //        throw ex;
        //    }
        //}

        //public static Movie addMovie(Movie movie)
        //{
        //    try
        //    {
        //        var db = new BakalarkaEntities();
        //        db.Movies.Add(movie);
        //        db.SaveChanges();
        //        return movie;
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Error.Show("CHYBA>" + ex.ToString(), Form1.get_user_id().ToString());

        //        Debug.WriteLine("CHYBA>" + ex.ToString());
        //        throw new Exception();
        //    }

        //}

        //public static Movie getLastLoggedMovie(int user_id)
        //{
        //    var db = new BakalarkaEntities();
        //    var logs = db.Log_Movie.Where(lm => lm.user_id == user_id);

        //    Movie m;
        //    if (logs.Count() > 0)
        //    {
        //        m = db.Movies.Find(logs.OrderByDescending(lm => lm.started).First().movie_id);
        //        return m;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public static void endLastLoggedMovie(int user_id)
        //{

        //    var db = new BakalarkaEntities();
        //    var logs = db.Log_Movie.Where(lm => lm.user_id == user_id);

        //    Log_Movie lastlm;
        //    if (logs.Count() > 0)
        //    {
        //        lastlm = logs.OrderByDescending(lm => lm.started).First();
        //        if (lastlm.ended == null)
        //        {
        //            lastlm.ended = DateTime.Now;
        //        }
        //        db.SaveChanges();

        //    }
        //}

        //public static Movie getByFilename(String fileName)
        //{
        //    var db = new BakalarkaEntities();
        //    var movies = db.Movies.Where(p => p.filename == fileName);
        //    Debug.WriteLine(movies.Count() + " > " + fileName);

        //    Movie m;
        //    if (movies.Count() > 0)
        //    {
        //        m = movies.First();
        //        return m;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public int getMovieLength(String file_name)
        //{
        //    return 0;
        //    try
        //    {


        //        var mediaDet = (IMediaDet)new MediaDet();
        //        DsError.ThrowExceptionForHR(mediaDet.put_Filename(file_name));

        //        // find the video stream in the file
        //        int index;
        //        var type = Guid.Empty;
        //        for (index = 0; index < 1000 && type != MediaType.Video; index++)
        //        {
        //            mediaDet.put_CurrentStream(index);
        //            mediaDet.get_StreamType(out type);
        //        }

        //        // retrieve some measurements from the video
        //        double frameRate;
        //        mediaDet.get_FrameRate(out frameRate);

        //        var mediaType = new AMMediaType();
        //        mediaDet.get_StreamMediaType(mediaType);
        //        var videoInfo = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader));
        //        DsUtils.FreeAMMediaType(mediaType);
        //        var width = videoInfo.BmiHeader.Width;
        //        var height = videoInfo.BmiHeader.Height;

        //        double mediaLength;
        //        mediaDet.get_StreamLength(out mediaLength);
        //        var frameCount = (int)(frameRate * mediaLength);
        //        var duration = frameCount / frameRate;
        //        return int.Parse(Math.Round(duration).ToString());
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return 0;
        //    }
        //}




    }
}
