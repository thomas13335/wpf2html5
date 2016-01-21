using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Wpf2Html5.Exceptions;
using Wpf2Html5.StockObjects;
using Wpf2Html5.TypeSystem.Collections;
using Wpf2Html5.TypeSystem.Interface;
using Wpf2Html5.TypeSystem.Profiles;

namespace Wpf2Html5.TypeSystem.Items
{
    /// <summary>
    /// Represents the root of a type system used for code translation.
    /// </summary>
    class Root : Container, ITypeRoot
    {
        #region Private

        private List<ScriptReference> _scriptrefs = new List<ScriptReference>();

        #endregion

        #region Properties

        public override IEnumerable<IScriptReference> ScriptReferences { get { return _scriptrefs; } }

        #endregion

        #region ITypeContext

        public override ITypeItem TranslateRType(Type type, TranslateOptions options)
        {
            ITypeItem result;

            if (type.IsGenericParameter)
            {
                throw new Exception("generic parameters are not supported.");
            }

            if (!Types.TryGetItem(type.GetRName(), out result))
            {
                if (options.IsAdd())
                {
                    result = new NativeTypeRef(this, type);
                    result.SetExternal();
                }
                else
                {
                    if (type.IsGenericType)
                    {
                        var ge = new GenericInterfaceEnumerator(type);

                        foreach (var itf in ge.Interfaces)
                        {
                            if (Types.TryGetItem(itf.GetRName(), out result))
                            {
                                break;
                            }
                        }

                        if (null == result)
                        {
                            throw new UnresolvedTypeException("no mapping for generic type [" + type.GetRName() + "]");
                        }
                    }
                }
            }

            if (null == result && options == TranslateOptions.MustExist)
            {
                throw new UnresolvedTypeException("runtime type '" + type.GetRName() + "' could not be resolved.");
            }

            return result;
        }

        /// <summary>
        /// Translates C# abrevations to their corresponding CLR types.
        /// </summary>
        /// <param name="name">The name to resolve.</param>
        /// <returns>The corresponding type item.</returns>
        public override ITypeItem ResolveLType(string name)
        {
            switch (name)
            {
                case "object":
                    return this.GetLType(typeof(object));

                case "string":
                    return this.GetLType(typeof(string));

                case "int":
                    return this.GetLType(typeof(int));

                case "long":
                    return this.GetLType(typeof(long));

                case "bool":
                    return this.GetLType(typeof(bool));

                default:
                    return base.ResolveLType(name);
            }
        }

        /// <summary>
        /// All types inserted on this level are considered native types.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private ITypeItem AddRType(Type type)
        {
            ITypeItem result;
            if (!Types.TryGetItem(type.GetRName(), out result))
            {
                result = new NativeTypeRef(this, type);
                result.SetExternal();
            }

            return result;
        }

        #endregion

        #region Construction

        public Root()
            : base(null, "global")
        {
            Verbose = true;

            Trace("initializing type system ...");

            AddTypeItem(new VoidType());

            // special methods
            AddVariable(new Method(this, "trace", null));

            var loader = new ProfileLoader(this);
            loader.LoadResource("System");
            loader.LoadResource("System.Collections.Generic");

            var c = typeof(ICommand);

#if FOO
            // collections
            // new NativeTypeRef(this, typeof(IList<>).GetGenericTypeDefinition());
            new NativeTypeRef(this, typeof(List<int>).GetGenericTypeDefinition());
            new NativeTypeRef(this, typeof(IEnumerable<int>).GetGenericTypeDefinition());
            new NativeTypeRef(this, typeof(IEnumerator<int>).GetGenericTypeDefinition());
            new NativeTypeRef(this, typeof(ObservableCollection<int>).GetGenericTypeDefinition())
                .WithScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.ObservableCollection.js");

            new NativeTypeRef(this, typeof(IDictionary<int, int>).GetGenericTypeDefinition());
            new NativeTypeRef(this, typeof(Dictionary<int, int>).GetGenericTypeDefinition())
                .WithScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.Dictionary.js");

            AddRType(typeof(NotImplementedException));

            // other system
            //AddRType(typeof(Action));
            new NativeTypeRef(this, typeof(Action<int>).GetGenericTypeDefinition());
            AddRType(typeof(ICommand));

            AddRType(typeof(CultureInfo));

            // XML
            new NativeTypeRef(this, "IGRA3.XML.XmlSerializable").SetExternal();


            InitializeWPF();

            InitializeHtmlControls();
#endif

            Trace("type system initialized.");
        }

