using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PipServices3.Commons.Config;
using PipServices3.Commons.Errors;
using PipServices3.Commons.Refer;
using PipServices3.Components.Log;

namespace PipServices3.Data.Persistence
{
    /// <summary>
    /// Persistence component that loads and saves data from/to flat file.
    /// 
    /// It is used by FilePersistence, but can be useful on its own.
    /// 
    /// ### Configuration parameters ###
    /// 
    /// - path:          path to the file where data is stored
    /// </summary>
    /// <typeparam name="T">the class type</typeparam>
    /// <example>
    /// <code>
    /// var persister = new JsonFilePersister("./data/data.json");
    /// 
    /// var list = new List<string>() {{add("A"); add("B"); add("C"); }};
    /// 
    /// persister.Save("123", list);
    /// ...
    /// persister.Load("123", items);
    /// Console.Out.WriteLine(items); // Result: ["A", "B", "C"] 
    /// </code>
    /// </example>
    public sealed class JsonFilePersister<T> : IReferenceable, ILoader<T>, ISaver<T>, IConfigurable
    {
        private CompositeLogger _logger = new CompositeLogger();

        /// <summary>
        /// Creates a new instance of the persistence.
        /// </summary>
        public JsonFilePersister()
            : this(null)
        { }

        /// <summary>
        /// Creates a new instance of the persistence.
        /// </summary>
        /// <param name="path">(optional) a path to the file where data is stored.</param>
        public JsonFilePersister(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Gets the file path where data is stored.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Configures component by passing configuration parameters.
        /// </summary>
        /// <param name="config">configuration parameters to be set.</param>
        public void Configure(ConfigParams config)
        {
            if (config == null || !config.ContainsKey("path"))
                throw new ConfigException(null, "NO_PATH", "Data file path is not set");

            Path = config.GetAsString("path");
        }

        /// <summary>
        /// Sets references to dependent components.
        /// </summary>
        /// <param name="references">references to locate the component dependencies.</param>
        public void SetReferences(IReferences references)
        {
            _logger.SetReferences(references);
        }

        /// <summary>
        /// Loads data items from external JSON file.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <returns>loaded items.</returns>
        public async Task<List<T>> LoadAsync(string correlationId)
        {
            if (!System.IO.File.Exists(Path))
            {
                _logger.Warn(correlationId, $"The file path '{Path}' is not found.");
                return new List<T>();
            }

            try
            {
                using (var reader = new StreamReader(System.IO.File.OpenRead(Path)))
                {
                    var json = await reader.ReadToEndAsync();
                    var list = JsonConvert.DeserializeObject<T[]>(json);
                    return list != null ? new List<T>(list) : new List<T>();
                }
            }
            catch (Exception ex)
            {
                throw new FileException(correlationId, "READ_FAILED", "Failed to read data file: " + Path, ex)
                    .WithCause(ex);
            }
        }

        /// <summary>
        /// Saves given data items to external JSON file.
        /// </summary>
        /// <param name="correlationId">(optional) transaction id to trace execution through call chain.</param>
        /// <param name="entities">list if data items to save</param>
        /// <returns></returns>
        public async Task SaveAsync(string correlationId, IEnumerable<T> entities)
        {
            try
            {
                using (var writer = new StreamWriter(System.IO.File.Create(Path)))
                {
                    var json = JsonConvert.SerializeObject(entities.ToArray(), Formatting.Indented);
                    await writer.WriteAsync(json);
                }
            }
            catch (Exception ex)
            {
                throw new FileException(correlationId, "WRITE_FAILED", "Failed to write data file: " + Path, ex)
                    .WithCause(ex);
            }
        }
    }
}
