using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BSF.Aop.Attributes.Around;

namespace BSF.Aop.Test
{
    public class AroundAopTest
    {
        [MyAroundAop]
        [AttributeInfo(Des = "测试2")]
        public void Method(TempInfo info, out int b,int a=1)
        {
            a = 222;
            b = 3;
            System.Console.WriteLine("Hello world!"+a);
            
        }
    }

    public static class AroundAopTest2
    {
        [MyAroundAop][AttributeInfo(Des ="测试")]
        public static void Method2(TempInfo info, int a = 1)
        {
            a = 222;
            System.Console.WriteLine("Hello world!" + a);

        }
    }

    public class MyAroundAop : Aop.Attributes.Around.AroundAopAttribute
    {
        public MyAroundAop()
        {
        }


        public override void Before(AroundInfo info)
        {
            var att = info.Method.CustomAttributes.ToList()[0];
            info.Params["a"] = 55;
            System.Console.WriteLine("before" + info.Params["a"]);
        }

        public override void After(AroundInfo info)
        {
            System.Console.WriteLine("after"+ info.Params["a"]);
        }
    }

    public class TempInfo
    {
        public int T1 { get; set; }
    }

    public class AttributeInfo : System.Attribute
    {
        public string Des { get; set; }
    }

}
