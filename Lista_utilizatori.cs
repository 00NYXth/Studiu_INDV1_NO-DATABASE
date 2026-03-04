using MoldCom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDI_Test_Figuri
{
    public class USER
    {
        public string username { get; set; }
        public string password { get; set; } 
        
        public string rol { get; set; }
    }

    public static class ListaUser
    {
        public static List<USER> ListaUSR = new List<USER>();

        public static void InitializeData()
        {
            if (ListaUSR.Count > 0)
                return;

            ListaUSR.AddRange(new[]
            {
                new USER { username = "admin", password = "123", rol = "Admin" },
                new USER { username = "angajat", password  = "123", rol = "Employee" },
                new USER { username = "angajat1", password = "222", rol = "Employee"},
                new USER { username = "client", password = "123", rol = "Client" },
                new USER { username = "client1", password = "999", rol = "Client"}
            });
        }
    }

}
