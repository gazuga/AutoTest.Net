using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AutoTest.Core.BuildRunners
{
    public class PluginIntegratingPreProcessor : IPreProcessBuildruns
    {
        IEnumerable<IPreProcessBuildruns> _plugins;

        public PluginIntegratingPreProcessor()
        {
            var path = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath;
            this._plugins = Directory.GetFiles(path).Where(f => f.Contains("AutoTest.Plugins") && f.Contains(".dll"))
                .SelectMany(f => {
                    return Assembly.LoadFile(Path.Combine(path, f)).GetTypes()
                        .Where(t => typeof(IPreProcessBuildruns).IsAssignableFrom(t) && !t.IsInterface)
                        .Select(t => (IPreProcessBuildruns) Activator.CreateInstance(t));
                });
        }

        public Messaging.MessageConsumers.RunInfo[] PreProcess(Messaging.MessageConsumers.RunInfo[] details)
        {
            foreach (var plugin in _plugins)
                details = plugin.PreProcess(details);
            return details;
        }

        public Messages.BuildRunResults PostProcessBuildResults(Messages.BuildRunResults runResults)
        {
            foreach (var plugin in _plugins)
                runResults = plugin.PostProcessBuildResults(runResults);
            return runResults;
        }

        public Messaging.MessageConsumers.RunInfo[] PostProcess(Messaging.MessageConsumers.RunInfo[] details, ref Messages.RunReport runReport)
        {
            foreach (var plugin in _plugins)
                details = plugin.PostProcess(details, ref runReport);
            return details;
        }
    }
}
