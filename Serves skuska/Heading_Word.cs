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
    
    public partial class Heading_Word
    {
        public Heading_Word()
        {
            this.Heading_Word1 = new HashSet<Heading_Word>();
        }
    
        public int id { get; set; }
        public int word_id { get; set; }
        public System.DateTime timestamp { get; set; }
        public int rank { get; set; }
        public Nullable<int> previous_id { get; set; }
        public string text { get; set; }
    
        public virtual ICollection<Heading_Word> Heading_Word1 { get; set; }
        public virtual Heading_Word Heading_Word2 { get; set; }
        public virtual Word Word { get; set; }
        public virtual Keyword_Heading Keyword_Heading { get; set; }
    }
}
