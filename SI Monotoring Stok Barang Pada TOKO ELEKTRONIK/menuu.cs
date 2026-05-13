using System;
using System.Windows.Forms;

namespace SI_Monotoring_Stok_Barang_Pada_TOKO_ELEKTRONIK
{
    public partial class FormMenu : Form
    {
        private string _namaUser;
        private string _role;
        private Timer _idleTimer = new Timer();
        private const int IDLE_MENIT = 5;

        public FormMenu(string namaUser, string role)
        {
            InitializeComponent();
            _namaUser = namaUser;
            _role = role;
        }

        private void FormMenu_Load_1(object sender, EventArgs e)
        {
     
        }

       
        //  IDLE TIMER — Auto Logout setelah 5 menit tidak aktif
        
        void MulaiIdleTimer()
        {
            _idleTimer.Interval = IDLE_MENIT * 60 * 1000; // 5 menit
            _idleTimer.Tick += IdleTimer_Tick;
            _idleTimer.Start();
        }

        void IdleTimer_Tick(object sender, EventArgs e)
        {
            _idleTimer.Stop();
            MessageBox.Show(
                $"⏰ Sesi kamu sudah habis!\n\n" +
                $"Tidak ada aktivitas selama {IDLE_MENIT} menit.\n" +
                $"Kamu akan dikembalikan ke halaman login.",
                "Auto Logout", MessageBoxButtons.OK, MessageBoxIcon.Information);
            AutoLogout();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            ResetIdleTimer();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            ResetIdleTimer();
        }

        void ResetIdleTimer()
        {
            _idleTimer.Stop();
            _idleTimer.Start();
        }

        void AutoLogout()
        {
            _idleTimer.Stop();
            _idleTimer.Dispose();
            Form2 frmLogin = new Form2();
            frmLogin.Show();
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult konfirm = MessageBox.Show(
                    "Tutup aplikasi?\n\nKamu akan otomatis logout.",
                    "Keluar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (konfirm == DialogResult.No) { e.Cancel = true; return; }
                _idleTimer.Stop();
                Application.Exit();
            }
        }

       
        //  TOMBOL 1 - KELOLA SUPPLIER
       
        private void btnSupplier_Click(object sender, EventArgs e)
        {
            ResetIdleTimer();
            Form3 frmSupplier = new Form3();
            frmSupplier.ShowDialog();
        }

       
        //  TOMBOL 2 - KELOLA BARANG
       
        private void btnBarang_Click(object sender, EventArgs e)
        {
            ResetIdleTimer();
            Form1 frmBarang = new Form1();
            frmBarang.ShowDialog();
        }

        
        //  TOMBOL 3 - LIHAT RIWAYAT
        
        private void btnRiwayat_Click(object sender, EventArgs e)
        {
            ResetIdleTimer();
            try
            {
                FormRiwayat frmRiwayat = new FormRiwayat();
                frmRiwayat.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Gagal membuka riwayat: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}