﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using Ariadna.Data;
using Manina.Windows.Forms;

namespace Ariadna.DatabaseStrategies;

public abstract class AbstractDbStrategy
{
    public event EntryInsertedEventHandler EntryInserted;
    // ReSharper disable once IdentifierTypo
    public delegate void EntryInsertedEventHandler(object sender, EntryInsertedEventArgs hlpevent);

    public class EntryInsertedEventArgs(int id) : EventArgs
    {
        public int Id { get; } = id;
    }

    public class QueryParams 
    {
        public string Name { get; set; }
        public string Director { get; set; }
        public string Actor { get; set; }
        public string Genre { get; set; }
        public string Subgenre { get; set; }
        public bool IsWish { get; set; }
        public bool IsRecent { get; set; }
        public bool IsNew { get; set; }
        public bool IsVr { get; set; }
        public bool IsNonVr { get; set; }
        public bool IsSeries { get; set; }
        public bool IsMovies { get; set; }
    }
    public abstract ImageListView.ImageListViewItemAdaptor GetPosterImageAdapter();
    public abstract List<EntryDto> GetEntries();
    public abstract List<EntryDto> QueryEntries(QueryParams values);
    public abstract EntryInfo GetEntryInfo(int id);
    public abstract void ShowEntryDetails(int id);
    public abstract void ExecuteEntry(int id);
    public abstract void RemoveEntry(int id);
    public abstract bool FindNextEntryAutomatically();
    public abstract void FindNextEntryManually();
    public abstract void UpdateSubgenre(MainPanel panel);
    public abstract string[] QuickListFilter();
    public abstract ImmutableSortedDictionary<string, Bitmap> GetDirectors(string name, int limit);
    public abstract ImmutableSortedDictionary<string, Bitmap> GetActors(string name, int limit);
    public abstract ImmutableSortedDictionary<string, Bitmap> GetGenres();
    public abstract ImmutableSortedDictionary<string, Bitmap> GetSubgenres(string name);
    public abstract void FilterControls(MainPanel panel);
    protected virtual void OnEntryInserted(EntryInsertedEventArgs e) => EntryInserted!.Invoke(this, e);
}