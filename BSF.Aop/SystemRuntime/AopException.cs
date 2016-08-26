using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSF.Aop.SystemRuntime
{
    public class AopException:Exception
    {
        public string BSFMessage = "";
        public List<Exception> BSFExceptionList = new List<Exception>();
        public AopException(string message):base(message)
        {
            BSFMessage += (string.IsNullOrEmpty(message)?"":message);
        }

        public AopException(string message, Exception exception):base(message,exception)
        {
            if (exception != null)
            {
                BSFMessage += message + (string.IsNullOrEmpty(exception.Message) ? "" : exception.Message);
                BSFExceptionList.Add(exception);
            }
        }
    }
}
