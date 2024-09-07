using Autodesk.Revit.DB;

namespace FillPatternEditor
{
    public static class FillPatternExtensions
    {
        public static FillPattern GetFillPattern(this FillPatternElement fillPatternElement)
        {
            if (fillPatternElement == null) return null;

            return fillPatternElement.GetFillPattern();
        }
    }
}
