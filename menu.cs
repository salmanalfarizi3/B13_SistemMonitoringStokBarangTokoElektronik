using System;
using System.Windows.Forms;

namespace SI_Monotoring_Stok_Barang_Pada_TOKO_ELEKTRONIK
{
    // ============================================================
    //  FORM MENU / DASHBOARD UTAMA
    //  Muncul setelah login berhasil.
    //  Dari sini user pilih mau ke mana:
    //  1. Kelola Supplier (harus duluan)
    //  2. Kelola Barang
    //  3. Lihat Riwayat
    //  4. Logout
    // ============================================================
    public partial class FormMenu : Form
    {
        private string _namaUser; // Simpan nama user yang login
        private string _role;     // Simpan role (Admin / Kasir)

        public FormMenu(string namaUser, string role)
        {
            InitializeComponent();
            _namaUser = namaUser;
            _role = role;
        }

        private void FormMenu_Load(object sender, EventArgs e)
        {
            // Tampilkan sapaan di label
            lblSambutan.Text = $"Selamat datang, {_namaUser}!  |  Role: {_role}";

            // Jika role Kasir, sembunyikan tombol yang tidak relevan
            if (_role == "Kasir")
            {
                btnSupplier.Enabled = false; // Kasir tidak boleh kelola supplier
            }
        }

        // ============================================================
        //  TOMBOL 1 - KELOLA SUPPLIER
        //  Logika: Supplier diisi DULU sebelum barang
        // ============================================================
        private void btnSupplier_Click(object sender, EventArgs e)
        {
            Form3 frmSupplier = new Form3();
            frmSupplier.ShowDialog(); // Modal: harus tutup dulu baru balik ke menu
        }

        // ============================================================
        //  TOMBOL 2 - KELOLA BARANG
        // ============================================================
        private void btnBarang_Click(object sender, EventArgs e)
        {
            Form1 frmBarang = new Form1();
            frmBarang.ShowDialog();
        }

        // ============================================================
        //  TOMBOL 3 - LIHAT RIWAYAT
        // ============================================================
        private void btnRiwayat_Click(object sender, EventArgs e)
        {
            Form1 frmBarang = new Form1();
            // Langsung buka halaman riwayat
            frmBarang.ShowDialog();
        }

        // ============================================================
        //  TOMBOL 4 - LOGOUT
        // ============================================================
        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Yakin ingin logout?", "Konfirmasi",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Form2 frmLogin = new Form2();
                frmLogin.Show();
                this.Close();
            }
        }
    }
}