using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pixelations
{
    public class MySeaphore
    {
        private int maxVal;
        private SemaphoreSlim[] mutexes;
        private int nrLocked = 0;

        public int publicSignal = 0;

        public MySeaphore(int nr)
        {
            maxVal = nr;
            mutexes = new SemaphoreSlim[nr];
            for (int i = 0; i < nr; i++)
            {
                mutexes[i] = new SemaphoreSlim(0);
            }
        }

        public void Lock(int index)
        {
            Console.WriteLine("Lock " + index);
            nrLocked++;
            if (nrLocked == maxVal)
                Unlock(index);
            else mutexes[index].Wait();
        }

        private void Unlock(int index)
        {
            nrLocked = 0;
            for(int i=0;i<maxVal;i++)
            {
                if (i != index)
                    mutexes[i].Release();
            }
        }
    }
}
