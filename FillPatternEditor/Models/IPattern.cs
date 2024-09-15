using Autodesk.Revit.DB;

namespace FillPatternEditor.Models
{
    // IPattern interface defines the common properties for patterns
    public interface IPattern
    {
        // The name of the pattern
        string Name { get; set; }

        // The Revit FillPattern associated with the pattern
        FillPattern FillPattern { get; set; }

        // The target type of the pattern (Drafting or Model)
        FillPatternTarget Target { get; set; }
    }
}
