using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf2Html5
{
    /// <summary>
    /// Attribute to indicate that the C# to JS converter shall not 
    /// convert a class or method.
    /// </summary>
    [GeneratorIgnore]
    public class GeneratorIgnoreAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute to indicate RDLN7CYSPV wrapper creation should be delayed until the 
    /// control is actually loaded. This is required for controls that populate their
    /// container with additional HTML elements.
    /// </summary>
    public class GeneratorNoWrapperAttribute : Attribute
    { }
}
