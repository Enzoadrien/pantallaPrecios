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
        internal int monedas;
        internal int billetes;
        internal CambioSeparado()
        {
            monedas = 0;
            billetes = 0;
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
            if(SQLiteClass2 != null)
            {
                cambioMaximoBilletes = SQLiteClass2.PagoMax;
            }
        }

        internal bool calculaCambio(int cambio, List<ChannelData>? chanelsDataHopper, List<ChannelData>? chanelsDataPayout)
        {
            List<ChannelData> chanelsData = chanelsDataPayout.ConvertAll(x => new ChannelData { Channel = x.Channel, Currency = x.Currency, Level = x.Level, Recycling = x.Recycling, Value = x.Value }).ToList();
            CambioSeparado cambioSeparado = SepararCambio(cambio);

            if(cambioSeparado.billetes <= cambioMaximoBilletes || cambioMaximoBilletes == 0)
            {
                int totalCambioFaltante = 0;
                if (cambioSeparado.billetes > 0)
                    totalCambioFaltante = cambioPayout(cambioSeparado.billetes, chanelsData);

                bool cambioMonedas = true;
                if (totalCambioFaltante == 0 && cambioSeparado.monedas == 0)
                    return true;
                if (cambioSeparado.monedas + totalCambioFaltante <= cambioMaximoMonedas || cambioMaximoMonedas == 0)
                {
                    chanelsData = chanelsDataHopper.ConvertAll(x => new ChannelData { Channel = x.Channel, Currency = x.Currency, Level = x.Level, Recycling = x.Recycling, Value = x.Value }).ToList();
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

        internal CambioSeparado SepararCambio(int pvTotalSeparar)
        {
            CambioSeparado cambioSeparado = new CambioSeparado();
            if (pvTotalSeparar < 20 && pvTotalSeparar >= 10)
                cambioSeparado.monedas = pvTotalSeparar;
            else if (pvTotalSeparar % 10 >= 1 || pvTotalSeparar == 10)
                cambioSeparado.monedas = pvTotalSeparar % 10;
            cambioSeparado.billetes = pvTotalSeparar - cambioSeparado.monedas;

            return cambioSeparado;
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
                        case 1:
                            if (chanelData.Recycling == true)
                            {
                                while (chanelData.Level != 0)
                                {
                                    if (totalMonedas + 1 <= total)
                                    {
                                        totalMonedas += 1;
                                        chanelData.Level--;
                                    }
                                    else
                                        break;
                                    if (totalMonedas == total)
                                        break;
                                }
                            }
                            break;
                        case 2:
                            if (chanelData.Recycling == true)
                            {
                                while (chanelData.Level != 0)
                                {
                                    if (totalMonedas + 2 <= total)
                                    {
                                        totalMonedas += 2;
                                        chanelData.Level--;
                                    }
                                    else
                                        break;
                                }
                            }
                            break;
                        case 3:
                            if (chanelData.Recycling == true)
                            {
                                while (chanelData.Level != 0)
                                {
                                    if (totalMonedas + 5 <= total)
                                    {
                                        totalMonedas += 5;
                                        chanelData.Level--;
                                    }
                                    else
                                        break;
                                }
                            }
                            break;
                        case 4:
                            if (chanelData.Recycling == true)
                            {
                                while (chanelData.Level != 0)
                                {
                                    if (totalMonedas + 10 <= total)
                                    {
                                        totalMonedas += 10;
                                        chanelData.Level--;
                                    }
                                    else
                                        break;
                                }
                            }
                            break;

                    }
                }
                else
                    break;


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
                            if (chanelData.Recycling == true)
                            {
                                while (chanelData.Level != 0)
                                {
                                    if (totalBilletes + 20 <= total)
                                    {
                                        totalBilletes += 20;
                                        chanelData.Level--;
                                    }
                                    else
                                        break;
                                    if (totalBilletes == total)
                                        break;
                                }
                            }
                            break;
                        case 2:
                            if (chanelData.Recycling == true)
                            {
                                while (chanelData.Level != 0)
                                {
                                    if (totalBilletes + 50 <= total)
                                    {
                                        totalBilletes += 50;
                                        chanelData.Level--;
                                    }
                                    else
                                        break;
                                }
                            }
                            break;
                        case 3:
                            if (chanelData.Recycling == true)
                            {
                                while (chanelData.Level != 0)
                                {
                                    if (totalBilletes + 100 <= total)
                                    {
                                        totalBilletes += 100;
                                        chanelData.Level--;
                                    }
                                    else
                                        break;
                                }
                            }
                            break;
                        case 4:
                            if (chanelData.Recycling == true)
                            {
                                while (chanelData.Level != 0)
                                {
                                    if (totalBilletes + 200 <= total)
                                    {
                                        totalBilletes += 200;
                                        chanelData.Level--;
                                    }
                                    else
                                        break;
                                }
                            }
                            break;
                        case 5:
                            if (chanelData.Recycling == true)
                            {
                                while (chanelData.Level != 0)
                                {
                                    if (totalBilletes + 500 <= total)
                                    {
                                        totalBilletes += 500;
                                        chanelData.Level--;
                                    }
                                    else
                                        break;
                                }
                            }
                            break;
                        case 6:
                            if (chanelData.Recycling == true)
                            {
                                while (chanelData.Level != 0)
                                {
                                    if (totalBilletes + 1000 <= total)
                                    {
                                        totalBilletes += 1000;
                                        chanelData.Level--;
                                    }
                                    else
                                        break;
                                }
                            }
                            break;

                    }
                }
                else
                    break;


            }
            billetesCambio = totalBilletes;
            return total - totalBilletes;
        }

        internal bool cambioPayoutByChanel(int total, int chanel,List<ChannelData> chanelsData)
        {
            foreach (ChannelData chanelData in chanelsData)
                if(chanelData.Channel== chanel && chanelData.Level>= total)
                    return true;
           return false;
        }
    }
}
