//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tabber
{
    using System;
    using System.Collections.Generic;
    
    public partial class Software
    {
        public Software()
        {
            this.Log_Movie = new HashSet<Log_Movie>();
            this.Log_Software = new HashSet<Log_Software>();
            this.Log_Song = new HashSet<Log_Song>();
            this.Movie = new HashSet<Movie>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public string process { get; set; }
        public string filepath { get; set; }
        public Nullable<byte> ignore { get; set; }
    
        public virtual ICollection<Log_Movie> Log_Movie { get; set; }
        public virtual ICollection<Log_Software> Log_Software { get; set; }
        public virtual ICollection<Log_Song> Log_Song { get; set; }
        public virtual ICollection<Movie> Movie { get; set; }
    }
}
