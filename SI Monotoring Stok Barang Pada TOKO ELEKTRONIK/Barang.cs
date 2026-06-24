using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions; // Ditambahkan untuk mendukung fungsi Regex
using System.Windows.Forms;

namespace SI_Monotoring_Stok_Barang_Pada_TOKO_ELEKTRONIK
{
    public partial class Form1 : Form
    {
        private readonly string connectionString =
            @"Data Source=LAPTOP-ANV5L9LG\ALFA; Initial Catalog=DB_TokoElektronik; " +
            @"Integrated Security=True; TrustServerCertificate=True";

        private BindingSource _bindingSource = new BindingSource();
        private bool _sedangTampilRiwayat = false;

        public Form1()
        {
            InitializeComponent();

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = _bindingSource;
            bindingNavigator1.BindingSource = _bindingSource;

            this.Load += (s, e) =>
            {
                PastikanViewAda();
                MuatSupplierKeComboBox();
                TampilData();
                TampilTotalBarang();
            };

            txtHarga.KeyPress += ValidasiAngka;
            txtStok.KeyPress += ValidasiAngkaBulat;

            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
        }


        //  MUAT SUPPLIER KE COMBOBOX

        void MuatSupplierKeComboBox()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    new SqlDataAdapter(
                        "SELECT ID_Supplier, Nama_Supplier FROM Supplier ORDER BY Nama_Supplier",
                        conn).Fill(dt);

                    DataRow baris = dt.NewRow();
                    baris["ID_Supplier"] = "";
                    baris["Nama_Supplier"] = "-- Pilih Supplier --";
                    dt.Rows.InsertAt(baris, 0);

