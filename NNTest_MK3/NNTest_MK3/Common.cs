using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NNTest_MK3
{
    static class Common
    {
        public static JObject GetJsonFromFile()
        {
            string data = null;
            var openFileDialog = new OpenFileDialog { Filter = "Text files (*.json)|*.json" };
            if (openFileDialog.ShowDialog() == true)
                data = File.ReadAllText(openFileDialog.FileName);

            if (data == null) return null;

            return JsonConvert.DeserializeObject(data) as JObject;
        }

        public static void SaveJsonToFile(JToken json)
        {
            if (json == null) return;
            var saveFileDialog = new SaveFileDialog { Filter = "Text files (*.json)|*.json" };
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(json));
        }

        public static void VaidateOnlyDigits(TextBox tb)
        {
            if (!tb.Text.All(char.IsDigit))
                tb.Text = new string(tb.Text.Where(char.IsDigit).ToArray());
        }

        public static void ValidatePositiveDouble(TextBox tb)
        {
            try
            {
                tb.Text = $"{Math.Abs(Convert.ToDouble(tb.Text)):0.########}";
            }
            catch (Exception)
            {
                tb.Text = "";
            }
        }

        public static void ValidatePositiveInt(TextBox tb)
        {
            try
            {
                tb.Text = $"{Math.Abs(Convert.ToInt16(tb.Text))}";
            }
            catch (Exception)
            {
                tb.Text = "";
            }
        }
    }
}
