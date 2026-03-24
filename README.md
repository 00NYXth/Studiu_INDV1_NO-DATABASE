# Raport INDV1 — Gestiunea Datelor în Windows Forms fără Baze de Date

| Câmp | Detalii |
|---|---|
| **Autor** | Găină Valentin |
| **Grupa** | P2333 |
| **Instituție** | CEITI |
| **Tehnologie** | C#, .NET Framework, Windows Forms |
| **Repository** | [github.com/00NYXth/Studiu_INDV1_NO-DATABASE](https://github.com/00NYXth/Studiu_INDV1_NO-DATABASE/tree/master) |

---

## 1. Introducere

Aplicațiile desktop folosesc de obicei o bază de date pentru a stoca datele. Dar există cazuri — prototipuri, proiecte didactice — unde asta e prea mult. Proiectul ăsta face exact asta: gestionează datele **doar în memoria RAM**, folosind colecții din .NET (`List<T>`, etc.), fără nicio bază de date.

Avantaje: simplu, portabil, fără dependențe externe.  
Dezavantaj principal: datele se pierd când închizi aplicația.

Scopul proiectului:
- Modelarea entităților (`Produs`, `Client`, `Employee`) prin clase C#.
- Un depozit in-memory (`DepozitProduse`) ca sursă unică de date.
- Un flux complet de comenzi prin coș (`CartForm`) cu actualizarea stocului.
- Autentificare cu redirecționare spre interfețe diferite (admin vs. client).

---

## 2. Arhitectura Proiectului

```
Studiu_INDV1_NO-DATABASE/
│
├── Program.cs                  ← Entry point
├── App.config
│
├── DepozitProduse.cs           ← Stocul de produse (in-memory)
├── Lista_utilizatori.cs        ← Utilizatori și autentificare
│
├── LoginForm.cs                ← Formular login
├── AdminPan.cs                 ← Panou administrator
├── EmployeeForm.cs             ← Interfață angajat
├── ClientForm.cs               ← Interfață client
└── CartForm.cs                 ← Coșul de cumpărături
```

Structura urmează un **pattern two-tier** (prezentare + logică de business) — adecvat pentru un proiect didactic.

### Clasele principale

**`Client`** — datele cumpărătorului: Nume, Prenume, Email, Parola, CosCumparaturi (`List<Produs>`).

**`Employee`** — datele adminului: NumeAngajat, Rol, Parola.

**`DepozitProduse`** — clasa centrală. Deține lista de produse ca membru **static**, accesibil global din orice formular.

| Metodă | Descriere |
|---|---|
| `AdaugaProdus()` | Adaugă un produs în stoc |
| `StergeProdusDupaId()` | Elimină un produs |
| `GetProdusById()` | Returnează produsul după ID |
| `ActualizeazaStoc()` | Scade cantitatea la plasarea comenzii |

> Membrii statici din `DepozitProduse` există o singură dată pe toată durata rulării aplicației — echivalentul unui singleton, fără framework.

**`Lista_utilizatori`** — gestionează `List<Client>` și `List<Employee>`, plus logica de autentificare.

### Fluxul de lansare

```
Program.cs
    └─► LoginForm()
            ├─► Lista_utilizatori.InitializeazaDate()
            │       ├─► new Client("Ana", "Pop", ...)
            │       └─► new Employee("admin", ...)
            │
            └─► [autentificare reușită]
                    ├─► Client   → new CartForm(clientCurent)
                    └─► Employee → new AdminPan()
```

---

## 3. Sincronizarea Datelor

### Problema

Cum se actualizează stocul în timp real când un client plasează o comandă, fără o bază de date care să gestioneze tranzacțiile?

### Soluția — 3 principii combinate

1. **Referința la obiect** — `List<T>` e tip-referință în C#. Orice formular care accesează `DepozitProduse.ListaProduse` operează pe același obiect din heap, nu pe o copie.
2. **Câmpuri statice** — există o singură instanță a listei indiferent de câte formulare sunt deschise.
3. **Modificare directă** — la plasarea comenzii, se decrementează direct proprietatea `Cantitate` a obiectului din heap.

### Cod reprezentativ

```csharp
public static class DepozitProduse
{
    public static List<Produs> ListaProduse = new List<Produs>()
    {
        new Produs { Id = 1, Denumire = "Laptop",    Pret = 4999.99m, Cantitate = 10 },
        new Produs { Id = 2, Denumire = "Mouse",     Pret = 149.50m,  Cantitate = 50 },
        new Produs { Id = 3, Denumire = "Tastatura", Pret = 299.00m,  Cantitate = 30 }
    };

    public static bool ActualizeazaStoc(int produsId, int cantitateComanda)
    {
        Produs produs = ListaProduse.FirstOrDefault(p => p.Id == produsId);

        if (produs == null) return false;
        if (produs.Cantitate < cantitateComanda) return false;

        produs.Cantitate -= cantitateComanda;
        return true;
    }
}
```

```csharp
// CartForm.cs — butonul "Plasează Comanda"
private void btnPlaseazaComanda_Click(object sender, EventArgs e)
{
    if (cosClient.Count == 0)
    {
        MessageBox.Show("Coșul este gol!", "Atenție", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    // Pasul 1: Validare stoc
    foreach (var item in cosClient)
    {
        Produs produsStoc = DepozitProduse.ListaProduse.FirstOrDefault(p => p.Id == item.ProdusId);
        if (produsStoc == null || produsStoc.Cantitate < item.Cantitate)
        {
            MessageBox.Show("Stoc insuficient pentru: " + item.Denumire, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
    }

    // Pasul 2: Actualizare stoc
    foreach (var item in cosClient)
        DepozitProduse.ActualizeazaStoc(item.ProdusId, item.Cantitate);

    // Pasul 3: Confirmare
    MessageBox.Show("Comanda plasată cu succes!", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
    cosClient.Clear();
    RefreshDataGridView();
}
```

### Diagrama fluxului de date

```
┌─────────────────────────────────────────────────────────────┐
│                    MEMORIA RAM (Heap .NET)                  │
│                                                             │
│   DepozitProduse.ListaProduse (static)                      │
│   ┌─────────────────────────────────────────────┐           │
│   │  [0] Produs { Id=1, "Laptop",  Cant=10 }    │◄──── ┐    │
│   │  [1] Produs { Id=2, "Mouse",   Cant=50 }    │      │    │
│   │  [2] Produs { Id=3, "Tastatura",Cant=30 }   │      │    │
│   └─────────────────────────────────────────────┘      │    │
│                                                        │    │
└────────────────────────────────────────────────────────┼────┘
                                                         │
         CartForm                              AdminPan  │
    ┌─────────────┐                        ┌────────────┐│
    │ cosClient:  │   btnPlaseaza_Click()  │DataGridView││
    │ [{Id=1,Ct=2}│──────────────────────► │ Laptop: 10 ││
    │  {Id=3,Ct=1}│   ActualizeazaStoc()   │ (→ 8 după  ││
    └─────────────┘   ←modif. directă→     │   comandă) ││
                       pe obiect heap      └────────────┘│
                                                ▲        │
                                                └────────┘
                                          (referință la același obiect)
```

### Ciclul de viață al datelor

| Eveniment | Starea datelor |
|---|---|
| Lansare aplicație | Colecțiile statice sunt create și populate cu date seed |
| Navigare între formulare | Datele persistă — toate formularele accesează aceleași referințe |
| Plasare comandă | `Cantitate` se decrementează imediat pe obiectul din heap |
| Închidere formular | Datele din colecțiile statice rămân intacte |
| Închidere aplicație | **Toate datele se pierd** |

---

## 4. Interfața Utilizator

### LoginForm

Punct de intrare și „router" al aplicației. Verifică credențialele și deschide formularul potrivit.

```csharp
private void btnLogin_Click(object sender, EventArgs e)
{
    string utilizator = txtUtilizator.Text.Trim();
    string parola = txtParola.Text;

    Employee angajat = Lista_utilizatori.Angajati
                        .FirstOrDefault(a => a.NumeAngajat == utilizator && a.Parola == parola);
    if (angajat != null) { this.Hide(); new AdminPan().ShowDialog(); return; }

    Client client = Lista_utilizatori.Clienti
                     .FirstOrDefault(c => c.Email == utilizator && c.Parola == parola);
    if (client != null) { this.Hide(); new CartForm(client).ShowDialog(); return; }

    lblEroare.Text = "Credențiale incorecte.";
    lblEroare.Visible = true;
}
```

### AdminPan

Afișează produsele într-un `DataGridView` și permite adăugarea/ștergerea lor. Refresh-ul se face prin reatribuirea `DataSource`:

```csharp
private void RefreshGridProduse()
{
    dgvProduse.DataSource = null;
    dgvProduse.DataSource = DepozitProduse.ListaProduse;
}
```

### CartForm

Cel mai complex formular — operează simultan cu produsele din depozit și cu coșul clientului. `NumericUpDown`-ul pentru cantitate e limitat dinamic la stocul disponibil:

```csharp
private void dgvProduse_SelectionChanged(object sender, EventArgs e)
{
    if (dgvProduse.SelectedRows.Count > 0)
    {
        Produs selectat = (Produs)dgvProduse.SelectedRows[0].DataBoundItem;
        nudCantitate.Minimum = 1;
        nudCantitate.Maximum = selectat.Cantitate;
        nudCantitate.Value = 1;
    }
}
```

---

## 5. Validări și Testare

### Validări implementate

- **Stoc insuficient** — verificat la adăugare în coș (prin `NumericUpDown.Maximum`) și re-verificat la plasarea comenzii.
- **Input numeric invalid** — `decimal.TryParse()` pentru preț, `NumericUpDown` pentru cantitate.
- **Autentificare** — mesaj generic la eroare, fără a indica dacă userul există sau parola e greșită.
- **Coș gol** — verificare înainte de orice procesare.

### Scenarii de testare manuală

| Nr. | Scenariu | Rezultat așteptat |
|---|---|---|
| TC-01 | Login client valid | Deschide `CartForm` |
| TC-02 | Login admin valid | Deschide `AdminPan` |
| TC-03 | Login cu parolă greșită | Mesaj eroare, fără navigare |
| TC-04 | Adăugare produs valid | Apare în `DataGridView` |
| TC-05 | Adăugare produs cu preț invalid | Mesaj validare |
| TC-06 | Comandă cu stoc suficient | Stoc decrementat, confirmare |
| TC-07 | Comandă cu stoc insuficient | Mesaj eroare, stoc neschimbat |
| TC-08 | Comandă cu coș gol | Mesaj avertizare |
| TC-09 | Golire coș | Grid actualizat, coș gol |
| TC-10 | Verificare persistență în sesiune | Stocul rămâne decrementat între formulare |

---
## 6. Reprezentarea Grafică

### 🔐 Login Page
<p align="center">
  <img src="https://github.com/user-attachments/assets/28db0b07-2d81-4b3f-ac64-0fad340b3574" width="500"/>
</p>

---

### 🛠️ Admin Page
<p align="center">
  <img src="https://github.com/user-attachments/assets/0395b5eb-c8a6-4c8b-b43c-65683b261fea" width="800"/>
</p>

---

### 👤 Client Page
<p align="center">
  <img src="https://github.com/user-attachments/assets/c7d21e47-ea55-4f07-adc9-0178a8f8a3ff" width="700"/>
</p>

---

### 🛒 Cart Page
<p align="center">
  <img src="https://github.com/user-attachments/assets/386f1dc0-abc4-45ea-9c85-bae135229b91" width="450"/>
</p>

---

### 👨‍💼 Angajat Page
<p align="center">
  <img src="https://github.com/user-attachments/assets/be2d4bc7-7482-4c8d-8db7-b7c1fd931371" width="700"/>
</p>

---

## 6. Concluzii

Proiectul arată că se poate construi o aplicație desktop funcțională și coerentă folosind doar mecanismele native ale .NET — fără nicio dependență externă. Câmpuri statice + colecții generice + referințe la obiecte = comportament echivalent cu un strat de date simplu.

**Ce s-a învățat:** encapsulare, gestionarea stării globale, validare tranzacțională fără SQL, separarea responsabilităților între clase.

### Limitarea principală și pasul următor

Datele se pierd la repornire. Pasul natural următor e conectarea la o bază de date (SQL Server sau SQLite), înlocuind metodele din `DepozitProduse` cu interogări SQL. Formularele rămân **neschimbate** — GUI-ul construit acum devine direct reutilizabil.

| Componentă | Acum | Viitor |
|---|---|---|
| `ListaProduse` | `static List<Produs>` in-memory | `SELECT * FROM Produse` |
| `ActualizeazaStoc()` | `produs.Cantitate -= x` pe heap | `UPDATE Produse SET Cantitate=...` |
| `Lista_utilizatori` | `static List<Client/Employee>` | Query cu hash parolă |
| Formularele WinForms | Implementate | **Neschimbate** |

---

## WEBOGRAFIE


*Raport INDV1 — CEITI, 2026. Autor: Găină Valentin, grupa P2333.*
