using System;
using System.Collections.Generic; // TAMBAHAN: Agar bisa menggunakan List
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

        private void btnCari_Click(object sender, EventArgs e)
        {
            if (_modeInjection) return;

            string keyword = txtCari.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                _bindingSource.DataSource = _dtSemua;
                return;
            }

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
                        WHERE r.ID_Barang LIKE @keyword OR r.Nama_Barang LIKE @keyword
                        ORDER BY r.ID_Riwayat DESC";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                    DataTable dtHasil = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dtHasil);

                    _bindingSource.DataSource = dtHasil;
                    lblTotal.Text = $"Ditemukan: {dtHasil.Rows.Count} transaksi";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mencari data: " + ex.Message);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtCari.Clear();
            MuatRiwayat();
        }

        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            if (!_modeInjection) JalankanInjection();
            else btnReset_Click(null, null);
        }

        void JalankanInjection()
        {
            int indeksDipilih = dgvRiwayat.CurrentRow != null ? dgvRiwayat.CurrentRow.Index : -1;

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
                ORDER BY r.ID_Riwayat DESC";

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

                        DataTable dtHacked = new DataTable();
                        foreach (DataColumn col in dt.Columns)
                        {
                            dtHacked.Columns.Add(col.ColumnName, typeof(string));
                        }

                        int barisKe = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            DataRow nr = dtHacked.NewRow();
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                string colName = dt.Columns[i].ColumnName;

                                if (barisKe == indeksDipilih && (colName == "No" || colName == "ID Barang" || colName == "Nama Barang"))
                                {
                                    nr[i] = "HACKED 🛑";
                                }
                                else
                                {
                                    nr[i] = row[i] == DBNull.Value || string.IsNullOrWhiteSpace(row[i].ToString())
                                        ? "-"
                                        : row[i].ToString();
                                }
                            }
                            dtHacked.Rows.Add(nr);
                            barisKe++;
                        }

                        _bindingSource.DataSource = dtHacked;

                        lblTotal.Text = "⚠️ SQL INJECTION SUCCESS (Selected Row Affected)";

                        if (indeksDipilih >= 0 && indeksDipilih < dgvRiwayat.Rows.Count)
                        {
                            dgvRiwayat.ClearSelection();
                            dgvRiwayat.Rows[indeksDipilih].Selected = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Injeksi: " + ex.Message);
            }
        }

        private void btnKembali_Click(object sender, EventArgs e) => this.Close();

        private void btnExportRiwayat_Click(object sender, EventArgs e)
        {
            if (dgvRiwayat.SelectedCells.Count == 0)
            {
                MessageBox.Show("Silakan blok kolom/sel yang ingin diexport terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<int> indeksKolomTerpilih = new List<int>();
            foreach (DataGridViewCell cell in dgvRiwayat.SelectedCells)
            {
                if (!indeksKolomTerpilih.Contains(cell.ColumnIndex))
                {
                    indeksKolomTerpilih.Add(cell.ColumnIndex);
                }
            }
            indeksKolomTerpilih.Sort();

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                FileName = "Laporan_Kustom_Riwayat_Barang.csv",
                Title = "Simpan Blok Laporan Excel"
            };

            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(saveFileDialog.FileName, false, System.Text.Encoding.UTF8))
                {
                    sw.WriteLine("sep=,");

                    for (int i = 0; i < indeksKolomTerpilih.Count; i++)
                    {
                        string headerText = dgvRiwayat.Columns[indeksKolomTerpilih[i]].HeaderText;
                        sw.Write($"\"{headerText}\"");
                        if (i < indeksKolomTerpilih.Count - 1) sw.Write(",");
                    }
                    sw.WriteLine();

                    foreach (DataGridViewRow row in dgvRiwayat.Rows)
                    {
                        if (row.IsNewRow) continue;

                        bool barisIkutTerblok = false;
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (cell.Selected)
                            {
                                barisIkutTerblok = true;
                                break;
                            }
                        }

                        if (!barisIkutTerblok) continue;

                        for (int j = 0; j < indeksKolomTerpilih.Count; j++)
                        {
                            int colIndex = indeksKolomTerpilih[j];
                            string nilaiSel = row.Cells[colIndex].Value?.ToString() ?? "";
                            nilaiSel = nilaiSel.Replace("\"", "\"\"");

                            sw.Write($"\"{nilaiSel}\"");
                            if (j < indeksKolomTerpilih.Count - 1) sw.Write(",");
                        }
                        sw.WriteLine();
                    }
                }

                MessageBox.Show("Sukses! Hanya kolom yang diblok saja yang berhasil diexport.",
                    "Berhasil", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Gagal mengexport data.\nError: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // BAGIAN YANG DIUBAH: DGVPrinter diganti dengan Pemanggil Crystal Reports
        private void btnCetak_Click(object sender, EventArgs e)
        {
            List<DataBarang> listData = new List<DataBarang>();

            foreach (DataGridViewRow row in dgvRiwayat.Rows)
            {
                if (!row.IsNewRow)
                {
                    DataBarang brg = new DataBarang();

                    // Menyesuaikan dengan nama kolom hasil SQL (ID Barang, Nama Barang, Jumlah Masuk, Keterangan, Tanggal)
                    brg.ID_Barang = row.Cells["ID Barang"].Value?.ToString() ?? "";
                    brg.Nama_Barang = row.Cells["Nama Barang"].Value?.ToString() ?? "";
                    brg.Jumlah = row.Cells["Jumlah Masuk"].Value != DBNull.Value ? Convert.ToInt32(row.Cells["Jumlah Masuk"].Value) : 0;
                    brg.Keterangan = row.Cells["Keterangan"].Value?.ToString() ?? "";

                    // Mengubah format string Tanggal (dd-MM-yyyy) kembali menjadi DateTime objek
                    if (row.Cells["Tanggal"].Value != null)
                    {
                        string tglStr = row.Cells["Tanggal"].Value.ToString();
                        if (DateTime.TryParseExact(tglStr, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime tgl))
                        {
                            brg.Tanggal = tgl;
                        }
                        else
                        {
                            brg.Tanggal = DateTime.Now;
                        }
                    }

                    listData.Add(brg);
                }
            }

            // Membuka FormCetak Crystal Report sambil mengirimkan datanya
            FormCetak halamanCetak = new FormCetak(listData);
            halamanCetak.Show();
        }
    }
}