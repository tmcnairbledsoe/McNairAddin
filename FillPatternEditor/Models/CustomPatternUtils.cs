using Autodesk.Revit.DB;
using FillPatternEditor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FillPatternEditor.Models
{
    public static class CustomPatternUtils
    {
        /// <summary>
        /// Modifies the size of the fill grids by applying a change function to each property of the grid.
        /// Optionally verifies the new offset using a verification function.
        /// </summary>
        /// <param name="fillGrids">The collection of fill grids to modify.</param>
        /// <param name="changeFunc">Function to apply to grid properties like Offset, Shift, and Origin.</param>
        /// <param name="verificationOffsetFunc">Optional function to verify the new Offset value.</param>
        /// <returns>A list of modified fill grids, or null if verification fails.</returns>
        public static List<FillGrid> ChangeFillGridSize(
            IEnumerable<FillGrid> fillGrids,
            Func<double, double> changeFunc,
            Func<double, bool> verificationOffsetFunc = null)
        {
            var modifiedFillGrids = new List<FillGrid>();

            // Loop through each fill grid and apply the changeFunc to its properties
            foreach (FillGrid originalFillGrid in fillGrids)
            {
                // Apply the change function to the Offset
                double newOffset = changeFunc(originalFillGrid.Offset);

                // If verification function exists and fails, return null
                if (verificationOffsetFunc != null && !verificationOffsetFunc(newOffset))
                    return null;

                // Create a new FillGrid with modified properties
                var modifiedFillGrid = new FillGrid(originalFillGrid)
                {
                    Offset = newOffset,
                    Shift = changeFunc(originalFillGrid.Shift),
                    Origin = new UV(changeFunc(originalFillGrid.Origin.U), changeFunc(originalFillGrid.Origin.V))
                };

                // Apply change function to each segment in the fill grid
                var newSegments = new List<double>();
                foreach (double segment in originalFillGrid.GetSegments())
                {
                    newSegments.Add(changeFunc(segment));
                }

                // Set modified segments and add the new fill grid to the list
                modifiedFillGrid.SetSegments(newSegments);
                modifiedFillGrids.Add(modifiedFillGrid);
            }

            return modifiedFillGrids;
        }

        /// <summary>
        /// Detects whether the fill pattern is basic or custom based on grid count and stroke properties.
        /// </summary>
        /// <param name="fillPattern">The fill pattern to analyze.</param>
        /// <returns>FillPatternCreationType.Basic or FillPatternCreationType.Custom based on analysis.</returns>
        public static FillPatternCreationType DetectCreationType(this FillPattern fillPattern)
        {
            // Conditions for a custom fill pattern:
            // - More than 2 grids, or
            // - StrokesPerArea is greater than 0, or
            // - Grid count is not 1, and either:
            //   - Grid count is not 2, or
            //   - The angle difference between the first two grids is not approximately 90 degrees (Pi/2 radians)
            if (fillPattern.GridCount > 2 ||
                fillPattern.StrokesPerArea > 0.0 ||
                (fillPattern.GridCount != 1 &&
                 (fillPattern.GridCount != 2 ||
                  Math.Abs(Math.Abs(fillPattern.GetFillGrid(0).Angle - fillPattern.GetFillGrid(1).Angle) - Math.PI / 2.0) >= 0.1)))
            {
                return FillPatternCreationType.Custom;
            }

            return FillPatternCreationType.Basic;
        }
    }
}
