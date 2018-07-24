using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PipServices.Commons.Config;
using PipServices.Commons.Errors;
using PipServices.Commons.Refer;
using PipServices.Components.Log;

namespace PipServices.Data.Persistence
{
    public sealed class JsonFilePersister<T> : IReferenceable, ILoader<T>, ISaver<T>, IConfigurable
    {
        private CompositeLogger _logger = new CompositeLogger();

        public JsonFilePersister()
            : this(null)
        { }

        public JsonFilePersister(string path)
        {
            Path = path;
        }

        public string Path { get; private set; }

        public void Configure(ConfigParams config)
        {
            if (config == null || !config.ContainsKey("path"))
                throw new ConfigException(null, "NO_PATH", "Data file path is not set");

            Path = config.GetAsString("path");
        }

        public void SetReferences(IReferences references)
        {
            _logger.SetReferences(references);
        }

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
