using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions; // Diperlukan untuk mendeteksi karakter unik
using System.Windows.Forms;

namespace SI_Monotoring_Stok_Barang_Pada_TOKO_ELEKTRONIK
{
    public partial class Form3 : Form
    {
        private readonly string connectionString =
            @"Data Source=LAPTOP-ANV5L9LG\ALFA; Initial Catalog=DB_TokoElektronik; " +
            @"Integrated Security=True; TrustServerCertificate=True";

        private const int ID_MAX = 6;
        private const int NAMA_MIN = 2;
        private const int NAMA_MAX = 16;
        private const int ALAMAT_MAX = 24;
        private const int TELP_MIN = 9;
        private const int TELP_MAX = 13;

        private BindingSource _bindingSource = new BindingSource();

        public Form3()
        {
            InitializeComponent();

            dgvSupplier.AutoGenerateColumns = true;
            dgvSupplier.DataSource = null;
            dgvSupplier.DataSource = _bindingSource;

            bindingNavigator3.BindingSource = _bindingSource;

            // 1. Validasi ID Supplier (Hanya boleh Huruf dan Angka, TANPA karakter unik/spasi)
            txtIDSupp.KeyPress += (s, e) =>
            {
                if (!char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                {
                    e.Handled = true;
                }
            };

            // 2. Validasi No. Telepon (Hanya angka, backspace, +, dan -)
            txtTelp.KeyPress += (s, e) =>
            {
                if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back
                    && e.KeyChar != '+' && e.KeyChar != '-')
                    e.Handled = true;
            };

            // 3. Validasi Real-time Nama (Mencegah ketik karakter unik, hanya huruf, angka, dan spasi)
            txtNamaSupp.KeyPress += (s, e) =>
            {
                if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                {
                    e.Handled = true;
                }
            };

            // 4. Validasi Real-time Alamat (Mencegah ketik karakter unik selain huruf, angka, spasi, titik, koma, garis miring)
            txtAlamat.KeyPress += (s, e) =>
            {
                if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar) &&
                    e.KeyChar != (char)Keys.Back && e.KeyChar != '.' && e.KeyChar != ',' && e.KeyChar != '/')
                {
                    e.Handled = true;
                }
            };

