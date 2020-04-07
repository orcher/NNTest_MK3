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
    class NNVis_ErrorChangeIndicator
    {
        private Canvas _canvas;
        private Ellipse _shape;
        private Line _arrow;
        private TextBlock _text;
        private double _radius;
        private double _value;

        public NNVis_ErrorChangeIndicator(Canvas canvas, double radius)
        {
            _canvas = canvas;
            _radius = radius;
            _value = 0.0;

            _shape = new Ellipse
            {
                Width = _radius * 2,
                Height = _radius * 2,
                //Fill = new SolidColorBrush(Colors.White),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 2,
                Margin = new Thickness(0, _canvas.ActualHeight - _radius * 2, 0, 0)
            };
            _canvas.Children.Add(_shape);

            _arrow = new Line
            {
                X1 = 0,
                Y1 = 0,
                X2 = _radius,
                Y2 = 0,
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 2,
                Margin = new Thickness(_radius, _canvas.ActualHeight - _radius, 0, 0)
            };
            _canvas.Children.Add(_arrow);

            _text = new TextBlock
            {
                //Text = $"{_value:F}",
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, _canvas.ActualHeight - _radius * 2 - 17, 0, 0)
            };
            _canvas.Children.Add(_text);
        }

        public void Refresh()
        {
            _shape.Margin = new Thickness(0, _canvas.ActualHeight - _radius * 2, 0, 0);
            _arrow.Margin = new Thickness(_radius, _canvas.ActualHeight - _radius, 0, 0);
            _text.Margin = new Thickness(0, _canvas.ActualHeight - _radius * 2 - 17, 0, 0);
        }

        public void Update(double value)
        {
            _value = value;
            //_text.Text = $"{(int) _value}";
            _arrow.RenderTransform = new RotateTransform(_value, _arrow.X1, _arrow.Y1);

            var b = (byte) (383 - ((Math.Abs(value / 180)) * 255));
            if (value > 0)
            {
                _shape.Stroke = new SolidColorBrush(Color.FromRgb(b, 255, b));
                _arrow.Stroke = new SolidColorBrush(Color.FromRgb(b, 255, b));
            }
            else
            {
                _shape.Stroke = new SolidColorBrush(Color.FromRgb(255, b, b));
                _arrow.Stroke = new SolidColorBrush(Color.FromRgb(255, b, b));
            }
        }
    }
}
