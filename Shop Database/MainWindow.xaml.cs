using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Shop_Database
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataTable table;
        SqlConnection conn;
        string cs = "";
        SqlDataReader reader;
        DataSet dataset;
        SqlDataAdapter adapter;
        DataRowView datarow;
        string ProductName;
        string ProductId;
        string price;
        string Description;
        int count;
        int count1;
        string customerid;

        public MainWindow()
        {
            InitializeComponent();
            conn = new SqlConnection();
            cs = ConfigurationManager.ConnectionStrings["MyConnString"].ConnectionString;
            Combo_box.Items.Add("Products");
            Combo_box.Items.Add("Orders");
            Combo_box.Items.Add("Customers");
            Combo_box.Items.Add("OrderDetails");
        }

        public void RefreshTable()
        {
            using (conn = new SqlConnection())
            {
                MeinGirid.ItemsSource = null;
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(adapter);
                adapter.Fill(dataset);

                MeinGirid.ItemsSource = dataset.Tables[0].DefaultView;
                commandBuilder.GetUpdateCommand();
            }
        }

        private void Combo_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (conn = new SqlConnection())
            {
                conn.ConnectionString = cs;
                dataset = new DataSet();
                adapter = new SqlDataAdapter($@"select * from {Combo_box.SelectedItem.ToString()}",conn);
                RefreshTable();
            }
        }

        private void MeinGirid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            datarow = MeinGirid.SelectedItem as DataRowView;

            try
            {
                if (Combo_box.SelectedItem.ToString() == "Products")
                {
                    if (datarow != null)
                    {
                        ProductId = datarow["Id"].ToString();
                        ProductName = datarow["Name"].ToString();
                        price = datarow["Price"].ToString();
                        Description = datarow["Description"].ToString();
                    }
                }
                else if (Combo_box.SelectedItem.ToString() == "Customers")
                {
                    if (datarow != null)
                    {
                        customerid = datarow["Id"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Buy_btn_Click(object sender, RoutedEventArgs e)
        {
            using (conn = new SqlConnection())
            {
                conn.ConnectionString = cs;
                conn.Open();

                ++count;
                SqlCommand comm = new SqlCommand("sp_InsertOrder", conn);
                comm.CommandType = CommandType.StoredProcedure;

                var p1 = new SqlParameter();
                p1.Value = count;
                p1.ParameterName = "@OrderId";
                p1.SqlDbType = SqlDbType.Int;

                var p2 = new SqlParameter();
                p2.Value = int.Parse(ProductId);
                p2.ParameterName = "@ProductId";
                p2.SqlDbType = SqlDbType.Int;

                var p3 = new SqlParameter();
                p3.Value = 1;
                p3.ParameterName = "@CustomerId";
                p3.SqlDbType = SqlDbType.Int;

                comm.Parameters.Add(p1);
                comm.Parameters.Add(p2);
                comm.Parameters.Add(p3);

                comm.ExecuteNonQuery();
                ++count1;

                SqlCommand command = new SqlCommand("sp_InsertOrderDetails", conn);
                command.CommandType = CommandType.StoredProcedure;

                var v2p1 = new SqlParameter();
                v2p1.Value = ProductName;
                v2p1.ParameterName = "@ProductName";
                v2p1.SqlDbType = SqlDbType.NVarChar;

                var v2p2 = new SqlParameter();
                v2p2.Value = price;
                v2p2.ParameterName = "@ProductPrice";
                v2p2.SqlDbType = SqlDbType.NVarChar;

                var v2p3 = new SqlParameter();
                v2p3.Value = price;
                v2p3.ParameterName = "@ProductDescription";
                v2p3.SqlDbType = SqlDbType.NVarChar;

                command.Parameters.Add(v2p1);
                command.Parameters.Add(v2p2);
                command.Parameters.Add(v2p3);

                command.ExecuteNonQuery();
            }
        }
        public void RefreshAfterRemove()
        {
            using (conn = new SqlConnection())
            {
                conn.ConnectionString = cs;
                conn.Open();
                SqlCommand command = new SqlCommand();
                command.CommandText = $"select*from  {Combo_box.SelectedItem.ToString()} ";
                command.Connection = conn;
                table = new DataTable();

                bool hasColumnAdded = false;
                using (reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (!hasColumnAdded)
                        {

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                table.Columns.Add(reader.GetName(i));
                            }
                            hasColumnAdded = true;
                        }
                        DataRow row = table.NewRow();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[i] = reader[i];
                        }
                        table.Rows.Add(row);
                    }
                    MeinGirid.ItemsSource = table.DefaultView;

                }



            }
        }

        private void Sell_btn_Click(object sender, RoutedEventArgs e)
        {
            // delete button
            using (conn = new SqlConnection())
            {
                conn.ConnectionString = cs;
                conn.Open();

                if (Combo_box.SelectedItem.ToString() == "Products")
                {


                    SqlCommand command = new SqlCommand("sp_DeleteProduct", conn);
                    command.CommandType = CommandType.StoredProcedure;

                    var param1 = new SqlParameter();
                    param1.Value = ProductId;
                    param1.ParameterName = "@ProductId";
                    param1.SqlDbType = SqlDbType.Int;
                    command.Parameters.Add(param1);

                    command.ExecuteNonQuery();


                    RefreshAfterRemove();
                }
                else if (Combo_box.SelectedItem.ToString() == "Customers")
                {
                    SqlCommand command = new SqlCommand("sp_DeleteCustomer", conn);
                    command.CommandType = CommandType.StoredProcedure;

                    var param1 = new SqlParameter();
                    param1.Value = customerid;
                    param1.ParameterName = "@id";
                    param1.SqlDbType = SqlDbType.Int;
                    command.Parameters.Add(param1);

                    command.ExecuteNonQuery();


                    RefreshAfterRemove();
                }
                else
                {
                    MessageBox.Show("Only Delete Customer And Products");
                }
            }
        }
    }
}
