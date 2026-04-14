using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SI_Monotoring_Stok_Barang_Pada_TOKO_ELEKTRONIK
{
    public partial class Form1 : Form
    {
        string connectionString = @"Data Source=LAPTOP-ANV5L9LG\ALFA; Initial Catalog=DB_TokoElektronik; Integrated Security=True; TrustServerCertificate=True";

        public Form1()
        {
            InitializeComponent();
            TampilData();
        }

        // 1. Fungsi Tampil Data Barang Utama
        void TampilData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Barang";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;
            }
        }

        // 2. Tombol INSERT (Tambah Barang)
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Barang (ID_Barang, Nama_Barang, Kategori, Harga, Stok) VALUES (@id, @nm, @kt, @hr, @st)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", txtID.Text);
                    cmd.Parameters.AddWithValue("@nm", txtNama.Text);
                    cmd.Parameters.AddWithValue("@kt", txtKat.Text);
                    cmd.Parameters.AddWithValue("@hr", txtHarga.Text);
                    cmd.Parameters.AddWithValue("@st", txtStok.Text);
                    cmd.ExecuteNonQuery();

                    // Otomatis catat ke Riwayat
                    string queryRiwayat = "INSERT INTO Riwayat_Masuk (ID_Barang, Nama_Barang, Jumlah_Masuk, Keterangan) VALUES (@id, @nm, @st, 'Barang Baru')";
                    SqlCommand cmdLog = new SqlCommand(queryRiwayat, conn);
                    cmdLog.Parameters.AddWithValue("@id", txtID.Text);
                    cmdLog.Parameters.AddWithValue("@nm", txtNama.Text);
                    cmdLog.Parameters.AddWithValue("@st", txtStok.Text);
                    cmdLog.ExecuteNonQuery();

                    MessageBox.Show("Data Berhasil Disimpan!");
                }
                TampilData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // 3. Tombol UPDATE (Ubah Barang)
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Barang SET Nama_Barang=@nm, Kategori=@kt, Harga=@hr, Stok=@st WHERE ID_Barang=@id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", txtID.Text);
                    cmd.Parameters.AddWithValue("@nm", txtNama.Text);
                    cmd.Parameters.AddWithValue("@kt", txtKat.Text);
                    cmd.Parameters.AddWithValue("@hr", txtHarga.Text);
                    cmd.Parameters.AddWithValue("@st", txtStok.Text);
                    cmd.ExecuteNonQuery();

                    // Catat ke Riwayat saat Update
                    string queryRiwayat = "INSERT INTO Riwayat_Masuk (ID_Barang, Nama_Barang, Jumlah_Masuk, Keterangan) VALUES (@id, @nm, @st, 'Update Stok')";
                    SqlCommand cmdLog = new SqlCommand(queryRiwayat, conn);
                    cmdLog.Parameters.AddWithValue("@id", txtID.Text);
                    cmdLog.Parameters.AddWithValue("@nm", txtNama.Text);
                    cmdLog.Parameters.AddWithValue("@st", txtStok.Text);
                    cmdLog.ExecuteNonQuery();

                    MessageBox.Show("Sipp! Data berhasil diubah.");
                    txtID.ReadOnly = false;
                }
                TampilData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal update: " + ex.Message);
            }
        }

        // 4. Tombol DELETE
        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text))
            {
                MessageBox.Show("Pilih dulu data yang mau dihapus di tabel!");
                return;
            }

            if (MessageBox.Show("Yakin mau hapus barang ini?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM Barang WHERE ID_Barang=@id";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id", txtID.Text);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Data berhasil dibuang!");
                    }
                    button4_Click(sender, e); // Panggil fungsi Clear
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        // 5. Tombol CLEAR / BACK
        private void button4_Click(object sender, EventArgs e)
        {
            txtID.Clear();
            txtNama.Clear();
            txtKat.Clear();
            txtHarga.Clear();
            txtStok.Clear();
            txtID.ReadOnly = false;
            TampilData(); 
        }

        // 6. Tombol VIEW (Menampilkan Riwayat Log)
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Mengambil 5 kolom agar pas (ID, Nama, Jumlah, Ket, Tanggal)
                    string query = "SELECT ID_Barang, Nama_Barang, Jumlah_Masuk, Keterangan, Tanggal_Masuk FROM Riwayat_Masuk ORDER BY Tanggal_Masuk DESC";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                    MessageBox.Show("Menampilkan Riwayat (Log) Transaksi");
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal: " + ex.Message); }
        }

        
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            
            if (dataGridView1.ColumnCount > 5)
            {
                return; 
            }

            