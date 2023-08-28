using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using HandyControl.Controls;
using HandyControl.Data;
using SmartSQL.Framework;
using SmartSQL.Framework.Exporter;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Annotations;
using SmartSQL.Models;
using SmartSQL.Views;
using TextBox = System.Windows.Controls.TextBox;
using UserControlE = System.Windows.Controls.UserControl;
using MessageBox = HandyControl.Controls.MessageBox;
using DbType = SqlSugar.DbType;
using SmartSQL.DocUtils;
using SqlSugar;
using SmartSQL.Helper;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainObjects.xaml 的交互逻辑
    /// </summary>
    public partial class UcMainObjects : BaseUserControl
    {
        #region Filds
        public static readonly DependencyProperty MenuDataProperty = DependencyProperty.Register(
            "MenuData", typeof(Model), typeof(UcMainObjects), new PropertyMetadata(default(Model)));

        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(TreeNodeItem), typeof(UcMainObjects), new PropertyMetadata(default(TreeNodeItem)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(UcMainObjects), new PropertyMetadata(default(DataBase)));

        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcMainObjects), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty ObjectsViewDataProperty = DependencyProperty.Register(
            "ObjectsViewData", typeof(List<TreeNodeItem>), typeof(UcMainObjects), new PropertyMetadata(default(List<TreeNodeItem>)));

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(UcMainObjects), new PropertyMetadata(default(string)));

        /// <summary>
        /// 菜单数据
        /// </summary>
        public Model MenuData
        {
            get => (Model)GetValue(MenuDataProperty);
            set => SetValue(MenuDataProperty, value);
        }

        /// <summary>
        /// 当前选中对象
        /// </summary>
        public TreeNodeItem SelectedObject
        {
            get => (TreeNodeItem)GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }
        /// <summary>
        /// 当前选中数据库
        /// </summary>
        public DataBase SelectedDataBase
        {
            get => (DataBase)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }
        /// <summary>
        /// 当前数据连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }
        public List<TreeNodeItem> ObjectsViewData
        {
            get => (List<TreeNodeItem>)GetValue(ObjectsViewDataProperty);
            set
            {
                SetValue(ObjectsViewDataProperty, value);
                OnPropertyChanged(nameof(ObjectsViewData));
            }
        }
        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set
            {
                SetValue(PlaceholderProperty, value);
                OnPropertyChanged(nameof(Placeholder));
            }
        }
        #endregion
        public UcMainObjects()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 加载页面数据
        /// </summary>
        public void LoadPageData()
        {
            #region MyRegion
            ChkAll.IsChecked = false;
            NoDataText.Visibility = Visibility.Collapsed;
            if (SelectedObject.Type == ObjType.Type)
            {
                var headerT = "表名称";
                var placeHolderT = "请输入表名称或备注说明";
                switch (SelectedObject.Name)
                {
                    case "treeTable": break;
                    case "treeView":
                        headerT = "视图名称";
                        placeHolderT = "请输入视图名称或备注说明"; break;
                    default:
                        headerT = "存储过程名称";
                        placeHolderT = "请输入存储过程名称或备注说明"; break;
                }
                ObjHead.Header = headerT;
                Placeholder = placeHolderT;
                if (SelectedObject.Parent == null)
                {
                    ObjectsViewData = ObjectsViewData.First(x => x.Name == SelectedObject.Name).Children;
                }
                else
                {
                    ObjectsViewData = ObjectsViewData.First(x => x.DisplayName == SelectedObject.Parent.DisplayName)
                        .Children;
                    ObjectsViewData = ObjectsViewData.First(x => x.Name == SelectedObject.Name).Children;
                }
                if (!ObjectsViewData.Any())
                {
                    NoDataText.Visibility = Visibility.Visible;
                }
                ObjectsViewData.ForEach(x =>
                {
                    x.IsChecked = false;
                });
                ObjItems = ObjectsViewData;
                SearchObject.Text = string.Empty;
            }
            #endregion
        }

        private List<TreeNodeItem> ObjItems;
        /// <summary>
        /// 实时搜索对象
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchObject_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            NoDataText.Visibility = Visibility.Collapsed;
            var searchText = SearchObject.Text.Trim();
            var searchData = ObjItems;
            if (!string.IsNullOrEmpty(searchText))
            {
                var obj = ObjItems.Where(x => x.DisplayName.ToLower().Contains(searchText.ToLower()) || (!string.IsNullOrEmpty(x.Comment) && x.Comment.ToLower().Contains(searchText.ToLower())));
                if (!obj.Any())
                {
                    searchData = new List<TreeNodeItem>();
                }
                else
                {
                    searchData = obj.ToList();
                }
            }
            NoDataText.Visibility = searchData.Any() ? Visibility.Collapsed : Visibility.Visible;
            ObjectsViewData = searchData;
        }

        private void DisplayToolTip_MouseEnter(object sender, MouseEventArgs e)
        {
            var currCell = (DataGridCell)sender;
            if (currCell.Column.SortMemberPath.Equals("DisplayName"))
            {
                var currObject = (TreeNodeItem)currCell.DataContext;
                currCell.ToolTip = currObject.DisplayName;
            }
        }

        private string _cellEditValue = string.Empty;
        private void TableGrid_OnBeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (((DataGrid)sender).SelectedCells.Any())
            {
                //获取选中单元格(仅限单选)
                var selectedCell = ((DataGrid)sender).SelectedCells[0];
                //获取选中单元格数据
                var selectedData = selectedCell.Column.GetCellContent(selectedCell.Item);
                if (selectedData is TextBlock selectedText)
                {
                    _cellEditValue = selectedText.Text;
                }
            }
        }

        /// <summary>
        /// 单元格编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TableGrid_OnPreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.EditingElement != null && e.EditingElement is TextBox)
            {
                var cell = (DataGridCell)e.EditingElement.Parent;
                //设置单元格中的TextBox宽度
                e.EditingElement.Width = cell.ActualWidth - 10;
            }
        }

        private void TableGrid_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            #region MyRegion
            if (e.EditingElement != null && e.EditingElement is TextBox)
            {
                string newValue = (e.EditingElement as TextBox).Text;
                if (!_cellEditValue.Equals(newValue))
                {
                    if (SelectedObject.Type == ObjType.View)
                    {
                        ((TextBox)e.EditingElement).Text = _cellEditValue;
                        return;
                    }
                    var selectItem = (TreeNodeItem)e.Row.Item;
                    var msgResult = MessageBox.Show(
                        $"确认修改{selectItem.DisplayName}的备注为{newValue}？",
                        "温馨提示",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Question);
                    if (msgResult == MessageBoxResult.OK)
                    {
                        var dbConnectionString = SelectedConnection.SelectedDbConnectString(SelectedDataBase.DbName);
                        var dbInstance = ExporterFactory.CreateInstance(SelectedConnection.DbType, dbConnectionString, SelectedDataBase.DbName);
                        try
                        {
                            var objectType = selectItem.Type.Equals(ObjType.Table) ? DbObjectType.Table : (selectItem.Type.Equals(ObjType.View) ? DbObjectType.View : DbObjectType.Proc);
                            var editResult = dbInstance.UpdateObjectRemark(selectItem.Name, newValue, objectType);
                            if (editResult)
                            {
                                Oops.Success("更新成功");
                            }
                            else
                            {
                                Oops.Oh("更新失败");
                            }
                        }
                        catch (Exception ex)
                        {
                            Oops.Oh($"更新失败，原因：" + ex.ToMsg());
                        }
                    }
                    else
                    {
                        ((TextBox)e.EditingElement).Text = _cellEditValue;
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// 导出文档
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExport_OnClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = System.Windows.Window.GetWindow(this);
            var exportDoc = new ExportDoc();
            exportDoc.Owner = mainWindow;
            exportDoc.MenuData = MenuData;
            exportDoc.SelectedConnection = SelectedConnection;
            exportDoc.SelectedDataBase = SelectedDataBase;
            var exportData = ObjectsViewData.Any(x => x.IsChecked == true)
                ? ObjectsViewData.Where(x => x.IsChecked == true).ToList()
                : ObjectsViewData;
            exportDoc.ExportData = exportData;
            exportDoc.ShowDialog();
        }

        /// <summary>
        /// 代码生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnGenCode_OnClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = System.Windows.Window.GetWindow(this);
            var genCode = new GenCode();
            genCode.Owner = mainWindow;
            genCode.MenuData = MenuData;
            genCode.SelectedConnection = SelectedConnection;
            genCode.SelectedDataBase = SelectedDataBase;
            var exportData = ObjectsViewData.Any(x => x.IsChecked == true)
                ? ObjectsViewData.Where(x => x.IsChecked == true).ToList()
                : ObjectsViewData;
            genCode.ExportData = exportData;
            genCode.ShowDialog();
        }

        private void ChkAll_OnClick(object sender, RoutedEventArgs e)
        {
            var isChecked = ((CheckBox)sender).IsChecked;
            var selectedItem = ObjectsViewData;
            selectedItem.ForEach(x =>
            {
                x.IsChecked = isChecked;
            });
            ObjectsViewData = selectedItem;
        }

        private void TableGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                var curItem = (TreeNodeItem)dataGrid.CurrentItem;
                if (curItem == null)
                {
                    return;
                }
                var curCell = dataGrid.CurrentCell.Column.Header;
                if (curCell.Equals("备注说明"))
                {
                    return;
                }
                curItem.IsChecked = !curItem.IsChecked;
            }
        }

        private void TableGrid_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                var curItem = (TreeNodeItem)dataGrid.CurrentItem;
                if (curItem == null)
                {
                    return;
                }
                var curCell = dataGrid.CurrentCell.Column.Header;
                if (curCell.Equals("选择"))
                {
                    curItem.IsChecked = !curItem.IsChecked;
                }
            }
        }
    }
}
