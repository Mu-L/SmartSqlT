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
using HandyControl.Controls;
using HandyControl.Data;
using SmartSQL.DocUtils;
using SmartSQL.Framework;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Framework.Util;
using SmartSQL.Helper;
using SmartSQL.Views;
using SqlSugar;
using Window = System.Windows.Window;

namespace SmartSQL.UserControl.Connect
{
    /// <summary>
    /// SqlServerUC.xaml 的交互逻辑
    /// </summary>
    public partial class SqlServerUC : System.Windows.Controls.UserControl
    {
        public event ConnectChangeRefreshHandlerExt ChangeRefreshEvent;

        public static readonly DependencyProperty ConnectConfigProperty = DependencyProperty.Register(
            "ConnectConfig", typeof(ConnectConfigs), typeof(SqlServerUC), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 连接配置信息
        /// </summary>
        public ConnectConfigs ConnectConfig
        {
            get => (ConnectConfigs)GetValue(ConnectConfigProperty);
            set => SetValue(ConnectConfigProperty, value);
        }

        public SqlServerUC()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 初始化加载页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SqlServerUC_OnLoaded(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded || ConnectConfig == null)
            {
                return;
            }
            var connect = ConnectConfig;
            var pwd = EncryptHelper.Decode(connect.Password);
            var defaultBase = new List<DataBase> { new DataBase { DbName = connect.DefaultDatabase } };
            HidId.Text = connect.ID.ToString();
            TextConnectName.Text = connect.ConnectName;
            TextServerAddress.Text = connect.ServerAddress;
            TextServerPort.Value = connect.ServerPort;
            TextServerName.Text = connect.UserName;
            ComboAuthentication.SelectedItem = connect.Authentication == 1 ? SqlServer : Windows;
            TextServerPassword.Password = pwd;
            ComboDefaultDatabase.ItemsSource = defaultBase;
            ComboDefaultDatabase.SelectedItem = defaultBase.First();
            #endregion
        }

        /// <summary>
        /// 重置表单
        /// </summary>
        public bool VerifyForm()
        {
            #region MyRegion
            var connectName = TextConnectName.Text.Trim();
            var serverAddress = TextServerAddress.Text.Trim();
            var serverPort = TextServerPort.Value;
            var userName = TextServerName.Text.Trim();
            var password = TextServerPassword.Password.Trim();
            var tipMsg = new StringBuilder();
            if (string.IsNullOrEmpty(connectName))
            {
                tipMsg.Append("- 请填写连接名称" + Environment.NewLine);
            }
            if (string.IsNullOrEmpty(serverAddress))
            {
                tipMsg.Append("- 请填写服务器地址" + Environment.NewLine);
            }
            if (serverPort < 1)
            {
                tipMsg.Append("- 请填写端口号" + Environment.NewLine);
            }
            if (SqlServer.IsSelected)
            {
                if (string.IsNullOrEmpty(userName))
                {
                    tipMsg.Append("- 请填写登录名" + Environment.NewLine);
                }
                if (string.IsNullOrEmpty(password))
                {
                    tipMsg.Append("- 请填写密码");
                }
            }
            if (tipMsg.ToString().Length > 0)
            {
                Growl.Warning(new GrowlInfo { Message = tipMsg.ToString(), WaitTime = 1, ShowDateTime = false });
                return false;
            }
            return true;
            #endregion
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        /// <param name="isTest"></param>
        public void TestConnect(bool isTest)
        {
            #region MyRegion
            if (!VerifyForm())
            {
                return;
            }
            var mainWindow = (ConnectManage)Window.GetWindow(this);
            if (mainWindow == null)
            {
                return;
            }
            mainWindow.LoadingG.Visibility = Visibility.Visible;
            var connectId = Convert.ToInt32(HidId.Text);
            var authentication = ComboAuthentication.SelectedValue == SqlServer ? 1 : 0;
            var password = EncryptHelper.Encode(TextServerPassword.Password.Trim());
            var connectionString = ConnectionStringUtil.SqlServerString(TextServerAddress.Text.Trim(),
                Convert.ToInt32(TextServerPort.Value), authentication, "master", TextServerName.Text.Trim(), password);
            Task.Run(() =>
            {
                try
                {
                    var exporter = ExporterFactory.CreateInstance(DbType.SqlServer, connectionString);
                    var list = exporter.GetDatabases();
                    Dispatcher.Invoke(() =>
                    {
                        ComboDefaultDatabase.ItemsSource = list;
                        if (connectId < 1)
                        {
                            ComboDefaultDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.Equals("master"));
                        }
                        else
                        {
                            var sqLiteHelper = new SQLiteHelper();
                            var connect = sqLiteHelper.db.Table<ConnectConfigs>().FirstOrDefault(x => x.ID == connectId);
                            if (connect != null)
                            {
                                ComboDefaultDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.Equals(connect.DefaultDatabase));
                            }
                        }
                        mainWindow.LoadingG.Visibility = Visibility.Collapsed;
                        if (isTest)
                        {
                            Oops.Success("连接成功");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Oops.God($"连接失败，原因：" + ex.ToMsg());
                        mainWindow.LoadingG.Visibility = Visibility.Collapsed;
                    }));
                }
            });
            #endregion
        }

