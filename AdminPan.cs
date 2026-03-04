using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MoldCom
{
    public class AdminPan : Form
    {
        private Panel pnlSidebar;
        private Button btnManageProducts, btnManageUsers, btnReports, btnLogout;
        private Panel pnlContent;

        public AdminPan()
        {
            InitializeComponent();
            DataStore.InitializeData(); // Ne asigurăm că există date
            ShowProductManagement();
        }

        private void InitializeComponent()
        {
            this.Text = "Admin Panel / Depozit Produse";
            this.Size = new System.Drawing.Size(1200, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.FromArgb(44, 55, 68);

            // Sidebar
            pnlSidebar = new Panel();
            pnlSidebar.BackColor = System.Drawing.Color.FromArgb(33, 44, 57);
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Width = 200;

            Label lblAdmin = new Label();
            lblAdmin.Text = "ADMIN PANEL";
            lblAdmin.ForeColor = System.Drawing.Color.White;
            lblAdmin.Font = new System.Drawing.Font("Arial", 13, System.Drawing.FontStyle.Bold);
            lblAdmin.AutoSize = false;
            lblAdmin.Width = 200;
            lblAdmin.Height = 60;
            lblAdmin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblAdmin.Location = new System.Drawing.Point(0, 20);

            btnManageProducts = CreateSidebarButton("Manage Products", 100);
            btnManageUsers = CreateSidebarButton("Manage Users", 160);
            btnReports = CreateSidebarButton("Reports", 220);
            btnLogout = CreateSidebarButton("Logout", 280);

            btnManageProducts.Click += (s, e) => ShowProductManagement();
            btnManageUsers.Click += (s, e) => ShowUserManagement();
            btnReports.Click += (s, e) => ShowReports();
            btnLogout.Click += (s, e) => { this.Close(); };

            pnlSidebar.Controls.AddRange(new Control[] { lblAdmin, btnManageProducts, btnManageUsers, btnReports, btnLogout });

            // Content panel
            pnlContent = new Panel();
            pnlContent.BackColor = System.Drawing.Color.FromArgb(44, 55, 68);
            pnlContent.Dock = DockStyle.Fill;

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlSidebar);
        }

        private Button CreateSidebarButton(string text, int y)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new System.Drawing.Point(20, y);
            btn.Size = new System.Drawing.Size(160, 45);
            btn.BackColor = System.Drawing.Color.FromArgb(70, 130, 180);
            btn.ForeColor = System.Drawing.Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new System.Drawing.Font("Arial", 10);
            return btn;
        }

        private void ShowProductManagement()
        {
            pnlContent.Controls.Clear();

            // Title bar
            Panel pnlTitleBar = new Panel();
            pnlTitleBar.BackColor = System.Drawing.Color.FromArgb(55, 68, 82);
            pnlTitleBar.Dock = DockStyle.Top;
            pnlTitleBar.Height = 60;

            Label lbl = new Label();
            lbl.Text = "Product Management";
            lbl.ForeColor = System.Drawing.Color.White;
            lbl.Font = new System.Drawing.Font("Arial", 18, System.Drawing.FontStyle.Bold);
            lbl.AutoSize = false;
            lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            pnlTitleBar.Controls.Add(lbl);

            // ListView
            ListView lvProducts = new ListView();
            lvProducts.Location = new System.Drawing.Point(20, 80);
            lvProducts.Size = new System.Drawing.Size(570, 480);
            lvProducts.View = View.Details;
            lvProducts.FullRowSelect = true;
            lvProducts.GridLines = true;
            lvProducts.BackColor = System.Drawing.Color.FromArgb(33, 44, 57);
            lvProducts.ForeColor = System.Drawing.Color.White;
            lvProducts.Columns.AddRange(new ColumnHeader[] {
                new ColumnHeader { Text = "Cod", Width = 70 },
                new ColumnHeader { Text = "Nume Produs", Width = 150 },
                new ColumnHeader { Text = "Categorie", Width = 100 },
                new ColumnHeader { Text = "Pret (MDL)", Width = 80 },
                new ColumnHeader { Text = "Cantitate", Width = 70 },
                new ColumnHeader { Text = "Locație", Width = 90 } // Modificat din Supplier in Locatie
            });

            // Metodă locală pentru a reîncărca produsele în ListView
            Action<IEnumerable<Produs>> LoadProductsToGrid = (productsList) =>
            {
                lvProducts.Items.Clear();
                foreach (var produs in productsList)
                {
                    ListViewItem item = new ListViewItem(produs.Cod);
                    item.SubItems.Add(produs.Nume);
                    item.SubItems.Add(produs.Categorie);
                    item.SubItems.Add(produs.Pret.ToString("0.00"));
                    item.SubItems.Add(produs.Cantitate.ToString());
                    item.SubItems.Add(produs.Locatie);
                    item.Tag = produs; // Salvăm referința obiectului original
                    lvProducts.Items.Add(item);
                }
            };

            // Încărcăm inițial toate produsele din DataStore
            LoadProductsToGrid(DataStore.ListaProduse);

            // Right panel - form (TextBox-urile)
            Label lblCode = CreateLabel("Product Code", 100, 620);
            TextBox txtCode = CreateTextBox(80, 760, "txtCode");

            Label lblName = CreateLabel("Product Name", 135, 620);
            TextBox txtName = CreateTextBox(115, 760, "txtName");

            Label lblCat = CreateLabel("Category", 170, 620);
            TextBox txtCat = CreateTextBox(150, 760, "txtCategory");

            Label lblPrice = CreateLabel("Price (MDL)", 205, 620);
            TextBox txtPrice = CreateTextBox(185, 760, "txtPrice");

            Label lblQty = CreateLabel("Quantity", 240, 620);
            TextBox txtQty = CreateTextBox(220, 760, "txtQuantity");

            Label lblLoc = CreateLabel("Location", 275, 620);
            TextBox txtLoc = CreateTextBox(255, 760, "txtLocation");

            // Acțiunea de Selectare din listă
            lvProducts.SelectedIndexChanged += (s, e) =>
            {
                if (lvProducts.SelectedItems.Count > 0)
                {
                    if (lvProducts.SelectedItems[0].Tag is Produs p)
                    {
                        txtCode.Text = p.Cod;
                        txtName.Text = p.Nume;
                        txtCat.Text = p.Categorie;
                        txtPrice.Text = p.Pret.ToString();
                        txtQty.Text = p.Cantitate.ToString();
                        txtLoc.Text = p.Locatie;
                    }
                }
            };

            // Butoanele de Acțiune
            Button btnAdd = CreateButton("Add Product", 80, 620);
            Button btnUpdate = CreateButton("Update Product", 80, 770);
            Button btnDelete = CreateButton("Delete Product", 125, 620);
            Button btnSearch = CreateButton("Search (by Name)", 125, 770);

            // LOGICA: Adăugare Produs
            btnAdd.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Codul și Numele sunt obligatorii!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal pret = decimal.TryParse(txtPrice.Text, out var p) ? p : 0;
                int cantitate = int.TryParse(txtQty.Text, out var c) ? c : 0;

                Produs produsNou = new Produs
                {
                    Cod = txtCode.Text,
                    Nume = txtName.Text,
                    Categorie = txtCat.Text,
                    Pret = pret,
                    Cantitate = cantitate,
                    Locatie = txtLoc.Text,
                    Descriere = "Adăugat din Admin Panel"
                };

                DataStore.ListaProduse.Add(produsNou); // Adăugăm în memoria centrală
                LoadProductsToGrid(DataStore.ListaProduse); // Reîmprospătăm grila
                MessageBox.Show("Produs adăugat cu succes!");
            };

            // LOGICA: Modificare Produs
            btnUpdate.Click += (s, e) =>
            {
                if (lvProducts.SelectedItems.Count == 0) return;

                if (lvProducts.SelectedItems[0].Tag is Produs produsSelectat)
                {
                    produsSelectat.Cod = txtCode.Text;
                    produsSelectat.Nume = txtName.Text;
                    produsSelectat.Categorie = txtCat.Text;
                    produsSelectat.Pret = decimal.TryParse(txtPrice.Text, out var p) ? p : produsSelectat.Pret;
                    produsSelectat.Cantitate = int.TryParse(txtQty.Text, out var c) ? c : produsSelectat.Cantitate;
                    produsSelectat.Locatie = txtLoc.Text;

                    LoadProductsToGrid(DataStore.ListaProduse); // Reîmprospătăm lista pentru a vedea modificările
                    MessageBox.Show("Produs actualizat cu succes!");
                }
            };

            // LOGICA: Ștergere Produs
            btnDelete.Click += (s, e) =>
            {
                if (lvProducts.SelectedItems.Count == 0) return;

                if (lvProducts.SelectedItems[0].Tag is Produs produsSelectat)
                {
                    var result = MessageBox.Show($"Sigur dorești să ștergi produsul {produsSelectat.Nume}?", "Confirmare", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        DataStore.ListaProduse.Remove(produsSelectat); // Ștergem din memoria centrală
                        LoadProductsToGrid(DataStore.ListaProduse);

                        // Golim câmpurile
                        txtCode.Clear(); txtName.Clear(); txtCat.Clear();
                        txtPrice.Clear(); txtQty.Clear(); txtLoc.Clear();
                    }
                }
            };

            // LOGICA: Căutare Produs
            btnSearch.Click += (s, e) =>
            {
                string searchTerm = txtName.Text.ToLower(); // Căutăm după ce scrie în căsuța de Nume

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    LoadProductsToGrid(DataStore.ListaProduse); // Reset list
                }
                else
                {
                    var filtre = DataStore.ListaProduse.Where(prod => prod.Nume.ToLower().Contains(searchTerm)).ToList();
                    LoadProductsToGrid(filtre);
                }
            };

            pnlContent.Controls.AddRange(new Control[] {
                pnlTitleBar, lvProducts,
                lblCode, txtCode, lblName, txtName, lblCat, txtCat,
                lblPrice, txtPrice, lblQty, txtQty, lblLoc, txtLoc,
                btnAdd, btnUpdate, btnDelete, btnSearch
            });
        }

        private Label CreateLabel(string text, int y, int x)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.ForeColor = System.Drawing.Color.White;
            lbl.Location = new System.Drawing.Point(x, y);
            lbl.AutoSize = true;
            return lbl;
        }

        private TextBox CreateTextBox(int y, int x, string name)
        {
            TextBox txt = new TextBox();
            txt.Name = name;
            txt.Location = new System.Drawing.Point(x, y + 18);
            txt.Width = 200;
            return txt;
        }

        private Button CreateButton(string text, int y, int x)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new System.Drawing.Point(x, y + 290);
            btn.Size = new System.Drawing.Size(140, 35);
            btn.BackColor = System.Drawing.Color.FromArgb(70, 130, 180);
            btn.ForeColor = System.Drawing.Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            return btn;
        }

        private void ShowUserManagement()
        {
            pnlContent.Controls.Clear();

            // Title bar
            Panel pnlTitleBar = new Panel();
            pnlTitleBar.BackColor = System.Drawing.Color.FromArgb(55, 68, 82);
            pnlTitleBar.Dock = DockStyle.Top;
            pnlTitleBar.Height = 60;

            Label lbl = new Label();
            lbl.Text = "User Management";
            lbl.ForeColor = System.Drawing.Color.White;
            lbl.Font = new System.Drawing.Font("Arial", 18, System.Drawing.FontStyle.Bold);
            lbl.AutoSize = false;
            lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            pnlTitleBar.Controls.Add(lbl);

            // ListView Users
            ListView lvUsers = new ListView();
            lvUsers.Location = new System.Drawing.Point(20, 80);
            lvUsers.Size = new System.Drawing.Size(570, 480);
            lvUsers.View = View.Details;
            lvUsers.FullRowSelect = true;
            lvUsers.GridLines = true;
            lvUsers.BackColor = System.Drawing.Color.FromArgb(33, 44, 57);
            lvUsers.ForeColor = System.Drawing.Color.White;
            lvUsers.Columns.AddRange(new ColumnHeader[] {
        new ColumnHeader { Text = "Username", Width = 200 },
        new ColumnHeader { Text = "Password", Width = 200 },
        new ColumnHeader { Text = "Role", Width = 150 }
    });

            Action<System.Collections.Generic.IEnumerable<MDI_Test_Figuri.USER>> LoadUsers = (lista) =>
            {
                lvUsers.Items.Clear();
                foreach (var u in lista)
                {
                    ListViewItem item = new ListViewItem(u.username);
                    item.SubItems.Add(u.password); // Poți pune "***" dacă vrei să ascunzi parolele
                    item.SubItems.Add(u.rol);
                    item.Tag = u;
                    lvUsers.Items.Add(item);
                }
            };

            LoadUsers(MDI_Test_Figuri.ListaUser.ListaUSR);

            // Formular dreapta
            Label lblUser = CreateLabel("Username", 100, 620);
            TextBox txtUser = CreateTextBox(80, 760, "txtUsr");

            Label lblPass = CreateLabel("Password", 135, 620);
            TextBox txtPass = CreateTextBox(115, 760, "txtPass");

            Label lblRole = CreateLabel("Role", 170, 620);
            ComboBox cmbRole = new ComboBox();
            cmbRole.Location = new System.Drawing.Point(760, 188);
            cmbRole.Width = 200;
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRole.Items.AddRange(new string[] { "Admin", "Employee", "Client" });

            // Selectare în listă
            lvUsers.SelectedIndexChanged += (s, e) =>
            {
                if (lvUsers.SelectedItems.Count > 0 && lvUsers.SelectedItems[0].Tag is MDI_Test_Figuri.USER u)
                {
                    txtUser.Text = u.username;
                    txtPass.Text = u.password;
                    cmbRole.SelectedItem = u.rol;
                }
            };

            // Butoane
            Button btnAddUser = CreateButton("Add User", 80, 620);
            Button btnUpdateUser = CreateButton("Update User", 80, 770);
            Button btnDeleteUser = CreateButton("Delete User", 125, 620);

            btnAddUser.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtUser.Text) || cmbRole.SelectedItem == null)
                {
                    MessageBox.Show("Numele și rolul sunt obligatorii!"); return;
                }
                MDI_Test_Figuri.ListaUser.ListaUSR.Add(new MDI_Test_Figuri.USER
                {
                    username = txtUser.Text,
                    password = txtPass.Text,
                    rol = cmbRole.SelectedItem.ToString()
                });
                LoadUsers(MDI_Test_Figuri.ListaUser.ListaUSR);
            };

            btnUpdateUser.Click += (s, e) =>
            {
                if (lvUsers.SelectedItems.Count > 0 && lvUsers.SelectedItems[0].Tag is MDI_Test_Figuri.USER u)
                {
                    u.username = txtUser.Text;
                    u.password = txtPass.Text;
                    if (cmbRole.SelectedItem != null) u.rol = cmbRole.SelectedItem.ToString();
                    LoadUsers(MDI_Test_Figuri.ListaUser.ListaUSR);
                }
            };

            btnDeleteUser.Click += (s, e) =>
            {
                if (lvUsers.SelectedItems.Count > 0 && lvUsers.SelectedItems[0].Tag is MDI_Test_Figuri.USER u)
                {
                    MDI_Test_Figuri.ListaUser.ListaUSR.Remove(u);
                    LoadUsers(MDI_Test_Figuri.ListaUser.ListaUSR);
                    txtUser.Clear(); txtPass.Clear(); cmbRole.SelectedIndex = -1;
                }
            };

            pnlContent.Controls.AddRange(new Control[] {
        pnlTitleBar, lvUsers, lblUser, txtUser, lblPass, txtPass, lblRole, cmbRole,
        btnAddUser, btnUpdateUser, btnDeleteUser
    });
        }

        private void ShowReports()
        {
            pnlContent.Controls.Clear();

            // 1. Bara de titlu
            Panel pnlTitleBar = new Panel();
            pnlTitleBar.BackColor = System.Drawing.Color.FromArgb(55, 68, 82);
            pnlTitleBar.Dock = DockStyle.Top;
            pnlTitleBar.Height = 60;

            Label lblTitle = new Label();
            lblTitle.Text = "General Reports & Dashboard";
            lblTitle.ForeColor = System.Drawing.Color.White;
            lblTitle.Font = new System.Drawing.Font("Arial", 18, System.Drawing.FontStyle.Bold);
            lblTitle.AutoSize = false;
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            pnlTitleBar.Controls.Add(lblTitle);

            // 2. Calculăm statisticile folosind LINQ
            int totalProduseDiferite = DataStore.ListaProduse.Count;
            int totalBucatiInStoc = DataStore.ListaProduse.Sum(p => p.Cantitate);
            decimal valoareTotalaDepozit = DataStore.ListaProduse.Sum(p => p.Pret * p.Cantitate);
            var produseStocCritic = DataStore.ListaProduse.Where(p => p.Cantitate < 5).ToList();

            // 3. Creăm Panel-ul principal (centrat, ca la login)
            Panel pnlMain = new Panel();
            pnlMain.BackColor = System.Drawing.Color.FromArgb(55, 68, 82);
            pnlMain.Size = new System.Drawing.Size(450, 450);

            // Centrare în pnlContent
            int centerX = (pnlContent.Width > 0 ? pnlContent.Width : 1000) / 2 - (pnlMain.Width / 2);
            int centerY = 60 + ((pnlContent.Height > 0 ? pnlContent.Height : 650) - 60) / 2 - (pnlMain.Height / 2);
            pnlMain.Location = new System.Drawing.Point(centerX, centerY);
            pnlMain.Anchor = AnchorStyles.None; // Se menține centrat la redimensionare

            // 4. Afișăm Statisticile Generale
            Label lblStats = new Label();
            lblStats.Text = $" STATISTICI GENERALE DEPOZIT\n\n" +
                            $"• Total produse unice: {totalProduseDiferite}\n" +
                            $"• Total bucăți fizice în stoc: {totalBucatiInStoc} buc.\n" +
                            $"• Valoarea totală a mărfii: {valoareTotalaDepozit:N2} MDL";
            lblStats.ForeColor = System.Drawing.Color.White;
            lblStats.Font = new System.Drawing.Font("Arial", 14);
            lblStats.Location = new System.Drawing.Point(20, 20);
            lblStats.AutoSize = true;

            // 5. Afișăm lista cu produsele care trebuie comandate (Stoc Critic)
            Label lblCriticalTitle = new Label();
            lblCriticalTitle.Text = "!!! ATENTIE STOC CRITIC (Sub 5 bucati)";
            lblCriticalTitle.ForeColor = System.Drawing.Color.Orange;
            lblCriticalTitle.Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold);
            lblCriticalTitle.Location = new System.Drawing.Point(20, 150);
            lblCriticalTitle.AutoSize = true;

            ListView lvCriticalStock = new ListView();
            lvCriticalStock.Location = new System.Drawing.Point(20, 190);
            lvCriticalStock.Size = new System.Drawing.Size(405, 240);
            lvCriticalStock.View = View.Details;
            lvCriticalStock.FullRowSelect = true;
            lvCriticalStock.GridLines = true;
            lvCriticalStock.BackColor = System.Drawing.Color.FromArgb(33, 44, 57);
            lvCriticalStock.ForeColor = System.Drawing.Color.White;

            lvCriticalStock.Columns.Add("Cod Produs", 100);
            lvCriticalStock.Columns.Add("Nume Produs", 200);
            lvCriticalStock.Columns.Add("Stoc Rămas", 100);

            foreach (var p in produseStocCritic)
            {
                ListViewItem item = new ListViewItem(p.Cod);
                item.SubItems.Add(p.Nume);
                item.SubItems.Add(p.Cantitate.ToString());
                item.ForeColor = System.Drawing.Color.OrangeRed;
                lvCriticalStock.Items.Add(item);
            }

            if (produseStocCritic.Count == 0)
            {
                lvCriticalStock.Items.Add(new ListViewItem("Toate produsele au stoc suficient!") { ForeColor = System.Drawing.Color.LightGreen });
            }

            // Adăugăm elementele în Panel-ul centrat
            pnlMain.Controls.AddRange(new Control[] { lblStats, lblCriticalTitle, lvCriticalStock });

            // Adăugăm elementele pe ecran
            pnlContent.Controls.AddRange(new Control[] { pnlTitleBar, pnlMain });
        }
    }
}