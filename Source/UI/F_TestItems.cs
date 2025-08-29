using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P
{
    public partial class TestItemOnOff : Form
    {
        public Spec Spec { get { return STATIC.Rcp.Spec; } }
        public TestItemOnOff()
        {
            InitializeComponent();
            InitTestItemGird();
        }
        private void TestItemOnOff_Load(object sender, EventArgs e)
        {
            Location = new Point(611, 163);
        }
        private void InitTestItemGird()
        {
            TestItemGrid.ColumnCount = 5;
            TestItemGrid.Font = new Font("Calibri", 10, FontStyle.Bold);
            for (int i = 0; i < TestItemGrid.ColumnCount; i++)
            {
                TestItemGrid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            TestItemGrid.RowHeadersVisible = false;
            TestItemGrid.BackgroundColor = Color.LightGray;

            // Column
            TestItemGrid.Columns[0].Name = "Axis";
            TestItemGrid.Columns[1].Name = "Test Item";
            TestItemGrid.Columns[2].Name = "Min";
            TestItemGrid.Columns[3].Name = "Max";
            TestItemGrid.Columns[4].Name = "Enable";
            for (int i = 0; i < 5; i++)
                TestItemGrid.Columns[i].DefaultCellStyle.Font = new Font("Calibri", 10, FontStyle.Bold);

            TestItemGrid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            TestItemGrid.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            TestItemGrid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            TestItemGrid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            TestItemGrid.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            TestItemGrid.Columns[0].Width = 80;
            TestItemGrid.Columns[1].Width = 140;
            TestItemGrid.Columns[2].Width = 70;
            TestItemGrid.Columns[3].Width = 70;
            TestItemGrid.Columns[4].Width = 80;

            string colTitle;
            TestItemGrid.Rows.Clear();

            for (int i = 0; i < Spec.Param.Count; i++)
            {
                if (i == 0) colTitle = Spec.Param[i][0].ToString();
                else
                {
                    if (Spec.Param[i - 1][0].ToString() == Spec.Param[i][0].ToString()) colTitle = "";
                    else colTitle = Spec.Param[i][0].ToString();
                }

                TestItemGrid.Rows.Add(colTitle, Spec.Param[i][1], Spec.Param[i][2], Spec.Param[i][3], Spec.Param[i][10]);

                bool bUse = Convert.ToBoolean(TestItemGrid[4, i].Value);
                if (bUse) TestItemGrid[4, i].Style.BackColor = Color.White;
                else TestItemGrid[4, i].Style.BackColor = Color.LightGray;
            }
            TestItemGrid.Rows[0].Visible = false;

            TestItemGrid.Rows.Add("", "", "", "", "");

            TestItemGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            TestItemGrid.ColumnHeadersHeight = 22;

            for (int i = 0; i < Spec.Param.Count; i++)
            {
                TestItemGrid.Rows[i].Height = 15;
                TestItemGrid.Rows[i].Resizable = DataGridViewTriState.False;
                TestItemGrid.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9, FontStyle.Bold);
                TestItemGrid[1, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                TestItemGrid[2, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                TestItemGrid[4, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
            }
            TestItemGrid.ReadOnly = true;
        }

        private void TestItemSaveBtn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Spec.Param.Count; i++)
            {
                Spec.Param[i][10] = TestItemGrid[4, i].Value;
            }
            Spec.Save();
            Close();
        }

        private void TestItemGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                bool bUse = Convert.ToBoolean(TestItemGrid[4, e.RowIndex].Value);
                TestItemGrid[4, e.RowIndex].Value = !bUse;
                if (!bUse) TestItemGrid[4, e.RowIndex].Style.BackColor = Color.White;
                else TestItemGrid[4, e.RowIndex].Style.BackColor = Color.LightGray;
            }
        }
    }
}
