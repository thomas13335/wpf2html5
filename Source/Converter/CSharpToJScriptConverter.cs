using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf2Html5.Builder;
using Wpf2Html5.Converter;
using Wpf2Html5.Converter.Rewriter;
using Wpf2Html5.Exceptions;
using Wpf2Html5.TypeSystem;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5
{
    /// <summary>
    /// Converts CS code to JScript (with many limitations).
    /// </summary>
    public class CSharpToJScriptConverter
    {
        #region Private Fields

        private ITypeContext _context;
        private List<Assembly> _assemblies = new List<Assembly>();
        private JScriptBuilder _declarations = new JScriptBuilder();
        private List<string> _nativescripts = new List<string>();
        private List<DeclarationEmitContext> _output = new List<DeclarationEmitContext>();
        private bool _preambleemitted;

        /// <summary>
        /// Collection of classes referenced in this context.
        /// </summary>
        private Dictionary<string, ITypeItem> _classes = new Dictionary<string, ITypeItem>();

        private Queue<ITypeItem> _classqueue = new Queue<ITypeItem>();

        private HashSet<string> _classesref = new HashSet<string>();

        private ITypeItem CurrentClass { get; set; }

        #endregion

        #region Properties

        /// <summary>Returns all declarations emitted so far.</summary>
        public JScriptBuilder Declarations { get { return _declarations; } }

        public IEnumerable<Assembly> Assemblies { get { return _assemblies; } }

        public string CurrentClassName { get; private set; }

        #endregion

        #region Events

        public event ConverterErrorEventHandler Error;

        #endregion

        #region Construction

        public CSharpToJScriptConverter(ITypeContext context)
        {
            _context = context;

            // extract assembly references from the container ...
            var assemblies = _context as IAssemblyContainer;
            if(null != assemblies)
            {
                foreach(var assembly in assemblies.Assemblies)
                {
                    AddAssembly(assembly);
                }
            }
        }

        #endregion

        #region Diagnostics

        private static void Trace(string format, params object[] args)
        {
            if (Log.ShowCodeGeneration)
            {
                Log.Trace("cs2js: " + format, args);
            }
        }

        private static void TraceEmit(string format, params object[] args)
        {
            if (Log.ShowCodeGeneration)
            {
                Log.Trace("cs2js: " + format, args);
            }
        }

        private static void TraceDependency(string format, params object[] args)
        {
            if (Log.ShowClassDependencies)
            {
                Log.Trace("cs2js: " + format, args);
            }
        }

        private static void Information(string format, params object[] args)
        {
            Log.Trace(format, args);
        }

        private static void Warning(string format, params object[] args)
        {
            Log.Warning(format, args);
        }

        #endregion

        #region Public Methods

        #region Modification Methods

        /// <summary>
        /// Adds an assembly for type resolution.
        /// </summary>
        /// <param name="assembly">The assembly to add.</param>
        /// <returns>True if the assembly was added, false if it already existed.</returns>
        public bool AddAssembly(Assembly assembly)
        {
            if (!_assemblies.Contains(assembly))
            {
                _assemblies.Add(assembly);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a .csproj file.
        /// </summary>
        /// <param name="projectfile"></param>
        public void AddProjectFile(string projectfile)
        {
            var ws = MSBuildWorkspace.Create();
            var project = ws.OpenProjectAsync(projectfile).Result;
            var compilation = project.GetCompilationAsync().Result;

            foreach (var tree in compilation.SyntaxTrees)
            {
                AddSyntaxTree(tree);
            }

        }

        /// <summary>
        /// Adds a C# source file
        /// </summary>
        /// <param name="sourcefile"></param>
        public void AddSource(string sourcefile)
        {
            Trace("processing source '{0}' ...", sourcefile);

            // read and parse code ...
            string cstext;
            using (var reader = new StreamReader(sourcefile))
            {
                cstext = reader.ReadToEnd();
            }

            var tree = CSharpSyntaxTree.ParseText(cstext);
            AddSyntaxTree(tree);
        }

        public void AddSyntaxTree(SyntaxTree tree)
        {
            var root = tree.GetRoot();

            // assume the usings are at the beginning and apply to all classes
            var usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>();

            // iterate class and enum declarations, including nested items.
            foreach (var node in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                var qname = GetQualifiedTypeName(node);

                if (node is ClassDeclarationSyntax | node is EnumDeclarationSyntax | node is InterfaceDeclarationSyntax)
                {
                    Trace("processing [{0}] '{1}' ...", node.GetType().Name, qname);

                    ITypeItem ltype;

                    try
                    {
                        CurrentClassName = qname;

                        ltype = LoadType(qname);
                    }
                    catch (Exception ex)
                    {
                        if (ProcessException(ex))
                        {
                            continue;
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                    finally
                    {
                        CurrentClassName = null;
                    }

                    if (ltype.DoNotGenerate)
                    {
                        continue;
                    }

                    // associate the source syntax node with the type item.
                    var dsc = new DeclarationSourceContext(node, usings);
                    ltype.SetSourceNode(dsc);

                    // register this class
                    ClassToQueue(ltype);
                }
            }
        }

        #endregion


        public void PrepareCode()
        {
            while(_classqueue.Count > 0)
            {
                var ltype = _classqueue.Dequeue();
                try
                {
                    var dsc = ltype.SourceNode as DeclarationSourceContext;

                    if (!ltype.Prepare())
                    {
                        if (null != dsc)
                        {
                            throw new RewriterException(dsc.Declaration, ErrorCode.PreparationError,
                                "class [" + ltype.ID + "] preparation error.");
                        }
                        else
                        {
                            throw new Exception("unable to extract DSC from ltype " + ltype + ".");
                        }
                    }

                    if (null != dsc && ltype.GStatus == TypeGenerationStatus.convert)
                    {
                        // preparation successful, generate code
                        EmitClass(ltype);

                        // produces the final dependencies?
                        ItemReferenced(ltype);
                    }
                }
                catch(Exception ex)
                {
                    ProcessException(ex);
                }
            }
        }

        /// <summary>
        /// Generates JScript code from sources added.
        /// </summary>
        public void GenerateCode()
        {
            EmitPreamle();

            // generate code for classes individually.
            // this must not add type references anymore.
            foreach(var ltype in SortClassesByDependency())
            {
                try
                {
                    //if (null != ltype.SourceNode)
                    if(ltype.GStatus == TypeGenerationStatus.convert)
                    {
                        // EmitClass(ltype);
                        var dgc = (DeclarationEmitContext)ltype.EmitContext;

                        if (Log.ShowClassDependencies || Log.ShowCodeGeneration)
                        {
                            Log.Trace("emit class [{0}] ...", ltype.ID);
                        }

                        RewriteDeclaration(dgc.Writer);
                    }
                    else if(ltype.GStatus == TypeGenerationStatus.external)
                    {
                        EmitNativeScripts(ltype);
                    }
                    else if(ltype.GStatus == TypeGenerationStatus.failed)
                    {
                        Log.Trace("item {0} has failed, not generating.", ltype);
                    }
                    else if(ltype.GStatus != TypeGenerationStatus.source && ltype.GStatus != TypeGenerationStatus.cloaked)
                    {
                        // silent
                        Log.Trace("item {0} status {1} not generating.", ltype, ltype.GStatus);
                    }
                }
                catch(Exception ex)
                {
                    if(ProcessException(ex))
                    {
                        Log.Warning("class '{0}' code generation failed.", ltype.ID);
                        ltype.SetFailed();
                        continue;
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }

            // emit type system scripts first
            var root = ModuleFactory.System;
            foreach (var script in root.ScriptReferences)
            {
                EmitNativeScript(script);
            }
        }

        /// <summary>
        /// Queues the specified ltype for code generation and associates dependencies.
        /// </summary>
        /// <param name="ltype">The type item to enqueue.</param>
        public void ItemReferenced(ITypeItem ltype)
        {
            if(null != CurrentClass)
            {
            }

            if(ltype.GStatus == TypeGenerationStatus.initial || ltype.GStatus == TypeGenerationStatus.source)
            {
                // later
                return;
            }

            ClassToQueue(ltype);

            if(!_classesref.Contains(ltype.ID))
            {
                _classesref.Add(ltype.ID);

                foreach (var dependency in ltype.Dependencies.Select(e => e.Target))
                {
                    ItemReferenced(dependency);
                }

            }
        }

        private void ClassToQueue(ITypeItem ltype)
        {
            if (!_classes.ContainsKey(ltype.ID))
            {
                _classes.Add(ltype.ID, ltype);
                _classqueue.Enqueue(ltype);
            }
        }

        public bool ItemReferenced(string typename)
        {
            var type = _context.ResolveLType(typename);

            if (null == type)
            {
                return false;
            }
            else
            {
                ItemReferenced(type);
                return true;
            }
        }

        public CSharpSyntaxRewriter CreateRewriter(IDeclarationContext context = null)
        {
            return CreateRewriter(null, context);
        }

        private IEnumerable<ITypeItem> SortClassesByDependency()
        {
            Trace("sorting classes ... ");

            var classes = _classes.Values.ToList();
            var dict = new Dictionary<string, ITypeItem>();
            var result = new List<ITypeItem>();

            while(classes.Any())
            {
                for (int j = 0; j < (int)DependencyLevel.Limit; ++j)
                {
                    int processed = 0;
                    foreach (var cls in classes.ToArray())
                    {
                        bool flag = true;
                        foreach (var dep in cls.Dependencies
                            .Where(e => e.Level >= (DependencyLevel)j)
                            .Select(e => e.Target))
                        {
                            TraceDependency("class {0} -> {1}", cls, dep);

                            if (!dict.ContainsKey(dep.ID))
                            {
                                flag = false;
                                break;
                            }
                        }

                        if (!flag) continue;

                        dict[cls.ID] = cls;
                        result.Add(cls);
                        classes.Remove(cls);

                        Trace("  {0}", cls.ID);

                        processed++;
                    }

                    if (0 == processed)
                    {
                        var atlimit = j + 1 == (int)DependencyLevel.Limit;

                        if (Log.ShowCodeGeneration || Log.ShowClassDependencies || atlimit)
                        {
                            var sb = new StringBuilder();
                            foreach (var cls in classes)
                            {
                                sb.AppendFormat("  class [{0}]:", cls.ID);
                                sb.AppendLine();
                                foreach (var dep in cls.Dependencies)
                                {
                                    sb.AppendFormat("    -> [{0,-40}] {1}", dep.Target.CodeName, dep.Level);
                                    sb.AppendLine();
                                }
                            }

                            Log.Trace("circular dependency level {0}:\n{1}", (DependencyLevel)j, sb);
                        }

                        if (atlimit)
                        {
                            throw new Exception("circular dependency: " + classes.Select(c => c.ID).ToSeparatorList());
                        }
                    }
                    else
                    {
                        // exit inner (level) loop
                        break;
                    }
                }
            }

            foreach(var e in result)
            {
            }

            return result;
        }

        private CSharpToJScriptRewriter CreateRewriter(DeclarationEmitContext dgc, IDeclarationContext context = null)
        {
            return new CSharpToJScriptRewriter(dgc, context ?? _context as IDeclarationContext);
        }

        #endregion

        #region Code Generation

        /// <summary>
        /// Emits code before any other generate script code.
        /// </summary>
        private void EmitPreamle()
        {
            if (!_preambleemitted)
            {
                _preambleemitted = true;
            }
        }

        private void EmitDependencyPropertyDefinitions(JScriptWriter jsw, List<DependencyProperty> dplist)
        {
            foreach (var dp in dplist)
            {
                var dptype = dp.PropertyType;
                var ldptype = _context.TranslateRType(dptype, TranslateOptions.MustExist);
                var convert = ldptype.GetConvertMethod();

                jsw.Write(dp.Name + "Property:");
                jsw.EnterBlock();

                jsw.WriteLine("Type: \"" + ldptype.ID + "\",");

                if (null != convert)
                {
                    jsw.WriteLine("Convert: function(p) { return " + convert.ID + "(p); },");
                }

                jsw.LeaveBlock();
                jsw.WriteLine(",");
            }
        }

        private void EmitStaticInitializer(DeclarationEmitContext dgc, JScriptWriter staticinit)
        {
            var cls = dgc.Source.ClassDeclaration;
            var ltype = dgc.LType;
            var typename = ltype.CodeName;
            var rtype = ltype.RType;

            staticinit.WriteLine(typename + ".prototype.$static = ");
            staticinit.EnterBlock();

            if (null != ltype.BaseType)
            {
                staticinit.WriteLine("BaseClass: \"" + ltype.BaseType.CodeName + "\",");
            }

            var dplist = new List<DependencyProperty>();

            // emit static variables
            foreach (var fielddecl in cls.ChildNodes().OfType<FieldDeclarationSyntax>())
            {
                foreach (var declarator in fielddecl.DescendantNodes().OfType<VariableDeclaratorSyntax>())
                {
                    var id = declarator.Identifier.ToString();
                    var field = rtype.GetField(id, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                    if (null != field && field.IsStatic)
                    {
                        if (field.FieldType == typeof(DependencyProperty))
                        {
                            var dp = field.GetValue(null) as DependencyProperty;
                            if (null == dp) continue;
                            dplist.Add(dp);
                        }
                    }
                }
            }

            EmitDependencyPropertyDefinitions(staticinit, dplist);

            staticinit.LeaveBlock();
            staticinit.WriteLine(";");

        }

        private void EmitAutomaticPropertyAccessors(DeclarationEmitContext dgc, PropertyDeclarationSyntax propsyntax)
        {
            var ltype = dgc.LType;
            var prop = ltype.GetMember(propsyntax.Identifier.ToString());
            var name = prop.CodeName;

            dgc.Writer.WriteLine(GetPrototypeClause(ltype, "get_" + name) + " = function() { return this." + DeriveAutomaticName(name) + "; }");
            dgc.Writer.WriteLine(GetPrototypeClause(ltype, "set_" + name) + " = function(value) { this." + DeriveAutomaticName(name) + " = value; }");
        }

        private void EmitAutomaticPropertyInitializer(DeclarationEmitContext dgc, string membername)
        {
            var prop = dgc.LType.GetMember(membername);
        }

        private void EmitConstructorFields(DeclarationEmitContext dgc, CSharpToJScriptRewriter rewriter, JScriptWriter jsw)
        {
            var info = dgc.LType;
            var rtype = info.RType;

            var cls = dgc.Source.ClassDeclaration;

            // emit class member fields (LN4NL4H5PI)
            foreach (var fielddecl in cls.ChildNodes().OfType<FieldDeclarationSyntax>())
            {
                foreach (var declarator in fielddecl.DescendantNodes().OfType<VariableDeclaratorSyntax>())
                {
                    var id = declarator.Identifier.ToString();
                    if (null != rtype)
                    {
                        var field = rtype.GetField(id, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        if (null != field && field.IsStatic)
                        {
                            continue;
                        }
                    }

                    // identifier, prepend 'this'.
                    //var idsyntax = SyntaxFactory.IdentifierName(declarator.Identifier);
                    var idsyntax = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(Settings.OuterThis),
                        SyntaxFactory.IdentifierName(declarator.Identifier)
                        );
                    var idnode = rewriter.Visit(idsyntax);
                    jsw.Write(idnode.ToString());

                    var equals = declarator.ChildNodes().FirstOrDefault();
                    if (null != equals)
                    {
                        var node = rewriter.Visit(equals);
                        jsw.Write(node.ToString());
                    }
                    else
                    {
                        jsw.Write(" = null");
                    }

                    jsw.WriteLine(";");
                }
            }

            // event objects (LMGMELDMQ7)
            foreach (var ev in cls.Members.OfType<EventFieldDeclarationSyntax>())
            {
                foreach (var v in ev.Declaration.Variables)
                {
                    // initialize event object to null, as in C#
                    var id = Settings.OuterThis + "." + v.Identifier;
                    jsw.WriteLine(id + " = null;");

                    jsw.WriteLine("this.add_" + v.Identifier + " = function(h) {");
                    jsw.WriteLine(id + " = Delegate.Combine(" + id + ", h); };");
                    jsw.WriteLine("this.remove_" + v.Identifier + " = function(h) {");
                    jsw.WriteLine(id + " = Delegate.Remove(" + id + ", h); };");
                    jsw.WriteLine("this.fire_" + v.Identifier + " = function(sender, e) {");
                    jsw.WriteLine("Delegate.Fire(" + id + ", sender, e); };");
                }
            }

            // constructor
            var constructor = cls.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
            if (null != constructor)
            {
                foreach (var stmt in constructor.Body.Statements)
                {
                    var ctor = rewriter.Visit(stmt);
                    // ctor = AddLambdaThis(ctor);
                    jsw.Write(ctor.ToString());
                }
            }
        }

        private void EmitConstructor(DeclarationEmitContext dgc, IEnumerable<string> autoprops)
        {
            var ltype = dgc.LType;
            var rtype = ltype.RType;
            var typename = ltype.CodeName;

            var declctx = (IDeclarationContext)ltype;

            // method context for constructor
            var context = ModuleFactory.CreateMethodContext(declctx);

            // rewriter
            var rewriter = CreateRewriter(dgc, context);

            // writer receiving constructor and related code.
            JScriptWriter jsw = new JScriptWriter();

            // initialize the prototype
            string basename = null;
            if (null != ltype.BaseType)
            {
                if (null != ltype.BaseType.RType)
                {
                    basename = ltype.BaseType.CodeName;
                    jsw.WriteLine(typename + ".prototype = new " + basename + "();");
                    jsw.WriteLine(typename + ".prototype.constructor = " + typename + ";");
                }
            }

            // static
            EmitStaticInitializer(dgc, jsw);

            // analyze constructors ...
            var cls = dgc.Source.ClassDeclaration;
            var constructors = cls.Members.OfType<ConstructorDeclarationSyntax>();
            if (constructors.Count() > 1)
            {
                throw new Exception("multiple constructors are not supported [" + typename + "].");
            }

            var constructor = constructors.FirstOrDefault();
            var ps = string.Empty;
            if (null != constructor)
            {
                var parameters = constructor.ParameterList.ChildNodes().OfType<ParameterSyntax>();
                rewriter.AddMethodParameters(parameters);
                ps = TranslateMethodParameters(parameters);
            }


            // constructor method
            jsw.WriteLine("function " + typename + "(" + ps + ")");
            jsw.EnterBlock();

            // call base constructor
            if (null != basename)
            {
                jsw.WriteLine(basename + ".prototype.constructor.call(this);");
            }

            // type identifier
            jsw.WriteLine("this.$type = '" + typename + "';");

            // initialize our 'this' replacement.
            var cthis = CreateLambdaThis();
            jsw.Write(cthis.ToString());

            foreach (var ap in autoprops)
            {
                jsw.Write(Settings.OuterThis + "." + DeriveAutomaticName(ap) + " = null;");
            }

            foreach (var dp in dgc.DependencyPropertyInitializations)
            {
                // TODO: constant management required.
                //var dvalue = dp.DefaultMetadata.DefaultValue;
                object dvalue = null;
                if (null == dvalue)
                {
                    jsw.Write(Settings.OuterThis + ".InitValue(\"" + dp.Name + "\");");
                }
                else
                {
                    jsw.Write(Settings.OuterThis + ".InitValue(\"" + dp.Name + "\", " + dvalue + ");");
                }
            }

            // field initializers and constructor body ...
            EmitConstructorFields(dgc, rewriter, jsw);

            jsw.LeaveBlock();

            // commit to class output
            dgc.Writer.Write(jsw);
        }

        private void EmitGetAccessor(DeclarationEmitContext dgc, string name, SyntaxNode body)
        {
            EmitMethod(dgc, name, null, body);
        }

        private void EmitSetAccessor(DeclarationEmitContext dgc, string name, SyntaxNode body)
        {
            var z = SyntaxFactory.ParseParameterList("object value;");
            EmitMethod(dgc, name, z.DescendantNodes().OfType<ParameterSyntax>(), body);
        }

        private void EmitMethod(DeclarationEmitContext dgc, MethodDeclarationSyntax m)
        {
            var args = m.ParameterList.DescendantNodes().OfType<ParameterSyntax>();
            EmitMethod(dgc, m.Identifier.ToString(), args, m.Body);
        }

        private void EmitMethod(DeclarationEmitContext dgc, string id, IEnumerable<ParameterSyntax> parameters, SyntaxNode body)
        {
            var ltype = dgc.LType;
            var qname = ltype.ID + "." + id;

            TraceEmit("  method '{0}' ...", id);

            var rtype = ltype.RType;
            var ps = TranslateMethodParameters(parameters);

            var jsw = new JScriptWriter();
            jsw.Write(GetPrototypeClause(ltype, id) + " = function(" + ps + ")");

            // method context
            var context = ModuleFactory.CreateMethodContext((IDeclarationContext)ltype);

            // CS to JS rewriter using the method context.
            var rewriter = CreateRewriter(dgc, context);

            // parameter variables 
            rewriter.AddMethodParameters(parameters);

            if (null == body)
            {
                Trace("WARNING: method " + id + " has no body.");
                return;
            }

            // convert the method body
            var node = rewriter.Visit(body);

            // prefix with the 'this' replacement
            node = AddLambdaThis(node);

            // and emit the JS method declaration
            jsw.WriteLine(node.ToString());
            dgc.Writer.Write(jsw);
        }

        /// <summary>
        /// Generates code for a given class type and associates it with the type item.
        /// </summary>
        /// <param name="ltype">The class type item to generate.</param>
        /// <remarks>
        /// <para>The DeclarationEmitContext will be set as the EmitContext.</para></remarks>
        private void EmitClass(ITypeItem ltype)
        {
            if (Log.ShowClassDependencies || Log.ShowCodeGeneration)
            {
                Log.Trace("generate class [{0}] ...", ltype.ID);
            }

            if(null != ltype.EmitContext)
            {
                throw new InvalidOperationException("item " + ltype + " already has an emit context.");
            }

            try
            {
                CurrentClass = ltype;

                var dsc = (DeclarationSourceContext)ltype.SourceNode;
                var cls = (ClassDeclarationSyntax)dsc.ClassDeclaration;

                // create emit context
                var dgc = new DeclarationEmitContext(ltype, dsc);
                ltype.SetEmitContext(dgc);
                dgc.OnItemReferenced = ItemReferenced;

                dgc.AddItemReferences();

                // look for automatic properties
                var autoprops = cls.Members
                    .OfType<PropertyDeclarationSyntax>()
                    .Where(IsAutomaticProperty)
                    .Select(ap => ap.Identifier.ToString());



                EmitConstructor(dgc, autoprops);

                var classmembers = ltype as IVariableContext;

                // add member methods and properties ...
                foreach (var member in cls.Members)
                {
                    if (member is MethodDeclarationSyntax)
                    {
                        var method = member as MethodDeclarationSyntax;
                        var name = method.Identifier.ToString();
                        var v = classmembers.GetVariable(name);
                        if (!v.DoNotGenerate)
                        {
                            EmitMethod(dgc, method);
                        }
                    }
                    else if (member is PropertyDeclarationSyntax)
                    {
                        var prop = (PropertyDeclarationSyntax)member;
                        var propname = prop.Identifier.ToString();

                        /*Trace("property {0}:", propname);
                        SyntaxTreeHelper.PrintTree(prop);*/

                        if (IsAutomaticProperty(prop))
                        {
                            // automatic property 
                            EmitAutomaticPropertyAccessors(dgc, prop);
                        }
                        else
                        {
                            // emit property accessors
                            foreach (var acc in member.DescendantNodes().OfType<AccessorDeclarationSyntax>())
                            {
                                if (acc.Keyword.ToString() == "get")
                                {
                                    EmitGetAccessor(dgc, "get_" + propname, acc.Body);
                                }
                                else
                                {
                                    EmitSetAccessor(dgc, "set_" + propname, acc.Body);
                                }
                            }
                        }
                    }
                    else if (member is EventFieldDeclarationSyntax)
                    {
                        // done LMGMELDMQ7
                    }
                    else if (member is FieldDeclarationSyntax)
                    {
                        // done LN4NL4H5PI
                    }
                    else if (member is ConstructorDeclarationSyntax)
                    {
                        // handled before
                    }
                    else if (member is EnumDeclarationSyntax)
                    {
                        // skip nested
                    }
                    else if (member is ClassDeclarationSyntax)
                    {
                        // skip nested
                    }
                    else
                    {
                        Log.Warning("skipped production [{0}] while generating class code.", member.GetType().Name);
                        SyntaxTreeHelper.PrintTree(member);
                    }
                }
            }
            finally
            {
                CurrentClass = null;
            }
        }

        #endregion

        #region Private Methods

        private bool ProcessException(Exception error)
        {
            if (null != Error)
            {
                var e = new ConverterErrorEventArgs(error);
                Error(this, e);
                return e.Handled;
            }
            else
            {
                return false;
            }
        }

        private string GetQualifiedTypeName(BaseTypeDeclarationSyntax cls)
        {
            var qname = cls.Identifier.ToString();

            foreach (var a in cls.Ancestors())
            {
                if(a is NamespaceDeclarationSyntax)
                {
                    qname = a.ChildNodes().First().ToString() + "." + qname;
                }
                else if(a is ClassDeclarationSyntax)
                {
                    qname = ((ClassDeclarationSyntax)a).Identifier + "+" + qname;
                }
            }

            return qname;
        }

        private bool IsAutomaticProperty(PropertyDeclarationSyntax prop)
        {
            return prop.DescendantNodes()
                .OfType<AccessorDeclarationSyntax>()
                .Where(acc => !acc.ChildNodes().Any())
                .Any();
        }

        private ITypeItem LoadType(string qname)
        {
            Type rtype = null;
            foreach (var assembly in _assemblies)
            {
                if (null != (rtype = assembly.GetType(qname))) break;
            }

            if (null == rtype)
            {
                throw new Exception("rtype [" + qname + "] was not found in any assembly, did you compile the projects?");
            }

            return _context.TranslateRType(rtype, TranslateOptions.Add);
        }

        private void RewriteDeclaration(JScriptWriter jsw)
        {
            try
            {
                _declarations.RewriteProgram(jsw.Text);
            }
            catch (Exception ex)
            {
                while (ex is TargetInvocationException) ex = ex.InnerException;

                Warning("parse error: {0}\n  failed source:\n{1}", ex.Message, jsw.Text);
                throw ex;
            }
        }

        private string DeriveAutomaticName(string name)
        {
            return "_" + name;
        }

        private StatementSyntax CreateLambdaThis()
        {
            return SyntaxFactory.ParseStatement("var " + Settings.OuterThis + " = this;");
        }

        private SyntaxNode AddLambdaThis(SyntaxNode node)
        {
            //SyntaxTreeHelper.PrintTree(node);
            if (!node.GetAnnotations("haslambdas").Any())
            {
                return node;
            }

            /*if (!node.DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>().Any())
            {
                return node;    
            }*/

            var stmt = CreateLambdaThis();
            var stmtlist = new SyntaxList<StatementSyntax>();
            stmtlist = stmtlist.Add(stmt);
            stmtlist = stmtlist.AddRange(node.ChildNodes().OfType<StatementSyntax>());
            node = SyntaxFactory.Block(stmtlist);


            return node;
        }

        private string GetPrototypeClause(ITypeItem typeinfo, string id)
        {
            return typeinfo.CodeName + ".prototype." + id;
        }

        private static string ResolveLType(TypeSyntax node)
        {
            var gensyn = node as GenericNameSyntax;
            if (null != gensyn) return gensyn.Identifier.ToString();
            else return node.ToString();
        }

        private ITypeItem ResolveType(ITypeItem container, Type rtype)
        {
            return null;
        }

        internal static void AddMethodParameters(IDeclarationContext context, IEnumerable<ParameterSyntax> parameters)
        {
            if (null != parameters)
            {
                foreach (var par in parameters)
                {
                    ITypeItem ltype = null;
                    string typename = null;
                    if (null != par.Type)
                    {
                        typename = ResolveLType(par.Type);
                        ltype = context.ResolveLType(typename);
                    }

                    if (null == ltype && null != par.Type)
                    {
                        // TraceTarget.Trace("WARNING: unresolved parameter type '{0}'.", typename);
                    }

                    context.AddVariable(par.Identifier.ToString(), ltype);
                }
            }
        }

        private string TranslateMethodParameters(IEnumerable<ParameterSyntax> parameters)
        {
            return null == parameters ?
                string.Empty
                : parameters.Select(p => p.Identifier.ToString()).ToSeparatorList();
        }

        private void EmitNativeScript(IScriptReference script)
        {
            using (var s = script.Assembly.GetManifestResourceStream(script.Key))
            {
                if (null == s)
                {
                    Log.Warning("native script '{0}' was not found.", script.Key);
                }
                else
                {
                    if (Log.ShowScripts || Log.ShowCodeGeneration)
                    {
                        Log.Trace("  inline script '{0}' ...", script.Path);
                    }

                    var jsw = new JScriptWriter();
                    using (var reader = new StreamReader(s))
                    {
                        jsw.WriteLine(reader.ReadToEnd());
                    }

                    RewriteDeclaration(jsw);
                }
            }
        }

        private void EmitNativeScripts(ITypeItem ltype)
        {
            if(Log.ShowCodeGeneration)
            {
                Log.Trace("emit native [" + ltype.ID + "] ...");
            }

            foreach(var script in ltype.ScriptReferences)
            {
                if(!_nativescripts.Contains(script.Key))
                {
                    _nativescripts.Add(script.Key);
                    EmitNativeScript(script);
                }
            }
        }


        #endregion

        #region Settings

        internal class Settings
        {
            public static string OuterThis = "t$";
        }

        #endregion
    }
}
