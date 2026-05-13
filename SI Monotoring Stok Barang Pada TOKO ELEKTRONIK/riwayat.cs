using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SI_Monotoring_Stok_Barang_Pada_TOKO_ELEKTRONIK
{
    public partial class FormRiwayat : Form
    {
      
        private readonly string connectionString =
            @"Data Source=LAPTOP-ANV5L9LG\ALFA; Initial Catalog=DB_TokoElektronik; " +
            @"Integrated Security=True; TrustServerCertificate=True";

        private DataTable _dtSemua = new DataTable();
        private BindingSource _bindingSource = new BindingSource();
        private bool _modeInjection = false;

       