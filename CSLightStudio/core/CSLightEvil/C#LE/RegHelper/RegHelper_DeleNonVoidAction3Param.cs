﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CSLE
{
    /// <summary>
    /// 支持有返回值的带 3 个参数的委托注册.
    /// 注意这里和void类型委托注册的用法有些区别：
    /// 这里的类模板第一个参数是返回类型.
    ///    比如有个返回bool型的委托定义如下：
    ///    public class Class {
    ///         public delegate bool BoolParam3Delegate(int param1, int param2, int param3);
    ///    }
    ///    那么注册方式如下：
    ///    env.RegType(new RegHelper_DeleNonVoidAction<bool, int, int, int>(typeof(Class.BoolParam3Delegate), "Class.BoolParam3Delegate"));
    /// </summary>
    public class RegHelper_DeleNonVoidAction<ReturnType, T, T1, T2> : RegHelper_Type, ICLS_Type_Dele
    {
        /// <summary>
        /// 有返回值,同时带 3 个 参数的委托.
        /// </summary>
        /// <returns></returns>
        public delegate ReturnType NonVoidDelegate(T param0, T1 param1, T2 param2);

        public RegHelper_DeleNonVoidAction(Type type, string setkeyword)
            : base(type, setkeyword, true)
        {

        }

        public override object Math2Value(CLS_Content env, char code, object left, CLS_Content.Value right, out CLType returntype)
        {
            returntype = null;

            if (left is DeleEvent)
            {
                DeleEvent info = left as DeleEvent;
                Delegate calldele = null;

                //!--exist bug.
                /*if (right.value is DeleFunction) calldele = CreateDelegate(env.environment, right.value as DeleFunction);
                else if (right.value is DeleLambda) calldele = CreateDelegate(env.environment, right.value as DeleLambda);
                else if (right.value is Delegate) calldele = right.value as Delegate;*/

                object rightValue = right.value;
                if (rightValue is DeleFunction)
                {
                    if (code == '+')
                    {
                        calldele = CreateDelegate(env.environment, rightValue as DeleFunction);
                    }
                    else if (code == '-')
                    {
                        calldele = CreateDelegate(env.environment, rightValue as DeleFunction);
                    }
                }
                else if (rightValue is DeleLambda)
                {
                    if (code == '+')
                    {
                        calldele = CreateDelegate(env.environment, rightValue as DeleLambda);
                    }
                    else if (code == '-')
                    {
                        calldele = CreateDelegate(env.environment, rightValue as DeleLambda);
                    }
                }
                else if (rightValue is Delegate)
                {
                    calldele = rightValue as Delegate;
                }

                if (code == '+')
                {
                    info._event.AddEventHandler(info.source, calldele);
                    //if (!(rightValue is Delegate)) {
                    //    Dele_Map_Delegate.Map(rightValue as IDeleBase, calldele);
                    //}
                    return null;
                }
                else if (code == '-')
                {
                    info._event.RemoveEventHandler(info.source, calldele);
                    //if (!(rightValue is Delegate)) {
                    //    Dele_Map_Delegate.Destroy(rightValue as IDeleBase);
                    //}
                    return null;
                }

            }
            else if (left is Delegate)
            {
                Delegate info = left as Delegate;
                Delegate calldele = null;
                if (right.value is DeleFunction)
                    calldele = CreateDelegate(env.environment, right.value as DeleFunction);
                else if (right.value is DeleLambda)
                    calldele = CreateDelegate(env.environment, right.value as DeleLambda);
                else if (right.value is Delegate)
                    calldele = right.value as Delegate;
                if (code == '+')
                {
                    Delegate.Combine(info, calldele);
                    return null;
                }
                else if (code == '-')
                {
                    Delegate.Remove(info, calldele);
                }
            }
            return new NotSupportedException();
        }

        public Delegate CreateDelegate(ICLS_Environment env, DeleFunction delefunc)
        {
            DeleFunction _func = delefunc;
            Delegate _dele = delefunc.cacheFunction(null);
            if (_dele != null) return _dele;
            NonVoidDelegate dele = delegate(T param0, T1 param1, T2 param2)
            {
                var func = _func.calltype.functions[_func.function];
                if (func.expr_runtime != null)
                {
                    CLS_Content content = new CLS_Content(env);
                    try
                    {
                        content.DepthAdd();
                        content.CallThis = _func.callthis;
                        content.CallType = _func.calltype;
                        content.function = _func.function;

                        content.DefineAndSet(func._paramnames[0], func._paramtypes[0].type, param0);
                        content.DefineAndSet(func._paramnames[1], func._paramtypes[1].type, param1);
                        content.DefineAndSet(func._paramnames[2], func._paramtypes[2].type, param2);

                        CLS_Content.Value retValue = func.expr_runtime.ComputeValue(content);
                        content.DepthRemove();

                        return (ReturnType)retValue.value;
                    }
                    catch (Exception err)
                    {
                        env.logger.Log(content.Dump());
                        throw err;
                    }
                }
                return default(ReturnType);
            };

            _dele = Delegate.CreateDelegate(this.type, dele.Target, dele.Method);
            return delefunc.cacheFunction(_dele);
        }

        public Delegate CreateDelegate(ICLS_Environment env, DeleLambda lambda)
        {
            CLS_Content content = lambda.content.Clone();
            var pnames = lambda.paramNames;
            var expr = lambda.expr_func;

            NonVoidDelegate dele = delegate(T param0, T1 param1, T2 param2)
            {
                if (expr != null)
                {
                    try
                    {
                        content.DepthAdd();

                        content.DefineAndSet(pnames[0], typeof(T), param0);
                        content.DefineAndSet(pnames[1], typeof(T1), param1);
                        content.DefineAndSet(pnames[2], typeof(T2), param2);
                        CLS_Content.Value retValue = expr.ComputeValue(content);

                        content.DepthRemove();

                        return (ReturnType)retValue.value;
                    }
                    catch (Exception err)
                    {
                        env.logger.Log(content.Dump());
                        throw err;
                    }
                }
                return default(ReturnType);
            };

            Delegate d = dele as Delegate;
            return Delegate.CreateDelegate(this.type, d.Target, d.Method);
        }
    }
}