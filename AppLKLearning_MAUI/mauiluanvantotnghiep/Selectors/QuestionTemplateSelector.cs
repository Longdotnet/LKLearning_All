// Selectors/QuestionTemplateSelector.cs
using mauiluanvantotnghiep.Models;
using mauiluanvantotnghiep.ViewModels;
using Microsoft.Maui.Controls;

namespace mauiluanvantotnghiep.Selectors;

public class QuestionTemplateSelector : DataTemplateSelector
{
    public DataTemplate MCQTemplate { get; set; }
    public DataTemplate TrueFalseTemplate { get; set; }
    public DataTemplate FillBlankTemplate { get; set; }
    public DataTemplate AudioTemplate { get; set; }
    public DataTemplate VideoTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is QuestionItemViewModel vm)
        {
            switch (vm.Model.QuestionTypeId)
            {
                case 1: return MCQTemplate;
                case 2: return TrueFalseTemplate;
                case 3: return FillBlankTemplate;
                case 4: return AudioTemplate;
                case 8: return VideoTemplate;
            }
        }
        return MCQTemplate;
    }
}
