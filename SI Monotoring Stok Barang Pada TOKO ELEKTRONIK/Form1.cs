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
              