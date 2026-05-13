using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SI_Monotoring_Stok_Barang_Pada_TOKO_ELEKTRONIK
{
    public partial class FormRiwayat : Form
    {
      
        private readonly string connectionString =
            @"Data Source=LAPTOP-ANV5L9LG\ALFA; Initial Catalog=DB_TokoElektronik; " +
            @"Integrated Security=True; TrustServerCertificate=True";

        private DataTable _dtSemua = new DataTable();
        private BindingSource _bindingSource = new BindingSource();
        private bool _modeInjection = false;

        public FormRiwayat()
        {
            InitializeComponent();
            dgvRiwayat.AutoGenerateColumns = true;
            dgvRiwayat.DataSource = _bindingSource;
            bindingNavigator1.BindingSource = _bindingSource;

            this.KeyPreview = true;
            this.Shown += (s, e) => MuatRiwayat();
        }

        // 1. LOAD DATA NORMAL
        void MuatRiwayat()
        {
            _modeInjection = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT
                            r.ID_Riwayat                                AS [No],
                            r.ID_Barang                                 AS [ID Barang],
                            r.Nama_Barang                               AS [Nama Barang],
                            r.Jumlah_Masuk                              AS [Jumlah Masuk],
                            r.Keterangan                                AS [Keterangan],
                            ISNULL(s.Nama_Supplier, '-')                AS [Nama Supplier],
                            CONVERT(VARCHAR, r.Tanggal_Masuk, 105)      AS [Tanggal],
                            CONVERT(VARCHAR, r.Tanggal_Masuk, 108)      AS [Jam]
                        FROM Riwayat_Masuk r
                        LEFT JOIN Supplier s ON r.ID_Supplier = s.ID_Supplier
                        ORDER BY r.ID_Riwayat DESC";

                    _dtSemua = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                    da.Fill(_dtSemua);

                    _bindingSource.DataSource = _dtSemua;
                    dgvRiwayat.DefaultCellStyle.BackColor = Color.White;
                    dgvRiwayat.DefaultCellStyle.ForeColor = Color.Black;
                    lblTotal.Text = $"Total: {_dtSemua.Rows.Count} transaksi";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal muat data: " + ex.Message);
            }
        }

       