        #endregion

        #region Private

        private NativeTypeRef InitializeWPF()
        {
            // WPF types
            var ntype = (NativeTypeRef)AddRType(typeof(DependencyObject));
            AddDependencyObjectScripts(ntype);

            AddRType(typeof(DependencyProperty));
            var ltype = AddRType(typeof(DependencyPropertyChangedEventArgs));
            ltype.SetDisableTranslation("Property", "OldValue", "NewValue");

            var controls = new string[] 
            { 
                "TextBlock", 
                "TextBox", 
                "Button", 
                "StackPanel", 
                "ContentPresenter",
                "ItemsControl",
                "DockPanel",
                "ComboBox",
                "Border",
                "Image",
                "CheckBox",
                "UserControl"
            };

            var wpf = typeof(FrameworkElement).Assembly;
            foreach (var control in controls)
            {
                AddControlScripts(control, "System.Windows.Controls", wpf);
            }

            var docc = new string[] 
            {
                "Hyperlink",
                "InlineUIContainer",
                "Run"
            };

            foreach (var control in docc)
            {
                AddControlScripts(control, "System.Windows.Documents", wpf);
            }

            var stockcontrols = new string[] { "PleaseWaitControl", "BorderWithMouse" };
            var stock = typeof(Stock).Assembly;
            foreach (var control in stockcontrols)
            {
                AddControlScripts(control, "Wpf2Html5.StockObjects", stock);
            }

            // stock types
            ntype = (NativeTypeRef)AddRType(typeof(RelayCommand));
            ntype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.RelayCommand.js");

            // global script refs
            _scriptrefs.Add(new ScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.trace.js"));
            _scriptrefs.Add(new ScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.ready.js"));

            AddControlScripts(typeof(W2HPasswordBox));
            AddControlScripts(typeof(GoogleReCaptcha));

            AddControlScripts(typeof(JsonWebRequest));

            new NativeTypeRef(this, typeof(JsonObjectFactory<int>).GetGenericTypeDefinition())
                .WithScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.JsonObjectFactory.js");

            AddClassScripts(typeof(PathOperations));
            AddClassScripts(typeof(StringValidation));
            AddClassScripts(typeof(StringOperations));

            //AddClassScripts(typeof(ListUtility));
            new NativeTypeRef(this, typeof(ListUtility<int>).GetGenericTypeDefinition())
                .WithScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.List.js");

            AddClassScripts(typeof(WebViewModelBase));

            return ntype;
        }

        private void InitializeHtmlControls()
        {
            AddClassScripts(typeof(HtmlEditorView));
            AddClassScripts(typeof(HtmlUploadControl));
            AddClassScripts(typeof(HtmlFlexBox));
            AddClassScripts(typeof(HtmlTextView));
            AddClassScripts(typeof(HtmlIFrame));
            AddClassScripts(typeof(LocalStorageAccessor));
        }

        private NativeTypeRef AddClassScripts(Type type)
        {
            var ctype = new NativeTypeRef(this, type, true);
            ctype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts." + type.Name + ".js");
            return ctype;
        }

        private void AddControlScripts(Type type)
        {
            var ctype = new NativeTypeRef(this, type, true);
            ctype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts." + type.Name + ".js");
        }

        private void AddDependencyObjectScripts(NativeTypeRef ntype)
        {
            ntype.SetExternal();
            ntype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.Delegate.js");
            ntype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.ControlFactory.js");
            ntype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.Binding.js");
            ntype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.InputBinding.js");
            ntype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.DependencyObject.js");
            ntype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.Control.js");
            ntype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.TemplateFactory.js");
            ntype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.DataTypes.js");

            // TODO: does not belong here!
            ntype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.TypeSystem.js");

            // TODO: neither do these.
            ntype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.UserControl.js");
            ntype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.PathControl.js");
            ntype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.Converters.js");

        }

        private void AddControlScripts(string typename, string ns, Assembly assembly)
        {
            var fullname = ns + "." + typename;
            var rtype = assembly.GetType(fullname);
            var ctype = new NativeTypeRef(this, rtype);

            AddDependencyObjectScripts(ctype);

            ctype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.Control.js");

            if (typename == "ComboBox")
            {
                ctype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts.ItemsControl.js");
            }

            ctype.AddScriptReference(typeof(StockObjectRoot).Assembly, "Scripts." + typename + ".js");
        }

        #endregion
    }
}
