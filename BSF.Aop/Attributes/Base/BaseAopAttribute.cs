using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSF.Aop.Attributes.Base
{
    public abstract class BaseAopAttribute:System.Attribute
    {
        static BaseAopAttribute()
        {
            AopStartLoader.Start(null);
        }
    }
}
