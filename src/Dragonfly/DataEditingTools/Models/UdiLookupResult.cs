
namespace Dragonfly.DataEditingTools.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

partial class Enums
{
	public enum LookupStatus
	{
		NotSearchedYet,
		SearchInProgress,
		ObjectFound,
		ObjectNotFound,
		Error,
		ObjectTypeNotSupported
	}
}

public class UdiLookupResult
{
	public Enums.LookupStatus Status { get; set; }
	public Enums.UmbracoObjectType ObjectType { get; set; }
	public string ObjectTypeDisplayName { get; set; }
	public int Id { get; set; }
	public string Name { get; set; }
	public string ErrorMsg { get; set; }
	public string Udi { get; set; }
	public Guid Guid { get; set; }
	public FormInputsUdiLookup LookupCriteria { get; set; }


	public UdiLookupResult(Enums.LookupStatus Status)
	{

	}
}

