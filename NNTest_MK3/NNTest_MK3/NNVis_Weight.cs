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
    class NNVis_Weight
    {
        protected Canvas _canvas;
        protected Line _shape;
        protected TextBlock _weightText;
        protected double _value;
        protected double _maxWeightThikness = 5.0;

        public NNVis_Weight(Canvas canvas, Point positionStart, Point positionEnd)
        {
            _canvas = canvas;

            _shape = new Line
            {
                X1 = positionStart.X,
                Y1 = positionStart.Y,
                X2 = positionEnd.X,
                Y2 = positionEnd.Y,
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Colors.Black),
            };
            _canvas.Children.Add(_shape);

            _weightText = new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.White),
                Background = new SolidColorBrush(Colors.Transparent),
                Text = $"{_value:F}",
                TextAlignment = TextAlignment.Center,
            };
            _weightText.Margin = new Thickness(
                _shape.X1 + (_shape.X2 - _shape.X1) / 2 - (_weightText.Text.Length * _weightText.FontSize) / 4,
                _shape.Y1 + (_shape.Y2 - _shape.Y1) / 2 - _weightText.FontSize / 2, 0, 0);
            _canvas.Children.Add(_weightText);

            SetValue(1.00);
        }

        public void SetValue(double value)
        {
            _value = value;
            _weightText.Text = $"{_value:F}";
            var cv = _value * 255;
            //var tv = cv < 128 ? Colors.White : Colors.Black;
            _shape.Fill = new SolidColorBrush(Color.FromRgb((byte) cv, (byte) cv, (byte) cv));
            _shape.StrokeThickness = Math.Min(Math.Abs(_value), _maxWeightThikness);
            //_weightText.Foreground = NeuronSettings.ShowWeights
            //    ? new SolidColorBrush(tv)
            //    : new SolidColorBrush(Colors.Transparent);
            _weightText.Margin = new Thickness(
                _shape.X1 + 3 * (_shape.X2 - _shape.X1) / 4 - (_weightText.Text.Length * _weightText.FontSize) / 4,
                _shape.Y1 + 3 * (_shape.Y2 - _shape.Y1) / 4 - _weightText.FontSize / 2, 0, 0);
        }

        public void UpdateSettings()
        {
            //if (NeuronSettings.ShowWeights) SetValue(_value);
            //else _weightText.Foreground = new SolidColorBrush(Colors.Transparent);

            _weightText.Foreground = new SolidColorBrush(NeuronSettings.ShowWeights ? Colors.White : Colors.Transparent);
        }
    }
}
