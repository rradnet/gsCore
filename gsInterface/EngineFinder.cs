using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

namespace gs.interfaces
{
    public class EngineFinder
    {
        #pragma warning disable CS0649
        [ImportMany(typeof(IEngine))]
        private IEnumerable<Lazy<IEngine, IEngineData>> engines;
        #pragma warning restore CS0649

        private CompositionContainer container;

        public Dictionary<string, Lazy<IEngine, IEngineData>> EngineDictionary { get; private set; }

        public EngineFinder(string path, Action<string> logger = null)
        {
            // An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            // This iteration is required because of the bundling done by Fody.Costura
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                catalog.Catalogs.Add(new AssemblyCatalog(asm));
            }

            if (!Directory.Exists(path))
            {
                logger?.Invoke("Invalid path passed to EngineFinder constructor");
                logger?.Invoke(Path.GetFullPath(path));
                return;
            }

            catalog.Catalogs.Add(new DirectoryCatalog(path));
            foreach (var p in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
            {
                // Add catalogs from file path
                catalog.Catalogs.Add(new DirectoryCatalog(p));
            }

            // Create the CompositionContainer with the parts in the catalog
            container = new CompositionContainer(catalog);

            // Fill the imports of this object
            try
            {
                container.ComposeParts(this);
            }
            catch (ReflectionTypeLoadException e)
            {
                logger?.Invoke("Composition loader exceptions:");
                foreach (var a in e.LoaderExceptions)
                {
                    logger?.Invoke(a.ToString());
                    return;
                }
            }
            catch (CompositionException e)
            {
                logger?.Invoke(e.ToString());
                return;
            }

            EngineDictionary = new Dictionary<string, Lazy<IEngine, IEngineData>>();

            if (engines == null)
            {
                logger?.Invoke("No engines found");
                return;
            }

            foreach (var e in engines)
            {
                if (!EngineDictionary.ContainsKey(e.Metadata.Name))
                {
                    EngineDictionary.Add(e.Metadata.Name.ToLower(), e);
                    logger?.Invoke("Found engine: " + e.Metadata.Name);
                }
            }
        }
    }
}
