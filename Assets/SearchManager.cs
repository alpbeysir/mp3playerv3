using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchManager : Singleton<SearchManager>
{
    [SerializeField] private RecyclingListView listView;

    public int maxResults;

    private void Start()
    {
        listView.ItemCallback += PopulateDelegate;
        listView.RowCount = maxResults;
    }

    private void PopulateDelegate(RecyclingListViewItem item, int index)
    {
        _ = (item as VideoInfo).Populate("Nj2U6rhnucI", index);
    } 
}
