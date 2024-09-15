using Autodesk.Revit.DB;
using FillPatternEditor.Enums;
using FillPatternEditor.Models;
using FillPatternEditor.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FillPatternEditor.Utils
{
    public static class PatPatternConverter
    {
        private const int Round = 9; // Number of decimal places for rounding
        private const int MaxSpaceCount = 14; // Maximum number of spaces for formatting

        // Regular expressions for pattern file parsing
        private static readonly Regex UnitsRegex = new Regex("^;%UNITS=(.*)");
        private static readonly Regex HeaderRegex = new Regex("^\\*(.*),\\s+(.*)");
        private static readonly Regex TypeRegex = new Regex("^;%TYPE=(.*)");
        private static readonly Regex FamilyDataRegex = new Regex("^-?\\d*");

        // Reads pattern (.pat) files and converts them to a collection of PatPattern objects
        public static IEnumerable<PatPattern> ReadPatPatternsFromFile(string fileName)
        {
            PatPattern patPattern = null;
            PatUnits patUnits = PatUnits.INCH;

            // Iterate through each line in the .pat file
            foreach (string readLine in File.ReadLines(fileName, Encoding.Default))
            {
                string line = readLine;
                // Ignore comment lines unless they contain metadata (e.g. ;%UNITS)
                if (!line.StartsWith(";") || line.StartsWith(";%"))
                {
                    Match match = UnitsRegex.Match(line);
                    if (match.Success && match.Groups.Count > 0)
                    {
                        // Determine pattern units (MM or INCH)
                        patUnits = match.Groups[0].Value == "MM" ? PatUnits.MM : PatUnits.INCH;
                        if (patPattern != null)
                            patPattern.PatUnits = patUnits;
                    }
                    else
                    {
                        match = HeaderRegex.Match(line);
                        if (match.Success)
                        {
                            // If a pattern is already being processed, yield it
                            if (patPattern != null)
                                yield return patPattern;

                            // Create a new PatPattern based on the header
                            patPattern = new PatPattern
                            {
                                PatUnits = patUnits,
                                Name = match.Groups[1].Value
                            };

                            if (match.Groups.Count > 2)
                                patPattern.Comment = match.Groups[2].Value;

                            continue;
                        }
                        match = TypeRegex.Match(line);
                        if (match.Success && match.Groups.Count > 0 && patPattern != null)
                        {
                            // Set the pattern target type (MODEL or DRAFTING)
                            patPattern.Target = match.Groups[1].Value == "MODEL"
                                ? FillPatternTarget.Model
                                : FillPatternTarget.Drafting;
                        }
                        else
                        {
                            match = FamilyDataRegex.Match(line);
                            if (match.Success && patPattern != null)
                            {
                                // Parse the family data
                                IEnumerable<string> dStrValues = line.Split(',').Select(s => s.Trim());
                                List<double> dValues = new List<double>();

                                foreach (string dStrValue in dStrValues)
                                {
                                    if (double.TryParse(dStrValue, NumberStyles.Number, CultureInfo.InvariantCulture, out double d))
                                        dValues.Add(d);
                                }

                                // If enough values are parsed, add a new PatLineFamily
                                if (dValues.Count >= 5)
                                    patPattern.LineFamilies.Add(new PatLineFamily(dValues));

                                dStrValues = null;
                                dValues = null;
                            }
                        }
                    }
                }
            }

            // Yield the last pattern if one exists
            if (patPattern != null)
                yield return patPattern;
        }

        // Converts PatPattern to FillGrids (used in Revit)
        public static List<FillGrid> ToFillGrids(this PatPattern patPattern, double importScale)
        {
            List<FillGrid> fillGrids = new List<FillGrid>();
            PatUnits patUnits = patPattern.PatUnits;

            foreach (PatLineFamily lineFamily in patPattern.LineFamilies)
            {
                // Create FillGrid based on line family data
                FillGrid fillGrid = new FillGrid
                {
                    Angle = Utils.DegreeToRadian(lineFamily.Angle),
                    Origin = new UV(lineFamily.XOrigin.ConvertToFeet(patUnits, importScale), lineFamily.YOrigin.ConvertToFeet(patUnits, importScale)),
                    Shift = lineFamily.Shift.ConvertToFeet(patUnits, importScale),
                    Offset = lineFamily.Offset.ConvertToFeet(patUnits, importScale)
                };

                if (lineFamily.DashSpace.Any())
                {
                    List<double> source = new List<double>();
                    foreach (var tuple in lineFamily.DashSpace)
                    {
                        // Convert dash and space values to feet
                        source.Add(tuple.Item1.ConvertToFeet(patUnits, importScale));
                        source.Add(tuple.Item2.ConvertToFeet(patUnits, importScale));
                    }

                    // Set the segments (dash/space) for the fill grid
                    fillGrid.SetSegments(source.Select(Math.Abs).ToList());
                }

                fillGrids.Add(fillGrid);
            }

            return fillGrids;
        }

        // Converts an IPattern object to a PatPattern
        public static PatPattern ToPatPattern(this IPattern pattern, PatUnits patUnits)
        {
            return pattern.FillPattern.ToPatPattern(patUnits, pattern.Name, pattern.Target);
        }

        // Converts a FillPattern to a PatPattern
        public static PatPattern ToPatPattern(this FillPattern fillPattern, PatUnits patUnits, string patternName, FillPatternTarget fillPatternTarget)
        {
            PatPattern patPattern = new PatPattern
            {
                Name = patternName,
                Target = fillPatternTarget,
                PatUnits = patUnits
            };

            // Convert FillGrids to PatLineFamily
            foreach (FillGrid fillGrid in fillPattern.GetFillGrids())
            {
                PatLineFamily patLineFamily = new PatLineFamily
                {
                    Angle = Utils.RadianToDegree(fillGrid.Angle),
                    Shift = fillGrid.Shift.ConvertToPatUnit(patUnits),
                    Offset = fillGrid.Offset.ConvertToPatUnit(patUnits),
                    XOrigin = fillGrid.Origin.U.ConvertToPatUnit(patUnits),
                    YOrigin = fillGrid.Origin.V.ConvertToPatUnit(patUnits)
                };

                // Convert segments (dash/space)
                IList<double> segments = fillGrid.GetSegments();
                for (int index = 0; index < segments.Count; index += 2)
                {
                    double dash = segments[index];
                    double space = segments[index + 1];
                    if (!double.IsNaN(dash) && !double.IsNaN(space))
                        patLineFamily.DashSpace.Add(new Tuple<double, double>(dash.ConvertToPatUnit(patUnits), space.ConvertToPatUnit(patUnits)));
                }

                patPattern.LineFamilies.Add(patLineFamily);
            }

            return patPattern;
        }

        // Converts PatPattern to its string representation for saving to a file
        public static string GetStringRepresentation(this PatPattern patPattern)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"*{patPattern.Name},  {patPattern.Comment}");
            stringBuilder.AppendLine($";%TYPE={patPattern.GetTargetStringRepresentation()}");

            foreach (PatLineFamily lineFamily in patPattern.LineFamilies)
            {
                stringBuilder.AppendLine(lineFamily.GetStringRepresentation());
            }

            return stringBuilder.ToString();
        }

        // Converts PatUnits to the corresponding unit directive string
        public static string GetUnitsDirective(this PatUnits patUnits)
        {
            return patUnits == PatUnits.MM ? ";%UNITS=MM" : ";%UNITS=INCH";
        }

        // Writes patterns to a .pat file
        public static void WriteStringRepresentationsToFile(string fileName, IEnumerable<IPattern> patterns, PatUnits patUnits)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(";; This file created by plugin \"Patterns\" included in ModPlus");
            stringBuilder.AppendLine(";; https://modplus.org");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(patUnits.GetUnitsDirective());
            stringBuilder.AppendLine(";%VERSION=3.0");
            stringBuilder.AppendLine();

            foreach (IPattern pattern in patterns)
            {
                stringBuilder.AppendLine(pattern.ToPatPattern(patUnits).GetStringRepresentation());
            }

            File.WriteAllText(fileName, stringBuilder.ToString(), Encoding.Default);
        }

        // Converts a PatLineFamily to its string representation
        private static string GetStringRepresentation(this PatLineFamily patLineFamily)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(patLineFamily.Angle.GetStringRepresentation(4, 14));
            stringBuilder.Append(patLineFamily.XOrigin.GetStringRepresentation(9, 14));
            stringBuilder.Append(patLineFamily.YOrigin.GetStringRepresentation(9, 14));
            stringBuilder.Append(patLineFamily.Shift.GetStringRepresentation(9, 14));

            string offsetStr = Math.Round(patLineFamily.Offset, 9).ToString(CultureInfo.InvariantCulture);
            if (offsetStr.Length >= 14)
                offsetStr = offsetStr.Substring(0, 13);
            stringBuilder.Append(offsetStr);

            if (patLineFamily.DashSpace.Any())
            {
                stringBuilder.Append($",{new string(' ', 14 - offsetStr.Length)}");

                foreach (var tuple in patLineFamily.DashSpace)
                {
                    stringBuilder.Append($"{tuple.Item1.GetStringRepresentation(9, 14)}-{tuple.Item2.GetStringRepresentation(9, 13)}");
                }
            }

            return stringBuilder.ToString().TrimEnd().TrimEnd(',');
        }

        // Helper method for formatting a double value to a string with a specified rounding and spacing
        private static string GetStringRepresentation(this double value, int round, int maxSpace)
        {
            string str = Math.Round(value, round).ToString(CultureInfo.InvariantCulture);
            if (str.Length >= maxSpace)
                str = str.Substring(0, maxSpace - 1);
            return str + "," + new string(' ', maxSpace - str.Length);
        }

        // Converts PatPattern's target to a string ("MODEL" or "DRAFTING")
        private static string GetTargetStringRepresentation(this PatPattern patPattern)
        {
            return patPattern.Target == FillPatternTarget.Model ? "MODEL" : "DRAFTING";
        }

        // Converts value from pattern units to feet (supports MM and INCH units)
        private static double ConvertToFeet(this double value, PatUnits patUnits, double importScale)
        {
            return patUnits == PatUnits.MM ? Utils.MmToFt(value) * importScale : value.InchToFt() * importScale;
        }
    }
}
