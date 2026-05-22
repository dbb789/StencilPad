using System.Windows.Controls;
using StencilPad.Canvases.Tools.Actions;
using StencilPad.Models;

namespace StencilPad.Canvases.Tools.Widgets;

public static class ContextMenuUtil
{
    public static bool RebuildContextMenu(ContextMenu contextMenu,
                                          Sheet sheet,
                                          IEnumerable<ISheetElement> selection,
                                          IEnumerable<ISheetElementAction?> actions,
                                          Action<ISheetElementAction>? actionInvoked)
    {
        contextMenu.Items.Clear();

        if (selection.Any())
        {
            foreach (var action in actions)
            {
                if (action is null)
                {
                    contextMenu.Items.Add(new Separator());
                }
                else if (action.IsVisible(sheet, selection))
                {
                    var menuItem = new MenuItem
                    {
                        Header = action.Name
                    };

                    menuItem.IsEnabled = action.IsEnabled(sheet, selection);
                    menuItem.Click += (s, e) =>
                    {
                        actionInvoked?.Invoke(action);
                    };

                    contextMenu.Items.Add(menuItem);
                }
            }
        }

        return contextMenu.Items.Count > 0;
    }
}
