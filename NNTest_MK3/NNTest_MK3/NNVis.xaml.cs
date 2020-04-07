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
    /// Interaction logic for NNVis.xaml
    /// </summary>
    public partial class NNVis : UserControl
    {
        public enum NodeType
        {
            Input,
            Neuron
        }

        private List<List<NNVis_Node>> _nodes;
        private List<List<List<NNVis_Weight>>> _weights;
        private PointCollection _error;

        public NNVis()
        {
            InitializeComponent();
        }

        public void Initialize(FFNeuralNetwork nn)
        {
            InitializeNNVis(nn);
            InitializeNNErrorVis();
        }

        private void InitializeNNVis(FFNeuralNetwork nn)
        {
            Canvas.Children.Clear();

            _nodes = new List<List<NNVis_Node>>();

            var inputs = new List<NNVis_Node>();
            for (var i = 0; i < nn.InputLayerSize; i++)
                inputs.Add(new NNVis_Input(Canvas, new Point(10 + NNVisSettings.NodeHSpace, 10 + 50 + i * NNVisSettings.NodeVSpace)));
            _nodes.Add(inputs);

            _weights = new List<List<List<NNVis_Weight>>>();
            for (var l = 0; l < nn.Network.Count; l++)
            {
                var layer = new List<NNVis_Node>();
                var weightstmp = new List<List<NNVis_Weight>>();
                for (var n = 0; n < nn.Network[l].Count; n++)
                {
                    var node = new NNVis_Neuron(Canvas,
                        new Point((_nodes.Count + 1) * NNVisSettings.NodeHSpace, 50 + n * NNVisSettings.NodeVSpace));
                    node.SetOutput(nn.Network[l][n].Output);
                    node.SetBias(nn.Network[l][n].Bias);
                    node.SetError(nn.Network[l][n].Error);
                    layer.Add(node);

                    var weightstmp2 = new List<NNVis_Weight>();
                    for (var w = 0; w < _nodes[l].Count; w++)
                    {
                        var weight = new NNVis_Weight(Canvas, _nodes[l][w].GetWeightPosition(true),
                            layer[n].GetWeightPosition(false));
                        weight.SetValue(nn.Network[l][n].Weights[w]);
                        weightstmp2.Add(weight);
                    }

                    weightstmp.Add(weightstmp2);
                }

                _nodes.Add(layer);

                _weights.Add(weightstmp);
            }

            RescaleCanvas();
            UpdateSettings();
        }

        private void InitializeNNErrorVis()
        {
            Plot.Clear();
            Plot.ScaleXAxis = true;
            Plot.MaxPointsCount = NNVisSettings.MaxErrorPoints;
            Plot.LineThickness = 2.5;

            _error = new PointCollection();
            ErrorLabel.Content = "";
            IterationLabel.Content = "";
        }

        private void RescaleCanvas()
        {
            var NNHeight = 0;
            foreach (var t in _nodes) NNHeight = Math.Max(NNHeight, t.Count * NNVisSettings.NodeVSpace);
            var NNWidth = (_nodes.Count + 1) * NNVisSettings.NodeHSpace;
            Canvas.Height = NNHeight;
            Canvas.Width = NNWidth;
        }

        public void Update(FFNeuralNetwork nn)
        {
            if (NNVisSettings.EnableNNVis) UpdateNNVis(nn);
            if (NNVisSettings.EnableNNErrorVis) UpdateNNErrorVis(nn);
        }

        private void UpdateNNVis(FFNeuralNetwork nn)
        {
            for (var l = 0; l < nn.Network.Count; l++)
            {
                for (var n = 0; n < nn.Network[l].Count; n++)
                {
                    _nodes[l + 1][n].SetOutput(nn.Network[l][n].Output);
                    _nodes[l + 1][n].SetError(nn.Network[l][n].Error);
                    _nodes[l + 1][n].SetBias(nn.Network[l][n].Bias);
                    for (var w = 0; w < nn.Network[l][n].Weights.Count; w++)
                        _weights[l][n][w].SetValue(nn.Network[l][n].Weights[w]);
                }
            }
        }

        private void UpdateNNErrorVis(FFNeuralNetwork nn)
        {
            _error.Add(new Point(_error.Count, nn.Error));
            if (_error.Count > NNVisSettings.MaxErrorPoints)
            {
                _error.RemoveAt(0);
                var pc = new PointCollection();
                for (var x = 0; x < _error.Count; x++) pc.Add(new Point(x, _error[x].Y));
                _error = pc;
            }

            Plot.Add("Error", _error, Colors.White);

            IterationLabel.Content = $"I: {nn.Iteration}";
            ErrorLabel.Content = $"E: {nn.Error:0.#######}";
        }

        public void UpdateSettings()
        {
            for (var l = 1; l < _nodes.Count; l++)
            {
                for (var n = 0; n < _nodes[l].Count; n++)
                {
                    _nodes[l][n].UpdateSettings();
                    for (var w = 0; w < _weights[l - 1][n].Count; w++)
                        _weights[l - 1][n][w].UpdateSettings();
                }
            }

            Plot.MaxPointsCount = NNVisSettings.MaxErrorPoints;
        }
    }
}
