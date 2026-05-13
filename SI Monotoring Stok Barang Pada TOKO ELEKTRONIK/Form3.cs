using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SI_Monotoring_Stok_Barang_Pada_TOKO_ELEKTRONIK
{
    public partial class Form3 : Form
    {
        private readonly string connectionString =
            @"Data Source=LAPTOP-ANV5L9LG\ALFA; Initial Catalog=DB_TokoElektronik; " +
            @"Integrated Security=True; TrustServerCertificate=True";

        private const int ID_MAX = 10;
        private const int NAMA_MIN = 2;
        private const int NAMA_MAX = 100;
        private const int ALAMAT_MAX = 200;
        private const int TELP_MIN = 9;
        private const int TELP_MAX = 15;

        private BindingSource _bindingSource = new BindingSource();

        public Form3()
        {
            InitializeComponent();

       
            dgvSupplier.AutoGenerateColumns = true;

            dgvSupplier.DataSource = null;
            dgvSupplier.DataSource = _bindingSource;

            bindingNavigator3.BindingSource = _bindingSource;

            // Validasi No. Telepon
            txtTelp.KeyPress += (s, e) =>
            {
                if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back
                    && e.KeyChar != '+' && e.KeyChar != '-')
                    e.Handled = true;
            };

            this.Shown += (s, e) => TampilSupplier();
        }

        private void Form3_Load(object sender, EventArgs e) { }

      
        //  TAMPIL SUPPLIER
        
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

        //  VALIDASI INPUT
 
        private bool ValidasiInput(bool cekID = true)
        {
            string id = txtIDSupp.Text.Trim();
            string nama = txtNamaSupp.Text.Trim();
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
            }

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
            if (txtAlamat.Text.Trim().Length > ALAMAT_MAX)
            {
                MessageBox.Show($"Alamat maksimal {ALAMAT_MAX} karakter!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAlamat.Focus(); return false;
            }
            if (string.IsNullOrWhiteSpace(telp) || telp.Length < TELP_MIN || telp.Length > TELP_MAX)
            {
                MessageBox.Show($"No. Telepon harus {TELP_MIN}–{TELP_MAX} karakter!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTelp.Focus(); return false;
            }

            return true;
        }

        
        //  TAMBAH SUPPLIER
        
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

        //  UPDATE SUPPLIER
        
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

                //  HAPUS SUPPLIER
        
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

        
        //  BERSIHKAN FORM
        
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

        
        //  KLIK BARIS GRID → ISI TEXTBOX
        
        private void dgvSupplier_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            try
            {
                DataGridViewRow row = dgvSupplier.Rows[e.RowIndex];
                // Nama kolom sesuai alias di query: [ID Supplier], [Nama Supplier], dll
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