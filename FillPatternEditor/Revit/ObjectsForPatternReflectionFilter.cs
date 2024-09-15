using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;

namespace FillPatternEditor.Revit
{
    // Selection filter to allow only certain Revit elements for pattern selection
    public class ObjectsForPatternSelectionFilter : ISelectionFilter
    {
        private readonly List<ElementId> _excludeIds;

        // Constructor to initialize the filter with a list of element IDs to exclude
        public ObjectsForPatternSelectionFilter(List<ElementId> excludeIds)
        {
            _excludeIds = excludeIds;
        }

        // Allows elements that are not in the excluded list and are either DetailCurve or ModelCurve
        public bool AllowElement(Element elem)
        {
            // Check if the element ID is not in the exclusion list, and is either a DetailCurve or ModelCurve
            return !_excludeIds.Contains(elem.Id) && (elem is DetailCurve || elem is ModelCurve);
        }

        // This method is not implemented for this filter
        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
}
