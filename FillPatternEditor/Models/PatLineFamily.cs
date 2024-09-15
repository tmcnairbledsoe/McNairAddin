using System;
using System.Collections.Generic;
using System.Linq;

namespace FillPatternEditor.Models
{
    // Class representing a line family in a pattern, including geometric and line dash properties
    public class PatLineFamily
    {
        // Default constructor
        public PatLineFamily()
        {
        }

        // Constructor that initializes the line family using a list of double values
        public PatLineFamily(IReadOnlyList<double> dValues)
        {
            // Set properties based on the first few elements of the dValues list
            this.Angle = dValues[0];
            this.XOrigin = dValues[1];
            this.YOrigin = dValues[2];
            this.Shift = dValues[3];
            this.Offset = dValues[4];

            // If there are more than 5 values and the count is odd, process the remaining as dash/space pairs
            if (dValues.Count > 5 && dValues.Count % 2 != 0)
            {
                for (int index = 5; index < dValues.Count; index += 2)
                {
                    // Add dash and space values as a tuple to DashSpace list
                    this.DashSpace.Add(new Tuple<double, double>(dValues[index], dValues[index + 1]));
                }
            }
        }

        // Properties for geometric and line dash characteristics
        public double Angle { get; set; }  // Angle of the line
        public double XOrigin { get; set; }  // X origin point
        public double YOrigin { get; set; }  // Y origin point
        public double Shift { get; set; }  // Shift of the line
        public double Offset { get; set; }  // Offset of the line

        // List of dash and space values represented as a tuple (Dash length, Space length)
        public List<Tuple<double, double>> DashSpace { get; set; } = new List<Tuple<double, double>>();

        // Property to determine if the line family needs to expand dots (based on dash length)
        public bool NeedExpandDots
        {
            get
            {
                // Check if any dash length is very close to zero (considered as needing to expand dots)
                return this.DashSpace
                           .Select(ds => ds.Item1) // Select the dash length
                           .Any(d => Math.Abs(d) < 0.0001); // Check if the absolute value is almost zero
            }
        }
    }
}
