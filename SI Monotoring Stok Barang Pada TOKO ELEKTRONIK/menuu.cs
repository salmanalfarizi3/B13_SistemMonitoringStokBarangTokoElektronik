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
