using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using MaterialDesignThemes.Wpf;
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
using System.Windows.Shapes;




namespace Zahard_Cars
{
    /// <summary>
    /// Interaction logic for Program.xaml
    /// </summary>

    public partial class Program : Window
    {
        ZahardCarsDBEntities1 context1 = new ZahardCarsDBEntities1();
        Worker work = new Worker();
        Client clinet = new Client();
        Car cars = new Car();
        oreder_custm ordcu = new oreder_custm();
        Order ord = new Order();
        List<int> linechart = new List<int>();
        public bool IsDarkTheme { get; set; }
        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        public Program()
        {
            InitializeComponent();
            var dbContext = new ZahardCarsDBEntities1();
            var prices = dbContext.Orders
                .GroupBy(t => new { t.Date_end.Year, t.Date_end.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, TotalPrice = g.Sum(t => t.price) })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToList();

            var incomes = dbContext.Orders
                .GroupBy(t => new { t.Date_end.Year, t.Date_end.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalPrice = g.Sum(t => t.price)
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToList();

            var expenses = dbContext.costs
                .GroupBy(t => new { t.months })
                .Select(g => new
                {
                    months = g.Key.months,
                    TotalTaxes = g.Sum(t => t.taxes) - g.Sum(t => t.electricity) - g.Sum(t => t.maintenance) - g.Sum(t => t.others)
                })
                .OrderBy(g => g.months)
                .ToList();

            var result = incomes
                .Select((income, index) => new
                {
                    Year = income.Year,
                    Month = income.Month,
                    TotalIncome = income.TotalPrice - (index < expenses.Count ? expenses[index].TotalTaxes : 0)
                })
                .ToList();

            Values = new ChartValues<ObservableValue>();
            foreach (var price in prices)
            {
                Values.Add(new ObservableValue(price.TotalPrice));
            }

            Values1 = new ChartValues<ObservableValue>();
            foreach (var income in result)
            {
                Values1.Add(new ObservableValue(income.TotalIncome));
            }

            // Lets define a custom mapper, to set fill and stroke
            // according to chart values...
            Mapper = Mappers.Xy<ObservableValue>()
                .X((item, index) => index)
                .Y(item => item.Value)
                .Fill(item => item.Value > 200 ? DangerBrush : null)
                .Stroke(item => item.Value > 200 ? DangerBrush : null);

            Formatter = x => x + " ms";

            DangerBrush = new SolidColorBrush(Color.FromRgb(238, 83, 80));

            DataContext = this;

        }
        public Func<double, string> Formatter { get; set; }
        public ChartValues<ObservableValue> Values { get; set; }
        public ChartValues<ObservableValue> Values1 { get; set; }
        public Brush DangerBrush { get; set; }
        public CartesianMapper<ObservableValue> Mapper { get; set; }




        private void btu_Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        private void text_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            int id = 0;
            int salary = 0;

            int.TryParse(text_search.Text, out id);
            int.TryParse(text_search.Text, out salary);

            string search = text_search.Text;

            var work = context1.Workers.Where(w => w.Worker_name.Contains(search) || w.Email.Contains(search) || w.Phone.Contains(search) || w.Worker_id == id || w.Salary == salary).ToList();
            data_workers.ItemsSource = work;
        }

        private void btu_max_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)

                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;

            Window_Loaded(sender, e);
        }

        private void btu_min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btu_dark_Click(object sender, RoutedEventArgs e)
        {
            ITheme theme = paletteHelper.GetTheme();
            if (IsDarkTheme = theme.GetBaseTheme() == BaseTheme.Dark)
            {
                IsDarkTheme = false;
                theme.SetBaseTheme(Theme.Light);
            }
            else
            {
                IsDarkTheme = true;
                theme.SetBaseTheme(Theme.Dark);
            }

            paletteHelper.SetTheme(theme);
        }

        private void btu_home_Click(object sender, RoutedEventArgs e)
        {
            can_form_Loaded(sender, e);
            can_form.Visibility = Visibility.Visible;
            can_cars.Visibility = Visibility.Hidden;
            can_client.Visibility = Visibility.Hidden;
            can_orders.Visibility = Visibility.Hidden;
            can_Workers.Visibility = Visibility.Hidden;
            can_orders_add.Visibility = Visibility.Hidden;



        }

