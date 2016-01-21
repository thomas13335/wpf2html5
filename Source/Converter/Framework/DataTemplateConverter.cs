using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using Wpf2Html5.Builder;
using Wpf2Html5.Converter.Interface;

namespace Wpf2Html5.Converter.Framework
{
    /// <summary>
    /// Converts a DataTemplate.
    /// </summary>
    class DataTemplateConverter : ControlConverterBase<DataTemplate>, IConverterContext
    {
        #region Private

        #region Nested Classes

        class ControlBindings
        {
            public Dictionary<string, object> PropertyMap = new Dictionary<string, object>();

            public IControlConverter Target { get; set; }
        }

        class HtmlItem
        {
            private StringBuilder _innertext;

            /// <summary>
            /// The static ID of the control converter.
            /// </summary>
            public string ConverterID { get; set; }

            public string InnerText { get { return null == _innertext ? null : _innertext.ToString(); } }

            public string ctype;

            public bool WrapperCreated { get; set; }

            public void AddInnerText(string text)
            {
                if (null == _innertext) _innertext = new StringBuilder();
                _innertext.Append(text);
            }
        }

        #endregion

        private IConverterStack _cstack = new ConverterStack();
        private static int _templateseed = 0;
        private string _templatecontextid = null;

        private JScriptBuilder _builder = new JScriptBuilder();
        private int _id;
        private Dictionary<string, ControlBindings> _controlmap = new Dictionary<string, ControlBindings>();

        #endregion

        #region Properties

        public override bool IsTypeRegistrable { get { return false; } }

        #endregion

        #region Construction

        public DataTemplateConverter()
        {
        }

        #endregion

        #region Public Methods

        public override void Convert(System.Xml.XmlWriter writer, ConverterArguments args)
        {
            // get the template context ... must exist
            var dc = Context.GetAncestors<DataTemplateContext>().First();
            _templatecontextid = dc.ID;

            _id = ++_templateseed;


            ConvertInner(writer, args);
        }

        #endregion

        #region Private Methods

        private void ConvertInner(System.Xml.XmlWriter writer, ConverterArguments args)
        {
            string typename;
            object datatype = Control.DataType;
            if (datatype is Type)
            {
                typename = (datatype as Type).Name;
            }
            else if (datatype is string)
            {
                typename = datatype.ToString();
            }
            else if(null == datatype)
            {
                throw new Exception("missing DataType specification on DataTemplate.");
            }
            else
            {
                throw new Exception("expected type, got [" + datatype + "].");
            }

            object resourcekey = null;
            var dc = Context.GetAncestors<DataTemplateContext>().First();

            if(null != dc.KeyMap && dc.KeyMap.ContainsKey(Control))
            {
                resourcekey = dc.KeyMap[Control];
            }

            // generate a unique name for this template
            var factory = typename + "_Template_" + _id;

            // convert the content of the template into HTML
            DependencyObject templatecontent;
            try
            {
                templatecontent = Control.LoadContent();
            }
            catch (Exception ex)
            {
                Log.Warning("data template {0} failed to load content: {1}", Control.DataTemplateKey, ex.Message);
                return;
            }

            var innerhtml = new StringBuilder();

            if (null != templatecontent)
            {
                using (var innerwriter = XmlWriter.Create(innerhtml, new XmlWriterSettings { Indent = true }))
                {
                    // pass this context as an argument, needed for nested data templates.
                    Convert(templatecontent, innerwriter, this);
                }
            }
            else
            {
                Log.Trace("DataTemplate for [{0}] is empty.", typename);
                return;
            }

            if (Log.ShowDataTemplateHtml)
            {
                Log.Trace("converting HTML factory {0}:\n{1}", factory, innerhtml);
            }


            // generate a JScript factory function for this template
            var jsw = new JScriptWriter();
            jsw.Write("function " + factory + "(parentid, container, datacontext) ");
            jsw.EnterBlock();

            if (innerhtml.Length > 0)
            {
                using (var sreader = new StringReader(innerhtml.ToString()))
                using (var reader = XmlReader.Create(sreader))
                {
                    EmitHtmlFactory(jsw, reader);
                }
            }

            jsw.LeaveBlock();

            // TraceTarget.Trace("out:\n{0}", jsw.Text);

            var info = new DataTemplateInfo();
            info.Key = factory;
            info.ContextID = dc.ID;
            info.TypeName = typename;
            info.ResourceKey = resourcekey;
            info.Control = Control;
            info.FactoryCode = jsw;

            // register the factory 
            Context.RegisterDataTemplate(Control, info);
        }

        private void EmitJSSetAttributeConst(JScriptWriter jsw, string id, string name, string value)
        {
            jsw.WriteLine(id + ".setAttribute(" + QuoteString(name) + ", " + QuoteString(value) + ");");
        }

        private void EmitJSSetAttributeArg(JScriptWriter jsw, string id, string name, string arg)
        {
            jsw.WriteLine(id + ".setAttribute(" + QuoteString(name) + ", " + arg + ");");
        }