        /// <summary>
        /// 保存连接
        /// </summary>
        public void SaveForm(bool isConnect)
        {
            #region MyRegion
            if (!VerifyForm())
            {
                return;
            }
            var mainWindow = (ConnectManage)Window.GetWindow(this);
            if (mainWindow == null)
            {
                return;
            }

            var connectId = Convert.ToInt32(HidId.Text);
            var connectName = TextConnectName.Text.Trim();
            var serverAddress = TextServerAddress.Text.Trim();
            var serverPort = Convert.ToInt32(TextServerPort.Value);
            var authentication = ComboAuthentication.SelectedValue == SqlServer ? 1 : 0;
            var userName = TextServerName.Text.Trim();
            var password = EncryptHelper.Encode(TextServerPassword.Password.Trim());
            var defaultDataBase = (DataBase)ComboDefaultDatabase.SelectedItem;
            var connectionString =
                ConnectionStringUtil.SqlServerString(serverAddress, serverPort, authentication, "master", userName, password);
            var sqLiteHelper = new SQLiteHelper();
            ConnectConfigs connectConfig;
            mainWindow.LoadingG.Visibility = Visibility.Visible;
            Task.Run(() =>
            {
                try
                {
                    if (isConnect)
                    {
                        var exporter = ExporterFactory.CreateInstance(DbType.SqlServer, connectionString);
                        exporter.GetDatabases();
                    }
                    Dispatcher.Invoke(() =>
                    {
                        mainWindow.LoadingG.Visibility = Visibility.Collapsed;
                        if (isConnect)
                        {
                            Growl.Success(new GrowlInfo { Message = $"连接成功", WaitTime = 1, ShowDateTime = false });
                        }
                        if (connectId > 0)
                        {
                            connectConfig = sqLiteHelper.db.Table<ConnectConfigs>().FirstOrDefault(x => x.ID == connectId);
                            if (connectConfig == null)
                            {
                                Growl.Warning(new GrowlInfo { Message = $"当前连接不存在或已被删除", WaitTime = 1, ShowDateTime = false });
                                return;
                            }
                            var connectAny = sqLiteHelper.db.Table<ConnectConfigs>().FirstOrDefault(x => x.ConnectName == connectName && x.ID != connectId);
                            if (connectAny != null)
                            {
                                Growl.Warning(new GrowlInfo { Message = $"已存在相同名称的连接名", WaitTime = 1, ShowDateTime = false });
                                return;
                            }
                            connectConfig.ConnectName = connectName;
                            connectConfig.DbType = DbType.SqlServer;
                            connectConfig.ServerAddress = serverAddress;
                            connectConfig.ServerPort = serverPort;
                            connectConfig.UserName = userName;
                            connectConfig.Password = password;
                            connectConfig.DefaultDatabase = defaultDataBase.DbName;
                            connectConfig.Authentication = authentication;
                            sqLiteHelper.db.Update(connectConfig);
                        }
                        else
                        {
                            var connect = sqLiteHelper.db.Table<ConnectConfigs>().FirstOrDefault(x => x.ConnectName.ToLower() == connectName.ToLower());
                            if (connect != null)
                            {
                                Growl.Warning(new GrowlInfo { Message = $"已存在相同名称的连接名", WaitTime = 1, ShowDateTime = false });
                                return;
                            }
                            connectConfig = new ConnectConfigs()
                            {
                                ConnectName = connectName,
                                DbType = DbType.SqlServer,
                                ServerAddress = serverAddress,
                                ServerPort = serverPort,
                                Authentication = authentication,
                                UserName = userName,
                                Password = password,
                                CreateDate = DateTime.Now,
                                DefaultDatabase = defaultDataBase == null ? "master" : defaultDataBase.DbName

                            };
                            sqLiteHelper.db.Insert(connectConfig);
                        }

                        Task.Run(() =>
                        {
                            var datalist = sqLiteHelper.db.Table<ConnectConfigs>().
                                ToList();
                            Dispatcher.Invoke(() =>
                            {
                                mainWindow.NoDataText.Visibility = Visibility.Collapsed;
                                mainWindow.DataList = datalist;
                                if (!isConnect)
                                {
                                    Growl.Success(new GrowlInfo { Message = $"保存成功", WaitTime = 1, ShowDateTime = false });
                                }
                                if (isConnect && ChangeRefreshEvent != null)
                                {
                                    ChangeRefreshEvent(connectConfig);
                                    mainWindow.Close();
                                }
                            });
                        });
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Oops.God($"连接失败，原因：" + ex.ToMsg());
                        mainWindow.LoadingG.Visibility = Visibility.Collapsed;
                    });
                }
            });
            #endregion
        }

        /// <summary>
        /// 刷新数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFresh_OnClick(object sender, RoutedEventArgs e)
        {
            TestConnect(false);
        }

        /// <summary>
        /// 身份认证切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboAuthentication_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            TextServerName.Visibility = Visibility.Visible;
            TextServerPassword.Visibility = Visibility.Visible;
            TbServerName.Visibility = Visibility.Visible;
            TbPassword.Visibility = Visibility.Visible;
            TbDefaultDataBase.Margin = new Thickness(0, 228, 20, 20);
            GdDefaultDatabase.Margin = new Thickness(0, 220, 0, 20);
            if (!SqlServer.IsSelected)
            {
                TbServerName.Visibility = Visibility.Collapsed;
                TbPassword.Visibility = Visibility.Collapsed;
                TextServerName.Visibility = Visibility.Collapsed;
                TextServerPassword.Visibility = Visibility.Collapsed;
                TextServerName.Background = TextServerPassword.Background;
                TbDefaultDataBase.Margin = new Thickness(0, 148, 20, 20);
                GdDefaultDatabase.Margin = new Thickness(0, 140, 0, 20);
            }
        }
    }
}
