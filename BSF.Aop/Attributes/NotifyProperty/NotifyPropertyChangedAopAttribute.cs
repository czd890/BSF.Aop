using BSF.Aop.Attributes.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSF.Aop.Attributes.NotifyProperty
{
    /// <summary>
    /// 注入INotifyPropertyChanged信息
    /// [NoAopAttribute] 特性，可以排除特定属性的注入
    /// </summary>
    public sealed class NotifyPropertyChangedAopAttribute: BaseAopAttribute
    {
    }
}
