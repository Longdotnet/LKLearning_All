// Selectors/SegmentTemplateSelector.cs
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;
using mauiluanvantotnghiep.ViewModels;

namespace mauiluanvantotnghiep.Selectors;

public class SegmentTemplateSelector : DataTemplateSelector
{
    public DataTemplate TextTemplate { get; set; }
    public DataTemplate BlankTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is BlankItem)
            return BlankTemplate;
        return TextTemplate;
    }
}
