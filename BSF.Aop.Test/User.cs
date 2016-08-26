using BSF.Aop.Attributes.Base;
using BSF.Aop.Attributes.NotifyProperty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSF.Aop.Test
{
    [NotifyPropertyChangedAop]
    public class User
    {
        public string Name { get; set; }

        public int Age { get; set; }

        [NoAop]
        public int B { get; set; }
    }
}
