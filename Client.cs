using MDI_Test_Figuri; // Asigură-te că namespace-urile tale extra sunt aici
using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MoldCom
{
    public class CartItem // Pentru produsele plasate in cos
    {
        // 1. Folosim clasa centralizată Produs
        public Produs Produs { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Produs.Pret * Quantity;
    }

    public class Client : Form
    {
        private ListView lvProducts;
        private TextBox txtSearch, txtDetails;
        private NumericUpDown nudQuantity;
        private Button btnSearch, btnAddToCart, btnViewCart, btnPlaceOrder, btnLogout;
        private Label lblWelcome;

        // 2. Folosim Lista din DataStore, deci nu mai avem nevoie de o lista locala de produse
        // Lista care ține locul coșului de cumpărături
        private List<CartItem> shoppingCart = new List<CartItem>();

        public Client()
        {
            InitializeComponent();

            // 3. Inițializăm datele din DataStore (dacă nu au fost deja inițializate de alt formular)
            DataStore.InitializeData();

            // 4. Afișăm produsele direct din sursa centralizată
            DisplayProducts(DataStore.ListaProduse);
        }

        private void DisplayProducts(IEnumerable<Produs> productsToDisplay)
        {
            lvProducts.Items.Clear();

            foreach (var produs in productsToDisplay)
            {
                ListViewItem item = new ListViewItem(produs.Nume);
                item.SubItems.Add(produs.Categorie);
                item.SubItems.Add($"{produs.Pret:N2} MDL");

                item.Tag = produs;
                lvProducts.Items.Add(item);
            }
        }

        private void InitializeComponent()
        {
            this.Text = "MoldCom Online Store";
            this.Size = new System.Drawing.Size(780, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.FromArgb(44, 55, 68);

            // Top bar
            Label lblTitle = new Label();
            lblTitle.Text = "MoldCom Online Store";
            lblTitle.ForeColor = System.Drawing.Color.White;
            lblTitle.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
            lblTitle.Location = new System.Drawing.Point(15, 15);
            lblTitle.AutoSize = true;

            lblWelcome = new Label();
            lblWelcome.Text = "Welcome, Client";
            lblWelcome.ForeColor = System.Drawing.Color.LightGray;
            lblWelcome.Location = new System.Drawing.Point(550, 20);
            lblWelcome.AutoSize = true;

            btnLogout = new Button();
            btnLogout.Text = "Logout";
            btnLogout.Location = new System.Drawing.Point(670, 12);
            btnLogout.Size = new System.Drawing.Size(80, 30);
            btnLogout.BackColor = System.Drawing.Color.FromArgb(80, 80, 80);
            btnLogout.ForeColor = System.Drawing.Color.White;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Click += (s, e) => this.Close();

            // Search bar
            Label lblSearch = new Label();
            lblSearch.Text = "Search product:";
            lblSearch.ForeColor = System.Drawing.Color.White;
            lblSearch.Location = new System.Drawing.Point(15, 60);
            lblSearch.AutoSize = true;

            txtSearch = new TextBox();
            txtSearch.Location = new System.Drawing.Point(130, 57);
            txtSearch.Width = 200;

            btnSearch = new Button();
            btnSearch.Text = "Search";
            btnSearch.Location = new System.Drawing.Point(340, 56);
            btnSearch.Size = new System.Drawing.Size(70, 26);
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.BackColor = System.Drawing.Color.FromArgb(80, 90, 100);
            btnSearch.ForeColor = System.Drawing.Color.White;
            btnSearch.Click += BtnSearch_Click;

            // Product ListView
            lvProducts = new ListView();
            lvProducts.Location = new System.Drawing.Point(15, 95);
            lvProducts.Size = new System.Drawing.Size(440, 400);
            lvProducts.View = View.Details;
            lvProducts.FullRowSelect = true;
            lvProducts.GridLines = true;
            lvProducts.BackColor = System.Drawing.Color.FromArgb(180, 180, 180);
            lvProducts.Columns.AddRange(new ColumnHeader[] {
                new ColumnHeader { Text = "ProductName", Width = 160 },
                new ColumnHeader { Text = "Category", Width = 130 },
                new ColumnHeader { Text = "Price", Width = 146 }
            });
            lvProducts.SelectedIndexChanged += LvProducts_SelectedIndexChanged;

            // Right panel
            GroupBox grpDetails = new GroupBox();
            grpDetails.Text = "Product Details";
            grpDetails.ForeColor = System.Drawing.Color.White;
            grpDetails.Location = new System.Drawing.Point(470, 95);
            grpDetails.Size = new System.Drawing.Size(280, 400);

            txtDetails = new TextBox();
            txtDetails.Multiline = true;
            txtDetails.ScrollBars = ScrollBars.Vertical;
            txtDetails.ReadOnly = true;
            txtDetails.Location = new System.Drawing.Point(10, 20);
            txtDetails.Size = new System.Drawing.Size(255, 120);

            Label lblQty = new Label();
            lblQty.Text = "Quantity";
            lblQty.ForeColor = System.Drawing.Color.White;
            lblQty.Location = new System.Drawing.Point(10, 150);
            lblQty.AutoSize = true;

            nudQuantity = new NumericUpDown();
            nudQuantity.Location = new System.Drawing.Point(80, 147);
            nudQuantity.Width = 80;
            nudQuantity.Minimum = 1;
            nudQuantity.Maximum = 100;
            nudQuantity.Value = 1;

            btnAddToCart = CreateButton("Add to Cart", 185);
            btnViewCart = CreateButton("View Cart", 235);
            btnPlaceOrder = CreateButton("Place Order", 285);

            btnAddToCart.Click += (s, e) =>
            {
                // 5. Verificăm după tipul Produs
                if (lvProducts.SelectedItems.Count > 0 && lvProducts.SelectedItems[0].Tag is Produs selectedProduct)
                {
                    int qty = (int)nudQuantity.Value;

                    // Extra: Verificăm să nu comande mai mult decât e pe stoc
                    if (qty > selectedProduct.Cantitate)
                    {
                        MessageBox.Show($"Stoc insuficient! Mai sunt doar {selectedProduct.Cantitate} bucăți disponibile.", "Atenție");
                        return;
                    }

                    shoppingCart.Add(new CartItem { Produs = selectedProduct, Quantity = qty });
                    MessageBox.Show($"{qty} x {selectedProduct.Nume} au fost adăugate în coș!", "Succes");
                }
                else
                {
                    MessageBox.Show("Selecteaza un produs inainte de a adauga in cos", "Atentie");
                }
            };

            btnViewCart.Click += (s, e) =>
            {
                if (shoppingCart.Count == 0)
                {
                    MessageBox.Show("Coșul tău este gol momentan.", "Informație");
                    return;
                }

                using (CartForm cartForm = new CartForm(shoppingCart))
                {
                    cartForm.ShowDialog();
                }
            };

            btnPlaceOrder.Click += (s, e) =>
            {
                if (shoppingCart.Count == 0)
                {
                    MessageBox.Show("Nu a fost adaugata nici o comanda in cos.", "Atentie!");
                    return;
                }

                // 6. Scădem stocul direct din DataStore.ListaProduse
                foreach (var cartItem in shoppingCart)
                {
                    cartItem.Produs.Cantitate -= cartItem.Quantity;
                }

                shoppingCart.Clear();

                // Opțional: Reîncărcăm detaliile pentru a reflecta noul stoc dacă e ceva selectat
                LvProducts_SelectedIndexChanged(null, null);

                MessageBox.Show("Comanda a fost plasata cu succes!", "Succes");
            };

            grpDetails.Controls.AddRange(new Control[] { txtDetails, lblQty, nudQuantity, btnAddToCart, btnViewCart, btnPlaceOrder });

            this.Controls.AddRange(new Control[] {
                lblTitle, lblWelcome, btnLogout, lblSearch, txtSearch, btnSearch, lvProducts, grpDetails
            });
        }

        private Button CreateButton(string text, int y)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new System.Drawing.Point(10, y);
            btn.Size = new System.Drawing.Size(255, 35);
            btn.BackColor = System.Drawing.Color.FromArgb(70, 130, 180);
            btn.ForeColor = System.Drawing.Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            return btn;
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.ToLower();

            // 7. Căutăm în sursa centralizată DataStore
            var filteredProducts = DataStore.ListaProduse
                .Where(p => p.Nume.ToLower().Contains(searchTerm) || p.Categorie.ToLower().Contains(searchTerm))
                .ToList();

            DisplayProducts(filteredProducts);

            if (filteredProducts.Count == 0)
            {
                MessageBox.Show("Niciun produs găsit!", "Informație", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LvProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvProducts != null && lvProducts.SelectedItems.Count > 0)
            {
                var item = lvProducts.SelectedItems[0];
                if (item.Tag is Produs selectedProduct)
                {
                    // 8. Afișăm detaliile folosind proprietățile clasei Produs
                    txtDetails.Text = $"Produs: {selectedProduct.Nume}\r\n" +
                                      $"Categorie: {selectedProduct.Categorie}\r\n" +
                                      $"Preț: {selectedProduct.Pret:N2} MDL\r\n" +
                                      $"Stoc Disponibil: {selectedProduct.Cantitate} buc.\r\n\r\n" +
                                      $"Descriere: {selectedProduct.Descriere}";
                }
            }
            else
            {
                txtDetails.Clear();
            }
        }
    }
    
}