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

