using BSF.Aop.Attributes.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BSF.Aop.Attributes.Around
{
    /// <summary>
    /// Aop 环切（前后切面）实现
    /// 备注:
    /// 1.必须定义默认构造函数,否则注入报错;
    /// 2.暂不支持定义注入特性时,写入属性信息;如[AroundAopAttribute(Property1="bb",Property2="cc")]等
    /// </summary>
    public class AroundAopAttribute: BaseAopAttribute
    {
        /// <summary>
        /// 前切
        /// </summary>
        public virtual void Before(AroundInfo info)
        {

        }

        /// <summary>
        /// 后切
        /// </summary>
        public virtual void After(AroundInfo info)
        {

        }
    }

    /// <summary>
    /// 环切信息
    /// </summary>
    public class AroundInfo
    {
        public MethodBase Method { get; set; }
        public Dictionary<string, Object> Params { get; set; }
        public Object Obj { get; set; }

        public AroundInfo(MethodBase method, Dictionary<string, Object> param,Object obj)
        {
            this.Method = method;
            Params = param;
            Obj = obj;
        }

        public AroundInfo(MethodBase method, Dictionary<string, Object> param)
        {
            this.Method = method;
            Params = param;
            Obj = null;
        }
    }

    
}
