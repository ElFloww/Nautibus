using System.Linq.Expressions;

namespace AmetisAPI.Infrastructure.Tools
{
    internal static class ReflectionTools
    {
        public static object GetDefaultValue(this Type p_Type)
        {
            // Validate parameters.
            Guard.Against.Null(p_Type);

            // We want an Func<object> which returns the default.
            // Create that expression here.
            Expression<Func<object>> v_Exp = Expression.Lambda<Func<object>>(
                // Have to convert to object.
                Expression.Convert(
                    // The default value, always get what the *code* tells us.
                    Expression.Default(p_Type), typeof(object)
                )
            );

            // Compile and return the value.
            return v_Exp.Compile().Invoke();
        }
    }
}