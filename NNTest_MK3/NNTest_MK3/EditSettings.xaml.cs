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
    /// Interaction logic for EditSettings.xaml
    /// </summary>
    public partial class EditSettings : Window
    {
        public EditSettings()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            EditSettings_LearningFactor_tb.Text = $"{NeuronSettings.LearningFactor:0.########}";
            EditSettings_MobilityFactor_tb.Text = $"{NeuronSettings.MobilityFactor:0.########}";
            EnableNNVis_cb.IsChecked = NNVisSettings.EnableNNVis;
            EnableNNErrorVis_cb.IsChecked = NNVisSettings.EnableNNErrorVis;
            MaxNNErrorPoints_tb.Text = $"{NNVisSettings.MaxErrorPoints}";
        }

        private void EditSettings_LearningFactor_tb_LostFocus(object sender, RoutedEventArgs e)
        {
            Common.ValidatePositiveDouble(sender as TextBox);
        }

        private void EditSettings_MobilityFactor_tb_LostFocus(object sender, RoutedEventArgs e)
        {
            Common.ValidatePositiveDouble(sender as TextBox);
        }

        private void EditSettings_OK_btn_Click(object sender, RoutedEventArgs e)
        {
            NeuronSettings.LearningFactor = Convert.ToDouble(EditSettings_LearningFactor_tb.Text);
            NeuronSettings.MobilityFactor = Convert.ToDouble(EditSettings_MobilityFactor_tb.Text);
            NNVisSettings.EnableNNVis = EnableNNVis_cb.IsChecked == true;
            NNVisSettings.EnableNNErrorVis = EnableNNErrorVis_cb.IsChecked == true;
            NNVisSettings.MaxErrorPoints = Convert.ToInt16(MaxNNErrorPoints_tb.Text);

            DialogResult = true;
        }

        private void EditSettings_Cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void MaxNNErrorPoints_tb_LostFocus(object sender, RoutedEventArgs e)
        {
            Common.ValidatePositiveInt(sender as TextBox);
        }
    }
}
