using MoldCom;
using System.Windows.Forms;
using System.Drawing;

public class CartForm : Form
{
    public CartForm(System.Collections.Generic.List<CartItem> cartItems)
    {
        this.Text = "Coșul tău de cumpărături";
        this.Size = new Size(450, 400);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(44, 55, 68);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        Label lblTitle = new Label();
        lblTitle.Text = "Produse în Coș";
        lblTitle.ForeColor = Color.White;
        lblTitle.Font = new Font("Arial", 16, FontStyle.Bold);
        lblTitle.Location = new Point(15, 15);
        lblTitle.AutoSize = true;

        ListView lvCart = new ListView();
        lvCart.Location = new Point(15, 60);
        lvCart.Size = new Size(350, 220);
        lvCart.View = View.Details;
        lvCart.FullRowSelect = true;
        lvCart.GridLines = true;
        lvCart.BackColor = Color.FromArgb(33, 44, 57);
        lvCart.ForeColor = Color.White;

        lvCart.Columns.Add("Produs", 180);
        lvCart.Columns.Add("Cant.", 60);
        lvCart.Columns.Add("Total", 140);

        decimal grandTotal = 0;

        foreach (var item in cartItems)
        {
            ListViewItem lvi = new ListViewItem(item.Produs.Nume);
            lvi.SubItems.Add(item.Quantity.ToString());
            lvi.SubItems.Add($"{item.TotalPrice:N2} MDL");

            lvCart.Items.Add(lvi);
            grandTotal += item.TotalPrice;
        }

        Label lblTotal = new Label();
        lblTotal.Text = $"Total: {grandTotal:N2} MDL";
        lblTotal.ForeColor = Color.LightGreen;
        lblTotal.Font = new Font("Arial", 14, FontStyle.Bold);
        lblTotal.AutoSize = true;
        lblTotal.Location = new Point(15, 300);

        Button btnClose = new Button();
        btnClose.Text = "Închide";
        btnClose.Location = new Point(315, 295);
        btnClose.Size = new Size(100, 35);
        btnClose.BackColor = Color.FromArgb(80, 80, 80);
        btnClose.ForeColor = Color.White;
        btnClose.FlatStyle = FlatStyle.Flat;
        btnClose.Click += (s, e) => this.Close();

        this.Controls.AddRange(new Control[] { lblTitle, lvCart, lblTotal, btnClose });
    }
}