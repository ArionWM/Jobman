using NCrontab;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace JobMan
{
    public class DefaultWorkItemDefinitionFactory : IWorkItemDefinitionFactory
    {
        private static readonly ParameterExpression UnusedParameterExpr = Expression.Parameter(typeof(object), "_unused");

        protected struct ExpressionMetadata
        {
            public MethodCallExpression MethodCallExpression;
            public Type ParentClassType;
            public MethodInfo MethodInfo;
            public ParameterInfo[] Parameters;
            public object[] Values;
            public JobDefinitionAttributeBase[] JobDefinitionAttributes;
        }


        protected virtual void ValidateParameterType(ParameterInfo parameterInfo)
        {
            Type type = parameterInfo.ParameterType;

            if (parameterInfo.IsOut || type.IsByRef)
                throw new NotSupportedException("Output or byref parameters not supported");

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }
        }
        protected virtual void ValidateParameters(ParameterInfo[] parameters)
        {
            foreach (ParameterInfo parameter in parameters)
            {
                ValidateParameterType(parameter);
            }
        }

        protected virtual string[] GetParameterTypeNames(ParameterInfo[] parameters)
        {
            List<string> parameterTypeNames = new List<string>();
            foreach (ParameterInfo parameter in parameters)
            {
                parameterTypeNames.Add(parameter.ParameterType.FullName);
            }
            return parameterTypeNames.ToArray();
        }

        protected virtual void ValidateMethodInfo(MethodInfo methodInfo)
        {
            //Async methods?

            if (methodInfo.IsAbstract || !methodInfo.IsPublic || methodInfo.ContainsGenericParameters)
                throw new NotSupportedException($"Abstract, instance, generic or 'non public' methods not supported. Only public static metods alloved. ('{methodInfo.DeclaringType.FullName}' / '{methodInfo.Name}')");
        }

        protected virtual object GetExpressionValue(Expression expression)
        {
            if (expression is ConstantExpression constantExpression)
            {
                return constantExpression.Value;
            }
            else
            {
                var lambda = Expression.Lambda<Func<object, object>>(Expression.Convert(expression, typeof(object)), UnusedParameterExpr);
                Func<object, object> func = lambda.Compile();
                object value = func(null);
                return value;
            }
        }


        protected virtual ExpressionMetadata ProcessExpression(Expression<Action> expression)
        {
            MethodCallExpression methodCallExpression = expression.Body as MethodCallExpression ?? throw new InvalidOperationException("MethodCallExpression required");

            ValidateMethodInfo(methodCallExpression.Method);

            Type type = methodCallExpression.Method.DeclaringType;
            MethodInfo methodInfo = methodCallExpression.Method;

            ParameterInfo[] parameters = methodInfo.GetParameters();

            ValidateParameters(parameters);

            IEnumerable<object> values = methodCallExpression.Arguments.Select(exp => GetExpressionValue(exp));
            IEnumerable<JobDefinitionAttributeBase> jobDefinitionAttributes = methodInfo.GetCustomAttributes<JobDefinitionAttributeBase>();

            //TODO: Cache attributes and parameter types?

            ExpressionMetadata em = new ExpressionMetadata()
            {
                MethodCallExpression = methodCallExpression,
                ParentClassType = type,
                MethodInfo = methodInfo,
                Parameters = parameters,
                Values = values.ToArray(),
                JobDefinitionAttributes = jobDefinitionAttributes.ToArray()

            };

            return em;
        }

        protected virtual InvokeData ToInvokeData(ExpressionMetadata em)
        {
            string[] parameterTypeNames = GetParameterTypeNames(em.Parameters);
            InvokeData invokeData = new InvokeData(em.ParentClassType.FullName, em.MethodInfo.Name, parameterTypeNames, em.Values);
            return invokeData;
        }

        protected virtual void CallFilters(ExpressionMetadata em, IWorkItemDefinition itemDefinition)
        {
            foreach (JobDefinitionAttributeBase attr in em.JobDefinitionAttributes)
            {
                attr.Define(itemDefinition);
            }
        }

        protected virtual void CheckDefaults(ExpressionMetadata em, IWorkItemDefinition itemDefinition)
        {
            if (string.IsNullOrWhiteSpace(itemDefinition.Pool))
                itemDefinition.Pool = WorkPoolOptions.POOL_DEFAULT;

        }

        public virtual IWorkItemDefinition Create()
        {
            return new WorkItemDefinition();
        }

        protected virtual IWorkItemDefinition Create(Expression<Action> expression, string poolName, WorkItemType type, string cron, DateTime nextExecute, string tag)
        {
            ExpressionMetadata em = ProcessExpression(expression);
            InvokeData invokeData = ToInvokeData(em);

            WorkItemDefinition workItemDefinition = new WorkItemDefinition()
            {
                Id = 0,// GetNextId(),
                Pool = poolName,
                Data = invokeData,
                Cron = cron,
                Status = WorkItemStatus.WaitingProcess,
                Type = type,
                NextExecuteTime = nextExecute,
                Tag = tag
            };

            this.CallFilters(em, workItemDefinition);

            this.CheckDefaults(em, workItemDefinition);

            this.Validate(workItemDefinition);

            return workItemDefinition;
        }

        public virtual IWorkItemDefinition Create(Expression<Action> expression, string poolName = null, string tag = null)
        {
            IWorkItemDefinition workItemDefinition = Create(expression, poolName, WorkItemType.SingleRun, null, JobManGlobals.Time.Now, tag);
            return workItemDefinition;
        }

        public virtual IWorkItemDefinition Create(Expression<Action> expression, TimeSpan runAfter, string poolName = null, string tag = null)
        {
            IWorkItemDefinition workItemDefinition = Create(expression, poolName, WorkItemType.SingleRun, null, JobManGlobals.Time.Now.Add(runAfter), tag);
            return workItemDefinition;

        }

        public virtual IWorkItemDefinition Create(Expression<Action> expression, string cronExpression, string poolName = null, string tag = null)
        {
            DateTime nextExecute = JobManGlobals.Time.GetNextOccurrence(cronExpression, JobManGlobals.Time.Now);
            IWorkItemDefinition workItemDefinition = Create(expression, poolName, WorkItemType.RecurrentRun, cronExpression, nextExecute, tag);
            return workItemDefinition;

        }

        public virtual void Validate(IWorkItemDefinition wiDef)
        {
            if (string.IsNullOrWhiteSpace(wiDef.Pool))
                throw new InvalidDataException("Pool name required");

            switch (wiDef.Type)
            {
                case WorkItemType.SingleRun:
                    break;
                case WorkItemType.RecurrentRun:
                    if (string.IsNullOrWhiteSpace(wiDef.Cron))
                        throw new InvalidDataException($"Cron information required");
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported WorkItemType: {wiDef.Type}");
            }

            if (wiDef.NextExecuteTime < JobmanHelperExtensions.MinDateTime)
                throw new InvalidDataException($"NextExecuteTime required");

            if (wiDef.Data == null || string.IsNullOrWhiteSpace(wiDef.Data.ClassType) || string.IsNullOrWhiteSpace(wiDef.Data.MethodName))
                throw new InvalidDataException($"InvokeData information required");
        }
    }

}
