using DataGate;
using Org.BouncyCastle.Asn1.X509;
using Precios_Turnos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Priceio
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
        internal int cambioMaximoMonedas = 100;

        internal bool calculaCambio(int cambio, List<ChannelData> chanelsDataHopper, List<ChannelData> chanelsDataPayout)
        {
            CambioSeparado cambioSeparado = SepararCambio(cambio);

            int totalCambioFaltante=0;
            if (cambioSeparado.billetes > 0)
                totalCambioFaltante = cambioPayout(cambioSeparado.billetes, chanelsDataPayout);

            bool cambioMonedas = true;
            if (totalCambioFaltante + cambioSeparado.monedas > 0 && totalCambioFaltante + cambioSeparado.monedas < cambioMaximoMonedas)
                cambioMonedas = cambioHopper(totalCambioFaltante + cambioSeparado.monedas, chanelsDataHopper);

            if(cambioMonedas)
                return true;

            return false;
        }

        internal CambioSeparado SepararCambio(int pvTotalSeparar)
        {
            CambioSeparado cambioSeparado = new CambioSeparado();
            if (pvTotalSeparar < 20 && pvTotalSeparar >= 10)
                cambioSeparado.monedas = pvTotalSeparar;
            else if ((pvTotalSeparar % 10) >= 1 || pvTotalSeparar == 10)
                cambioSeparado.monedas = (pvTotalSeparar % 10);
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
                            if (chanelData.Recycling == true && chanelData.Level > 0)
                            {
                                for (int x = 0; x <= chanelData.Level; x++)
                                {
                                    if ((totalMonedas + 1) <= total)
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
                            if (chanelData.Recycling == true && chanelData.Level > 0)
                            {
                                for (int x = 0; x <= chanelData.Level; x++)
                                {
                                    if ((totalMonedas + 2) <= total)
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
                            if (chanelData.Recycling == true && chanelData.Level > 0)
                            {
                                for (int x = 0; x <= chanelData.Level; x++)
                                {
                                    if ((totalMonedas + 5) <= total)
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
                            if (chanelData.Recycling == true && chanelData.Level > 0)
                            {
                                for (int x = 0; x <= chanelData.Level; x++)
                                {
                                    if ((totalMonedas + 10) <= total)
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
                            if (chanelData.Recycling == true && chanelData.Level > 0)
                            {
                                for (int x = 0; x <= chanelData.Level; x++)
                                {
                                    if ((totalBilletes + 20) <= total)
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
                            if (chanelData.Recycling == true && chanelData.Level > 0)
                            {
                                for (int x = 0; x <= chanelData.Level; x++)
                                {
                                    if ((totalBilletes + 50) <= total)
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
                            if (chanelData.Recycling == true && chanelData.Level > 0)
                            {
                                for (int x = 0; x <= chanelData.Level; x++)
                                {
                                    if ((totalBilletes + 100) <= total)
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
                            if (chanelData.Recycling == true && chanelData.Level > 0)
                            {
                                for (int x = 0; x <= chanelData.Level; x++)
                                {
                                    if ((totalBilletes + 200) <= total)
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
                            if (chanelData.Recycling == true && chanelData.Level > 0)
                            {
                                for (int x = 0; x <= chanelData.Level; x++)
                                {
                                    if ((totalBilletes + 500) <= total)
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
                            if (chanelData.Recycling == true && chanelData.Level > 0)
                            {
                                for (int x = 0; x <= chanelData.Level; x++)
                                {
                                    if ((totalBilletes + 1000) <= total)
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
    }
}
