using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Priceio.ClasesGenericas;

namespace Priceio.Turnero.Kretz
{
    class Comunicacion
    {
        internal Seguridad seguridad = new Seguridad();

        internal string CrearComandoBascula(string pvStrComando)
        {
            //Optiene el checksum del comando
            string chk = seguridad.GenerarCheckSum(2, pvStrComando, false);
            //Construye la informacion a enviar
            string requestData = seguridad.ConvertirHEXToASCII("2") + pvStrComando + chk + seguridad.ConvertirHEXToASCII("4");
            return requestData;
        }
    }
}
