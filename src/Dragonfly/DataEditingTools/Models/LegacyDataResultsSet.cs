namespace Dragonfly.DataEditingTools.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;


public partial class Enums
{

}

public class LegacyDataResultsSet
{
	public FormInputsStoreLegacyData FormInputs { get; set; }

	public List<LegacyDataResult> Results { get; set; }
	//public IEnumerable<string> PropertyAliases { get; set; }
	public bool DataUpdatedAndSaved { get; set; }

	public LegacyDataResultsSet()
	{
		Results = new List<LegacyDataResult>();
	}
}

public class LegacyDataResult
{
	public Enums.NodeType Type { get; set; }
	public IContent ContentNode { get; set; }

	public IMedia MediaNode { get; set; }

	public string IdPropertyAlias { get; set; }

	public object IdData { get; set; }

	//public string PropertyFromDataFormat { get; set; }
	//public string PropertyToAlias { get; set; }
	//public object PropertyToData { get; set; }
	//public string PropertyToDataFormat { get; set; }
	public bool DataFormatIsNotValidForTransfer { get; set; }
	public bool ValidToTransfer { get; set; }
	public bool NodeUpdated { get; set; }

	public Guid Key { get; set; }
	public bool ContentUpdated { get; set; }
	public PublishResult SavePublishResult { get; set; }
	public OperationResult SaveOnlyResult { get; set; }


	public LegacyDataResult()
	{
		Key = Guid.NewGuid();
	}
}
