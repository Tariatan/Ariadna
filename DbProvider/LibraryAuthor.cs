//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DbProvider
{
    using System;
    using System.Collections.Generic;
    
    public partial class LibraryAuthor
    {
        public int Id { get; set; }
        public Nullable<int> libraryId { get; set; }
        public Nullable<int> authorId { get; set; }
    
        public virtual Author Author { get; set; }
        public virtual Library Library { get; set; }
    }
}