        private void can_form_Loaded(object sender, RoutedEventArgs e)
        {
            using (var cont = new ZahardCarsDBEntities1())
            {
                var tax = cont.costs.Sum(p => p.taxes);
                var elc = cont.costs.Sum(p => p.electricity);
                var minn = cont.costs.Sum(p => p.maintenance);
                var oth = cont.costs.Sum(p => p.others);
                var salarys = cont.Workers.Sum(p => p.Salary);
                var sum = cont.Orders.Sum(p => p.price) - elc - minn - oth - tax - salarys;

                var count_car = cont.Cars.Count(p => p.Car_id > 0);
                var count_car_in_order = cont.Orders.Count(o => o.Date_end >= DateTime.Now);

                var mostUsedCar = cont.Orders.GroupBy(c => c.Car_id).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
                var carName = cont.Cars.Where(c => c.Car_id == mostUsedCar).Select(c => c.Car_name).FirstOrDefault();

                var count_worker = cont.Workers.Count();
                var top = cont.Workers.Max(w => w.Salary);
                var worst = cont.Workers.Min(w => w.Salary);






                ban.Text = sum.ToString() + "DA";

                txt_Taxes.Text = "Taxes:" + tax.ToString() + "DA";
                txt_electricity.Text = "Electricity:" + elc.ToString() + "DA";
                txt_maintenance.Text = "Maintenance:" + minn.ToString() + "DA";
                txt_others.Text = "Others:" + oth.ToString() + "DA";
                txt_salary.Text = "Salarrys:" + salarys.ToString() + "DA";


                txt_count_car.Text = "Count of cars:" + count_car.ToString();
                txt_count_car_in_order.Text = "Count of cars in order:" + count_car_in_order.ToString();
                txt_most_car.Text = "Most Used Car: " + carName.ToString();

                txt_count_worker.Text = "Count of Worker:" + count_worker.ToString();
                txt_top_salary.Text = "Max salary:" + top.ToString();
                txt_worst_salary.Text = "Min salary: " + worst.ToString();

                var prices = cont.Orders
                .GroupBy(t => new { t.Date_end.Year, t.Date_end.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, TotalPrice = g.Sum(t => t.price) })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToList();

                Values.Clear();
                foreach (var price in prices)
                {
                    Values.Add(new ObservableValue(price.TotalPrice));

                }
                // Retrieve the data from the database


                chart.Update(true);

            }


        }

        private void can_Workers_Loaded(object sender, RoutedEventArgs e)
        {
            using (var context = new ZahardCarsDBEntities1())
            {
                var workers = context.Workers.ToList();
                data_workers.ItemsSource = workers;


            }
        }

        private void data_workers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var row = data_workers.SelectedItem;
                int id = Convert.ToInt32((data_workers.SelectedCells[0].Column.GetCellContent(row) as TextBlock).Text);
                work = context1.Workers.Where(x => x.Worker_id == id).FirstOrDefault();