                    cmbSupplier.DataSource = dt;
                    cmbSupplier.DisplayMember = "Nama_Supplier";
                    cmbSupplier.ValueMember = "ID_Supplier";
                    cmbSupplier.SelectedIndex = 0;
                }
            }
            catch
            {
                if (cmbSupplier.Items.Count == 0)
                {
                    cmbSupplier.Items.Add("-- Tidak ada supplier --");
                    cmbSupplier.SelectedIndex = 0;
                }
            }
        }


        //  PastikanViewAda() — TAMBAH KOLOM Status Stok
        //  Sesuaikan dengan ALTER VIEW di database SQL

        void PastikanViewAda()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Hapus view lama jika ada
                    new SqlCommand(
                        "IF EXISTS (SELECT 1 FROM sys.views WHERE name='vw_Barang') DROP VIEW vw_Barang;",
                        conn).ExecuteNonQuery();


                    string sqlView = @"
                        CREATE VIEW vw_Barang AS
                        SELECT 
                            b.ID_Barang      AS [ID Barang], 
                            b.Nama_Barang    AS [Nama Barang],
                            b.Kategori, 
                            b.Harga, 
                            b.Stok,
                            ISNULL(s.Nama_Supplier, '-') AS [Nama Supplier],
                            CASE
                                WHEN b.Stok = 0  THEN 'Habis'
                                WHEN b.Stok < 5  THEN 'Menipis'
                                WHEN b.Stok < 20 THEN 'Normal'
                                ELSE                  'Aman'
                            END AS [Status Stok]
                        FROM Barang b
                        LEFT JOIN Supplier s ON b.ID_Supplier = s.ID_Supplier;";

                    new SqlCommand(sqlView, conn).ExecuteNonQuery();
                }
            }
            catch { }
        }


        //  TAMPIL DATA BARANG (menggunakan VIEW)

        void TampilData()
        {
            _sedangTampilRiwayat = false;
            button4.Text = "Bersihkan";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    new SqlDataAdapter(
                        "SELECT * FROM vw_Barang ORDER BY [ID Barang]",
                        conn).Fill(dt);

                    _bindingSource.DataSource = null;
                    _bindingSource.DataSource = dt;
                    dataGridView1.DataSource = _bindingSource;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Gagal memuat data: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //  PERBAIKAN 2: TampilRiwayat() — Gunakan vw_RiwayatMasuk

        void TampilRiwayat(string idBarang, string namaBarang)
        {
            _sedangTampilRiwayat = true;
            button4.Text = "◀ Kembali ke Data Barang";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();


                    string sql = @"
                        SELECT * FROM vw_RiwayatMasuk
                        WHERE [ID Barang] = @ID
                        ORDER BY [ID Riwayat] DESC";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ID", idBarang);

                    DataTable dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);

                    _bindingSource.DataSource = null;
                    _bindingSource.DataSource = dt;
                    dataGridView1.DataSource = _bindingSource;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Gagal memuat riwayat: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //  TAMPIL TOTAL BARANG (menggunakan Stored Procedure)

        void TampilTotalBarang()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("sp_CountBarang", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter p = new SqlParameter("@Total", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();
                    this.Text = "MAXIMA ELECTRONICA | Total Barang: " + p.Value;
                }
            }
            catch { }
        }


        //  TOMBOL TAMBAH (sp_InsertBarang)

        private void button1_Click(object sender, EventArgs e)
        {
            if (!ValidasiInput(cekID: true)) return;

            string idSupplier = "";
            if (cmbSupplier.SelectedValue != null)
                idSupplier = cmbSupplier.SelectedValue.ToString();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("sp_InsertBarang", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID_Barang", txtID.Text.Trim().ToUpper());
                    cmd.Parameters.AddWithValue("@Nama_Barang", txtNama.Text.Trim());
                    cmd.Parameters.AddWithValue("@Kategori", txtKat.Text.Trim());
                    cmd.Parameters.AddWithValue("@Harga", decimal.Parse(txtHarga.Text.Replace(",", "")));
                    cmd.Parameters.AddWithValue("@Stok", int.Parse(txtStok.Text));
                    cmd.Parameters.AddWithValue("@ID_Supplier", idSupplier);

                    // Menyesuaikan parameter output RowsAffected yang dilempar dari Stored Procedure
                    SqlParameter outRows = new SqlParameter("@RowsAffected", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(outRows);
                    cmd.ExecuteNonQuery();

                    if (outRows.Value != DBNull.Value && (int)outRows.Value == -1)
                    {
                        MessageBox.Show(" ID Barang sudah ada!", "Duplikat",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    MessageBox.Show(" Barang berhasil ditambahkan!", "Sukses",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    TampilData();
                    TampilTotalBarang();
                    BersihkanForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(" Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //  TOMBOL UPDATE (sp_UpdateBarang)

        private void button2_Click(object sender, EventArgs e)
        {
            if (!ValidasiInput(cekID: false)) return; // Diubah ke false jika ID bersifat ReadOnly saat update

            string idSupplier = "";
            if (cmbSupplier.SelectedValue != null)
                idSupplier = cmbSupplier.SelectedValue.ToString();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("sp_UpdateBarang", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID_Barang", txtID.Text.Trim());
                    cmd.Parameters.AddWithValue("@Nama_Barang", txtNama.Text.Trim());
                    cmd.Parameters.AddWithValue("@Kategori", txtKat.Text.Trim());
                    cmd.Parameters.AddWithValue("@Harga", decimal.Parse(txtHarga.Text.Replace(",", "")));
                    cmd.Parameters.AddWithValue("@Stok", int.Parse(txtStok.Text));
                    cmd.Parameters.AddWithValue("@ID_Supplier", idSupplier);

                    SqlParameter outRows = new SqlParameter("@RowsAffected", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(outRows);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show(" Barang berhasil diperbarui!", "Sukses",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    TampilData();
                    BersihkanForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //  TOMBOL HAPUS (sp_DeleteBarang)

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                MessageBox.Show("Pilih barang terlebih dahulu.", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Yakin ingin menghapus barang ini?", "Konfirmasi",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("sp_DeleteBarang", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID_Barang", txtID.Text.Trim());

                    SqlParameter outRows = new SqlParameter("@RowsAffected", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(outRows);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Barang berhasil dihapus!", "Sukses",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    TampilData();
                    TampilTotalBarang();
                    BersihkanForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(" Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnCari_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCari.Text)) { TampilData(); return; }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Stored Procedure sp_SearchBarang
                    SqlCommand cmd = new SqlCommand("sp_SearchBarang", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Keyword", txtCari.Text.Trim());

                    DataTable dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);

                    _bindingSource.DataSource = null;
                    _bindingSource.DataSource = dt;
                    dataGridView1.DataSource = _bindingSource;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //  KLIK BARIS GRID (single click → isi textbox)

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _sedangTampilRiwayat) return;

            try
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                txtID.Text = row.Cells["ID Barang"].Value?.ToString() ?? "";
                txtNama.Text = row.Cells["Nama Barang"].Value?.ToString() ?? "";
                txtKat.Text = row.Cells["Kategori"].Value?.ToString() ?? "";
                txtHarga.Text = row.Cells["Harga"].Value?.ToString() ?? "";
                txtStok.Text = row.Cells["Stok"].Value?.ToString() ?? "";
                txtID.ReadOnly = true;

                // Mencocokkan ComboBox dengan Supplier milik barang yang dipilih
                string namaSupplierGrid = row.Cells["Nama Supplier"].Value?.ToString() ?? "-";
                if (namaSupplierGrid == "-")
                {
                    cmbSupplier.SelectedIndex = 0;
                }
                else
                {
                    cmbSupplier.SelectedIndex = cmbSupplier.FindStringExact(namaSupplierGrid);
                    if (cmbSupplier.SelectedIndex == -1) cmbSupplier.SelectedIndex = 0;
                }
            }
            catch { }
        }


        //  DOUBLE CLICK BARIS → Tampil Riwayat barang tersebut

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _sedangTampilRiwayat) return;

            try
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                string idBarang = row.Cells["ID Barang"].Value?.ToString() ?? "";
                string namaBarang = row.Cells["Nama Barang"].Value?.ToString() ?? "";
                if (!string.IsNullOrWhiteSpace(idBarang))
                    TampilRiwayat(idBarang, namaBarang);
            }
            catch { }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (_sedangTampilRiwayat)
                TampilData();
            else
                BersihkanForm();
        }

        void BersihkanForm()
        {
            txtID.Clear();
            txtNama.Clear();
            txtKat.Clear();
            txtHarga.Clear();
            txtStok.Clear();
            txtID.ReadOnly = false;
            if (cmbSupplier.Items.Count > 0)
                cmbSupplier.SelectedIndex = 0;
            txtID.Focus();
        }

        private void btnKembali_Click(object sender, EventArgs e) => this.Close();

        private void ValidasiAngka(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back && e.KeyChar != '.')
                // Membiarkan input angka, backspace, dan titik desimal
                e.Handled = true;
        }

        private void ValidasiAngkaBulat(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                e.Handled = true;
        }

        // METHOD VALIDASI BARU (ANTI-SIMBOL & HARUS PILIH SUPPLIER)
        private bool ValidasiInput(bool cekID)
        {
            // 1. Validasi Input Kosong
            if (cekID && string.IsNullOrWhiteSpace(txtID.Text))
            {
                MessageBox.Show("ID Barang tidak boleh kosong!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("Nama Barang tidak boleh kosong!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtHarga.Text))
            {
                MessageBox.Show("Harga tidak boleh kosong!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtStok.Text))
            {
                MessageBox.Show("Stok tidak boleh kosong!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 2. Validasi Pilih Supplier (Menolak "-- Pilih Supplier --")
            if (cmbSupplier.SelectedValue == null || string.IsNullOrEmpty(cmbSupplier.SelectedValue.ToString()))
            {
                MessageBox.Show("Silakan pilih Supplier terlebih dahulu!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 3. Validasi Blokir Karakter Unik (Hanya Alfanumerik dan Spasi)
            Regex regexAlfanumerik = new Regex(@"^[a-zA-Z0-9\s]+$");

            if (cekID && !regexAlfanumerik.IsMatch(txtID.Text))
            {
                MessageBox.Show("ID Barang tidak boleh mengandung simbol/karakter unik!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!regexAlfanumerik.IsMatch(txtNama.Text))
            {
                MessageBox.Show("Nama Barang tidak boleh mengandung simbol/karakter unik!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            // Kategori opsional, namun jika diisi harus berupa alfanumerik
            if (!string.IsNullOrWhiteSpace(txtKat.Text) && !regexAlfanumerik.IsMatch(txtKat.Text))
            {
                MessageBox.Show("Kategori tidak boleh mengandung simbol/karakter unik!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}