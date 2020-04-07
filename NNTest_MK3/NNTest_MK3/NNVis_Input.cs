using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NNTest_MK3
{
    class NNVis_Input : NNVis_Node
    {
        public NNVis_Input(Canvas canvas, Point position) : base(canvas, position)
        {
            _width = 20;
            _height = 20;
            _shape.Width = _width;
            _shape.Height = _height;
        }
    }
}
