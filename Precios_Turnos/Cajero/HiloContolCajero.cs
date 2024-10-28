using Priceio.Cajero.Hopper;
using Priceio.Cajero.Payout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Priceio.Cajero
{
    internal class HiloContolCajero
    {

        internal SMARTPayout smartPayout;
        internal SMARTHopper smartHopper;

        internal HiloContolCajero()
        {
            smartPayout = new SMARTPayout();
            smartHopper = new SMARTHopper();
        }

        internal void iniciarPayout()
        {
            if(smartPayout != null)
                Task.Run(() => smartPayout.RunPayout());
            else
            {
                smartPayout = new SMARTPayout();
                Task.Run(() => smartPayout.RunPayout());
            }

        }
        internal void detenerPayout()
        {
            if(smartPayout != null)
                Task.Run(() => smartPayout.detenerPayout());
        }
        internal void iniciarHopper()
        {
            if (smartHopper != null)
                Task.Run(() => smartHopper.RunHopper());
            else
            {
                smartHopper = new SMARTHopper();
                Task.Run(() => smartHopper.RunHopper());

            } 
        }
        internal void detenerHopper()
        {
            if (smartHopper != null)
                Task.Run(() => smartHopper.detenerHopper());
        }

        internal void escucharPuerto()
        {
            Task.Run(() => AsynchronousSocketListenerCajero.StartListening(smartPayout, smartHopper));
        }
        internal List<ChannelData> NivelesPayout()
        {
            return smartPayout.Payout.UnitDataList;
        }
        internal List<ChannelData> NivelesHopper()
        {
            return smartHopper.Hopper.UnitDataList;
        }
    }
}
