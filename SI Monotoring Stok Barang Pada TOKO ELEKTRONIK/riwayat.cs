using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SI_Monotoring_Stok_Barang_Pada_TOKO_ELEKTRONIK
{
    public partial class FormRiwayat : Form
    {
        // Ganti Data Source sesuai dengan PC kamu
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

        // 2. TOMBOL CARI & RESET
        private void btnCari_Click(object sender, EventArgs e)
        {
            if (_modeInjection) return;
            string keyword = txtCari.Text.Trim().Replace("'", "''");
            DataView dv = new DataView(_dtSemua);
            if (!string.IsNullOrWhiteSpace(keyword))
                dv.RowFilter = $"[ID Barang] LIKE '%{keyword}%' OR [Nama Barang] LIKE '%{keyword}%'";
            _bindingSource.DataSource = dv.ToTable();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtCari.Clear();
            MuatRiwayat();
        }

        // 3. TOMBOL TEST INJECTION (PASTI ADA TULISAN HACKED)
        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            if (!_modeInjection) JalankanInjection();
            else btnReset_Click(null, null);
        }

        void JalankanInjection()
        {
            // Jika kosong, otomatis isi payload injection
            if (string.IsNullOrWhiteSpace(txtCari.Text))
            {
                txtCari.Text = "' OR 1=1 --";
            }

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
                WHERE r.Nama_Barang = '" + txtCari.Text + "'";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        _modeInjection = true;

                        // Buat tabel bayangan untuk tampilan HACKED
                        DataTable dtHacked = dt.Clone();
                        foreach (DataColumn col in dtHacked.Columns) col.DataType = typeof(string);

                        // Isi semua baris hasil query dengan tulisan HACKED pada kolom yang diminta
                        foreach (DataRow row in dt.Rows)
                        {
                            DataRow nr = dtHacked.NewRow();
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                string colName = dt.Columns[i].ColumnName;
                                // GANTI 3 KOLOM INI JADI HACKED
                                if (colName == "No" || colName == "ID Barang" || colName == "Nama Barang")
                                {
                                    nr[i] = "HACKED";
                                }
                                else
                                {
                                    nr[i] = row[i].ToString();
                                }
                            }
                            dtHacked.Rows.Add(nr);
                        }

                        _bindingSource.DataSource = dtHacked;
                        lblTotal.Text = "⚠️ SQL INJECTION SUCCESS!";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Injeksi: " + ex.Message);
            }
        }

        private void btnKembali_Click(object sender, EventArgs e) => this.Close();
    }
}