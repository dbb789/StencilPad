using System.Windows.Input;
using StencilPad.Canvases.Tools.Actions;
using StencilPad.Common;
using StencilPad.Models;

namespace StencilPad.Canvases.Tools.Widgets;

public class InputBindingsBuilder
{
    private readonly Sheet _sheet;
    private readonly Action<ISheetElementAction>? _actionInvoked;
    private readonly InputBindingCollection _inputBindings;
    
    public InputBindingsBuilder(Sheet sheet,
                                Action<ISheetElementAction>? actionInvoked,
                                InputBindingCollection inputBindings)
    {
        _sheet = sheet;
        _actionInvoked = actionInvoked;
        _inputBindings = inputBindings;
    }

    public void Add(Key key, ModifierKeys modifiers, params ISheetElementAction[] actionSet)
    {
        _inputBindings.Add(new KeyBinding(CreateActionCommand(actionSet),
                                          key,
                                          modifiers));
    }
    
    private RelayCommand CreateActionCommand(params ISheetElementAction [] actionSet)
    {
        return new RelayCommand(() =>
        {
            foreach (var action in actionSet)
            {
                if (action.IsEnabled(_sheet, _sheet.Selection) &&
                    action.IsVisible(_sheet, _sheet.Selection))
                {
                    action.Invoke(_sheet, _sheet.Selection);
                    return;
                }
            }
        });
    }
}
