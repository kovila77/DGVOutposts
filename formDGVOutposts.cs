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
        private DataTable dt;

        private readonly string sConnStr = new NpgsqlConnectionStringBuilder
        {
            Host = Database.Default.Host,
            Port = Database.Default.Port,
            Database = Database.Default.Name,
            Username = "postgres",//Environment.GetEnvironmentVariable("POSTGRESQL_USERNAME"),
            Password = "kovila77",//Environment.GetEnvironmentVariable("POSTGRESQL_PASSWORD"),
        }.ConnectionString;

        public formDGVOutposts()
        {
            InitializeComponent();

            dt = new DataTable();
            dt.Columns.Add("id", typeof(int));
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("economic_value", typeof(int));
            dt.Columns.Add("x", typeof(int));
            dt.Columns.Add("y", typeof(int));
            dt.Columns.Add("z", typeof(int));

            InitializeDGVOutposts();
            InitializeDGVMissions();
        }

        private void InitializeDGVMissions()
        {
        }

        private void InitializeDGVOutposts()
        {
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
                    dt.Rows.Add(reader["outpost_id"], reader["outpost_name"], reader["outpost_economic_value"], reader["outpost_coordinate_x"],
                        reader["outpost_coordinate_y"], reader["outpost_coordinate_z"]);
                    //dgvOutposts.Rows.Add(reader["outpost_id"], reader["outpost_name"], reader["outpost_economic_value"], reader["outpost_coordinate_x"],
                    //    reader["outpost_coordinate_y"], reader["outpost_coordinate_z"]);
                }
                dgvOutposts.DataSource = dt;
                dgvOutposts.Columns[dgvOutposts.Columns.Count - 1].HeaderText = "asdfa!!";
                //while (reader.Read())
                //{
                //    dgvOutposts.Rows.Add(reader["outpost_id"], reader["outpost_name"], reader["outpost_economic_value"], reader["outpost_coordinate_x"],
                //        reader["outpost_coordinate_y"], reader["outpost_coordinate_z"]);
                //    //outpost_id.Items.Add(new StringWithTag(reader["outpost_name"] as string, Convert.ToInt32(reader["outpost_id"])));                    
                //}
            }
            //outpost_id.DataSource = new BindingSource(dgvOutposts.Columns, "name");
            //outpost_id.ValueMember = "0";
            //outpost_id.DisplayMember = "1";
            //string fff = "";
            //foreach (var item in dgvOutposts.Rows)
            //{
            //    fff += dgvOutposts.Rows + "\n";
            //}
            //MessageBox.Show(fff);
        }

        private void dgvMissions_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
