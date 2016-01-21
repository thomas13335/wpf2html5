using System.Reflection;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    class ScriptReference : IScriptReference
    {
        public Assembly Assembly { get; private set; }

        public string Path { get; private set; }

        public string Key { get { return Assembly.GetName().Name + "." + Path; } }

        public ScriptReference(Assembly assembly, string path)
        {
            Assembly = assembly;
            Path = path;
        }
    }
}
