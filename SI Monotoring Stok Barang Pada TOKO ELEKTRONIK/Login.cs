using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SI_Monotoring_Stok_Barang_Pada_TOKO_ELEKTRONIK
{
    public partial class Login : Form
    {
        private const int USERNAME_MIN = 3;
        private const int USERNAME_MAX = 16;
        private const int PASSWORD_MIN = 6;
        private const int PASSWORD_MAX = 16;

        private readonly string connectionString =
            @"Data Source=LAPTOP-ANV5L9LG\ALFA; Initial Catalog=DB_TokoElektronik; " +
            @"Integrated Security=True; TrustServerCertificate=True";

        public Login()
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

        
      
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (!ValidasiInput()) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("sp_Login", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Username", textBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@Password", textBox2.Text.Trim());

                    SqlParameter outValid = new SqlParameter("@IsValid", System.Data.SqlDbType.Bit);
                    outValid.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(outValid);

                    SqlParameter outRole = new SqlParameter("@Role", System.Data.SqlDbType.VarChar, 20);
                    outRole.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(outRole);

                    cmd.ExecuteNonQuery();

                    bool isValid = (bool)outValid.Value;
                    string role = outRole.Value?.ToString() ?? "";

                    if (isValid)
                    {
                        MessageBox.Show(
                            $"Selamat datang, {textBox1.Text.Trim()}!\n" +
                            $"{role}",
                            "Login Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        FormMenu frmMenu = new FormMenu(textBox1.Text.Trim(), role);
                        frmMenu.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Username atau Password salah!\nSilakan coba lagi.",
                            "Login Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBox2.Clear();
                        textBox2.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Koneksi gagal!\nDetail: " + ex.Message,
                    "Error Koneksi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) textBox2.Focus();
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) button1_Click(sender, e);
        }
    }
}