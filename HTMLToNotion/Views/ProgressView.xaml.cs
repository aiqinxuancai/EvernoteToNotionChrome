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

namespace HTMLToNotion.Views
{
    /// <summary>
    /// ProgressView.xaml 的交互逻辑
    /// </summary>
    public partial class ProgressView : UserControl
    {
        public string Status
        {
            get { return (string)GetValue(StatusProperty); }
            set { 
                
                SetValue(StatusProperty, value); 
            }
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(string), typeof(ProgressView), new PropertyMetadata(string.Empty));



        public ProgressView()
        {
            InitializeComponent();
        }



    }
}
