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
using System.Windows.Shapes;

namespace NNTest_MK3
{
    /// <summary>
    /// Interaction logic for NewNN.xaml
    /// </summary>
    public partial class NewNN : Window
    {
        private class LayerInfo
        {
            public int LayerId { get; set; }
            public string LayerType { get; set; }
            public int NodeCount { get; set; }
            public NeuronType NodeType { get; set; }
        }

        private List<Tuple<int, NeuronType>> _resultLayout;

        private List<LayerInfo> _layoutData;

        public NewNN()
        {
            InitializeComponent();

            HiddenLayersCount_tb.Text = "1";
        }

        private void Ok_btn_Click(object sender, RoutedEventArgs e)
        {
            BuildResultData();

            DialogResult = true;
        }

        private void Cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BuildResultData()
        {
            _resultLayout = new List<Tuple<int, NeuronType>>();
            foreach (LayerInfo row in LayersInfo_dg.ItemsSource)
                _resultLayout.Add(new Tuple<int, NeuronType>(row.NodeCount, row.NodeType));
        }

        private void HiddenLayersCount_tb_LostFocus(object sender, RoutedEventArgs e)
        {
            Common.VaidateOnlyDigits(sender as TextBox);

            if (HiddenLayersCount_tb.Text.Length == 0) HiddenLayersCount_tb.Text = "1";
        }

        private void SetUpColumns()
        {
            foreach (var column in LayersInfo_dg.Columns)
                column.Width = 0.90 * (LayersInfo_dg.ActualWidth / LayersInfo_dg.Columns.Count);

            LayersInfo_dg.Columns[0].IsReadOnly = true;
        }

        private void PopulateData()
        {
            _layoutData = new List<LayerInfo>
            {
                new LayerInfo
                {
                    LayerId = 0,
                    LayerType = "Input",
                    NodeCount = 1,
                    NodeType = NeuronType.INPUT
                }
            };
            for (var i = 0; i < Convert.ToInt16(HiddenLayersCount_tb.Text); i++)
                _layoutData.Add(new LayerInfo
                {
                    LayerId = i + 1,
                    LayerType = "Hidden",
                    NodeCount = 1,
                    NodeType = NeuronType.SUGMOID
                });

            _layoutData.Add(new LayerInfo
            {
                LayerId = _layoutData.Count,
                LayerType = "Output",
                NodeCount = 1,
                NodeType = NeuronType.SUGMOID
            });

            LayersInfo_dg.ItemsSource = _layoutData;
        }

        public List<Tuple<int, NeuronType>> GetLayout()
        {
            return _resultLayout;
        }

        private void Apply_btn_Click(object sender, RoutedEventArgs e)
        {
            PopulateData();

            SetUpColumns();

            Ok_btn.IsEnabled = true;
        }
    }
}
