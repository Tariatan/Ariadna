﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Manina.Windows.Forms;

namespace Ariadna
{
    public abstract class AbstractDBStrategy
    {
        public event EntryInsertedEventHandler EntryInserted;
        public delegate void EntryInsertedEventHandler(object sender, EntryInsertedEventArgs hlpevent);

        public class EntryInsertedEventArgs : EventArgs
        {
            public EntryInsertedEventArgs(int id)
            {
                Id = id;
            }

            public int Id { get; }
        }

        public class QueryParams 
        {
            public string Name { get; set; }
            public string Director { get; set; }
            public string Actor { get; set; }
            public string Genre { get; set; }
            public bool IsWish { get; set; }
            public bool IsRecent { get; set; }
            public bool IsNew { get; set; }
            public bool IsVR { get; set; }
            public bool IsNonVR { get; set; }
            public bool IsSeries { get; set; }
            public bool IsMovies { get; set; }
        }
        public abstract ImageListView.ImageListViewItemAdaptor GetPosterImageAdapter();
        public abstract List<Utilities.EntryDto> GetEntries();
        public abstract List<Utilities.EntryDto> QueryEntries(QueryParams values);
        public abstract Utilities.EntryInfo GetEntryInfo(int id);
        public abstract void ShowEntryDetails(int id);
        public abstract void ExecuteEntry(int id);
        public abstract void RemoveEntry(int id);
        public abstract bool FindNextEntryAutomatically();
        public abstract void FindNextEntryManually();
        public abstract SortedDictionary<string, Bitmap> GetDirectors(string name, int limit);
        public abstract SortedDictionary<string, Bitmap> GetActors(string name, int limit);
        public abstract SortedDictionary<string, Bitmap> GetGenres(string name);
        public abstract void FilterControls(MainPanel panel);
        protected virtual void OnEntryInserted(EntryInsertedEventArgs e) => EntryInserted.Invoke(this, e);
    }
}
