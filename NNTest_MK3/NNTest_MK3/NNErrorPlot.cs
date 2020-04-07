using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NNTest_MK3
{
    class NNErrorPlot : Plot
    {
        private NNVis_ErrorChangeIndicator _errorIndicator;

        public NNErrorPlot() : base()
        {
            InitializeErrorChangeIndicator();
        }

        private void InitializeErrorChangeIndicator()
        {
            _errorIndicator = new NNVis_ErrorChangeIndicator(Canvas, 10);
        }

        public override void Add(string name, PointCollection pc, Color color)
        {
            base.Add(name, pc, color);

            _errorIndicator.Update(GetErrorAcceleration());
        }

        public override void Clear()
        {
            base.Clear();

            InitializeErrorChangeIndicator();
        }

        private double GetErrorAcceleration()
        {
            var canvasSpace = Canvas.ActualWidth * Canvas.ActualHeight;
            canvasSpace *= _maxPoints / MaxPointsCount;
            var accRatio = ((_accumulatedNormalizedValue["Error"]) - canvasSpace / 2) / canvasSpace;
            var value = accRatio * 2 * 180;
            return value;
        }

        protected override void Refresh()
        {
            base.Refresh();

            _errorIndicator.Refresh();
        }
    }
}