            this.Shown += (s, e) => TampilSupplier();
        }

        private void Form3_Load(object sender, EventArgs e) { }

        // ==========================================
        //  METHOD TAMPIL SUPPLIER
        // ==========================================
        void TampilSupplier()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    new SqlDataAdapter(
                        "SELECT ID_Supplier AS [ID Supplier], Nama_Supplier AS [Nama Supplier], Alamat, No_Telp AS [No Telpon] FROM Supplier ORDER BY ID_Supplier",
                        conn).Fill(dt);

                    _bindingSource.DataSource = null;
                    _bindingSource.DataSource = dt;

                    dgvSupplier.AutoGenerateColumns = true;
                    dgvSupplier.DataSource = _bindingSource;
                    dgvSupplier.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Koneksi Gagal: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        //  METHOD VALIDASI INPUT (ANTI KARAKTER UNIK)
        // ==========================================
        private bool ValidasiInput(bool cekID = true)
        {
            string id = txtIDSupp.Text.Trim();
            string nama = txtNamaSupp.Text.Trim();
            string alamat = txtAlamat.Text.Trim();
            string telp = txtTelp.Text.Trim();

            if (cekID)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    MessageBox.Show(" ID Supplier tidak boleh kosong!", "Validasi",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtIDSupp.Focus(); return false;
                }
                if (id.Length > ID_MAX)
                {
                    MessageBox.Show($" ID Supplier maksimal {ID_MAX} karakter!", "Validasi",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtIDSupp.Focus(); return false;
                }
                // Proteksi Copy-Paste ID Supplier (Hanya huruf dan angka)
                if (!Regex.IsMatch(id, @"^[a-zA-Z0-9]+$"))
                {
                    MessageBox.Show("ID Supplier tidak boleh mengandung karakter unik, simbol, atau spasi!", "Validasi",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtIDSupp.Focus(); return false;
                }
            }

            // --- Validasi Nama Supplier ---
            if (string.IsNullOrWhiteSpace(nama))
            {
                MessageBox.Show(" Nama Supplier tidak boleh kosong!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNamaSupp.Focus(); return false;
            }
            if (nama.Length < NAMA_MIN || nama.Length > NAMA_MAX)
            {
                MessageBox.Show($"Nama Supplier harus {NAMA_MIN}–{NAMA_MAX} karakter!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNamaSupp.Focus(); return false;
            }
            // Proteksi Copy-Paste Nama (Hanya huruf, angka, dan spasi)
            if (!Regex.IsMatch(nama, @"^[a-zA-Z0-9 ]+$"))
            {
                MessageBox.Show("Nama Supplier tidak boleh mengandung karakter unik atau simbol!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNamaSupp.Focus(); return false;
            }

            // --- Validasi Alamat ---
            if (alamat.Length > ALAMAT_MAX)
            {
                MessageBox.Show($"Alamat maksimal {ALAMAT_MAX} karakter!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAlamat.Focus(); return false;
            }
            // Proteksi Copy-Paste Alamat (Perbaikan Variabel Error Terbaca Sistem)
            if (!string.IsNullOrEmpty(alamat) && !Regex.IsMatch(alamat, @"^[a-zA-Z0-9 .,/]+$"))
            {
                MessageBox.Show("Alamat tidak boleh mengandung karakter unik/simbol selain tanda baca (. , /)!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAlamat.Focus(); return false;
            }

            // --- Validasi No Telepon ---
            if (string.IsNullOrWhiteSpace(telp) || telp.Length < TELP_MIN || telp.Length > TELP_MAX)
            {
                MessageBox.Show($"No. Telepon harus {TELP_MIN}–{TELP_MAX} karakter!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTelp.Focus(); return false;
            }

            return true;
        }

        
        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (!ValidasiInput()) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("sp_InsertSupplier", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID_Supplier", txtIDSupp.Text.Trim().ToUpper());
                    cmd.Parameters.AddWithValue("@Nama_Supplier", txtNamaSupp.Text.Trim());
                    cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text.Trim());
                    cmd.Parameters.AddWithValue("@No_Telp", txtTelp.Text.Trim());

                    SqlParameter outRows = new SqlParameter("@RowsAffected", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(outRows);
                    cmd.ExecuteNonQuery();

                    if ((int)outRows.Value == -1)
                    {
                        MessageBox.Show($"❌ ID '{txtIDSupp.Text.Trim().ToUpper()}' sudah ada!",
                            "Duplikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    MessageBox.Show("✅ Supplier berhasil ditambahkan!", "Sukses",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    TampilSupplier();
                    BersihkanForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        //  BUTTON UPDATE SUPPLIER
        // ==========================================
        private void btnUpdate_Click(object sender, EventArgs e) => UpdateProses();

        void UpdateProses()
        {
            if (string.IsNullOrWhiteSpace(txtIDSupp.Text))
            {
                MessageBox.Show("⚠️ Pilih data di tabel terlebih dahulu!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!ValidasiInput(cekID: false)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("sp_UpdateSupplier", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID_Supplier", txtIDSupp.Text.Trim());
                    cmd.Parameters.AddWithValue("@Nama_Supplier", txtNamaSupp.Text.Trim());
                    cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text.Trim());
                    cmd.Parameters.AddWithValue("@No_Telp", txtTelp.Text.Trim());

                    SqlParameter outRows = new SqlParameter("@RowsAffected", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(outRows);
                    cmd.ExecuteNonQuery();

                    if ((int)outRows.Value > 0)
                    {
                        MessageBox.Show("✅ Supplier berhasil diubah!", "Sukses",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        TampilSupplier();
                        BersihkanForm();
                    }
                    else
                    {
                        MessageBox.Show("❌ ID tidak ditemukan!", "Gagal",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        //  BUTTON HAPUS SUPPLIER
        // ==========================================
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIDSupp.Text))
            {
                MessageBox.Show("⚠️ Pilih data di tabel terlebih dahulu!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(
                $"Hapus Supplier?\nID   : {txtIDSupp.Text}\nNama : {txtNamaSupp.Text}\n\nYakin?",
                "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                != DialogResult.Yes) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("sp_DeleteSupplier", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID_Supplier", txtIDSupp.Text.Trim());

                    SqlParameter outRows = new SqlParameter("@RowsAffected", SqlDbType.Int)
                    { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(outRows);
                    cmd.ExecuteNonQuery();

                    if ((int)outRows.Value > 0)
                    {
                        MessageBox.Show(" Supplier berhasil dihapus!", "Sukses",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        TampilSupplier();
                        BersihkanForm();
                    }
                    else
                    {
                        MessageBox.Show(" Data tidak ditemukan!", "Gagal",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        //  BUTTON BERSIHKAN FORM & KEMBALI
        // ==========================================
        private void btnClear_Click(object sender, EventArgs e) => BersihkanForm();

        void BersihkanForm()
        {
            txtIDSupp.Clear();
            txtNamaSupp.Clear();
            txtAlamat.Clear();
            txtTelp.Clear();
            txtIDSupp.ReadOnly = false;
            txtIDSupp.Focus();
        }

        private void btnBack_Click(object sender, EventArgs e) => this.Close();

        // ==========================================
        //  EVENT KLIK DATA GRID VIEW -> TEXTBOX
        // ==========================================nc
        private void dgvSupplier_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            try
            {
                DataGridViewRow row = dgvSupplier.Rows[e.RowIndex];
                txtIDSupp.Text = row.Cells["ID Supplier"].Value?.ToString() ?? "";
                txtNamaSupp.Text = row.Cells["Nama Supplier"].Value?.ToString() ?? "";
                txtAlamat.Text = row.Cells["Alamat"].Value?.ToString() ?? "";
                txtTelp.Text = row.Cells["No Telpon"].Value?.ToString() ?? "";

                txtIDSupp.ReadOnly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca baris: " + ex.Message);
            }
        }

        private void label1_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
    }
}