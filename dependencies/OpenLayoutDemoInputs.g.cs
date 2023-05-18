// This code was generated by Hypar.
// Edits to this code will be overwritten the next time you run 'hypar init'.
// DO NOT EDIT THIS FILE.

using Elements;
using Elements.GeoJSON;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Validators;
using Elements.Serialization.JSON;
using Hypar.Functions;
using Hypar.Functions.Execution;
using Hypar.Functions.Execution.AWS;
using Hypar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Line = Elements.Geometry.Line;
using Polygon = Elements.Geometry.Polygon;

namespace OpenLayoutDemo
{
    #pragma warning disable // Disable all warnings

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.1.21.0 (Newtonsoft.Json v13.0.0.0)")]
    
    public  class OpenLayoutDemoInputs : S3Args
    
    {
        [Newtonsoft.Json.JsonConstructor]
        
        public OpenLayoutDemoInputs(IList<Extract> @extract, string bucketName, string uploadsBucket, Dictionary<string, string> modelInputKeys, string gltfKey, string elementsKey, string ifcKey):
        base(bucketName, uploadsBucket, modelInputKeys, gltfKey, elementsKey, ifcKey)
        {
            var validator = Validator.Instance.GetFirstValidatorForType<OpenLayoutDemoInputs>();
            if(validator != null)
            {
                validator.PreConstruct(new object[]{ @extract});
            }
        
            this.Extract = @extract;
        
            if(validator != null)
            {
                validator.PostConstruct(this);
            }
        }
    
        [Newtonsoft.Json.JsonProperty("Extract", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public IList<Extract> Extract { get; set; }
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.1.21.0 (Newtonsoft.Json v13.0.0.0)")]
    
    public partial class Extract 
    
    {
        [Newtonsoft.Json.JsonConstructor]
        public Extract(string @category, string @program, string @layout, double @aisle, double @gap, double @forward, double @backward, double @inset, bool @showPattern)
        {
            var validator = Validator.Instance.GetFirstValidatorForType<Extract>();
            if(validator != null)
            {
                validator.PreConstruct(new object[]{ @category, @program, @layout, @aisle, @gap, @forward, @backward, @inset, @showPattern});
            }
        
            this.Category = @category;
            this.Program = @program;
            this.Layout = @layout;
            this.Aisle = @aisle;
            this.Gap = @gap;
            this.Forward = @forward;
            this.Backward = @backward;
            this.Inset = @inset;
            this.ShowPattern = @showPattern;
        
            if(validator != null)
            {
                validator.PostConstruct(this);
            }
        }
    
        /// <summary>Select the Rhino model category to work with</summary>
        [Newtonsoft.Json.JsonProperty("Category", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Category { get; set; }
    
        [Newtonsoft.Json.JsonProperty("Program", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Program { get; set; }
    
        /// <summary>What layout strategy should be used?</summary>
        [Newtonsoft.Json.JsonProperty("Layout", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Layout { get; set; }
    
        [Newtonsoft.Json.JsonProperty("Aisle", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, double.MaxValue)]
        public double Aisle { get; set; } = 2D;
    
        [Newtonsoft.Json.JsonProperty("Gap", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.Range(0.001D, double.MaxValue)]
        public double Gap { get; set; } = 0.001D;
    
        [Newtonsoft.Json.JsonProperty("Forward", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, double.MaxValue)]
        public double Forward { get; set; } = 0D;
    
        [Newtonsoft.Json.JsonProperty("Backward", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, double.MaxValue)]
        public double Backward { get; set; } = 0D;
    
        [Newtonsoft.Json.JsonProperty("Inset", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, double.MaxValue)]
        public double Inset { get; set; } = 0D;
    
        [Newtonsoft.Json.JsonProperty("Show Pattern", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool ShowPattern { get; set; } = false;
    
        private System.Collections.Generic.IDictionary<string, object> _additionalProperties = new System.Collections.Generic.Dictionary<string, object>();
    
        [Newtonsoft.Json.JsonExtensionData]
        public System.Collections.Generic.IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }
    }
}