                txt_worker_ID.Text = work.Worker_id.ToString();
                txt_worker_name.Text = work.Worker_name.ToString();
                txt_worker_phone.Text = work.Phone.ToString();
                txt_worker_salary.Text = work.Salary.ToString();
                txt_worker_email.Text = work.Email.ToString();
            }
            catch { }
        }

        private void button_delete_worker_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this worker", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    /*int id = Convert.ToInt32(txt_worker_ID.Text);
                    work = context1.Workers.FirstOrDefault(w => w.Worker_id == id);*/

                    context1.Entry(work).State = System.Data.Entity.EntityState.Deleted;
                    context1.SaveChanges();
                    MessageBox.Show("Delete is successful");
                    can_Workers_Loaded(sender, e);
                }
            }
            catch
            { }
        }
        private void button_edit_worker_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                work.Worker_id = Convert.ToInt32(txt_worker_ID.Text);
                work.Worker_name = txt_worker_name.Text;
                work.Salary = Convert.ToInt32(txt_worker_salary.Text);
                work.Phone = txt_worker_phone.Text;
                work.Email = txt_worker_email.Text;

                context1.Entry(work).State = System.Data.Entity.EntityState.Modified;
                context1.SaveChanges();
                MessageBox.Show("Edit is successful");
                can_Workers_Loaded(sender, e);
            }
            catch { }
        }
        private void button_add_worker_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                work.Worker_name = txt_worker_name.Text;
                work.Salary = Convert.ToInt32(txt_worker_salary.Text);
                work.Phone = txt_worker_phone.Text;
                work.Email = txt_worker_email.Text;

                context1.Entry(work).State = System.Data.Entity.EntityState.Added;
                context1.SaveChanges();
                MessageBox.Show("add is successful");
                can_Workers_Loaded(sender, e);
            }
            catch { }
        }

        private void button_clear_worker_Click(object sender, RoutedEventArgs e)
        {
            txt_worker_ID.Text = "";
            txt_worker_name.Text = "";
            txt_worker_phone.Text = "";
            txt_worker_salary.Text = "";
            txt_worker_email.Text = "";
        }



        private void data_client_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var row = data_client.SelectedItem;
                int id = Convert.ToInt32((data_client.SelectedCells[0].Column.GetCellContent(row) as TextBlock).Text);
                clinet = context1.Clients.FirstOrDefault(c => c.id == id);

                txt_client_ID.Text = clinet.id.ToString();
                txt_client_name.Text = clinet.Client_name.ToString();
                txt_client_phone.Text = clinet.Phone.ToString();
                txt_client_adress.Text = clinet.Adress.ToString();
                txt_client_email.Text = clinet.Email.ToString();
            }
            catch { }
        }

        private void text_search_ckient_TextChanged(object sender, TextChangedEventArgs e)
        {
            int id = 0;


            int.TryParse(text_search.Text, out id);


            string search = text_search_client.Text;

            var clin = context1.Clients.Where(w => w.Client_name.Contains(search) || w.Email.Contains(search) || w.Phone.Contains(search) || w.id == id || w.Adress.Contains(search)).ToList();
            data_client.ItemsSource = clin;
        }

        private void button_delete_client_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this Client", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    /*int id = Convert.ToInt32(txt_worker_ID.Text);
                    work = context1.Workers.FirstOrDefault(w => w.Worker_id == id);*/

                    context1.Entry(clinet).State = System.Data.Entity.EntityState.Deleted;
                    context1.SaveChanges();
                    MessageBox.Show("Delete is successful");
                    can_client_Loaded(sender, e);
                }
            }
            catch
            { }
        }
        private void button_edit_client_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                clinet.id = Convert.ToInt32(txt_client_ID.Text);
                clinet.Client_name = txt_client_name.Text;
                clinet.Email = txt_client_email.Text;
                clinet.Adress = txt_client_adress.Text;
                clinet.Phone = txt_client_phone.Text;

                context1.Entry(clinet).State = System.Data.Entity.EntityState.Modified;
                context1.SaveChanges();
                MessageBox.Show("Edit is successful");
                can_client_Loaded(sender, e);
            }
            catch { }
        }
        private void button_add_client_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                clinet.Client_name = txt_client_name.Text;
                clinet.Email = txt_client_email.Text;
                clinet.Adress = txt_client_adress.Text;
                clinet.Phone = txt_client_phone.Text;

                context1.Entry(clinet).State = System.Data.Entity.EntityState.Added;
                context1.SaveChanges();
                MessageBox.Show("add is successful");
                can_client_Loaded(sender, e);
            }
            catch { }
        }

        private void button_clear_client_Click(object sender, RoutedEventArgs e)
        {
            txt_client_ID.Text = "";
            txt_client_name.Text = "";
            txt_client_phone.Text = "";
            txt_client_adress.Text = "";
            txt_client_email.Text = "";
        }


        private void can_client_Loaded(object sender, RoutedEventArgs e)
        {
            using (var context = new ZahardCarsDBEntities1())
            {
                var clinets = context.Clients.ToList();
                data_client.ItemsSource = clinets;


            }
        }


        private void can_cars_Loaded(object sender, RoutedEventArgs e)
        {
            using (var context = new ZahardCarsDBEntities1())
            {
                var carss = context.Cars.ToList();
                data_cars.ItemsSource = carss;


            }
        }

        private void data_cars_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var row = data_cars.SelectedItem;
                int id = Convert.ToInt32((data_cars.SelectedCells[0].Column.GetCellContent(row) as TextBlock).Text);
                cars = context1.Cars.FirstOrDefault(c => c.Car_id == id);

                txt_cars_ID.Text = cars.Car_id.ToString();
                txt_cars_name.Text = cars.Car_name.ToString();
                txt_cars_color.Text = cars.colors.ToString();
                txt_cars_model.Text = cars.Model.ToString();

            }
            catch { }
        }

        private void text_search_cars_TextChanged(object sender, TextChangedEventArgs e)
        {
            int id = 0;
            int model = 0;


            int.TryParse(text_search_cars.Text, out id);
            int.TryParse(text_search_cars.Text, out model);


            string search = text_search_cars.Text;

            var car = context1.Cars.Where(w => w.Car_name.Contains(search) || w.colors.Contains(search) || w.Model == model || w.Car_id == id).ToList();
            data_cars.ItemsSource = car;
        }

        private void button_delete_cars_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this car", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    /*int id = Convert.ToInt32(txt_worker_ID.Text);
                    work = context1.Workers.FirstOrDefault(w => w.Worker_id == id);*/

                    context1.Entry(cars).State = System.Data.Entity.EntityState.Deleted;
                    context1.SaveChanges();
                    MessageBox.Show("Delete is successful");
                    can_cars_Loaded(sender, e);
                }
            }
            catch
            {
                MessageBox.Show("Delete is not successful");
            }
        }
        private void button_edit_cars_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cars.Car_id = Convert.ToInt32(txt_cars_ID.Text);
                cars.Car_name = txt_cars_name.Text;
                cars.colors = txt_cars_color.Text;
                cars.Model = Convert.ToInt32(txt_cars_model.Text);

                context1.Entry(cars).State = System.Data.Entity.EntityState.Modified;
                context1.SaveChanges();
                MessageBox.Show("Edit is successful");
                can_cars_Loaded(sender, e);
            }
            catch
            {
                MessageBox.Show("Delete is not successful");
            }
        }
        private void button_add_cars_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                cars.Car_name = txt_cars_name.Text;
                cars.colors = txt_cars_color.Text;
                cars.Model = Convert.ToInt32(txt_cars_model);

                context1.Entry(cars).State = System.Data.Entity.EntityState.Added;
                context1.SaveChanges();
                MessageBox.Show("add is successful");
                can_cars_Loaded(sender, e);
            }
            catch
            {
                MessageBox.Show("Delete is not successful");
            }
        }

        private void button_clear_cars_Click(object sender, RoutedEventArgs e)
        {
            txt_cars_model.Text = "";
            txt_cars_name.Text = "";
            txt_cars_ID.Text = "";
            txt_cars_color.Text = "";
        }

        private void can_orders_Loaded(object sender, RoutedEventArgs e)
        {
            using (var context = new ZahardCarsDBEntities1())
            {
                var ordcu = context.oreder_custm.ToList();
                data_orders.ItemsSource = ordcu;


            }
        }

        private void data_orders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var row = data_orders.SelectedItem;
                int id = Convert.ToInt32((data_orders.SelectedCells[0].Column.GetCellContent(row) as TextBlock).Text);
                var ord = context1.oreder_custm.FirstOrDefault(c => c.Order_id == id);

                txt_orders_Client_name.Text = ord.Client_name.ToString();
                txt_orders_car_ID.Text = ord.Car_id.ToString();
                txt_orders_car_name.Text = ord.Car_name.ToString();
                txt_orders_Client_ID.Text = ord.CLient_id.ToString();
                txt_orders_Model.Text = ord.Model.ToString();
                txt_orders_price.Text = ord.price.ToString();
                txt_orders_Email.Text = ord.Email.ToString();
                txt_orders_datestart.Text = ord.Date_start.ToString();
                txt_orders_datend.Text = ord.Date_end.ToString();

            }
            catch { }
        }

        private void text_search_orders_TextChanged(object sender, TextChangedEventArgs e)
        {
            int id = 0;
            int model = 0;


            int.TryParse(text_search_cars.Text, out id);
            int.TryParse(text_search_cars.Text, out model);


            string search = text_search_cars.Text;

            var car = context1.Cars.Where(w => w.Car_name.Contains(search) || w.colors.Contains(search) || w.Model == model || w.Car_id == id).ToList();
            data_cars.ItemsSource = car;
        }

        private void btu_worker_Click(object sender, RoutedEventArgs e)
        {
            can_form.Visibility = Visibility.Hidden;
            can_cars.Visibility = Visibility.Hidden;
            can_client.Visibility = Visibility.Hidden;
            can_orders.Visibility = Visibility.Hidden;
            can_Workers.Visibility = Visibility.Visible;
            can_orders_add.Visibility = Visibility.Hidden;
        }

        private void btu_client_Click(object sender, RoutedEventArgs e)
        {
            can_form.Visibility = Visibility.Hidden;
            can_cars.Visibility = Visibility.Hidden;
            can_client.Visibility = Visibility.Visible;
            can_orders.Visibility = Visibility.Hidden;
            can_Workers.Visibility = Visibility.Hidden;
        }

        private void btu_car_Click(object sender, RoutedEventArgs e)
        {
            can_form.Visibility = Visibility.Hidden;
            can_cars.Visibility = Visibility.Visible;
            can_client.Visibility = Visibility.Hidden;
            can_orders.Visibility = Visibility.Hidden;
            can_Workers.Visibility = Visibility.Hidden;
            can_orders_add.Visibility = Visibility.Hidden;
        }

        private void btu_order_Click(object sender, RoutedEventArgs e)
        {
            can_orders_Loaded(sender, e);
            can_form.Visibility = Visibility.Hidden;
            can_cars.Visibility = Visibility.Hidden;
            can_client.Visibility = Visibility.Hidden;
            can_orders.Visibility = Visibility.Visible;
            can_Workers.Visibility = Visibility.Hidden;
            can_orders_add.Visibility = Visibility.Hidden;
            can_orders_add.Visibility = Visibility.Hidden;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            data_cars.Width = data_client.Width = data_orders.Width = data_workers.Width = this.Width - 190;
            data_cars.Height = data_client.Height = data_orders.Height = data_workers.Height = this.Height - 266;
        }

        private void txt_orders_add_Client_ID_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int id = 0;
                int.TryParse(txt_orders_add_Client_ID.Text, out id);

                var cid = context1.Clients.FirstOrDefault(c => c.id == id);

                txt_orders_add_phone.Text = cid.Phone.ToString();
                txt_orders_Client_add_name.Text = cid.Client_name.ToString();
                txt_orders_add_Email.Text = cid.Email.ToString();
            }
            catch { }
        }

        private void txt_orders_add_car_ID_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int id = 0;
                int.TryParse(txt_orders_add_car_ID.Text, out id);

                var cid = context1.Cars.FirstOrDefault(c => c.Car_id == id);

                txt_orders_add_car_name.Text = cid.Car_name.ToString();
                txt_orders_add_color.Text = cid.colors.ToString();
                txt_orders_add_Model.Text = cid.Model.ToString();
            }
            catch { }
        }

        private void btu_login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ord.Car_id = Convert.ToInt32(txt_orders_add_car_ID.Text);
                ord.Client_id = Convert.ToInt32(txt_orders_add_Client_ID.Text);
                ord.Date_start = txt_dates.DisplayDate;
                ord.Date_end = txt_dateen.DisplayDate;
                ord.price = Convert.ToInt32(txt_orders_add_price.Text);

                context1.Entry(ord).State = System.Data.Entity.EntityState.Added;
                context1.SaveChanges();
                MessageBox.Show("add is successful");
            }
            catch
            {
                MessageBox.Show("add is not successful");
            }
        }

        private void btu_order_add_Click(object sender, RoutedEventArgs e)
        {
            can_form.Visibility = Visibility.Hidden;
            can_cars.Visibility = Visibility.Hidden;
            can_client.Visibility = Visibility.Hidden;
            can_orders.Visibility = Visibility.Hidden;
            can_Workers.Visibility = Visibility.Hidden;
            can_orders_add.Visibility = Visibility.Visible;
        }

        private void btu_logout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to Log out ", "Log out Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var test = new ZahardCarsDBEntities1())
                    {
                        var user = test.Admins.FirstOrDefault(u => u.Admin_name == "Jue Viole");
                        if (user != null)
                        {

                            test.Entry(user).State = System.Data.Entity.EntityState.Deleted;
                            test.SaveChanges();
                            Environment.Exit(0);

                        }
                    }
                }
                catch { }
            }
        }
    }
}
