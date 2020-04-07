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
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace NNTest_MK3
{
    /// <summary>
    /// Interaction logic for CreateDataSet.xaml
    /// </summary>
    public partial class CreateDataSet : Window
    {
        private MongoClient _mongo;
        private IMongoDatabase _db;
        private IMongoCollection<StockData> _collection;
        private bool _createTestDataSet;
        private long _collectionSamplesCount;

        public CreateDataSet(bool testing = false)
        {
            InitializeComponent();

            _createTestDataSet = testing;

            _mongo = null;
            _db = null;
            _collection = null;

            _collectionSamplesCount = 0;

            Initialize();
        }

        private async Task Initialize()
        {
            _mongo = new MongoClient("mongodb://localhost:27017");
            _db = _mongo.GetDatabase("traiding_data");
            var collections = new List<string>();
            using (var cursor = await _db.ListCollectionNamesAsync())
            {
                await cursor.ForEachAsync(d => collections.Add(d.ToString()));
            }

            Collection_cb.ItemsSource = collections;
            Collection_cb.SelectedIndex = 0;
        }

        private void Collection_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_db == null) return;

            _collection = _db.GetCollection<StockData>(((ComboBox) sender).SelectedValue.ToString());

            _collectionSamplesCount = _collection.EstimatedDocumentCount();
            CollectionSamplesCount_l.Content = _collectionSamplesCount;

            InputSize_tb.Text = "10";
            OutputSize_tb.Text = "10";
            OutputOffset_tb.Text = "0";
            SamplesCount_tb.Text = _collectionSamplesCount.ToString();
            SamplesOffset_tb.Text = "0";
        }

        private void SamplesCount_tb_LostFocus(object sender, RoutedEventArgs e)
        {
            Common.ValidatePositiveInt((TextBox) sender);
        }

        private void Offset_tb_LostFocus(object sender, RoutedEventArgs e)
        {
            Common.ValidatePositiveInt((TextBox) sender);
        }

        private void InputSize_tb_LostFocus(object sender, RoutedEventArgs e)
        {
            Common.ValidatePositiveInt((TextBox) sender);
        }

        private void OutputSize_tb_LostFocus(object sender, RoutedEventArgs e)
        {
            Common.ValidatePositiveInt((TextBox) sender);
        }

        private void OutputOffset_tb_LostFocus(object sender, RoutedEventArgs e)
        {
            Common.ValidatePositiveInt((TextBox) sender);
        }

        private void Ok_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate())
            {
                e.Handled = true;
                return;
            }

            if (_createTestDataSet) CreateTestingDataSet();
            else CreateTrainingDataSet();

            DialogResult = true;
        }

        private void Cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private bool Validate()
        {
            try
            {
                var inputSize = Convert.ToInt16(InputSize_tb.Text);
                var outputSize = Convert.ToInt16(OutputSize_tb.Text);
                var outputOffset = Convert.ToInt16(OutputOffset_tb.Text);
                var samplesCount = Convert.ToInt16(SamplesCount_tb.Text);
                var samplesOffset = Convert.ToInt16(SamplesOffset_tb.Text);

                if (outputOffset > inputSize)
                {
                    MessageBox.Show("Output offset can't be more than input size.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                if (samplesCount + samplesOffset > _collectionSamplesCount)
                {
                    MessageBox.Show(
                        "Sum of sample count and sample offset can't be more then entire collection sample count.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void CreateTestingDataSet()
        {
            var inputSize = Convert.ToInt16(InputSize_tb.Text);
            var outputSize = Convert.ToInt16(OutputSize_tb.Text);
            var outputOffset = Convert.ToInt16(OutputOffset_tb.Text);
            var samplesCount = Convert.ToInt16(SamplesCount_tb.Text);
            var samplesOffset = Convert.ToInt16(SamplesOffset_tb.Text);

            var meta = new JObject
            {
                {"Source", "MongoDB"},
                {"DataBase", "traiding_data"},
                {"Collection", Collection_cb.SelectedValue.ToString()},
                {"InputSize", inputSize},
                {"OutputSize", outputSize},
                {"OutputOffset", outputOffset},
                {"SampleCount", samplesCount},
                {"SampleOffset", samplesOffset},
                {"DataSetType", "Testing"}
            };

            var timePoints = (from c in _collection.AsQueryable() orderby c.Date descending select c)
                .Skip(samplesOffset).Take(samplesCount).ToList();

            var jObj = new JObject { { "Meta", meta }, { "Inputs", new JArray() } };
            var inputArray = (jObj["Inputs"] as JArray);
            var tmp = new JArray();
            foreach (var timePoint in timePoints)
            {
                var priceNormalized = timePoint.Close - timePoint.Low;

                tmp.Add(priceNormalized);

                if (tmp.Count != inputSize) continue;

                inputArray?.Add(new JArray(tmp));
                tmp.Clear();
            }

            tmp.Clear();

            if (MessageBox.Show(
                $"Testing data set created:\nInputs: {inputArray?.Count}\nSave to file?",
                "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) Common.SaveJsonToFile(jObj);
        }

        private void CreateTrainingDataSet()
        {
            var inputSize = Convert.ToInt16(InputSize_tb.Text);
            var outputSize = Convert.ToInt16(OutputSize_tb.Text);
            var outputOffset = Convert.ToInt16(OutputOffset_tb.Text);
            var samplesCount = Convert.ToInt16(SamplesCount_tb.Text);
            var samplesOffset = Convert.ToInt16(SamplesOffset_tb.Text);

            var meta = new JObject
            {
                {"Source", "MongoDB"},
                {"DataBase", "traiding_data"},
                {"Collection", Collection_cb.SelectedValue.ToString()},
                {"InputSize", inputSize},
                {"OutputSize", outputSize},
                {"OutputOffset", outputOffset},
                {"SampleCount", samplesCount},
                {"SampleOffset", samplesOffset},
                {"DataSetType", "Training"}
            };

            var timePoints = (from c in _collection.AsQueryable() orderby c.Date descending select c)
                .Skip(samplesOffset).Take(samplesCount).ToList();

            var jObj = new JObject {{"Meta", meta}, {"Inputs", new JArray()}, {"Outputs", new JArray()}};
            var inputArray = (jObj["Inputs"] as JArray);
            var outputArray = (jObj["Outputs"] as JArray);
            var tmp = new JArray();
            foreach (var timePoint in timePoints)
            {
                var priceNormalized = timePoint.Close - timePoint.Low;

                tmp.Add(priceNormalized);

                if (tmp.Count != inputSize) continue;

                inputArray?.Add(new JArray(tmp));
                tmp.Clear();
            }

            tmp.Clear();

            var skipIndex = 0;
            foreach (var timePoint in timePoints)
            {
                if (skipIndex++ < outputOffset) continue;

                var priceNormalized = timePoint.Close - timePoint.Low;

                tmp.Add(priceNormalized);

                if (tmp.Count == outputSize)
                {
                    outputArray?.Add(new JArray(tmp));
                    tmp.Clear();
                    skipIndex = outputSize;
                }

                if (outputArray?.Count == inputArray?.Count) break;
            }

            var removeLastInputSamplesCount = inputArray?.Count - outputArray?.Count;
            for (var i = 0; i < removeLastInputSamplesCount; ++i)
            {
                var lastIndex = inputArray?.Count - 1;
                inputArray?.RemoveAt((int) lastIndex);
            }

            if(MessageBox.Show(
                $"Training data set created:\nInputs: {inputArray?.Count}\nOutputs: {outputArray?.Count}\nSave to file?",
                "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) Common.SaveJsonToFile(jObj);
        }
    }
}
