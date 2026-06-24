using System;
using System.Collections.Generic; // WAJIB: Agar program mengenali List<>
using System.Windows.Forms;

namespace SI_Monotoring_Stok_Barang_Pada_TOKO_ELEKTRONIK
{
    public partial class FormCetak : Form
    {
        // 1. Ubah penampung dari DataTable menjadi List<DataBarang>
        private List<DataBarang> _listLaporan;

        // 2. PERBAIKAN CONSTRUCTOR: Sekarang menerima parameter List<DataBarang>
        public FormCetak(List<DataBarang> listData)
        {
            InitializeComponent();
            _listLaporan = listData; // Menyimpan data list dari FormRiwayat

            // Mengikat event Load form secara otomatis via kodingan
            this.Load += FormCetak_Load;
        }

        private void FormCetak_Load(object sender, EventArgs e)
        {
            try
            {
                // Instansiasi kertas desain Crystal Report
                CetakRiwayat rpt = new CetakRiwayat();

                // 3. Suntikkan List data ke dalam Crystal Report
                rpt.SetDataSource(_listLaporan);

                // Tampilkan kertas laporan ke komponen Viewer di layar
                crystalReportViewer1.ReportSource = rpt;
                crystalReportViewer1.RefreshReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat cetakan laporan:\n" + ex.Message,
                                "Error Cetak", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}