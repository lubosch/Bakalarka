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
    
    public partial class Word
    {
        public Word()
        {
            this.Heading_Word = new HashSet<Heading_Word>();
            this.Keyword_Word = new HashSet<Keyword_Word>();
            this.Text_Word = new HashSet<Text_Word>();
        }
    
        public int id { get; set; }
        public string filePath { get; set; }
        public string name { get; set; }
        public System.DateTime timestamp { get; set; }
        public Nullable<int> user_id { get; set; }
    
        public virtual ICollection<Heading_Word> Heading_Word { get; set; }
        public virtual ICollection<Keyword_Word> Keyword_Word { get; set; }
        public virtual ICollection<Text_Word> Text_Word { get; set; }
        public virtual User User { get; set; }
    }
}
