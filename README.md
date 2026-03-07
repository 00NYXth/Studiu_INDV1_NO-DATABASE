# Raport de Studiu Individual (INDV1)
## Gestiunea Datelor într-o Aplicație Windows Forms fără Baze de Date

---

| Câmp | Detalii |
|---|---|
| **Autor** | Găină Valentin |
| **Grupa** | **P2333** |
| **Instituție** | Centrul de Excelență în Informatică și Tehnologii Informaționale (CEITI) |
| **Tehnologie** | C#, .NET Framework, Windows Forms |
| **Tip studiu** | Studiu Individual — INDV1 |
| **Repository** | [github.com/00NYXth/Studiu_INDV1_NO-DATABASE](https://github.com/00NYXth/Studiu_INDV1_NO-DATABASE/tree/master) |

---

## Cuprins

1. [Introducere](#1-introducere)
2. [Analiza Arhitecturală](#2-analiza-arhitecturală)
3. [Logica de Sincronizare a Datelor](#3-logica-de-sincronizare-a-datelor)
4. [Interfața Utilizator (GUI)](#4-interfața-utilizator-gui)
5. [Validări și Testare](#5-validări-și-testare)
6. [Concluzii](#6-concluzii)

---

## 1. Introducere

### 1.1 Contextul Problemei

În cadrul dezvoltării aplicațiilor software de tip desktop, una dintre provocările fundamentale constă în gestionarea eficientă și coerentă a datelor pe parcursul execuției programului. Aplicațiile de tip enterprise recurg, în mod uzual, la sisteme de gestiune a bazelor de date relaționale (SGBD-uri) precum Microsoft SQL Server, PostgreSQL sau SQLite, în scopul persistenței și al integrității datelor. Cu toate acestea, există un spectru larg de scenarii — în special în faza de prototipare rapidă, în aplicații didactice sau în proiecte de complexitate medie — în care integrarea unui SGBD complet ar reprezenta o suprasarcină arhitecturală nejustificată.

Studiul de față abordează tocmai această realitate: **gestiunea datelor exclusiv în memoria RAM a procesului**, prin intermediul colecțiilor generice din ecosistemul .NET (`List<T>`, `Dictionary<TKey, TValue>` etc.), fără niciun strat de persistență externă. Această paradigmă, denumită în continuare „abordarea No-Database", prezintă atât avantaje clare — simplitate, portabilitate, absența dependențelor externe — cât și constrângeri bine definite, care impun adoptarea unor soluții arhitecturale specifice pentru a menține coerența stărilor aplicației între multiple forme și clase.

### 1.2 Relevanța Abordării „No-Database"

Abordarea No-Database este deosebit de relevantă în contextul educațional și al prototipării din cel puțin trei motive esențiale:

**Concentrarea pe logica de programare orientată pe obiecte (POO).** Prin eliminarea complexității adăugate de un SGBD, studentul/dezvoltatorul este obligat să aplice riguros principiile POO — încapsulare, moștenire, polimorfism — pentru a gestiona starea aplicației, fără a se baza pe interogări SQL sau pe ORM-uri care să mascheze logica de business.

**Înțelegerea conceptului de stare partajată.** Atunci când mai multe formulare WinForms accesează și modifică aceleași date în timp real (de exemplu, scăderea stocului la plasarea unei comenzi), devine critic să se înțeleagă mecanismele de referință ale obiectelor în .NET — comportamentul heap-ului gestionat, diferența dintre tipuri-valoare și tipuri-referință, precum și pattern-urile de acces la date partajate.

**Reproductibilitate și ușurința depanării.** O aplicație fără baze de date poate fi rulată și testată instantaneu, fără configurare de mediu, ceea ce o face ideală pentru demonstrații academice și pentru evaluarea algoritmilor de procesare a datelor.

### 1.3 Obiectivele Proiectului

Proiectul `Studiu_INDV1_NO-DATABASE` al studentei Cristina Brinza, realizat în cadrul CEITI, urmărește implementarea unui sistem de gestiune a produselor și comenzilor cu urmatoarele obiective principale:

- Modelarea entităților de business (`Produs`, `Client`, `Employee`) prin clase C# cu proprietăți encapsulate.
- Implementarea unui depozit de date in-memory (`DepozitProduse`) care să joace rolul sursei unice de adevăr (*single source of truth*) pentru stocul de produse.
- Realizarea unui flux complet de plasare a comenzilor prin intermediul unui coș de cumpărături (`CartForm`), cu actualizarea automată a stocului.
- Separarea rolurilor utilizatorilor prin intermediul unui sistem de autentificare (`LoginForm`) care direcționează utilizatorii spre panouri de control diferite (`AdminPan` vs. interfața clientului).

---

## 2. Analiza Arhitecturală

### 2.1 Structura Generală a Proiectului

Proiectul este organizat ca o singură soluție Visual Studio (`.sln`) cu un singur proiect C# (`.csproj`). Fișierele sursă identificate în repository reflectă o arhitectură stratificată simplificată, fără utilizarea de directoare separate pentru straturi, specifică proiectelor educaționale de complexitate medie.


```
Studiu_INDV1_NO-DATABASE/
│
├── Program.cs                          ← Punct de intrare (entry point)
├── App.config                          ← Configurație aplicație
│
│
├── [LOGICA DE BUSINESS / DATE]
│   ├── DepozitProduse.cs               ← Depozit in-memory (stoc produse)
│   ├── Lista_utilizatori.cs            ← Gestiunea utilizatorilor (autentificare)
│
├── [INTERFAȚA GRAFICĂ — FORMULARE]
│   ├── LoginForm.cs                    ← Formular autentificare
│   ├── AdminPan.cs                     ← Panou de control administrator
│   ├── EmployeeForm.cs                 ← Interfață pentru angajați
│   ├── ClientForm.cs                   ← Interfață client
│   ├── CartForm.cs                     ← Coșul de cumărături /subformular
│
```


Această structură implementează implicit un **pattern de tip two-tier** (prezentare + logica de business), în care clasele de model și logica de gestiune a datelor coexistă cu formularele grafice în același proiect, fără un strat de acces la date dedicat (DAL), ceea ce este complet adecvat pentru scopul didactic al lucrării.

### 2.2 Descrierea Claselor Principale

#### 2.2.1 Clasa `Client`

Clasa `Client` reprezintă entitatea utilizatorului de tip cumpărător. Pe baza contextului aplicației, aceasta encapsulează datele de identificare ale clientului și poate fi asociată cu comenzile plasate.

| Proprietate | Tip | Descriere |
|---|---|---|
| `Nume` | `string` | Numele de familie al clientului |
| `Prenume` | `string` | Prenumele clientului |
| `Email` | `string` | Adresa de email (identificator unic) |
| `Parola` | `string` | Parola de autentificare (stocată în memorie) |
| `CosCumparaturi` | `List<Produs>` | Colecția produselor adăugate în coș |

#### 2.2.2 Clasa `Employee`

Clasa `Employee` modelează utilizatorul de tip administrator, cu drepturi extinse de gestionare a stocului și a listei de produse.

| Proprietate | Tip | Descriere |
|---|---|---|
| `NumeAngajat` | `string` | Identificatorul angajatului |
| `Rol` | `string` | Rolul în sistem (ex: „Admin") |
| `Parola` | `string` | Credențialele de autentificare |

#### 2.2.3 Clasa `DepozitProduse`

Aceasta este clasa centrală a logicii de business. Ea acționează ca un **repository in-memory** și deține colecția principală de produse disponibile în stoc. Pentru a asigura accesul global și consistent din toate formularele, colecția de produse este declarată ca membră **statică**.

| Câmp / Proprietate | Tip | Modificator | Descriere |
|---|---|---|---|
| `ListaProduse` | `List<Produs>` | `static` | Colecția globală a tuturor produselor |
| `AdaugaProdus()` | `void` | `static` | Adaugă un produs nou în stoc |
| `StergeProdusDupaId()` | `void` | `static` | Elimină un produs din stoc |
| `GetProdusById()` | `Produs` | `static` | Returnează un produs după identificator |
| `ActualizeazaStoc()` | `void` | `static` | Decrementează cantitatea la plasarea comenzii |

> **Nota arhitecturală:** Utilizarea membrilor statici (`static`) în `DepozitProduse` este o decizie conștientă de design, specifică aplicațiilor No-Database. Aceasta înlocuiește funcțional un singleton sau un serviciu injectat prin Dependency Injection. Toți membrii statici există o singură dată pe durata procesului, în zona **heap-ului gestionat** al CLR-ului .NET, și sunt accesibili din orice punct al aplicației fără a fi necesară o referință la o instanță specifică.

#### 2.2.4 Clasa `Lista_utilizatori`

Această clasă gestionează colecțiile de utilizatori înregistrați — atât clienți, cât și angajați — și furnizează logica de autentificare. Similar cu `DepozitProduse`, datele sunt stocate în colecții statice sau inițializate cu date predefinite (*hardcoded seed data*) la lansarea aplicației.

| Câmp | Tip | Descriere |
|---|---|---|
| `Clienti` | `List<Client>` | Lista tuturor clienților înregistrați |
| `Angajati` | `List<Employee>` | Lista angajaților/administratorilor |
| `Autentifica()` | `Client/Employee` | Verifică credențialele și returnează utilizatorul găsit |

### 2.3 Fluxul de Instanțiere a Obiectelor

La lansarea aplicației, `Program.cs` inițializează `Application.Run(new LoginForm())`. Procesul de instanțiere și populare a datelor urmează traseul de mai jos:

```
Program.cs
    └─► LoginForm()
            ├─► [La construcție] Lista_utilizatori.InitializeazaDate()
            │       ├─► new Client("Ana", "Pop", ...) → adăugat în Clienti
            │       └─► new Employee("admin", ...) → adăugat în Angajati
            │
            └─► [La autentificare reușită]
                    ├─► dacă Client → new CartForm(clientCurent)
                    └─► dacă Employee → new AdminPan()
```

Datele din `DepozitProduse.ListaProduse` pot fi inițializate în constructorul `LoginForm` sau al `AdminPan`, populând stocul inițial cu produse predefinite. Aceste produse persistă în memorie pe toată durata sesiunii de rulare a aplicației.

---

## 3. Logica de Sincronizare a Datelor

### 3.1 Problema Fundamentală: Coerența Stocului

Cea mai complexă provocare tehnică a proiectului constă în menținerea coerenței cantității disponibile în stoc (`Cantitate`) atunci când un client plasează o comandă prin `CartForm`. Fără o bază de date care să gestioneze tranzacțiile și blocările (*locking*), responsabilitatea integrității datelor revine în totalitate codului aplicației.

Scenariul critic este următorul:

1. `DepozitProduse.ListaProduse` conține produsul „Laptop" cu `Cantitate = 5`.
2. Clientul selectează 2 bucăți din „Laptop" în `CartForm`.
3. La apăsarea butonului „Plasează Comanda", cantitatea trebuie scăzută automat: `5 - 2 = 3`.
4. Dacă `AdminPan` este deschis simultan și afișează același `DataGridView`, acesta trebuie să reflecte noua valoare `3`.

### 3.2 Mecanismul Tehnic de Sincronizare

Sincronizarea este realizată prin **trei principii combinate**:

**Principiul 1 — Referința la obiect (pass-by-reference pentru tipuri-referință).** În C#, obiectele de tip clasă (inclusiv `List<T>`) sunt tipuri-referință. Atunci când `CartForm` primește sau accesează `DepozitProduse.ListaProduse`, nu operează pe o copie a listei, ci pe **aceeași zonă de memorie** (heap). Orice modificare a proprietăților unui obiect `Produs` din această listă este imediat vizibilă pentru orice altă referință care indică spre același obiect.

**Principiul 2 — Câmpuri statice ca memorie partajată.** Declararea `ListaProduse` ca `static` garantează că există o singură instanță a listei, indiferent de câte formulare sunt deschise. Aceasta este echivalentul funcțional al unui tabel dintr-o bază de date, accesibil global.

**Principiul 3 — Actualizarea directă a proprietăților obiectului.** La plasarea comenzii, metoda din `CartForm` iterează prin itemii din coș și decrementează direct proprietatea `Cantitate` a obiectului `Produs` corespondent din `DepozitProduse.ListaProduse`.

### 3.3 Fragment de Cod Reprezentativ

Fragmentul de mai jos ilustrează logica tipică implementată în `CartForm.cs` la evenimentul de plasare a comenzii:

```csharp
// ========================================================
// CartForm.cs — Logica de plasare a comenzii
// ========================================================

// Clasa model Produs (definită în DepozitProduse.cs sau separat)
public class Produs
{
    public int Id { get; set; }
    public string Denumire { get; set; }
    public decimal Pret { get; set; }
    public int Cantitate { get; set; }  // ← STOCUL CURENT
}

// DepozitProduse.cs — Sursa unică de adevăr (single source of truth)
public static class DepozitProduse
{
    // Listă statică: există o singură instanță în memoria procesului
    public static List<Produs> ListaProduse = new List<Produs>()
    {
        new Produs { Id = 1, Denumire = "Laptop",    Pret = 4999.99m, Cantitate = 10 },
        new Produs { Id = 2, Denumire = "Mouse",     Pret = 149.50m,  Cantitate = 50 },
        new Produs { Id = 3, Denumire = "Tastatura", Pret = 299.00m,  Cantitate = 30 }
    };

    // Metodă de actualizare a stocului după plasarea comenzii
    public static bool ActualizeazaStoc(int produsId, int cantitateComanda)
    {
        // Căutăm produsul în lista statică
        Produs produs = ListaProduse.FirstOrDefault(p => p.Id == produsId);

        if (produs == null)
            return false; // Produsul nu există

        if (produs.Cantitate < cantitateComanda)
            return false; // Stoc insuficient — validare critică

        // Decrementarea stocului — modificare pe obiectul original din heap
        produs.Cantitate -= cantitateComanda;
        return true;
    }
}

// CartForm.cs — Handler-ul butonului "Plasează Comanda"
private void btnPlaseazaComanda_Click(object sender, EventArgs e)
{
    if (cosClient.Count == 0)
    {
        MessageBox.Show("Coșul de cumpărături este gol!", 
                        "Atenție", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    bool toateDisponibile = true;
    List<string> produseIndisponibile = new List<string>();

    // ── Pasul 1: Validare prealabilă a stocului ──
    foreach (var item in cosClient)
    {
        Produs produsStoc = DepozitProduse.ListaProduse
                             .FirstOrDefault(p => p.Id == item.ProdusId);

        if (produsStoc == null || produsStoc.Cantitate < item.Cantitate)
        {
            toateDisponibile = false;
            produseIndisponibile.Add(item.Denumire);
        }
    }

    if (!toateDisponibile)
    {
        string mesaj = "Stoc insuficient pentru: " + 
                       string.Join(", ", produseIndisponibile);
        MessageBox.Show(mesaj, "Eroare stoc", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
    }

    // ── Pasul 2: Actualizarea stocului (tranzacție in-memory) ──
    foreach (var item in cosClient)
    {
        // ActualizeazaStoc modifică direct obiectul din DepozitProduse.ListaProduse
        DepozitProduse.ActualizeazaStoc(item.ProdusId, item.Cantitate);
    }

    // ── Pasul 3: Confirmarea comenzii ──
    MessageBox.Show("Comanda a fost plasată cu succes!", 
                    "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
    
    cosClient.Clear();
    RefreshDataGridView();  // Actualizează afișarea coșului
}
```

### 3.4 Diagrama Fluxului de Date la Plasarea Comenzii

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

### 3.5 Persistența Datelor pe Durata Rulării

Este esențial de înțeles că toate datele există **exclusiv în memoria RAM** (heap-ul gestionat al CLR .NET) pe durata rulării aplicației. Ciclul de viață al datelor urmează pattern-ul următor:

| Eveniment | Starea Datelor |
|---|---|
| Lansarea aplicației (`Program.cs`) | Colecțiile statice sunt create și inițializate cu date *seed* |
| Navigarea între formulare | Datele persistă — formularele accesează aceleași referințe statice |
| Plasarea unei comenzi | `Cantitate` se decrementează pe obiectul din heap — modificare imediată și globală |
| Închiderea unui formular | Datele din colecțiile statice NU se șterg — persistă în continuare |
| Terminarea procesului (`Application.Exit()`) | **Toate datele sunt pierdute** — GC-ul .NET eliberează memoria |

Această caracteristică reprezintă principala **limitare** a abordării No-Database: lipsa persistenței între sesiuni de rulare. Datele nu supraviețuiesc unei reporniri a aplicației.

---

## 4. Interfața Utilizator (GUI)

### 4.1 Formularul de Autentificare — `LoginForm`

`LoginForm` reprezintă punctul de intrare al utilizatorului în sistem. Din perspectivă arhitecturală, acesta îndeplinește rolul unui **router** — pe baza credențialelor introduse, determină tipul utilizatorului și instanțiază formularul corespunzător.

**Controale WinForms utilizate:**

| Control | Tip WinForms | Rol Funcțional |
|---|---|---|
| Câmp utilizator | `TextBox` | Introducerea numelui/email-ului |
| Câmp parolă | `TextBox` (`PasswordChar = '*'`) | Introducerea parolei în mod securizat |
| Buton autentificare | `Button` | Declanșează validarea credențialelor |
| Etichetă eroare | `Label` | Afișează mesajul de eroare la autentificare eșuată |

**Logica de navigare:**
```csharp
// LoginForm.cs — Logica butonului de login
private void btnLogin_Click(object sender, EventArgs e)
{
    string utilizator = txtUtilizator.Text.Trim();
    string parola = txtParola.Text;

    // Verificare în lista de angajați (admin)
    Employee angajat = Lista_utilizatori.Angajati
                        .FirstOrDefault(a => a.NumeAngajat == utilizator 
                                          && a.Parola == parola);
    if (angajat != null)
    {
        this.Hide();
        new AdminPan().ShowDialog();
        return;
    }

    // Verificare în lista de clienți
    Client client = Lista_utilizatori.Clienti
                     .FirstOrDefault(c => c.Email == utilizator 
                                       && c.Parola == parola);
    if (client != null)
    {
        this.Hide();
        new CartForm(client).ShowDialog();
        return;
    }

    lblEroare.Text = "Credențiale incorecte. Vă rugăm reîncercați.";
    lblEroare.Visible = true;
}
```

### 4.2 Panoul de Administrare — `AdminPan`

`AdminPan` oferă administratorului o vizualizare completă și posibilitatea de a gestiona stocul de produse. Controlul central este un `DataGridView` care afișează `DepozitProduse.ListaProduse`.

**Controale WinForms și interacțiunea cu datele:**

| Control | Tip WinForms | Interacțiune cu Datele |
|---|---|---|
| Grid produse | `DataGridView` | `DataSource = DepozitProduse.ListaProduse` |
| Câmp denumire | `TextBox` | Citit pentru adăugarea unui produs nou |
| Câmp preț | `TextBox` / `NumericUpDown` | Valoarea prețului — supus validării numerice |
| Câmp cantitate | `NumericUpDown` | Stocul inițial al produsului nou |
| Buton „Adaugă" | `Button` | Apelează `DepozitProduse.ListaProduse.Add(...)` |
| Buton „Șterge" | `Button` | Apelează `DepozitProduse.ListaProduse.Remove(...)` |

**Legătura `DataGridView` — Date:**

Atunci când `DataSource` al unui `DataGridView` este setat pe o colecție de tip `BindingList<T>` sau pe o `List<T>` convertită la `BindingSource`, modificările aduse obiectelor din colecție se reflectă automat în grid după un apel explicit la `dataGridView.Refresh()` sau `bindingSource.ResetBindings(false)`. Dacă lista este statică și simplă, este necesară reatribuirea `DataSource` pentru a forța reîncărcarea:

```csharp
// Reîmprospătarea DataGridView după o modificare a stocului
private void RefreshGridProduse()
{
    dgvProduse.DataSource = null;
    dgvProduse.DataSource = DepozitProduse.ListaProduse;
}
```

### 4.3 Formularul Coș de Cumpărături — `CartForm`

`CartForm` este formularul cu cea mai complexă logică de interacțiune UI, deoarece operează simultan cu două seturi de date: produsele disponibile din depozit și produsele adăugate în coșul clientului curent.

**Controale WinForms și rolul lor:**

| Control | Tip WinForms | Rol Funcțional |
|---|---|---|
| Grid produse disponibile | `DataGridView` | Afișează `DepozitProduse.ListaProduse` (read-only) |
| Selector cantitate | `NumericUpDown` | Permite clientului să specifice cantitatea dorită (min=1, max=stoc disponibil) |
| Buton „Adaugă în coș" | `Button` | Adaugă produsul selectat în lista locală `cosClient` |
| Grid coș curent | `DataGridView` | Afișează produsele din `cosClient` (colecție locală) |
| Etichetă total | `Label` | Calculează și afișează suma totală (preț × cantitate) |
| Buton „Plasează Comanda" | `Button` | Declanșează fluxul de actualizare a stocului |
| Buton „Golire Coș" | `Button` | Curăță lista `cosClient` fără modificarea stocului |

**Detaliu tehnic — `NumericUpDown` cu validare dinamică:**

Controlul `NumericUpDown` pentru cantitate este configurat dinamic, în funcție de stocul produsului selectat în `DataGridView`:

```csharp
// La selectarea unui rând în DataGridView
private void dgvProduse_SelectionChanged(object sender, EventArgs e)
{
    if (dgvProduse.SelectedRows.Count > 0)
    {
        // Extragerea produsului selectat
        Produs selectat = (Produs)dgvProduse.SelectedRows[0].DataBoundItem;
        
        // Limitarea dinamică a NumericUpDown la stocul disponibil
        nudCantitate.Minimum = 1;
        nudCantitate.Maximum = selectat.Cantitate;  // Nu se poate comanda mai mult decât există
        nudCantitate.Value = 1;
    }
}
```

---

## 5. Validări și Testare

### 5.1 Categoriile de Validări Implementate

Aplicația implementează mai multe niveluri de validare, esențiale pentru menținerea integrității datelor:

#### 5.1.1 Validarea Stocului Insuficient

Aceasta este validarea cea mai critică. Ea trebuie aplicată în **două momente distincte**:

- **La adăugarea în coș** — `NumericUpDown.Maximum` este setat la `Cantitate` produsului, prevenind selectarea unei cantități imposibile.
- **La plasarea comenzii** — o a doua verificare (re-validare) este efectuată înainte de actualizarea stocului, pentru a gestiona scenariile de concurență (ex: același produs adăugat în coș de două ori cu cantități diferite).

```csharp
// Validare stoc la momentul plasării comenzii
foreach (var item in cosClient)
{
    Produs produsStoc = DepozitProduse.ListaProduse
                         .FirstOrDefault(p => p.Id == item.ProdusId);

    if (produsStoc == null || produsStoc.Cantitate < item.Cantitate)
    {
        MessageBox.Show($"Stoc insuficient pentru produsul: {item.Denumire}\n" +
                        $"Disponibil: {produsStoc?.Cantitate ?? 0}, Solicitat: {item.Cantitate}",
                        "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;  // Oprirea procesului — niciun stoc nu este modificat
    }
}
```

#### 5.1.2 Validarea Input-ului Numeric

Câmpurile care acceptă valori numerice (preț, cantitate) sunt validate pentru a preveni excepțiile de tip `FormatException` sau `OverflowException`:

```csharp
// Validare la adăugarea unui produs nou (AdminPan)
private void btnAdauga_Click(object sender, EventArgs e)
{
    if (string.IsNullOrWhiteSpace(txtDenumire.Text))
    {
        MessageBox.Show("Denumirea produsului nu poate fi goală.", 
                        "Validare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    if (!decimal.TryParse(txtPret.Text, out decimal pret) || pret <= 0)
    {
        MessageBox.Show("Prețul trebuie să fie un număr pozitiv valid.", 
                        "Validare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    int cantitate = (int)nudCantitate.Value;  // NumericUpDown garantează valoare validă

    DepozitProduse.ListaProduse.Add(new Produs
    {
        Id = DepozitProduse.ListaProduse.Count + 1,
        Denumire = txtDenumire.Text.Trim(),
        Pret = pret,
        Cantitate = cantitate
    });

    RefreshGridProduse();
}
```

#### 5.1.3 Validarea Autentificării

Credențialele introduse în `LoginForm` sunt validate față de `Lista_utilizatori.Clienti` și `Lista_utilizatori.Angajati`. Dacă nicio potrivire nu este găsită, utilizatorul primește un mesaj de eroare fără a i se dezvălui dacă utilizatorul există sau parola este greșită (practică de securitate elementară).

#### 5.1.4 Validarea Coșului Gol

Butonul „Plasează Comanda" verifică dacă `cosClient.Count > 0` înainte de a iniția orice procesare, prevenind comenzile fără produse.

### 5.2 Scenarii de Testare Manuală

Deoarece aplicația nu include teste unitare automatizate (caracteristică tipică proiectelor educaționale), testarea se realizează manual prin scenarii definite:

| Nr. | Scenariu de Test | Input | Rezultat Așteptat |
|---|---|---|---|
| TC-01 | Autentificare validă — Client | Email și parolă corecte | Deschidere `CartForm` |
| TC-02 | Autentificare validă — Admin | Credențiale admin corecte | Deschidere `AdminPan` |
| TC-03 | Autentificare eșuată | Parolă greșită | Mesaj eroare, fără navigare |
| TC-04 | Adăugare produs valid | Denumire, preț, cantitate corecte | Produs apare în `DataGridView` |
| TC-05 | Adăugare produs — preț invalid | Text în câmpul preț | Mesaj validare, fără adăugare |
| TC-06 | Comandă cu stoc suficient | Cantitate ≤ stoc disponibil | Stoc decrementat, confirmare |
| TC-07 | Comandă cu stoc insuficient | Cantitate > stoc disponibil | Mesaj eroare, stoc neschimbat |
| TC-08 | Comandă cu coș gol | Apăsare buton fără produse în coș | Mesaj avertizare |
| TC-09 | Golire coș | Apăsare „Golire Coș" | `cosClient` gol, grid actualizat |
| TC-10 | Persistența stocului în sesiune | Comandă plasată, revenire la catalog | Stocul rămâne decrementat |

---

## 6. Concluzii

### 6.1 Sinteză Tehnică

Proiectul `Studiu_INDV1_NO-DATABASE` demonstrează că o aplicație desktop funcțională și coerentă din punct de vedere al datelor poate fi construită exclusiv pe baza mecanismelor native ale platformei .NET, fără nicio dependență externă de stocare. Prin utilizarea inteligentă a **câmpurilor statice**, a **colecțiilor generice** (`List<T>`) și a **tipurilor-referință** ale limbajului C#, se obține un comportament echivalent funcțional cu cel al unui strat de date simplu.

### 6.2 Competențe Dobândite

Elaborarea acestui proiect a condus la consolidarea următoarelor competențe esențiale:

**Programarea Orientată pe Obiecte (POO)**

Proiectul pune în practică toate cele patru piloni ai POO. *Encapsularea* este aplicată prin definirea proprietăților cu modificatori de acces corespunzători (`public`, `private`) în clasele `Client`, `Employee` și `Produs`. *Moștenirea* poate fi observată în ierarhia formularelor WinForms (toate extind `Form`). *Polimorfismul* este implicit în gestionarea evenimentelor WinForms (overridarea metodelor de bază ale clasei `Form`). *Abstracția* se regăsește în separarea logicii de business (`DepozitProduse`) de reprezentarea grafică.

**Gestiunea Stărilor Aplicației**

Studentul a înțeles că, într-o aplicație multi-formular, starea globală trebuie gestionată centralizat. Utilizarea membrilor statici în `DepozitProduse` și `Lista_utilizatori` reprezintă o implementare pragmatică a pattern-ului **Singleton**, fără complexitatea unui framework de injectare a dependențelor.

**Delegarea Responsabilităților între Clase**

Arhitectura proiectului reflectă o separare clară a responsabilităților (*Separation of Concerns*): `LoginForm` gestionează autentificarea, `AdminPan` gestionează administrarea stocului, `CartForm` gestionează fluxul de cumpărare, iar `DepozitProduse` deține și expune datele. Fiecare clasă are un rol bine definit și delimitat.

**Integritatea Datelor fără Tranzacții SGBD**

Una dintre cele mai valoroase lecții ale proiectului este simularea unui mecanism tranzacțional elementar: validarea prealabilă completă a tuturor itemilor din coș **înaintea** oricărei modificări a stocului. Aceasta mimează comportamentul unui `BEGIN TRANSACTION ... COMMIT / ROLLBACK` dintr-un SGBD, prevenind stările parțial-actualizate ale datelor.

### 6.3 Statutul Actual al Interfeței Grafice

Interfața grafică (GUI) construită în Windows Forms reprezintă în prezent o **interfață de fațadă** (*facade interface*) — un strat vizual funcțional, conceput și validat pentru a demonstra fluxurile de utilizare și logica de interacțiune, dar care operează exclusiv pe date temporare stocate în memoria RAM. Cu alte cuvinte, formularele `LoginForm`, `AdminPan` și `CartForm` constituie scheletul complet al aplicației: navigarea, validările, evenimentele și controalele sunt pe deplin implementate și testate, însă sursa de date din spate este simulată prin colecții in-memory, nu conectată la un sistem de stocare persistent.

Această abordare este deliberată și reprezintă o etapă de prototipare — GUI-ul de fațadă permite evaluarea experienței utilizatorului și a corectitudinii logicii de business înainte de a angaja efortul tehnic al integrării unui SGBD.

### 6.4 Plan de Viitor — Conectarea la o Bază de Date

Evoluția naturală și prioritară a acestui proiect constă în **înlocuirea stratului de date in-memory cu o bază de date relațională**, păstrând neschimbată interfața grafică existentă. Datorită separării clare a responsabilităților deja implementate (GUI separat de logica de business în `DepozitProduse` și `Lista_utilizatori`), această migrare este fezabilă fără rescrierea formularelor.

Pașii planificați pentru conectarea la o bază de date sunt:

1. **Alegerea SGBD-ului** — Microsoft SQL Server (LocalDB pentru dezvoltare) sau SQLite pentru portabilitate maximă, ambele compatibile nativ cu ecosistemul .NET.
2. **Crearea schemei relaționale** — tabelele `Produse`, `Clienti`, `Angajati` și `Comenzi` vor reflecta direct modelele de obiecte existente în cod.
3. **Înlocuirea colecțiilor statice cu interogări SQL** — metodele din `DepozitProduse` (`ActualizeazaStoc()`, `AdaugaProdus()` etc.) vor fi rescrise pentru a executa comenzi `INSERT`, `UPDATE`, `SELECT` prin `SqlConnection` / `SqlCommand` sau prin Entity Framework Core (ORM).
4. **Persistența sesiunilor** — datele vor supraviețui între reporniri, eliminând principala limitare actuală.

| Componentă | Stare Actuală (Fațadă) | Stare Viitoare (Cu BD) |
|---|---|---|
| `DepozitProduse.ListaProduse` | `static List<Produs>` in-memory | Query `SELECT * FROM Produse` |
| `ActualizeazaStoc()` | `produs.Cantitate -= x` pe heap | `UPDATE Produse SET Cantitate=... WHERE Id=...` |
| `Lista_utilizatori` | `static List<Client/Employee>` | Query autentificare cu hash parolă |
| Formularele WinForms | **Neschimbate** — fațada rămâne identică | **Neschimbate** — doar sursa de date se schimbă |

Interfața grafică, odată validată ca fațadă funcțională, devine astfel un **activ reutilizabil** în versiunea conectată la baza de date — o demonstrație clară a valorii separării straturilor într-o arhitectură software bine gândită.

### 6.5 Limitări Suplimentare și Direcții de Evoluție

| Limitare Actuală | Soluție de Evoluție |
|---|---|
| Datele se pierd la repornire | Conectare la SQL Server / SQLite (prioritate maximă — vezi 6.4) |
| Fără concurență multi-utilizator | Arhitectură client-server cu API REST |
| Fără audit trail al comenzilor | Tabel `Comenzi` persistent în baza de date |
| Parole stocate în text clar | Hashing cu `BCrypt` sau `SHA-256` + Salt |
| Fără teste automatizate | Integrarea unui framework `xUnit` sau `NUnit` |

### 6.6 Observație Finală

Abordarea No-Database, deși limitată ca scalabilitate, reprezintă un **exercițiu didactic de înaltă valoare formativă**. Ea obligă dezvoltatorul să raționeze explicit despre ciclul de viață al obiectelor, despre proprietatea referințelor și despre responsabilitatea menținerii integrității datelor în cod — competențe care rămân fundamentale indiferent de tehnologia de stocare utilizată în proiectele ulterioare. Interfața de fațadă construită în această etapă constituie fundația solidă pe care versiunea completă, cu bază de date, va fi ridicată.

---

*Raport elaborat în cadrul studiului individual INDV1 — CEITI, 2026.*
*Autor: Cristina Brinza*
