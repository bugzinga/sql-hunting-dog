using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HuntingDog.Config;
using Xceed.Wpf.Toolkit;

namespace HuntingDog.DogFace
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();

        }

        public void ShowConfiguration(DogConfig cfg)
        {
            DogConfig = cfg.CloneMe();
            _propertyGrid.SelectedObject = DogConfig;
            _propertyGrid.ShowSearchBox = false;
            _propertyGrid.ShowSortOptions = false;
            _propertyGrid.ShowTitle = false;
        }

        public DogConfig DogConfig
        {
            get;
            private set;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
