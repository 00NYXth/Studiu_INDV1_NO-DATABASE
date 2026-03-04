using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MoldCom
{
    public class Employee : Form
    {
        private TextBox txtSearch, txtProductCode, txtNewQuantity;
        private ListView lvProducts;
        private Button btnSearch, btnUpdateStock, btnLogout;
        private Label lblLowStock;

        public Employee()
        {
            InitializeComponent();
            DataStore.InitializeData(); // Ne asigurăm că datele există
            LoadProductsToGrid(DataStore.ListaProduse); // Încărcăm produsele în listă
        }

        // Metodă nouă pentru a afișa produsele în ListView
        private void LoadProductsToGrid(IEnumerable<Produs> productsList)
        {
            lvProducts.Items.Clear();
            foreach (var produs in productsList)
            {
                ListViewItem item = new ListViewItem(produs.Cod);
                item.SubItems.Add(produs.Nume);
                item.SubItems.Add(produs.Cantitate.ToString());
                item.SubItems.Add(produs.Locatie); // Folosim Locatie în loc de WarehouseLocation

                item.Tag = produs; // Păstrăm referința la obiectul original
                lvProducts.Items.Add(item);
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Employee Dashboard";
            this.Size = new System.Drawing.Size(780, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.FromArgb(44, 55, 68);

            // Title
            Label lblTitle = new Label();
            lblTitle.Text = "Employee Dashboard";
            lblTitle.ForeColor = System.Drawing.Color.White;
            lblTitle.Font = new System.Drawing.Font("Arial", 18, System.Drawing.FontStyle.Bold);
            lblTitle.Location = new System.Drawing.Point(15, 15);
            lblTitle.AutoSize = true;

            btnLogout = new Button();
            btnLogout.Text = "Logout";
            btnLogout.Location = new System.Drawing.Point(670, 12);
            btnLogout.Size = new System.Drawing.Size(80, 30);
            btnLogout.BackColor = System.Drawing.Color.FromArgb(80, 80, 80);
            btnLogout.ForeColor = System.Drawing.Color.White;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Click += (s, e) => this.Close();

            // Search
            Label lblSearch = new Label();
            lblSearch.Text = "Search product (Name/Code):";
            lblSearch.ForeColor = System.Drawing.Color.White;
            lblSearch.Location = new System.Drawing.Point(15, 65);
            lblSearch.AutoSize = true;

            txtSearch = new TextBox();
            txtSearch.Location = new System.Drawing.Point(180, 62);
            txtSearch.Width = 150;

            btnSearch = new Button();
            btnSearch.Text = "Search";
            btnSearch.Location = new System.Drawing.Point(340, 61);
            btnSearch.Size = new System.Drawing.Size(80, 27);
            btnSearch.BackColor = System.Drawing.Color.FromArgb(70, 130, 180);
            btnSearch.ForeColor = System.Drawing.Color.White;
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.Click += BtnSearch_Click;

            // ListView
            lvProducts = new ListView();
            lvProducts.Location = new System.Drawing.Point(15, 100);
            lvProducts.Size = new System.Drawing.Size(460, 390);
            lvProducts.View = View.Details;
            lvProducts.FullRowSelect = true;
            lvProducts.GridLines = true;
            lvProducts.BackColor = System.Drawing.Color.FromArgb(180, 180, 180);
            lvProducts.Columns.AddRange(new ColumnHeader[] {
                new ColumnHeader { Text = "ProductCode", Width = 90 },
                new ColumnHeader { Text = "ProductName", Width = 150 },
                new ColumnHeader { Text = "QuantityInStock", Width = 100 },
                new ColumnHeader { Text = "WarehouseLocation", Width = 110 }
            });

            // Eveniment ca să completăm automat datele când dăm click pe un produs
            lvProducts.SelectedIndexChanged += (s, e) =>
            {
                if (lvProducts.SelectedItems.Count > 0 && lvProducts.SelectedItems[0].Tag is Produs selectedProduct)
                {
                    txtProductCode.Text = selectedProduct.Cod;
                    txtNewQuantity.Text = selectedProduct.Cantitate.ToString();
                    lblLowStock.Visible = selectedProduct.Cantitate < 5;
                }
            };

            // Right panel - Update Stock
            Label lblUpdateTitle = new Label();
            lblUpdateTitle.Text = "Update Stock";
            lblUpdateTitle.ForeColor = System.Drawing.Color.White;
            lblUpdateTitle.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
            lblUpdateTitle.Location = new System.Drawing.Point(500, 100);
            lblUpdateTitle.AutoSize = true;

            Label lblCode = new Label();
            lblCode.Text = "Product Code";
            lblCode.ForeColor = System.Drawing.Color.White;
            lblCode.Location = new System.Drawing.Point(500, 135);
            lblCode.AutoSize = true;

            txtProductCode = new TextBox();
            txtProductCode.Location = new System.Drawing.Point(500, 155);
            txtProductCode.Width = 220;

            Label lblNewQty = new Label();
            lblNewQty.Text = "New Quantity";
            lblNewQty.ForeColor = System.Drawing.Color.White;
            lblNewQty.Location = new System.Drawing.Point(500, 190);
            lblNewQty.AutoSize = true;

            txtNewQuantity = new TextBox();
            txtNewQuantity.Location = new System.Drawing.Point(500, 210);
            txtNewQuantity.Width = 220;

            btnUpdateStock = new Button();
            btnUpdateStock.Text = "Update Stock";
            btnUpdateStock.Location = new System.Drawing.Point(500, 250);
            btnUpdateStock.Size = new System.Drawing.Size(220, 35);
            btnUpdateStock.BackColor = System.Drawing.Color.FromArgb(70, 130, 180);
            btnUpdateStock.ForeColor = System.Drawing.Color.White;
            btnUpdateStock.FlatStyle = FlatStyle.Flat;
            btnUpdateStock.Click += BtnUpdateStock_Click;

            lblLowStock = new Label();
            lblLowStock.Text = "⚠️ LOW STOCK WARNING!\nQuantity is less than 5.";
            lblLowStock.ForeColor = System.Drawing.Color.Orange;
            lblLowStock.Location = new System.Drawing.Point(500, 310);
            lblLowStock.AutoSize = true;
            lblLowStock.Visible = false;
            lblLowStock.Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold);

            this.Controls.AddRange(new Control[] {
                lblTitle, btnLogout, lblSearch, txtSearch, btnSearch, lvProducts,
                lblUpdateTitle, lblCode, txtProductCode, lblNewQty, txtNewQuantity,
                btnUpdateStock, lblLowStock
            });
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                LoadProductsToGrid(DataStore.ListaProduse); // Resetare listă dacă search-ul e gol
            }
            else
            {
                // Căutăm după cod SAU după nume
                var filteredProducts = DataStore.ListaProduse
                    .Where(p => p.Nume.ToLower().Contains(searchTerm) || p.Cod.ToLower().Contains(searchTerm))
                    .ToList();

                LoadProductsToGrid(filteredProducts);

                if (filteredProducts.Count == 0)
                {
                    MessageBox.Show("Niciun produs găsit!", "Informație", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnUpdateStock_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductCode.Text) || string.IsNullOrWhiteSpace(txtNewQuantity.Text))
            {
                MessageBox.Show("Completați codul produsului și cantitatea nouă.", "Eroare");
                return;
            }

            if (int.TryParse(txtNewQuantity.Text, out int qty) && qty >= 0)
            {
                // Căutăm produsul în lista centralizată
                string searchCode = txtProductCode.Text.Trim();
                var produs = DataStore.ListaProduse.FirstOrDefault(p => p.Cod.Equals(searchCode, StringComparison.OrdinalIgnoreCase));

                if (produs != null)
                {
                    produs.Cantitate = qty; // Actualizăm stocul central

                    lblLowStock.Visible = qty < 5;
                    LoadProductsToGrid(DataStore.ListaProduse); // Dăm refresh la tabel

                    MessageBox.Show($"Stocul pentru {produs.Nume} a fost actualizat. Cantitate nouă: {qty}", "Succes");
                }
                else
                {
                    MessageBox.Show("Nu am găsit niciun produs cu acest cod în baza de date.", "Eroare");
                }
            }
            else
            {
                MessageBox.Show("Introduceți o cantitate validă (număr întreg pozitiv).", "Eroare");
            }
        }
    }
}