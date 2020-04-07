using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace NNTest_MK3
{
    public class FFNeuralNetwork
    {
        public List<List<Neuron>> Network { get; set; }
        public int InputLayerSize { get; set; }
        public double Error { get; set; }
        public int Iteration { get; set; }
        private readonly Random _rnd;

        public FFNeuralNetwork(Random rnd)
        {
            _rnd = rnd;
        }

        public bool Initialize(JObject jnn)
        {
            Network = new List<List<Neuron>>();

            try
            {
                InputLayerSize = jnn["InputLayerSize"].Value<int>();
                foreach (var l in jnn["Hidden"])
                {
                    var layer = new List<Neuron>();
                    foreach (var n in l)
                    {
                        var neuron = new Neuron(Network.Count == 0 ? InputLayerSize : Network[Network.Count - 1].Count,
                            (NeuronType)n["NeuronType"].Value<int>(), _rnd);

                        var weightIndex = 0;
                        foreach (var w in n["Weights"])
                            neuron.Weights[weightIndex++] = w.Value<double>();

                        neuron.Bias = n["Bias"].Value<double>();

                        layer.Add(neuron);
                    }

                    Network.Add(layer);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to initialize Neural Network!", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }

            return true;
        }

        public void Initialize(List<Tuple<int, NeuronType>> layout)
        {
            InputLayerSize = layout[0].Item2 == NeuronType.INPUT ? layout[0].Item1 : 0;

            Network = new List<List<Neuron>>();
            for (var l = 1; l < layout.Count; l++)
            {
                var layer = new List<Neuron>();
                for (var i = 0; i < layout[l].Item1; i++)
                    layer.Add(
                        new Neuron(Network.Count == 0 ? layout[0].Item1 : Network[Network.Count - 1].Count,
                            layout[l].Item2, _rnd));
                Network.Add(layer);
            }
        }

        public List<double> GenerateOutput(List<double> inputs)
        {
            var outputs = new List<double>();
            for (var leyer = 0; leyer < Network.Count; leyer++)
            {
                for (var neuron = 0; neuron < Network[leyer].Count; neuron++)
                    Network[leyer][neuron].Fire(leyer == 0 ? inputs : outputs);

                outputs.Clear();

                for (var neuron = 0; neuron < Network[leyer].Count; neuron++)
                    outputs.Add(Network[leyer][neuron].GetOutput());
            }

            return outputs;
        }

        public void Train(Dictionary<string, List<List<double>>> trainingData)
        {
            Error = 0.0;
            var outputs = new List<List<double>>();
            for (var i = 0; i < trainingData["Inputs"].Count; i++)
            {
                var output = GenerateOutput(trainingData["Inputs"][i]);
                outputs.Add(output);
                for (var j = 0; j < trainingData["Outputs"][i].Count; j++)
                    Error += Math.Pow(trainingData["Outputs"][i][j] - output[j], 2) / 2;

                for (var neuronIndex = 0; neuronIndex < Network[Network.Count - 1].Count; neuronIndex++)
                    Network[Network.Count - 1][neuronIndex].Error =
                        (output[neuronIndex] - trainingData["Outputs"][i][neuronIndex]);

                CalculateGradients();
                UpdateWeights();
            }

            Iteration++;
        }

        private void CalculateGradients()
        {
            for (var layerIndex = Network.Count - 1; layerIndex >= 0; layerIndex--)
            {
                for (var neuronIndex = 0; neuronIndex < Network[layerIndex].Count; neuronIndex++)
                {
                    var neuron = Network[layerIndex][neuronIndex];
                    if (layerIndex == Network.Count - 1)
                    {
                        var Ne_No = neuron.Error;
                        var No_Na = neuron.ActivationFunctionPrim();
                        for (var inputIndex = 0; inputIndex < neuron.Inputs.Count; inputIndex++)
                        {
                            var Na_Nw = neuron.Inputs[inputIndex];
                            neuron.Gradients[inputIndex] = Ne_No * No_Na * Na_Nw;
                        }

                        neuron.Bias -= Ne_No * No_Na;
                    }
                    else
                    {
                        var Ne_No = 0.0;
                        for (var i = 0; i < Network[layerIndex + 1].Count; i++)
                            Ne_No += Network[layerIndex + 1][i].Gradients[neuronIndex] *
                                     Network[layerIndex + 1][i].Weights[neuronIndex];
                        neuron.Error = Ne_No;
                        var No_Na = neuron.ActivationFunctionPrim();
                        for (var inputIndex = 0; inputIndex < neuron.Inputs.Count; inputIndex++)
                        {
                            var Na_Nw = neuron.Inputs[inputIndex];
                            neuron.Gradients[inputIndex] = Ne_No * No_Na * Na_Nw;
                        }

                        neuron.Bias -= Ne_No * No_Na;
                    }
                }
            }
        }

        private void UpdateWeights()
        {
            foreach (var leyer in Network)
                foreach (var neuron in leyer)
                    neuron.UpdateWeight();
        }

        public JToken GetJson()
        {
            var jnn = new JObject { { "InputLayerSize", InputLayerSize } };

            var jhidden = new JArray();
            foreach (var l in Network)
            {
                var layer = new JArray();
                foreach (var n in l)
                {
                    var neuron = new JObject { { "NeuronType", (int)n.Type } };

                    var weights = new JArray();
                    foreach (var w in n.Weights)
                        weights.Add(w);
                    neuron.Add("Weights", weights);

                    neuron.Add("Bias", n.Bias);

                    layer.Add(neuron);
                }
                jhidden.Add(layer);
            }

            jnn.Add("Hidden", jhidden);

            return jnn;
        }
    }
}