        /// <summary>
        /// Emits a JScript function that generates the given HTML content dynamically.
        /// </summary>
        /// <param name="jsw">The target script writer.</param>
        /// <param name="reader">The reader for the HTML content to convert.</param>
        private void EmitHtmlFactory(JScriptWriter jsw, XmlReader reader)
        {
            reader.MoveToContent();
            var stack = new Stack<HtmlItem>();

            jsw.WriteLine("var L = [ ]; var E = null; var C = null;");

            jsw.WriteLine("var sid;");

            // enumerate nodes in the XML argument recursively ...
            while(reader.NodeType != XmlNodeType.None)
            {
                bool popflag = false;
                reader.MoveToContent();
                if(reader.NodeType == XmlNodeType.Element)
                {
                    // generate constructor code for element ...
                    jsw.WriteLine("E = document.createElement(" + QuoteString(reader.LocalName) + ");");
                    if(stack.Count == 0)
                    {
                        // insert top level item into the container parameter.
                        jsw.WriteLine("container.appendChild(E);");
                    }
                    else
                    {
                        // append into parent
                        jsw.WriteLine("L[0].appendChild(E);");
                    }

                    // the template context is required to get the right data templates
                    // is just the static control identifier ...
                    var id = reader.GetAttribute("id");
                    if (null != id)
                    {
                        EmitJSSetAttributeConst(jsw, "E", "data-template-context", id);
                    }

                    // TODO: create object handle
                    // jsw.WriteLine("E.setAttribute('data-id', DependencyObject_CreateHandle(datacontext));");

                    // allocate a new stack item for this element
                    var top = new HtmlItem();
                    stack.Push(top);

                    var createwrapper = false;
                    var propdict = new Dictionary<string, string>();
                    

                    // process attributes
                    if(reader.HasAttributes)
                    {
                        if(reader.MoveToFirstAttribute())
                        {
                            do
                            {
                                // pass to output?
                                var skip = true;

                                // attribute name (not qualified)
                                var name = reader.LocalName;
                                if (name == "id")
                                {
                                    top.ConverterID = reader.Value;
                                    EmitJSSetAttributeArg(jsw, "E", "id", "Control_allocatedynamicid()");
                                }
                                else if(name.StartsWith("dp."))
                                {
                                    propdict.Add(name.Substring(3), reader.Value);
                                }
                                else if (reader.Name == "name")
                                { }
                                else if (reader.Name == "xmlns")
                                {
                                }
                                else if(reader.Name == "wrapper")
                                {
                                    // extract value of RDLN7CYSPV
                                    createwrapper = reader.Value == "true";
                                }
                                else if(reader.Name == "data-ctype")
                                {
                                    top.ctype = reader.Value;
                                    skip = false;
                                }
                                else
                                {
                                    skip = false;
                                }

                                // pass attribute to JS
                                if(!skip)
                                {
                                    EmitJSSetAttributeConst(jsw, "E", reader.LocalName, reader.Value);
                                }
                            }
                            while (reader.MoveToNextAttribute());
                        }

                        reader.MoveToContent();
                    }

                    // push the new element onto the stack.
                    jsw.WriteLine("L.unshift(E);");

                    if (createwrapper)
                    {
                        // RDLN7CYSPV: issue wrapper creation here, might be too early for some controls.
                        jsw.WriteLine("C = ControlFactory.getcontrolwrapper(E);");
                        foreach(var prop in propdict)
                        {
                            jsw.WriteLine("C.SetValue(" + QuoteString(prop.Key) + ", " + QuoteString(prop.Value) + ");");
                        }

                        top.WrapperCreated = true;
                    }

                    /*if (null != staticid)
                    {
                        EmitControlBindings(staticid, jsw);
                    }*/ 

                    if (reader.IsStartElement() && !reader.IsEmptyElement)
                    {
                        reader.ReadStartElement();
                    }
                    else
                    {
                        reader.Skip();
                        popflag = true;
                    }
                }
                else if(reader.NodeType == XmlNodeType.EndElement)
                {
                    reader.ReadEndElement();
                    popflag = true;
                }
                else if(reader.NodeType == XmlNodeType.Text)
                {
                    //jsw.WriteLine("list[0].innerText += '" + reader.Value + "';");
                    stack.Peek().AddInnerText(reader.Value);
                    reader.Read();
                }
                else
                {
                    reader.Skip();
                }

                if (popflag)
                {
                    // pop the current element, adding possible text
                    var current = stack.Peek();

                    string textmethod = null;

                    if (null != current.ConverterID)
                    {
                        var converter = Context.GetControlByID(current.ConverterID);
                        if (null != converter)
                        {
                            textmethod = converter.GetSupportMethodName(SupportMethodID.ApplyControlText);
                        }
                        else
                        {
                            Log.Warning("data template did not find converter '" + current.ConverterID + "'.");
                        }
                    }

                    if (null != current.InnerText && !current.WrapperCreated)
                    {
                        // 7A27DOZU5F
                        if (null == textmethod)
                        {
                            jsw.WriteLine("E.textContent = " + QuoteString(current.InnerText) + ";");
                        }
                        else
                        {
                            jsw.WriteLine(textmethod + "(E, " + QuoteString(current.InnerText) + ");");
                        }
                    }

                    if (null != current.ConverterID)
                    {
                        EmitControlBindings(current.ConverterID, jsw);
                    }

                    stack.Pop();

                    if (reader.Depth > 0)
                    {
                        //jsw.WriteLine("pop = " + ListHeadExpression + "; " + NameOfListVariable + ".shift();");
                        jsw.WriteLine("L.shift(); E = L[0];");
                    }

                }
            }

            //TraceTarget.Trace("jsw: \n{0}", jsw.Text);

            // jsw.WriteLine("trace('template result: ' + list[0].outerHTML);");
            jsw.WriteLine("return E;");
        }

