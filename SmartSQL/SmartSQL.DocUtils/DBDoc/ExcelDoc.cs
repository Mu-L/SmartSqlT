using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using SmartSQL.DocUtils;
using SmartSQL.DocUtils.Dtos;
using SmartSQL.DocUtils.Properties;
using SmartSQL.DocUtils.DBDoc;
using OfficeOpenXml.Style;

namespace SmartSQL.DocUtils.DBDoc
{
    public class ExcelDoc : Doc
    {
        public ExcelDoc(DBDto dto, string filter = "excel files (.xlsx)|*.xlsx") : base(dto, filter)
        {

        }

        public override bool Build(string filePath)
        {
            ExportExcelByEpplus(filePath, this.Dto);
            return true;
        }



        /// <summary>
        /// 引用EPPlus.dll导出excel数据库字典文档
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="databaseName"></param>
        /// <param name="tables"></param>
        public void ExportExcelByEpplus(string fileName, DBDto dto)
        {
            var tables = dto.Tables;

            FileInfo xlsFileInfo = new FileInfo(fileName);

            if (xlsFileInfo.Exists)
            {
                //  注意此处：存在Excel文档即删除再创建一个
                xlsFileInfo.Delete();
                xlsFileInfo = new FileInfo(fileName);
            }
            //  创建并添加Excel文档信息
            using (OfficeOpenXml.ExcelPackage epck = new OfficeOpenXml.ExcelPackage(xlsFileInfo))
            {
                //  创建overview sheet
                CreateLogSheet(epck, AppConst.LOG_CHAPTER_NAME, tables);

                //  创建overview sheet
                CreateOverviewSheet(epck, AppConst.TABLE_CHAPTER_NAME, dto, tables);

                //  创建tables sheet
                CreateTableSheet(epck, AppConst.TABLE_STRUCTURE_CHAPTER_NAME, dto, tables);

                epck.Save(); // 保存excel
                epck.Dispose();
            }
            // 更新进度
            base.OnProgress(new ChangeRefreshProgressArgs
            {
                BuildNum = tables.Count,
                TotalNum = tables.Count,
                IsEnd = true
            });
        }

