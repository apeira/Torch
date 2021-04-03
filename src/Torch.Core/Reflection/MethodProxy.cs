using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Torch.Core.Reflection
{
    public class MethodProxy
    {
        public static TDel Create<TDel>(string methodName) where TDel : Delegate
        {
            return Create<TDel>(GetInvoker<TDel>().GetParameters()[0].ParameterType, methodName);
        }
        
        public static TDel CreateStatic<TDel, TOwner>(string methodName) where TDel : Delegate
        {
            return Create<TDel>(typeof(TOwner), methodName);
        }
        
        public static TDel CreateStatic<TDel>(Type ownerType, string methodName) where TDel : Delegate
        {
            return Create<TDel>(ownerType, methodName);
        }

        public static TDel Create<TDel>(Type? ownerTypeOverride, string methodName) where TDel : Delegate
        {
            var delMethod = GetInvoker<TDel>();
            var delParams = delMethod.GetParameters();
            var ownerType = ownerTypeOverride ?? delParams[0].ParameterType;
            var targetMethod = ownerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            var methodParams = targetMethod.GetParameters();
            
            var delParamExprs = delParams.Select(x => Expression.Parameter(x.ParameterType)).ToArray();
            
            if (targetMethod.IsStatic)
            {
                return Expression.Lambda<TDel>(
                    Expression.Call(
                        targetMethod, 
                        MapParams(delParamExprs, methodParams)),
                    delParamExprs
                ).Compile();
            }
            else
            {
                return Expression.Lambda<TDel>(
                    Expression.Call(
                        MapParam(delParamExprs[0], ownerType),
                        targetMethod,
                        MapParams(delParamExprs.Skip(1), methodParams)),
                    delParamExprs
                    ).Compile();
            }
        }

        private static IEnumerable<Expression> MapParams(IEnumerable<ParameterExpression> source, IEnumerable<ParameterInfo> target)
        {
            using var sourceEnum = source.GetEnumerator();
            using var targetEnum = target.GetEnumerator();
            while (sourceEnum.MoveNext() && targetEnum.MoveNext())
            {
                yield return MapParam(sourceEnum.Current, targetEnum.Current.ParameterType);
            }
        }

        private static Expression MapParam(ParameterExpression source, Type target)
        {
            if (source.Type == target)
                return source;
            else
                return Expression.Convert(source, target);
        }

        private static MethodInfo GetInvoker<T>() where T : Delegate
        {
            return typeof(T).GetMethod("Invoke");
        }
    }
}