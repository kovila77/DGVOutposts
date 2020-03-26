using Npgsql;
using System;
using System.Data;
using System.Windows.Forms;

namespace DGVOutposts
{
    public partial class formDGVOutposts : Form
    {
        private DataTable dtOutposts;
        private DataGridViewCellEventArgs mouseLocation;
        private DataGridViewComboBoxColumn outpost_id;
        private readonly string sConnStr = new NpgsqlConnectionStringBuilder
        {
            Host = Database.Default.Host,
            Port = Database.Default.Port,
            Database = Database.Default.Name,
            Username = Environment.GetEnvironmentVariable("POSTGRESQL_USERNAME"),
            Password = Environment.GetEnvironmentVariable("POSTGRESQL_PASSWORD"),
        }.ConnectionString;

        public formDGVOutposts()
        {
            InitializeComponent();
            InitializeDGVOutposts();
            InitializeDGVMissions();
            OffColumnSortingDGV(dgvOutposts);
            OffColumnSortingDGV(dgvMissions);
            tabControl1.TabPages[0].Tag = dgvOutposts;
            tabControl1.TabPages[1].Tag = dgvMissions;
        }

        private void OffColumnSortingDGV(DataGridView dgv) { foreach (DataGridViewColumn column in dgv.Columns) column.SortMode = DataGridViewColumnSortMode.NotSortable; }

