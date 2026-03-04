using System;
using System.Collections.Generic;

namespace MoldCom // Am lăsat MoldCom ca să se potrivească cu formularul tău Employee
{
    // 1. Clasa Produs unificată (conține atât datele tale, cât și ce are nevoie Employee)
    public class Produs
    {
        public string Cod { get; set; }
        public string Nume { get; set; }
        public string Categorie { get; set; }
        public decimal Pret { get; set; }
        public string Descriere { get; set; }
        public int Cantitate { get; set; }
        public string Locatie { get; set; }
    }

    // 2. Depozitul Central
    public static class DataStore
    {
        // Lista unică, globală
        public static List<Produs> ListaProduse = new List<Produs>();

        public static void InitializeData()
        {
            if (ListaProduse.Count > 0) return;

            // Adăugăm exact produsele tale, completate cu Cod, Cantitate și Locație
            ListaProduse.AddRange(new[]
            {
                new Produs { Cod = "P001", Nume = "Laptop Asus ROGUS", Categorie = "IT & PC", Pret = 25000m, Cantitate = 12, Locatie = "Raft A1", Descriere = "Laptop de gaming performant, 16GB RAM, RTX 4060 TI." },
                new Produs { Cod = "P002", Nume = "Telefon Samsung S23", Categorie = "Telefoane", Pret = 18000m, Cantitate = 4, Locatie = "Raft B2", Descriere = "Smartphone 5G, cameră 50MP, baterie 4500mAh." },
                new Produs { Cod = "P003", Nume = "Monitor Dell 27", Categorie = "Periferice", Pret = 5500m, Cantitate = 8, Locatie = "Raft C3", Descriere = "Monitor 4K UHD, panou IPS, 60Hz." },
                new Produs { Cod = "P004", Nume = "Tastatură Mecanică Logitech", Categorie = "Periferice", Pret = 1200m, Cantitate = 25, Locatie = "Raft A2", Descriere = "Tastatură iluminată RGB, switch-uri liniare silențioase." },
                new Produs { Cod = "P005", Nume = "Căști Sony WH-1000XM5", Categorie = "Audio", Pret = 6500m, Cantitate = 10, Locatie = "Raft B1", Descriere = "Căști over-ear cu anulare activă a zgomotului (ANC)." },
                new Produs { Cod = "P006", Nume = "Mouse Razer DeathAdder", Categorie = "Periferice", Pret = 800m, Cantitate = 3, Locatie = "Raft A3", Descriere = "Mouse de gaming, 20.000 DPI, ergonomic." }
            });
        }
    }
}