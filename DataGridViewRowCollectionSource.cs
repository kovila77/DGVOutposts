using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DGVOutposts
{
    public class DataGridViewRowCollectionSource : DataGridViewRowCollection
    {
        public DataGridViewRowCollectionSource(DataGridView dataGridView) : base(dataGridView)
        {
        }
    }
}
