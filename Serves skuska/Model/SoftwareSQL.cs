using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tabber.Model
{
    class SoftwareSQL
    {

        //public static Software getById(int id)
        //{
        //    var db = new BakalarkaEntities();
        //    var softwares = db.Softwares.Where(p => p.id == id);

        //    Software s;
        //    if (softwares.Count() > 0)
        //    {
        //        s = softwares.First();
        //        return s;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public static Software getByProcessName(String processName)
        //{
        //    var db = new BakalarkaEntities();
        //    var softwares = db.Softwares.Where(p => p.process == processName);

        //    Software s;
        //    if (softwares.Count() > 0)
        //    {
        //        s = softwares.First();
        //        return s;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public static Software getOrAdd(ProcessData process)
        //{

        //    Software s = getByProcessName(process.processName);
        //    if (s == null)
        //    {
        //        var db = new BakalarkaEntities();
        //        s = new Software
        //        {
        //            name = process.description,
        //            process = process.processName,
        //            filepath = process.filePath
        //        };
        //        if (validate(s))
        //        {
        //            db.Softwares.Add(s);
        //            db.SaveChanges();
        //            Debug.WriteLine("Pridane do databazy " + process.processName + " > " + s.id + " |");
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //    return s;
        //}
        //public static Boolean validate(Software s)
        //{
        //    if (String.IsNullOrEmpty(s.name) || String.IsNullOrEmpty(s.process) || String.IsNullOrEmpty(s.filepath))
        //        return false;

        //    return true;
        //}



    }



}
