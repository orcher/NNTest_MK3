using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NNTest_MK3
{
    public enum NeuronType
    {
        INPUT,
        SUGMOID,
        LINEAR,
        RELU
    }

    public class Neuron
    {
        public NeuronType Type;
        public List<double> Weights;
        public double Output;
        public List<double> Gradients;
        public List<double> Inputs;
        public double Error;
        public double Bias;

        public Neuron(int inputSize, NeuronType type, Random rnd)
        {
            Weights = new List<double>();
            Gradients = new List<double>();
            Inputs = new List<double>();
            for (var i = 0; i < inputSize; i++)
            {
                Weights.Add(rnd.NextDouble());
                Gradients.Add(0.0);
                Inputs.Add(0.0);
            }
            Output = 0.0;
            Error = 0.0;
            Bias = rnd.NextDouble() * 2 - 1;
            Type = type;
        }

        public double Fire(List<double> inputs)
        {
            Output = 0.0;
            for (var i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];
                Inputs[i] = input;
                Output += input * Weights[i];
            }

            Output = ActivationFunction(Output + Bias);
            return Output;
        }

        public double GetOutput()
        {
            return Output;
        }

        private double ActivationFunction(double x)
        {
            switch (Type)
            {
                case NeuronType.SUGMOID:
                    return 1.0 / (1.0 + Math.Exp(-x));
                case NeuronType.LINEAR:
                    return x;
                case NeuronType.RELU:
                    return x > 0 ? x : 0;
                default:
                    return 1.0 / (1.0 + Math.Exp(-x));
            }
        }

        public double ActivationFunctionPrim()
        {
            switch (Type)
            {
                case NeuronType.SUGMOID:
                    return Output * (1.0 - Output);
                case NeuronType.LINEAR:
                    return 1;
                case NeuronType.RELU:
                    return Output > 0 ? 1 : 0;
                default:
                    return Output * (1.0 - Output);
            }
        }

        public void UpdateWeight()
        {
            for (var i = 0; i < Inputs.Count; i++)
                Weights[i] += NeuronSettings.MobilityFactor * Weights[i] - NeuronSettings.LearningFactor * Gradients[i];
        }
    }
}
