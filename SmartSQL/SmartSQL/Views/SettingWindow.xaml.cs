﻿using System;
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
using SmartSQL.Framework;
using SmartSQL.Framework.Const;
using SmartSQL.Framework.SqliteModel;

namespace SmartSQL.Views
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow
    {
        public SettingWindow()
        {
            InitializeComponent();
            ListGroup.SelectedItem = ListItemRoutine;
            ListItemRoutine.Focus();
            var sqLiteHelper = new SQLiteHelper();
            var sysSets = sqLiteHelper.db.Table<SystemSet>().Where(x => KeyList.Contains(x.Name)).ToList();
            sysSets.ForEach(x =>
            {
                var value = Convert.ToBoolean(x.Value);
                switch (x.Name)
                {
                    case "IsMultipleTab":
                        ChkIsMultipleTab.IsChecked = value; break;
                    case "IsLikeSearch":
                        ChkIsLikeSearch.IsChecked = value; break;
                    case "IsContainsObjName":
                        ChkIsContainsObjName.IsChecked = value; break;
                    case "IsShowSaveWin":
                        ChkIsShowSaveWin.IsChecked = value; break;
                }
            });
        }

        private readonly List<string> KeyList = new List<string> { SysConst.Sys_IsMultipleTab, SysConst.Sys_IsLikeSearch, SysConst.Sys_IsContainsObjName,SysConst.Sys_IsShowSaveWin };
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            var isMultipleTab = ChkIsMultipleTab.IsChecked == true;
            var isLikeSearch = ChkIsLikeSearch.IsChecked == true;
            var isContainsObjName = ChkIsContainsObjName.IsChecked == true;
            var isShowSaveWin = ChkIsShowSaveWin.IsChecked == true;

            var sqLiteHelper = new SQLiteHelper();
            var sysSets = sqLiteHelper.db.Table<SystemSet>().Where(x => KeyList.Contains(x.Name)).ToList();

            sysSets.ForEach(x =>
            {
                if (x.Name == SysConst.Sys_IsMultipleTab)
                {
                    x.Value = isMultipleTab.ToString();
                }
                if (x.Name == SysConst.Sys_IsLikeSearch)
                {
                    x.Value = isLikeSearch.ToString();
                }
                if (x.Name == SysConst.Sys_IsContainsObjName)
                {
                    x.Value = isContainsObjName.ToString();
                }
                if (x.Name == SysConst.Sys_IsShowSaveWin)
                {
                    x.Value = isShowSaveWin.ToString();
                }
            });
            sqLiteHelper.db.UpdateAll(sysSets);
            this.Close();
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
