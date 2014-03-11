using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tabber.Model
{
    class LogSoftwareSQL
    {

        //public LogSoftwareSQL()
        //{

        //}

        //public static void AddNewLog(ProcessData process, int user_id)
        //{
        //    Debug.WriteLine(process.filePath + " > " + process.description + " > " + process.processName);
        //    if (process.processName != "" && (lastLog(user_id) == null || lastLog(user_id).softwareWindowName != process.windowName))
        //    {
        //        Software s = SoftwareSQL.getOrAdd(process);
        //        if (s == null)
        //        {
        //            return;
        //        }
        //        var db = new BakalarkaEntities();
        //        Log_Software ls = new Log_Software
        //        {
        //            software_id = s.id,
        //            user_id = user_id,
        //            timestamp = DateTime.Now,
        //            softwareWindowName = process.windowName
        //        };
        //        Debug.WriteLine(s.id + " : " + process.windowName);

        //        db.Log_Software.Add(ls);
        //        db.SaveChanges();

        //    }
        //}

        //public static Log_Software lastLog(int user_id)
        //{
        //    var db = new BakalarkaEntities();
        //    if (db.Log_Software.Where(l => l.user_id == user_id).Count() > 0)
        //    {
        //        return db.Log_Software.Where(l => l.user_id == user_id).OrderByDescending(l => l.timestamp).First();
        //    }
        //    else
        //    {
        //        return null;
        //    }

        //}

    }
}
