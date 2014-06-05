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
    
    public partial class User
    {
        public User()
        {
            this.Log_Movie = new HashSet<Log_Movie>();
            this.Log_Software = new HashSet<Log_Software>();
            this.Log_Song = new HashSet<Log_Song>();
            this.Word = new HashSet<Word>();
            this.Clipboards = new HashSet<Clipboards>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string pass { get; set; }
        public string token { get; set; }
        public Nullable<int> annota_id { get; set; }
        public string pc_uniq { get; set; }
        public string persistence_token { get; set; }
        public string ip { get; set; }
    
        public virtual ICollection<Log_Movie> Log_Movie { get; set; }
        public virtual ICollection<Log_Software> Log_Software { get; set; }
        public virtual ICollection<Log_Song> Log_Song { get; set; }
        public virtual ICollection<Word> Word { get; set; }
        public virtual ICollection<Clipboards> Clipboards { get; set; }
    }
}
