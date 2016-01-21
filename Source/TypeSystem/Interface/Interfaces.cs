using System;
using System.Collections.Generic;
using System.Reflection;

namespace Wpf2Html5.TypeSystem.Interface
{
    /// <summary>
    /// Provides means to retrieve debug information from a type item.
    /// </summary>
    public interface IDebugContext
    {
        /// <summary>
        /// Returns a debug string.
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="expand"></param>
        /// <returns></returns>
        string ToDebugString(int depth = 0, bool expand = false);
    }


    /// <summary>
    /// Basic interface provided by elements of the type system.
    /// </summary>
    public interface ITypeItem : IDebugContext
    {
        /// <summary>
        /// Identifier of the item or fully qualified type name.
        /// </summary>
        /// <remarks>
        /// <para>Items representing data types return the type's fullname.</para>
        /// <para>Members return the member name.</para>
        /// </remarks>
        string ID { get; }

        /// <summary>
        /// True if this item represents a member of a class or native type.
        /// </summary>
        bool IsMember { get; }

        /// <summary>
        /// True if this represents a delegate object.
        /// </summary>
        bool IsDelegate { get; }

        /// <summary>
        /// True if code generation for this class should be omitted.
        /// </summary>
        bool DoNotGenerate { get; }

        /// <summary>
        /// Collection of items this item depends on.
        /// </summary>
        IEnumerable<ITypeItem> Dependencies { get; }

        /// <summary>
        /// The code generation status.
        /// </summary>
        TypeGenerationStatus GStatus { get; }

        /// <summary>
        /// The logical type of the item.
        /// </summary>
        ITypeItem LType { get; }

        /// <summary>
        /// Base class of a class or native type.
        /// </summary>
        ITypeItem BaseType { get; }

        /// <summary>
        /// The runtime type of the item.
        /// </summary>
        Type RType { get; }

        /// <summary>
        /// Object representing the source code (a SyntaxNode).
        /// </summary>
        object SourceNode { get; }

        /// <summary>
        /// The translated code name to be used in the target language.
        /// </summary>
        string CodeName { get; }

        void SetDoNotGenerate();

        /// <summary>
        /// Configure this item for external code.
        /// </summary>
        void SetExternal();

        /// <summary>
        /// Prepare for code generation.
        /// </summary>
        /// <returns></returns>
        bool Prepare();

        /// <summary>
        /// Mark item as failed.
        /// </summary>
        void SetFailed();

        void SetDisableTranslation(params string[] members);

        /// <summary>
        /// Returns a member of a class or native type.
        /// </summary>
        /// <param name="name">The case sensitive name of the member.</param>
        /// <returns>The corresponding item or null, if not found.</returns>
        /// <remarks>
        /// <para>TODO: this does not support overloaded method names.</para></remarks>
        ITypeItem GetMember(string name);

        /// <summary>
        /// Returns a collection of members of this item.
        /// </summary>
        IEnumerable<ITypeItem> Members { get; }

        /// <summary>
        /// Sets the source code for the item.
        /// </summary>
        /// <param name="astitem">The corresponding abstract syntax tree item.</param>
        /// <remarks>This changes the state of the item to 'source' if successful.</remarks>
        void SetSourceNode(object astitem);

        ITypeItem GetConvertMethod();

        /// <summary>
        /// Adds a native script reference to this item.
        /// </summary>
        /// <param name="assembly">The assembly where to take the resource from.</param>
        /// <param name="path">The path of the embedded resource within the assembly.</param>
        void AddScriptReference(Assembly assembly, string path);

        /// <summary>
        /// Native script references associated with the item.
        /// </summary>
        IEnumerable<IScriptReference> ScriptReferences { get; }

    }

    /// <summary>
    /// Interface provided by objects that expose a type system of their own like modules and classes.
    /// </summary>
    public interface ITypeContext
    {
        /// <summary>
        /// Returns the logical type corresponding to a runtime type.
        /// </summary>
        /// <param name="rtype">The runtime type.</param>
        /// <param name="options">Options for the translation.</param>
        /// <returns>The corresponding logical type object, if found or created.</returns>
        ITypeItem TranslateRType(Type rtype, TranslateOptions options = TranslateOptions.None);

        /// <summary>
        /// Resolves a qualified type name.
        /// </summary>
        /// <param name="name">The qualified type name.</param>
        /// <returns>The type item or null, if not found.</returns>
        ITypeItem ResolveLType(string name);

        /// <summary>
        /// Adds a type dependency.
        /// </summary>
        /// <param name="dependent">The dependent type item.</param>
        /// <param name="target">The referenced type item.</param>
        void AddDepdendency(ITypeItem dependent, ITypeItem target);

        /// <summary>
        /// Registers a logical type item in this context.
        /// </summary>
        /// <param name="ltype">The logical type object.</param>
        /// <remarks><para>An item with the same ID must not already exist.</para></remarks>
        void RegisterType(ITypeItem ltype);
    }

    /// <summary>
    /// Implemented by type items that represent a context with variables.
    /// </summary>
    public interface IVariableContext
    {
        /// <summary>
        /// Returns the related declaration context where types should be registered.
        /// </summary>
        /// <returns></returns>
        IDeclarationContext GetTypeDeclarationContext();

        /// <summary>
        /// Returns the item representing the variable or method given a name.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <returns></returns>
        ITypeItem GetVariable(string name);

        /// <summary>
        /// Adds a variable to the context.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        void AddVariable(string name, ITypeItem type);
    }

    public interface IAssemblyContainer
    {
        Assembly[] Assemblies { get; }
    }

    /// <summary>
    /// A context providing types and variables via the respective interfaces.
    /// </summary>
    public interface IDeclarationContext : ITypeContext, IVariableContext
    {
    }

    /// <summary>
    /// Interface explosed by module level items.
    /// </summary>
    public interface IModule : ITypeItem, IDeclarationContext
    {
        Func<Type, bool> ShouldTranslateType { get; set; }

        void AddSourceNamespace(string ns, Assembly assembly);
    }

    /// <summary>
    /// Interface provided by items that represent class-like objects.
    /// </summary>
    interface IClassContext
    {
    }

    /// <summary>
    /// Interface exposed by the type system root.
    /// </summary>
    public interface ITypeRoot : ITypeItem, IDeclarationContext
    {
    }

    /// <summary>
    /// A reference to an external script.
    /// </summary>
    public interface IScriptReference
    {
        Assembly Assembly { get; }

        string Path { get; }

        string Key { get; }
    }

    public interface IMethodContext : IDeclarationContext, IDebugContext
    {

    }
}