        private void InitializeDGVMissions()
        {
            if (!(dgvMissions.Tag is null) && (bool)dgvMissions.Tag)
            {
                dgvMissions.Columns.Remove("id");
                dgvMissions.Columns.Remove("description");
                dgvMissions.Columns.Remove("date_begin");
                dgvMissions.Columns.Remove("date_plan_end");
                dgvMissions.Columns.Remove("date_actual_end");
                dgvMissions.Columns.Remove("outpost_id");
            }            
            dgvMissions.Columns.Add(outpost_id);
            dgvMissions.Columns[dgvMissions.Columns.Add("id", "id")].Visible = false;
            dgvMissions.Columns.Add("description", "Описание миссии");
            dgvMissions.Columns.Add(new CalendarColumn { Name = "date_begin", HeaderText = "Дата начала" });
            dgvMissions.Columns.Add(new CalendarColumn { Name = "date_plan_end", HeaderText = "Планируемое завершение" });
            var ccDAE = new CalendarColumn { Name = "date_actual_end", HeaderText = "Реальное завершение" };
            ccDAE.ContextMenuStrip = contextMenuStrip1;
            dgvMissions.Columns.Add(ccDAE);

            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                var sCommand = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT mission_id,
                                           outpost_id,
                                           mission_description,
                                           date_begin,
                                           date_plan_end,
                                           date_actual_end
                                    FROM outpost_missions order by outpost_id; "
                };
                var reader = sCommand.ExecuteReader();
                while (reader.Read())
                {
                    dgvMissions.Rows.Add(
                        reader["outpost_id"],
                        reader["mission_id"],
                        reader["mission_description"],
                        reader["date_begin"],
                        reader["date_plan_end"],
                        reader["date_actual_end"]);
                }
            }
            dgvMissions.Tag = true;
        }

        private void InitializeDGVOutposts()
        {
            dtOutposts = new DataTable();
            dtOutposts.Columns.Add("id", typeof(int));
            dtOutposts.Columns.Add("name", typeof(string));
            dtOutposts.Columns.Add("economic_value", typeof(int));
            dtOutposts.Columns.Add("x", typeof(int));
            dtOutposts.Columns.Add("y", typeof(int));
            dtOutposts.Columns.Add("z", typeof(int));
            outpost_id = new DataGridViewComboBoxColumn() { Name = "outpost_id", HeaderText = "Форпост" };

            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                var sCommand = new NpgsqlCommand
                {
                    Connection = sConn,
                    CommandText = @"SELECT outpost_id,
                                        outpost_name,
                                        outpost_economic_value,
                                        outpost_coordinate_x,
                                        outpost_coordinate_y,
                                        outpost_coordinate_z
                                    FROM outposts order by outpost_name; "
                };
                var reader = sCommand.ExecuteReader();
                while (reader.Read())
                {
                    dtOutposts.Rows.Add(
                        reader["outpost_id"],
                        reader["outpost_name"],
                        reader["outpost_economic_value"],
                        reader["outpost_coordinate_x"],
                        reader["outpost_coordinate_y"],
                        reader["outpost_coordinate_z"]);
                }
            }
            dgvOutposts.DataSource = dtOutposts;

            dgvOutposts.Columns["id"].Visible = false;
            dgvOutposts.Columns["name"].HeaderText = "Название";
            dgvOutposts.Columns["economic_value"].HeaderText = "Ценность";
            dgvOutposts.Columns["x"].HeaderText = "x";
            dgvOutposts.Columns["y"].HeaderText = "y";
            dgvOutposts.Columns["z"].HeaderText = "z";

            outpost_id.DataSource = dtOutposts;
            outpost_id.ValueMember = "id";
            outpost_id.DisplayMember = "name";
        }

        private void dgvOutposts_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (!dgvOutposts.IsCurrentRowDirty)
            {
                return;
            }
            var row = dgvOutposts.Rows[e.RowIndex];
            bool willCommit = true;
            row.Tag = false;
            var cellsWithPotentialErrors = new[] { row.Cells["name"], row.Cells["economic_value"], row.Cells["x"], row.Cells["y"], row.Cells["z"] };
            foreach (var cell in cellsWithPotentialErrors)
            {
                if (cell.Value is DBNull
                    || string.IsNullOrWhiteSpace((string)cell.FormattedValue))
                {
                    cell.ErrorText = "Пустое значение!";
                    row.ErrorText = $"Проверьте данные";
                    //row.Tag = true;
                    willCommit = false;
                }
                else
                {
                    cell.ErrorText = "";
                }
            }
            if (willCommit)
            {
                row.ErrorText = ""; row.Tag = willCommit;
                return;
            }
        }

        private void dgvOutposts_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (!(dgvOutposts.Rows[e.RowIndex].Tag is bool && (bool)dgvOutposts.Rows[e.RowIndex].Tag) || dgvOutposts.CurrentRow.ErrorText != "")
            {
                return;
            }
            var row = dgvOutposts.Rows[e.RowIndex];

            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                var sCommand = new NpgsqlCommand
                {
                    Connection = sConn
                };
                sCommand.Parameters.AddWithValue("name", row.Cells["name"].Value);
                sCommand.Parameters.AddWithValue("economic_value", row.Cells["economic_value"].Value);
                sCommand.Parameters.AddWithValue("x", row.Cells["x"].Value);
                sCommand.Parameters.AddWithValue("y", row.Cells["y"].Value);
                sCommand.Parameters.AddWithValue("z", row.Cells["z"].Value);
                if (row.Cells["id"].Value is int)
                {
                    sCommand.CommandText = @"
                        UPDATE outposts
                        SET outpost_name           = @name,
                            outpost_economic_value = @economic_value,
                            outpost_coordinate_x   = @x,
                            outpost_coordinate_y   = @y,
                            outpost_coordinate_z   = @z
                        WHERE outpost_id = @id;";
                    sCommand.Parameters.AddWithValue("id", (int)row.Cells["id"].Value);
                    sCommand.ExecuteNonQuery();
                }
                else
                {
                    sCommand.CommandText = @"
                        INSERT INTO outposts (outpost_name,
                                              outpost_economic_value,
                                              outpost_coordinate_x,
                                              outpost_coordinate_y,
                                              outpost_coordinate_z)
                        VALUES (@name,
                                @economic_value,
                                @x,
                                @y,
                                @z)
                        RETURNING outpost_id;";
                    row.Cells["id"].Value = sCommand.ExecuteScalar();
                }
            }
            row.Tag = false;
        }

        private void dgvMissions_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (!dgvMissions.IsCurrentRowDirty)
            {
                return;
            }
            var row = dgvMissions.Rows[e.RowIndex];
            row.Tag = false;
            bool willCommit = true;
            //row.ErrorText = "error";
            var cellsWithPotentialErrors = new[] { row.Cells["outpost_id"], row.Cells["description"], row.Cells["date_begin"], row.Cells["date_plan_end"] };
            foreach (var cell in cellsWithPotentialErrors)
            {
                if (cell.Value is DBNull
                        || string.IsNullOrWhiteSpace((string)cell.FormattedValue))
                {
                    if (cell.OwningColumn.Name == "outpost_id" && cell.Value is DBNull)
                    {
                        cell.ErrorText = "Данный форпост не сохранён в базе данных!";
                    }
                    else
                    {
                        cell.ErrorText = "Пустое значение!";
                    }
                    willCommit = false;
                    row.ErrorText = $"Проверьте данные";
                }
                else
                {
                    cell.ErrorText = "";
                }
            }
            if (willCommit)
            {
                row.ErrorText = ""; row.Tag = willCommit;
                return;
            }
        }

        private void dgvMissions_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (!(dgvMissions.Rows[e.RowIndex].Tag is bool && (bool)dgvMissions.Rows[e.RowIndex].Tag) || dgvMissions.CurrentRow.ErrorText != "")
            {
                return;
            }
            var row = dgvMissions.Rows[e.RowIndex];

            using (var sConn = new NpgsqlConnection(sConnStr))
            {
                sConn.Open();
                var sCommand = new NpgsqlCommand
                {
                    Connection = sConn
                };
                sCommand.Parameters.AddWithValue("description", row.Cells["description"].Value);
                sCommand.Parameters.AddWithValue("outpost_id", row.Cells["outpost_id"].Value);
                sCommand.Parameters.AddWithValue("date_begin", row.Cells["date_begin"].Value);
                sCommand.Parameters.AddWithValue("date_plan_end", row.Cells["date_plan_end"].Value);
                sCommand.Parameters.AddWithValue("date_actual_end", row.Cells["date_actual_end"].Value is null ? DBNull.Value : row.Cells["date_actual_end"].Value);

                if (row.Cells["id"].Value is int)
                {
                    sCommand.CommandText = @"
                        UPDATE outpost_missions
                        SET outpost_id          = @outpost_id,
                            mission_description = @description,
                            date_begin          = @date_begin,
                            date_plan_end       = @date_plan_end,
                            date_actual_end     = @date_actual_end
                        WHERE mission_id        = @id;";
                    sCommand.Parameters.AddWithValue("id", (int)row.Cells["id"].Value);
                    var res = sCommand.ExecuteScalar();
                }
                else
                {
                    sCommand.CommandText = @"
                        INSERT INTO outpost_missions (outpost_id,
                                                      mission_description,
                                                      date_begin,
                                                      date_plan_end,
                                                      date_actual_end)
                        VALUES (@outpost_id,
                                @description,
                                @date_begin,
                                @date_plan_end,
                                @date_actual_end)
                        RETURNING mission_id;";
                    row.Cells["id"].Value = sCommand.ExecuteScalar();
                }
            }
            row.Tag = false;
        }

        private void dgvOutposts_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Context.ToString().Contains(DataGridViewDataErrorContexts.Parsing.ToString()))
            {
                var cell = dgvOutposts[e.ColumnIndex, e.RowIndex];
                cell.Value = null;
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void setNULLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //var t = (DataGridViewRow)contextMenuStrip1.SourceControl;
            //t.CurrentCell.Value = DBNull.Value;
            //var cell = (DataGridCell)contextMenuStrip1.Tag;
            dgvMissions[mouseLocation.ColumnIndex, mouseLocation.RowIndex].Value = DBNull.Value;
            dgvMissions.Rows[mouseLocation.RowIndex].Tag = true;
            dgvMissions_RowValidated(null, new DataGridViewCellEventArgs(mouseLocation.ColumnIndex, mouseLocation.RowIndex));
        }

        private void dgvMissions_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            mouseLocation = e;
        }

        private void перезагрузитьДанныеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    InitializeDGVOutposts();
                    break;
                case 1:
                    InitializeDGVMissions();
                    break;
                default:
                    break;
            }
        }
    }
}
