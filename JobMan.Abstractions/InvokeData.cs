using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{
    public class InvokeData
    {
        public string ClassType { get; set; }
        public string MethodName { get; set; }
        public string[] PropertyTypes { get; set; }
        public object[] ArgumentValues { get; set; }

        public override string ToString()
        {
            return this.ClassType + " / " + this.MethodName;
        }

        public InvokeData()
        {

        }

        public InvokeData(string classType, string methodName, string[] propertyTypeNames, object[] argumentValues)
        {
            if (string.IsNullOrEmpty(classType))
            {
                throw new ArgumentException($"'{nameof(classType)}' cannot be null or empty.", nameof(classType));
            }

            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentException($"'{nameof(methodName)}' cannot be null or empty.", nameof(methodName));
            }

            ClassType = classType;
            MethodName = methodName;
            PropertyTypes = propertyTypeNames;
            ArgumentValues = argumentValues ?? throw new ArgumentNullException(nameof(argumentValues));
        }
    }
}
