using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NNTest_MK3
{
    class NNVis_Node
    {
        protected Canvas _canvas;
        protected Ellipse _shape;
        protected double _width = 40;
        protected double _height = 40;

        public NNVis_Node(Canvas canvas, Point position)
        {
            _canvas = canvas;

            _shape = new Ellipse
            {
                Width = _width,
                Height = _height,
                StrokeThickness = 2,
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = new SolidColorBrush(Colors.White),
                Margin = new Thickness(position.X - _width / 2, position.Y - _height / 2, 0, 0)
            };
            _canvas.Children.Add(_shape);
        }

        public Point GetWeightPosition(bool start)
        {
            return new Point(_shape.Margin.Left + (start ? _width : 0), _shape.Margin.Top + _height / 2);
        }

        public virtual void SetOutput(double output)
        {
        }

        public virtual void SetError(double error)
        {
        }

        public virtual void SetBias(double bias)
        {
        }

        public virtual void UpdateSettings()
        {

        }
    }
}
