using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace Cloudpunk_Trainer
{
    public sealed class Utils
    {
        public static void ChangeControlStyles(Control ctrl, ControlStyles flag, bool value)
        {
            MethodInfo method = ctrl.GetType().GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null)
            {
                method.Invoke(ctrl, new object[] { flag, value });
            }               
        }

        public static ProcessModule GetProcessModuleByName(Process process, string name)
        {
            if (process == null || !process.Responding || process.HasExited)
            {
                return null;
            }

            foreach (ProcessModule module in process.Modules)
            {
                if (!string.IsNullOrEmpty(module.ModuleName))
                {
                    if (module.ModuleName == name)
                    {
                        return module;
                    }
                }
            }

            return null;
        }
    }
}
