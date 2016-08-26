


using Microsoft.Build.Evaluation;
using System;

namespace BSF.Aop.Test
{

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Aop.AopStartLoader.Start(null);
            int b;
            new AroundAopTest().Method(new TempInfo() { T1 = 2 }, out b,3);

            AroundAopTest2.Method2(new TempInfo() { T1 = 333 }, 4);
            var u = new User();
            u.Name = "1";
            u.Age = 2;

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }
    }
}
