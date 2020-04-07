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
    class NNVis_Neuron : NNVis_Node
    {
        protected TextBlock _outputText;
        protected TextBlock _biasText;
        protected TextBlock _errorText;
        protected double _output;
        protected double _bias;
        protected double _error;

        public NNVis_Neuron(Canvas canvas, Point position) : base(canvas, position)
        {
            _outputText = new TextBlock
            {
                Width = _width,
                Height = _height,
                Padding = new Thickness(0, _height / 4, 0, 0),
                Foreground = new SolidColorBrush(Colors.Black),
                Background = new SolidColorBrush(Colors.Transparent),
                Text = $"{_output:F}",
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.UltraBold,
                Margin = new Thickness(position.X - _width / 2, position.Y - _height / 2, 0, 0)
            };
            _canvas.Children.Add(_outputText);

            _errorText = new TextBlock
            {
                Padding = new Thickness(0, _height / 4, 0, 0),
                Foreground = new SolidColorBrush(Colors.White),
                Background = new SolidColorBrush(Colors.Transparent),
                Text = $"E: {_error:F}",
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(position.X - _width / 2, position.Y - _height - 5, 0, 0)
            };
            _canvas.Children.Add(_errorText);

            _biasText = new TextBlock
            {
                Padding = new Thickness(0, _height / 4, 0, 0),
                Foreground = new SolidColorBrush(Colors.White),
                Background = new SolidColorBrush(Colors.Transparent),
                Text = $"B: {_bias:F}",
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(position.X - _width / 2, position.Y - _height - 20, 0, 0)
            };
            _canvas.Children.Add(_biasText);

            SetOutput(0.00);
        }

        public override void SetOutput(double output)
        {
            _output = output;
            _outputText.Text = $"{_output:F}";
            var cv = _output * 255;
            var tv = cv < 128 ? Colors.White : Colors.Black;
            _shape.Fill = new SolidColorBrush(Color.FromRgb((byte) cv, (byte) cv, (byte) cv));
            _outputText.Foreground = NeuronSettings.ShowOutput
                ? new SolidColorBrush(tv)
                : new SolidColorBrush(Colors.Transparent);
        }

        public override void SetError(double error)
        {
            _error = error;
            _errorText.Text = $"E: {_error:F}";
        }

        public override void SetBias(double bias)
        {
            _bias = bias;
            _biasText.Text = $"B: {_bias:F}";
        }

        public override void UpdateSettings()
        {
            if (NeuronSettings.ShowOutput) SetOutput(_output);
            else _outputText.Foreground = new SolidColorBrush(Colors.Transparent);

            _biasText.Foreground = new SolidColorBrush(NeuronSettings.ShowBias ? Colors.White : Colors.Transparent);
            _errorText.Foreground = new SolidColorBrush(NeuronSettings.ShowError ? Colors.White : Colors.Transparent);
        }
    }
}
