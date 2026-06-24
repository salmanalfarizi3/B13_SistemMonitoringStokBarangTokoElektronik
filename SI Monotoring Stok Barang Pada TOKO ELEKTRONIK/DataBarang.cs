using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SI_Monotoring_Stok_Barang_Pada_TOKO_ELEKTRONIK
{
    // UBAH internal MENJADI public DISINI:
    public class DataBarang
    {
        public string ID_Barang { get; set; }
        public string Nama_Barang { get; set; }
        public int Jumlah { get; set; }
        public string Keterangan { get; set; }
        public DateTime Tanggal { get; set; }
    }
}