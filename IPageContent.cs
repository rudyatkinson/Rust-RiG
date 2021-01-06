using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPageContent
{
    void InitPage();
    void FilterItems(NavigationPageEventArgs e);
    void Search();
    void CleanSearch();
    void Reset();
}
