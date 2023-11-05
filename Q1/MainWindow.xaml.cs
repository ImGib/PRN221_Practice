using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Q1.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Q1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly LuyenOnThiDBContext _context;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            _context = new LuyenOnThiDBContext();
            //LoadProdct();
            LoadCategory();
        }
        public void LoadProdct()
        {
            var source = _context.Products.Include(x => x.Category).ToList();
            lvProduct.ItemsSource = source;
        }
        public void LoadCategory()
        {
            var source = _context.Categories.ToList();
            cboCategory.ItemsSource = source;
            cboCategory.SelectedIndex = 0;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Product? product = InputCheck();
            if (product != null)
            {
                _context.Products.Add(product);
                _context.SaveChanges();
                LoadProdct();
                MessageBox.Show("Add successful");
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            Product input_Pro = InputCheck();
            if (!IDCheck() || input_Pro == null)
            {
                return;
            }

            try
            {
                Product? product = _context.Products.FirstOrDefault(x => x.ProductId == int.Parse(txtID.Text));
                if(product != null)
                {
                    product.ProductName = txtName.Text;
                    product.CategoryId = (cboCategory.SelectedItem as Category).CategoryId;
                    product.QuantityPerUnit = txtQuantity.Text;
                    product.Discontinued = rbTrue.IsChecked == true;

                    _context.Products.Update(product);
                    if(_context.SaveChanges() > 0)
                    {
                        MessageBox.Show("Update successfull");
                        LoadProdct();
                        return;
                    }
                }

                MessageBox.Show("Update Fail");
            }
            catch (Exception ex) { }
        }
        private bool IDCheck()
        {
            if (String.IsNullOrEmpty(txtID.Text))
            {
                MessageBox.Show("ID is not null");
                return false;
            }
            try
            {
                int.Parse(txtID.Text);
            } catch (Exception ex)
            {
                MessageBox.Show("ID must be nummber");
                return false;
            }
            return true;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!IDCheck()) return;
            try
            {
                Product? product = _context.Products.Include(p => p.OrderDetails).FirstOrDefault(x => x.ProductId == int.Parse(txtID.Text));
                if(product != null)
                {
                    if(product.OrderDetails.Count > 0)
                    {
                        _context.OrderDetails.RemoveRange(product.OrderDetails);
                    }
                    _context.Remove(product);
                    if (_context.SaveChanges() > 0)
                    {
                        MessageBox.Show("Delete successfull");
                        LoadProdct();
                        return;
                    }
                }
                MessageBox.Show("Delete Fail");
            }
            catch
            {

            }
        }
        private Product InputCheck()
        {
            Product pro = new Product();

            if (String.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Name is not null");
                return null;
            }

            pro.ProductName = txtName.Text;

            pro.CategoryId = (cboCategory.SelectedItem as Category).CategoryId;

            pro.QuantityPerUnit = txtQuantity.Text;

            pro.Discontinued = rbTrue.IsChecked == true;
            return pro;
        }

        private void lvProduct_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Product item = (sender as ListView).SelectedItem as Product;
            if (item != null)
            {
                rbTrue.IsChecked = item.Discontinued;
                rbFalse.IsChecked = !item.Discontinued;
            }
        }

        private void btnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".json";
            saveFileDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";

            if(saveFileDialog.ShowDialog() == true)
            {
                List<Product> products = _context.Products.ToList();
                foreach (Product product in products)
                {
                    product.Category = null;
                }
                string jsonContent = JsonSerializer.Serialize(products, new JsonSerializerOptions { WriteIndented = true });
                
                File.WriteAllText(saveFileDialog.FileName, jsonContent);

                MessageBox.Show("Save successfull");
            }
        }

        private void btnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".json";
            openFileDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";

            if(openFileDialog.ShowDialog() == true)
            {
                string jsoncontent = File.ReadAllText(openFileDialog.FileName);
                List<Product> products = JsonSerializer.Deserialize<List<Product>>(jsoncontent);
                lvProduct.ItemsSource = products;
            }
        }
    }
}
