using DataGate;
using Org.BouncyCastle.Asn1.X509;
using Precios_Turnos;
using Priceio.ClasesSQLite;
using Priceio.SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Priceio.Cajero
{
    internal class CambioSeparado
    {
        internal int monedas;          // Valor total en monedas
        internal int billetes;         // Valor total en billetes (lo que payout necesita)
        internal int cantidadBilletes; // Cantidad total de billetes usados

        internal Dictionary<int, int> detalleMonedas;   // detalle: 10,5,2,1
        internal Dictionary<int, int> detalleBilletes;  // detalle: 200,100,50,20

        internal CambioSeparado()
        {
            monedas = 0;
            billetes = 0;
            cantidadBilletes = 0;
            detalleMonedas = new Dictionary<int, int>();
            detalleBilletes = new Dictionary<int, int>();
        }
    }

    internal class ValidarCambio
    {
        internal int billetesCambio = 0;
        internal int monedasCambio = 0;
        internal int cambioMaximoMonedas = 0;
        internal int cambioMaximoBilletes = 0;

        internal ValidarCambio()
        {
            ConfiguracionCanalesHopper SQLiteClass = new SQLiteClassManager().GetConfiguracionCanalesHopper();
            if (SQLiteClass != null)
            {
                cambioMaximoMonedas = SQLiteClass.PagoMax;
            }
            ConfiguracionCanalesPayout SQLiteClass2 = new SQLiteClassManager().GetConfiguracionCanalesPayout();
            if (SQLiteClass2 != null)
            {
                cambioMaximoBilletes = SQLiteClass2.PagoMax;  // máximo de BILLETES que puedo pagar
            }
        }

        internal bool calculaCambio(int cambio, List<ChannelData>? chanelsDataHopper, List<ChannelData>? chanelsDataPayout)
        {
            List<ChannelData> chanelsData = chanelsDataPayout.ConvertAll(x =>
                new ChannelData { Channel = x.Channel, Currency = x.Currency, Level = x.Level, Recycling = x.Recycling, Value = x.Value }).ToList();

            CambioSeparado cambioSeparado = SepararCambio(cambio);

            // VALIDACIÓN CORRECTA: evaluar CANTIDAD DE BILLETES, no el valor total
            if (cambioSeparado.cantidadBilletes <= cambioMaximoBilletes || cambioMaximoBilletes == 0)
            {
                int totalCambioFaltante = 0;

                // Pago total de billetes (VALOR TOTAL)
                if (cambioSeparado.billetes > 0)
                    totalCambioFaltante = cambioPayout(cambioSeparado.billetes, chanelsData);

                bool cambioMonedas = true;

                // Si no hay faltante y no se deben monedas → todo OK
                if (totalCambioFaltante == 0 && cambioSeparado.monedas == 0)
                    return true;

                // Validación de monedas (total de monedas = monedas + faltante de billetes)
                if (cambioSeparado.monedas + totalCambioFaltante <= cambioMaximoMonedas || cambioMaximoMonedas == 0)
                {
                    chanelsData = chanelsDataHopper.ConvertAll(x =>
                        new ChannelData { Channel = x.Channel, Currency = x.Currency, Level = x.Level, Recycling = x.Recycling, Value = x.Value }).ToList();

                    if (totalCambioFaltante + cambioSeparado.monedas > 0)
                        cambioMonedas = cambioHopper(totalCambioFaltante + cambioSeparado.monedas, chanelsData);
                    else
                        cambioMonedas = false;

                    if (cambioMonedas)
                        return true;
                }
            }

            return false;
        }

        private CambioSeparado SepararCambio(int cambio)
        {
            CambioSeparado sepa = new CambioSeparado();
            int restante = cambio;

            // BILLETES
            int[] billetes = new int[] { 200, 100, 50, 20 };
            foreach (int b in billetes)
            {
                int cantidad = restante / b;
                if (cantidad > 0)
                {
                    sepa.detalleBilletes[b] = cantidad;
                    sepa.billetes += cantidad * b;     // VALOR TOTAL
                    sepa.cantidadBilletes += cantidad; // CANTIDAD TOTAL
                    restante -= cantidad * b;
                }
            }

            // MONEDAS
            int[] monedas = new int[] { 10, 5, 2, 1 };
            foreach (int m in monedas)
            {
                int cantidad = restante / m;
                if (cantidad > 0)
                {
                    sepa.detalleMonedas[m] = cantidad;
                    sepa.monedas += cantidad * m;      // valor total
                    restante -= cantidad * m;
                }
            }

            return sepa;
        }

        internal bool cambioHopper(int total, List<ChannelData> chanelsDataHopper)
        {
            int totalMonedas = 0;

            foreach (ChannelData chanelData in chanelsDataHopper.OrderByDescending(x => x.Channel))
            {
                if (totalMonedas < total)
                {
                    switch (chanelData.Channel)
                    {
                        case 1: // $1
                            if (chanelData.Recycling)
                                while (chanelData.Level > 0 && totalMonedas + 1 <= total)
                                { totalMonedas += 1; chanelData.Level--; }
                            break;

                        case 2: // $2
                            if (chanelData.Recycling)
                                while (chanelData.Level > 0 && totalMonedas + 2 <= total)
                                { totalMonedas += 2; chanelData.Level--; }
                            break;

                        case 3: // $5
                            if (chanelData.Recycling)
                                while (chanelData.Level > 0 && totalMonedas + 5 <= total)
                                { totalMonedas += 5; chanelData.Level--; }
                            break;

                        case 4: // $10
                            if (chanelData.Recycling)
                                while (chanelData.Level > 0 && totalMonedas + 10 <= total)
                                { totalMonedas += 10; chanelData.Level--; }
                            break;
                    }
                }
                else break;
            }

            monedasCambio = totalMonedas;

            return totalMonedas == total;
        }

        internal int cambioPayout(int total, List<ChannelData> chanelsDataPayout)
        {
            int totalBilletes = 0;

            foreach (ChannelData chanelData in chanelsDataPayout.OrderByDescending(x => x.Channel))
            {
                if (totalBilletes < total)
                {
                    switch (chanelData.Channel)
                    {
                        case 1:
                            if (chanelData.Recycling)
                                while (chanelData.Level > 0 && totalBilletes + 20 <= total)
                                { totalBilletes += 20; chanelData.Level--; }
                            break;

                        case 2:
                            if (chanelData.Recycling)
                                while (chanelData.Level > 0 && totalBilletes + 50 <= total)
                                { totalBilletes += 50; chanelData.Level--; }
                            break;

                        case 3:
                            if (chanelData.Recycling)
                                while (chanelData.Level > 0 && totalBilletes + 100 <= total)
                                { totalBilletes += 100; chanelData.Level--; }
                            break;

                        case 4:
                            if (chanelData.Recycling)
                                while (chanelData.Level > 0 && totalBilletes + 200 <= total)
                                { totalBilletes += 200; chanelData.Level--; }
                            break;

                        case 5:
                            if (chanelData.Recycling)
                                while (chanelData.Level > 0 && totalBilletes + 500 <= total)
                                { totalBilletes += 500; chanelData.Level--; }
                            break;

                        case 6:
                            if (chanelData.Recycling)
                                while (chanelData.Level > 0 && totalBilletes + 1000 <= total)
                                { totalBilletes += 1000; chanelData.Level--; }
                            break;
                    }
                }
                else break;
            }

            billetesCambio = totalBilletes;

            return total - totalBilletes; // regreso cuánto falta pagar en monedas
        }

        internal bool cambioPayoutByChanel(int total, int chanel, List<ChannelData> chanelsData)
        {
            foreach (ChannelData chanelData in chanelsData)
                if (chanelData.Channel == chanel && chanelData.Level >= total)
                    return true;

            return false;
        }
    }
}
