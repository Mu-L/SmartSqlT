using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SmartSQL.Views
{
    public partial class DownloadView
    {
        public DownloadView()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            //����Grid
            Grid grid = this.Owner.Content as Grid;
            //��������ԭ��������
            UIElement original = VisualTreeHelper.GetChild(grid, 0) as UIElement;
            //����������ԭ��������������Grid���Ƴ�
            grid.Children.Remove(original);
            //������������
            this.Owner.Content = original;
            //var mainWindow = (GenCode)Window.GetWindow(this);
            this.Close();
        }
    }
}
