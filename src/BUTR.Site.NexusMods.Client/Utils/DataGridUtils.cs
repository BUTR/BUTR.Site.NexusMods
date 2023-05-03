using Blazorise.DataGrid;

namespace BUTR.Site.NexusMods.Client.Utils;

public static class DataGridUtils
{
    public static void SelectDeselect<TEntry>(TEntry? model, ref TEntry? entry, ref DataGrid<TEntry> dataGrid) where TEntry : class
    {
        if (entry != model)
        {
            entry = model;
        }
        else if (entry is not null)
        {
#pragma warning disable BL0005
            dataGrid.SelectedRow = null!;
#pragma warning restore BL0005
            entry = null;
        }
        else
        {
            entry = model;
        }
    }
}