using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using Microsoft.Win32;
using NAudio.Gui;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace NNTest_MK3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FFNeuralNetwork _nn;

        private Dictionary<string, List<List<double>>> _trainingData;
        private JObject _trainingDataMeta;

        private DispatcherTimer _timer;

        private WaveIn _waveIn;

        public MainWindow()
        {
            InitializeComponent();

            Initialize();
        }

        private void XOnDataAvailable(object sender, WaveInEventArgs e)
        {
            var spectrum_cp = new PointCollection();

            var buff = new double[1024];
            for (var i = 0; i < buff.Length; ++i) buff[i] = e.Buffer[i];

            var spectrum = DSP.FourierTransform.Spectrum(ref buff);

            for (var i = 0; i < spectrum.Length; ++i) spectrum_cp.Add(new Point(i, spectrum[i]));

            OutputPlot.Add("Spectrum", spectrum_cp, Colors.White);
        }

        private void Initialize()
        {
            Background = new LinearGradientBrush(Colors.White, Colors.Blue, new Point(-0.5, -0.5), new Point(1, 1));

            InitializeSettings();

            InitializeTimer();

            InitializeNNToolBar();

            InitializeWaveSource();
        }

        private void InitializeWaveSource()
        {
            var dc = WaveIn.DeviceCount;
            _waveIn = new WaveIn {DeviceNumber = 0};
            //_waveIn.WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(0).Channels);
            _waveIn.DataAvailable += XOnDataAvailable;
        }

        private void InitializeSettings()
        {
            NeuronSettings.LearningFactor = 0.35;
            NeuronSettings.MobilityFactor = 0.00001;
            NeuronSettings.ShowBias = false;
            NeuronSettings.ShowError = false;
            NeuronSettings.ShowWeights = false;
            NeuronSettings.ShowOutput = true;

            GlobalSettings.AllowNNErrorVisualization = true;
            GlobalSettings.AllowNNVisualization = true;
            GlobalSettings.NNTrainingDelayMs = 10;

            NNVisSettings.NodeVSpace = 100;
            NNVisSettings.NodeHSpace = 100;
            NNVisSettings.MaxErrorPoints = 300;
            NNVisSettings.EnableNNVis = true;
            NNVisSettings.EnableNNErrorVis = true;
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, GlobalSettings.NNTrainingDelayMs) };
            _timer.Tick += TimerOnTick;
        }

        private void InitializeNNToolBar()
        {
            ShowNeuronOutput_cb.IsChecked = NeuronSettings.ShowOutput;
            ShowNeuronWeights_cb.IsChecked = NeuronSettings.ShowWeights;
            ShowNeuronBias_cb.IsChecked = NeuronSettings.ShowBias;
            ShowNeuronError_cb.IsChecked = NeuronSettings.ShowError;

            switch (GlobalSettings.NNTrainingDelayMs)
            {
                case 0:
                    NN_TrainDelay_cmb.SelectedIndex = 0;
                    break;
                case 10:
                    NN_TrainDelay_cmb.SelectedIndex = 1;
                    break;
                case 100:
                    NN_TrainDelay_cmb.SelectedIndex = 2;
                    break;
                case 500:
                    NN_TrainDelay_cmb.SelectedIndex = 3;
                    break;
                case 1000:
                    NN_TrainDelay_cmb.SelectedIndex = 4;
                    break;
                default:
                    NN_TrainDelay_cmb.SelectedIndex = 1;
                    break;
            }
        }

        private void InitializeNN(List<Tuple<int, NeuronType>> layout)
        {
            _nn = new FFNeuralNetwork(new Random());
            _nn.Initialize(layout);
            NNVis.Initialize(_nn);

            NN_Export_btn.IsEnabled = true;
            NN_toolBar.IsEnabled = true;
        }

        private void InitializeNN(JObject jnn)
        {
            _nn = new FFNeuralNetwork(new Random());
            if (!_nn.Initialize(jnn))
            {
                _nn = null;
                return;
            }

            NNVis.Initialize(_nn);

            NN_Export_btn.IsEnabled = true;
            NN_toolBar.IsEnabled = true;
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            _nn.Train(_trainingData);
            NNVis.Update(_nn);
        }

        private void NN_New_btn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new NewNN();
            if (dlg.ShowDialog() != true) return;
            var layout = dlg.GetLayout();
            InitializeNN(layout);
        }

        private void NN_Import_btn_Click(object sender, RoutedEventArgs e)
        {
            var jnn = Common.GetJsonFromFile();
            if (jnn == null) return;
            InitializeNN(jnn);
        }

        private void NN_Export_btn_Click(object sender, RoutedEventArgs e)
        {
            Common.SaveJsonToFile(_nn.GetJson());
        }

        private void NN_Train_btn_Click(object sender, RoutedEventArgs e)
        {
            if (_nn == null || _trainingData == null) return;

            if(_timer.IsEnabled)
            {
                _timer.Stop();
                NN_Train_btn.Content = "Run";
            }
            else
            {
                _timer.Start();
                NN_Train_btn.Content = "Pause";
            }
        }

        private void ShowNeuronOutput_cb_Click(object sender, RoutedEventArgs e)
        {
            NeuronSettings.ShowOutput = (sender as CheckBox)?.IsChecked == true;
            NNVis.UpdateSettings();
        }

        private void ShowNeuronWeights_cb_Click(object sender, RoutedEventArgs e)
        {
            NeuronSettings.ShowWeights = (sender as CheckBox)?.IsChecked == true;
            NNVis.UpdateSettings();
        }

        private void ShowNeuronBias_cb_Click(object sender, RoutedEventArgs e)
        {
            NeuronSettings.ShowBias = (sender as CheckBox)?.IsChecked == true;
            NNVis.UpdateSettings();
        }

        private void ShowNeuronError_cb_Click(object sender, RoutedEventArgs e)
        {
            NeuronSettings.ShowError = (sender as CheckBox)?.IsChecked == true;
            NNVis.UpdateSettings();
        }

        private void Training_LoadData_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var json = Common.GetJsonFromFile();

                _trainingData = new Dictionary<string, List<List<double>>>();

                var inputs = new List<List<double>>();
                var outputs = new List<List<double>>();

                inputs.AddRange(json["Inputs"].Select(inputSet => inputSet.Select(input => input.Value<double>()).ToList()));
                outputs.AddRange(json["Outputs"].Select(outputSet => outputSet.Select(output => output.Value<double>()).ToList()));

                _trainingData.Add("Inputs", inputs);
                _trainingData.Add("Outputs", outputs);

                _trainingDataMeta = json["Meta"] as JObject;

                NN_Train_btn.IsEnabled = true;
                Print_btn.IsEnabled = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to load training data!", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);

                _trainingData = null;
            }
        }

        private void Print_btn_Click(object sender, RoutedEventArgs e)
        {
            var ret = "Set | Input | Output | Exp. Output" + Environment.NewLine;
            for (var i = 0; i < _trainingData["Inputs"].Count; i++)
            {
                var r = _nn.GenerateOutput(_trainingData["Inputs"][i]);
                var ins = "";
                var outs = "";
                var expOuts = "";
                foreach (var t in _trainingData["Inputs"][i]) ins += $"{t:F} ";
                foreach (var t in r) outs += $"{t:F} ";
                foreach (var t in _trainingData["Outputs"][i]) expOuts += $"{t:F} ";

                ret += $@"{i} | {ins} | {outs} | {expOuts}" + Environment.NewLine;
            }
            Output_tb.Text += ret + Environment.NewLine + Environment.NewLine;
            Output_tb.ScrollToEnd();
            MainTabControl_tc.SelectedItem = Output_Tab_ti;

            //PrintOutputGraph_1stOfEach();
            PrintOutputGraph_AllOfEach();
        }

        private void NN_TrainDelay_cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((sender as ComboBox)?.SelectedIndex)
            {
                case 0:
                    GlobalSettings.NNTrainingDelayMs = 0;
                    break;
                case 1:
                    GlobalSettings.NNTrainingDelayMs = 10;
                    break;
                case 2:
                    GlobalSettings.NNTrainingDelayMs = 100;
                    break;
                case 3:
                    GlobalSettings.NNTrainingDelayMs = 500;
                    break;
                case 4:
                    GlobalSettings.NNTrainingDelayMs = 1000;
                    break;
                default:
                    GlobalSettings.NNTrainingDelayMs = 10;
                    break;
            }
            _timer.Interval = new TimeSpan(0, 0, 0, 0, GlobalSettings.NNTrainingDelayMs);
        }

        private void Settings_btn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new EditSettings();
            dlg.ShowDialog();
            NNVis.UpdateSettings();
        }

        private void PrintOutputGraph_1stOfEach()
        {
            var input = new PointCollection();
            var output = new PointCollection();
            var expected = new PointCollection();
            for (var i = 0; i < _trainingData["Inputs"].Count; i++)
            {
                input.Add(new Point(i, _trainingData["Inputs"][i][0]));
                output.Add(new Point(i, _nn.GenerateOutput(_trainingData["Inputs"][i])[0]));
                expected.Add(new Point(i, _trainingData["Outputs"][i][0]));
            }

            OutputPlot.LineThickness = 2.5;
            OutputPlot.ScaleXAxis = true;
            OutputPlot.Add("Input", input, Colors.Black);
            OutputPlot.Add("Expected", expected, Colors.White);
            OutputPlot.Add("Output", output, Colors.Purple);
        }

        private void PrintOutputGraph_AllOfEach()
        {
            var outputOffset = _trainingDataMeta["OutputOffset"].Value<int>();
            var input = new PointCollection();
            var output = new PointCollection();
            var expected = new PointCollection();
            var sampleCount = _trainingData["Inputs"].Count;
            for (var i = 0; i < sampleCount; i++)
            {
                var inputSize = _trainingData["Inputs"][i].Count;
                var outputSize = _trainingData["Outputs"][i].Count;
                for (var j = 0; j < inputSize; ++j)
                    input.Add(new Point(i * inputSize + j, _trainingData["Inputs"][i][j]));

                for (var j = 0; j < inputSize; ++j)
                    output.Add(new Point(i * inputSize + j + outputOffset,
                        j < outputSize ? _nn.GenerateOutput(_trainingData["Inputs"][i])[j] : 0));

                for (var j = 0; j < inputSize; j++)
                    expected.Add(new Point(i * inputSize + j + outputOffset,
                        j < outputSize ? _trainingData["Outputs"][i][j] : 0));
            }

            OutputPlot.LineThickness = 2.5;
            OutputPlot.ScaleXAxis = true;
            OutputPlot.Add("Input", input, Colors.Black);
            OutputPlot.Add("Expected", expected, Colors.White);
            OutputPlot.Add("Output", output, Colors.Purple);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            _waveIn.StartRecording();
            MainTabControl_tc.SelectedItem = OutputGraph_Tab_ti;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            _waveIn.StopRecording();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            TestNN();
        }

        private void TestNN()
        {
            var testingData = new List<List<double>>();

            var testingDataJson = Common.GetJsonFromFile();
            foreach (JArray testSet in testingDataJson["Inputs"])
            {
                var ts = new List<double>();
                for (var i = 0; i < testSet.Count(); ++i)
                    ts.Add(testSet[i].Value<double>());
                testingData.Add(ts);
            }

            _trainingDataMeta = testingDataJson["Meta"] as JObject;
            var outputOffset = _trainingDataMeta["OutputOffset"].Value<int>();
            var outputSize = _trainingDataMeta["OutputSize"].Value<int>();
            var inputSize = _trainingDataMeta["InputSize"].Value<int>();

            var inputPoints = new PointCollection();
            var outputPoints = new PointCollection();
            for (var i = 0; i < testingData.Count; i++)
            {
                for (var j = 0; j < inputSize; ++j)
                {
                    var input = testingData[i][j];
                    var output = j < outputSize ? _nn.GenerateOutput(testingData[i])[j] : 0;

                    inputPoints.Add(new Point(i * inputSize + j, input));
                    outputPoints.Add(new Point(i * inputSize + j + outputOffset, output));
                }
            }

            OutputPlot.Clear();
            OutputPlot.LineThickness = 2.5;
            OutputPlot.ScaleXAxis = true;
            OutputPlot.Add("Input", inputPoints, Colors.White);
            OutputPlot.Add("Output", outputPoints, Colors.Purple);
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            Mongo_GetLastDate();
        }

        private async Task Mongo_GetLastDate()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            using (var cursor = await client.ListDatabasesAsync())
            {
                await cursor.ForEachAsync(d => Console.WriteLine(d.ToString()));
            }

            var db = client.GetDatabase("traiding_data");
            
            var msft = db.GetCollection<StockData>("MSFT");

            var res = (from c in msft.AsQueryable() orderby c.Date descending select c).Take(1).ToList();

            var last_date = res[0].Date;

            MessageBox.Show($"Last traiding time stamp for MSFT: {last_date:g}");
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            Mongo_UpdateTradeingData();
        }

        private async Task Mongo_UpdateTradeingData()
        {
            var AlphaVentageEndpoint = "https://www.alphavantage.co/query";
            var AlphaVentageApiKey = "PRQYP9H6RW77UULI";
            var Stocks = new List<string> { "MSFT", "AAPL", "IBM", "EBAY", "FB" };

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var traidingData = new Dictionary<string, List<StockData>>();
            foreach (var stock in Stocks)
            {
                traidingData.Add(stock, new List<StockData>());

                try
                {
                    var url = AlphaVentageEndpoint + "?function=TIME_SERIES_INTRADAY&interval=1min&outputsize=full&symbol=" + stock + "&apikey=" + AlphaVentageApiKey;
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var contentString = await response.Content.ReadAsStringAsync();
                        dynamic contentJson = JsonConvert.DeserializeObject(contentString);
                        if (contentString.Contains("Time Series (1min)"))
                        {
                            foreach (var timePoint in contentJson["Time Series (1min)"] as JObject)
                            {
                                var timeStamp = DateTime.Parse(timePoint.Key);
                                var yesterday = DateTime.Today.AddDays(-1);
                                if (timeStamp.Date != yesterday) continue;

                                var value = timePoint.Value;
                                var sd = new StockData
                                {
                                    _id = ObjectId.GenerateNewId(),
                                    Date = DateTime.Parse(timePoint.Key),
                                    Open = value["1. open"].Value<double>(),
                                    Close = value["4. close"].Value<double>(),
                                    High = value["2. high"].Value<double>(),
                                    Low = value["3. low"].Value<double>(),
                                    Volume = value["5. volume"].Value<int>()
                                };
                                traidingData[stock].Add(sd);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine(response.ReasonPhrase);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            try
            {
                var mongo = new MongoClient("mongodb://localhost:27017");
                var db = mongo.GetDatabase("traiding_data");
                var stockCollections = new List<string>();
                using (var cursor = await db.ListCollectionNamesAsync())
                {
                    await cursor.ForEachAsync(d => stockCollections.Add(d.ToString()));
                }

                var summary = "";

                foreach (var stock in Stocks)
                {
                    if(!stockCollections.Contains(stock)) db.CreateCollection(stock);

                    var collection = db.GetCollection<StockData>(stock);
                    var res = (from c in collection.AsQueryable() orderby c.Date descending select c).Take(1).ToList();
                    if (traidingData[stock].Count <= 0 || traidingData[stock][0].Date <= res[0].Date)
                    {
                        summary += $"{stock} no need to update" + Environment.NewLine;
                        continue;
                    }

                    collection.InsertMany(traidingData[stock]);

                    summary += $"{stock} updated - {traidingData[stock].Count} added" + Environment.NewLine;
                }

                MessageBox.Show(summary, "Traiding data update summary");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task ShowStock(string stock)
        {
            try
            {
                var mongo = new MongoClient("mongodb://localhost:27017");
                var db = mongo.GetDatabase("traiding_data");
                var stockCollections = new List<string>();
                using (var cursor = await db.ListCollectionNamesAsync())
                {
                    await cursor.ForEachAsync(d => stockCollections.Add(d.ToString()));
                }


                if (!stockCollections.Contains(stock))
                {
                    MessageBox.Show($"{stock} collection doesn't exist!");
                    return;
                }

                var collection = db.GetCollection<StockData>(stock);
                var res = (from c in collection.AsQueryable() orderby c.Date descending select c).ToList();

                var pc = new PointCollection();
                for(var i = 0; i < res.Count; ++i)
                    pc.Add(new Point(i, res[i].Close - res[i].Low));

                OutputPlot.Clear();
                OutputPlot.Add(stock, pc, Colors.White);
                MainTabControl_tc.SelectedItem = OutputGraph_Tab_ti;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            ShowStock(((MenuItem)sender).Header.ToString());
        }

        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            ShowStock(((MenuItem)sender).Header.ToString());
        }

        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {
            ShowStock(((MenuItem)sender).Header.ToString());
        }

        private void MenuItem_Click_8(object sender, RoutedEventArgs e)
        {
            ShowStock(((MenuItem)sender).Header.ToString());
        }

        private void MenuItem_Click_9(object sender, RoutedEventArgs e)
        {
            ShowStock(((MenuItem)sender).Header.ToString());
        }

        private void MenuItem_Click_10(object sender, RoutedEventArgs e)
        {
            new CreateDataSet().ShowDialog();
        }

        private void MenuItem_Click_11(object sender, RoutedEventArgs e)
        {
            new CreateDataSet(true).ShowDialog();
        }
    }
}
