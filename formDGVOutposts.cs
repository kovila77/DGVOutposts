using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DGVOutposts
{
    public partial class formDGVOutposts : Form
    {
        private DataTable dtOutposts;

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
        }

        private void InitializeDGVMissions()
        {
            dgvMissions.Columns[dgvMissions.Columns.Add("id", "id")].Visible = false;
            dgvMissions.Columns.Add("description", "Описание миссии");
            dgvMissions.Columns.Add(new CalendarColumn { Name = "date_begin", HeaderText = "Дата начала" });
            dgvMissions.Columns.Add(new CalendarColumn { Name = "date_plan_end", HeaderText = "Планируемое завершение" });
            dgvMissions.Columns.Add(new CalendarColumn { Name = "date_actual_end", HeaderText = "Реальное завершение" });


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
                                    FROM outpost_missions;; "
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
                                    FROM outposts; "
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
            bool errorInRow = false;
            var cellsWithPotentialErrors = new[] { row.Cells["name"], row.Cells["economic_value"], row.Cells["x"], row.Cells["y"], row.Cells["z"] };
            foreach (var cell in cellsWithPotentialErrors)
            {
                if (cell.Value is null || cell.Value is string && string.IsNullOrWhiteSpace((string)cell.Value))
                {
                    cell.ErrorText = "Пустое значение!";
                    //row.ErrorText = $"Значение в столбце '{cell.OwningColumn.HeaderText} не может быть пустым";
                    row.ErrorText = $"Проверьте данные";
                    errorInRow = true;
                }
                else
                {
                    cell.ErrorText = "";
                }
            }
            int trash;
            //if (int.TryParse((string)row.Cells["economic_value"].Value, out trash) && int.TryParse((string)row.Cells["x"].Value, out trash) && int.TryParse((string)row.Cells["y"].Value, out trash) && int.TryParse((string)row.Cells["z"].Value, out trash))
            //{

            //}
            if (errorInRow) return;
            row.ErrorText = "";
            row.Tag = true;
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
            bool errorInRow = false;
            var cellsWithPotentialErrors = new[] { row.Cells["outpost_id"], row.Cells["description"], row.Cells["date_begin"], row.Cells["date_plan_end"] };
            foreach (var cell in cellsWithPotentialErrors)
            {
                if (cell.Value is null || cell.Value is string && string.IsNullOrWhiteSpace((string)cell.Value))
                {
                    cell.ErrorText = "Пустое значение!";
                    row.ErrorText = $"Проверьте данные";
                    errorInRow = true;
                }
                else
                {
                    cell.ErrorText = "";
                }
            }
            if (errorInRow) return;
            row.ErrorText = "";
            row.Tag = true;
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
                sCommand.Parameters.AddWithValue("date_actual_end", row.Cells["date_actual_end"].Value);

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
    }
}
