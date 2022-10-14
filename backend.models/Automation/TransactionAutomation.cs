﻿using assetgrid_backend.models.Search;
using assetgrid_backend.Models;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reflection;

namespace assetgrid_backend.models.Automation
{
    public class TransactionAutomation
    {
        public int Version => 1;
        public TransactionAutomationTrigger Triggers { get; set; }
        public SearchGroup Query { get; set; } = null!;
        public List<TransactionAutomationAction> Actions { get; set; } = null!;
    }

    [Flags]
    public enum TransactionAutomationTrigger
    {
        None = 0,
        Create = 1,
        Modify = 2
    }

    [JsonConverter(typeof(TransactionAutomationActionConverter))]
    public abstract class TransactionAutomationAction
    {
        public abstract string Key { get; }
        public abstract int Version { get; }
        public abstract Task Run(IQueryable<Transaction> transactions, AssetgridDbContext context);
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class TransactionActionAttribute : Attribute
    {
        public int? Version { get; set; }
        public string Key { get; set; }
        public TransactionActionAttribute (int version, string key)
        {
            Version = version;
            Key = key;
        }

        public TransactionActionAttribute(string key)
        {
            Key = key;
        }
    }

    #region Actions

    [TransactionAction("set-timestamp")]
    public class ActionSetTimestamp : TransactionAutomationAction
    {
        public override string Key => "set-timestamp";
        public override int Version => 1;
        public DateTime Value { get; set; }
        public override Task Run(IQueryable<Transaction> transactions, AssetgridDbContext context)
        {
            transactions.ToList().ForEach(transaction => transaction.DateTime = Value);
            return Task.CompletedTask;
        }
    }

    [TransactionAction("set-description")]
    public class ActionSetDescription : TransactionAutomationAction
    {
        public override string Key => "set-description";
        public override int Version => 1;
        public string Value { get; set; } = null!;
        public override Task Run(IQueryable<Transaction> transactions, AssetgridDbContext context)
        {
            transactions.ToList().ForEach(transaction => transaction.Description = Value);
            return Task.CompletedTask;
        }
    }

    [TransactionAction("set-amount")]
    public class ActionSetAmount : TransactionAutomationAction
    {
        public override string Key => "set-amount";
        public override int Version => 1;
        public long Value { get; set; }
        public string ValueString { get => Value.ToString(); set => Value = long.Parse(value); }
        public override Task Run(IQueryable<Transaction> transactions, AssetgridDbContext context)
        {
            foreach (var transaction in transactions.Where(t => ! t.IsSplit).ToList())
            {
                transaction.Total = Value;
                transaction.TransactionLines.Single().Amount = Value;
            }

            return Task.CompletedTask;
        }
    }

    #endregion

    #region JSON deserializers

    public class TransactionAutomationConverter : JsonConverter<TransactionAutomation>
    {
        public override TransactionAutomation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var readerCopy = reader;
            var version = JsonSerializer.Deserialize<VersionStruct?>(ref readerCopy, options);

            if (version == null || version.Value.Version == null)
            {
                // Assume newest version. Use default converter as this function would otherwise be called in an infinate loop
                return JsonSerializer.Deserialize<TransactionAutomation>(ref reader, options);
            }
            // Once more versions are added, this will switch between versions and automatically migrate them if needed
            return JsonSerializer.Deserialize<TransactionAutomation>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, TransactionAutomation value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }

        private struct VersionStruct
        {
            public int? Version { get; set; }
        }
    }

    public class TransactionAutomationActionConverter : JsonConverter<TransactionAutomationAction>
    {
        public override TransactionAutomationAction? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var readerCopy = reader;
            var version = JsonSerializer.Deserialize<VersionStruct?>(ref readerCopy, options)!;

            // Get all implementations of TransactionAction
            var type = Assembly.GetAssembly(typeof(TransactionAutomationAction))!
                .GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(TransactionAutomationAction)))
                .Where(myType =>
                {
                    var attribute = (TransactionActionAttribute)Attribute.GetCustomAttribute(myType, typeof(TransactionActionAttribute))!;
                    return attribute.Key == version.Value.Key && (version.Value.Version == attribute.Version);
                })
                .SingleOrDefault();
            
            if (type == null)
            {
                throw new Exception("Unable to parse JSON");
            }

            if (version.Value.Version == null)
            {
                var result = (TransactionAutomationAction)JsonSerializer.Deserialize(ref reader, type, options)!;
                return result;
            }
            // Automatically upgrade previous versions
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, TransactionAutomationAction value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }

        private struct VersionStruct
        {
            public int? Version { get; set; }
            public string Key { get; set; }
        }
    }

    #endregion
}
