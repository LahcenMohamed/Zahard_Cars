using System;
using System.Collections.Generic;
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

namespace Zahard_Cars
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Admin ad = new Admin();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btu_sign_up_Click(object sender, RoutedEventArgs e)
        {
            can_up.Visibility = Visibility.Visible;
            can_in.Visibility = Visibility.Hidden;
        }

        private void btu_sign_in_Click(object sender, RoutedEventArgs e)
        {
            can_in.Visibility = Visibility.Visible;
            can_up.Visibility = Visibility.Hidden;
        }

        private void Check_show_Checked(object sender, RoutedEventArgs e)
        {

                txt_up_password.Visibility = Visibility.Hidden;
                txt_up_pass_Copy.Text = txt_up_password.Password;
                txt_up_pass_Copy.Visibility = Visibility.Visible;

        }

        private void Check_show_Unchecked(object sender, RoutedEventArgs e)
        {
            txt_up_pass_Copy.Visibility = Visibility.Hidden;
            txt_up_password.Password = txt_up_pass_Copy.Text;
            txt_up_password.Visibility = Visibility.Visible;
        }

        private void btu_login_Click(object sender, RoutedEventArgs e)
        {
         

            string name = txt_up_username.Text;
            string password;
            if (txt_up_pass_Copy.Visibility == Visibility.Hidden)
                password = txt_up_password.Password;
            else
                password = txt_up_pass_Copy.Text;
            if (name == "" || password == "")
            {
                MessageBox.Show("Fill in all fields");
                return;
            }

            try
            {
                using (var context = new ZahardCarsDBEntities1())
                {
                    var user = context.Admins.FirstOrDefault(u => u.Admin_name == name && u.password == password);
                    if (user != null)
                    {
                        ad.Admin_name = "Jue Viole";
                        ad.Admin_type = user.Admin_type;
                        ad.password = "123";
                        context.Entry(ad).State = System.Data.Entity.EntityState.Added;
                        context.SaveChanges();
                        Program p = new Program();
                        p.Show();
                        this.Close();
                    }
                    else
                        MessageBox.Show("username or password isn't correct");

                }
            }
            catch
            {
                MessageBox.Show("cannot conatct with server");
            }

        }

        private void btu_signin_Click(object sender, RoutedEventArgs e)
        {


            string name = txt_in_username_man.Text;
            string password = txt_in_password_man.Password;

            

            try
            {
                using (var context = new ZahardCarsDBEntities1())
                {
                    var user = context.Admins.Any(u => u.Admin_name == name && u.password == password && u.Admin_type == "Manager");
                    if (user)
                    {
                        Admin ad = new Admin();
                        ad.Admin_name = txt_in_username.Text;
                        ad.password = txt_in_password.Password;
                        ad.Admin_type = cmb_type.Text;

                        context.Admins.Add(ad);
                        context.SaveChanges();
                        Program p = new Program();
                        p.Show();
                        this.Close();
                    }
                    else
                        MessageBox.Show("username or password isn't correct");

                }
            }
            catch
            {
                MessageBox.Show("cannot conatct with server");
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Program p = new Program();
            p.Show();
            this.Close();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            using (var context = new ZahardCarsDBEntities1())
            {
                var user = context.Admins.Any(u => u.Admin_name == "Jue Viole");
                if (user)
                {
                    Program p = new Program();
                    p.Show();
                    this.Close();
                }
                
                   

            }
        }
    }
}
