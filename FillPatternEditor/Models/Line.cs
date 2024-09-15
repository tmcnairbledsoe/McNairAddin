using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FillPatternEditor.Utils;

namespace FillPatternEditor.Models
{

    public class Line
    {
        public Point FirstPoint { get; set; }
        public Point SecondPoint { get; set; }

        public Line(Point firstPoint, Point secondPoint)
        {
            FirstPoint = firstPoint;
            SecondPoint = secondPoint;
        }

        public double Length => FirstPoint.DistanceTo(SecondPoint);
    }
}
