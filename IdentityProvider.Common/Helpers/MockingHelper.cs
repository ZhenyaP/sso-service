//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the MockingHelper type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IdentityProvider.Common.Helpers
{
    /// <summary>
    /// The Mocking Helper.
    /// </summary>
    public static class MockingHelper
    {
        /// <summary>
        /// Creates a new instance of type T
        /// </summary>
        /// <typeparam name="T">The type of instance to create.</typeparam>
        /// <param name="args">The arguments passed to the constructor when creating an instance.</param>
        /// <returns>The created instance of type T.</returns>
        public static T CreateInstance<T>(params object[] args)
        {
            var typeToCreate = typeof(T);

            var parameterTypes = args.Select(arg => arg.GetType()).ToArray();

            // Use reflection to get the ConstructorInfo object that matches our parameters
            // even if it's not public.
            var constructorInfoObj = typeToCreate.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, parameterTypes, null);

            return (T)constructorInfoObj.Invoke(args);
        }

        /// <summary>
        /// Sets the property value in the object.
        /// </summary>
        /// <param name="target">The target object where to set the property value.</param>
        /// <param name="memberName">The name of the property.</param>
        /// <param name="newValue">The new property value.</param>
        public static void SetPropertyValue(object target, string memberName, object newValue)
        {
            var prop = GetPropertyReference(target.GetType(), memberName);
            prop.SetValue(target, newValue, null);
        }

        /// <summary>
        /// Gets the property reference from the type.
        /// </summary>
        /// <param name="targetType">The target type.</param>
        /// <param name="memberName">The member name.</param>
        /// <returns>The property reference from the type.</returns>
        private static PropertyInfo GetPropertyReference(Type targetType, string memberName)
        {
            var propInfo = targetType.GetProperty(memberName,
                                                  BindingFlags.Public |
                                                  BindingFlags.NonPublic |
                                                  BindingFlags.Static |
                                                  BindingFlags.Instance);

            if (propInfo == null && targetType.BaseType != null)
            {
                //if the member isn't actually on the type we're working on, rather it's
                //defined in a base class as private, it won't be returned in the above call,
                //so we have to walk the type hierarchy until we find it.
                // See: http://agsmith.wordpress.com/2007/12/13/where-are-my-fields/

                return GetPropertyReference(targetType.BaseType, memberName);
            }
            return propInfo;
        }

        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="target">The target type.</param>
        /// <param name="fieldName">The field name.</param>
        /// <param name="newValue">The new field value.</param>
        public static void SetFieldValue(object target, string fieldName, object newValue)
        {
            var field = GetFieldReference(target.GetType(), fieldName);
            field.SetValue(target, newValue);
        }

        /// <summary>
        /// Gets the field reference.
        /// </summary>
        /// <param name="targetType">The target type.</param>
        /// <param name="fieldName">The field name.</param>
        /// <returns>The field reference.</returns>
        private static FieldInfo GetFieldReference(Type targetType, string fieldName)
        {
            var field = targetType.GetField(fieldName,
                                                  BindingFlags.Public |
                                                  BindingFlags.NonPublic |
                                                  BindingFlags.Static |
                                                  BindingFlags.Instance);

            if (field == null && targetType.BaseType != null)
            {
                //if the field isn't actually on the type we're working on, rather it's
                //defined in a base class as private, it won't be returned in the above call,
                //so we have to walk the type hierarchy until we find it.
                // See: http://agsmith.wordpress.com/2007/12/13/where-are-my-fields/

                return GetFieldReference(targetType.BaseType, fieldName);
            }
            return field;
        }

        /// <summary>
        /// Gets the property name from type.
        /// </summary>
        /// <typeparam name="T">The type where to find the property name.</typeparam>
        /// <param name="property">The property lambda expression.</param>
        /// <returns>The property name.</returns>
        public static string GetPropertyName<T>(Expression<Func<T>> property)
        {
            LambdaExpression lambdaExpression = property;
            var memberExpression = lambdaExpression.Body as MemberExpression ??
                ((UnaryExpression)lambdaExpression.Body).Operand as MemberExpression;
            return memberExpression?.Member.Name;
        }
    }
}
