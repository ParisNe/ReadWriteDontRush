using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadWriteDontRush
{
    internal class Core
    {
        public static UPKoshelevaEntities Context = new UPKoshelevaEntities();
        public static Users CurrentUser { get; set; }

    }   
}
