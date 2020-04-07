using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NNTest_MK3
{
    /// <summary>
    /// Interaction logic for Plot.xaml
    /// </summary>
    public partial class Plot : UserControl
    {
        private Dictionary<string, Polyline> _lines;
        private Dictionary<string, PointCollection> _linesData;
        protected Dictionary<string, double> _accumulatedNormalizedValue;
        protected Dictionary<string, bool> _directionIndicator;
        private double _minX, _maxX, _minY, _maxY;
        protected double _maxPoints;
        public bool ScaleXAxis { get; set; }
        public int MaxPointsCount { get; set; }
        public double LineThickness { get; set; }

        public Plot()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            LineThickness = 1;
            ScaleXAxis = true;

            _lines = new Dictionary<string, Polyline>();
            _linesData = new Dictionary<string, PointCollection>();
            _accumulatedNormalizedValue = new Dictionary<string, double>();
            _directionIndicator = new Dictionary<string, bool>();
            _minX = double.MaxValue;
            _maxX = double.MinValue;
            _minY = double.MaxValue;
            _maxY = double.MinValue;
            _maxPoints = int.MinValue;
        }

        public virtual void Add(string name, PointCollection pc, Color color)
        {
            if (_linesData.ContainsKey(name))
            {
                _linesData[name] = pc;
                UpdateMinMax(pc);
                Refresh();
                return;
            }

            _linesData.Add(name, pc);

            var line = new Polyline
            {
                StrokeThickness = LineThickness,
                Stroke = new SolidColorBrush(color),
                Points = _linesData[name]
            };

            _lines.Add(name, line);
            Canvas.Children.Add(line);

            _accumulatedNormalizedValue.Add(name, 0.0);
            _directionIndicator.Add(name, false);

            UpdateMinMax(pc);
            Refresh();
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Refresh();
        }

        protected virtual void Refresh()
        {
            foreach (var pair in _linesData)
            {
                var line = pair.Value;

                if (line.Count < 2) continue;

                var pc = new PointCollection();
                _accumulatedNormalizedValue[pair.Key] = 0.0;
                for(var i = 0; i < line.Count; i++)
                {
                    var ratioX = Canvas.ActualWidth / (_maxX - _minX);
                    var ratioY = Canvas.ActualHeight / (_maxY - _minY);
                    var x = ScaleXAxis ? (line[i].X - _minX) * ratioX : line[i].X * (Canvas.ActualWidth / MaxPointsCount);
                    var y = (line[i].Y - _minY) * ratioY;
                    if (i - 1 < line.Count)
                    {
                        _accumulatedNormalizedValue[pair.Key] += y* ratioX;
                    }

                    pc.Add(new Point(x, Canvas.ActualHeight - y));
                }

                if (pc.Count > 1) _directionIndicator[pair.Key] = pc[pc.Count - 2].Y < pc[pc.Count - 1].Y;
                _lines[pair.Key].Points = pc;
            }
        }

        private void UpdateMinMax(PointCollection pc)
        {
            _minX = double.MaxValue;
            _maxX = double.MinValue;
            _minY = double.MaxValue;
            _maxY = double.MinValue;
            _maxPoints = int.MinValue;

            foreach (var lineData in _linesData)
            {
                foreach (var p in lineData.Value)
                {
                    _minX = Math.Min(_minX, p.X);
                    _maxX = Math.Max(_maxX, p.X);
                    _minY = Math.Min(_minY, p.Y);
                    _maxY = Math.Max(_maxY, p.Y);
                }
                _maxPoints = Math.Max(_maxPoints, lineData.Value.Count);
            }
        }

        public virtual void Clear()
        {
            Canvas.Children.Clear();
            _lines.Clear();
            _linesData.Clear();
            _accumulatedNormalizedValue.Clear();
            _directionIndicator.Clear();
        }
    }
}