        private bool IsQuotedAttributeValue(string name)
        {
            switch(name)
            {
                //case "onkeyup":
                //case "ondblclick":
                    //return false;

                default:
                    return true;
            }
        }

        private string QuoteString(string s)
        {
            return "\"" + s.Replace("\"", "\\\"") + "\"";
        }

        private ControlBindings RegisterControlBinding(IControlConverter target)
        {
            ControlBindings cb;
            if (!_controlmap.TryGetValue(target.ID, out cb))
            {
                _controlmap[target.ID] = cb = new ControlBindings { Target = target };

                // Trace("  registerbinding {0} in {1}", target.ID, _id);
            }

            return cb;
        }

        private void EmitControlBindings(string id, JScriptWriter jsw)
        {
            var bconv = new BindingConverter(jsw);

            //Trace("  emit binding for {0} in {1} ...", id, _id);

            ControlBindings cb;
            if(_controlmap.TryGetValue(id, out cb))
            {
                // Trace("  emit binding {0}", id);
                foreach (var e in cb.PropertyMap)
                {
                    if(e.Value is Binding)
                    {
                        bconv.AddBinding("E", e.Key, e.Value as Binding);
                    }
                    else if(e.Value is KeyBinding)
                    {
                        bconv.AddKeyBinding("E", e.Value as KeyBinding);
                    }
                    else if(e.Value is MouseBinding)
                    {
                        bconv.AddMouseBinding("E", e.Value as MouseBinding);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            else
            {
                // Trace("  nobinding {0} in {1}", id, this.GetHashCode());
            }
        }

        #endregion

        #region IConverterContext

        public string AllocateIdentifier(IControlConverter converter)
        {
            return Context.AllocateIdentifier(converter);
        }

        public IControlConverter GetControlByID(string id)
        {
            return Context.GetControlByID(id);
        }

        public IControlConverter Convert(object obj, XmlWriter writer, IConverterContext cc = null, ConverterArguments args = null)
        {
            return Context.Convert(obj, writer, cc ?? this, args);
        }

        public JScriptBuilder CodeBuilder
        {
            get { return _builder; }
        }

        public JScriptBuilder Declarations
        {
            get { return Context.Declarations; }
        }

        public void AddBinding(IControlConverter target, string prop, Binding b)
        {
            var cb = RegisterControlBinding(target);
            cb.PropertyMap[prop] = b;
        }

        public void AddKeyBinding(IControlConverter target, KeyBinding kb)
        {
            var cb = RegisterControlBinding(target);
            var fkey = KeyInterop.VirtualKeyFromKey(kb.Key);

            var name = "__key_" + fkey;
            cb.PropertyMap[name] = kb;
        }

        public void AddMouseBinding(IControlConverter target, MouseBinding mb)
        {
            var cb = RegisterControlBinding(target);
            var name = "__mouse_" + mb.MouseAction.ToString();
            cb.PropertyMap[name] = mb;
        }

        public void AddSizeBinding(IControlConverter container, string suffix)
        {
            // throw new NotImplementedException();
        }

        public void RegisterDataTemplate(DataTemplate template, DataTemplateInfo info)
        {
            Context.RegisterDataTemplate(template, info);
        }

        public void GenerateDataTemplate(DataTemplate template, XmlWriter writer, IConverterContext context)
        {
            Context.GenerateDataTemplate(template, writer, context ?? this);
        }

        public void TriggerItemReference(string typename)
        {
            Context.TriggerItemReference(typeof(DataTemplate).FullName);
            Context.TriggerItemReference(typename);
        }

        #endregion

        #region IConverterStack

        public void Push(object obj)
        {
             _cstack.Push(obj);
        }

        public void Pop()
        {
            _cstack.Pop();
        }

        public object GetCurrent(Type type)
        {
            return _cstack.GetCurrent(type);
        }

        public IEnumerable<T> GetAncestors<T>()
        {
            return _cstack.GetAncestors<T>();
        }

        #endregion


    }
}
