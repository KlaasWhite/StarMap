using System.Reflection;

namespace StarMap.API
{
    /// <summary>
    /// Methods marked with this attribute will be called before KSA starts creating its ImGui interface.
    /// </summary>
    /// <remarks>
    /// Methods using this attribute must match the following signature:
    ///
    /// <code>
    /// public void MethodName(double dt);
    /// </code>
    ///
    /// Parameter requirements:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <paramref name="dt"/>
    ///     </description>
    ///   </item>
    /// </list>
    /// 
    /// Requirements:
    /// <list type="bullet">
    ///   <item><description>Return type must be <see cref="void"/>.</description></item>
    ///   <item><description>Method must be an instance method (non-static).</description></item>
    /// </list>
    /// </remarks>
    public sealed class StarMapBeforeGuiAttribute : StarMapMethodAttribute
    {
        public override bool IsValidSignature(MethodInfo method)
        {
            return method.ReturnType == typeof(void) &&
                   method.GetParameters().Length == 1 &&
                   method.GetParameters()[0].ParameterType == typeof(double);

        }
    }

    /// <summary>
    /// Methods marked with this attribute will be called when KSA has finished creating its ImGui interface.
    /// </summary>
    /// <remarks>
    /// Methods using this attribute must match the following signature:
    ///
    /// <code>
    /// public void MethodName(double dt);
    /// </code>
    ///
    /// Parameter requirements:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <paramref name="dt"/>
    ///     </description>
    ///   </item>
    /// </list>
    /// 
    /// Requirements:
    /// <list type="bullet">
    ///   <item><description>Return type must be <see cref="void"/>.</description></item>
    ///   <item><description>Method must be an instance method (non-static).</description></item>
    /// </list>
    /// </remarks>
    public sealed class StarMapAfterGuiAttribute : StarMapMethodAttribute
    {
        public override bool IsValidSignature(MethodInfo method)
        {
            return method.ReturnType == typeof(void) &&
                   method.GetParameters().Length == 1 &&
                   method.GetParameters()[0].ParameterType == typeof(double);

        }
    }
}
