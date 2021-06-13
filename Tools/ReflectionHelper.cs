using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
class ReflectionHelper
{
    public static void FindChildsToDo(Type superType, string name, BindingFlags invokeAttr, Binder binder, object target, object[] args)
    {
        var types = Assembly.GetCallingAssembly().GetTypes();
        foreach (var type in types)
        {
            var baseType = type.BaseType;  //获取基类
            while (baseType != null)  //获取所有基类
            {
                if (baseType.Name == superType.Name)
                {
                    Type objtype = Type.GetType(type.FullName, true);
                    objtype.InvokeMember(name, invokeAttr, binder, target, args);
                    break;
                }
                else
                {
                    baseType = baseType.BaseType;
                }
            }
        }
    }
}