        /// <summary>
        /// 创建修订日志sheet
        /// </summary>
        /// <param name="epck"></param>
        /// <param name="sheetName"></param>
        /// <param name="tables"></param>
        private void CreateLogSheet(OfficeOpenXml.ExcelPackage epck, string sheetName, List<TableDto> tables)
        {
            #region MyRegion
            OfficeOpenXml.ExcelWorksheet overviewTbWorksheet = epck.Workbook.Worksheets.Add(sheetName);
            int row = 1;
            overviewTbWorksheet.Cells[row, 1, row, 5].Merge = true;
            row++; // 行号+1
            overviewTbWorksheet.Cells[row, 1].Value = "版本号";
            overviewTbWorksheet.Cells[row, 2].Value = "修订日期";
            overviewTbWorksheet.Cells[row, 3].Value = "修订内容";
            overviewTbWorksheet.Cells[row, 4].Value = "修订人";
            overviewTbWorksheet.Cells[row, 5].Value = "审核人";
            overviewTbWorksheet.Cells[row, 1, row, 5].Style.Font.Bold = true;
            overviewTbWorksheet.Cells[row, 1, row, 5].Style.Font.Size = 10;
            overviewTbWorksheet.Row(1).Height = 14.5; // 行高
            //  循环日志记录
            row++; // 行号+1
            for (var i = 0; i < 16; i++)
            {
                //  添加列标题
                overviewTbWorksheet.Cells[row, 1].Value = "";
                overviewTbWorksheet.Cells[row, 2].Value = "";
                overviewTbWorksheet.Cells[row, 3].Value = "";
                overviewTbWorksheet.Cells[row, 4].Value = "";
                overviewTbWorksheet.Cells[row, 5].Value = "";

                overviewTbWorksheet.Row(row).Height = 14.5; // 行高

                row++; // 行号+1
            }
            //  水平居中
            overviewTbWorksheet.Cells[1, 1, row - 1, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //  垂直居中
            overviewTbWorksheet.Cells[1, 1, row - 1, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //  上下左右边框线
            overviewTbWorksheet.Cells[1, 1, row - 1, 5].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            overviewTbWorksheet.Cells[1, 1, row - 1, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            overviewTbWorksheet.Cells[1, 1, row - 1, 5].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            overviewTbWorksheet.Cells[1, 1, row - 1, 5].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            overviewTbWorksheet.Column(1).Width = 25;
            overviewTbWorksheet.Column(2).Width = 25;
            overviewTbWorksheet.Column(3).Width = 50;
            overviewTbWorksheet.Column(4).Width = 25;
            overviewTbWorksheet.Column(5).Width = 25; 
            #endregion
        }

        /// <summary>
        /// 创建表目录sheet
        /// </summary>
        /// <param name="epck"></param>
        /// <param name="sheetName"></param>
        /// <param name="tables"></param>
        private void CreateOverviewSheet(OfficeOpenXml.ExcelPackage epck, string sheetName, DBDto dto, List<TableDto> tables)
        {
            #region MyRegion
            OfficeOpenXml.ExcelWorksheet overviewTbWorksheet = epck.Workbook.Worksheets.Add(sheetName);
            int row = 1;
            overviewTbWorksheet.Cells[row, 1, row, 3].Value = dto.DocTitle;
            overviewTbWorksheet.Cells[row, 1, row, 3].Merge = true;
            overviewTbWorksheet.Cells[row, 1, row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            overviewTbWorksheet.Cells[row, 1, row, 3].Style.Font.Bold = true;
            overviewTbWorksheet.Row(1).Height = 20; // 行高
            var colFromHex = System.Drawing.ColorTranslator.FromHtml("#f2f2f2");
            overviewTbWorksheet.Cells[row, 1, row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
            overviewTbWorksheet.Cells[row, 1, row, 3].Style.Fill.BackgroundColor.SetColor(colFromHex);
            row++;
            overviewTbWorksheet.Cells[row, 1].Value = "序号";
            overviewTbWorksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            overviewTbWorksheet.Cells[row, 2].Value = "表名";
            overviewTbWorksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            overviewTbWorksheet.Cells[row, 3].Value = "注释/说明";
            overviewTbWorksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            overviewTbWorksheet.Cells[row, 1, row, 3].Style.Font.Bold = true;
            overviewTbWorksheet.Cells[row, 1, row, 3].Style.Font.Size = 10;
            overviewTbWorksheet.Row(2).Height = 14.5; // 行高
            //  循环数据库表名
            row++;
            foreach (var table in tables)
            {
                var comment = !string.IsNullOrWhiteSpace(table.Comment) ? table.Comment : "";
                //  数据库名称
                //  添加列标题
                overviewTbWorksheet.Cells[row, 1].Value = table.TableOrder;
                overviewTbWorksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                overviewTbWorksheet.Cells[row, 2].Value = table.TableName;
                overviewTbWorksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                overviewTbWorksheet.Cells[row, 3].Value = comment;
                overviewTbWorksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                overviewTbWorksheet.Row(row).Height = 14.5; // 行高
                row++; // 行号+1
            }
            //  上下左右边框线
            overviewTbWorksheet.Cells[1, 1, row - 1, 3].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            overviewTbWorksheet.Cells[1, 1, row - 1, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            overviewTbWorksheet.Cells[1, 1, row - 1, 3].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            overviewTbWorksheet.Cells[1, 1, row - 1, 3].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            overviewTbWorksheet.Column(1).Width = 10;
            overviewTbWorksheet.Column(2).Width = 50;
            overviewTbWorksheet.Column(3).Width = 50; 
            #endregion
        }

        /// <summary>
        /// 创建表结构sheet
        /// </summary>
        /// <param name="epck"></param>
        /// <param name="sheetName"></param>
        /// <param name="tables"></param>
        private void CreateTableSheet(OfficeOpenXml.ExcelPackage epck, string sheetName, DBDto dto, List<TableDto> tables)
        {
            #region MyRegion
            OfficeOpenXml.ExcelWorksheet tbWorksheet = epck.Workbook.Worksheets.Add(sheetName);
            int rowNum = 1, fromRow = 0, count = 0; // 行号计数器
                                                    //  循环数据库表名
            foreach (var table in tables)
            {
                var lstName = new List<string>
                {
                    "序号","列名","数据类型","长度","主键","自增","允许空","默认值","列说明"
                };

                //oracle不显示 列是否自增
                if (table.DBType.StartsWith("Oracle"))
                {
                    lstName.Remove("自增");
                }
                var spColCount = lstName.Count;

                // 表名称
                var comment = table.TableName + " " + (!string.IsNullOrWhiteSpace(table.Comment) ? table.Comment : "");
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Merge = true;
                tbWorksheet.Cells[rowNum, 1].Value = comment;
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Font.Bold = true;
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Font.Size = 10;
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                tbWorksheet.Row(rowNum).Height = 16;
                var colFromHex = System.Drawing.ColorTranslator.FromHtml("#f2f2f2");
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Fill.PatternType = ExcelFillStyle.Solid;
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Fill.BackgroundColor.SetColor(colFromHex);

                //  注意：保存起始行号
                fromRow = rowNum;

                rowNum++; // 行号+1

                // tbWorksheet.Cells[int FromRow, int FromCol, int ToRow, int ToCol]
                //  列标题字体为粗体
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Font.Bold = true;

                //  添加列标题
                for (int j = 0; j < lstName.Count; j++)
                {
                    tbWorksheet.Cells[rowNum, j + 1].Value = lstName[j];
                    tbWorksheet.Cells[rowNum, j + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                rowNum++; // 行号+1

                //  添加数据行,遍历数据库表字段
                foreach (var column in table.Columns)
                {
                    tbWorksheet.Cells[rowNum, 1].Value = column.ColumnOrder;
                    tbWorksheet.Cells[rowNum, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tbWorksheet.Cells[rowNum, 2].Value = column.ColumnName;
                    tbWorksheet.Cells[rowNum, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    tbWorksheet.Cells[rowNum, 3].Value = column.ColumnTypeName;
                    tbWorksheet.Cells[rowNum, 4].Value = column.Length;
                    tbWorksheet.Cells[rowNum, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tbWorksheet.Cells[rowNum, 5].Value = column.IsPK;
                    tbWorksheet.Cells[rowNum, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    //oracle不显示 列是否自增
                    if (table.DBType.StartsWith("Oracle"))
                    {
                        tbWorksheet.Cells[rowNum, 6].Value = column.CanNull;
                        tbWorksheet.Cells[rowNum, 7].Value = column.DefaultVal;
                        tbWorksheet.Cells[rowNum, 8].Value = column.Comment;
                        tbWorksheet.Cells[rowNum, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    else
                    {
                        tbWorksheet.Cells[rowNum, 6].Value = column.IsIdentity;
                        tbWorksheet.Cells[rowNum, 7].Value = column.CanNull;
                        tbWorksheet.Cells[rowNum, 8].Value = column.DefaultVal;
                        tbWorksheet.Cells[rowNum, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tbWorksheet.Cells[rowNum, 9].Value = column.Comment;
                        tbWorksheet.Cells[rowNum, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    tbWorksheet.Cells[rowNum, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tbWorksheet.Cells[rowNum, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rowNum++; // 行号+1
                }
                //  上下左右边框线
                tbWorksheet.Cells[fromRow, 1, rowNum - 1, spColCount].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                tbWorksheet.Cells[fromRow, 1, rowNum - 1, spColCount].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                tbWorksheet.Cells[fromRow, 1, rowNum - 1, spColCount].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                tbWorksheet.Cells[fromRow, 1, rowNum - 1, spColCount].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                //  处理空白行，分割用
                if (count < tables.Count - 1)
                {
                    tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Merge = true;
                }
                rowNum++; // 行号+1
                count++; // 计数器+1
                // 更新进度
                base.OnProgress(new ChangeRefreshProgressArgs
                {
                    BuildNum = count,
                    TotalNum = tables.Count,
                    BuildName = table.TableName
                });
            }

            tbWorksheet.Column(1).Width = 10;
            tbWorksheet.Column(2).Width = 35;
            tbWorksheet.Column(3).Width = 15;
            tbWorksheet.Column(4).Width = 10;
            tbWorksheet.Column(5).Width = 10;
            //oracle不显示 列是否自增
            if (dto.DBType.StartsWith("Oracle"))
            {
                tbWorksheet.Column(6).Width = 10;
                tbWorksheet.Column(7).Width = 10;
                tbWorksheet.Column(8).Width = 35;
            }
            else
            {
                tbWorksheet.Column(6).Width = 10;
                tbWorksheet.Column(7).Width = 10;
                tbWorksheet.Column(8).Width = 10;
                tbWorksheet.Column(9).Width = 35;
            }
            //  设置表格样式
            tbWorksheet.Cells.Style.WrapText = true; // 自动换行
            tbWorksheet.Cells.Style.ShrinkToFit = true; // 单元格自动适应大小 
            #endregion
        }



        /// <summary>
        /// 创建表结构sheet
        /// </summary>
        /// <param name="epck"></param>
        /// <param name="sheetName"></param>
        /// <param name="tables"></param>
        private void CreateTableTabSheet(OfficeOpenXml.ExcelPackage epck, string sheetName, DBDto dto, List<TableDto> tables)
        {
            #region MyRegion
            OfficeOpenXml.ExcelWorksheet tbWorksheet = epck.Workbook.Worksheets.Add(sheetName);
            int rowNum = 1, fromRow = 0, count = 0; // 行号计数器
                                                    //  循环数据库表名
            foreach (var table in tables)   
            {
                var lstName = new List<string>
                {
                    "序号","列名","数据类型","长度","主键","自增","允许空","默认值","列说明"
                };

                //oracle不显示 列是否自增
                if (table.DBType.StartsWith("Oracle"))
                {
                    lstName.Remove("自增");
                }
                var spColCount = lstName.Count;

                // 表名称
                var comment = table.TableName + " " + (!string.IsNullOrWhiteSpace(table.Comment) ? table.Comment : "");
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Merge = true;
                tbWorksheet.Cells[rowNum, 1].Value = comment;
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Font.Bold = true;
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Font.Size = 10;
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                tbWorksheet.Row(rowNum).Height = 16;
                var colFromHex = System.Drawing.ColorTranslator.FromHtml("#f2f2f2");
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Fill.PatternType = ExcelFillStyle.Solid;
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Fill.BackgroundColor.SetColor(colFromHex);

                //  注意：保存起始行号
                fromRow = rowNum;

                rowNum++; // 行号+1

                // tbWorksheet.Cells[int FromRow, int FromCol, int ToRow, int ToCol]
                //  列标题字体为粗体
                tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Font.Bold = true;

                //  添加列标题
                for (int j = 0; j < lstName.Count; j++)
                {
                    tbWorksheet.Cells[rowNum, j + 1].Value = lstName[j];
                    tbWorksheet.Cells[rowNum, j + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                rowNum++; // 行号+1

                //  添加数据行,遍历数据库表字段
                foreach (var column in table.Columns)
                {
                    tbWorksheet.Cells[rowNum, 1].Value = column.ColumnOrder;
                    tbWorksheet.Cells[rowNum, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tbWorksheet.Cells[rowNum, 2].Value = column.ColumnName;
                    tbWorksheet.Cells[rowNum, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    tbWorksheet.Cells[rowNum, 3].Value = column.ColumnTypeName;
                    tbWorksheet.Cells[rowNum, 4].Value = column.Length;
                    tbWorksheet.Cells[rowNum, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tbWorksheet.Cells[rowNum, 5].Value = column.IsPK;
                    tbWorksheet.Cells[rowNum, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    //oracle不显示 列是否自增
                    if (table.DBType.StartsWith("Oracle"))
                    {
                        tbWorksheet.Cells[rowNum, 6].Value = column.CanNull;
                        tbWorksheet.Cells[rowNum, 7].Value = column.DefaultVal;
                        tbWorksheet.Cells[rowNum, 8].Value = column.Comment;
                        tbWorksheet.Cells[rowNum, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    else
                    {
                        tbWorksheet.Cells[rowNum, 6].Value = column.IsIdentity;
                        tbWorksheet.Cells[rowNum, 7].Value = column.CanNull;
                        tbWorksheet.Cells[rowNum, 8].Value = column.DefaultVal;
                        tbWorksheet.Cells[rowNum, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        tbWorksheet.Cells[rowNum, 9].Value = column.Comment;
                        tbWorksheet.Cells[rowNum, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    tbWorksheet.Cells[rowNum, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tbWorksheet.Cells[rowNum, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rowNum++; // 行号+1
                }
                //  上下左右边框线
                tbWorksheet.Cells[fromRow, 1, rowNum - 1, spColCount].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                tbWorksheet.Cells[fromRow, 1, rowNum - 1, spColCount].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                tbWorksheet.Cells[fromRow, 1, rowNum - 1, spColCount].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                tbWorksheet.Cells[fromRow, 1, rowNum - 1, spColCount].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                //  处理空白行，分割用
                if (count < tables.Count - 1)
                {
                    tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    tbWorksheet.Cells[rowNum, 1, rowNum, spColCount].Merge = true;
                }
                rowNum++; // 行号+1
                count++; // 计数器+1
                // 更新进度
                base.OnProgress(new ChangeRefreshProgressArgs
                {
                    BuildNum = count,
                    TotalNum = tables.Count,
                    BuildName = table.TableName
                });
            }

            tbWorksheet.Column(1).Width = 10;
            tbWorksheet.Column(2).Width = 35;
            tbWorksheet.Column(3).Width = 15;
            tbWorksheet.Column(4).Width = 10;
            tbWorksheet.Column(5).Width = 10;
            //oracle不显示 列是否自增
            if (dto.DBType.StartsWith("Oracle"))
            {
                tbWorksheet.Column(6).Width = 10;
                tbWorksheet.Column(7).Width = 10;
                tbWorksheet.Column(8).Width = 35;
            }
            else
            {
                tbWorksheet.Column(6).Width = 10;
                tbWorksheet.Column(7).Width = 10;
                tbWorksheet.Column(8).Width = 10;
                tbWorksheet.Column(9).Width = 35;
            }
            //  设置表格样式
            tbWorksheet.Cells.Style.WrapText = true; // 自动换行
            tbWorksheet.Cells.Style.ShrinkToFit = true; // 单元格自动适应大小 
            #endregion
        }
    }
}
