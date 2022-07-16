using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyAndCode.UI;

public class SearchManager : Singleton<SearchManager>, IRecyclableScrollRectDataSource
{
    [SerializeField] private RecyclableScrollRect scrollRect;

    void Awake()
    {
        scrollRect.DataSource = this;
    }
    
    private int maxResults = 500;
    public int GetItemCount() => maxResults;
    
    public void SetCell(ICell cell, int index)
    {
        _ = (cell as VideoInfo).Populate("Nj2U6rhnucI", index);
    }

    public void Search()
    {
        //TODO search
        scrollRect.ReloadData();
    }
}
