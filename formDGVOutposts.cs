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
        }

        private void InitializeDGVMissions()
        {
            dgvMissions.Columns[dgvMissions.Columns.Add("id", "id")].Visible = false;
            dgvMissions.Columns.Add("description", "Описание миссии");
            dgvMissions.Columns.Add(new CalendarColumn { Name = "cc_date_begin", HeaderText = "Дата начала" });
            dgvMissions.Columns.Add(new CalendarColumn { Name = "cc_date_plan_end", HeaderText = "Планируемое завершение" });
            dgvMissions.Columns.Add(new CalendarColumn { Name = "cc_date_actual_end", HeaderText = "Реальное завершение" });


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

        private void dgvMissions_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvMissions_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void formDGVOutposts_Load(object sender, EventArgs e)
        {
            InitializeDGVOutposts();
            InitializeDGVMissions();
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
        }

        private void dgvMissions_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
        }
    }
}
