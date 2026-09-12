using Helios.Search;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Helios.UI.Views;

/// <summary>
/// Ctrl+K quick-launch surface: fuzzy-searches devices, views, alerts,
/// reports, snapshots and rules via GlobalSearchService (SQLite FTS5) and
/// executes the chosen action/navigation on selection.
/// </summary>
public sealed partial class CommandPaletteWindow : Window
{
    public CommandPaletteWindow()
    {
        InitializeComponent();
        QueryBox.Focus(FocusState.Programmatic);
    }

    private void QueryBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var dbPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Helios", "helios.db");

        using var search = new GlobalSearchService(dbPath);
        var results = search.Search(QueryBox.Text);
        ResultsList.ItemsSource = results.Select(r => $"[{r.EntityType}] {r.Title}").ToList();
    }

    private void ResultsList_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        // A full implementation resolves the selected SearchResult back to its
        // entity and navigates MainWindow to the corresponding view/node.
        Close();
    }
}
