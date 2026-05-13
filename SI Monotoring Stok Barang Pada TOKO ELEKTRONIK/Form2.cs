using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SI_Monotoring_Stok_Barang_Pada_TOKO_ELEKTRONIK
{
    public partial class Form2 : Form
    {
        private const int USERNAME_MIN = 3;
        private const int USERNAME_MAX = 50;
        private const int PASSWORD_MIN = 6;
        private const int PASSWORD_MAX = 50;

        private readonly string connectionString =
            @"Data Source=LAPTOP-ANV5L9LG\ALFA; Initial Catalog=DB_TokoElektronik; " +
            @"Integrated Security=True; TrustServerCertificate=True";

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            textBox1.MaxLength = USERNAME_MAX;
            textBox2.MaxLength = PASSWORD_MAX;
            textBox2.PasswordChar = '●';
            textBox1.Focus();
        }

        private bool ValidasiInput()
        {
            string username = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("⚠️ Username tidak boleh kosong!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus(); return false;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("⚠️ Password tidak boleh kosong!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox2.Focus(); return false;
            }
            if (username.Length < USERNAME_MIN)
            {
                MessageBox.Show(
                    $"⚠️ Username minimal {USERNAME_MIN} karakter!\n" +
                    $"Kamu memasukkan {username.Length} karakter.",
                    "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus(); return false;
            }
            if (password.Length < PASSWORD_MIN)
            {
                MessageBox.Show(
                    $"⚠️ Password minimal {PASSWORD_MIN} karakter!\n" +
                    $"Kamu memasukkan {password.Length} karakter.",
                    "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox2.Focus(); return false;
            }
            foreach (char c in username)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    MessageBox.Show(
                        "⚠️ Username hanya boleh huruf, angka, dan underscore (_)!\n" +
                        "Spasi dan simbol tidak diperbolehkan.",
                        "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox1.Focus(); return false;
                }
            }
            return true;
        }

        
      
      