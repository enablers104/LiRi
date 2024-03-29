// <auto-generated>
// Code generated by LUISGen liri_v0.1.json -cs Luis.LiriModel -o 
// Tool github: https://github.com/microsoft/botbuilder-tools
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
namespace Microsoft.BotBuilderSamples
{
    public partial class LiriModel: IRecognizerConvert
    {
        [JsonProperty("text")]
        public string Text;

        [JsonProperty("alteredText")]
        public string AlteredText;

        public enum Intent {
            BookIBT, 
            Cancel, 
            GetWeather, 
            None, 
            TFGAccount, 
            TFGHR, 
            TFGStock,
            TFGLegals,
            TFGSkuLookup
        };
        [JsonProperty("intents")]
        public Dictionary<Intent, IntentScore> Intents;

        public class _Entities
        {
            // Simple entities
            public string[] Brand;

            public string[] Color;

            public string[] EmployeeNumber;

            public string[] FirstName;

            public string[] Garment;

            public string[] IDNumber;

            public string[] LastName;

            public string[] Size;

            public string[] Title;

            // Built-in entities
            public DateTimeSpec[] datetime;

            public string[] phonenumber;

            // Lists
            public string[][] Airport;

            public string[][] Branch;

            public string[] QueryType;

            public string[] SkuCode;

            // Composites
            public class _InstanceFrom
            {
                public InstanceData[] Airport;
            }
            public class FromClass
            {
                public string[][] Airport;
                [JsonProperty("$instance")]
                public _InstanceFrom _instance;
            }
            public FromClass[] From;

            public class _InstanceTo
            {
                public InstanceData[] Airport;
            }
            public class ToClass
            {
                public string[][] Airport;
                [JsonProperty("$instance")]
                public _InstanceTo _instance;
            }
            public ToClass[] To;

            // Instance
            public class _Instance
            {
                public InstanceData[] Airport;
                public InstanceData[] Branch;
                public InstanceData[] Brand;
                public InstanceData[] Color;
                public InstanceData[] EmployeeNumber;
                public InstanceData[] FirstName;
                public InstanceData[] From;
                public InstanceData[] Garment;
                public InstanceData[] IDNumber;
                public InstanceData[] LastName;
                public InstanceData[] Size;
                public InstanceData[] Title;
                public InstanceData[] To;
                public InstanceData[] datetime;
                public InstanceData[] phonenumber;
                public InstanceData[] skuCode;
            }
            [JsonProperty("$instance")]
            public _Instance _instance;
        }
        [JsonProperty("entities")]
        public _Entities Entities;

        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties {get; set; }

        /// <summary>
        /// Converts the specified result.
        /// </summary>
        /// <param name="result">The result.</param>
        public void Convert(dynamic result)
        {
            var app = JsonConvert.DeserializeObject<LiriModel>(JsonConvert.SerializeObject(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            Text = app.Text;
            AlteredText = app.AlteredText;
            Intents = app.Intents;
            Entities = app.Entities;
            Properties = app.Properties;
        }

        /// <summary>
        /// Tops the intent.
        /// </summary>
        /// <returns></returns>
        public (Intent intent, double score) TopIntent()
        {
            Intent maxIntent = Intent.None;
            var max = 0.0;
            foreach (var entry in Intents)
            {
                if (entry.Value.Score > max)
                {
                    maxIntent = entry.Key;
                    max = entry.Value.Score.Value;
                }
            }
            return (maxIntent, max);
        }
    }
}
