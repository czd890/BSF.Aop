using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSF.Aop.SystemRuntime.Tool
{
    /// <summary>
    /// 映射帮助类库
    /// </summary>
    public static class MapperHelper
    {
        /// <summary>
        /// 对象属性拷贝类
        /// 反射实现,性能不高
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void CopyProperty(object from, object to)
        {
            //利用反射获得类成员
            var fieldFroms = from.GetType().GetProperties();
            var fieldTos = to.GetType().GetProperties();
            int lenTo = fieldTos.Length;

            for (int i = 0, l = fieldFroms.Length; i < l; i++)
            {
                for (int j = 0; j < lenTo; j++)
                {
                    if (fieldTos[j].Name != fieldFroms[i].Name) continue;
                    if(fieldTos[j].GetSetMethod()!=null)
                        fieldTos[j].SetValue(to, fieldFroms[i].GetValue(from, null), null);
                    break;
                }
            }
        }
    }
}
